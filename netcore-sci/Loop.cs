using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using static System.FormattableString;

using netDxf;
using netDxf.Entities;

using SearchAThing;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;

namespace SearchAThing
{

    public enum LoopContainsPointMode
    {
        /// <summary>
        /// point is on perimeter
        /// </summary>
        Perimeter,

        /// <summary>
        /// point is inside (perimeter excluded)
        /// </summary>
        InsideExcludedPerimeter,

        /// <summary>
        /// point is inside or on perimeter
        /// </summary>
        InsideOrPerimeter
    };

    public enum LoopContainsEdgeMode
    {
        /// <summary>
        /// point is on perimeter
        /// </summary>
        Perimeter,

        /// <summary>
        /// point is inside (perimeter excluded)
        /// </summary>
        InsideExcludedPerimeter,

        /// <summary>
        /// point is inside or on perimeter
        /// </summary>
        InsideOrPerimeter,

        /// <summary>
        /// midpoint is inside
        /// </summary>
        MidPointInside,
    };

    /// <summary>
    /// planar edges loop containing line and arcs
    /// </summary>    
    public class Loop
    {

        /// <summary>
        /// plane where loop edges resides
        /// </summary>        
        public Plane3D Plane { get; private set; }

        /// <summary>
        /// loop edges ( line, arc )
        /// </summary>        
        public IReadOnlyList<Edge> Edges { get; private set; }

        /// <summary>
        /// check if two loops equals
        /// </summary>        
        public bool Equals(double tol, Loop other)
        {
            if (Edges.Count != other.Edges.Count) return false;
            if (!Area.EqualsTol(tol, other.Area)) return false;
            if (!Length.EqualsTol(tol, other.Length)) return false;

            foreach (var edge in Edges)
            {
                if (!other.Edges.Any(otherEdge => otherEdge.GeomEquals(tol, edge))) return false;
            }

            return true;
        }

        /// <summary>
        /// loop edge distinct filtered vertexes ( not optimized )
        /// </summary>
        public IEnumerable<Vector3D> Vertexes(double tol) =>
            Edges.SelectMany(w => w.Vertexes).Distinct(new Vector3DEqualityComparer(tol));

        public double Tol { get; private set; }

#pragma warning disable CS8618
        [JsonConstructor]
        Loop()
        {

        }
#pragma warning restore

        /// <summary>
        /// precondition: edges must lie on given plane
        /// </summary>        
        public Loop(double tol, Plane3D plane, IEnumerable<Edge> _edges, bool checkSense, bool checkSort = false)
        {
            Tol = tol;

            if (checkSort) _edges = _edges.CheckSort(tol).ToList();

            if (checkSense) _edges = _edges.CheckSense(tol).ToList();

            Edges = _edges.ToReadOnlyList();
            Plane = plane;
        }

        /// <summary>
        /// create loop from given edge template ; plane is detected from edges distribution
        /// </summary>
        public Loop(double tol, IEnumerable<Edge> edges, bool checkSense = true, bool checkSort = false)
        {
            Tol = tol;

            List<Edge>? _edges = null;

            if (checkSort) _edges = edges.CheckSort(tol).ToList();

            if (checkSense) _edges = (_edges == null) ? edges.CheckSense(tol).ToList() : _edges.CheckSense(tol).ToList();

            Edges = (_edges == null) ? edges.ToList() : _edges;

            Plane = Edges.DetectPlane(tol);
        }

        /// <summary>
        /// create loop from given edges, plane template
        /// </summary>
        public Loop(double tol, IEnumerable<Edge> edges, Plane3D plane, bool checkSense = true)
        {
            Tol = tol;
            Edges = checkSense ? edges.CheckSense(tol).ToList() : edges.ToList();
            Plane = plane;
        }

        /// <summary>
        /// create a loop from given lwpolyline template
        /// </summary>        
        public Loop(double tol, Polyline2D lwPolyline)
        {
            Tol = tol;
            Edges = lwPolyline.ToGeometries(tol).Cast<Edge>().CheckSense(tol).ToList();
            Plane = lwPolyline.ToPlane();
        }

        /// <summary>
        /// retrieve another loop with reversed edges with sense toggled
        /// </summary>
        public Loop InvertSense(double tol) => new Loop(tol,
           Plane, Edges.Reverse().Select(w => (Edge)w.ToggleSense()).ToList(), checkSense: false);

        /// <summary>
        /// move loop (plane and edges) of given delta
        /// </summary>
        public Loop Move(Vector3D delta) =>
            new Loop(Tol, Plane.Move(delta), Edges.Select(edge => (Edge)edge.Move(delta)).ToList(), checkSense: false);

        double? _Area = null;

        /// <summary>
        /// (cached) area of the loop ( segment and arc are evaluated )
        /// </summary>        
        public double Area
        {
            get
            {
                if (_Area == null) _Area = ComputeArea(Tol);

                return _Area.Value;
            }
        }

        double? _Length = null;

        /// <summary>
        /// (cached) loop perimeter length
        /// </summary>        
        public double Length
        {
            get
            {
                if (_Length == null) _Length = Edges.Sum(w => w.Length);
                return _Length.Value;
            }
        }

        double ComputeArea(double tol)
        {
            if (Edges.Count <= 2) return 0;

            var polysegs = Edges.Select(w => w.SGeomFrom).ToList();
            var res = polysegs.Select(v => v.ToUCS(Plane.CS)).ToList().XYArea(tol);

            foreach (var edge in Edges.Where(r => r.GeomType == GeometryType.Arc3D))
            {
                var arc = (Arc3D)edge;

                var arcmidpt = edge.MidPoint;
                var segmidpt = edge.SGeomFrom.LineTo(edge.SGeomTo).MidPoint;
                var testpt = segmidpt + (arcmidpt - segmidpt).Normalized() * 2 * tol;

                if (polysegs.ContainsPoint(tol, testpt))
                    res -= arc.SegmentArea;
                else
                    res += arc.SegmentArea;
            }

            return res;
        }

        /// <summary>
        /// test if given edge contained in this loop
        /// </summary>        
        public bool Contains(double tol, Edge edge,
            LoopContainsEdgeMode mode = LoopContainsEdgeMode.InsideOrPerimeter)
        {
            LoopContainsPointMode? sgeomFromMode = null;
            LoopContainsPointMode? sgeomToMode = null;
            LoopContainsPointMode? midPointMode = null;

            switch (mode)
            {
                case LoopContainsEdgeMode.Perimeter:
                    sgeomFromMode = sgeomToMode = midPointMode = LoopContainsPointMode.Perimeter;
                    break;

                case LoopContainsEdgeMode.MidPointInside:
                    //sgeomFromMode = sgeomToMode = LoopContainsPointMode.InsideOrPerimeter;
                    midPointMode = LoopContainsPointMode.InsideExcludedPerimeter;
                    break;

                case LoopContainsEdgeMode.InsideExcludedPerimeter:
                    sgeomFromMode = sgeomToMode = midPointMode = LoopContainsPointMode.InsideExcludedPerimeter;
                    break;

                case LoopContainsEdgeMode.InsideOrPerimeter:
                    sgeomFromMode = sgeomToMode = midPointMode = LoopContainsPointMode.InsideOrPerimeter;
                    break;

                default: throw new Exception($"unknown mode {mode}");
            }

            if (sgeomFromMode != null)
            {
                if (!ContainsPoint(tol, edge.SGeomFrom, sgeomFromMode.Value)) return false;
            }

            if (sgeomToMode != null)
            {
                if (!ContainsPoint(tol, edge.SGeomTo, sgeomToMode.Value)) return false;
            }

            if (midPointMode != null)
            {
                if (!ContainsPoint(tol, edge.MidPoint, midPointMode.Value)) return false;
            }

            return true;
        }

        Vector3D? _MidPoint = null;
        /// <summary>
        /// (cached) geometric midpoint of all edges midpoint of this loop ( used in Contains for ray construction )
        /// </summary>        
        public Vector3D MidPoint
        {
            get
            {
                if (_MidPoint == null) _MidPoint = Edges.Select(w => w.MidPoint).Sum() / Edges.Count;
                return _MidPoint;
            }
        }

        /// <summary>
        /// states if given point is included into this loop
        /// </summary>                
        public bool ContainsPoint(double tol, Vector3D pt, LoopContainsPointMode mode)
        {
            var onperimeter = this.Edges.Any(edge => edge.EdgeContainsPoint(tol, pt));

            if (mode == LoopContainsPointMode.Perimeter) return onperimeter;

            if (mode == LoopContainsPointMode.InsideExcludedPerimeter && onperimeter) return false;

            if (onperimeter) return true;

            var ray = pt.LineV(Plane.CS.BaseX);

            int K = 0;

            var ipHs = new HashSet<Vector3D>(new Vector3DEqualityComparer(tol));

            var tolfactor = 1.1d;
            // if use 1.0 tolfactor some test will fail because
            // of different tolerance management between test of line overlap
            // with colinear test result and line/line intersect method

            for (int edgeIdx = 0; edgeIdx < Edges.Count; ++edgeIdx)
            {
                var edge = Edges[edgeIdx];

                var intersectRes = edge.GeomIntersect(tol, ray, GeomSegmentMode.FromTo, GeomSegmentMode.Infinite).ToList();

                foreach (var ires in intersectRes)
                {
                    switch (ires.GeomType)
                    {
                        case GeometryType.Vector3D:
                            {
                                var ip = (Vector3D)ires;
                                if (ipHs.Contains(ip))
                                    return this.ContainsPoint(tol, pt + tolfactor * tol * Plane.CS.BaseY, mode);

                                if (ray.SemiLineContainsPoint(tol, ip))
                                    ++K;

                                ipHs.Add(ip);
                            }
                            break;

                        case GeometryType.Line3D:
                            {


                                return this.ContainsPoint(tol, pt + tolfactor * tol * Plane.CS.BaseY, mode);
                            }
                    }
                }
            }

            return K % 2 != 0;
        }

        BBox3D? _BBox = null;

        /// <summary>
        /// (cached) bbox of this loop WCS coords
        /// </summary>        
        public BBox3D BBox
        {
            get
            {
                if (_BBox == null) _BBox = this.Edges.Select(w => w.SGeomFrom).BBox();

                return _BBox;
            }
        }


        BBox3D? _CSBBox = null;

        /// <summary>
        /// (cached) bbox of this loop CS coords
        /// </summary>        
        public BBox3D CSBox
        {
            get
            {
                if (_CSBBox == null) _CSBBox = this.Edges.Select(w => w.SGeomFrom.ToUCS(Plane.CS)).BBox();

                return _CSBBox;
            }
        }

        /// <summary>
        /// states if this loop contains given other loop
        /// </summary>
        public bool Contains(double tol, Loop other,
            LoopContainsEdgeMode mode = LoopContainsEdgeMode.InsideOrPerimeter)
        {
            foreach (var otherEdge in other.Edges)
                if (!Contains(tol, otherEdge, mode)) return false;

            return true;
        }

        /// <summary>
        /// create an hatch from given loop, pattern
        /// </summary>        
        public netDxf.Entities.Hatch ToHatch(double tol, HatchPattern pattern, bool associative = true) =>
            ToLwPolyline(tol).ToHatch(pattern, associative);

        /// <summary>
        /// create dxf lwpolyline from this loop
        /// </summary>
        public Polyline2D ToLwPolyline(double tol) => Edges.Cast<Geometry>().ToLwPolyline(tol, Plane.CS, closed: true);

        /// <summary>
        /// create dxf lwpolyline from this loop
        /// </summary>
        public Polyline2D DxfEntity(double tol) => ToLwPolyline(tol);

        public override string ToString() => Invariant($"A:{Area} L:{Length} Edges:{Edges.Count}");

        public string DebugDump(int digits = 3)
        {
            var sb = new StringBuilder();
            foreach (var edge in Edges)
            {
                sb.AppendLine(edge.ToString(digits));
            }

            return sb.ToString();
        }

        /// <summary>
        /// qcad 2d script
        /// </summary>
        /// <param name="final">adds QQ command</param>        
        public string QCadScript(bool final = true)
        {
            var sb = new StringBuilder();

            foreach (var edge in Edges)
            {
                sb.Append(edge.QCadScript(final: false));
            }

            if (final) sb.AppendLine("QQ");

            return sb.ToString();
        }

        public string A0QCadScript => QCadScript();

        public string ProgeCadScript(bool final = true)
        {
            var sb = new StringBuilder();

            foreach (var edge in Edges)
            {
                sb.Append(edge.ProgeCadScript(final: false));
                if (edge.GeomType == GeometryType.Line3D) sb.AppendLine();
            }

            if (final) sb.AppendLine("");

            return sb.ToString();
        }

        public string A0ProgeCadScript => ProgeCadScript();

    }

    public class LoopEqualityComparer : IEqualityComparer<Loop>
    {
        double tol;

        public LoopEqualityComparer(double tol)
        {
            this.tol = tol;
        }

        public bool Equals(Loop? x, Loop? y) => (x == null || y == null) ? false : x.Equals(tol, y);

        public int GetHashCode([DisallowNull] Loop obj) => 0;
    }

    public static partial class SciExt
    {

        public static Loop ToLoop(this Polyline2D lwpolyline, double tol) =>
            new Loop(tol, lwpolyline);

        public static Plane3D DetectPlane(this IEnumerable<Edge> edges, double tol)
        {
            var lines = new List<Line3D>();

            foreach (var edge in edges)
            {
                switch (edge.GeomType)
                {
                    case GeometryType.Line3D: lines.Add((Line3D)edge); break;
                    case GeometryType.Arc3D: return new Plane3D(((Arc3D)edge).CS);
                    default:
                        throw new Exception($"unexpected edge type {edge.GeomType}");
                }
            }

            if (lines.Count == 0) throw new Exception($"can't state edges (cnt:{edges.Count()}) plane");

            return lines.BestFittingPlane(tol);
        }

    }

    public static partial class SciToolkit
    {

    }

}