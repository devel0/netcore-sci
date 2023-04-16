namespace SearchAThing.Sci.Tests;

public partial class Arc3DTests
{

    [Fact]
    public void Arc3DTest_0004()
    {
        var tol = 0.1;

        var arc = JsonConvert.DeserializeObject<Arc3D>(File.ReadAllText("Arc3D/Arc3DTest_0004.json"), SciJsonSettings)
            .Act(w => Assert.NotNull(w))!;

        var res = arc.Split(tol, new[] { new Vector3D(15, 12.719999999999814, -2) }).ToList();

        Assert.True(res.Count == 1);
    }

}