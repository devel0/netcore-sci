namespace SearchAThing.Sci.Tests;

public partial class PolygonTests
{

    [Fact]
    public void PolygonTest_0009()
    {
        var tol = 1e-1;

        {
            var pts = new List<Vector3D>()
                {
                    new Vector3D(57.37453942, -83.12137046),
                    new Vector3D(-35.81509359, -94.43664051),
                    new Vector3D(12.17420471, -100.26359628),
                };

            var sorted_pts = pts.SortCCW(tol, CoordinateSystem3D.WCS)
                .RouteFirst(pts[1])
                .ToList();

            sorted_pts[0].AssertEqualsTol(tol, pts[1]);
            sorted_pts[1].AssertEqualsTol(tol, pts[2]);
            sorted_pts[2].AssertEqualsTol(tol, pts[0]);
        }

        {
            var pts = new List<Vector3D>()
                {
                    new Vector3D(89.43105859, 46.93704038),
                    new Vector3D(89.43105859, -46.93704038),
                    new Vector3D(101.00000000, 0)
                };

            var sorted_pts = pts.SortCCW(tol, CoordinateSystem3D.WCS)
                .RouteFirst(pts[1])
                .ToList();

            sorted_pts[0].AssertEqualsTol(tol, pts[1]);
            sorted_pts[1].AssertEqualsTol(tol, pts[2]);
            sorted_pts[2].AssertEqualsTol(tol, pts[0]);
        }

        {
            var pts = new List<Vector3D>()
                {
                    new Vector3D(101.00000000, 0),
                    new Vector3D(89.43105859, 46.93704038),
                    new Vector3D(89.43105859, -46.93704038),
                };

            var sorted_pts = pts.SortCCW(tol, CoordinateSystem3D.WCS)
                .RouteFirst(pts[2])
                .ToList();

            sorted_pts[0].AssertEqualsTol(tol, pts[2]);
            sorted_pts[1].AssertEqualsTol(tol, pts[0]);
            sorted_pts[2].AssertEqualsTol(tol, pts[1]);
        }
    }

}