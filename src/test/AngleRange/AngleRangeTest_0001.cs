namespace SearchAThing.Sci.Tests;

public partial class AngleRangeTests
{

    [Fact]
    public void AngleRangeTest_0001()
    {
        var cmp = new NormalizedAngleComparer(0, 2 * PI);

        Assert.True(cmp.Compare(359d.ToRad(), 359.9d.ToRad()) == -1);
        Assert.True(cmp.Compare(359d.ToRad(), 360d.ToRad()) == -1); // not valid call: angle must normalized

        360d.ToRad().NormalizeAngle().ToDeg().AssertEqualsTol(1e-8, 0);
        Assert.True(cmp.Compare(359d.ToRad(), 360d.ToRad().NormalizeAngle()) == 1);
        Assert.True(cmp.Compare(359d.ToRad(), 361d.ToRad().NormalizeAngle()) == 1);

        Assert.True(80d.ToRad().AngleInRange(0, 2 * PI));
    }

}