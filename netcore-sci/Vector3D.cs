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
using SearchAThing;
using netDxf.Entities;
using netDxf;
using System.Text.RegularExpressions;

namespace SearchAThing
{

    namespace Sci
    {

        /// <summary>
        /// can be used to describe a wcs point or a vector x,y,z components from some reference origin
        /// </summary>
        public partial class Vector3D : Geometry
        {

            /// <summary>
            /// zero vector (0,0,0)            
            /// </summary> 
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0001.cs)
            /// </remarks>           
            public static readonly Vector3D Zero = new Vector3D(0, 0, 0);

            /// <summary>
            /// xaxis vector (1,0,0)            
            /// </summary>   
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0004.cs)
            /// </remarks>
            public static readonly Vector3D XAxis = new Vector3D(1, 0, 0);

            /// <summary>
            /// yaxis vector (0,1,0)            
            /// </summary>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0004.cs)
            /// </remarks>
            public static readonly Vector3D YAxis = new Vector3D(0, 1, 0);

            /// <summary>
            /// zaxis vector (0,0,1)            
            /// </summary>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0004.cs)
            /// </remarks>
            public static readonly Vector3D ZAxis = new Vector3D(0, 0, 1);

            /// <summary>
            /// retrieve wcs axis by given index            
            /// </summary>
            /// <param name="ord">0:(1,0,0) 1:(0,1,0) 2:(0,0,1)</param>                                    
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0004.cs)
            /// </remarks>
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

            /// <summary>
            /// retrieve the component (0:X, 1:Y, 2:Z)            
            /// </summary>        
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0003.cs)
            /// </remarks>
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
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0003.cs)
            /// </remarks>
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

            /// <summary>
            /// X vector component            
            /// </summary>
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0005.cs)
            /// </remarks>
            public double X { get; private set; }

            /// <summary>
            /// Y vector component            
            /// </summary>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0005.cs)
            /// </remarks>
            public double Y { get; private set; }

            /// <summary>
            /// Z vector component            
            /// </summary>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0005.cs)
            /// </remarks>
            public double Z { get; private set; }

            /// <summary>
            /// zero vector            
            /// </summary>
            /// <remarks>  
            /// [unit test](/test/Vector3D/Vector3DTest_0006.cs)
            /// </remarks>
            public Vector3D() : base(GeometryType.Vector3D)
            {
            }

            /// <summary>
            /// build a vector (x,y,0) or (x,y,z) from given 2 or 3 doubles            
            /// </summary>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0007.cs)
            /// </remarks>
            public Vector3D(double[] arr) : base(GeometryType.Vector3D)
            {
                X = arr[0];
                Y = arr[1];
                if (arr.Length == 3) Z = arr[2];
                if (arr.Length > 3) throw new ArgumentException($"too much coordinates to build a vector3d");
            }

            /// <summary>
            /// build a vector by given components            
            /// </summary>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0008.cs)
            /// </remarks>
            public Vector3D(double x, double y, double z) : base(GeometryType.Vector3D)
            {
                X = x; Y = y; Z = z;
            }

            /// <summary>
            /// build a vector (x,y,0) by given components            
            /// </summary>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0008.cs)
            /// </remarks>
            public Vector3D(double x, double y) : base(GeometryType.Vector3D)
            {
                X = x; Y = y;
            }

            static Regex _cad_id_regex = null;
            /// <summary>
            /// static instance of regex to parse cad id string
            /// https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex?view=netcore-3.0#thread-safety
            /// </summary>            
            static Regex cad_id_regex
            {
                get
                {
                    if (_cad_id_regex == null)
                        _cad_id_regex = new Regex(@"X\s*=\s*([-]?[\d\.]*)\s*Y\s*=\s*([-]?[\d\.]*)\s*Z\s*=\s*([-]?[\d\.]*)");
                    return _cad_id_regex;
                }
            }

            /// <summary>
            /// parse cad id string (eg. "X = 4.11641325 Y = 266.06066703 Z = 11.60392802")
            /// constructing a point            
            /// </summary>
            /// <param name="cad_id_string">cad id string</param>
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0009.cs)
            /// </remarks>
            public Vector3D(string cad_id_string) : base(GeometryType.Vector3D)
            {
                var matches = cad_id_regex.Match(cad_id_string);
                if (!matches.Success || matches.Groups.Count != 4)
                    throw new ArgumentException($"unable to parse cad id string [{cad_id_string}]");

                var x = matches.Groups[1].Value.InvDoubleParse();
                var y = matches.Groups[2].Value.InvDoubleParse();
                var z = matches.Groups[3].Value.InvDoubleParse();

                X = x; Y = y; Z = z;
            }

            /// <summary>
            /// enumerate coordinates            
            /// </summary>
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0010.cs)
            /// </remarks>
            public IEnumerable<double> Coordinates
            {
                get
                {
                    yield return X;
                    yield return Y;
                    yield return Z;
                }
            }

            /// <summary>
            /// states if this is a zero vector            
            /// </summary>
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0011.cs)
            /// </remarks>
            public bool IsZeroLength { get { return (X + Y + Z).EqualsTol(Constants.NormalizedLengthTolerance, 0); } }

            /// <summary>
            /// checks vector component equality vs other given                       
            /// </summary>
            /// <param name="tol">geometric tolerance ( note: use Constants.NormalizedLengthTolerance )</param>
            /// <param name="other">vector to compare to this</param>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0012.cs)
            /// </remarks>
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
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0012.cs)
            /// </remarks>
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
            /// <param name="tol">geometric tolerance ( note: use Constants.NormalizedLengthTolerance )</param>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0012.cs)
            /// </remarks>
            public bool EqualsTol(double tol, double x, double y)
            {
                return X.EqualsTol(tol, x) && Y.EqualsTol(tol, y);
            }

            /// <summary>
            /// checks vector component equality vs other given                        
            /// </summary>
            /// <param name="tol">geometric tolerance ( note: use Constants.NormalizedLengthTolerance )</param>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0012.cs)
            /// </remarks>
            public bool EqualsTol(double tol, double x, double y, double z)
            {
                return X.EqualsTol(tol, x) && Y.EqualsTol(tol, y) && Z.EqualsTol(tol, z);
            }

            /// <summary>
            /// create a normalized version of this vector            
            /// </summary>
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0013.cs)
            /// </remarks>
            public Vector3D Normalized()
            {
                var l = Length;
                return new Vector3D(X / l, Y / l, Z / l);
            }

            /// <summary>
            /// compute distance between this point and the other given                        
            /// </summary>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0014.cs)
            /// </remarks>
            public double Distance(Vector3D other)
            {
                return (this - other).Length;
            }

            /// <summary>
            /// compute perpendicular(min) distance of this point from given line            
            /// </summary>
            /// <param name="tol">length tolerance ( used to check if point contained in line )</param>
            /// <param name="other">line</param>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0015.cs)
            /// ![](/test/Vector3D/Vector3DTest_0015.png)
            /// </remarks>
            public double Distance(double tol, Line3D other)
            {
                var q = other.Perpendicular(tol, this);
                if (q == null) return 0;
                return q.Length;
            }

            /// <summary>
            /// compute distance of this point from the given in 2d ( x,y ) without consider z component            
            /// </summary>
            /// <param name="other">other point</param>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0016.cs)
            /// ![](/test/Vector3D/Vector3DTest_0016.png)
            /// </remarks>
            public double Distance2D(Vector3D other)
            {
                return Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));
            }

            /// <summary>
            /// compute dot product of this vector for the given one            
            /// a b = |a| |b| cos(alfa)            
            /// </summary>
            /// <param name="other">second vector</param>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0017.cs)            
            /// </remarks>
            public double DotProduct(Vector3D other)
            {
                return X * other.X + Y * other.Y + Z * other.Z;
            }

            /// <summary>
            /// states is this vector is perpendicular to the given one            
            /// </summary>
            /// <param name="other">other vector</param>
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0018.cs)
            /// </remarks>
            public bool IsPerpendicular(Vector3D other)
            {
                return Normalized().DotProduct(other.Normalized())
                    .EqualsTol(Constants.NormalizedLengthTolerance, 0);
            }

            /// <summary>
            /// Cross product ( not normalized ) ;            
            /// a x b = |a| |b| sin(alfa) N ;        
            /// a x b = |  x  y  z |
            ///         | ax ay az |
            ///         | bx by bz |            
            /// [reference](https://en.wikipedia.org/wiki/Cross_product) ;            
            /// </summary>               
            /// <param name="other">other vector</param>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0019.cs)
            /// ![](/test/Vector3D/Vector3DTest_0019.png)
            /// </remarks>
            public Vector3D CrossProduct(Vector3D other)
            {
                return new Vector3D(
                    Y * other.Z - Z * other.Y,
                    -X * other.Z + Z * other.X,
                    X * other.Y - Y * other.X);
            }

            /// <summary>
            /// angle between this and given vector
            /// </summary>
            /// <param name="tolLen">geometric tolerance to test vector equalities ( use Constants.NormalizedLengthTolerance when comparing normalized vectors )</param>
            /// <param name="to">other vector</param>
            /// <returns>angle between two vectors (rad)</returns>
            /// <remarks>      
            /// [unit test](https://github.com/devel0/netcore-sci/blob/master/test/Vector3D/Vector3DTest_0020.cs)
            /// 
            /// <a href="https://github.com/devel0/netcore-sci/blob/master/test/Vector3D/Vector3DTest_0020.png">![image](https://github.com/devel0/netcore-sci/blob/master/test/Vector3D/Vector3DTest_0020.png)</a>
            /// </remarks>
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
            /// project this vector to the given one
            /// </summary>
            /// <param name="to">other vector</param>
            /// <returns>projected vector ( will be colinear to the given one )</returns>
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0021.cs)
            /// ![](/test/Vector3D/Vector3DTest_0021.png)
            /// </remarks>
            public Vector3D Project(Vector3D to)
            {
                // https://en.wikipedia.org/wiki/Vector_projection
                // http://math.oregonstate.edu/bridge/papers/dot+cross.pdf (fig.1)

                if (to.Length == 0) throw new Exception($"project on null vector");

                return DotProduct(to) / to.Length * to.Normalized();
            }

            /// <summary>
            /// project this point to the given line
            /// </summary>
            /// <param name="line">line to project the point onto</param>
            /// <returns>projected point onto the line ( perpendicularly )</returns>
            public Vector3D Project(Line3D line)
            {
                return (this - line.From).Project(line.V) + line.From;
            }

            /// <summary>
            /// create a point copy of this one with component changed
            /// </summary>
            /// <param name="ordIdx">component to change ( 0:x 1:y 2:z )</param>
            /// <param name="value">value to assign to the component</param>
            /// <returns>new vector with component changed</returns>
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
            /// create a vector relative to given origin from this point and given origin
            /// </summary>
            /// <param name="origin">origin to make this point relative to</param>
            /// <returns>vector</returns>
            public Vector3D Rel(Vector3D origin)
            {
                return this - origin;
            }

            /// <summary>
            /// Note: tol must be Constants.NormalizedLengthTolerance
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
            /// states if this vector is colinear to the given one
            /// </summary>
            /// <param name="tol">geometric tolerance</param>
            /// <param name="other">other vector</param>            
            public bool Colinear(double tol, Vector3D other)
            {
                //return this.IsParallelTo(tol, other);
                return new Line3D(Vector3D.Zero, this).Colinear(tol, new Line3D(Vector3D.Zero, other));
            }

            /// <summary>
            /// states if this vector concord to the given one
            /// 
            /// **NOTE**: it does not test two vectors are parallels ( precondition must meet )
            /// </summary>
            /// <param name="tol">geometric tolerance ( Constants.NormalizedLengthTolerance if comparing normalized vectors )</param>
            /// <param name="other">other vector</param>            
            public bool Concordant(double tol, Vector3D other)
            {
                return DotProduct(other) > tol;
            }

            /// <summary>
            /// statis if this vector is concordant and colinear to the given one
            /// </summary>
            /// <param name="tol">geometric tolerance ( Constants.NormalizedLengthTolerance if comparing normalized vectors )</param>
            /// <param name="other">other vector</param>                        
            public bool ConcordantColinear(double tol, Vector3D other)
            {
                return Concordant(tol, other) && Colinear(tol, other);
            }

            /// <summary>
            /// compute angle required to make this point go to the given one
            /// if rotate right-hand around given reference axis
            /// </summary>
            /// <param name="tolLen">geometric tolerance ( use Constants.NormalizedLengthTolerance if working with normalized vectors )</param>
            /// <param name="to">point toward rotate this one</param>
            /// <param name="refAxis">reference axis to make right-hand rotation of this point toward given one</param>
            /// <returns>angle (rad)</returns>
            public double AngleToward(double tolLen, Vector3D to, Vector3D refAxis)
            {
                var c = this.CrossProduct(to);

                if (c.Concordant(tolLen, refAxis))
                    return this.AngleRad(tolLen, to);
                else
                    return 2 * PI - AngleRad(tolLen, to);
            }

            /// <summary>
            /// rotate this point around x-axis using quaternion
            /// </summary>
            /// <param name="angleRad">angle (rad) of rotation</param>
            /// <returns>rotated point</returns>
            public Vector3D RotateAboutXAxis(double angleRad)
            {
                var t = new Transform3D();
                t.RotateAboutXAxis(angleRad);
                return t.Apply(this);
            }

            /// <summary>
            /// rotate this point around y-axis using quaternion
            /// </summary>
            /// <param name="angleRad">angle (rad) of rotation</param>
            /// <returns>rotated point</returns>
            public Vector3D RotateAboutYAxis(double angleRad)
            {
                var t = new Transform3D();
                t.RotateAboutYAxis(angleRad);
                return t.Apply(this);
            }

            /// <summary>
            /// rotate this point around z-axis using quaternion
            /// </summary>
            /// <param name="angleRad">angle (rad) of rotation</param>
            /// <returns>rotated point</returns>
            public Vector3D RotateAboutZAxis(double angleRad)
            {
                var t = new Transform3D();
                t.RotateAboutZAxis(angleRad);
                return t.Apply(this);
            }

            /// <summary>
            /// rotate this point right-hand around given axis using quaternion
            /// </summary>
            /// <param name="axis">rotation axis</param>
            /// <param name="angleRad">angle (rad) of rotation</param>
            /// <returns>rotated point</returns>
            public Vector3D RotateAboutAxis(Vector3D axis, double angleRad)
            {
                var t = new Transform3D();
                t.RotateAboutAxis(axis.Normalized(), angleRad);
                return t.Apply(this);
            }

            /// <summary>
            /// rotate this point right-hand around given segment using quaternion
            /// </summary>
            /// <param name="axisSegment">rotation axis segment</param>
            /// <param name="angleRad">angle (rad) of rotation</param>
            /// <returns>rotated point</returns>
            public Vector3D RotateAboutAxis(Line3D axisSegment, double angleRad)
            {
                var vrel = this - axisSegment.From;
                var vrot = vrel.RotateAboutAxis(axisSegment.V, angleRad);
                return vrot + axisSegment.From;
            }

            /// <summary>
            /// Note: tol must be Constants.NormalizedLengthTolerance
            /// if comparing normalized vectors
            /// rotation from-to will be multiplied for given angleFactor ( default 1.0 )
            /// </summary>      

            /// <summary>
            /// rotate this point using rotation like point from goes toward point to
            /// </summary>
            /// <param name="tol">geometric tolerance ( use Constants.NormalizedLengthTolerance if vectors are normalized )</param>
            /// <param name="from">point from describing rotation path</param>
            /// <param name="to">point to describing rotation path</param>
            /// <param name="angleFactor">optional angle rotation scaler</param>
            /// <param name="angleAddictionalRad">optional angle (rad) component (added after angleFactor scaler)</param>
            /// <returns></returns>
            public Vector3D RotateAs(double tol, Vector3D from, Vector3D to, double angleFactor = 1.0, double angleAddictionalRad = 0)
            {
                var angle = from.AngleRad(tol, to) * angleFactor + angleAddictionalRad;
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
            /// Convert this wcs point to given cs coord
            /// </summary>
            /// <param name="cs">dest CS</param>
            /// <param name="evalCSOrigin">if true CS origin will subtracted before transform</param>            
            public Vector3D ToUCS(CoordinateSystem3D cs, bool evalCSOrigin = true)
            {
                return cs.ToUCS(this, evalCSOrigin);
            }

            /// <summary>
            /// Convert this ucs considered vector using given cs to the wcs
            /// </summary>
            /// <param name="cs">ucs point</param>
            /// <param name="evalCSOrigin">if true CS origin will added after transform</param>
            public Vector3D ToWCS(CoordinateSystem3D cs, bool evalCSOrigin = true)
            {
                return cs.ToWCS(this, evalCSOrigin);
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
                var digits = Math.Max(0, -tol.Magnitude());
                return Invariant($"({X.MRound(tol).ToString(digits)}, {Y.MRound(tol).ToString(digits)}, {Z.MRound(tol).ToString(digits)})");
            }

            /// <summary>
            /// string invariant representation "(x,y,z)"
            /// </summary>            
            public string StringRepresentation()
            {
                return Invariant($"({X}, {Y}, {Z})");
            }

            /// <summary>
            /// cad script for this vector as wcs point
            /// </summary>
            public string CadScript
            {
                get
                {
                    return string.Format(CultureInfo.InvariantCulture, "_POINT {0},{1},{2}\r\n", X, Y, Z);
                }
            }

            /// <summary>
            /// cad script for a line (0,0,0) to this vector
            /// </summary>
            public string CadScriptLine => new Line3D(Vector3D.Zero, this).CadScript;

            /// <summary>
            /// cad script for a line departing from this wcs point
            /// </summary>
            public string CadScriptLineFrom
            {
                get
                {
                    return string.Format(CultureInfo.InvariantCulture, "_LINE {0},{1},{2}\r\n", X, Y, Z);
                }
            }

            #region Geometry implementation

            /// <summary>
            /// This vector.
            /// ( Geometry GeomFrom implementation )            
            /// </summary>
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0002.cs)
            /// </remarks>
            [JsonIgnore]
            public override Vector3D GeomFrom => this;

            /// <summary>
            /// This vector.
            /// ( Geometry GeomTo implementation)             
            /// </summary>
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0002.cs)
            /// </remarks>
            [JsonIgnore]
            public override Vector3D GeomTo => this;

            /// <summary>
            /// Enumerable with only this vector.
            /// ( Geometry Vertexes implementation )            
            /// </summary>
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0002.cs)
            /// </remarks>
            [JsonIgnore]
            public override IEnumerable<Vector3D> Vertexes
            {
                get
                {
                    yield return this;
                }
            }

            /// <summary>
            /// Length of this vector.
            /// ( Geometry Length implementation )            
            /// </summary>
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0002.cs)
            /// </remarks>
            public override double Length { get { return Sqrt(X * X + Y * Y + Z * Z); } }

            /// <summary>
            /// Create dxf point entity suitable for netDxf addEntity.
            /// ( Geometry DxfEntity implementation )            
            /// </summary>
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0002.cs)
            /// </remarks>
            public override netDxf.Entities.EntityObject DxfEntity
            {
                get
                {
                    return this.ToDxfPoint();
                }
            }

            /// <summary>
            /// Divide this point returning itself.
            /// ( Geometry Divide implementation )            
            /// </summary>
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0002.cs)
            /// </remarks>
            public override IEnumerable<Vector3D> Divide(int cnt, bool include_endpoints = false)
            {
                return new[] { this };
            }

            /// <summary>
            /// Compute bbox of this point.
            /// ( Geometry BBox implementation ).            
            /// </summary>
            /// <param name="tol_len">length tolerance</param>            
            /// <remarks>      
            /// [unit test](/test/Vector3D/Vector3DTest_0002.cs)
            /// </remarks>
            public override BBox3D BBox(double tol_len)
            {
                return new BBox3D(new[] { this });
            }

            #endregion

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

    public static partial class SciExt
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

        public static string CadScriptPolyline(this IEnumerable<Vector3D> points)
        {
            var sb = new StringBuilder();
            sb.Append("_POLYLINE ");
            foreach (var p in points)
            {
                sb.Append(Invariant($"{p.X},{p.Y},{p.Z}\r\n"));
            }
            sb.AppendLine();

            return sb.ToString();
        }

        public static string CadScriptPoint(this IEnumerable<Vector3D> points)
        {
            var sb = new StringBuilder();

            foreach (var p in points)
            {
                sb.AppendLine(Invariant($"_POINT {p.X},{p.Y},{p.Z}\r\n"));
            }
            sb.AppendLine();

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

        /// <summary>
        /// states if given 3 vectors are linearly independent        
        /// </summary>            
        /// <returns>true if given vector are linearly independent</returns>
        /// <remarks>
        /// [unit test](/test/Vector3D/Vector3DTest_0001.cs)
        /// </remarks>
        public static bool IsLinearIndependent(this IEnumerable<Vector3D> vectors)
        {
            var en = vectors.GetEnumerator();
            Action<bool> testMoveNext = (expectedResult) =>
            {
                if (en.MoveNext() != expectedResult)
                    throw new ArgumentException($"3 vectors required to test 3d linear independence");
            };

            testMoveNext(true);
            var v1 = en.Current;

            testMoveNext(true);
            var v2 = en.Current;

            testMoveNext(true);
            var v3 = en.Current;

            testMoveNext(false);

            var m = Matrix3D.FromVectorsAsColumns(v1, v2, v3);

            return !m.Determinant().EqualsAutoTol(0);
        }

    }

}
