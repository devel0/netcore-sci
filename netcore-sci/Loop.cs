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
        /// loop edge distinct filtered vertexes ( not optimized )
        /// </summary>
        public IEnumerable<Vector3D> Vertexes(double tol) =>
            Edges.SelectMany(w => w.Vertexes).Distinct(new Vector3DEqualityComparer(tol));

        public double Tol { get; private set; }

        [JsonConstructor]
        Loop()
        {
        }

        /// <summary>
        /// precondition: edges must lie on given plane
        /// </summary>        
        public Loop(double tol, Plane3D plane, IReadOnlyList<Edge> edges, bool checkSense)
        {
            Tol = tol;
            Edges = checkSense ? edges.CheckSense(tol).ToList() : edges.ToList();
            Plane = plane;
        }

        /// <summary>
        /// create loop from given edge template ; plane is detected from edges distribution
        /// </summary>
        public Loop(double tol, IEnumerable<Edge> edges, bool checkSense = true)
        {
            Tol = tol;
            Edges = checkSense ? edges.CheckSense(tol).ToList() : edges.ToList();
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
        public Loop(double tol, LwPolyline lwPolyline)
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
            var polysegs = Edges.Select(w => w.SGeomFrom).ToList();
            var res = polysegs.Select(v => v.ToUCS(Plane.CS)).ToList().XYArea(tol);

            foreach (var edge in Edges.Where(r => r.GeomType  == GeometryType.Arc3D))
            {
                var arc = (Arc3D)edge;

                var midpt = edge.MidPoint;
                if (polysegs.ContainsPoint(tol, midpt))
                    res -= arc.SegmentArea;
                else
                    res += arc.SegmentArea;
            }

            return res;
        }

        /// <summary>
        /// states if given edge is contained into this loop
        /// </summary>                                
        public (bool SGeomFromTestResult, bool SGeomToTestResult, bool MidPointTestResult)
        Contains(double tol,
            Edge edge,
            LoopContainsPointMode SGeomFromTestType,
            LoopContainsPointMode SGeomToTestType,
            LoopContainsPointMode MidPointTestType) =>
                (
                    SGeomFromTestResult: ContainsPoint(tol, edge.SGeomFrom, SGeomFromTestType),
                    SGeomToTestResult: ContainsPoint(tol, edge.SGeomTo, SGeomToTestType),
                    MidPointTestResult: ContainsPoint(tol, edge.MidPoint, MidPointTestType)
                );

        /// <summary>
        /// test if given edge contained in this loop ( perimeter not excluded )
        /// </summary>        
        public bool Contains(double tol, Edge edge) =>
            Contains(tol, edge,
                SGeomFromTestType: LoopContainsPointMode.InsideOrPerimeter,
                SGeomToTestType: LoopContainsPointMode.InsideOrPerimeter,
                MidPointTestType: LoopContainsPointMode.InsideOrPerimeter)
                .Eval(x => x.SGeomFromTestResult && x.SGeomToTestResult && x.MidPointTestResult);

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

            if (onperimeter)
            {
                if (mode == LoopContainsPointMode.InsideExcludedPerimeter) return false;
                return true;
            }
            else if (mode == LoopContainsPointMode.Perimeter) return false;

            var ray = pt.LineV(Plane.CS.BaseX);
            //var ray = pt.LineTo(MidPoint);

            var qits = this.Edges.Cast<Geometry>()
                .Intersect(tol, new[] { ray }, GeomSegmentMode.FromTo, GeomSegmentMode.Infinite)
                .ToList();

            if (qits.Count == 0) return false;

            var qips = qits.Where(r => r.GeomType == GeometryType.Vector3D)
                .Select(w => (Vector3D)w)
                .Where(w => ray.SemiLineContainsPoints(tol, w))
                .ToList();

            var rayVNorm = ray.V.Normalized();
            var sortedColinearPts = qips
                .Select(ip => new
                {
                    ip,
                    off = ip.ColinearScalarOffset(tol, ray.From, rayVNorm)
                })
                .OrderBy(w => w.off)
                .Where(w => w.off.GreatThanOrEqualsTol(tol, 0))
                .Select(w => w.ip)
                .ToList();

            return sortedColinearPts.Count % 2 != 0;
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
        /// states if this loop contains given other loop (perimeter not excluded)
        /// </summary>
        public bool Contains(double tol, Loop other)
        {
            foreach (var otherEdge in other.Edges)
                if (!Contains(tol, otherEdge)) return false;

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
        public LwPolyline ToLwPolyline(double tol) => Edges.Cast<Geometry>().ToLwPolyline(tol, Plane.CS);

        /// <summary>
        /// create dxf lwpolyline from this loop
        /// </summary>
        public LwPolyline DxfEntity(double tol) => ToLwPolyline(tol);

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

    }

    public static partial class SciExt
    {

        public static Loop ToLoop(this netDxf.Entities.LwPolyline lwpolyline, double tol) =>
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