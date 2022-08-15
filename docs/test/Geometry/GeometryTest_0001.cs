using Xunit;
using System.Linq;
using System;

using netDxf;
using netDxf.Entities;
using System.Collections.Generic;

namespace SearchAThing.Sci.Tests
{
    public partial class GeometryTests
    {

        [Fact]
        public void GeometryTest_0001()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Geometry/GeometryTest_0001.dxf"));
            netDxf.DxfDocument? outdxf = null;
            // outdxf = new netDxf.DxfDocument();

            var tol = 1e-8;

            var lw = dxf.Entities.Polylines2D.First(w => w.Color.Index == AciColor.Red.Index);

            var lw1geoms = lw.ToGeometries(tol).ToList();

            var origlw = (Polyline2D)lw.Clone();
            origlw.SetColor(AciColor.Red);
            if (outdxf != null) outdxf.AddEntity(origlw);

            var greenCs = lw1geoms.SelectMany(w => w.Vertexes).BestFittingPlane(tol).CS;
            var relw = lw1geoms.ToLwPolyline(tol, greenCs, closed: lw.IsClosed);
            if (outdxf != null) outdxf.AddEntity(relw.Act(ent => ent.SetColor(netDxf.AciColor.Green)));

            var lw2geoms = lw.ToGeometries(tol).ToList();

            Assert.True(lw1geoms.Count == lw2geoms.Count);

            for (int i = 0; i < lw1geoms.Count; ++i)
            {
                Assert.True(lw1geoms[i].GeomEquals(tol, lw2geoms[i]));
            }

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }

    }
}