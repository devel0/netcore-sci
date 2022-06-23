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
        EdgeType EdgeType { get; }
        
        /// <summary>
        /// allow to store info about sense; when false SGeomFrom = GeomTo and SGeomTo = GeomFrom
        /// </summary>        
        bool Sense { get; }

        void ToggleSense();

        Vector3D SGeomFrom { get; }

        Vector3D SGeomTo { get; }

        Vector3D MidPoint { get; }

        bool EdgeContainsPoint(double tol, Vector3D pt);

        IEnumerable<Geometry> Split(double tol, IEnumerable<Vector3D> breaks);

        netDxf.Entities.EntityObject DxfEntity { get; }

    }

}