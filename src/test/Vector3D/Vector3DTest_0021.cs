namespace SearchAThing.Sci.Tests;

public partial class Vector3DTests
{

    [Fact]
    public void Vector3DTest_0021()
    {
        var tol = 1e-8;

        var v1 = new Vector3D("X = 7.17678145 Y = 9.61488416 Z = 3.26614373");
        var v2 = new Vector3D("X = 1.03262618 Y = 4.33930275 Z = 4.34962008");

        var p = v2.Project(v1);
        Assert.True(p.EqualsTol(tol, new Vector3D("X = 2.93993485 Y = 3.93869218 Z = 1.33796046")));
    }
}