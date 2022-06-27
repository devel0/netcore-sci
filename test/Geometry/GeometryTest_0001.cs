using Xunit;
using System.Linq;
using System;

using netDxf;
using netDxf.Entities;

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
            //outdxf = new netDxf.DxfDocument();

            var tol = 1e-8;

            var lw = dxf.LwPolylines.First();

            var geoms = lw.ToGeometries(tol);

            var origlw = (LwPolyline)lw.Clone();
            origlw.SetColor(AciColor.Blue);
            if (outdxf != null) outdxf.AddEntity(origlw);

            var relw = geoms.ToLwPolyline(tol, geoms.SelectMany(w => w.Vertexes).BestFittingPlane(tol).CS);
            if (outdxf != null) outdxf.AddEntity(relw.Set(ent => ent.SetColor(netDxf.AciColor.Red)));

            Assert.True(lw.Vertexes.Count == relw.Vertexes.Count);
            for (int i = 0; i < lw.Vertexes.Count; ++i)
            {
                ((Vector3D)lw.Vertexes[i].Position)
                    .AssertEqualsTol(tol, relw.Vertexes[i].Position);
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