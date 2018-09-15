using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using SearchAThing.Util;

namespace SearchAThing
{

    #region python pipe
    public class PythonPipe : IDisposable
    {

        #region python path
        static string _PythonExePathfilename;

        internal static string PythonExePathfilename
        {
            get
            {
                if (_PythonExePathfilename == null)
                {
                    var searchFor =
                        (
                        Environment.OSVersion.Platform == PlatformID.Unix
                        ||
                        Environment.OSVersion.Platform == PlatformID.MacOSX
                        )
                        ? "python" : "python.exe";

                    _PythonExePathfilename = Util.Path.SearchInPath(searchFor);
                    if (_PythonExePathfilename == null) _PythonExePathfilename = "";
                }
                return _PythonExePathfilename;
            }
        }
        #endregion

        object wrapper_initialized = new object();
        Action<string> debug = null;

        Process process = null;
        StringBuilder sberr = new StringBuilder();
        StringBuilder sbout = new StringBuilder();

        string TempFolder = null;
        public bool DeleteTmpFiles { get; set; }

        const string initial_imports_default = @"
import matplotlib
matplotlib.use('Agg')
";

        Thread th_pipe = null;

        bool startup_error = false;

        public PythonPipe(string initial_imports = "", Action<string> _debug = null, string tempFolder = null, bool delete_tmp_files = true,
            string custom_python_executable = null, string custom_python_args = null)
        {
            DeleteTmpFiles = delete_tmp_files;
            TempFolder = tempFolder;
            debug = _debug;

            th_pipe = new Thread(() =>
            {
                debug?.Invoke("initializing python");
                lock (wrapper_initialized)
                {
                    var guid = Guid.NewGuid().ToString();

                    process = new Process();
                    process.StartInfo.FileName = custom_python_executable == null ? PythonExePathfilename : custom_python_executable;
                    process.StartInfo.Arguments = (custom_python_args == null) ? "-i" : custom_python_args;
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
                            debug?.Invoke($"(init) using process id = [{process.Id}]");

                            switch (Environment.OSVersion.Platform)
                            {
                                case PlatformID.Unix:
                                case PlatformID.MacOSX:
                                    {
                                        process.StandardInput.Write($"{initial_imports.Replace("\r\n", "\n")}\nprint('{guid}')\n");
                                    }
                                    break;

                                default:
                                    {
                                        process.StandardInput.Write($"{initial_imports}\r\nprint('{guid}')\r\n");
                                    }
                                    break;
                            }

                            process.StandardInput.Flush();

                            while (!initialized)
                            {
                                Thread.Sleep(250);
                            }

                            //process.CancelOutputRead();
                            //process.CancelErrorRead();

                            if (hasErr) throw new Exception($"python init err [{sberr}]");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"err [{ex.Details()}]");
                        }
                    }
                }
                process.WaitForExit();
            });
            th_pipe.Start();
        }

        bool initialized = false;
        string guid = null;

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            if (finished) return;

            debug?.Invoke($"output received [{e.Data}]");

            if (!initialized)
                initialized = true;
            else
            {
                var str = e.Data;

                if (str == guid) finished = true;
                else
                {
                    if (str.EndsWith(guid + "\r\n"))
                    {
                        str = str.Substring(0, str.Length - guid.Length);
                        finished = true;
                    }

                    sbout.AppendLine(str);
                }
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            if (finished) return;

            if (e.Data.StartsWith("Python ") || e.Data.StartsWith("[GCC ") || e.Data.StartsWith("Type \"help\"")) return;

            debug?.Invoke($"***error received [{e.Data}]");

            hasErr = true;

            if (e.Data == null) return;

            var s = e.Data;
            while (s.StartsWith(">>> ")) s = s.Substring(4);
            sberr.AppendLine(s);
        }

        internal const int win32_string_len_safe = 3000;
        internal const int win32_max_string_len = 4000;

        bool hasErr = false;
        bool finished = false;

        /// <summary>
        /// exec given code through a temp file
        /// </summary>        
        public StringWrapper Exec(StringWrapper code, bool remove_tmp_file = true)
        {
            string tmp_pathfilename = null;
            if (TempFolder == null)
                tmp_pathfilename = System.IO.Path.GetTempFileName() + ".py";
            else
                tmp_pathfilename = System.IO.Path.Combine(TempFolder, "_" + Guid.NewGuid().ToString() + ".py");

            guid = Guid.NewGuid().ToString();

            using (var sw0 = new StreamWriter(tmp_pathfilename))
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        {
                            sw0.WriteLine(code.str.Replace("\r\n", "\n"));
                        }
                        break;

                    default:
                        {
                            sw0.WriteLine(code.str);
                        }
                        break;
                }
                sw0.WriteLine($"print('{guid}')");
            }

            sberr.Clear();
            sbout.Clear();

            var sw = new Stopwatch();
            sw.Start();

            string res = "";

            while (!initialized) 
            {
                if (startup_error) throw new Exception($"startup error [{sberr.ToString()}]");
                Thread.Sleep(25);
            }

            lock (wrapper_initialized)
            {
                finished = false;
                hasErr = false;

                //process.BeginErrorReadLine();
                //process.BeginOutputReadLine();

                var cmd = $"exec(open('{tmp_pathfilename.Replace('\\', '/')}').read())\n";
                debug?.Invoke($"using process id = [{process.Id}]");
                process.StandardInput.WriteLine(cmd);
                process.StandardInput.Flush();

                while (!finished)
                {
                    //process.StandardInput.Flush();
                    Thread.Sleep(25);
                    if (hasErr)
                    {
                        Thread.Sleep(25); // gather other errors
                        break;
                    }
                }

                //process.CancelErrorRead();
                //process.CancelOutputRead();

                if (hasErr) throw new PythonException($"pyhton[{PythonExePathfilename}] script[{tmp_pathfilename}] : {sberr.ToString()}", sbout.ToString());

                res = sbout.ToString();
            }

            sw.Stop();
            debug?.Invoke($"python took [{sw.Elapsed}]");

            if (remove_tmp_file) File.Delete(tmp_pathfilename);

            return new StringWrapper() { str = res };
        }

        public void Dispose()
        {
            if (th_pipe != null)
            {
                debug?.Invoke($"kill python");
                process.Kill();
                if (Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX)
                {
                    th_pipe.Abort();
                }
                th_pipe = null;
            }
        }
    }
    #endregion


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

        public override string ToString()
        {
            return $"output [{Output}] error [{Error}]";
        }

    }



}
