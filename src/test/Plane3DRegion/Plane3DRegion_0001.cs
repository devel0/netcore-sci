namespace SearchAThing.Sci.Tests;

public partial class Plane3DRegionTests
{

    [Fact]
    public void Plane3DRegion_0001()
    {
        var tol = 1e-1;

        var pts = Vector3D.FromStringArray(
                "(135.036276937634, 150.02239286989007, -0.7510061128450616);" +
                "(135.036276937634, 150.10366770710075, -0.7510061128450616);" +
                "(138.4175736255904, 150.10366770710075, -0.7510061128450616);" +
                "(138.4175736255904, 150.02239286989007, -0.7510061128450616)")
                .ToList();

        try
        {
            var plane = new Plane3DRegion(tol, pts);
            Assert.True(false);
        }
        catch
        {
        }

        try
        {
            tol = 1e-3;
            var plane = new Plane3DRegion(tol, pts);
        }
        catch
        {
            Assert.True(false);
        }

        {
            var plane = new Plane3DRegion(tol, pts);
            foreach (var x in plane.CSPoints)
            {
                Assert.True(x.Z.EqualsTol(1e-3, 0));
            }
        }

    }

}