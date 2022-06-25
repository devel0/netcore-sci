using Xunit;
using System.Linq;
using System;
using netDxf;

namespace SearchAThing.Sci.Tests
{
    public partial class PolygonTests
    {

        [Fact]
        public void Line3DTests()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Line3D/Line3DTest_0003.dxf"));

            var tol = 1e-8;

            var magentaLines = dxf.Lines.Where(r => r.Color.Index == AciColor.Magenta.Index).Select(w => w.ToLine3D()).ToList();
            var greenLines = dxf.Lines.Where(r => r.Color.Index == AciColor.Green.Index).Select(w => w.ToLine3D()).ToList();
            var cyanLines = dxf.Lines.Where(r => r.Color.Index == AciColor.Cyan.Index).Select(w => w.ToLine3D()).ToList();

            var q = magentaLines[0].GeomIntersect(tol, magentaLines[1]).ToList();
            Assert.True(q.Count == 1);
            q[0].GeomFrom.AssertEqualsTol(tol, magentaLines[0].From);
            q[0].GeomTo.AssertEqualsTol(tol, magentaLines[0].To);

            q = greenLines[0].GeomIntersect(tol, greenLines[1]).ToList();
            Assert.True(q.Count == 0);

            q = cyanLines[0].GeomIntersect(tol, cyanLines[1]).ToList();
            Assert.True(q.Count == 1);
            q[0].GeomFrom.AssertEqualsTol(tol, cyanLines[0].From);
            q[0].GeomTo.AssertEqualsTol(tol, cyanLines.OrderBy(w => w.Length).First().To);
        }

    }
}