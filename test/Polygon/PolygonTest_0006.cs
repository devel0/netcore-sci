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
        public void PolygonTest_0006()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Polygon/PolygonTest_0006.dxf"));

            var tol = 1e-8;

            var poly = dxf.LwPolylines.First().Vertexes.Select(w => new Vector3D(w.Position)).ToList();
            if (poly.Last().EqualsTol(tol, poly.First())) poly.RemoveAt(poly.Count - 1);

            var loop = dxf.LwPolylines.First().ToLoop(tol);
            
            loop.Area.AssertEqualsTol(tol, 6.74067686);
            
        }

    }
}