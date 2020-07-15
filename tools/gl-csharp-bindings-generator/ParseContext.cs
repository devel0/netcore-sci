using System.Collections.Generic;

namespace gl_csharp_bindings_generator
{
    public class ParseContext
    {
        Dictionary<string, List<GLEnum>> grpToEnums = new Dictionary<string, List<GLEnum>>();

        public IReadOnlyDictionary<string, List<GLEnum>> GroupToEnums => grpToEnums;

        public void GroupAddEnum(string group, GLEnum @enum)
        {
            List<GLEnum> lst = null;
            if (!grpToEnums.TryGetValue(group, out lst))
            {
                lst = new List<GLEnum>();
                grpToEnums.Add(group, lst);
            }
            lst.Add(@enum);
        }

        public ParseContext()
        {
        }
    }
}