using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

using netDxf;
using netDxf.Entities;

using SearchAThing;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;

namespace SearchAThing
{

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
        public IReadOnlyList<IEdge> Edges { get; private set; }

        /// <summary>
        /// loop edge vertexes
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
        public Loop(double tol, Plane3D plane, IReadOnlyList<IEdge> edges, bool checkSense)
        {
            Tol = tol;
            Edges = checkSense ? edges.CheckSense(tol).ToList() : edges.ToList();
            Plane = plane;
        }

        /// <summary>
        /// create loop from given edge template ; plane is detected from edges distribution
        /// </summary>
        public Loop(double tol, IEnumerable<IEdge> edges, bool checkSense = true)
        {
            Tol = tol;
            Edges = checkSense ? edges.CheckSense(tol).ToList() : edges.ToList();
            Plane = Edges.DetectPlane(tol);
        }

        /// <summary>
        /// create loop from given edges, plane template
        /// </summary>
        public Loop(double tol, IEnumerable<IEdge> edges, Plane3D plane, bool checkSense = true)
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
            Edges = lwPolyline.ToGeometries(tol).Cast<IEdge>().CheckSense(tol).ToList();
            Plane = lwPolyline.ToPlane();
        }

        /// <summary>
        /// retrieve another loop with reversed edges with sense toggled
        /// </summary>
        public Loop InvertSense(double tol) => new Loop(tol,
           Plane, Edges.Reverse().Select(w => (IEdge)w.ToggleSense()).ToList(), checkSense: false);

        /// <summary>
        /// move loop (plane and edges) of given delta
        /// </summary>
        public Loop Move(Vector3D delta) =>
            new Loop(Tol, Plane.Move(delta), Edges.Select(edge => edge.EdgeMove(delta)).ToList(), checkSense: false);

        double? _Area = null;

        /// <summary>
        /// area of the loop ( segment and arc are evaluated )
        /// </summary>        
        public double Area
        {
            get
            {
                if (_Area == null) _Area = ComputeArea(Tol);

                return _Area.Value;
            }
        }

        /// <summary>
        /// loop perimeter length
        /// </summary>        
        public double Length => Edges.Sum(w => w.Length);

        double ComputeArea(double tol)
        {
            var polysegs = Edges.Select(w => w.SGeomFrom).ToList();
            var res = polysegs.Select(v => v.ToUCS(Plane.CS)).ToList().XYArea(tol);

            foreach (var edge in Edges.Where(r => r.EdgeType == EdgeType.Arc3D))
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
        /// states if given point is included into this loop
        /// </summary>        
        /// <param name="excludePerimeter">if true, point on perimeter isn't included</param>        
        public bool ContainsPoint(double tol, Vector3D pt, bool excludePerimeter = false)
        {
            var onperimeter = this.Edges.Any(edge => edge.EdgeContainsPoint(tol, pt));

            if (onperimeter)
            {
                if (excludePerimeter) return false;
                return true;
            }

            var ray = pt.LineV(Plane.CS.BaseX);

            var qits = this.Edges.Cast<Geometry>()
                .Intersect(tol, new[] { ray }, GeomSegmentMode.FromTo, GeomSegmentMode.Infinite)
                .ToList();

            if (qits.Count == 0) return false;

            var qips = qits.Where(r => r.GeomType == GeometryType.Vector3D).Select(w => (Vector3D)w).ToList();

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


        public BBox3D BBox => this.Edges.Select(w => w.SGeomFrom).BBox();

        public bool Contains(double tol, Loop other, bool excludePerimeter = true)
        {
            foreach (var otherEdge in other.Edges)
            {
                if (!ContainsPoint(tol, otherEdge.SGeomFrom, excludePerimeter)) return false;
            }

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

        public override string ToString()
        {
            return $"Edges:{Edges.Count}";
        }

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

        public static Plane3D DetectPlane(this IEnumerable<IEdge> edges, double tol)
        {
            var lines = new List<Line3D>();

            foreach (var edge in edges)
            {
                switch (edge.EdgeType)
                {
                    case EdgeType.Line3D: lines.Add((Line3D)edge); break;
                    case EdgeType.Arc3D: return new Plane3D(((Arc3D)edge).CS);
                    default:
                        throw new Exception($"unexpected edge type {edge.EdgeType}");
                }
            }

            if (lines.Count == 0) throw new Exception($"can't state edges (cnt:{edges.Count()}) plane");

            return lines.BestFittingPlane(tol);
        }

        /// <summary>
        /// from given set of edges returns the same set eventually toggling sense of edges to make them glue so that SGeomTo of previous equals SGeomFrom of current.
        /// it can raise exception if there isn't availability to glue edges regardless toggling their sense.
        /// first element will toggled to match second one, then other elements will follow the sense matching established.
        /// </summary>        
        public static IEnumerable<IEdge> CheckSense(this IEnumerable<IEdge> edges, double tol)
        {
            IEdge? overrideCur = null;

            var lst = edges.ToList();

            int i = 0;

            foreach (var edgeItem in edges.WithNext())
            {
                if (edgeItem.next == null)
                {
                    if (overrideCur != null)
                        yield return overrideCur;
                    else
                        yield return edgeItem.item;
                }

                else
                {
                    var cur = edgeItem.item;

                    if (overrideCur != null) cur = overrideCur;

                    var q = cur.CheckSense(tol, edgeItem.next);

                    if (q == null || (overrideCur != null && q.Value.needToggleSenseThis))
                        throw new Exception($"can't glue [{cur}] idx:{i} with [{edgeItem.next}]");

                    overrideCur = null;

                    if (q.Value.needToggleSenseThis)
                        yield return (IEdge)cur.ToggleSense();
                    else
                        yield return cur;

                    if (q.Value.needToggleSenseNext)
                        overrideCur = (IEdge)edgeItem.next.ToggleSense();
                }

                ++i;
            }
        }

    }

    public static partial class SciToolkit
    {

    }

}