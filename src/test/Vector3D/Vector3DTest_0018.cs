namespace SearchAThing.Sci.Tests;

public partial class Vector3DTests
{

    [Fact]
    public void Vector3DTest_0018()
    {
        Assert.True(Vector3D.XAxis.IsPerpendicular(Vector3D.YAxis));
        Assert.True(Vector3D.XAxis.IsPerpendicular(Vector3D.ZAxis));

        var v1 = new Vector3D(2.5101, 1.7754, -2.1324);
        var v2 = new Vector3D(-9.7136, 8.0369, -4.7428);
        var v3 = new Vector3D("X = 2.26538393 Y = 1.96321486 Z = -2.23910697");

        Assert.True(v1.IsPerpendicular(v2)); // 90deg
        Assert.False(v2.IsPerpendicular(v3)); // 85deg
    }
}