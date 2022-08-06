using Xunit;
using System.Linq;
using System;
using static System.Math;
using netDxf;

namespace SearchAThing.Sci.Tests
{
    public partial class Arc3DTests
    {

        [Fact]
        public void Arc3DTest_0006()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Arc3D/Arc3DTest_0006.dxf"));

            var tol = 1e-8;

            var arcYellow = dxf.Entities.Arcs.First(w => w.Color.Index == AciColor.Yellow.Index).ToArc3D();
            var arcGreen = dxf.Entities.Arcs.First(w => w.Color.Index == AciColor.Green.Index).ToArc3D();
            var arcCyan = dxf.Entities.Arcs.First(w => w.Color.Index == AciColor.Cyan.Index).ToArc3D();

            var qYellowGreen = arcYellow.Intersect(tol, arcGreen).ToList();
            var qGreenCyan = arcGreen.Intersect(tol, arcCyan).ToList();
            var qYellowCyan = arcYellow.Intersect(tol, arcCyan).ToList();

            Assert.True(qYellowGreen.Count == 1);
            Assert.True(qGreenCyan.Count == 2);
            Assert.True(qYellowCyan.Count == 0);

            qYellowGreen[0].AssertEqualsTol(tol, 3.73937215, 16.89896682, 0);

            qGreenCyan[0].AssertEqualsTol(tol, 7.88706002, 17.31056877, 0);
            qGreenCyan[1].AssertEqualsTol(tol, 5.59847914, 17.38831075, 0);
        }
    }

}