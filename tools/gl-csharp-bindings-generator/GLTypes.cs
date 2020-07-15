using System;

namespace gl_csharp_bindings_generator
{

    public static partial class Utils
    {        

        public static string NormalizedName(this string name)
        {
            switch (name)
            {
                case "ref":
                case "params":
                case "object":
                case "string":
                case "in":
                case "event":
                case "base":
                    return "@" + name;
                default: return name;
            }
        }

        public static string NormalizedType(this string type, GLCommandParam param, ParseContext parseContext)
        {
            switch (type)
            {

                case "GLintptr":
                case "GLsizeiptr":
                case "GLsizeiptrARB":
                case "GLintptrARB":
                case "GLeglClientBufferEXT":
                case "struct _cl_context":
                case "struct _cl_event":
                case "GLeglImageOES":
                case "GLvdpauSurfaceNV":
                    return "IntPtr";

                case "GLint":
                case "GLbitfield":
                case "GLhandle":
                case "GLhandleARB":
                case "GLsizei":
                case "GLsync":
                case "GLclampx":
                case "ShadingRateQCOM":
                    return "int";

                case "GLuint":
                    return "uint";

                case "GLint64":
                case "GLint64EXT":
                    return "Int64";

                case "GLuint64":
                case "uint64":
                case "GLuint64EXT":
                    return "UInt64";

                case "GLchar":
                case "GLcharARB":
                    return "char";

                case "GLfloat":
                case "GLclampf":
                case "GLhalfNV":
                case "GLfixed":
                    return "float";

                case "GLdouble":
                case "GLclampd":
                    return "double";

                case "GLshort":
                    return "short";

                case "GLushort":
                    return "ushort";

                case "GLubyte":
                    return "byte";

                case "GLbyte":
                    return "sbyte";

                case "boolean":
                case "GLboolean":
                    return "bool";

                case "GLenum":
                    {
                        if (param?.group != null && parseContext.GroupToEnums.ContainsKey(param.group))
                            return param.group;
                        else
                            return "int";
                    }

                case "GLDEBUGPROCARB":
                case "GLDEBUGPROC":
                case "GLDEBUGPROCKHR":
                case "GLDEBUGPROCAMD":
                    return "GlDebugProc";

                case "GLVULKANPROCNV":
                    return "VulkanProc";

                default: return type;
            }
        }
    }

}