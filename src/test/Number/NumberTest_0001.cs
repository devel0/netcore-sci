namespace SearchAThing.Sci.Tests;

public partial class NumberTests
{

    [Fact]
    public void NumberTest_0001()
    {
        var v = 13.45;

        // use Round(13.45, 3) to get precise 13.45
        Assert.False((v.MRound(1e-3) - 13.45) < double.Epsilon);
    }

}