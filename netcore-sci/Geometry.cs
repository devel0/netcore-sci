using System.Collections.Generic;
using System.Linq;

namespace SearchAThing
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

        public Geometry(GeometryType type) { GeomType = type; }

        public GeometryType GeomType { get; protected set; }

        public abstract IEnumerable<Vector3D> Vertexes { get; }
        public abstract Vector3D GeomFrom { get; }
        public abstract Vector3D GeomTo { get; }
        public abstract double Length { get; }
        public abstract IEnumerable<Vector3D> Divide(int cnt, bool include_endpoints = false);
        public abstract BBox3D BBox(double tol_len);
        public abstract IEnumerable<Geometry> Intersect(double tol_len, Geometry other);

        public abstract netDxf.Entities.EntityObject DxfEntity { get; }

        public static implicit operator netDxf.Entities.EntityObject(Geometry geom)
        {
            return geom.DxfEntity;
        }

    }

    public static partial class SciExt
    {

        /// <summary>
        /// extracs Arc3D, Line3D from given lwpolyline
        /// </summary>        
        public static IEnumerable<Geometry> ToGeometries(this netDxf.Entities.LwPolyline lwpolyline,
            double tolLen)
        {
            var geoms = new List<Geometry>();

            var els = lwpolyline.Explode();

            foreach (var el in els)
            {
                if (el.Type == netDxf.Entities.EntityType.Arc)
                {
                    yield return ((netDxf.Entities.Arc)el).ToArc3D(tolLen);
                }
                else if (el.Type == netDxf.Entities.EntityType.Line)
                {
                    var line = (netDxf.Entities.Line)el;
                    if (((Vector3D)line.StartPoint).EqualsTol(tolLen, line.EndPoint)) continue;

                    yield return line.ToLine3D();
                }
            }
        }

        public static Loop ToLoop(this netDxf.Entities.LwPolyline lwpolyline, double tol) =>
            new Loop(tol, lwpolyline);

        /// <summary>
        /// segments representation of given geometries
        /// if arc found a segment between endpoints returns
        /// </summary>        
        public static IEnumerable<Line3D> Segments(this IEnumerable<Geometry> geometry_block, double tol_len)
        {
            var en = geometry_block.GetEnumerator();

            Vector3D? prev = null;

            while (en.MoveNext())
            {
                var geom = en.Current;

                switch (geom.GeomType)
                {
                    case GeometryType.Vector3D:
                        {
                            var cur = (Vector3D)geom;
                            if (prev != null) yield return new Line3D(prev, cur);
                            prev = cur;
                        }
                        break;

                    case GeometryType.Line3D:
                        {
                            var cur = (Line3D)geom;
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
                            var cur = (Arc3D)geom;
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

                    default: throw new System.Exception($"unsupported type [{geom.GeomType}] on Segments function");
                }
            }
        }

        public static IEnumerable<Vector3D> Vertexes(this IReadOnlyList<Geometry> geometry_block, double tolLen)
        {
            Vector3D? last = null;
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

            if (geometry_block.Count(r => r.GeomType == GeometryType.Arc3D) > 1)
            {
                var arcs = geometry_block.Where(r => r.GeomType == GeometryType.Arc3D).Take(2).Cast<Arc3D>().ToList();
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

        public static IEnumerable<Geometry> Intersect(this IEnumerable<Geometry> _geom1, double tol_len,
            IEnumerable<Geometry> _geom2)
        {
            var geom1 = _geom1.ToList();
            var geom2 = _geom2.ToList();

            var res = new List<Geometry>();

            foreach (var g1 in geom1)
            {
                foreach (var g2 in geom2)
                {
                    var g1g2_intersection = g1.Intersect(tol_len, g2);

                    if (g1g2_intersection != null)
                        foreach (var geom in g1g2_intersection) yield return geom;
                }
            }
        }

        public static BBox3D BBox(this IEnumerable<Geometry> geometry_block, double tol_len)
        {
            var bbox = new BBox3D();

            foreach (var x in geometry_block) bbox = bbox.Union(x.BBox(tol_len));

            return bbox;
        }

    }

}