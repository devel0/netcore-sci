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

    public class Loop
    {

        public List<IEdge> Edges { get; private set; }

        public Loop(IEnumerable<IEdge> edges)
        {
            Edges = edges.ToList();
        }

        public Loop(double tol, LwPolyline lwPolyline)
        {
            Edges = lwPolyline.ToGeometries(tol).Cast<IEdge>().ToList();
        }

        public List<List<Geometry>> Intersect(double tol, Loop other)
        {
            var res = new List<List<Geometry>>();

            var thisGeoms = Edges.Cast<Geometry>().ToList();
            var otherGeoms = other.Edges.Cast<Geometry>().ToList();

            var combs = from tg in thisGeoms
                        from og in otherGeoms
                        select new { tg, og };

            var intersections = combs.SelectMany(x => x.tg.Intersect(tol, x.og));

            return res;
        }

    }

    public static partial class SciExt
    {

    }

    public static partial class SciToolkit
    {

    }

}