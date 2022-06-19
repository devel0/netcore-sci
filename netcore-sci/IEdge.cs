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

        Vector3D GeomFrom { get; }

        Vector3D GeomTo { get; }

    }

}