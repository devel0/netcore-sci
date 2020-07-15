using System;

namespace gl_csharp_bindings_generator
{
    public class GLEnum
    {
        public UInt32 value { get; set; }
        public string name { get; set; }
        public string fqName => api == null ? name : $"{api}_{name}";
        public string api { get; set; }
        public string group { get; set; }

        /// <summary>
        /// u (unsigned 32 bit) ;
        /// ull (unsigned 64 bit )
        /// </summary>        
        public string type { get; set; }

        public string alias { get; set; }
    }
}