using System;
using System.Numerics;
using System.Text;
using static System.Math;
using System.Collections.Generic;

using netDxf.Tables;
using Vector3 = System.Numerics.Vector3;
using Newtonsoft.Json;

using static SearchAThing.SciToolkit;
using netDxf;

namespace SearchAThing
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

    public enum SmartCsMode
    {
        /// <summary>
        /// first vector must parallel csXaxis ; second vector must lie on desired csXY plane and not parallel to csXaxis ;
        /// csZaxis is computed as csXaxis cross second vector
        /// </summary>
        X_YQ,

        /// <summary>
        /// first vector must parallel to csZaxis ; second vector must parallel to csXaxis
        /// </summary>
        Z_X,

        /// <summary>
        /// first vector must parallel to csZaxis ; second vector must parallel to csYaxis
        /// </summary>
        Z_Y
    }

    /// <summary>
    /// CS with origin and basex, basey, basez orthonormal vectors.
    /// WCS coord can be translated to this cs using ToUCS() method.
    /// UCS coord can be translated back to wcs using ToWCS() method.
    /// </summary>
    public partial class CoordinateSystem3D
    {

        Matrix3D? _m = null;
        Matrix3D m
        {
            get
            {
                if (_m == null) _m = Matrix3D.FromVectorsAsColumns(BaseX, BaseY, BaseZ);
                return _m;
            }
        }

        Matrix3D? _mInv = null;
        Matrix3D mInv
        {
            get
            {
                if (_mInv == null) _mInv = m.Inverse();
                return _mInv;
            }
        }

        /// <summary>
        /// not null if this cs basex, basey honors cs auto type rule
        /// </summary>        
        public CoordinateSystem3DAutoEnum? csAutoType { get; private set; } = null;

        /// <summary>
        /// origin of cs where x,y,z base vectors applied
        /// </summary>            
        public Vector3D Origin { get; private set; }

        /// <summary>
        /// cs x versor ( normalized )
        /// </summary>            
        public Vector3D BaseX { get; private set; }

        /// <summary>
        /// cs y versor ( normalized )
        /// </summary>            
        public Vector3D BaseY { get; private set; }

        /// <summary>
        /// cs z versor ( normalized )
        /// </summary>            
        public Vector3D BaseZ { get; private set; }

        /// <summary>
        /// right handed XY ( Z ) : top view
        /// </summary>
        public static readonly CoordinateSystem3D XY =
            new CoordinateSystem3D(Vector3D.Zero, Vector3D.XAxis, Vector3D.YAxis, Vector3D.ZAxis);

        /// <summary>
        /// right handed XZ ( -Y ) : front view
        /// </summary>
        public static readonly CoordinateSystem3D XZ =
            new CoordinateSystem3D(Vector3D.Zero, Vector3D.XAxis, Vector3D.ZAxis, -Vector3D.YAxis);

        /// <summary>
        /// right handed YZ ( X ) : side view
        /// </summary>
        public static readonly CoordinateSystem3D YZ =
            new CoordinateSystem3D(Vector3D.Zero, Vector3D.YAxis, Vector3D.ZAxis, Vector3D.XAxis);

        /// <summary>
        /// world cs : basex=(1,0,0) basey=(0,1,0) basez=(0,0,1)
        /// </summary>
        /// <remarks>      
        /// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0001.cs)
        /// </remarks>
        public static readonly CoordinateSystem3D WCS = XY;

        /// <summary>
        /// constant used for arbitrary axis alghoritm cs construction
        /// </summary>
        const double aaaSmall = 1d / 64;

        [JsonConstructor]
        CoordinateSystem3D()
        {

        }

        /// <summary>
        /// build coordinate system with given origin and given BaseZ on given vector normal;
        /// given normal will subjected to normalization;
        /// depending on csAutoType one or another arbitrary axis alghoritm will used to build cs from a point and a normal.
        /// </summary>
        /// <param name="o">origin of cs</param>
        /// <param name="normal">Z vector of cs</param>
        /// <param name="csAutoType">auto cs type</param>
        public CoordinateSystem3D(Vector3D o, Vector3D normal, CoordinateSystem3DAutoEnum csAutoType = CoordinateSystem3DAutoEnum.AAA)
        {
            this.csAutoType = csAutoType;
            Origin = o;

            switch (csAutoType)
            {
                case CoordinateSystem3DAutoEnum.AAA:
                    {
                        Vector3D Ax;

                        normal = normal.Normalized();

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
                        if (BaseZ.IsParallelTo(NormalizedLengthTolerance, Vector3D.ZAxis))
                            BaseY = Vector3D.YAxis;
                        else
                            BaseY = Vector3D.ZAxis.CrossProduct(BaseZ).Normalized();

                        // axis 1
                        BaseX = BaseY.CrossProduct(BaseZ).Normalized();
                    }
                    break;

                default:
                    throw new Exception($"unknown cs {csAutoType}");
            }

            //m = Matrix3D.FromVectorsAsColumns(BaseX, BaseY, BaseZ);
            //mInv = m.Inverse();
        }

        /// <summary>
        /// construct a coordinate system with the given origin and orthonormal bases
        /// note that given bases MUST already normalized
        /// </summary>
        /// <param name="o">cs origin</param>
        /// <param name="baseX">cs X base ( must already normalized )</param>
        /// <param name="baseY">cs Y base ( must already normalized )</param>
        /// <param name="baseZ">cs Z base ( must already normalized )</param>        
        public CoordinateSystem3D(Vector3D o, Vector3D baseX, Vector3D baseY, Vector3D baseZ)
        {
            Origin = o;
            BaseX = baseX;
            BaseY = baseY;
            BaseZ = baseZ;
        }

        /// <summary>
        /// if BaseZ matches one of XY, XZ, YZ default cs then a new cs with origin preserved but baseX, baseY, baseZ overriden will returned.
        /// </summary>        
        public CoordinateSystem3D Simplified()
        {
            if (BaseZ.EqualsTol(NormalizedLengthTolerance, CoordinateSystem3D.WCS.BaseZ) ||
                BaseZ.EqualsTol(NormalizedLengthTolerance, -CoordinateSystem3D.WCS.BaseZ))
                return CoordinateSystem3D.WCS.Move(Origin);

            if (BaseZ.EqualsTol(NormalizedLengthTolerance, CoordinateSystem3D.XZ.BaseZ) ||
                BaseZ.EqualsTol(NormalizedLengthTolerance, -CoordinateSystem3D.XZ.BaseZ))
                return CoordinateSystem3D.XZ.Move(Origin);
            
            if (BaseZ.EqualsTol(NormalizedLengthTolerance, CoordinateSystem3D.YZ.BaseZ) ||
                BaseZ.EqualsTol(NormalizedLengthTolerance, -CoordinateSystem3D.YZ.BaseZ))
                return CoordinateSystem3D.YZ.Move(Origin);            

            return this;
        }

        /// <summary>
        /// retrieve a new cs with same origin but basex flipped (Origin, -BaseX, BaseY, -BaseZ)
        /// </summary>        
        public CoordinateSystem3D FlipX() => new CoordinateSystem3D(Origin, -BaseX, BaseY, -BaseZ);

        /// <summary>
        /// retrieve a new cs with same origin but basey flipped (Origin, BaseX, -BaseY, -BaseZ)
        /// </summary>        
        public CoordinateSystem3D FlipY() => new CoordinateSystem3D(Origin, BaseX, -BaseY, -BaseZ);

        /// <summary>
        /// retrieve a new cs with same origin but basez inverted (Origin, BaseX, -BaseY, -BaseZ)
        /// </summary>        
        public CoordinateSystem3D FlipZ() => new CoordinateSystem3D(Origin, BaseX, -BaseY, -BaseZ);

        /// <summary>
        /// Construct a right-hand coordinate system with the given origin and two vector
        /// </summary>
        /// <param name="o">cs origin</param>
        /// <param name="v1">first vector</param>
        /// <param name="v2">second vector</param>
        /// <param name="mode">specify how to consider first and second vector to build the cs</param>
        public CoordinateSystem3D(Vector3D o, Vector3D v1, Vector3D v2, SmartCsMode mode = SmartCsMode.X_YQ)
        {
            Origin = o;

            switch (mode)
            {
                case SmartCsMode.X_YQ:
                    {
                        BaseX = v1.Normalized();
                        BaseZ = v1.CrossProduct(v2).Normalized();
                        BaseY = BaseZ.CrossProduct(BaseX).Normalized();
                    }
                    break;

                case SmartCsMode.Z_X:
                    {
                        BaseY = v1.CrossProduct(v2).Normalized();
                        BaseX = v2.Normalized();
                        BaseZ = v1.Normalized();
                    }
                    break;

                case SmartCsMode.Z_Y:
                    {
                        BaseX = v2.CrossProduct(v1).Normalized();
                        BaseY = v2.Normalized();
                        BaseZ = v1.Normalized();
                    }
                    break;

                default:
                    throw new Exception($"unknown cs mode {mode}");
            }

            //m = Matrix3D.FromVectorsAsColumns(BaseX, BaseY, BaseZ);
            //mInv = m.Inverse();
        }

        /// <summary>
        /// Transform wcs point to given cs
        /// </summary>
        /// <param name="p">wcs point</param>
        /// <param name="evalCSOrigin">if true CS origin will subtracted from wcs point before transform</param>            
        public Vector3D ToUCS(Vector3D p, bool evalCSOrigin = true) =>
            evalCSOrigin
            ?
            mInv * (p - Origin)
            :
            mInv * p;

        /// <summary>
        /// transform ucs point to wcs
        /// </summary>
        /// <param name="p">ucs point</param>
        /// <param name="evalCSOrigin">if true CS origin will added after transform</param>            
        public Vector3D ToWCS(Vector3D p, bool evalCSOrigin = true) =>
            evalCSOrigin
            ?
            m * p + Origin
            :
            m * p;

        /// <summary>
        /// verify if this cs XY plane contains given wcs point
        /// </summary>
        /// <param name="tol">calc tolerance</param>
        /// <param name="point">point to verify</param>
        /// <param name="evalCSOrigin">if true CS origin will subtracted before transform test</param>
        /// <returns>true if point contained in cs, else otherwise</returns>
        public bool Contains(double tol, Vector3D point, bool evalCSOrigin = true) =>
            point.ToUCS(this, evalCSOrigin).Z.EqualsTol(tol, 0);

        /// <summary>
        /// verify is this cs is equals to otherByLayer ( same origin, x, y, z base vectors )            
        /// </summary>            
        /// <param name="tol">calc tolerance ( for origin check )</param>
        /// <param name="other">cs to check equality against</param>
        /// <returns>true if this cs equals the given on, false otherwise</returns>
        /// <remarks>      
        /// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0001.cs)
        /// </remarks>
        public bool Equals(double tol, CoordinateSystem3D other) =>
            Origin.EqualsTol(tol, other.Origin) &&
            BaseX.EqualsTol(tol, other.BaseX) &&
            BaseY.EqualsTol(tol, other.BaseY) &&
            BaseZ.EqualsTol(tol, other.BaseZ);

        /// <summary>
        /// states if this cs have Z base parallel to the other given cs
        /// </summary>            
        public bool IsParallelTo(double tol, CoordinateSystem3D other) => BaseZ.IsParallelTo(tol, other.BaseZ);

        /// <summary>
        /// return another cs with origin translated
        /// </summary>            
        public CoordinateSystem3D Move(Vector3D delta) =>
            new CoordinateSystem3D(Origin + delta, BaseX, BaseY, BaseZ);

        /// <summary>
        /// create transformed CS by given transformation matrix
        /// </summary>
        public CoordinateSystem3D Transform(Matrix4x4 m)
        {
            var o = Vector3.Transform(Origin, m);
            var p1 = Vector3.Transform(Origin + BaseX, m);
            var p2 = Vector3.Transform(Origin + BaseY, m);

            return new CoordinateSystem3D(o, p1 - o, p2 - o, SmartCsMode.X_YQ);
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
        /// return another cs rotated as from goes toward to
        /// </summary>            
        public CoordinateSystem3D RotateAs(double tol, Vector3D from, Vector3D to)
        {
            var bx = (Origin + BaseX).RotateAs(tol, from, to);
            var by = (Origin + BaseY).RotateAs(tol, from, to);
            var bz = (Origin + BaseZ).RotateAs(tol, from, to);

            return new CoordinateSystem3D(
                Origin.RotateAs(tol, from, to),
                (bx - Origin).Normalized(),
                (by - Origin).Normalized(),
                (bz - Origin).Normalized());
        }

        /// <summary>
        /// return another cs with same origin and base vector rotated about given vector            
        /// </summary>            
        public CoordinateSystem3D Rotate(Vector3D vectorAxis, double angleRad) =>
            new CoordinateSystem3D(
                Origin,
                BaseX.RotateAboutAxis(vectorAxis, angleRad),
                BaseY.RotateAboutAxis(vectorAxis, angleRad),
                BaseZ.RotateAboutAxis(vectorAxis, angleRad));

        /// <summary>
        /// return intersect line between two cs xy planes
        /// </summary>
        /// <param name="tol">len tolernace</param>
        /// <param name="other">other cs</param>
        /// <returns>null if cs parallel to the given other</returns>
        public Line3D? Intersect(double tol, CoordinateSystem3D other)
        {
            if (this.IsParallelTo(tol, other)) return null;

            var l1 = new Line3D(other.Origin, other.BaseX, Line3DConstructMode.PointAndVector);
            var l2 = new Line3D(other.Origin, other.BaseY, Line3DConstructMode.PointAndVector);

            var i1 = l1.Intersect(tol, this);
            var i2 = l2.Intersect(tol, this);

            if (i1 == null) i1 = i2 + l1.V;
            else if (i2 == null) i2 = i1 + l2.V;

            return new Line3D(i1, i2);
        }

        public UCS ToDxfUCS(string name) => new UCS(name, Origin, BaseX, BaseY);

        /// <summary>
        /// retrieve a set of 3 dxf line (RED:x, GREEN:y, BLUE:z) representing CS
        /// </summary>
        public IEnumerable<netDxf.Entities.EntityObject> ToDxfLines(double len = 1)
        {
            yield return Origin.LineV(len * BaseX).DxfEntity.Set(ent => ent.SetColor(AciColor.Red));
            yield return Origin.LineV(len * BaseY).DxfEntity.Set(ent => ent.SetColor(AciColor.Green));
            yield return Origin.LineV(len * BaseZ).DxfEntity.Set(ent => ent.SetColor(AciColor.Blue));
        }

        /// <summary>
        /// debug string
        /// </summary>
        /// <returns>formatted representation of cs origin, x, y, z</returns>
        public string ToString(int digits) => $"O:{Origin.ToString(digits)} X:{BaseX.ToString(digits)} Y:{BaseY.ToString(digits)} Z:{BaseZ.ToString(digits)}";

        public override string ToString() => ToString(3);

        /// <summary>
        /// script to paste in cad to draw cs rgb mode ( x=red, y=green, z=blue )
        /// </summary>                        
        /// <param name="axisLen">length of x,y,z axes</param>
        /// <returns>cad script</returns>
        public string ToCadString(double axisLen)
        {
            var sb = new StringBuilder();

            sb.Append(string.Format("_-COLOR 1\r\n"));
            sb.Append(new Line3D(Origin, BaseX * axisLen, Line3DConstructMode.PointAndVector).CadScript);
            sb.Append("\r\n");

            sb.Append(string.Format("_-COLOR 3\r\n"));
            sb.Append(new Line3D(Origin, BaseY * axisLen, Line3DConstructMode.PointAndVector).CadScript);
            sb.Append("\r\n");

            sb.Append(string.Format("_-COLOR 5\r\n"));
            sb.Append(new Line3D(Origin, BaseZ * axisLen, Line3DConstructMode.PointAndVector).CadScript);
            sb.Append("\r\n");

            return PostProcessCadScript(sb.ToString());
        }

        /// <summary>
        /// script to paste in cad ( axis length = 1 )
        /// </summary>
        public string CadScript => ToCadString(1.0);

    }

    public static partial class SciExt
    {


    }

}
