namespace SearchAThing.Sci.Tests;

public partial class Vector3DTests
{

    [Fact]
    public void Vector3DTest_0014()
    {
        var tol = 1e-8;

        var v1 = new Vector3D("X = 64.34945471 Y = 108.89733154 Z = -34.78875667");
        var v2 = new Vector3D("X = 260.87876324 Y = 240.36185984 Z = 107.99814397");

        Assert.True(v1.Distance(v2).EqualsTol(tol, 276.215116));
    }
}