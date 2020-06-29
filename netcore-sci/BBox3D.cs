using static System.Math;
using System.Collections.Generic;
using SearchAThing.Sci;
using System.Linq;
using System.Text;
using System;
using netDxf.Entities;
using netDxf;
using netDxf.Tables;
using SearchAThing.Util;

namespace SearchAThing
{

    namespace Sci
    {

        public class BBox3D
        {

            /// <summary>
            /// states if bbox empty ( Min == Max == null )
            /// </summary>            
            public bool IsEmpty { get { return Min == null; } }

            /// <summary>
            /// Min coord of bbox resulting by all inserted points
            /// </summary>            
            public Vector3D Min { get; private set; }

            /// <summary>
            /// Max coord of bbox resulting by all inserted points
            /// </summary>            
            public Vector3D Max { get; private set; }

            /// <summary>
            /// middle point of bbox = (Min+Max)/2
            /// </summary>
            /// <returns>middle point of bbox</returns>
            public Vector3D Middle => (Min + Max) / 2;

            /// <summary>
            /// Size of bbox as Max-Min point distance
            /// </summary>            
            public Vector3D Size { get { return Max - Min; } }

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
                Min = other.Min;
                Max = other.Max;
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
            /// <returns>true if given other bbox contained in this one</returns>
            public bool Contains(double tol, BBox3D other)
            {
                if (IsEmpty) return false;
                if (other.IsEmpty) return true;
                return
                    other.Min.X.GreatThanOrEqualsTol(tol, Min.X) &&
                    other.Min.Y.GreatThanOrEqualsTol(tol, Min.Y) &&
                    other.Min.Z.GreatThanOrEqualsTol(tol, Min.Z) &&
                    other.Max.X.LessThanOrEqualsTol(tol, Max.X) &&
                    other.Max.Y.LessThanOrEqualsTol(tol, Max.Y) &&
                    other.Max.Z.LessThanOrEqualsTol(tol, Max.Z);
            }

            /// <summary>
            /// states if given point contained in this bbox
            /// </summary>
            /// <param name="tol">tolerance against Min, Max comparision</param>
            /// <param name="p">point to check if contained in this bbox</param>
            /// <returns>true if given point contained in this bbox</returns>
            public bool Contains(double tol, Vector3D p)
            {
                if (IsEmpty) return false;

                return
                    p.X.GreatThanOrEqualsTol(tol, Min.X) &&
                    p.Y.GreatThanOrEqualsTol(tol, Min.Y) &&
                    p.Z.GreatThanOrEqualsTol(tol, Min.Z) &&
                    p.X.LessThanOrEqualsTol(tol, Max.X) &&
                    p.Y.LessThanOrEqualsTol(tol, Max.Y) &&
                    p.Z.LessThanOrEqualsTol(tol, Max.Z);
            }

            /// <summary>
            /// states if given point is contained in this bbox excluding Z evaluation
            /// </summary>
            /// <param name="tol">tolerance against Min, Max comparision</param>
            /// <param name="p">point to check if contained in this bbox (Z ignored)</param>
            /// <returns>true if given point (Z ignored) contained in this bbox</returns>
            public bool Contains2D(double tol, Vector3D p)
            {
                if (IsEmpty) return false;

                return
                    p.X.GreatThanOrEqualsTol(tol, Min.X) &&
                    p.Y.GreatThanOrEqualsTol(tol, Min.Y) &&
                    p.X.LessThanOrEqualsTol(tol, Max.X) &&
                    p.Y.LessThanOrEqualsTol(tol, Max.Y);
            }

            /// <summary>
            /// create new bbox extending by subtract margin to Min and by add to Max
            /// </summary>            
            public BBox3D AddMargin(Vector3D margin)
            {
                return new BBox3D(new Vector3D[]
                {
                    Min - margin,
                    Max + margin
                });
            }

            /// <summary>
            /// script to paste in cad to draw bbox
            /// </summary>
            public string CadScript
            {
                get
                {
                    var sb = new StringBuilder();

                    var coords = Coords3D.ToList();

                    {
                        var q = coords.Take(4).ToList();
                        q.Add(q[0]);
                        sb.Append(q.CadScriptPolyline());
                    }

                    for (int i = 0; i < 4; ++i)
                    {
                        sb.AppendLine(new Line3D(coords[i], coords[i + 4]).CadScript);
                    }

                    {
                        var q = coords.Skip(4).ToList();
                        q.Add(q[0]);
                        sb.Append(q.CadScriptPolyline());
                    }

                    return sb.ToString();
                }
            }

            /// <summary>
            /// stringify bbox as Min-Max
            /// </summary>
            public override string ToString()
            {
                return $"{Min}-{Max}";
            }
        }

    }

    public static partial class SciExt
    {

        /// <summary>
        /// construct a bbox from given enumerable set of points
        /// </summary>
        /// <param name="pts">points to build bbox</param>
        /// <returns>bbox from given enumerable set of points</returns>
        public static BBox3D BBox(this IEnumerable<Vector3D> pts)
        {
            return new BBox3D(pts);
        }

        /// <summary>
        /// construct a bbox from given dxf EntityObject        
        /// </summary>
        /// <remarks>
        /// Currently Text, MText width not evaluated ( only insertion point is considered ).
        /// Arc is considered only From, MidPoint, To.
        /// Circle is considered only 4 points on circumference.
        /// </remarks>
        /// <param name="eo">dxf entity object</param>
        /// <param name="tol_len">tolerance for comparision length tests</param>
        /// <returns>new bbox that contains given dxf entity object</returns>
        public static BBox3D BBox(this EntityObject eo, double tol_len)
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
                        var arc = (eo as Arc).ToArc3D(tol_len);
                        return new BBox3D(new[] { arc.From, arc.To, arc.MidPoint });
                    }

                case EntityType.Circle: return ((Circle)eo).ToPolyline(4).BBox(tol_len);

                case EntityType.LwPolyline:
                    {
                        var lwpoly = (LwPolyline)eo;

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
        /// <param name="tol_len">tolerance for comparision length tests</param>
        /// <returns>new bbox containing given set of dxf entity objects</returns>        
        public static BBox3D BBox(this IEnumerable<EntityObject> ents, double tol_len)
        {
            var bbox = new BBox3D();

            foreach (var x in ents)
            {
                bbox = bbox.Union(x.BBox(tol_len));
            }

            return bbox;
        }

        /// <summary>
        /// create a text ready to paste in cad to generate corresponding bbox
        /// </summary>
        /// <param name="bbox">bbox for which generate cadscript</param>
        /// <returns>cadscript of given bbox</returns>
        public static string CadScript(this BBox3D bbox)
        {
            var sb = new StringBuilder();

            foreach (var x in bbox.ToFace3DList())
            {
                sb.AppendLine(x.CadScript());
            }

            return sb.ToString();
        }

        public static IEnumerable<Face3d> ToFace3DList(this BBox3D bbox)
        {
            var d = bbox.Max - bbox.Min;
            return DxfKit.Cuboid((bbox.Max + bbox.Min) / 2, d);
        }

        public static IEnumerable<Face3d> DrawCuboid(this BBox3D bbox, DxfObject dxfObj, Layer layer = null)
        {
            var ents = bbox.ToFace3DList().ToList();

            dxfObj.AddEntities(ents, layer);

            return ents;
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

                case EntityType.LwPolyline:
                    {
                        var lw = (LwPolyline)eo;
                        foreach (var x in lw.Vertexes) yield return x.Position.ToVector3D();
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
                        var circleLw = ((Circle)eo).ToPolyline(4);
                        foreach (var x in circleLw.Vertexes) yield return x.Position.ToVector3D();
                    }
                    break;

                default:
                    throw new NotImplementedException($"bbox not implemented for dxf entity type [{eo.Type.ToString()}]");
            }

        }

    }

}