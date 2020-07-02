using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SearchAThing
{

    public static partial class SciExt
    {

        /// <summary>
        /// exports to a csv string some known fields
        /// note: not really a csv its a tab separated values for debug purpose
        /// just copy and paste
        /// </summary>        
        public static string ToCSV(this IEnumerable<object> lst)
        {
            var finalSb = new StringBuilder();

            if (!lst.Any()) return "";

            var qT = lst.First().GetType();

            var sbHeader = new StringBuilder();

            // header
            foreach (var x in qT.GetProperties())
            {                
                if (x.PropertyType == typeof(Vector3D))
                {                    
                    if (sbHeader.Length > 0) sbHeader.Append('\t');
                    sbHeader.Append($"{x.Name}.X\t{x.Name}.Y\t{x.Name}.Z");
                }
                else
                {                    
                    if (sbHeader.Length > 0) sbHeader.Append('\t');
                    sbHeader.Append($"{x.Name}");
                }                
            }

            finalSb.AppendLine(sbHeader.ToString());

            // data
            foreach (var o in lst)
            {
                var sbLine = new StringBuilder();

                foreach (var x in qT.GetProperties())
                {
                    if (x.PropertyType == typeof(string))
                    {
                        var s = (string)x.GetMethod.Invoke(o, null);

                        if (sbLine.Length > 0) sbLine.Append('\t');
                        sbLine.Append($"{s}");
                    }
                    else if (x.PropertyType == typeof(double))
                    {
                        var d = (double)x.GetMethod.Invoke(o, null);

                        if (sbLine.Length > 0) sbLine.Append('\t');
                        sbLine.Append($"{d}");
                    }
                    else if (x.PropertyType == typeof(Vector3D))
                    {
                        var v = (Vector3D)x.GetMethod.Invoke(o, null);

                        foreach (var c in v.Coordinates)
                        {
                            if (sbLine.Length > 0) sbLine.Append('\t');
                            sbLine.Append($"{c}");
                        }
                    }
                    else
                    {
                        var s = (object)x.GetMethod.Invoke(o, null);

                        if (sbLine.Length > 0) sbLine.Append('\t');
                        sbLine.Append($"{s.ToString()}");
                    }
                }

                finalSb.AppendLine(sbLine.ToString());
            }

            return finalSb.ToString();
        }

    }

}
