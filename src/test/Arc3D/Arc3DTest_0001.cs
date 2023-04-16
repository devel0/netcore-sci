namespace SearchAThing.Sci.Tests;

public partial class Arc3DTests
{

    [Fact]
    public void Arc3DTest_0001()
    {
        var tol = 1e-7;

        var p1 = new Vector3D("X = 13.87329226 Y = 134.93466652 Z = -3.70729037");
        var p2 = new Vector3D("X = 75.89230418 Y = 224.11406806 Z = 35.97437873");
        var p3 = new Vector3D("X = 97.48181688 Y = 229.31008314 Z = 16.9314998");

        var arc = new Arc3D(tol, p1, p2, p3);

        var plo = new Vector3D("X = -74.96786784 Y = 178.50832685 Z = -43.45380285");
        var plx = new Vector3D("X = 61.65932598 Y = 313.82398026 Z = -33.75931542");
        var ply = new Vector3D("X = 87.35160811 Y = 8.94741958 Z = 35.66234059");

        var plcs = new CoordinateSystem3D(plo, plx - plo, ply - plo);

        var iSegmentPts = arc.Intersect(tol, plcs, onlyPerimeter: false, inArcAngleRange: true).ToList();
        Assert.True(iSegmentPts.Count == 2);
        var i1 = iSegmentPts[0];
        var i2 = iSegmentPts[1];
        var iSegment = new Line3D(i1, i2);

        Assert.True(plcs.Contains(tol, i1));
        Assert.True(plcs.Contains(tol, i2));

        i1.AssertEqualsTol(tol, new Vector3D("X = 40.09735573 Y = 156.48945821 Z = -7.46179105"));
        i2.AssertEqualsTol(tol, new Vector3D("X = 72.20796391 Y = 188.29182351 Z = -5.18335819"));

        Assert.True(arc.Contains(tol, i1, onlyPerimeter: false));
        Assert.True(arc.Contains(tol, i2, onlyPerimeter: false));
    }
}