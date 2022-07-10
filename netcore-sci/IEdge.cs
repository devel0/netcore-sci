using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchAThing
{

    public enum EdgeType
    {
        Line3D,
        Arc3D
    }

    /// <summary>
    /// interface implemented by some type of geometries used in Loop such as Line3D, Arc3D and Circle3D
    /// </summary>
    public interface IEdge
    {

        /// <summary>
        /// type of edge
        /// </summary>                
        EdgeType EdgeType { get; }

        /// <summary>
        /// allow to store info about sense; when false SGeomFrom = GeomTo and SGeomTo = GeomFrom
        /// </summary>        
        bool Sense { get; }

        /// <summary>        
        /// Toggle Sense flag so that SGeomFrom, SGeomTo equals to GeomFrom, GeomTo (Sense:true)
        /// or GeomTo, GeomFrom (Sense:false)
        /// </summary>
        Geometry ToggleSense();

        /// <summary>
        /// GeomFrom (Sense:true) or GeomTo (Sense:false)
        /// </summary>
        Vector3D SGeomFrom { get; }

        /// <summary>
        /// GeomTo (Sense:true) or GeomFrom (Sense:false)
        /// </summary>
        /// <value></value>
        Vector3D SGeomTo { get; }

        Vector3D MidPoint { get; }

        /// <summary>
        /// States if edge contains given point on its perimeter
        /// </summary>        
        bool EdgeContainsPoint(double tol, Vector3D pt);

        /// <summary>
        /// Split this edge into one or more geometries based on given breaks.        
        /// </summary>        
        IEnumerable<Geometry> Split(double tol, IEnumerable<Vector3D> breaks);

        /// <summary>
        /// translate this edge of given amount
        /// </summary>
        IEdge EdgeMove(Vector3D delta);

        /// <summary>
        /// Retrieve corresponding dxf entity to this edge
        /// </summary>                
        netDxf.Entities.EntityObject DxfEntity { get; }

        /// <summary>
        /// states if this edge equals other
        /// </summary>        
        /// <param name="includeSense">if true then two geometrical equals edges but with different sense cause they considered to be different</param>        
        bool Equals(double tol, IEdge other, bool includeSense = false);

        /// <summary>
        /// length of the edge
        /// </summary>        
        double Length { get; }

        IEnumerable<Vector3D> Vertexes { get; }

        string ToString(int digits);

        /// <summary>
        /// qcad 2d script
        /// </summary>
        /// <param name="final">adds QQ command</param>        
        string QCadScript(bool final = true);

    }

    public static partial class SciExt
    {

        /// <summary>
        /// states if this and/or next given edge need to be toggled in their sense to allow glueing.
        /// precedence is given to toggling sense of the next one.
        /// returns null if no solution.
        /// </summary>        
        public static (bool needToggleSenseThis, bool needToggleSenseNext)?
        CheckSense(this IEdge thisEdge, double tol, IEdge nextEdge)
        {
            if (thisEdge.SGeomTo.EqualsTol(tol, nextEdge.SGeomFrom))
                return (false, false);

            if (thisEdge.SGeomTo.EqualsTol(tol, nextEdge.SGeomTo))
                return (false, true);

            if (thisEdge.SGeomFrom.EqualsTol(tol, nextEdge.SGeomFrom))
                return (true, false);

            if (thisEdge.SGeomFrom.EqualsTol(tol, nextEdge.SGeomTo))
                return (true, true);

            return null;
        }

    }

}