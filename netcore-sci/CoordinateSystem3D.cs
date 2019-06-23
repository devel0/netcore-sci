using SearchAThing.Sci;
using System.Text;
using static System.Math;

namespace SearchAThing
{

    namespace Sci
    {

        public enum CoordinateSystem3DAutoEnum
        {
            /// <summary>
            /// Arbitrary Axis Alghoritm ( dxf spec )
            /// </summary>
            AAA,

            /// <summary>
            /// Strand7 ( Element Library : Beam Principal Axis System ; Default principal axes of a beam element )
            /// Note: Normal must beam Start to End direction
            /// </summary>
            St7
        }

        public partial class CoordinateSystem3D
        {

            Matrix3D m;
            Matrix3D mInv;

            public Vector3D Origin { get; private set; }
            public Vector3D BaseX { get; private set; }
            public Vector3D BaseY { get; private set; }
            public Vector3D BaseZ { get; private set; }

            /// <summary>
            /// right handed XY ( Z ) : top view
            /// </summary>
            public static CoordinateSystem3D XY =
                new CoordinateSystem3D(Vector3D.Zero, Vector3D.XAxis, Vector3D.YAxis, Vector3D.ZAxis);

            /// <summary>
            /// right handed XZ ( -Y ) : front view
            /// </summary>
            public static CoordinateSystem3D XZ =
                new CoordinateSystem3D(Vector3D.Zero, Vector3D.XAxis, Vector3D.ZAxis, -Vector3D.YAxis);

            /// <summary>
            /// right handed YZ ( X ) : side view
            /// </summary>
            public static CoordinateSystem3D YZ =
                new CoordinateSystem3D(Vector3D.Zero, Vector3D.YAxis, Vector3D.ZAxis, Vector3D.XAxis);

            public static CoordinateSystem3D WCS = XY;

            const double aaaSmall = 1.0 / 64;

            public CoordinateSystem3D(Vector3D o, Vector3D normal, CoordinateSystem3DAutoEnum csAutoType = CoordinateSystem3DAutoEnum.AAA)
            {
                Origin = o;

                switch (csAutoType)
                {
                    case CoordinateSystem3DAutoEnum.AAA:
                        {
                            Vector3D Ax = null;

                            if (Abs(normal.X) < aaaSmall && Abs(normal.Y) < aaaSmall)
                                Ax = Vector3D.YAxis.CrossProduct(normal).Normalized();
                            else
                                Ax = Vector3D.ZAxis.CrossProduct(normal).Normalized();

                            var Ay = normal.CrossProduct(Ax).Normalized();
                            
                            BaseX = Ax;
                            BaseY = Ay;
                            BaseZ = Ax.CrossProduct(Ay).Normalized();
                        }
                        break;

                    case CoordinateSystem3DAutoEnum.St7:
                        {                            
                            BaseZ = normal.Normalized();

                            // axis 2
                            if (BaseZ.IsParallelTo(Constants.NormalizedLengthTolerance, Vector3D.ZAxis))
                                BaseY = Vector3D.YAxis;
                            else
                                BaseY = Vector3D.ZAxis.CrossProduct(BaseZ).Normalized();

                            // axis 1
                            BaseX = BaseY.CrossProduct(BaseZ).Normalized();
                        }
                        break;
                }

                m = Matrix3D.FromVectorsAsColumns(BaseX, BaseY, BaseZ);
                mInv = m.Inverse();
            }

            /// <summary>
            /// Construct a coordinate system with the given origin and orthonormal bases
            /// </summary>        
            public CoordinateSystem3D(Vector3D o, Vector3D baseX, Vector3D baseY, Vector3D baseZ)
            {
                Origin = o;
                BaseX = baseX;
                BaseY = baseY;
                BaseZ = baseZ;

                m = Matrix3D.FromVectorsAsColumns(BaseX, BaseY, BaseZ);
                mInv = m.Inverse();
            }

            /// <summary>
            /// Construct a right-hand coordinate system with the given origin and bases such as:
            /// BaseX = v1
            /// BaseZ = v1 x BaseY
            /// BaseY = BaseZ x BaseX
            /// </summary>        
            public CoordinateSystem3D(Vector3D o, Vector3D v1, Vector3D v2)
            {
                Origin = o;
                BaseX = v1.Normalized();
                BaseZ = v1.CrossProduct(v2).Normalized();
                BaseY = BaseZ.CrossProduct(BaseX).Normalized();

                m = Matrix3D.FromVectorsAsColumns(BaseX, BaseY, BaseZ);
                mInv = m.Inverse();
            }

            /// <summary>
            /// Transform given wcs vector into this ucs
            /// </summary>        
            public Vector3D ToUCS(Vector3D p)
            {
                return mInv * (p - Origin);
            }

            /// <summary>
            /// transform given CS coordinate to WCS
            /// </summary>            
            public Vector3D ToWCS(Vector3D p)
            {
                return m * p + Origin;
            }

            /// <summary>
            /// states if this cs have Z base parallel to the other given cs
            /// </summary>            
            public bool IsParallelTo(double tol, CoordinateSystem3D other)
            {
                return BaseZ.IsParallelTo(tol, other.BaseZ);
            }

            /// <summary>
            /// return another cs with origin translated
            /// </summary>            
            public CoordinateSystem3D Move(Vector3D delta)
            {
                return new CoordinateSystem3D(Origin + delta, BaseX, BaseY, BaseZ);
            }

            /// <summary>
            /// return another cs rotated respect given axis
            /// </summary>            
            public CoordinateSystem3D Rotate(Line3D axis, double angleRad)
            {
                var bx = new Line3D(Origin, Origin + BaseX).RotateAboutAxis(axis, angleRad);
                var by = new Line3D(Origin, Origin + BaseY).RotateAboutAxis(axis, angleRad);
                var bz = new Line3D(Origin, Origin + BaseZ).RotateAboutAxis(axis, angleRad);

                return new CoordinateSystem3D(
                    Origin.RotateAboutAxis(axis, angleRad),
                    (bx - Origin).Normalized().V,
                    (by - Origin).Normalized().V,
                    (bz - Origin).Normalized().V);
            }

            /// <summary>
            /// return another cs with same origin and base vector rotated about given vector            
            /// </summary>            
            public CoordinateSystem3D Rotate(Vector3D vectorAxis, double angleRad)
            {
                return new CoordinateSystem3D(
                    Origin,
                    BaseX.RotateAboutAxis(vectorAxis, angleRad),
                    BaseY.RotateAboutAxis(vectorAxis, angleRad),
                    BaseZ.RotateAboutAxis(vectorAxis, angleRad));
            }

            public override string ToString()
            {
                return $"O:{Origin} X:{BaseX} Y:{BaseY} Z:{BaseZ}";
            }

            /// <summary>
            /// CS RGB ( X=RED Y=GREEN Z=BLUE )
            /// </summary>            
            public string ToCadString(double axisLen)
            {
                var sb = new StringBuilder();

                sb.Append(string.Format("-COLOR 1\r\n"));
                sb.Append(new Line3D(Origin, BaseX * axisLen, Line3DConstructMode.PointAndVector).CadScript);
                sb.Append("\r\n");

                sb.Append(string.Format("-COLOR 3\r\n"));
                sb.Append(new Line3D(Origin, BaseY * axisLen, Line3DConstructMode.PointAndVector).CadScript);
                sb.Append("\r\n");

                sb.Append(string.Format("-COLOR 5\r\n"));
                sb.Append(new Line3D(Origin, BaseZ * axisLen, Line3DConstructMode.PointAndVector).CadScript);
                sb.Append("\r\n");

                return sb.ToString();
            }

            public string CadScript
            {
                get
                {
                    return ToCadString(1.0);
                }
            }

        }

    }

    public static partial class Extensions
    {

        /// <summary>
        /// project given vector to the given cs ( zap vector z' to 0 )
        /// </summary>        
        public static Vector3D Project(this Vector3D v, CoordinateSystem3D cs)
        {
            return v.ToUCS(cs).Set(OrdIdx.Z, 0).ToWCS(cs);
        }

    }

}
