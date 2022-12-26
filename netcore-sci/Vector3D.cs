using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using static System.Math;
using static System.FormattableString;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Numerics;
using NVector2 = System.Numerics.Vector2;
using NVector3 = System.Numerics.Vector3;
using DVector3 = netDxf.Vector3;
using GPoint3 = GShark.Geometry.Point3;

using Newtonsoft.Json;
using netDxf.Entities;
using netDxf;
using static SearchAThing.SciToolkit;

namespace SearchAThing
{

	/// <summary>
	/// can be used to describe a wcs point or a vector x,y,z components from some reference origin
	/// </summary>
	public partial class Vector3D : Geometry
	{

		#region Geometry

#if NETSTANDARD2_1_OR_GREATER
		public override Geometry Copy() => new Vector3D(this);
#elif NET6_0_OR_GREATER
        public override Vector3D Copy() => new Vector3D(this);
#endif

		/// <summary>
		/// Enumerable with only this vector.
		/// ( Geometry Vertexes implementation )            
		/// </summary>
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0002.cs)
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
		/// This vector.
		/// ( Geometry GeomFrom implementation )            
		/// </summary>
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0002.cs)
		/// </remarks>
		[JsonIgnore]
		public override Vector3D GeomFrom => this;

		/// <summary>
		/// This vector.
		/// ( Geometry GeomTo implementation)             
		/// </summary>
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0002.cs)
		/// </remarks>
		[JsonIgnore]
		public override Vector3D GeomTo => this;

		double? _Length = null;
		/// <summary>
		/// Length of this vector.
		/// ( Geometry Length implementation )            
		/// </summary>
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0002.cs)
		/// </remarks>    
		public override double Length
		{
			get
			{
				if (_Length is null)
					_Length = Sqrt(X * X + Y * Y + Z * Z);

				return _Length.Value;
			}
		}

		/// <summary>
		/// (other.X - this.X)^2 + (other.Y - this.Y)^2 + (other.Z - this.Z)^2
		/// </summary>
		public double SquaredDistance(Vector3D other) =>
			((other.X - this.X) * (other.X - this.X)) +
			((other.Y - this.Y) * (other.Y - this.Y)) +
			((other.Z - this.Z) * (other.Z - this.Z));

		public override Vector3D MidPoint => this;

		/// <summary>
		/// Divide this point returning itself.
		/// ( Geometry Divide implementation )            
		/// </summary>
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0002.cs)
		/// </remarks>
		public override IEnumerable<Vector3D> Divide(int cnt, bool include_endpoints = false) => new[] { this };

		public override IEnumerable<Geometry> Split(double tol, IEnumerable<Vector3D> breaks)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Compute bbox of this point.
		/// ( Geometry BBox implementation ).            
		/// </summary>
		/// <param name="tol">length tolerance</param>            
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0002.cs)
		/// </remarks>
		public override BBox3D BBox(double tol) => new BBox3D(new[] { this });

		public override IEnumerable<Geometry> GeomIntersect(double tol, Geometry other,
			GeomSegmentMode thisSegmentMode, GeomSegmentMode otherSegmentMode)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Create dxf point entity suitable for netDxf addEntity.
		/// ( Geometry DxfEntity implementation )            
		/// </summary>
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0002.cs)
		/// </remarks>        
		public override netDxf.Entities.EntityObject DxfEntity => this.ToDxfPoint();

		public override bool GeomEquals(double tol, Geometry other, bool checkSense = false)
		{
			if (other == this) return true;
			if (other.GeomType != GeometryType.Vector3D) return false;

			return this.EqualsTol(tol, (Vector3D)other);
		}

		#endregion

		/// <summary>
		/// zero vector (0,0,0)            
		/// </summary> 
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0001.cs)
		/// </remarks>           
		public static readonly Vector3D Zero = new Vector3D(0, 0, 0);

		/// <summary>
		/// xaxis vector (1,0,0)            
		/// </summary>   
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0004.cs)
		/// </remarks>
		public static readonly Vector3D XAxis = new Vector3D(1, 0, 0);

		/// <summary>
		/// yaxis vector (0,1,0)            
		/// </summary>            
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0004.cs)
		/// </remarks>
		public static readonly Vector3D YAxis = new Vector3D(0, 1, 0);

		/// <summary>
		/// zaxis vector (0,0,1)            
		/// </summary>            
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0004.cs)
		/// </remarks>
		public static readonly Vector3D ZAxis = new Vector3D(0, 0, 1);

		/// <summary>
		/// one vector (1,1,1)            
		/// </summary>         
		public static readonly Vector3D One = new Vector3D(1, 1, 1);

		/// <summary>
		/// retrieve wcs axis by given index            
		/// </summary>
		/// <param name="ord">0:(1,0,0) 1:(0,1,0) 2:(0,0,1)</param>                                    
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0004.cs)
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
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0003.cs)
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
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0003.cs)
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
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0005.cs)
		/// </remarks>
		public double X { get; private set; }

		/// <summary>
		/// Y vector component            
		/// </summary>            
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0005.cs)
		/// </remarks>
		public double Y { get; private set; }

		/// <summary>
		/// Z vector component            
		/// </summary>            
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0005.cs)
		/// </remarks>
		public double Z { get; private set; }

		/// <summary>
		/// zero vector            
		/// </summary>
		/// <remarks>  
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0006.cs)
		/// </remarks>
		[JsonConstructor]
		public Vector3D() : base(GeometryType.Vector3D)
		{
		}

		/// <summary>
		/// build a copy of given vector
		/// </summary>        
		public Vector3D(Vector3D v) : base(GeometryType.Vector3D)
		{
			X = v.X;
			Y = v.Y;
			Z = v.Z;
		}

		/// <summary>
		/// build a vector (x,y,0) or (x,y,z) from given 2 or 3 doubles            
		/// </summary>            
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0007.cs)
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
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0008.cs)
		/// </remarks>
		public Vector3D(double x, double y, double z) : base(GeometryType.Vector3D)
		{
			X = x; Y = y; Z = z;
		}

		/// <summary>
		/// build a vector (x,y,0) by given components            
		/// </summary>            
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0008.cs)
		/// </remarks>
		public Vector3D(double x, double y) : base(GeometryType.Vector3D)
		{
			X = x; Y = y;
		}

		static Regex? _cad_id_regex = null;
		/// <summary>
		/// static instance of regex to parse cad id string
		/// https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex?view=netcore-3.0#thread-safety
		/// </summary>            
		static Regex cad_id_regex
		{
			get
			{
				if (_cad_id_regex is null)
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
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0009.cs)
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
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0010.cs)
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

		public override Geometry Move(Vector3D delta) => this + delta;

		/// <summary>
		/// states if this is a zero vector            
		/// </summary>
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0011.cs)
		/// </remarks>
		public bool IsZeroLength => (X + Y + Z).EqualsTol(NormalizedLengthTolerance, 0);

		/// <summary>
		/// checks vector component equality vs other given                       
		/// </summary>
		/// <param name="tol">geometric tolerance ( note: use Constants.NormalizedLengthTolerance )</param>
		/// <param name="other">vector to compare to this</param>            
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0012.cs)
		/// </remarks>            
		public bool EqualsTol(double tol, Vector3D? other)
		{
			if (other is null) return false;

			return
				X.EqualsTol(tol, other.X) &&
				Y.EqualsTol(tol, other.Y) &&
				Z.EqualsTol(tol, other.Z);
		}

		/// <summary>
		/// check if this vector equals the given one component by component using EqualsAutoTol            
		/// </summary>
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0012.cs)
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
		/// <param name="x">x coord</param>            
		/// <param name="y">y coord</param>            
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0012.cs)
		/// </remarks>
		public bool EqualsTol(double tol, double x, double y) =>
			X.EqualsTol(tol, x) && Y.EqualsTol(tol, y);

		/// <summary>
		/// checks vector component equality vs other given                        
		/// </summary>
		/// <param name="tol">geometric tolerance ( note: use Constants.NormalizedLengthTolerance )</param>            
		/// <param name="x">x coord</param>            
		/// <param name="y">y coord</param>            
		/// <param name="z">z coord</param>            
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0012.cs)
		/// </remarks>
		public bool EqualsTol(double tol, double x, double y, double z) =>
			X.EqualsTol(tol, x) && Y.EqualsTol(tol, y) && Z.EqualsTol(tol, z);

		/// <summary>
		/// create a normalized version of this vector            
		/// </summary>
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0013.cs)
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
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0014.cs)
		/// </remarks>
		public double Distance(Vector3D other) => (this - other).Length;

		/// <summary>
		/// retrieve signed offset of this point respect given origin point in the given normalized direction v.
		/// precondition: vector v must colinear to (this-origin) and must already normalized
		/// </summary>        
		public double ColinearScalarOffset(double tol, Vector3D origin, Vector3D v)
		{
			var dst = (this - origin).Length;

			if ((origin + dst * v).EqualsTol(tol, this))
				return dst;
			else
				return -dst;
		}

		/// <summary>
		/// compute perpendicular(min) distance of this point from given line            
		/// </summary>
		/// <param name="tol">length tolerance ( used to check if point contained in line )</param>
		/// <param name="other">line</param>            
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0015.cs)
		/// ![image](../test/Vector3D/Vector3DTest_0015.png)
		/// </remarks>
		public double Distance(double tol, Line3D other)
		{
			var q = other.Perpendicular(tol, this);
			if (q is null) return 0;
			return q.Length;
		}

		/// <summary>
		/// compute distance of this point from the given in 2d ( x,y ) without consider z component            
		/// </summary>
		/// <param name="other">other point</param>            
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0016.cs)
		/// ![image](../test/Vector3D/Vector3DTest_0016.png)
		/// </remarks>
		public double XYDistance(Vector3D other) =>
			Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));

		/// <summary>
		/// compute dot product of this vector for the given one            
		/// a b = |a| |b| cos(alfa)            
		/// </summary>
		/// <param name="other">second vector</param>            
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0017.cs)            
		/// </remarks>
		public double DotProduct(Vector3D other) => X * other.X + Y * other.Y + Z * other.Z;

		/// <summary>
		/// states is this vector is perpendicular to the given one            
		/// </summary>
		/// <param name="other">other vector</param>
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0018.cs)
		/// </remarks>
		public bool IsPerpendicular(Vector3D other) =>
			Normalized().DotProduct(other.Normalized()).EqualsTol(NormalizedLengthTolerance, 0);

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
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0019.cs)
		/// ![image](../test/Vector3D/Vector3DTest_0019.png)
		/// </remarks>
		public Vector3D CrossProduct(Vector3D other) =>
			new Vector3D(Y * other.Z - Z * other.Y, -X * other.Z + Z * other.X, X * other.Y - Y * other.X);

		/// <summary>
		/// angle between this and given vector
		/// </summary>
		/// <param name="tol">length tolerance to test vector equalities ( use Constants.NormalizedLengthTolerance when comparing normalized vectors )</param>
		/// <param name="to">other vector</param>
		/// <returns>angle between two vectors (rad)</returns>
		/// <remarks>      
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0020.cs)
		/// ![image](../test/Vector3D/Vector3DTest_0020.png)            
		/// </remarks>
		public double AngleRad(double tol, Vector3D to)
		{
			if (this.EqualsTol(tol, to)) return 0;

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
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0021.cs)
		/// ![image](../test/Vector3D/Vector3DTest_0021.png)
		/// </remarks>
		public Vector3D Project(Vector3D to)
		{
			// https://en.wikipedia.org/wiki/Vector_projection
			// http://math.oregonstate.edu/bridge/papers/dot+cross.pdf (fig.1)

			if (to.Length == 0) throw new Exception($"project on null vector");

			return DotProduct(to) / to.Length * to.Normalized();
		}

		/// <summary>
		/// project this point to the given line considered as infinite line
		/// </summary>
		/// <param name="line">line to project the point onto</param>
		/// <returns>projected point onto the line ( perpendicularly )</returns>
		public Vector3D Project(Line3D line) => (this - line.From).Project(line.V) + line.From;

		/// <summary>
		/// wcs coord of projected coord to the given cs
		/// </summary>        
		/// <param name="cs">cs to project</param>
		/// <param name="evalCSOrigin">if true cs origin will subtracted before transform, then readded to obtain wcs point</param>                        
		public Vector3D Project(CoordinateSystem3D cs, bool evalCSOrigin = true) =>
			ToUCS(cs, evalCSOrigin).Set(OrdIdx.Z, 0).ToWCS(cs, evalCSOrigin);

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
		/// create a point copy of this one with component changed
		/// </summary>
		/// <param name="ordIdx">component to change ( 0:x 1:y 2:z )</param>
		/// <param name="value">value to assign to the component</param>
		/// <returns>new vector with component changed</returns>
		public Vector3D Set(int ordIdx, double value)
		{
			var x = X;
			var y = Y;
			var z = Z;

			switch (ordIdx)
			{
				case 0: x = value; break;
				case 1: y = value; break;
				case 2: z = value; break;
				default: throw new Exception($"invalid ordIdx:{ordIdx}");
			}

			return new Vector3D(x, y, z);
		}

		/// <summary>
		/// create new vector with X changed
		/// </summary>
		/// <param name="value">input vector</param>
		/// <returns>output vector with X changed</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3D SetX(double value) => new Vector3D(value, Y, Z);

		/// <summary>
		/// create new vector with Y changed
		/// </summary>
		/// <param name="value">input vector</param>
		/// <returns>output vector with Y changed</returns>        
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3D SetY(double value) => new Vector3D(X, value, Z);

		/// <summary>
		/// create new vector with Z changed
		/// </summary>
		/// <param name="value">input vector</param>
		/// <returns>output vector with Z changed</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3D SetZ(double value) => new Vector3D(X, Y, value);

		/// <summary>
		/// create a vector relative to given origin from this point and given origin
		/// </summary>
		/// <param name="origin">origin to make this point relative to</param>
		/// <returns>vector</returns>
		public Vector3D Rel(Vector3D origin) => this - origin;

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
		public bool Colinear(double tol, Vector3D other) =>
			new Line3D(Vector3D.Zero, this).Colinear(tol, new Line3D(Vector3D.Zero, other));

		/// <summary>
		/// states if this vector concord to the given one
		/// 
		/// **NOTE**: it does not test two vectors are parallels ( precondition must meet )
		/// </summary>
		/// <param name="tol">geometric tolerance ( Constants.NormalizedLengthTolerance if comparing normalized vectors )</param>
		/// <param name="other">other vector</param>            
		public bool Concordant(double tol, Vector3D other) => DotProduct(other) > tol;

		/// <summary>
		/// statis if this vector is concordant and colinear to the given one
		/// </summary>
		/// <param name="tol">geometric tolerance ( Constants.NormalizedLengthTolerance if comparing normalized vectors )</param>
		/// <param name="other">other vector</param>                        
		public bool ConcordantColinear(double tol, Vector3D other) => Concordant(tol, other) && Colinear(tol, other);

		/// <summary>
		/// compute angle required to make this point go to the given one
		/// if rotate right-hand around given reference axis
		/// </summary>
		/// <param name="tol">length tolerance ( use Constants.NormalizedLengthTolerance if working with normalized vectors )</param>
		/// <param name="to">point toward rotate this one</param>
		/// <param name="refAxis">reference axis to make right-hand rotation of this point toward given one</param>
		/// <returns>angle (rad)</returns>
		public double AngleToward(double tol, Vector3D to, Vector3D refAxis)
		{
			var c = this.CrossProduct(to);

			if (c.Concordant(tol, refAxis))
				return this.AngleRad(tol, to);
			else
				return 2 * PI - AngleRad(tol, to);
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
		public Vector3D Mirror(Line3D axis) => this + 2 * (Project(axis) - this);

		/// <summary>
		/// Convert this wcs point to given cs coord
		/// </summary>
		/// <param name="cs">dest CS</param>
		/// <param name="evalCSOrigin">if true CS origin will subtracted before transform</param>            
		public Vector3D ToUCS(CoordinateSystem3D cs, bool evalCSOrigin = true) => cs.ToUCS(this, evalCSOrigin);

		/// <summary>
		/// Convert this ucs considered vector using given cs to the wcs
		/// </summary>
		/// <param name="cs">ucs point</param>
		/// <param name="evalCSOrigin">if true CS origin will added after transform</param>
		public Vector3D ToWCS(CoordinateSystem3D cs, bool evalCSOrigin = true) => cs.ToWCS(this, evalCSOrigin);

		/// <summary>
		/// Scalar multiply each components
		/// </summary>                
		public Vector3D Scalar(double xs, double ys, double zs) => new Vector3D(X * xs, Y * ys, Z * zs);

		/// <summary>
		/// return clamped Vector3D between [min,max] interval
		/// </summary>        
		/// <param name="min">min value admissible</param>
		/// <param name="max">max value admissible</param>
		/// <returns>given vector with xyz components clamped to corresponding min,max components</returns>        
		public Vector3D Clamp(Vector3D min, Vector3D max)
		{
			var vx = X.Clamp(min.X, max.X);
			var vy = Y.Clamp(min.Y, max.Y);
			var vz = Z.Clamp(min.Z, max.Z);

			return new Vector3D(vx, vy, vz);
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
		public static Vector3D operator +(Vector3D a, Vector3D b) => new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

		/// <summary>
		/// negate
		/// </summary>        
		public static Vector3D operator -(Vector3D a) => -1d * a;

		/// <summary>
		/// sub
		/// </summary>        
		public static Vector3D operator -(Vector3D a, Vector3D b) => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

		/// <summary>
		/// scalar mul
		/// </summary>        
		public static Vector3D operator *(double s, Vector3D v) => new Vector3D(s * v.X, s * v.Y, s * v.Z);

		/// <summary>
		/// scalar mul
		/// </summary>        
		public static Vector3D operator *(Vector3D v, double s) => new Vector3D(s * v.X, s * v.Y, s * v.Z);

		/// <summary>
		/// scalar multiply vector components V1 * V2 =
		/// (V1.x * V2.x, V1.y * V2.y, V1.z * V2.z)
		/// </summary>        
		public static Vector3D operator *(Vector3D v1, Vector3D v2) => v1.Scalar(v2.X, v2.Y, v2.Z);

		/// <summary>
		/// scalar div
		/// </summary>        
		public static Vector3D operator /(double s, Vector3D v) => new Vector3D(s / v.X, s / v.Y, s / v.Z);

		/// <summary>
		/// scalar div
		/// </summary>        
		public static Vector3D operator /(Vector3D v, double s) => new Vector3D(v.X / s, v.Y / s, v.Z / s);

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

		/// <summary>
		/// retrieve list of Vector3D by reading from a txt file, for example:
		/// -53.54533794,-141.18745265
		/// 18.20103872,-149.89903999
		/// 85.77777676,-124.27056375
		/// 
		/// notes:
		/// - supports also third coord (Z) ;
		/// - whitespace are removed ;
		/// - empty lines are removed
		/// </summary>
		/// <param name="txt">txt data to read</param>
		/// <param name="culture">culture for number parsing (default: invariant)</param>
		/// <returns>enumerable of Vector3D corresponding to data</returns>
		public static IEnumerable<Vector3D> FromTxtPointsList(string txt, CultureInfo? culture = null)
		{
			if (culture is null) culture = CultureInfo.InvariantCulture;

			foreach (var l in txt.Lines().Select(l => l.Trim()).Where(l => l.Length > 0))
			{
				var ss = l.Split(',');
				var X = double.Parse(ss[0].Trim(), culture);
				var Y = double.Parse(ss[1].Trim(), culture);
				var Z = 0d;
				if (ss.Length > 2) Z = double.Parse(ss[2].Trim(), culture);

				yield return new Vector3D(X, Y, Z);
			}
		}

		public static IEnumerable<Vector3D> Random(int N, double L, int seed = 0) =>
			Random(N, -L / 2, L / 2, -L / 2, L / 2, -L / 2, L / 2, seed);

		/// <summary>
		/// Span a set of qty vector3d with random coord between given range.
		/// Optionally a seed can be specified for rand or Random obj directly ( in latter case seed aren't used )
		/// </summary>        
		public static IEnumerable<Vector3D> Random(int qty,
			double xmin, double xmax, double ymin, double ymax, double zmin, double zmax, int seed = 0, Random? random = null)
		{
			var dx = xmax - xmin;
			var dy = ymax - ymin;
			var dz = zmax - zmin;

			var rnd = (random is null) ? new Random(seed) : random;
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
		/// parse vector3d from array "(x1,y1,z1);(x2,y2,z2)";
		/// an appropriate string can be generated with StringRepresentation extension.
		/// </summary>                    
		public static IEnumerable<Vector3D> FromStringArray(string str) =>
			str.Split(";").Where(f => f.Trim().Length > 0).Select(f => FromString(f));

		/// <summary>
		/// string invariant representation "(x,y,z)"
		/// w/3 decimal places
		/// </summary>            
		public override string ToString() => this.ToString(digits: 3);

		/// <summary>
		/// string invariant representation "(x,y,z)" w/given digits
		/// </summary>            
		public string ToString(int digits = 3) => Invariant($"({X.ToString(digits)}, {Y.ToString(digits)}, {Z.ToString(digits)})");

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
		public string StringRepresentation() => Invariant($"({X}, {Y}, {Z})");

		public string QCadScript(bool final = true) => Invariant($"POINT\n{X},{Y}\n{(final ? "QQ\n" : "")}");

		public string A0QCadScript => QCadScript(final: true);

		/// <summary>
		/// cad script for this vector as wcs point
		/// </summary>
		public string CadScript => SciToolkit.PostProcessCadScript(
			string.Format(CultureInfo.InvariantCulture, "_POINT {0},{1},{2}\r\n", X, Y, Z));

		/// <summary>
		/// cad script for a line (0,0,0) to this vector
		/// </summary>
		public string CadScriptLine => new Line3D(Vector3D.Zero, this).CadScript;

		/// <summary>
		/// cad script for a line departing from this wcs point
		/// </summary>
		public string CadScriptLineFrom => SciToolkit.PostProcessCadScript(string.Format(CultureInfo.InvariantCulture, "_LINE {0},{1},{2}", X, Y, Z));

		/// <summary>
		/// build Line3D from this to given to
		/// </summary>
		/// <param name="to">line3d to point</param>
		/// <returns>build Line3D from this to given to</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Line3D LineTo(Vector3D to) => new Line3D(this, to);

		/// <summary>
		/// build Line3D from this to (this+vector)
		/// </summary>
		/// <param name="vector">vector to add this to obtain line to</param>
		/// <returns>Line3D from this to (this+given vector)</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Line3D LineV(Vector3D vector) => new Line3D(this, vector, Line3DConstructMode.PointAndVector);

		/// <summary>
		/// build Line3D from this to (this+dir*len)
		/// </summary>
		/// <param name="dir">direction</param>
		/// <param name="len">length of the line</param>
		/// <param name="applyDirNorm">apply normalization to given direction ( default:false )</param>
		/// <returns>Line3D from this to (this+dir*len)</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Line3D LineDir(Vector3D dir, double len, bool applyDirNorm = false) =>
			new Line3D(this, (applyDirNorm ? dir.Normalized() : dir) * len, Line3DConstructMode.PointAndVector);

		/// <summary>
		/// convert given (netdxf) Vector2 to a Vector3D ( assume z=0 )
		/// </summary>            
		public static implicit operator Vector3D(netDxf.Vector2 v) => new Vector3D(v.X, v.Y, 0);

		/// <summary>
		/// convert given (System.Numerics) Vector2 to a Vector3D ( with z=0 )
		/// </summary>            
		public static implicit operator Vector3D(NVector2 v) => new Vector3D(v.X, v.Y, 0);

		/// <summary>
		/// Convert given (netdxf) Vector3 to Vector3D
		/// </summary>            
		public static implicit operator Vector3D(netDxf.Vector3 v) => new Vector3D(v.X, v.Y, v.Z);

		/// <summary>
		/// Convert given Vector3D to (netdxf) Vector3
		/// </summary>            
		public static implicit operator netDxf.Vector3(Vector3D v) => new DVector3(v.X, v.Y, v.Z);

		/// <summary>
		/// Convert given Vector3D to System.Numerics.Vector3            
		/// </summary>
		/// <remarks>
		/// double to float conversion will be done
		/// </remarks>
		/// <param name="v">input vector</param>
		public static implicit operator NVector3(Vector3D v) =>
			new NVector3((float)v.X, (float)v.Y, (float)v.Z);

		/// <summary>
		/// Convert given Vector3D to GShark.Geometry.Point3
		/// </summary>
		/// <param name="v">input vector</param>
		public static implicit operator Vector3D(GPoint3 v)
		{
			var res = new Vector3D(v.X, v.Y, v.Z);
			return res;
		}

		/// <summary>
		/// Convert given System.Numerics.Vector3 to Vector3D
		/// </summary>
		/// <param name="v">input vector</param>
		public static implicit operator Vector3D(NVector3 v) => new Vector3D(v.X, v.Y, v.Z);

		/// <summary>
		/// Convert given LibTessDotNet.Vec3 to Vector3D
		/// </summary>
		/// <param name="v">input vector</param>
		public static implicit operator Vector3D(LibTessDotNet.Vec3 v) => new Vector3D(v.X, v.Y, v.Z);

		/// <summary>
		/// Convert given QuantumConcepts.Formats.StereoLithography.Vertex to Vector3D
		/// </summary>
		/// <param name="v">input vector</param>
		public static implicit operator Vector3D(QuantumConcepts.Formats.StereoLithography.Vertex v) =>
			new Vector3D(v.X, v.Y, v.Z);

		/// <summary>
		/// Convert given Vector3D to QuantumConcepts.Formats.StereoLithography.Vertex
		/// </summary>
		/// <remarks>
		/// double to float conversion will be done
		/// </remarks>
		/// <param name="v">input vector</param>
		public static implicit operator QuantumConcepts.Formats.StereoLithography.Vertex(Vector3D v) =>
			new QuantumConcepts.Formats.StereoLithography.Vertex((float)v.X, (float)v.Y, (float)v.Z);


		/// <summary>
		/// convert to (netdxf) discarding z
		/// </summary>
		public netDxf.Vector2 ToDxfVector2() => new netDxf.Vector2(X, Y);

		/// <summary>
		/// convert to (netdxf) discarding z
		/// </summary>
		public netDxf.Vector3 ToDxfVector3() => new netDxf.Vector3(X, Y, Z);

		/// <summary>
		/// convert to (system.numerics) Vector2 ( casting double to float, discarding z )
		/// </summary>
		public NVector2 ToNVector2() => new NVector2((float)X, (float)Y);

		/// <summary>
		/// convert to (system.numerics) Vector3 ( casting double to float )
		/// </summary>
		public NVector3 ToNVector3() => new NVector3((float)X, (float)Y, (float)Z);

		/// <summary>
		/// convert to GShark.Geometry.Point3
		/// </summary>
		public GPoint3 ToGPoint3() => new GPoint3(X, Y, Z);

		/// <summary>
		/// To point (double x, double y)
		/// </summary>        
		public Point ToPoint() => new Point(X, Y, 0);

		/// <summary>
		/// create dxf point from given vector3d
		/// </summary>        
		public netDxf.Entities.Point ToDxfPoint() => new netDxf.Entities.Point(new DVector3(X, Y, Z));

		/// <summary>
		/// convert xyz from deg to rad
		/// </summary>        
		/// <returns>xyz rad angles</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3D ToRad() => new Vector3D(X.ToRad(), Y.ToRad(), Z.ToRad());

		/// <summary>
		/// convert xyz from rad to deg
		/// </summary>        
		/// <returns>xyz deg angles</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3D ToDeg() => new Vector3D(X.ToDeg(), Y.ToDeg(), Z.ToDeg());

		/// <summary>
		/// debug to console with optional prefix
		/// </summary>        
		/// <param name="prefix">optional prefix</param>
		/// <returns>vector</returns>
		public Vector3D Debug(string prefix = "")
		{
			System.Diagnostics.Debug.WriteLine($"{(prefix.Length > 0 ? ($"{prefix}:") : "")}{this}");
			return this;
		}

		/// <summary>
		/// compute (Abs(v.x), Abs(v.y), Abs(v.z))
		/// </summary>        
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3D Abs() => new Vector3D(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));

		/// <summary>
		/// compute (Sign(v.x), Sign(v.y), Sign(v.z))
		/// </summary>                
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3D Sign() => new Vector3D(Math.Sign(X), Math.Sign(Y), Math.Sign(Z));

		/// <summary>
		/// returns p1 and p2 if one of the p1 coords are less than corresponding p2 coords ;
		/// elsewhere returns p2 and p1.
		/// Useful to obtain the same sequence order independant from order of operands.
		/// </summary>        
		public IEnumerable<Vector3D> DisambiguatedPoints(double tol, Vector3D other)
		{
			if (X.LessThanTol(tol, other.X) || Y.LessThanTol(tol, other.Y) || Z.LessThanTol(tol, other.Z))
			{
				yield return this;
				yield return other;
			}
			else
			{
				yield return other;
				yield return this;
			}
		}

		/// <summary>
		/// max between X,Y,Z
		/// </summary>           
		public double Max => Math.Max(Math.Max(X, Y), Z);

		/// <summary>
		/// min between X,Y,Z
		/// </summary>                
		public double Min => Math.Min(Math.Min(X, Y), Z);

		/// <summary>
		/// return this vector transformed by given (float) transformation
		/// </summary>
		public Vector3D Transform(Matrix4x4 transform) =>
			NVector3.Transform(this, transform);


	}

	/// <summary>
	/// helper class to compare vector3d set using given tolerance
	/// </summary>
	public class Vector3DEqualityComparer : IEqualityComparer<Vector3D>
	{
		double tol;

		public Vector3DEqualityComparer(double _tol)
		{
			tol = _tol;
		}

		public bool Equals(Vector3D? x, Vector3D? y) => x != null && y != null && x.EqualsTol(tol, y);

		public int GetHashCode(Vector3D obj) => 0;
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

	/// <summary>
	/// helper class to compare vector3d in a distinct operation retaining original order, used by DistinctKeepOrder()
	/// </summary>
	public class Vector3DWithOrderEqualityComparer : IEqualityComparer<Vector3DWithOrder>
	{
		Vector3DEqualityComparer cmp;

		public Vector3DWithOrderEqualityComparer(Vector3DEqualityComparer _cmp)
		{
			cmp = _cmp;
		}

		public bool Equals(Vector3DWithOrder? x, Vector3DWithOrder? y) =>
			x != null && y != null && cmp.Equals(x.Vector, y.Vector);

		public int GetHashCode(Vector3DWithOrder obj) => cmp.GetHashCode(obj.Vector);
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
		/// retrieve reversed version of given point set ( used to convert ccw, cw )
		/// </summary>        
		public static IList<Vector3D> Reversed(this IEnumerable<Vector3D> pts)
		{
			var q = pts.ToList();
			q.Reverse();
			return q;
		}

		/// <summary>
		/// retrieve reversed version of given point set ( used to convert ccw, cw )
		/// </summary>        
		public static IEnumerable<Vector3D> Reversed(this IList<Vector3D> pts)
		{
			for (int i = pts.Count - 1; i > -1; --i)
			{
				yield return pts[i];
			}
		}

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
		/// array invariant string vector3d representation "(x1,y1,z2);(x2,y2,z2)";
		/// an array of Vector3D can be rebuilt from string using Vector3D.FromStringArray
		/// </summary>        
		public static string StringRepresentation(this IEnumerable<Vector3D> pts) =>
			string.Join(";", pts.Select(g => g.StringRepresentation()));

		/// <summary>
		/// compute length of polyline from given seq_pts
		/// </summary>        
		public static double Length(this IEnumerable<Vector3D> seq_pts)
		{
			var l = 0.0;

			Vector3D? prev = null;
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

		/// <summary>
		/// create a 3dpolyline cadscript from given set of points
		/// </summary>
		/// <param name="points">point set</param>
		/// <returns>cad script</returns>
		public static string CadScriptPolyline(this IEnumerable<Vector3D> points)
		{
			var sb = new StringBuilder();
			sb.Append("_3DPOLY ");
			foreach (var p in points)
			{
				sb.AppendLine(Invariant($"{p.X},{p.Y},{p.Z}"));
			}
			sb.AppendLine();

			return SciToolkit.PostProcessCadScript(sb.ToString());
		}

		/// <summary>
		/// create script that draw a point foreach of point set
		/// </summary>
		/// <param name="points">points</param>
		/// <returns>cadscript</returns>
		public static string CadScriptPoint(this IEnumerable<Vector3D> points)
		{
			var sb = new StringBuilder();

			foreach (var p in points)
			{
				sb.AppendLine(Invariant($"_POINT {p.X},{p.Y},{p.Z}\r\n"));
			}
			sb.AppendLine();

			return SciToolkit.PostProcessCadScript(sb.ToString());
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

		/// <summary>
		/// retrieve the sum of given vector list
		/// </summary>
		/// <param name="lst">vector list</param>
		/// <returns>sum of given vector list</returns>
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
		public static Vector3D Center(this IEnumerable<Vector3D> lst) => lst.Mean();

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

		/// <summary>
		/// return pts (maintaining order) w/out duplicates
		/// use the other overloaded method if already have a vector 3d equality comparer
		/// </summary>        
		public static IEnumerable<Vector3D> ZapDuplicates(this IEnumerable<Vector3D> pts, double tol) =>
			pts.ZapDuplicates(new Vector3DEqualityComparer(tol));

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
		/// build polygons from given list of 2d segments by intersecting segments.
		/// (does not consider z)
		/// </summary>        
		public static IEnumerable<IReadOnlyList<Vector3D>> XYClosedPolys(this IEnumerable<Line3D> segs, double tol,
			int polyMaxPoints = 0)
		{
			var minCoord = new BBox3D(segs.SelectMany(r => new[] { r.From, r.To })).Min;

			var vcmp = new Vector3DEqualityComparer(tol);
			var lcmp = new Line3DEqualityComparer(tol);
			var segsDict = segs.ToDictionary(k => k.ToStringTol(tol), v => v);
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
					List<Line3D>? segsNext = null;
					{
						var hs = new HashSet<Line3D>(lcmp);
						{
							if (segsFromDict.TryGetValue(seg.To, out var tmp)) foreach (var x in tmp.Where(r => !r.EqualsTol(tol, seg))) hs.Add(x);
						}
						{
							if (segsToDict.TryGetValue(seg.To, out var tmp)) foreach (var x in tmp.Where(r => !r.EqualsTol(tol, seg))) hs.Add(x);
						}
						segsNext = hs.Select(w => w.EnsureFrom(tol, seg.To)).ToList();
					}

					Line3D? segNext = null;
					var force_close_poly = false;

					if (polyMaxPoints > 0 && poly.Count > polyMaxPoints)
						throw new Exception($"polygon [{poly.PolygonSegments(tol).ToCadScript()}] max point exceeded");

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
							.OrderBy(w => (-seg.V).AngleRad(tol, w.V))
							.First();
						rotDir = seg.V.CrossProduct(segNext.V).Z > 0 ? 1 : -1;
					}
					else
					{
						var qSegsNext = segsNext
							.Select(w => new
							{
								arad = (seg.V).AngleToward(tol, w.V, Vector3D.ZAxis * rotDir),
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

					segsLeft.Remove(segNext!);
					if (segNext!.To.EqualsTol(tol, poly[0])) break;
					poly.Add(segNext.To);

					seg = segNext!;
				}

				if (poly.Count > 2)
				{
					var polyMean = poly.Mean();
					poly = poly.SortCCW(tol, CoordinateSystem3D.WCS.Move(polyMean)).ToList();

					var polyCentroid = poly.XYCentroid(tol);
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
		/// states if given 3 vectors are linearly independent        
		/// </summary>            
		/// <returns>true if given vector are linearly independent</returns>
		/// <remarks>
		/// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Vector3D/Vector3DTest_0001.cs)
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

		/// <summary>
		/// detect best fitting plane for given set of coplanar points
		/// </summary>        
		public static Plane3D BestFittingPlane(this IEnumerable<Vector3D> pts, double tol)
		{
			Vector3D? firstPt = null;
			double distantPtDst = 0;
			Vector3D? distantPt = null;
			foreach (var ptItem in pts.WithIndex())
			{
				var pt = ptItem.item;
				if (ptItem.idx == 0)
					firstPt = pt;
				else
				{
					var dst = pt.Distance(firstPt!);
					if (distantPt is null || dst > distantPtDst)
					{
						distantPt = pt;
						distantPtDst = dst;
					}
				}
			}

			if (firstPt is null || distantPt is null)
				throw new Exception($"can't find two distant points");

			var v1 = distantPt - firstPt;
			double? ang = null;
			Vector3D? v2 = null;

			var ptHs = new HashSet<Vector3D>(new Vector3DEqualityComparer(tol));

			foreach (var pt in pts)
			{
				if (ptHs.Contains(pt)) continue;
				ptHs.Add(pt);

				if (pt == firstPt || pt == distantPt) continue;

				var qv2 = pt - firstPt;

				var qang = qv2.AngleRad(tol, v1);
				if (ang is null || qang > ang.Value)
				{
					ang = qang;
					v2 = qv2;
				}
			}

			if (v2 is null) throw new Exception($"can't find v2");

			var pl = new Plane3D(new CoordinateSystem3D(firstPt, v1, v2).Simplified());

			return pl;
		}

		/// <summary>
		/// compute (Sqrt(v.x), Sqrt(v.y), Sqrt(v.z))
		/// </summary>
		/// <param name="v">input vector</param>
		/// <returns>sqrt(v)</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3D Sqrt(this Vector3D v) => new Vector3D(Math.Sqrt(v.X), Math.Sqrt(v.Y), Math.Sqrt(v.Z));

		/// <summary>
		/// convert given STL Vertex to Vector3 (System.Numerics)
		/// </summary>        
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NVector3 ToNVector3(this QuantumConcepts.Formats.StereoLithography.Vertex v) =>
			new NVector3(v.X, v.Y, v.Z);

	}

}
