using System;

namespace gl_csharp_bindings_generator
{

    public static partial class Utils
    {
        public static Int64? safeParseInt(this string str)
        {
            if (str == null) return null;

            if (str.StartsWith("0x"))
                return Convert.ToInt64(str.Substring(2), 16);

            return Int64.Parse(str);
        }
    }

}