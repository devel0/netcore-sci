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
        public void PolygonTest_0009()
        {
            var tol = 1e-1;

            var pts = new List<Vector3D>()
            {
                new Vector3D(57.37453942, -83.12137046),
                new Vector3D(-35.81509359, -94.43664051),
                new Vector3D(12.17420471, -100.26359628),
            };

            var sorted_pts = pts.SortCCW(tol, CoordinateSystem3D.WCS.Move(pts.Mean()))
                .RouteFirst(pts[1])
                .ToList();

            sorted_pts[0].AssertEqualsTol(tol, pts[1]);
            sorted_pts[1].AssertEqualsTol(tol, pts[2]);
            sorted_pts[2].AssertEqualsTol(tol, pts[0]);
        }

    }
}