using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using netDxf;
using ClipperLib;
using netDxf.Entities;

namespace SearchAThing
{

    public enum EdgeType
    {
        Line3D,
        Circle3D,
        Arc3D
    }

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
        /// (side effect)
        /// Toggle Sense flag so that SGeomFrom, SGeomTo equals to GeomFrom, GeomTo (Sense:true)
        /// or GeomTo, GeomFrom (Sense:false)
        /// </summary>
        void ToggleSense();

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
        /// Retrieve corresponding dxf entity to this edge
        /// </summary>        
        netDxf.Entities.EntityObject DxfEntity { get; }

    }

}