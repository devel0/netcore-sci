using System;
using System.Collections.Generic;

namespace gl_csharp_bindings_generator
{
  public class GLEnums
    {
        public string ns { get; set; }
        public string type { get; set; }
        public Int64? start { get; set; }
        public Int64? end { get; set; }
        public string vendor { get; set; }
        public string comment { get; set; }

        public List<GLEnum> enums { get; set; }

        public override string ToString()
        {
            return $"{enums.Count} enums";
        }
    }
}