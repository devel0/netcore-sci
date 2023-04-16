namespace SearchAThing.Sci.Tests;

public partial class LoopTests
{

    [Fact]
    public void LoopTest_0001()
    {
        var dxf = netDxf.DxfDocument.Load(
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0001.dxf"));

        var tol = 1e-8;

        var loop = dxf.Entities.Polylines2D.First().ToLoop(tol);

        Assert.True(loop.ContainsPoint(tol, new Vector3D(39.72, 30.77380445, 0), LoopContainsPointMode.InsideOrPerimeter));

        Assert.True(loop.ContainsPoint(tol, new Vector3D(31.3, 26.9, 0), LoopContainsPointMode.InsideOrPerimeter));
        Assert.False(loop.ContainsPoint(tol, new Vector3D(49.6, 26.1, 0), LoopContainsPointMode.InsideOrPerimeter));

        Assert.True(loop.ContainsPoint(tol, new Vector3D(40.7769, 28.1902, 0), LoopContainsPointMode.InsideOrPerimeter));
        Assert.False(loop.ContainsPoint(tol, new Vector3D(41.2243, 30.8036, 0), LoopContainsPointMode.InsideOrPerimeter));
    }

}