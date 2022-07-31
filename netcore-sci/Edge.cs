using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text;

namespace SearchAThing
{

    /// <summary>
    /// interface implemented by some type of geometries used in Loop such as Line3D, Arc3D and Circle3D
    /// </summary>
    public abstract class Edge : Geometry
    {

        protected Edge(GeometryType type) : base(type)
        {
        }

        protected void CopyFrom(Edge other)
        {
            Sense = other.Sense;
        }

        /// <summary>
        /// allow to store info about sense; when false SGeomFrom = GeomTo and SGeomTo = GeomFrom
        /// </summary>        
        public bool Sense { get; protected set; } = true;

        /// <summary>
        /// GeomFrom (Sense:true) or GeomTo (Sense:false)
        /// </summary>
        public Vector3D SGeomFrom => Sense ? GeomFrom : GeomTo;

        /// <summary>
        /// GeomTo (Sense:true) or GeomFrom (Sense:false)
        /// </summary>
        /// <value></value>
        public Vector3D SGeomTo => Sense ? GeomTo : GeomFrom;

        public Vector3D OtherEndpoint(double tol, Vector3D endpoint) =>
            endpoint.EqualsTol(tol, GeomFrom) ? GeomTo : GeomFrom;

        public bool EndpointMatches(double tol, Vector3D endpoint1, Vector3D endpoint2) =>
            (GeomFrom.EqualsTol(tol, endpoint1) && GeomTo.EqualsTol(tol, endpoint2))
            ||
            (GeomTo.EqualsTol(tol, endpoint1) && GeomFrom.EqualsTol(tol, endpoint2));

        public bool EndpointMatches(double tol, Vector3D endpoint1) =>
            GeomFrom.EqualsTol(tol, endpoint1) || GeomTo.EqualsTol(tol, endpoint1);

        /// <summary>        
        /// Toggle Sense flag so that SGeomFrom, SGeomTo equals to GeomFrom, GeomTo (Sense:true)
        /// or GeomTo, GeomFrom (Sense:false)
        /// </summary>
        public Geometry ToggleSense()
        {
            var toggled = (Edge)Copy();
            toggled.Sense = !toggled.Sense;
            return toggled;
        }

        /// <summary>
        /// states if this and/or next given edge need to be toggled in their sense to allow glueing.
        /// precedence is given to toggling sense of the next one.
        /// returns null if no solution.
        /// </summary>        
        public (bool needToggleSenseThis, bool needToggleSenseNext)?
        CheckSense(double tol, Edge nextEdge)
        {
            if (SGeomTo.EqualsTol(tol, nextEdge.SGeomFrom))
                return (false, false);

            if (SGeomTo.EqualsTol(tol, nextEdge.SGeomTo))
                return (false, true);

            if (SGeomFrom.EqualsTol(tol, nextEdge.SGeomFrom))
                return (true, false);

            if (SGeomFrom.EqualsTol(tol, nextEdge.SGeomTo))
                return (true, true);

            return null;
        }

        /// <summary>
        /// States if edge contains given point on its perimeter
        /// </summary>        
        public abstract bool EdgeContainsPoint(double tol, Vector3D pt);

        /// <summary>
        /// project this edge to given plane
        /// </summary>
        /// <param name="tol"></param>
        /// <param name="prjPlane"></param>
        /// <returns></returns>
        public abstract Edge Project(double tol, Plane3D prjPlane);

        public abstract string ToString(int digits);

        /// <summary>
        /// qcad 2d script
        /// </summary>
        /// <param name="final">adds QQ command</param>        
        public abstract string QCadScript(bool final = true);

        public abstract string ProgeCadScript(bool final = true);

    }

    public static partial class SciExt
    {

        /// <summary>
        /// qcad script from edge enumerable
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="final">if true adds QQ</param>        
        public static string QCadScript(this IEnumerable<Edge> edges, bool final = true)
        {
            var sb = new StringBuilder();

            foreach (var edge in edges)
            {
                sb.Append(edge.QCadScript(final: false));
            }

            if (final) sb.AppendLine("QQ");

            return sb.ToString();
        }

        /// <summary>
        /// Allow to sort edges when a not consequential set is given to allow subsequent CheckSense.
        /// It can raise exception if there isn't a possible sequence continuity between given edges.
        /// </summary>
        /// <param name="edges">input edges list</param>
        /// <param name="tol">length tolerance</param>
        /// <returns>edges in sequence ( sense not yet checked )</returns>
        public static IEnumerable<Edge> CheckSort(this IEnumerable<Edge> edges, double tol)
        {
            var vertexToEdges = new Dictionary<Vector3D, List<Edge>>(new Vector3DEqualityComparer(tol));
            void scanEdge(Edge edge, Vector3D vertex)
            {
                List<Edge>? vertexEdges;
                if (!vertexToEdges.TryGetValue(vertex, out vertexEdges))
                {
                    vertexEdges = new List<Edge>();
                    vertexToEdges.Add(vertex, vertexEdges);
                }
                vertexEdges.Add(edge);
            }
            foreach (var edge in edges)
            {
                scanEdge(edge, edge.SGeomFrom);
                scanEdge(edge, edge.SGeomTo);
            }

            foreach (var x in vertexToEdges)
            {
                if (x.Value.Count != 2) throw new Exception($"can't find two edges belonging to vertex [{x.Key}]");
            }

            var start = vertexToEdges.First();
            var startVertex = start.Key;
            var startEdge = start.Value.First();

            yield return startEdge;

            var lastVertex = startVertex;
            var lastEdge = startEdge;

            while (true)
            {
                var nextVertex = lastEdge.OtherEndpoint(tol, lastVertex);
                if (nextVertex.EqualsTol(tol, startVertex)) break;

                lastEdge = vertexToEdges[nextVertex].First(edge => edge != lastEdge);
                yield return lastEdge;

                lastVertex = nextVertex;
            }
        }

        /// <summary>
        /// From given set of edges returns the same set eventually toggling sense of edges to make them glue so that
        /// SGeomTo of previous equals SGeomFrom of current.
        /// It can raise exception if there isn't availability to glue edges regardless toggling their sense.
        /// first element will toggled to match second one, then other elements will follow the sense matching established.        
        /// </summary>
        /// <param name="edges">input edges list</param>
        /// <param name="tol">length tolerance</param>        
        public static IEnumerable<Edge> CheckSense(this IEnumerable<Edge> edges, double tol)
        {
            Edge? overrideCur = null;

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
                        yield return (Edge)cur.ToggleSense();
                    else
                        yield return cur;

                    if (q.Value.needToggleSenseNext)
                        overrideCur = (Edge)edgeItem.next.ToggleSense();
                }

                ++i;
            }
        }

        public static Dictionary<Vector3D, List<Edge>> MakeVertexToEdges(this IEnumerable<Edge> edges, double tol)
        {
            var res = new Dictionary<Vector3D, List<Edge>>(new Vector3DEqualityComparer(tol));

            void updateVertexToEdgeNfos(Vector3D vertex, Edge edge)
            {
                if (!res.TryGetValue(vertex, out var lst))
                {
                    lst = new List<Edge>();
                    res.Add(vertex, lst);
                }

                if (!lst.Any(lstedge => lstedge.GeomEquals(tol, edge)))
                    lst.Add(edge);
            }

            foreach (var edge in edges)
            {
                updateVertexToEdgeNfos(edge.SGeomFrom, edge);
                updateVertexToEdgeNfos(edge.SGeomTo, edge);
            }

            return res;
        }

    }

    public static partial class SciToolkit
    {

        public delegate bool WalkEdgeStopConditionDelegate(Vector3D startVertex, Vector3D nextVertex);

        /// <summary>
        /// walk edges
        /// </summary>
        /// <param name="tol">length tolerance</param>
        /// <param name="startEdge">starting edge</param>
        /// <param name="startVertex">starting vertex</param>
        /// <param name="vertexToEdges">vertex to edges dictionary</param>
        /// <param name="stopCondition">by default stop when meet startvertex again</param>
        /// <returns>walked edges</returns>
        public static IEnumerable<Edge> WalkEdges(
            double tol,
            Edge startEdge,
            Vector3D startVertex,
            Dictionary<Vector3D, List<Edge>> vertexToEdges,
            WalkEdgeStopConditionDelegate? stopCondition = null)
        {
            var lastEdge = startEdge;
            var lastVertex = startVertex;

            yield return lastEdge;

            if (stopCondition == null)
            {
                stopCondition = (_startVertex, _nextVertex) => _startVertex.EqualsTol(tol, _nextVertex);
            }

            while (true)
            {
                var prevVertex = lastVertex;
                var nextVertex = lastEdge.OtherEndpoint(tol, lastVertex);
                if (stopCondition(startVertex, nextVertex)) break;

                lastEdge = vertexToEdges[nextVertex].First(edge => !edge.EndpointMatches(tol, prevVertex));

                yield return lastEdge;
                lastVertex = nextVertex;
            }

        }

    }

}