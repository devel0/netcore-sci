namespace SearchAThing.Sci.Tests;

public partial class Arc3DTests
{

    [Fact]
    public void Circle3DTest_0002()
    {
        // create circle radius 5 at (0,0,0) origin ( xy plane )
        var c1 = new Circle3D(CoordinateSystem3D.WCS, 5);

        var c2 = c1.Move(new Vector3D(4, 3, 2));

        Assert.True(c2.GetType() == typeof(Circle3D));
    }

}