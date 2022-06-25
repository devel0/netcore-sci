using Xunit;
using System.Linq;
using System;

namespace SearchAThing.Sci.Tests
{
    public partial class PolygonTests
    {

        [Fact]
        public void PolygonTest_0003()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Polygon/PolygonTest_0003.dxf"));

            var tol = 1e-12;

            var poly = dxf.LwPolylines.First().Points().ToList();

            Assert.True(poly.ContainsPoint(tol, new Vector3D(30, 26, 0)));
            Assert.False(poly.ContainsPoint(tol, new Vector3D(57, 21, 0)));

            Assert.True(poly.ContainsPoint(tol, new Vector3D(39.72, 30.77380445)));
            Assert.False(poly.ContainsPoint(tol, new Vector3D(39.72 + 1e-8, 30.77380445)));

            Assert.True(poly.ContainsPoint(tol, new Vector3D(39.64867363, 16.35092007)));
            Assert.False(poly.ContainsPoint(tol, new Vector3D(39.64867363, 16.35092007 - 1e-8)));
        }

    }
}