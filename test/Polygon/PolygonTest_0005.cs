using Xunit;
using System.Linq;
using System;
using System.Collections.Generic;
using netDxf;

namespace SearchAThing.Sci.Tests
{
    public partial class PolygonTests
    {

        [Fact]
        public void PolygonTest_0005()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Polygon/PolygonTest_0005.dxf"));

            var tol = 1e-8;

            var poly = dxf.LwPolylines.First().Vertexes.Select(w => new Vector3D(w.Position)).ToList();
            if (poly.Last().EqualsTol(tol, poly.First())) poly.RemoveAt(poly.Count - 1);

            var loop = dxf.LwPolylines.First().ToLoop(tol);
            
            var outsidePts = new List<Vector3D>();

            foreach (var dxfpt in dxf.Points)
            {                
                if (dxfpt.Color.Index == AciColor.Magenta.Index) outsidePts.Add(new Vector3D(dxfpt.Position));
            }
 
            foreach (var point in outsidePts)
            {
                var qpoly = poly.ContainsPoint(tol, point);
                var qloop = loop.ContainsPoint(tol, point, LoopContainsPointMode.InsideExcludedPerimeter);

                Assert.False(qpoly);
                Assert.False(qloop);
            }
            
        }

    }
}