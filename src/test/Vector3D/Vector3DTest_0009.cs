namespace SearchAThing.Sci.Tests;

public partial class Vector3DTests
{

    [Fact]
    public void Vector3DTest_0009()
    {
        Assert.True(new Vector3D("X = -1736.46604788 Y = -5748.83626248 Z = -1847.86449554")
          .EqualsTol(1e-8, -1736.46604788, -5748.83626248, -1847.86449554));
        Assert.True(new Vector3D("X = -1736.46604788  Y = -5748.83626248  Z = -1847.86449554")
          .EqualsTol(1e-8, -1736.46604788, -5748.83626248, -1847.86449554));
        Assert.Throws<ArgumentException>(new Action(() =>
          new Vector3D("X = -1736.46604788 ; Y = -5748.83626248 ; Z = -1847.86449554")));
    }
}