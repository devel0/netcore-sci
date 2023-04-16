namespace SearchAThing.Sci.Tests;

public partial class Line3DTests
{

    [Fact]
    public void Line3DTest_0002()
    {
        var tol = 1e-4;

        var v1 = new Vector3D("X = 7.17678145 Y = 9.61488416 Z = 3.26614373");
        var v2 = new Vector3D("X = 1.03262618 Y = 4.33930275 Z = 4.34962008");

        var l = new Line3D(v1, v2);
        var sl = l.Scale(5624.89);

        Assert.True(sl.From.EqualsTol(tol, 17284.2035, 14844.2597, -3043.4098));
        Assert.True(sl.To.EqualsTol(tol, -17275.9941, -14830.3055, 3051.0255));
    }
}