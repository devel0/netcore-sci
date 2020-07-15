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

        public string txtBeforeName { get; set; }
        public string txtBeforePType { get; set; }

        public bool HasPtr => txtBeforeName.Count(w => w == '*') > 0;

        public string ToString(GLCommandParamMode mode)
        {
            var typeStr = type.NormalizedType(this, ParseContext);

            var ptrCnt = txtBeforeName.Count(w => w == '*');
            var constCnt = (txtBeforeName.Contains("const") ? 1 : 0) + (txtBeforePType.Contains("const") ? 1 : 0);
            var isVoid = typeStr == "void" || txtBeforePType.Contains("void") || txtBeforeName.Contains("void");

            var t = typeStr;

            // if (Command.protoName == "glGetShaderInfoLog")
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
                else
                    t = "IntPtr*";
            }

            return $"{t} {name.NormalizedName()}";
        }
    }
}