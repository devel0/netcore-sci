namespace SearchAThing.Sci.Tests;

public partial class Plane3DTests
{

    [Fact]
    public void Plane3D_0001()
    {
        var dxf = netDxf.DxfDocument.Load(
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plane3D/Plane3DTest_0001.dxf"));

        var pts = dxf.Entities.Points.Select(w => (Vector3D)w.Position).ToList();
        var plane = pts.BestFittingPlane(1e-3);

        var o = plane.CS.Origin;
        var vx = plane.CS.BaseX;
        var vy = plane.CS.BaseY;
        var vz = plane.CS.BaseZ;

        Assert.True(o.EqualsTol(1e-12, 27.2846772030272, 25.6050015783174, -7.74146524250876));
        Assert.True(vx.EqualsTol(1e-12, 0.9091096596546905, 0.060799740859053285, -0.4120958847574249));
        Assert.True(vy.EqualsTol(1e-12, -0.01729356343997878, -0.9829291438880156, -0.18316995048074994));
        Assert.True(vz.EqualsTol(1e-12, -0.4161977407267852, 0.17364817766692778, -0.8925389352890298));

        foreach (var p in pts) Assert.True(plane.Contains(1e-12, p));

    }

}
