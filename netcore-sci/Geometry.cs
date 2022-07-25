using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SearchAThing
{

    public enum GeometryType
    {
        Vector3D,
        Line3D,
        Circle3D,
        Arc3D
    }

    public enum GeomSegmentMode
    {
        /// <summary>
        /// infinite line
        /// </summary>
        Infinite,

        /// <summary>
        /// Semi-line start at From
        /// </summary>
        From,

        /// <summary>
        /// Semi-line ending at To
        /// </summary>
        To,

        /// <summary>
        /// Segment from-to
        /// </summary>
        FromTo
    };

    /// <summary>
    /// base abstract type for geometries such as Vector3D, Line3D, Arc3D and Circle3D
    /// </summary>
    public abstract class Geometry
    {

        /// <summary>
        /// create copy of this geometry.        
        /// </summary>        
        /// <remarks>
        /// it's required to call base.CopyFrom(other) to ensure geometry properties to be copied.
        /// </remarks>
        public abstract Geometry Copy();

        protected Geometry(GeometryType type) { GeomType = type; }

        /// <summary>
        /// type of geometry
        /// </summary>
        public GeometryType GeomType { get; protected set; }

        /// <summary>
        /// vertexes of this geom ( can be 1 for points, 2 for line/arc/circles )
        /// </summary>        
        public abstract IEnumerable<Vector3D> Vertexes { get; }

        /// <summary>
        /// start point
        /// </summary>        
        public abstract Vector3D GeomFrom { get; }

        /// <summary>
        /// end point
        /// </summary>        
        public abstract Vector3D GeomTo { get; }

        /// <summary>
        /// geometry length ( 0 for point, line length for lines, perimeter for arc/circles )
        /// </summary>    
        public abstract double Length { get; }

        public abstract Vector3D MidPoint { get; }

        /// <summary>
        /// find split points for this geometry splitter int cnt parts
        /// </summary>
        /// <param name="cnt"></param>
        /// <param name="include_endpoints">if true GeomFrom and GeomTo will added</param>        
        public abstract IEnumerable<Vector3D> Divide(int cnt, bool include_endpoints = false);

        /// <summary>
        /// split geometry in given break points.
        /// precondition: breaks must lie on the geometry perimeter
        /// </summary>        
        public abstract IEnumerable<Geometry> Split(double tol, IEnumerable<Vector3D> breaks);

        public abstract Geometry Move(Vector3D delta);

        /// <summary>
        /// bbox of this geom
        /// </summary>        
        public abstract BBox3D BBox(double tol);

        /// <summary>
        /// find intersections between this and another geometry resulting in zero or more geometries.        
        /// </summary>
        /// <param name="tol"></param>
        /// <param name="other"></param>
        /// <param name="thisSegmentMode">if this is Line3D specifies how to consider</param>
        /// <param name="otherSegmentMode">if other is Line3D specifies how to consider</param>
        /// <returns></returns>
        public abstract IEnumerable<Geometry> GeomIntersect(double tol, Geometry other,
            GeomSegmentMode thisSegmentMode = GeomSegmentMode.FromTo,
            GeomSegmentMode otherSegmentMode = GeomSegmentMode.FromTo);

        /// <summary>
        /// states if this geom equals to given other
        /// </summary>
        /// <param name="tol">length tolerance</param>
        /// <param name="other">other geom</param>
        /// <param name="checkSense">if false two geometry with different sense but same space coverage are considered equals</param>        
        public abstract bool GeomEquals(double tol, Geometry other, bool checkSense = false);

        /// <summary>
        /// dxf entity representing this geom
        /// </summary>        
        public abstract netDxf.Entities.EntityObject DxfEntity { get; }

        /// <summary>
        /// convert to dxf entity
        /// </summary>
        /// <param name="geom"></param>
        public static implicit operator netDxf.Entities.EntityObject(Geometry geom) => geom.DxfEntity;

    }

    public static partial class SciExt
    {

        public static Plane3D ToPlane(this netDxf.Entities.LwPolyline lwpolyline) =>
            new Plane3D(new CoordinateSystem3D(
                o: ((Vector3D)lwpolyline.Normal) * lwpolyline.Elevation,
                normal: lwpolyline.Normal,
                csAutoType: CoordinateSystem3DAutoEnum.AAA));

        /// <summary>
        /// extracs Arc3D, Line3D from given lwpolyline
        /// </summary>
        /// <param name="lwpolyline"></param>
        /// <param name="tol">length tolerance</param>        
        public static IEnumerable<Geometry> ToGeometries(this netDxf.Entities.LwPolyline lwpolyline,
            double tol)
        {
            var geoms = new List<Geometry>();

            var els = lwpolyline.Explode();

            foreach (var el in els)
            {
                if (el.Type == netDxf.Entities.EntityType.Arc)
                {
                    yield return ((netDxf.Entities.Arc)el).ToArc3D();
                }
                else if (el.Type == netDxf.Entities.EntityType.Line)
                {
                    var line = (netDxf.Entities.Line)el;
                    if (((Vector3D)line.StartPoint).EqualsTol(tol, line.EndPoint)) continue;

                    yield return line.ToLine3D();
                }
            }
        }

        /// <summary>
        /// segments representation of given geometries
        /// if arc found a segment between endpoints returns
        /// </summary>
        /// <param name="geometry_block"></param>
        /// <param name="tol">length tolerance</param>        
        public static IEnumerable<Line3D> Segments(this IEnumerable<Geometry> geometry_block, double tol)
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
                                if (cur.From.EqualsTol(tol, prev))
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
                                if (cur.From.EqualsTol(tol, prev))
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

        /// <summary>
        /// vertexes from given set of geometries
        /// </summary>
        /// <param name="geometry_block"></param>
        /// <param name="tol">length tolerance</param>        
        public static IEnumerable<Vector3D> Vertexes(this IReadOnlyList<Geometry> geometry_block, double tol)
        {
            Vector3D? last = null;
            for (int i = 0; i < geometry_block.Count; ++i)
            {
                var geom = geometry_block[i];
                var from = geom.GeomFrom;

                if (last != null)
                {
                    if (last.EqualsTol(tol, from))
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

        /// <summary>
        /// centroid of given geometries (not fully implemented)
        /// </summary>
        /// <param name="geometry_block"></param>
        /// <param name="tol"length tolerance></param>        
        public static Vector3D GeomCentroid(this IReadOnlyList<Geometry> geometry_block, double tol)
        {
            var segs = geometry_block.Vertexes(tol).ToList();

            // TODO centroid with polyline and arcs

            if (geometry_block.Count(r => r.GeomType == GeometryType.Arc3D) > 1)
            {
                var arcs = geometry_block.Where(r => r.GeomType == GeometryType.Arc3D).Take(2).Cast<Arc3D>().ToList();
                return (arcs[0].MidPoint + arcs[1].MidPoint) / 2;
            }
            else
            {
                var A = XYArea(segs, tol);
                var centroid = XYCentroid(segs, tol, A);

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

                throw new NotImplementedException();

                return centroid;
            }
        }

        /// <summary>
        /// find intersection geometries resulting from all this geometries with all given geom2
        /// </summary>
        /// <param name="_geom1"></param>
        /// <param name="tol">length tolerance</param>
        /// <param name="_geom2"></param>        
        /// <param name="geom1SegmentMode">if geom1 item is Line3D specifies how to consider it</param>        
        /// <param name="geom2SegmentMode">if geom2 item is Line3D specifies how to consider it</param>        
        public static IEnumerable<(Geometry intersectGeom, Geometry g1, Geometry g2)> Intersect(this IEnumerable<Geometry> _geom1,
            double tol, IEnumerable<Geometry> _geom2,
            GeomSegmentMode geom1SegmentMode = GeomSegmentMode.FromTo,
            GeomSegmentMode geom2SegmentMode = GeomSegmentMode.FromTo)
        {
            var geom1 = _geom1.ToList();
            var geom2 = _geom2.ToList();

            foreach (var g1 in geom1)
            {
                foreach (var g2 in geom2)
                {
                    var g1g2_intersection = g1.GeomIntersect(tol, g2, geom1SegmentMode, geom2SegmentMode);

                    if (g1g2_intersection != null)
                        foreach (var geom in g1g2_intersection) yield return (intersectGeom: geom, g1, g2);
                }
            }
        }

        /// <summary>
        /// bbox that cover all given geometries
        /// </summary>
        /// <param name="geometry_block"></param>
        /// <param name="tol">length tolerance</param>        
        public static BBox3D BBox(this IEnumerable<Geometry> geometry_block, double tol)
        {
            var bbox = new BBox3D();

            foreach (var x in geometry_block) bbox = bbox.Union(x.BBox(tol));

            return bbox;
        }

    }

}