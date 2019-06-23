using System;
using System.Collections.Generic;
using SearchAThing.Sci;
using System.Linq;
using System.Text;
using System.Globalization;
using SearchAThing.Util;
using SearchAThing.PsqlUtil;
using Newtonsoft.Json;
using static System.Math;
using static System.FormattableString;
using netDxf.Entities;
using netDxf;

namespace SearchAThing
{

    namespace Sci
    {
        public partial class Vector3D : Geometry
        {

            public static Vector3D Zero = new Vector3D(0, 0, 0);
            public static Vector3D XAxis = new Vector3D(1, 0, 0);
            public static Vector3D YAxis = new Vector3D(0, 1, 0);
            public static Vector3D ZAxis = new Vector3D(0, 0, 1);

            [JsonIgnore]
            public override Vector3D GeomFrom => this;

            [JsonIgnore]
            public override Vector3D GeomTo => this;

            public static Vector3D Axis(int ord)
            {
                switch (ord)
                {
                    case 0: return XAxis;
                    case 1: return YAxis;
                    case 2: return ZAxis;
                    default: throw new ArgumentException($"invalid ord {ord} must between 0,1,2");
                }
            }

            public double X { get; private set; }
            public double Y { get; private set; }
            public double Z { get; private set; }

            public Vector3D() : base(GeometryType.Vector3D)
            {
            }

            /// <summary>
            /// build a vector (x,y,0) or (x,y,z) from given 2 or 3 doubles
            /// </summary>            
            public Vector3D(double[] arr) : base(GeometryType.Vector3D)
            {
                X = arr[0];
                Y = arr[1];
                if (arr.Length == 3) Z = arr[2];
                if (arr.Length > 3) throw new Exception($"too much coordinates to build a vector3d");
            }

            public Vector3D(double x, double y, double z) : base(GeometryType.Vector3D)
            {
                X = x; Y = y; Z = z;
            }

            /// <summary>
            /// initialize 3d vector with z implicitly 0
            /// </summary>        
            public Vector3D(double x, double y) : base(GeometryType.Vector3D)
            {
                X = x; Y = y;
            }

            [JsonIgnore]
            public override IEnumerable<Vector3D> Vertexes
            {
                get
                {
                    yield return this;
                }
            }

            /// <summary>
            /// retrieve the component (0:X, 1:Y, 2:Z)
            /// </summary>        
            public double GetOrd(int ord)
            {
                switch (ord)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    default: throw new ArgumentException($"invalid ord {ord}. Must between one of 0,1,2");
                }
            }

            /// <summary>
            /// retrieve the component (0:X, 1:Y, 2:Z)
            /// </summary>        
            public double GetOrd(OrdIdx ord)
            {
                switch (ord)
                {
                    case OrdIdx.X: return X;
                    case OrdIdx.Y: return Y;
                    case OrdIdx.Z: return Z;
                    default: throw new ArgumentException($"invalid ord {ord}. Must between one of 0,1,2");
                }
            }

            public IEnumerable<double> Coordinates
            {
                get
                {
                    yield return X;
                    yield return Y;
                    yield return Z;
                }
            }

            public bool IsZeroLength { get { return (X + Y + Z).EqualsTol(Constants.NormalizedLengthTolerance, 0); } }

            /// <summary>
            /// Note: tol must be Constant.NormalizedLengthTolerance
            /// if comparing normalized vectors
            /// </summary>        
            public bool EqualsTol(double tol, Vector3D other)
            {
                return
                    X.EqualsTol(tol, other.X) &&
                    Y.EqualsTol(tol, other.Y) &&
                    Z.EqualsTol(tol, other.Z);
            }

            /// <summary>
            /// check if this vector equals the given one component by component using EqualsAutoTol
            /// </summary>
            public bool EqualsAutoTol(Vector3D other)
            {
                return
                    X.EqualsAutoTol(other.X) &&
                    Y.EqualsAutoTol(other.Y) &&
                    Z.EqualsAutoTol(other.Z);
            }

            /// <summary>
            /// checks only x,y
            /// </summary>        
            public bool EqualsTol(double tol, double x, double y)
            {
                return X.EqualsTol(tol, x) && Y.EqualsTol(tol, y);
            }

            public bool EqualsTol(double tol, double x, double y, double z)
            {
                return X.EqualsTol(tol, x) && Y.EqualsTol(tol, y) && Z.EqualsTol(tol, z);
            }

            public override double Length { get { return Sqrt(X * X + Y * Y + Z * Z); } }

            public override netDxf.Entities.EntityObject DxfEntity
            {
                get
                {
                    return this.ToDxfPoint();
                }
            }

            public Vector3D Normalized()
            {
                var l = Length;
                return new Vector3D(X / l, Y / l, Z / l);
            }

            /// <summary>
            /// distance between two points
            /// </summary>
            public double Distance(Vector3D other)
            {
                return (this - other).Length;
            }

            /// <summary>
            /// retrieve perpendicular distance of this point from the given line
            /// </summary>            
            public double Distance(double tol, Line3D other)
            {
                return other.Perpendicular(tol, this).Length;
            }

            /// <summary>
            /// distance between two points ( without considering Z )
            /// </summary>            
            public double Distance2D(Vector3D other)
            {
                return Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));
            }

            /// <summary>
            /// Dot product
            /// a b = |a| |b| cos(alfa)
            /// </summary>        
            public double DotProduct(Vector3D other)
            {
                return X * other.X + Y * other.Y + Z * other.Z;
            }

            /// <summary>
            /// states is this vector is perpendicular to the given one
            /// </summary>            
            public bool IsPerpendicular(Vector3D other)
            {
                return Normalized().DotProduct(other.Normalized()).EqualsTol(Constants.NormalizedLengthTolerance, 0);
            }

            /// <summary>
            /// Cross product ( note that resulting vector is not subjected to normalization )
            /// a x b = |a| |b| sin(alfa) N
            /// a x b = |  x  y  z |
            ///         | ax ay az |
            ///         | bx by bz |
            /// https://en.wikipedia.org/wiki/Cross_product
            /// </summary>        
            public Vector3D CrossProduct(Vector3D other)
            {
                return new Vector3D(
                    Y * other.Z - Z * other.Y,
                    -X * other.Z + Z * other.X,
                    X * other.Y - Y * other.X);
            }

            /// <summary>
            /// Angle (rad) between this and other given vector.
            /// Note: tol must be Constant.NormalizedLengthTolerance
            /// if comparing normalized vectors
            /// </summary>        
            public double AngleRad(double tolLen, Vector3D to)
            {
                if (this.EqualsTol(tolLen, to)) return 0;

                // dp = |a| |b| cos(alfa)
                var dp = this.DotProduct(to);

                // alfa = acos(dp / (|a| |b|))
                var L2 = Length * to.Length;
                var w = dp / L2;

                var ang = Acos(w);

                if (double.IsNaN(ang))
                {
                    if (dp * L2 < 0)
                        return PI;
                    else
                        return 0;
                }

                return ang;
            }

            /// <summary>
            /// project this vector to the other given,
            /// the resulting vector will be colinear the given one
            /// </summary>        
            public Vector3D Project(Vector3D to)
            {
                // https://en.wikipedia.org/wiki/Vector_projection
                // http://math.oregonstate.edu/bridge/papers/dot+cross.pdf (fig.1)

                if (to.Length == 0) throw new Exception($"project on null vector");

                return DotProduct(to) / to.Length * to.Normalized();
            }

            /// <summary>
            /// project this vector to the given line
            /// </summary>            
            public Vector3D Project(Line3D line)
            {
                return (this - line.From).Project(line.V) + line.From;
            }


            /// <summary>
            /// return a copy of this vector with ordinate ( 0:x 1:y 2:z ) changed
            /// </summary>            
            public Vector3D Set(OrdIdx ordIdx, double value)
            {
                var x = X;
                var y = Y;
                var z = Z;

                switch (ordIdx)
                {
                    case OrdIdx.X: x = value; break;
                    case OrdIdx.Y: y = value; break;
                    case OrdIdx.Z: z = value; break;
                    default: throw new Exception($"invalid ordIdx:{ordIdx}");
                }

                return new Vector3D(x, y, z);
            }

            /// <summary>
            /// Note: tol must be Constant.NormalizedLengthTolerance
            /// if comparing normalized vectors
            /// </summary>        
            public bool Concordant(double tol, Vector3D other)
            {
                return DotProduct(other) > tol;
            }

            /// <summary>
            /// this relative to given origin
            /// </summary>            
            public Vector3D Rel(Vector3D origin)
            {
                return this - origin;
            }

            public bool Colinear(double tol, Vector3D other)
            {
                return new Line3D(Vector3D.Zero, this).Colinear(tol, new Line3D(Vector3D.Zero, other));
            }

            /// <summary>
            /// Angle (rad) between this going toward the given other vector
            /// rotating (right-hand-rule) around the given comparing axis
            /// Note: tol must be Constant.NormalizedLengthTolerance
            /// if comparing normalized vectors
            /// </summary>        
            public double AngleToward(double tolLen, Vector3D to, Vector3D refAxis)
            {
                var c = this.CrossProduct(to);

                if (c.Concordant(tolLen, refAxis))
                    return this.AngleRad(tolLen, to);
                else
                    return 2 * PI - AngleRad(tolLen, to);
            }

            public Vector3D RotateAboutXAxis(double angleRad)
            {
                var t = new Transform3D();
                t.RotateAboutXAxis(angleRad);
                return t.Apply(this);
            }

            public Vector3D RotateAboutYAxis(double angleRad)
            {
                var t = new Transform3D();
                t.RotateAboutYAxis(angleRad);
                return t.Apply(this);
            }

            public Vector3D RotateAboutZAxis(double angleRad)
            {
                var t = new Transform3D();
                t.RotateAboutZAxis(angleRad);
                return t.Apply(this);
            }

            public Vector3D RotateAboutAxis(Vector3D axis, double angleRad)
            {
                var t = new Transform3D();
                t.RotateAboutAxis(axis.Normalized(), angleRad);
                return t.Apply(this);
            }

            public Vector3D RotateAboutAxis(Line3D axisSegment, double angleRad)
            {
                var vrel = this - axisSegment.From;
                var vrot = vrel.RotateAboutAxis(axisSegment.V, angleRad);
                return vrot + axisSegment.From;
            }

            /// <summary>
            /// Note: tol must be Constant.NormalizedLengthTolerance
            /// if comparing normalized vectors
            /// rotation from-to will be multiplied for given angleFactor ( default 1.0 )
            /// </summary>        
            public Vector3D RotateAs(double tol, Vector3D from, Vector3D to, double angleFactor = 1.0, double angleAddictional = 0)
            {
                var angle = from.AngleRad(tol, to) * angleFactor + angleAddictional;
                var N = from.CrossProduct(to);
                return this.RotateAboutAxis(N, angle);
            }

            /// <summary>
            /// Scale this point about the given origin with the given factor.
            /// </summary>            
            public Vector3D ScaleAbout(Vector3D origin, double factor)
            {
                var d = this - origin;

                return origin + d * factor;
            }

            /// <summary>
            /// Scale this point about the given origin with the given factor as (sx,sy,sz).
            /// </summary>            
            public Vector3D ScaleAbout(Vector3D origin, Vector3D factor)
            {
                var d = this - origin;

                return origin + d * factor;
            }

            /// <summary>
            /// mirror this point about given axis
            /// </summary>            
            public Vector3D Mirror(Line3D axis)
            {
                return this + 2 * (Project(axis) - this);
            }

            /// <summary>
            /// Note: tol must be Constant.NormalizedLengthTolerance
            /// if comparing normalized vectors
            /// </summary>        
            public bool IsParallelTo(double tol, Vector3D other)
            {
                // two vectors a,b are parallel if there is a factor c such that a=cb
                // but first we need to exclude test over null components

                var nullSum = 0;

                var xNull = false;
                var yNull = false;
                var zNull = false;

                if (X.EqualsTol(tol, 0) && other.X.EqualsTol(tol, 0)) { xNull = true; ++nullSum; }
                if (Y.EqualsTol(tol, 0) && other.Y.EqualsTol(tol, 0)) { yNull = true; ++nullSum; }
                if (Z.EqualsTol(tol, 0) && other.Z.EqualsTol(tol, 0)) { zNull = true; ++nullSum; }

                if (nullSum == 0) // 3-d
                {
                    var c = X / other.X;
                    return c.EqualsTol(tol, Y / other.Y) && c.EqualsTol(tol, Z / other.Z);
                }
                else if (nullSum == 1) // 2-d
                {
                    if (xNull) return (Y / other.Y).EqualsTol(tol, Z / other.Z);
                    if (yNull) return (X / other.X).EqualsTol(tol, Z / other.Z);
                    if (zNull) return (X / other.X).EqualsTol(tol, Y / other.Y);
                }
                else if (nullSum == 2) // 1-d
                {
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Convert this wcs considered vector to given cs
            /// </summary>
            public Vector3D ToUCS(CoordinateSystem3D cs)
            {
                return cs.ToUCS(this);
            }

            /// <summary>
            /// Convert this ucs considered vector using given cs to the wcs
            /// </summary>
            public Vector3D ToWCS(CoordinateSystem3D cs)
            {
                return cs.ToWCS(this);
            }

            /// <summary>
            /// Scalar multiply each components
            /// </summary>                
            public Vector3D Scalar(double xs, double ys, double zs)
            {
                return new Vector3D(X * xs, Y * ys, Z * zs);
            }

            /// <summary>
            /// convert each vector component value from to measure units
            /// </summary>
            public Vector3D Convert(MeasureUnit from, MeasureUnit to)
            {
                return new Vector3D(X.Convert(from, to), Y.Convert(from, to), Z.Convert(from, to));
            }

            /// <summary>
            /// convert each vector component value from to measure units
            /// to measure unit is given from the correspondent physical quantity measure unit of from mu        
            /// </summary>
            public Vector3D Convert(MeasureUnit from, IMUDomain to)
            {
                return new Vector3D(X.Convert(from, to), Y.Convert(from, to), Z.Convert(from, to));
            }

            /// <summary>
            /// convert each vector component value from to measure units
            /// from measure unit is given from the correspondent physical quantity measure unit of to mu
            /// </summary>
            public Vector3D Convert(IMUDomain from, MeasureUnit to)
            {
                return new Vector3D(X.Convert(from, to), Y.Convert(from, to), Z.Convert(from, to));
            }

            #region operators

            /// <summary>
            /// indexed vector component
            /// </summary>        
            public double this[int index]
            {
                get
                {
                    if (index == 0) return X;
                    if (index == 1) return Y;
                    if (index == 2) return Z;
                    throw new ArgumentOutOfRangeException("invalid index must between 0-2");
                }
            }

            /// <summary>
            /// sum
            /// </summary>        
            public static Vector3D operator +(Vector3D a, Vector3D b)
            {
                return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
            }

            /// <summary>
            /// negate
            /// </summary>        
            public static Vector3D operator -(Vector3D a)
            {
                return -1.0 * a;
            }

            /// <summary>
            /// sub
            /// </summary>        
            public static Vector3D operator -(Vector3D a, Vector3D b)
            {
                return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
            }

            /// <summary>
            /// scalar mul
            /// </summary>        
            public static Vector3D operator *(double s, Vector3D v)
            {
                return new Vector3D(s * v.X, s * v.Y, s * v.Z);
            }

            /// <summary>
            /// scalar mul
            /// </summary>        
            public static Vector3D operator *(Vector3D v, double s)
            {
                return new Vector3D(s * v.X, s * v.Y, s * v.Z);
            }

            /// <summary>
            /// scalar multiply vector components V1 * V2 =
            /// (V1.x * V2.x, V1.y * V2.y, V1.z * V2.z)
            /// </summary>        
            public static Vector3D operator *(Vector3D v1, Vector3D v2)
            {
                return v1.Scalar(v2.X, v2.Y, v2.Z);
            }

            /// <summary>
            /// scalar div
            /// </summary>        
            public static Vector3D operator /(double s, Vector3D v)
            {
                return new Vector3D(s / v.X, s / v.Y, s / v.Z);
            }

            /// <summary>
            /// scalar div
            /// </summary>        
            public static Vector3D operator /(Vector3D v, double s)
            {
                return new Vector3D(v.X / s, v.Y / s, v.Z / s);
            }

            #endregion

            /// <summary>
            /// Create an array of Vector3D from given list of 2d coords ( eg. { 100, 200, 300, 400 }
            /// will create follow list of vector3d = { (100,200,0), (300,400,0) }
            /// </summary>        
            public static List<Vector3D> From2DCoords(params double[] coords)
            {
                var res = new List<Vector3D>();

                for (var i = 0; i < coords.Length; i += 2)
                    res.Add(new Vector3D(coords[i], coords[i + 1], 0));

                return res;
            }

            /// <summary>
            /// Create an array of Vector3D from given list of 3d coords ( eg. { 100, 200, 10, 300, 400, 20 }
            /// will create follow list of vector3d = { (100,200,10), (300,400,20) }            
            /// </summary>        
            public static List<Vector3D> From3DCoords(params double[] coords)
            {
                var res = new List<Vector3D>();

                for (var i = 0; i < coords.Length; i += 3)
                    res.Add(new Vector3D(coords[i], coords[i + 1], coords[i + 2]));

                return res;
            }

            public static IEnumerable<Vector3D> Random(int N, double L, int seed = 0)
            {
                return Random(N, -L / 2, L / 2, -L / 2, L / 2, -L / 2, L / 2, seed);
            }

            /// <summary>
            /// Span a set of qty vector3d with random coord between given range.
            /// Optionally a seed can be specified for rand or Random obj directly ( in latter case seed aren't used )
            /// </summary>        
            public static IEnumerable<Vector3D> Random(int qty,
                double xmin, double xmax, double ymin, double ymax, double zmin, double zmax, int seed = 0, Random random = null)
            {
                var dx = xmax - xmin;
                var dy = ymax - ymin;
                var dz = zmax - zmin;

                var rnd = (random == null) ? new Random(seed) : random;
                for (int i = 0; i < qty; ++i)
                {
                    yield return new Vector3D(
                        xmin + dx * rnd.NextDouble(),
                        ymin + dy * rnd.NextDouble(),
                        zmin + dz * rnd.NextDouble());
                }
            }

            /*public sVector3D ToSystemVector3D()
            {
                return new sVector3D((float)X, (float)Y, (float)Z);
            }*/

            /// <summary>
            /// parse vector3d from string format "(x y z)" or "(x,y,z)" invariant type
            /// </summary>            
            public static Vector3D FromString(string str)
            {
                var q = str.Trim().StripBegin("(").StripEnd(")").Split(str.Contains(",") ? ',' : ' ');
                var x = double.Parse(q[0], CultureInfo.InvariantCulture);
                var y = double.Parse(q[1], CultureInfo.InvariantCulture);
                var z = 0d;
                if (q.Length > 2) z = double.Parse(q[2], CultureInfo.InvariantCulture);

                return new Vector3D(x, y, z);
            }

            /// <summary>
            /// parse vector3d from array "(x1,y1,z1);(x2,y2,z2)"
            /// </summary>            
            public static IEnumerable<Vector3D> FromStringArray(string str)
            {
                return str.Split(";").Where(f => f.Trim().Length > 0).Select(f => FromString(f));
            }

            /// <summary>
            /// string invariant representation "(x,y,z)"
            /// w/3 decimal places
            /// </summary>            
            public override string ToString()
            {
                return this.ToString(digits: 3);
            }

            /// <summary>
            /// string invariant representation "(x,y,z)" w/given digits
            /// </summary>            
            public string ToString(int digits = 3)
            {
                return Invariant($"({X.ToString(digits)}, {Y.ToString(digits)}, {Z.ToString(digits)})");
            }

            /// <summary>
            /// hash string with given tolerance
            /// </summary>            
            public string ToString(double tol)
            {
                return Invariant($"({X.MRound(tol)}, {Y.MRound(tol)}, {Z.MRound(tol)})");
            }

            /// <summary>
            /// string invariant representation "(x,y,z)"
            /// </summary>            
            public string StringRepresentation()
            {
                return Invariant($"({X}, {Y}, {Z})");
            }

            public string CadScript
            {
                get
                {
                    return string.Format(CultureInfo.InvariantCulture, "_POINT {0},{1},{2}\r\n", X, Y, Z);
                }
            }

            public string CadScriptLineFrom
            {
                get
                {
                    return string.Format(CultureInfo.InvariantCulture, "_LINE {0},{1},{2}\r\n", X, Y, Z);
                }
            }

            public override BBox3D BBox(double tol_len, double tol_rad)
            {
                return new BBox3D(new[] { this });
            }

            public override IEnumerable<Vector3D> Divide(int cnt, bool include_endpoints = false)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// convert given Vector2 to a Vector3D ( with z=0 )
            /// </summary>            
            public static implicit operator Vector3D(Vector2 v)
            {
                return new Vector3D(v.X, v.Y, 0);
            }

            /// <summary>
            /// Convert given Vector3 to Vector3D
            /// </summary>            
            public static implicit operator Vector3D(Vector3 v)
            {
                return new Vector3D(v.X, v.Y, v.Z);
            }

            /// <summary>
            /// Convert given Vector3D to Vector3
            /// </summary>            
            public static implicit operator Vector3(Vector3D v)
            {
                return new Vector3(v.X, v.Y, v.Z);
            }

        }

        public class Vector3DEqualityComparer : IEqualityComparer<Vector3D>
        {
            double tol;

            public Vector3DEqualityComparer(double _tol)
            {
                tol = _tol;
            }

            public bool Equals(Vector3D x, Vector3D y)
            {
                return x.EqualsTol(tol, y);
            }

            public int GetHashCode(Vector3D obj)
            {
                return 0;
            }
        }

        /// <summary>
        /// support class for DistinctKeepOrder extension
        /// </summary>
        public class Vector3DWithOrder
        {
            public int Order { get; private set; }
            public Vector3D Vector { get; private set; }
            public Vector3DWithOrder(Vector3D v, int order)
            {
                Vector = v;
                Order = order;
            }
        }

        public class Vector3DWithOrderEqualityComparer : IEqualityComparer<Vector3DWithOrder>
        {
            Vector3DEqualityComparer cmp;

            public Vector3DWithOrderEqualityComparer(Vector3DEqualityComparer _cmp)
            {
                cmp = _cmp;
            }

            public bool Equals(Vector3DWithOrder x, Vector3DWithOrder y)
            {
                return cmp.Equals(x.Vector, y.Vector);
            }

            public int GetHashCode(Vector3DWithOrder obj)
            {
                return cmp.GetHashCode(obj.Vector);
            }
        }

    }

    public enum OrdIdx
    {
        X = 0,
        Y = 1,
        Z = 2
    }

    public enum CadPointMode
    {
        Point,
        Circle
    };

    public static partial class Extensions
    {

        /// <summary>
        /// retrieve distinct of given vector set ensuring to maintain given order
        /// </summary>        
        public static IEnumerable<Vector3D> DistinctKeepOrder(this IEnumerable<Vector3D> vectors, Vector3DEqualityComparer cmp)
        {
            var ocmp = new Vector3DWithOrderEqualityComparer(cmp);

            return vectors
                .Select((w, i) => new Vector3DWithOrder(w, i))
                .Distinct(ocmp)
                .OrderBy(w => w.Order)
                .Select(w => w.Vector);
        }

        /// <summary>
        /// array invariant string vector3d representation "(x1,y1,z2);(x2,y2,z2)"
        /// </summary>        
        public static string StringRepresentation(this IEnumerable<Vector3D> pts)
        {
            return string.Join(";", pts.Select(g => g.StringRepresentation()));
        }

        /// <summary>
        /// compute length of polyline from given seq_pts
        /// </summary>        
        public static double Length(this IEnumerable<Vector3D> seq_pts)
        {
            var l = 0.0;

            Vector3D prev = null;
            var en = seq_pts.GetEnumerator();
            while (en.MoveNext())
            {
                if (prev != null) l += prev.Distance(en.Current);
                prev = en.Current;
            }

            return l;
        }

        /// <summary>
        /// from a list of vector3d retrieve x1,y1,z1,x2,y2,z2,... coord sequence
        /// </summary>        
        public static IEnumerable<double> ToCoordSequence(this IEnumerable<Vector3D> pts)
        {
            foreach (var p in pts)
            {
                yield return p.X;
                yield return p.Y;
                yield return p.Z;
            }
        }

        /// <summary>
        /// produce a string with x1,y1,x2,y2, ...
        /// </summary>        
        public static string ToCoordString2D(this IEnumerable<Vector3D> points)
        {
            var sb = new StringBuilder();

            var en = points.GetEnumerator();

            if (en.MoveNext())
            {
                while (true)
                {
                    var p = en.Current;
                    sb.Append(string.Format(CultureInfo.InvariantCulture, "{0},{1}", p.X, p.Y));

                    if (en.MoveNext())
                        sb.Append(",");
                    else
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// produce a string with x1,y1,z1,x2,y2,z2, ...
        /// </summary>        
        public static string ToCoordString3D(this IEnumerable<Vector3D> points)
        {
            var sb = new StringBuilder();

            var en = points.GetEnumerator();

            if (en.MoveNext())
            {
                while (true)
                {
                    var p = en.Current;
                    sb.Append(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", p.X, p.Y, p.Z));

                    if (en.MoveNext())
                        sb.Append(",");
                    else
                        break;
                }
            }

            return sb.ToString();
        }

        public static string ToCadScript(this IEnumerable<Vector3D> points, CadPointMode mode = CadPointMode.Point, double radius = 10)
        {
            var sb = new StringBuilder();

            switch (mode)
            {
                case CadPointMode.Point:
                    {
                        foreach (var p in points)
                        {
                            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "_POINT {0},{1},{2}", p.X, p.Y, p.Z));
                            sb.AppendLine();
                        }
                    }
                    break;

                case CadPointMode.Circle:
                    {
                        foreach (var p in points)
                        {
                            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "_CIRCLE {0},{1},{2} {3}", p.X, p.Y, p.Z, radius));
                            sb.AppendLine();
                        }
                    }
                    break;
            }

            return sb.ToString();
        }

        /// <summary>
        /// checks two list of vectors are equals and with same order of elements        
        /// </summary>        
        public static bool EqualsTol(this IEnumerable<Vector3D> lst, double tol, IEnumerable<Vector3D> other)
        {
            var thisEn = lst.GetEnumerator();
            var otherEn = other.GetEnumerator();

            while (thisEn.MoveNext())
            {
                var thisValue = thisEn.Current;

                if (!otherEn.MoveNext()) return false; // other smaller than this

                if (!thisValue.EqualsTol(tol, otherEn.Current)) return false;
            }

            if (otherEn.MoveNext()) return false; // other greather than this

            return true;
        }

        public static Vector3D Sum(this IEnumerable<Vector3D> lst)
        {
            var s = Vector3D.Zero;
            foreach (var v in lst) s += v;

            return s;
        }

        /// <summary>
        /// Same as mean
        /// </summary>
        [Obsolete("use Mean instead")]
        public static Vector3D Center(this IEnumerable<Vector3D> lst)
        {
            return lst.Mean();
        }

        /// <summary>
        /// mean of given vetor3d list
        /// note: if used to compute poly center enable skipFirstAtEnd
        /// </summary>        
        public static Vector3D Mean(this IEnumerable<Vector3D> lst, bool skipFirstAtEnd = false)
        {
            var n = 0;
            var s = Vector3D.Zero;
            foreach (var v in lst) { s += v; ++n; }

            return s / n;
        }

        /*public static Vector3D ToVector3D(this sVector3D v)
        {
            return new Vector3D(v.X, v.Y, v.Z);
        }*/

        public static Vector3D ToVector3D(this netDxf.Vector2 v)
        {
            return new Vector3D(v.X, v.Y);
        }

        /* public static netDxf.Vector3 ToVector3(this Vector3D v)
         {
             return new netDxf.Vector3(v.X, v.Y, v.Z);
         }*/

        public static netDxf.Vector2 ToVector2(this Vector3D v)
        {
            return new netDxf.Vector2(v.X, v.Y);
        }

        /// <summary>
        /// To point (double x, double y)
        /// </summary>        
        public static Point ToPoint(this Vector3D v)
        {
            return new Point(v.X, v.Y, 0);
        }

        /// <summary>
        /// creates a psql double[] string
        /// </summary>
        public static string ToPsql(this Vector3D v)
        {
            return v.Coordinates.ToPsql();
        }

        /// <summary>
        /// create a psql representation of double[] coord sequence x1,y1,z1,x2,y2,z2, ... of given points
        /// </summary>        
        public static string ToPsql(this IEnumerable<Vector3D> pts)
        {
            return pts.ToCoordSequence().ToPsql();
        }

        /// <summary>
        /// return pts (maintaining order) w/out duplicates
        /// use the other overloaded method if already have a vector 3d equality comparer
        /// </summary>        
        public static IEnumerable<Vector3D> ZapDuplicates(this IEnumerable<Vector3D> pts, double tol)
        {
            return pts.ZapDuplicates(new Vector3DEqualityComparer(tol));
        }

        /// <summary>
        /// return pts (maintaining order) w/out duplicates
        /// </summary>        
        public static IEnumerable<Vector3D> ZapDuplicates(this IEnumerable<Vector3D> pts, Vector3DEqualityComparer cmp)
        {
            var hs = new HashSet<Vector3D>(cmp);
            foreach (var p in pts)
            {
                if (!hs.Contains(p))
                {
                    yield return p;
                    hs.Add(p);
                }
            }
        }

        /// <summary>
        /// build polygons from given list of segments
        /// if want to represent arcs, add them as dummy lines to segs
        /// polys returned are ordered anticlockwise
        /// </summary>        
        public static IEnumerable<IReadOnlyList<Vector3D>> ClosedPolys2D(this IEnumerable<Line3D> segs, double tolLen,
            int polyMaxPoints = 0)
        {
            var minCoord = new BBox3D(segs.SelectMany(r => new[] { r.From, r.To })).Min;

            var vcmp = new Vector3DEqualityComparer(tolLen);
            var lcmp = new Line3DEqualityComparer(tolLen);
            var segsDict = segs.ToDictionary(k => k.ToString(tolLen), v => v);
            var segsFromDict = segs.GroupBy(g => g.From, v => v, vcmp).ToDictionary(k => k.Key, v => v.ToList(), vcmp);
            var segsToDict = segs.GroupBy(g => g.To, v => v, vcmp).ToDictionary(k => k.Key, v => v.ToList(), vcmp);

            var segsLeft = segs.OrderBy(w => w.MidPoint.Distance(minCoord)).ToList();
            var polys = new List<List<Vector3D>>();
            var polyCentroidDone = new HashSet<Vector3D>(vcmp);

            while (segsLeft.Count > 0)
            {
                //Console.WriteLine($"segsLeft: {segsLeft.Count} polys:{polys.Count}");

                var seg = segsLeft.First();
                segsLeft.Remove(seg);
                var poly = new List<Vector3D>() { seg.From, seg.To };
                var rotDir = 1.0; // 1=+Z, -1=-Z
                while (true)
                {
                    List<Line3D> segsNext = null;
                    {
                        var hs = new HashSet<Line3D>(lcmp);
                        {
                            List<Line3D> tmp = null;
                            if (segsFromDict.TryGetValue(seg.To, out tmp)) foreach (var x in tmp.Where(r => !r.EqualsTol(tolLen, seg))) hs.Add(x);
                            if (segsToDict.TryGetValue(seg.To, out tmp)) foreach (var x in tmp.Where(r => !r.EqualsTol(tolLen, seg))) hs.Add(x);
                        }
                        segsNext = hs.Select(w => w.EnsureFrom(tolLen, seg.To)).ToList();
                    }

                    Line3D segNext = null;
                    var force_close_poly = false;

                    if (polyMaxPoints > 0 && poly.Count > polyMaxPoints)
                        throw new Exception($"polygon [{poly.PolygonSegments(tolLen).ToCadScript()}] max point exceeded");

                    //#if DEBUG

                    //                    if (//poly.Count >= 2 &&
                    //                        poly.Any(r => r.EqualsTol(1e-2, 31.0626,-0.0018))
                    //                        //&&
                    //                        //poly.Any(r => r.EqualsTol(tolLen, -42.9561, 0))
                    //                        )
                    //                        ;
                    //#endif

                    if (poly.Count == 2)
                    {
                        if (segsNext.Count == 0)
                        {
                            throw new Exception($"check singular segment [{seg}] cadscript [{seg.CadScript}]");
                        }

                        segNext = segsNext
                            .OrderBy(w => (-seg.V).AngleRad(tolLen, w.V))
                            .First();
                        rotDir = seg.V.CrossProduct(segNext.V).Z > 0 ? 1 : -1;
                    }
                    else
                    {
                        var qSegsNext = segsNext
                            .Select(w => new
                            {
                                arad = (seg.V).AngleToward(tolLen, w.V, Vector3D.ZAxis * rotDir),
                                seg = w
                            })
                            .Where(r => r.arad <= PI).ToList();

                        if (qSegsNext.Count == 0)
                        {
                            force_close_poly = true;
                        }
                        else
                        {
                            // retrieve next segment with current rotation direction w/acutest angle
                            segNext = qSegsNext
                                .OrderByDescending(r => r.arad)
                                .First().seg;
                        }
                    }

                    if (force_close_poly) break;

                    segsLeft.Remove(segNext);
                    if (segNext.To.EqualsTol(tolLen, poly[0])) break;
                    poly.Add(segNext.To);

                    seg = segNext;
                }

                if (poly.Count > 2)
                {
                    poly = poly.SortPoly(tolLen, Vector3D.ZAxis).ToList();

                    var polyCentroid = poly.Centroid(tolLen);
                    if (!polyCentroidDone.Contains(polyCentroid))
                    {
                        polys.Add(poly);
                        polyCentroidDone.Add(polyCentroid);
                    }
                }
                else
                {
                    // todo warning
                }
            }

            return polys.OrderByDescending(w => w.Count);
        }

        /// <summary>
        /// create dxf point from given vector3d
        /// </summary>        
        public static netDxf.Entities.Point ToDxfPoint(this Vector3D pt)
        {
            return new netDxf.Entities.Point(new Vector3(pt.X, pt.Y, pt.Z));
        }

    }

}
