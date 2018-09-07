using SearchAThing.Sci;
using System.Collections.Generic;
using System.Linq;
using SearchAThing.Util;

namespace SearchAThing
{

    namespace Sci
    {

        public enum GeometryType
        {
            Vector3D,
            Line3D,
            Circle3D,
            Arc3D
        }

        public abstract class Geometry
        {

            public Geometry(GeometryType type) { Type = type; }

            public GeometryType Type { get; protected set; }

            public abstract IEnumerable<Vector3D> Vertexes { get; }
            public abstract Vector3D GeomFrom { get; }
            public abstract Vector3D GeomTo { get; }
            public abstract double Length { get; }
            public abstract IEnumerable<Vector3D> Divide(int cnt, bool include_endpoints = false);
            public abstract BBox3D BBox(double tol_len, double tol_rad);

            public abstract netDxf.Entities.EntityObject DxfEntity { get; }

            public static implicit operator netDxf.Entities.EntityObject(Geometry geom)
            {
                return geom.DxfEntity;
            }

        }

    }

    public static partial class Extensions
    {

        public static IEnumerable<Geometry> ToGeometryBlock(this netDxf.Entities.LwPolyline lwpolyline, double tolLen)
        {
            var geoms = new List<Geometry>();

            var els = lwpolyline.Explode();

            foreach (var el in els)
            {
                if (el.Type == netDxf.Entities.EntityType.Arc)
                {
                    yield return (el as netDxf.Entities.Arc).ToArc3D();
                }
                else if (el.Type == netDxf.Entities.EntityType.Line)
                {
                    var line = (el as netDxf.Entities.Line);
                    if (((Vector3D)line.StartPoint).EqualsTol(tolLen, line.EndPoint)) continue;

                    yield return line.ToLine3D();
                }
            }
        }

        /// <summary>
        /// segments representation of given geometries
        /// if arc found a segment between endpoints returns
        /// </summary>        
        public static IEnumerable<Line3D> Segments(this IEnumerable<Geometry> geometry_block, double tol_len)
        {
            var en = geometry_block.GetEnumerator();

            Vector3D prev = null;

            while (en.MoveNext())
            {
                var geom = en.Current;

                switch (geom.Type)
                {
                    case GeometryType.Vector3D:
                        {
                            var cur = geom as Vector3D;
                            if (prev != null) yield return new Line3D(prev, cur);
                            prev = cur;
                        }
                        break;

                    case GeometryType.Line3D:
                        {
                            var cur = geom as Line3D;
                            if (prev == null)
                            {
                                yield return cur;
                                prev = cur.To;
                            }
                            else
                            {
                                if (cur.From.EqualsTol(tol_len, prev))
                                {
                                    yield return cur;
                                    prev = cur.To;
                                }
                                else
                                {
                                    yield return cur.Reverse();
                                    prev = cur.From;
                                }
                            }
                        }
                        break;

                    case GeometryType.Arc3D:
                        {
                            var cur = geom as Arc3D;
                            if (prev == null)
                            {
                                yield return cur.Segment;
                                prev = cur.To;
                            }
                            else
                            {
                                if (cur.From.EqualsTol(tol_len, prev))
                                {
                                    yield return cur.Segment;
                                    prev = cur.To;
                                }
                                else
                                {
                                    yield return cur.Segment.Reverse();
                                    prev = cur.From;
                                }
                            }
                        }
                        break;

                    default: throw new System.Exception($"unsupported type [{geom.Type}] on Segments function");
                }
            }
        }

        public static IEnumerable<Vector3D> Vertexes(this IReadOnlyList<Geometry> geometry_block, double tolLen)
        {
            Vector3D last = null;
            for (int i = 0; i < geometry_block.Count; ++i)
            {
                var geom = geometry_block[i];
                var from = geom.GeomFrom;

                if (last != null)
                {
                    if (last.EqualsTol(tolLen, from))
                    {
                        last = geom.GeomTo;
                        yield return from;
                    }
                    else
                    {
                        last = geom.GeomFrom;
                        yield return geom.GeomTo;
                    }
                }
                else
                {
                    last = geom.GeomTo;
                    yield return from;
                }
            }
        }

        public static Vector3D GeomCentroid(this IReadOnlyList<Geometry> geometry_block, double tolLen)
        {
            var segs = geometry_block.Vertexes(tolLen).ToList();

            // TODO centroid with polyline and arcs

            if (geometry_block.Count(r => r.Type == GeometryType.Arc3D) > 1)
            {
                var arcs = geometry_block.Where(r => r.Type == GeometryType.Arc3D).Take(2).Cast<Arc3D>().ToList();
                return (arcs[0].MidPoint + arcs[1].MidPoint) / 2;
            }
            else
            {
                var A = Area(segs, tolLen);
                var centroid = Centroid(segs, tolLen, A);

                /*
                // search for arcs
                foreach (var geom in geometry_block)
                {
                    if (geom.Type == GeometryType.Arc3D)
                    {
                        var arc = geom as Arc3D;
                        var arc_sign = segs.ContainsPoint(tolLen, arc.MidPoint) ? -1.0 : 1.0;
                        var arc_A = 0.0;
                        var arc_centre_of_mass = arc.CentreOfMass(out arc_A);

                        var new_centroid_x = (centroid.X * A + arc_centre_of_mass.X * arc_A * arc_sign) / (A + arc_A * arc_sign);
                        var new_centroid_y = (centroid.Y * A + arc_centre_of_mass.Y * arc_A * arc_sign) / (A + arc_A * arc_sign);

                        A += arc_A * arc_sign;
                        centroid = new Vector3D(new_centroid_x, new_centroid_y, 0);
                    }
                }
                */

                return centroid;
            }
        }

        public static BBox3D BBox(this IEnumerable<Geometry> geometry_block, double tol_len, double tol_rad)
        {
            var bbox = new BBox3D();

            foreach (var x in geometry_block) bbox = bbox.Union(x.BBox(tol_len, tol_rad));

            return bbox;
        }

    }

}