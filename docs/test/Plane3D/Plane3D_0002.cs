using Xunit;
using System.Linq;
using System;

namespace SearchAThing.Sci.Tests
{
    public partial class Plane3DTests
    {

        [Fact]
        public void Plane3D_0002()
        {
            var tol = 1e-8;

            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plane3D/Plane3DTest_0002.dxf"));

            var pts = dxf.Points.Select(w => (Vector3D)w.Position).ToList();
            var plane = pts.BestFittingPlane(1e-3);

            var o = plane.CS.Origin;
            var vx = plane.CS.BaseX;
            var vy = plane.CS.BaseY;
            var vz = plane.CS.BaseZ;

            foreach (var pt in pts)
            {
                var cpt = pt.ToUCS(plane.CS);
                cpt.Z.AssertEqualsTol(tol, 0);
            }

            Assert.True(plane.CS.Equals(tol, CoordinateSystem3D.WCS.Move(o)));
        }

    }

}