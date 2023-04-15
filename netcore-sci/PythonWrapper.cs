using System.Threading;

namespace SearchAThing
{

    public enum PythonPipeOnErrorResultEnum { Ignore, GenerateException };

    public class PythonPipeOptions
    {

        /// <summary>
        /// debug messages of pythonpipe execution
        /// </summary>
        public Action<string>? Debug { get; set; } = null;

        /// <summary>
        /// action called when error message reported and allow to ignore by returning OnErrorResultEnum.Ignore
        /// </summary>
        public Func<string, PythonPipeOnErrorResultEnum>? OnError { get; set; } = null;

        /// <summary>
        /// action called when output from pipe ( full output already results from python exec )
        /// </summary>
        public Action<string>? OnOutput { get; set; } = null;

        /// <summary>
        /// allow to override environment python executable pathfilename
        /// </summary>
        public string? PythonExecutablePathfilename = null;

        /// <summary>
        /// allow to specify custom arguments in place of default "-i" argument
        /// </summary>
        public string? PythonArgs = null;
    }

    /// <summary>
    /// Helper to invoke python and retrieve results.
    /// </summary>
    public class PythonPipe : IDisposable
    {

        #region python path
        static string? _PythonExePathfilename = null;

        internal static string PythonExePathfilename
        {
            get
            {
                if (_PythonExePathfilename is null)
                {
                    var searchFor = new List<string>();

                    switch (Environment.OSVersion.Platform)
                    {
                        case PlatformID.Unix:
                        case PlatformID.MacOSX:
                            {
                                searchFor = new List<string>() { "python", "python3" };
                            }
                            break;

                        default:
                            {
                                searchFor = new List<string>() { "python.exe" };
                            }
                            break;
                    }

                    foreach (var fname in searchFor)
                    {
                        var q = SearchInPath(fname);
                        if (q != null)
                        {
                            _PythonExePathfilename = q;
                            break;
                        }
                    }

                    if (_PythonExePathfilename is null)
                    {
                        _PythonExePathfilename = "";
                    }
                }
                return _PythonExePathfilename;
            }
        }
        #endregion

        public AutoResetEvent areInit = new AutoResetEvent(false);

        Process? process = null;
        StringBuilder sberr = new StringBuilder();
        StringBuilder sbout = new StringBuilder();

        bool startup_error = false;

        public PythonPipeOptions Options { get; private set; } = new PythonPipeOptions();

        public PythonPipe(PythonPipeOptions? options = null)
        {
            if (options is not null)
                Options = options;

            ThFunction();
        }

        /// <summary>
        /// naive python comment removal, only beginning (#) or ending (whitespace #) or multiline beginning and ending with (''')
        /// </summary>        
        string StripPythonComments(string input)
        {
            var ll = input.Lines().ToList();
            var ll2 = new List<string>();

            var comment_block_open = false;

            var endingCommentRegex = new Regex(@"(.*)\s#(.*)");

            for (int i = 0; i < ll.Count; ++i)
            {
                var line = ll[i].Trim();
                if (line.StartsWith("'''"))
                {
                    comment_block_open = !comment_block_open;
                    continue;
                }

                if (comment_block_open) continue;

                if (line.StartsWith("#")) continue;

                var endingCommentTest = endingCommentRegex.Match(line);
                if (endingCommentTest.Success)
                    ll2.Add(endingCommentTest.Groups[1].Value);
                else

                    ll2.Add(ll[i]);
            }

            var src = string.Join("\\n", ll2.Select(w => w.Replace("'", "\\'")));

            return src;
        }

        void ThFunction()
        {
            Options.Debug?.Invoke("initializing python");

            guid = Guid.NewGuid().ToString();

            process = new Process();

            process.StartInfo.FileName = Options?.PythonExecutablePathfilename is null ?
                PythonExePathfilename :
                Options?.PythonExecutablePathfilename;

            process.StartInfo.Arguments = Options?.PythonArgs is null ?
                "-i" :
                Options.PythonArgs;

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            var started = process.Start();

            if (process.HasExited)
            {
                startup_error = true;
                return;
            }

            if (started)
            {
                try
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.StandardInput.AutoFlush = false;
                    Options.Debug?.Invoke($"(init) using process id = [{process.Id}]");

                    process.StandardInput.Write($"{Environment.NewLine}print('{guid}'){Environment.NewLine}");

                    process.StandardInput.Flush();

                    while (!initialized && !cts.Token.IsCancellationRequested)
                    {
                        Thread.Sleep(250);
                    }

                    if (hasErr) throw new Exception($"python init err [{sberr}]");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"using python[{process.StartInfo.FileName}] err [{ex.Message}]");
                }
            }

            areInit.Set();
        }

        bool initialized = false;
        string? guid = null;

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is null) return;
            if (finished) return;

            Options.Debug?.Invoke($"output received [{e.Data}]");

            if (!initialized)
                initialized = true;

            else
            {
                if (guid is null) throw new Exception($"guid not initialized in Exec");

                var str = e.Data;

                if (str == guid)
                    finished = true;

                else
                {
                    if (str.EndsWith(guid + Environment.NewLine))
                    {
                        str = str.Substring(0, str.Length - guid.Length);
                        finished = true;
                    }

                    sbout.AppendLine(str);

                    Options.OnOutput?.Invoke(str);
                }
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is null) return;
            if (finished) return;

            if (e.Data.StartsWith("Python ") || e.Data.StartsWith("[GCC ") || e.Data.StartsWith("Type \"help\"")) return;

            var ignore = Options.OnError is not null && Options.OnError(e.Data) == PythonPipeOnErrorResultEnum.Ignore;

            Options.Debug?.Invoke($"***error received{(ignore ? " (ignored)" : "")} [{e.Data}]");

            if (!ignore)
                hasErr = true;

            if (e.Data is null) return;

            var s = e.Data;
            while (s.StartsWith(">>> ")) s = s.Substring(4);
            sberr.AppendLine(s);
        }

        internal const int win32_string_len_safe = 3000;
        internal const int win32_max_string_len = 4000;

        bool hasErr = false;
        bool finished = false;

        /// <summary>
        /// exec given code lines
        /// </summary>
        public async Task<string> Exec(IEnumerable<string> code_lines) =>
            await Exec(string.Join("\n", code_lines));

        /// <summary>
        /// exec given code ( can contains newlines as read from a file ).
        /// Actually only one execution per PythonPipe object is allowed.
        /// </summary>        
        public async Task<string> Exec(string code)
        {
            if (finished) throw new NotImplementedException($"Subsequent exec not yet supported");

            sberr.Clear();
            sbout.Clear();

            var sw = new Stopwatch();
            sw.Start();

            while (!initialized)
            {
                if (startup_error) throw new Exception($"startup error [{sberr.ToString()}]");
                await Task.Delay(25);
            }

            if (process is null) throw new Exception($"process not initialized");

            areInit.WaitOne();

            string res;
            {
                finished = false;
                hasErr = false;

                var src = StripPythonComments(code);
                src += $"\\nprint(\\'{guid}\\')";

                process.StandardInput.WriteLine($"exec('{src}')");

                process.StandardInput.Flush();

                while (!finished)
                {
                    await Task.Delay(25);
                    if (hasErr)
                    {
                        await Task.Delay(25); // gather other errors
                                              // 
                        break;
                    }
                }

                if (hasErr)
                {
                    throw new PythonException($"pyhton[{PythonExePathfilename}] : {sberr.ToString()}", sbout.ToString());
                }

                res = sbout.ToString();
            }

            sw.Stop();
            Options.Debug?.Invoke($"python took [{sw.Elapsed}]");

            return res;
        }

        CancellationTokenSource cts = new CancellationTokenSource();

        public void Dispose()
        {
            cts.Cancel();

            process?.Kill();
        }
    }

    public class PythonException : Exception
    {

        public PythonException(string errmsg, string output) : base(errmsg)
        {
            Error = errmsg;
            Output = output;
        }

        /// <summary>
        /// stderr result
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// stdout result
        /// </summary>
        public string Output { get; private set; }

        public override string ToString() => $"output [{Output}] error [{Error}]";

    }

}
