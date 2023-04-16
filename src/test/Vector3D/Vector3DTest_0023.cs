namespace SearchAThing.Sci.Tests;

public partial class Vector3DTests
{

    [Fact]
    public void Vector3DTest_0023()
    {
        var tol = 1e-8;

        var coords = Vector3D.FromTxtPointsList(@"
-53.54533794,-141.18745265
18.20103872,-149.89903999
85.77777676,-124.27056375

133.70385987,-70.17319898,1
151.00000000,0.00000000,2
133.70385987,   70.17319898,3
").ToList();

        Assert.True(coords.Count == 6);

        coords[0].EqualsTol(tol, -53.54533794, -141.18745265, 0);
        coords[1].EqualsTol(tol, 18.20103872, -149.89903999, 0);
        coords[2].EqualsTol(tol, 85.77777676, -124.27056375, 0);

        coords[3].EqualsTol(tol, 133.70385987, -70.17319898, 1);
        coords[4].EqualsTol(tol, 151.00000000, 0.00000000, 2);
        coords[5].EqualsTol(tol, 133.70385987, 70.17319898, 3);



    }
}