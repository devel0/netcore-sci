using System;
using System.Numerics;
using System.Text;
using static System.Math;

namespace SearchAThing
{

    public static partial class SciExt
    {

        /// <summary>
        /// Retrieve memory contiguous representation of Matrix4x4.
        /// If want to use direct array struct fields are taken in byname sort
        /// check with ObjectLayoutInspector utility ( https://github.com/SergeyTeplyakov/ObjectLayoutInspector )
        ///
        /// PrintLayout<Matrix4x4>();
        ///            
        /// Type layout for 'Matrix4x4'
        /// Size: 64 bytes. Paddings: 0 bytes (%0 of empty space)
        /// |=============================|
        /// |   0-3: Single M11 (4 bytes) |
        /// |-----------------------------|
        /// |   4-7: Single M12 (4 bytes) |
        /// |-----------------------------|
        /// |  8-11: Single M13 (4 bytes) |
        /// |-----------------------------|
        /// | 12-15: Single M14 (4 bytes) |
        /// |-----------------------------|
        /// | 16-19: Single M21 (4 bytes) |
        /// |-----------------------------|
        /// | 20-23: Single M22 (4 bytes) |
        /// |-----------------------------|
        /// | 24-27: Single M23 (4 bytes) |
        /// |-----------------------------|
        /// | 28-31: Single M24 (4 bytes) |
        /// |-----------------------------|
        /// | 32-35: Single M31 (4 bytes) |
        /// |-----------------------------|
        /// | 36-39: Single M32 (4 bytes) |
        /// |-----------------------------|
        /// | 40-43: Single M33 (4 bytes) |
        /// |-----------------------------|
        /// | 44-47: Single M34 (4 bytes) |
        /// |-----------------------------|
        /// | 48-51: Single M41 (4 bytes) |
        /// |-----------------------------|
        /// | 52-55: Single M42 (4 bytes) |
        /// |-----------------------------|
        /// | 56-59: Single M43 (4 bytes) |
        /// |-----------------------------|
        /// | 60-63: Single M44 (4 bytes) |
        /// |=============================|
        /// </summary>
        /// <param name="m">matrix to convert to span</param>
        /// <returns>linear representation of given matrix</returns>
        public static Span<float> ToSpan(this Matrix4x4 m)
        {
            return new Span<float>(new[]
            {
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
            });
        }

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

        /// <summary>
        /// matrix4x4 formatted print suitable for monospace font or terminal display
        /// </summary>
        /// <param name="m">matrix</param>
        /// <param name="dec">decimal to display</param>
        /// <returns>formatted text representation of given matrix</returns>
        public static string Fmt(this Matrix4x4 m, int dec, int width = 10)
        {
            var sb = new StringBuilder();

            var fmt = $"{{0,{width}: 0." + "0".Repeat(dec) + ";-0." + "0".Repeat(dec) + "}";

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

        /// <summary>
        /// extract xyz rotation angles from given rotation matrix.
        /// Reference: [Extracting Euler Angles from a Rotation Matrix](https://pdfs.semanticscholar.org/6681/37fa4b875d890f446e689eea1e334bcf6bf6.pdf) by Mike Day
        /// </summary>
        /// <param name="rotation">rotation matrix</param>
        /// <returns>Vector3 with rotation angles(rad) around wcs xyz axes</returns>
        /// <remarks>      
        /// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Matrix4x4/Matrix4x4Test_0001.cs)
        /// </remarks>
        public static Vector3 ToEulerAngles(this Matrix4x4 rotation)
        {
            var m = rotation;

            // xangle
            var x = (float)Atan2(m.M23, m.M33);

            // yangle
            var c2 = Sqrt(m.M11 * m.M11 + m.M12 * m.M12);
            var y = (float)Atan2(-m.M13, c2);

            // zangle
            var s1 = Sin(x);
            var c1 = Cos(x);
            var z = (float)Atan2(s1 * m.M31 - c1 * m.M21, c1 * m.M22 - s1 * m.M32);

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// decompose given 4x4 matrix into scale, rotation, translation
        /// </summary>        
        public static (Vector3 scale, Quaternion rotation, Vector3 translation, bool success)
            Decompose(this Matrix4x4 m)
        {
            var scale = new Vector3();
            var translation = new Vector3();
            var rotation = new Quaternion();

            var success = Matrix4x4.Decompose(m, out scale, out rotation, out translation);

            return (scale, rotation, translation, success);
        }

    }

}