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
        public void PolygonTest_0008()
        {
            DxfDocument? outdxf = null;
            // outdxf = new DxfDocument();

            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Polygon/PolygonTest_0007.dxf"));

            var tol = 1e-8;

            var poly = dxf.Entities.Polylines2D.First(w => w.Color.Index == AciColor.Cyan.Index);
            var poly_exploded = poly.ToGeometries(tol);

            var arcs = poly_exploded.OfType<Arc3D>().ToList();

            Assert.False(arcs[0].Sense); // reversed
            Assert.False(arcs[1].Sense); // reversed
            Assert.False(arcs[2].Sense); // reversed
            Assert.True(arcs[3].Sense); // not reversed

            outdxf?.AddEntities(poly_exploded.Select(w => w.DxfEntity));

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }

    }
}