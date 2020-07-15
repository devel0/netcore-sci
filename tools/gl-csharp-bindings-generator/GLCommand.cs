using System.Collections.Generic;
using System.Linq;

namespace gl_csharp_bindings_generator
{

    public class GLCommand
    {
        public ParseContext ParseContext { get; private set; }

        public GLCommand(ParseContext parseContext)
        {
            this.ParseContext = parseContext;
        }

        public string comment { get; set; }
        string _protoPType;
        public bool protoIsPtr { get; set; }
        public string protoPType
        {
            get
            {
                if (protoIsPtr) return "IntPtr";
                return _protoPType == null ? "void" : _protoPType.NormalizedType(null, ParseContext);
            }
            set { _protoPType = value; }
        }
        public string protoName { get; set; }
        public List<GLCommandParam> paramLst { get; set; }

        public override string ToString()
        {
            return $"{protoPType} {protoName}({string.Join(",", paramLst.Select(w => w.ToString()))})";
        }
    }

}