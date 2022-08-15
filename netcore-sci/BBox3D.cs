using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

using netDxf.Entities;
using netDxf;
using netDxf.Tables;

using static SearchAThing.SciToolkit;

namespace SearchAThing
{

    /// <summary>
    /// Bounding Box.
    /// Initially empty, each added point will increase the extension if min or max exceed actual bbox limits.
    /// It works within wcs.
    /// </summary>
    public class BBox3D
    {

        /// <summary>
        /// states if bbox empty
        /// </summary>            
        public bool IsEmpty => _Min == null;

        Vector3D? _Min = null;
        /// <summary>
        /// Min coord of bbox resulting by all inserted points
        /// </summary>            
        public Vector3D Min
        {
            get
            {
                if (_Min == null) throw new Exception($"empty bbox");
                return _Min;
            }
            private set
            {
                _Min = value;
            }
        }

        Vector3D? _Max = null;
        /// <summary>
        /// Man coord of bbox resulting by all inserted points
        /// </summary>            
        public Vector3D Max
        {
            get
            {
                if (_Max == null) throw new Exception($"empty bbox");
                return _Max;
            }
            private set
            {
                _Max = value;
            }
        }


        /// <summary>
        /// middle point of bbox = (Min+Max)/2 ( Zero if empty )
        /// </summary>
        /// <returns>middle point of bbox</returns>
        public Vector3D Middle => (Min + Max) / 2;

        /// <summary>
        /// Size of bbox as Max-Min point distance ( Zero if empty)
        /// </summary>            
        public Vector3D Size => Max - Min;

        /// <summary>
        /// build a 4 point bbox coords for 2D using Z=Min.Z
        /// </summary>
        public IEnumerable<Vector3D> Coords2D
        {
            get
            {
                yield return new Vector3D(Min.X, Min.Y, Min.Z);
                yield return new Vector3D(Max.X, Min.Y, Min.Z);
                yield return new Vector3D(Max.X, Max.Y, Min.Z);
                yield return new Vector3D(Min.X, Max.Y, Min.Z);
            }
        }

        /// <summary>
        /// build 8 coords 3d of current bbox
        /// </summary>
        public IEnumerable<Vector3D> Coords3D
        {
            get
            {
                var min = Min;
                var max = Max;
                var dx = new Vector3D(Size.X, 0, 0);
                var dy = new Vector3D(0, Size.Y, 0);
                var dz = new Vector3D(0, 0, Size.Z);

                yield return min;
                yield return min + dx;
                yield return min + dx + dy;
                yield return min + dy;

                yield return min + dz;
                yield return min + dx + dz;
                yield return min + dx + dy + dz;
                yield return min + dy + dz;
            }
        }

        /// <summary>
        /// construct empy bbox
        /// </summary>
        public BBox3D()
        {
        }

        /// <summary>
        /// construct bbox that contains pt at center and with given radius
        /// </summary>
        public BBox3D(Vector3D pt, double radius) :
            this(new[]
            {
                pt - Vector3D.XAxis * radius,
                pt + Vector3D.XAxis * radius,

                pt - Vector3D.YAxis * radius,
                pt + Vector3D.YAxis * radius,

                pt - Vector3D.ZAxis * radius,
                pt + Vector3D.ZAxis * radius,
            })
        {
        }

        /// <summary>
        /// scale all bbox points Min,Max respect to the center
        /// </summary>
        /// <param name="factor">scale factor</param>
        /// <returns>new bbox scaled</returns>
        public BBox3D Scale(double factor)
        {
            var center = (Min + Max) / 2;

            return new BBox3D(new Vector3D[]
            {
                Min.ScaleAbout(center, factor),
                Max.ScaleAbout(center, factor)
            });
        }

        /// <summary>
        /// scale all bbox points Min,Max respect to the center
        /// </summary>
        /// <param name="factor">scale factor</param>        
        /// <returns>new bbox scaled</returns>
        public BBox3D Scale(Vector3D factor)
        {
            var center = (Min + Max) / 2;

            return new BBox3D(new Vector3D[]
            {
                Min.ScaleAbout(center, factor),
                Max.ScaleAbout(center, factor)
            });
        }

        /// <summary>
        /// construct bbox with given points
        /// </summary>
        /// <param name="pts">points to add to the bbox</param>
        public BBox3D(IEnumerable<Vector3D> pts)
        {
            ApplyUnion(pts);
        }

        /// <summary>
        /// add given points to current bbox with side effects
        /// </summary>
        /// <param name="pts">points to add</param>        
        public void ApplyUnion(params Vector3D[] pts)
        {
            ApplyUnion((IEnumerable<Vector3D>)pts);
        }

        /// <summary>
        /// add given points to current bbox with side effects
        /// </summary>
        /// <param name="pts">points to add</param>
        public void ApplyUnion(IEnumerable<Vector3D> pts)
        {
            double xmin, ymin, zmin, xmax, ymax, zmax;

            bool assignFromFirstPt;

            if (IsEmpty)
            {
                assignFromFirstPt = true;
                xmin = ymin = zmin = xmax = ymax = zmax = 0;
            }
            else
            {
                assignFromFirstPt = false;
                xmin = Min.X; ymin = Min.Y; zmin = Min.Z;
                xmax = Max.X; ymax = Max.Y; zmax = Max.Z;
            }

            foreach (var p in pts)
            {
                if (assignFromFirstPt)
                {
                    xmin = xmax = p.X;
                    ymin = ymax = p.Y;
                    zmin = zmax = p.Z;
                    assignFromFirstPt = false;
                }
                else
                {
                    xmin = Min(xmin, p.X);
                    ymin = Min(ymin, p.Y);
                    zmin = Min(zmin, p.Z);

                    xmax = Max(xmax, p.X);
                    ymax = Max(ymax, p.Y);
                    zmax = Max(zmax, p.Z);
                }
            }
            Min = new Vector3D(xmin, ymin, zmin);
            Max = new Vector3D(xmax, ymax, zmax);
        }

        /// <summary>
        /// construct a copy of bbox
        /// </summary>
        /// <param name="other">source bbox</param>
        public BBox3D(BBox3D other)
        {
            if (!other.IsEmpty)
            {
                Min = other.Min;
                Max = other.Max;
            }
        }

        /// <summary>
        /// union of this bbox with given point
        /// </summary>
        /// <param name="p">point to add to this bbox copy</param>
        /// <returns>new bbox with given point added to</returns>
        public BBox3D Union(Vector3D p)
        {
            var res = new BBox3D(this);

            res.ApplyUnion(p);

            return res;
        }

        /// <summary>
        /// union of this bbox with another
        /// </summary>
        /// <param name="other">other bbox to add to this</param>
        /// <returns>new bbox resulting from the union of this with given other one</returns>
        public BBox3D Union(BBox3D other)
        {
            if (IsEmpty) return other;
            if (other.IsEmpty) return this;

            return this.Union(other.Min).Union(other.Max);
        }

        /// <summary>
        /// states if this bbox equals to the other one
        /// </summary>
        /// <param name="tol">tolerance against Min, Max comparision</param>
        /// <param name="other">other bbox to compare for equality</param>
        /// <returns>true if given bbox equals this one</returns>
        public bool EqualsTol(double tol, BBox3D other)
        {
            if (IsEmpty) return other.IsEmpty;
            if (other.IsEmpty) return false;
            return Min.EqualsTol(tol, other.Min) && Max.EqualsTol(tol, other.Max);
        }

        /// <summary>
        /// states if given other bbox contained in this
        /// </summary>
        /// <param name="tol">tolerance against Min, Max comparision</param>
        /// <param name="other">other bbox to check if contained in this</param>
        /// <param name="strictly">if true it checks this bbox is stricly contained into given other</param>
        /// <param name="testZ">if false min,max test of Z isn't evaluated (useful for planar entities bboxes tests)</param>
        /// <returns>true if given other bbox contained in this one</returns>
        public bool Contains(double tol, BBox3D other, bool strictly = false, bool testZ = true)
        {
            if (IsEmpty) return false;

            if (other.IsEmpty) return true;

            return
                strictly
                ?
                other.Min.X.GreatThanTol(tol, Min.X) &&
                other.Min.Y.GreatThanTol(tol, Min.Y) &&
                (testZ ? other.Min.Z.GreatThanTol(tol, Min.Z) : true) &&
                other.Max.X.LessThanTol(tol, Max.X) &&
                other.Max.Y.LessThanTol(tol, Max.Y) &&
                (testZ ? other.Max.Z.LessThanTol(tol, Max.Z) : true)
                :
                other.Min.X.GreatThanOrEqualsTol(tol, Min.X) &&
                other.Min.Y.GreatThanOrEqualsTol(tol, Min.Y) &&
                (testZ ? other.Min.Z.GreatThanOrEqualsTol(tol, Min.Z) : true) &&
                other.Max.X.LessThanOrEqualsTol(tol, Max.X) &&
                other.Max.Y.LessThanOrEqualsTol(tol, Max.Y) &&
                (testZ ? other.Max.Z.LessThanOrEqualsTol(tol, Max.Z) : true);
        }

        /// <summary>
        /// states if given point contained in this bbox
        /// </summary>
        /// <param name="tol">tolerance against Min, Max comparision</param>
        /// <param name="p">point to check if contained in this bbox</param>
        /// <returns>true if given point contained in this bbox</returns>
        public bool Contains(double tol, Vector3D p) =>
            !IsEmpty &&
            p.X.GreatThanOrEqualsTol(tol, Min.X) &&
            p.Y.GreatThanOrEqualsTol(tol, Min.Y) &&
            p.Z.GreatThanOrEqualsTol(tol, Min.Z) &&
            p.X.LessThanOrEqualsTol(tol, Max.X) &&
            p.Y.LessThanOrEqualsTol(tol, Max.Y) &&
            p.Z.LessThanOrEqualsTol(tol, Max.Z);

        /// <summary>
        /// states if given point is contained in this bbox excluding Z evaluation
        /// </summary>
        /// <param name="tol">tolerance against Min, Max comparision</param>
        /// <param name="p">point to check if contained in this bbox (Z ignored)</param>
        /// <returns>true if given point (Z ignored) contained in this bbox</returns>
        public bool Contains2D(double tol, Vector3D p) =>
            !IsEmpty &&
            p.X.GreatThanOrEqualsTol(tol, Min.X) &&
            p.Y.GreatThanOrEqualsTol(tol, Min.Y) &&
            p.X.LessThanOrEqualsTol(tol, Max.X) &&
            p.Y.LessThanOrEqualsTol(tol, Max.Y);

        /// <summary>
        /// create new bbox extending by subtract margin to Min and by add to Max
        /// </summary>            
        public BBox3D AddMargin(Vector3D margin) =>
            new BBox3D(new Vector3D[]
            {
                Min - margin,
                Max + margin
            });


        public IEnumerable<Face3D> ToFace3DList()
        {
            var d = Max - Min;
            return Cuboid((Max + Min) / 2, d);
        }

        /// <summary>
        /// script to paste in cad to draw bbox
        /// </summary>
        public string CadScript
        {
            get
            {
                var sb = new StringBuilder();

                foreach (var x in ToFace3DList())
                {
                    sb.AppendLine(x.CadScript());
                }

                return PostProcessCadScript(sb.ToString());
            }
        }

        /// <summary>
        /// retrieve ordered set of points for this bbox (0..4) first face (5..7) other face
        /// - [0] = (Min.X, Min.Y, Min.Z)
        /// - [1] = (Max.X, Min.Y, Min.Z)
        /// - [2] = (Max.X, Max.Y, Min.Z)
        /// - [3] = (Min.X, Max.Y, Min.Z)
        /// - [4] = (Min.X, Min.Y, Max.Z)
        /// - [5] = (Max.X, Min.Y, Max.Z)
        /// - [6] = (Max.X, Max.Y, Max.Z)
        /// - [7] = (Min.X, Max.Y, Max.Z)
        /// </summary>
        public IReadOnlyList<Vector3D> Points => new List<Vector3D>()
        {
            new Vector3D(Min.X, Min.Y, Min.Z),
            new Vector3D(Max.X, Min.Y, Min.Z),
            new Vector3D(Max.X, Max.Y, Min.Z),
            new Vector3D(Min.X, Max.Y, Min.Z),

            new Vector3D(Min.X, Min.Y, Max.Z),
            new Vector3D(Max.X, Min.Y, Max.Z),
            new Vector3D(Max.X, Max.Y, Max.Z),
            new Vector3D(Min.X, Max.Y, Max.Z)
        };

        /// <summary>
        /// retrieve 6 faces (bottom, top, left, right, front, back) of bbox.
        /// (see Points property documentation for vertex enumeration), faces are:
        /// - [0]: 0321 (bottom)
        /// - [1]: 4567 (top)
        /// - [2]: 0473 (left)
        /// - [3]: 1265 (right)
        /// - [4]: 0154 (front)
        /// - [5]: 2376 (back)
        /// </summary>        
        public IEnumerable<Plane3DRegion> Faces(double tol)
        {
            //       3---------2
            //     / |       / |
            //    /  |      /  |
            //   7---------6   |
            //   |  0------|---1
            //   | /       |  /
            //   4---------5/
            // 
            var pts = Points;
            var p0 = pts[0];
            var p1 = pts[1];
            var p2 = pts[2];
            var p3 = pts[3];
            var p4 = pts[4];
            var p5 = pts[5];
            var p6 = pts[6];
            var p7 = pts[7];

            yield return new Plane3DRegion(tol, new[] { p0, p3, p2, p1 });
            yield return new Plane3DRegion(tol, new[] { p4, p5, p6, p7 });
            yield return new Plane3DRegion(tol, new[] { p0, p4, p7, p3 });
            yield return new Plane3DRegion(tol, new[] { p1, p2, p6, p5 });
            yield return new Plane3DRegion(tol, new[] { p0, p1, p5, p4 });
            yield return new Plane3DRegion(tol, new[] { p2, p3, p7, p6 });
        }

        /// <summary>
        /// find intersection points of given ray to this bbox faces
        /// </summary>
        /// <param name="tol">length tolerance</param>
        /// <param name="ray">ray to test if intersect one or more of this bbox faces</param>
        /// <returns>intersection points</returns>
        public IEnumerable<Vector3D> Intersect(double tol, Line3D ray)
        {
            var faces = Faces(tol);

            foreach (var face in faces)
            {
                var ip = ray.Intersect(tol, face.Plane);
                if (ip != null && face.Contains(tol, ip)) yield return ip;
            }
        }

        public IEnumerable<Face3D> DrawCuboid(DxfObject dxfObj, Layer? layer = null)
        {
            var ents = ToFace3DList().ToList();

            dxfObj.AddEntities(ents, layer);

            return ents;
        }

        /// <summary>
        /// stringify bbox as Max-Min=Size
        /// </summary>
        public override string ToString() => IsEmpty ? "Empty" : $"{Max}-{Min}={Size}";

    }

    public static partial class SciExt
    {

        /// <summary>
        /// construct a bbox from given enumerable set of points
        /// </summary>
        /// <param name="pts">points to build bbox</param>
        /// <returns>bbox from given enumerable set of points</returns>
        public static BBox3D BBox(this IEnumerable<Vector3D> pts) => new BBox3D(pts);

        /// <summary>
        /// construct a bbox from given dxf EntityObject        
        /// </summary>
        /// <remarks>
        /// Currently Text, MText width not evaluated ( only insertion point is considered ).
        /// Arc is considered only From, MidPoint, To.
        /// Circle is considered only 4 points on circumference.
        /// </remarks>
        /// <param name="eo">dxf entity object</param>
        /// <param name="tol">tolerance for comparision length tests</param>
        /// <returns>new bbox that contains given dxf entity object</returns>
        public static BBox3D BBox(this EntityObject eo, double tol)
        {
            switch (eo.Type)
            {
                // TODO consider text width
                case EntityType.Text:
                // TODO consider text width
                case EntityType.MText:
                case EntityType.Line:
                case EntityType.Point:
                case EntityType.Insert:
                    return eo.Points().BBox();

                case EntityType.Arc:
                    {
                        var arc = ((Arc)eo).ToArc3D();
                        return new BBox3D(new[] { arc.From, arc.To, arc.MidPoint });
                    }

                case EntityType.Circle: return ((Circle)eo).ToPolyline2D(4).BBox(tol);

                case EntityType.Polyline2D:
                    {
                        var lwpoly = (Polyline2D)eo;

                        var N = lwpoly.Normal;
                        var ocs = new CoordinateSystem3D(N * lwpoly.Elevation, N);

                        return new BBox3D(eo.Points().Select(k => k.ToWCS(ocs)));
                    }


                case EntityType.Hatch: return new BBox3D();

                default:
                    throw new NotImplementedException($"bbox not implemented for dxf entity type [{eo.Type.ToString()}]");
            }
        }

        /// <summary>
        /// construct a bbox from given set of dxf entity objects
        /// </summary>
        /// <param name="ents">enumerable of dxf entity object</param>
        /// <param name="tol">tolerance for comparision length tests</param>
        /// <returns>new bbox containing given set of dxf entity objects</returns>        
        public static BBox3D BBox(this IEnumerable<EntityObject> ents, double tol)
        {
            var bbox = new BBox3D();

            foreach (var x in ents)
            {
                bbox = bbox.Union(x.BBox(tol));
            }

            return bbox;
        }

        public static IEnumerable<Vector3D> Points(this EntityObject eo)
        {
            switch (eo.Type)
            {
                case EntityType.Line:
                    {
                        var line = (Line)eo;
                        yield return line.StartPoint;
                        yield return line.EndPoint;
                    }
                    break;

                case EntityType.Polyline2D:
                    {
                        var lw = (Polyline2D)eo;
                        foreach (var x in lw.Vertexes) yield return x.Position;
                    }
                    break;

                case EntityType.Text:
                    {
                        var txt = (Text)eo;
                        yield return txt.Position;
                    }
                    break;

                case EntityType.MText:
                    {
                        var mtxt = (MText)eo;
                        yield return mtxt.Position;
                    }
                    break;

                case EntityType.Face3D:
                    {
                        var f = (Face3D)eo;
                        yield return f.FirstVertex;
                        yield return f.SecondVertex;
                        yield return f.ThirdVertex;
                        if (((Vector3?)f.FourthVertex).HasValue)
                            yield return f.FourthVertex;
                    }
                    break;

                case EntityType.Point:
                    {
                        var pt = (Point)eo;
                        yield return pt.Position;
                    }
                    break;

                case EntityType.Insert:
                    {
                        var ins = (Insert)eo;
                        var insPt = ins.Position;
                        var pts = ins.Block.Entities.SelectMany(w => w.Points());

                        pts = pts.Select(w => w.ScaleAbout(Vector3D.Zero, ins.Scale));

                        var N = ins.Normal;
                        var ocs = new CoordinateSystem3D(insPt, N).Rotate(N, ins.Rotation.ToRad());

                        pts = pts.Select(w => w.ToWCS(ocs));

                        foreach (var x in pts) yield return x;
                    };
                    break;

                case EntityType.Hatch:
                    {
                    }
                    break;

                case EntityType.Circle:
                    {
                        var circleLw = ((Circle)eo).ToPolyline2D(4);
                        foreach (var x in circleLw.Vertexes) yield return x.Position;
                    }
                    break;

                case EntityType.Arc:
                    {
                        var arc = ((Arc)eo).ToArc3D();
                        yield return arc.From;
                        yield return arc.MidPoint;
                        yield return arc.To;
                    }
                    break;

                default:
                    throw new NotImplementedException($"bbox not implemented for dxf entity type [{eo.Type.ToString()}]");
            }

        }

        /// <summary>
        /// union of bboxes
        /// </summary>        
        public static BBox3D Union(this IEnumerable<BBox3D> bboxes)
        {
            var res = new BBox3D();

            foreach (var x in bboxes) res = res.Union(x);

            return res;
        }

    }

}