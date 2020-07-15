using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using SearchAThing;

namespace gl_csharp_bindings_generator
{

    // gl registry xml spec ( https://github.com/KhronosGroup/OpenGL-Registry/blob/04eeb97933ddefc71052d150957a3e91bbd6f829/xml/readme.pdf )    

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                System.Console.WriteLine($"{Assembly.GetExecutingAssembly().GetName().Name} <glspec> <glrefs> [dstfld]");
                System.Console.WriteLine($"  dstfld    Destination folder where GlConsts.cs, GlEnums.cs, GlInterface.delegates.cs and GlInterface.methods.cs will generated");
                System.Console.WriteLine($"  glspec    Pathfilename of gl.xml file (can be retrieved from https://github.com/KhronosGroup/OpenGL-Registry/blob/master/xml/gl.xml)");
                System.Console.WriteLine($"  glrefs    Pathname of OpenGL-Refpages/gl4 folder (can be retrieved cloning https://github.com/KhronosGroup/OpenGL-Refpages.git)");
                Environment.Exit(1);
            }

            // custom method wraps
            var customMethodWrap = new Dictionary<string, string>();

            customMethodWrap.Add("GetString", @"
        public string GetString(StringName name)
        {
            var ptr = _GetString(name);
            if (ptr != IntPtr.Zero)
                return Marshal.PtrToStringAnsi(ptr);

            return null;
        }");

            //
            // dstfld
            //
            //var dstfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../netcore-sci/GUI");
            var dstfolder = args[0];
            if (!System.IO.Directory.Exists(dstfolder))
            {
                System.Console.Error.WriteLine($"folder [{dstfolder}] not exists");
                Environment.Exit(3);
            }

            //
            // glspec
            //
            var pathfilename = args[1];
            //var pathfilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gl.xml");

            if (!File.Exists(pathfilename))
            {
                System.Console.Error.WriteLine($"file [{pathfilename}] not exists");
                Environment.Exit(3);
            }

            //
            // glrefs
            //
            var refDocFolder = args.Length < 3 ? "" : args[2];
            //var refDocFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../../OpenGL-Refpages/gl4");

            var GENDOC = Directory.Exists(refDocFolder);
            if (!GENDOC) System.Console.Error.WriteLine($"W: can't find ref pages [{refDocFolder}]; methods will not documented");

            // ---

            var glconstsPathfilename = Path.Combine(dstfolder, "GlConsts.cs");
            var glenumsPathfilename = Path.Combine(dstfolder, "GlEnums.cs");
            var glInterfaceDelegatesPathfilename = Path.Combine(dstfolder, "GlInterface.delegates.cs");
            var glInterfaceMethodsPathfilename = Path.Combine(dstfolder, "GlInterface.methods.cs");

            var swConsts = new StreamWriter(glconstsPathfilename);
            var swEnums = new StreamWriter(glenumsPathfilename);
            var swDelegates = new StreamWriter(glInterfaceDelegatesPathfilename);
            var swMethods = new StreamWriter(glInterfaceMethodsPathfilename);

            var el = XElement.Load(pathfilename);

            var parseContext = new ParseContext();

            var enums = new List<GLEnums>();

            foreach (var x in el.Descendants("enums"))
            {
                var enumsList = x.Descendants("enum").Select(y =>
                {
                    return new GLEnum
                    {
                        value = (uint)((string)y.Attribute("value")).safeParseInt().Value,
                        name = (string)y.Attribute("name"),
                        group = (string)y.Attribute("group"),
                        api = (string)y.Attribute("api"),
                        alias = (string)y.Attribute("alias"),
                    };
                }).ToList();

                var y = new GLEnums
                {
                    ns = (string)x.Attribute("namespace"),
                    type = (string)x.Attribute("type"),
                    start = ((string)x.Attribute("start")).safeParseInt(),
                    end = ((string)x.Attribute("start")).safeParseInt(),
                    vendor = (string)x.Attribute("vendor"),
                    comment = (string)x.Attribute("comment"),
                    enums = enumsList
                };

                foreach (var e in enumsList)
                {
                    if (e.group != null)
                    {
                        var gg = e.group.Split(",");
                        foreach (var g in gg)
                        {
                            parseContext.GroupAddEnum(g, e);
                        }
                    }
                }

                enums.Add(y);
            }

            var cmds = el.Element("commands").Elements("command").Select(x =>
            {
                var protoNfo = x.Element("proto");
                var pname = protoNfo.Element("name")?.Value;

                var protoIsPtr = protoNfo.Element("name")?.PreviousNode?.NodeType == XmlNodeType.Text &&
                    protoNfo.Element("name")?.PreviousNode.ToString().Trim() == "*";

                var cmdObj = new GLCommand(parseContext)
                {
                    comment = (string)x.Attribute("comment"),
                    protoName = pname,
                    protoIsPtr = protoIsPtr,
                    protoPType = protoNfo.Element("ptype")?.Value
                };

                cmdObj.paramLst = x.Elements("param")
                    .Select(z =>
                    {
                        var nameel = z.Element("name");
                        var name = nameel.Value;
                        var ptypeel = z.Element("ptype");

                        var txtBeforeName = nameel.PreviousNode.NodeType == XmlNodeType.Text ? nameel.PreviousNode.ToString().Trim() : "";
                        var txtBeforePType = (ptypeel?.PreviousNode?.NodeType == XmlNodeType.Text) ? ptypeel.PreviousNode.ToString().Trim() : "";
                        var typeName = ptypeel == null ? "" : ptypeel.Value.Trim();

                        var cparam = new GLCommandParam(cmdObj)
                        {
                            group = (string)z.Attribute("group"),
                            len = (string)z.Attribute("len"),
                            type = typeName,
                            name = z.Element("name").Value,
                            txtBeforeName = txtBeforeName,
                            txtBeforePType = txtBeforePType
                        };

                        string wSpc(string s) => s.Length > 0 ? $"{s} " : s;

                        Console.WriteLine($"[{pname}]: {wSpc(txtBeforePType)}{wSpc(typeName)}{wSpc(txtBeforeName)}{wSpc(name)}=> {cparam.ToString()}");

                        return cparam;
                    }).ToList();

                return cmdObj;
            }).ToList();

            //---

            var i1 = " ".Repeat(4);
            var i2 = " ".Repeat(8);

            #region --- CONSTS

            swConsts.WriteLine("using System;");
            swConsts.WriteLine();
            swConsts.WriteLine("namespace Avalonia.OpenGL");
            swConsts.WriteLine("{");

            var dictEnum = new Dictionary<string, UInt32>();
            {
                swConsts.WriteLine($"{i1}public static class GlConsts");
                swConsts.WriteLine($"{i1}{{");

                bool GL_NONE_INCLUDED = false;
                bool GL_FALSE_INCLUDED = false;
                bool GL_TRUE_INCLUDED = false;

                foreach (var x in enums)
                {
                    if (x.vendor != null) swConsts.WriteLine($"{i2}// VENDOR: {x.vendor}");
                    if (x.comment != null) swConsts.WriteLine($"{i2}// {x.comment}");

                    foreach (var y in x.enums)
                    {
                        UInt32 existingValue = 0;
                        if (dictEnum.TryGetValue(y.fqName, out existingValue))
                        {
                            if (existingValue != y.value) throw new Exception($"mismatching existing value for {y.fqName}");
                        }
                        else
                            dictEnum.Add(y.fqName, y.value);

                        if (!GL_NONE_INCLUDED && y.fqName == "GL_NONE") GL_NONE_INCLUDED = true;
                        if (!GL_TRUE_INCLUDED && y.fqName == "GL_TRUE") GL_TRUE_INCLUDED = true;
                        if (!GL_FALSE_INCLUDED && y.fqName == "GL_FALSE") GL_FALSE_INCLUDED = true;

                        //var tt = "uint";
                        //if (y.value <= int.MaxValue) tt = "int";
                        var val = y.value == uint.MaxValue ? "-1" : $"0x{Convert.ToString(y.value, 16).ToUpper()}";
                        var etype = "int";
                        if (y.value != uint.MaxValue && y.value > int.MaxValue) etype = "uint";
                        swConsts.WriteLine($"{i2}public const {etype} {y.fqName} = {val};");
                    }
                }

                if (!GL_NONE_INCLUDED)
                {
                    dictEnum.Add("GL_NONE", 0);
                    swConsts.WriteLine($"{i2}public const int GL_NONE = 0;");
                }
                if (!GL_TRUE_INCLUDED)
                {
                    dictEnum.Add("GL_TRUE", 1);
                    swConsts.WriteLine($"{i2}public const int GL_TRUE = 1;");
                }
                if (!GL_FALSE_INCLUDED)
                {
                    dictEnum.Add("GL_FALSE", 0);
                    swConsts.WriteLine($"{i2}public const int GL_FALSE = 0;");
                }

                swConsts.WriteLine($"{i1}}};");

                swConsts.WriteLine("}");
                swConsts.Flush();
            }
            #endregion

            #region --- ENUMS
            {
                swEnums.WriteLine("using System;");
                swEnums.WriteLine();
                swEnums.WriteLine("namespace Avalonia.OpenGL");
                swEnums.WriteLine("{");

                {
                    foreach (var grp in parseContext.GroupToEnums)
                    {
                        var enumType = "";
                        var uintMode = false;
                        foreach (var x in grp.Value)
                        {
                            if (x.value != uint.MaxValue && x.value > int.MaxValue)
                            {
                                enumType = " : uint";
                                uintMode = true;
                            }
                        }
                        uintMode = false;

                        swEnums.WriteLine();
                        swEnums.WriteLine($"{i1}public enum {grp.Key}{enumType}");
                        swEnums.WriteLine($"{i1}{{");
                        foreach (var x in grp.Value)
                        {
                            swEnums.WriteLine($"{i2}{x.name} = {(uintMode ? "(uint)" : "")}GlConsts.{x.name},");
                        }
                        swEnums.WriteLine($"{i1}}};");
                    }
                }

                swEnums.WriteLine();
                swEnums.WriteLine("}");
            }

            #endregion

            #region --- DELEGATES
            {
                swDelegates.WriteLine("using System;");
                swDelegates.WriteLine("using System.Runtime.CompilerServices;");
                swDelegates.WriteLine();
                swDelegates.WriteLine("namespace Avalonia.OpenGL");
                swDelegates.WriteLine("{");
                {
                    swDelegates.WriteLine($"{i1}public delegate void GlDebugProc(uint error, string command, int messageType, IntPtr threadLabel, IntPtr objectLabel, string message);");
                    swDelegates.WriteLine($"{i1}public delegate IntPtr VulkanProc();");
                    swDelegates.WriteLine();
                    swDelegates.WriteLine($"{i1}public unsafe partial class GlInterface");
                    swDelegates.WriteLine($"{i1}{{");
                }
                foreach (var cmd in cmds)
                {
                    var modes = new GLCommandParamMode[] { GLCommandParamMode.direct, GLCommandParamMode.ptr, GLCommandParamMode.intptr };

                    swDelegates.WriteLine();
                    swDelegates.WriteLine($"{i2}// ---");

                    var alreadyPrototyped = new HashSet<string>();

                    foreach (var mode in modes)
                    {
                        var strCheck = "";

                        var suffix = "";
                        switch (mode)
                        {
                            case GLCommandParamMode.ptr: suffix = "_ptr"; break;
                            case GLCommandParamMode.intptr: suffix = "_intptr"; break;
                        }
                        var cmdparamsStr = string.Join(", ", cmd.paramLst.Select(w => w.ToString(mode)));
                        strCheck = $"{cmd.protoPType} _{cmd.protoName}({cmdparamsStr})";
                        if (alreadyPrototyped.Contains(strCheck)) continue;

                        swDelegates.WriteLine();
                        swDelegates.WriteLine($"{i2}delegate {cmd.protoPType} _{cmd.protoName}{suffix}({cmdparamsStr});");
                        swDelegates.WriteLine($"{i2}[GlEntryPoint(\"{cmd.protoName}\")]");
                        swDelegates.WriteLine($"{i2}_{cmd.protoName}{suffix} _{cmd.protoName.StripBegin("gl")}{suffix} {{ get; }}");

                        alreadyPrototyped.Add(strCheck);

                        if (!cmd.paramLst.Any(w => w.HasPtr)) break; // skip ptr and intptr suffixes
                    }
                }

                swDelegates.WriteLine($"{i1}}}");
                swDelegates.WriteLine();
                swDelegates.WriteLine("}");
                swDelegates.Flush();
            }
            #endregion

            #region --- METHODS
            {
                swMethods.WriteLine(@"
/*

    FUNCTION DOCUMENTATION FROM: https://github.com/KhronosGroup/OpenGL-Refpages/blob/master/gl4

    API DOCUMENTATION LICENSE:
    Copyright (C) 2010-2014 Khronos Group.
    This material may be distributed subject to the terms and conditions set forth in
    the Open Publication License, v 1.0, 8 June 1999.                    
    http://opencontent.org/openpub/

*/

                    ");
                swMethods.WriteLine("using System;");
                swMethods.WriteLine("using System.Runtime.CompilerServices;");
                swMethods.WriteLine("using System.Runtime.InteropServices;");
                swMethods.WriteLine();
                swMethods.WriteLine("namespace Avalonia.OpenGL");
                swMethods.WriteLine("{");
                swMethods.WriteLine($"{i1}public unsafe partial class GlInterface");
                swMethods.WriteLine($"{i1}{{");

                {
                    var documented = 0;
                    var documentedErr = 0;
                    var documentedTarget = 0;
                    var documentedFileNotFound = 0;

                    foreach (var cmd in cmds)
                    {
                        ++documentedTarget;

                        var modes = new GLCommandParamMode[] { GLCommandParamMode.direct, GLCommandParamMode.ptr, GLCommandParamMode.intptr };

                        swMethods.WriteLine($"");
                        swMethods.WriteLine($"{i2}// ---");

                        var alreadyPrototyped = new HashSet<string>();

                        foreach (var mode in modes)
                        {
                            var suffix = "";
                            switch (mode)
                            {
                                case GLCommandParamMode.ptr: suffix = "_ptr"; break;
                                case GLCommandParamMode.intptr: suffix = "_intptr"; break;
                            }

                            var cmdparamsStr = string.Join(", ", cmd.paramLst.Select(w => w.ToString(mode)));
                            var cmdparamsNamesWithModifier = string.Join(", ", cmd.paramLst.Select(w => (w.isOut ? "out " : "") + w.name.NormalizedName()));

                            var cmddescr = "";
                            Dictionary<string, string> paramDoc = new Dictionary<string, string>();
                            if (GENDOC)
                            {
                                var docPathfilename = Path.Combine(refDocFolder, $"{cmd.protoName}.xml");

                                try
                                {
                                    if (File.Exists(docPathfilename))
                                    {
                                        var xmlContent = File.ReadAllText(docPathfilename).Replace("\r", "\n").Replace("\n", "");
                                        xmlContent = Regex.Replace(xmlContent, @"<inlineequation>.*</inlineequation>", "");

                                        var docel = XElement.Parse(xmlContent);

                                        var refsect1s = docel.Elements().Where(r => r.Name.LocalName == "refsect1");

                                        foreach (var x in refsect1s)
                                        {
                                            var attid = x.Attributes().First(w => w.Name.LocalName == "id").Value;

                                            switch (attid)
                                            {
                                                case "parameters":
                                                    {
                                                        var variableList = x.Elements().FirstOrDefault(w => w.Name.LocalName == "variablelist");
                                                        if (variableList != null)
                                                        {
                                                            foreach (var v in variableList.Elements())
                                                            {
                                                                var term = "";
                                                                var listItem = "";

                                                                foreach (var ve in v.Elements())
                                                                {
                                                                    switch (ve.Name.LocalName)
                                                                    {
                                                                        case "term":
                                                                            term = ve.Value;
                                                                            break;
                                                                        case "listitem":
                                                                            listItem = ve.Value;
                                                                            break;
                                                                    }
                                                                }
                                                                if (term.Length > 0 && listItem.Length > 0)
                                                                    paramDoc.Add(term, string.Join("\r\n", listItem.Lines().Select(o => o.Trim())));
                                                            }
                                                        }
                                                    }
                                                    break;

                                                case "description":
                                                    {
                                                        cmddescr = string.Join("\r\n", x.Elements().Skip(1)
                                                        .SelectMany(w => w.Value.Lines().Select(y => y.Trim())));
                                                    }
                                                    break;
                                            }
                                            ;
                                        }

                                        ++documented;
                                        ;
                                    }
                                    else ++documentedFileNotFound;
                                }
                                catch (Exception ex)
                                {
                                    System.Console.Error.WriteLine($"error parsing [{docPathfilename}]: {ex.Message}");
                                    ++documentedErr;
                                }
                            }

                            var pname = $"{cmd.protoName.StripBegin("gl")}";//{suffix}";
                            var checkStr = $"{cmd.protoPType} {pname}({cmdparamsStr})";
                            if (alreadyPrototyped.Contains(checkStr)) continue;

                            swMethods.WriteLine();
                            if (cmddescr.Length > 0)
                            {
                                swMethods.WriteLine($"{i2}/// <summary>");
                                var ll = cmddescr.Lines();
                                foreach (var l in ll)
                                    swMethods.WriteLine($"{i2}/// {l}");
                                swMethods.WriteLine($"{i2}/// </summary>");
                                foreach (var p in paramDoc)
                                {
                                    swMethods.WriteLine($"{i2}/// <param name=\"{p.Key}\">{p.Value}</param>");
                                }
                            }
                            swMethods.Write($"{i2}[MethodImpl(MethodImplOptions.AggressiveInlining)]");
                            string custom = null;

                            if (customMethodWrap.TryGetValue(pname, out custom))
                            {
                                if (!custom.StartsWith("\r\n"))
                                    swMethods.WriteLine();

                                swMethods.WriteLine(custom);
                            }
                            else
                            {
                                swMethods.WriteLine();

                                swMethods.WriteLine($"{i2}public {cmd.protoPType} {pname}({cmdparamsStr}) => _{cmd.protoName.StripBegin("gl")}{suffix}({cmdparamsNamesWithModifier});");
                            }

                            alreadyPrototyped.Add(checkStr);

                            if (!cmd.paramLst.Any(w => w.HasPtr)) break; // skip ptr and intptr suffixes
                        }
                    }

                    swMethods.WriteLine();
                    swMethods.WriteLine($"{i1}}}");
                    swMethods.WriteLine();
                    swMethods.WriteLine("}");

                    if (GENDOC)
                    {
                        System.Console.Error.WriteLine($"{documented} fn documented + {documentedErr} err + {documentedFileNotFound} notfound over {documentedTarget}");
                    }
                }
            }
            #endregion

            swMethods.Close();
            swDelegates.Close();
            swEnums.Close();
            swConsts.Close();
        }
    }
}
