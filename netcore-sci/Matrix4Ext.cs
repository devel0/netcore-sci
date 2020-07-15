using System.Numerics;
using System.Text;

namespace SearchAThing
{

    public static partial class SciExt
    {
       
        /// <summary>
        /// retrieve inverse of given matrix
        /// </summary>
        /// <param name="m">4x4 matrix</param>
        /// <returns>inverse of given matrix</returns>
        /// <remarks>can gen exception</remarks>
        public static Matrix4x4 Inverse(this Matrix4x4 m)
        {
            Matrix4x4 invMat;
            if (!Matrix4x4.Invert(m, out invMat))
                throw new System.Exception($"unable to get inverse matrix");
            return invMat;
        }

        public static string Fmt(this Matrix4x4 m, int dec)
        {
            var sb = new StringBuilder();

            var fmt = "{0,10:0." + "0".Repeat(dec) + "}";

            sb.AppendFormat(fmt, m.M11); sb.Append(" ");
            sb.AppendFormat(fmt, m.M12); sb.Append(" ");
            sb.AppendFormat(fmt, m.M13); sb.Append(" ");
            sb.AppendFormat(fmt, m.M14); sb.AppendLine();

            sb.AppendFormat(fmt, m.M21); sb.Append(" ");
            sb.AppendFormat(fmt, m.M22); sb.Append(" ");
            sb.AppendFormat(fmt, m.M23); sb.Append(" ");
            sb.AppendFormat(fmt, m.M24); sb.AppendLine();

            sb.AppendFormat(fmt, m.M31); sb.Append(" ");
            sb.AppendFormat(fmt, m.M32); sb.Append(" ");
            sb.AppendFormat(fmt, m.M33); sb.Append(" ");
            sb.AppendFormat(fmt, m.M34); sb.AppendLine();

            sb.AppendFormat(fmt, m.M41); sb.Append(" ");
            sb.AppendFormat(fmt, m.M42); sb.Append(" ");
            sb.AppendFormat(fmt, m.M43); sb.Append(" ");
            sb.AppendFormat(fmt, m.M44); sb.AppendLine();

            return sb.ToString();
        }

    }

}