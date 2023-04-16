namespace SearchAThing.Sci.Tests;

public partial class Vector3DTests
{

    [Fact]
    public void Vector3DTest_0013()
    {
        var tol = 1e-2;

        var vx = -1.1;
        var vy = 2.7;
        var vz = 0.09;
        var v = new Vector3D(vx, vy, vz);

        var l = v.Length;
        Assert.True(v.Normalized().EqualsTol(tol, vx / l, vy / l, vz / l));
    }
}