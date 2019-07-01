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
            public bool IsEmpty { get { return Min == null; } }

            public Vector3D Min { get; private set; }
            public Vector3D Max { get; private set; }
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

            public BBox3D()
            {
            }

            public BBox3D Scale(double factor)
            {
                var center = (Min + Max) / 2;

                return new BBox3D(new Vector3D[]
                {
                    Min.ScaleAbout(center, factor),
                    Max.ScaleAbout(center, factor)
                });
            }

            public BBox3D(IEnumerable<Vector3D> pts)
            {
                double xmin = 0, ymin = 0, zmin = 0;
                double xmax = 0, ymax = 0, zmax = 0;

                bool firstPt = true;

                foreach (var p in pts)
                {
                    if (firstPt)
                    {
                        xmin = xmax = p.X;
                        ymin = ymax = p.Y;
                        zmin = zmax = p.Z;
                        firstPt = false;
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

            public BBox3D Union(Vector3D p)
            {
                if (IsEmpty)
                {
                    return new BBox3D()
                    {
                        Min = p,
                        Max = p
                    };
                }
                else
                {
                    return new BBox3D()
                    {
                        Min = new Vector3D(Min(Min.X, p.X), Min(Min.Y, p.Y), Min(Min.Z, p.Z)),
                        Max = new Vector3D(Max(Max.X, p.X), Max(Max.Y, p.Y), Max(Max.Z, p.Z))
                    };
                }
            }

            public BBox3D Union(BBox3D other)
            {
                if (IsEmpty) return other;
                if (other.IsEmpty) return this;
                return this.Union(other.Min).Union(other.Max);
            }

            public bool EqualsTol(double tol, BBox3D other)
            {
                if (IsEmpty) return other.IsEmpty;
                if (other.IsEmpty) return false;
                return Min.EqualsTol(tol, other.Min) && Max.EqualsTol(tol, other.Max);
            }

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

            public override string ToString()
            {
                return $"{Min}-{Max}";
            }
        }

    }

    public static partial class SciExt
    {

        public static BBox3D BBox(this IEnumerable<Vector3D> pts)
        {            
            return new BBox3D(pts);
        }

        public static BBox3D BBox(this EntityObject eo)
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
                        var arc = (eo as Arc).ToArc3D();
                        return new BBox3D(new[] { arc.From, arc.To, arc.MidPoint });
                    }

                case EntityType.Circle: return ((Circle)eo).ToPolyline(4).BBox();

                case EntityType.LightWeightPolyline:
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

        public static BBox3D BBox(this IEnumerable<EntityObject> ents)
        {
            var bbox = new BBox3D();

            foreach (var x in ents)
            {
                bbox = bbox.Union(x.BBox());
            }

            return bbox;
        }

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

                case EntityType.LightWeightPolyline:
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