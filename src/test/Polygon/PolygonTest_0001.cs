namespace SearchAThing.Sci.Tests;

public partial class PolygonTests
{

    [Fact]
    public void PolygonTest_0001()
    {
        var tol = 1e-1;

        var pts = new[]
        {
                new Vector3D(0,0,0),
                new Vector3D(-1,-1,0),
                new Vector3D(-1,0,0),
            }.ToList();

        var pt = new Vector3D(
            -0.3955330146737879,
            0.03196609720086485,
            1.1102230246251565E-16);

        Assert.True(pts.ContainsPoint(tol, pt));
    }

}