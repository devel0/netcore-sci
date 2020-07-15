using System.Collections.Generic;
using System.Linq;

namespace gl_csharp_bindings_generator
{

    public enum GLCommandParamMode { direct, ptr, intptr };

    public class GLCommandParam
    {
        public ParseContext ParseContext => Command.ParseContext;
        public GLCommand Command { get; private set; }

        public GLCommandParam(GLCommand command)
        {
            this.Command = command;
        }

        public string name { get; set; }

        public string type { get; set; }

        public string group { get; set; }

        public string len { get; set; }

        public string txtBeforeName { get; set; }
        public string txtBeforePType { get; set; }

        public bool HasPtr => txtBeforeName.Count(w => w == '*') > 0;

        // static HashSet<string> _outTypes;
        // static HashSet<string> outTypes
        // {
        //     get
        //     {
        //         if (_outTypes == null)
        //         {
        //             _outTypes = new HashSet<string>()
        //             {
        //                 "int",
        //                 "uint",
        //                 "float",
        //                 "double",
        //                 "bool",
        //                 "byte",
        //                 "short",
        //                 "sbyte",
        //                 "Int64"
        //             };
        //         }
        //         return _outTypes;
        //     }
        // }

        int _ConstCnt => (txtBeforeName.Contains("const") ? 1 : 0) + (txtBeforePType.Contains("const") ? 1 : 0);

        int _PtrCnt => txtBeforeName.Count(w => w == '*');

        public bool isOut => len != null && len == "1" && _ConstCnt == 0 && _PtrCnt == 1;

        public string ToString(GLCommandParamMode mode)
        {
            var typeStr = type.NormalizedType(this, ParseContext);

            var ptrCnt = _PtrCnt;
            var constCnt = _ConstCnt;
            var isVoid = typeStr == "void" || txtBeforePType.Contains("void") || txtBeforeName.Contains("void");

            var t = typeStr;

            // if (Command.protoName == "glGetProgramInfoLog")
            //     ;

            if (ptrCnt > 0)
            {
                if (ptrCnt == 1 || (ptrCnt > 1 && typeStr == "char" && constCnt > 1))
                {
                    if (isVoid)
                    {
                        t = "IntPtr";
                    }
                    else
                    {
                        if (isOut)
                        {
                            t = "out " + typeStr;
                        }
                        else
                        {
                            switch (mode)
                            {
                                case GLCommandParamMode.direct:
                                    {
                                        if (typeStr == "char")
                                        {
                                            if (constCnt > 1)
                                                t = "string[]";
                                            else
                                                t = "string";
                                        }
                                        else
                                            t = t + "[]";
                                    }
                                    break;

                                case GLCommandParamMode.ptr:
                                    {
                                        t = "void*";
                                    }
                                    break;

                                case GLCommandParamMode.intptr:
                                    {
                                        t = "IntPtr";
                                    }
                                    break;
                            }
                        }
                    }
                }
                else
                    t = "IntPtr*";
            }

            return $"{t} {name.NormalizedName()}";
        }
    }
}