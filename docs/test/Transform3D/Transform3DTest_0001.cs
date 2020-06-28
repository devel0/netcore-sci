using Xunit;
using System.Linq;
using static System.Math;
using System;
using SearchAThing.Util;

namespace SearchAThing.Sci.Tests
{
    public partial class Transform3DTests
    {

        [Fact]
        public void Transform3DTest_0001()
        {
            var tol = 1e-8;

            // red
            var v1 = new Vector3D("X = -91.65821493 Y = 164.31850795 Z = 69.55900266");

            // identity ( red )
            var t = new Transform3D();
            Assert.True(t.Apply(v1).EqualsTol(tol, v1));

            // add rotate about zaxis ( green ) and compute from original v1
            t.RotateAboutZAxis(12.1.ToRad());
            var v2 = t.Apply(v1);
            Assert.True(v2.EqualsTol(tol, new Vector3D("X = -124.06607557 Y = 141.45461927 Z = 69.55900266")));

            // add rotate about xaxis ( cyan ) and compute from original v1
            t.RotateAboutXAxis(30.111.ToRad());
            var v3 = t.Apply(v1);
            Assert.True(v3.EqualsTol(tol, new Vector3D("X = -124.06607557 Y = 87.46990325 Z = 131.13687578")));

            // add rotate about yaxis ( magenta ) and compute from original v1
            t.RotateAboutYAxis(-40.12.ToRad());
            var v4 = t.Apply(v1);
            Assert.True(v4.EqualsTol(tol, new Vector3D("X = -179.3762652 Y = 87.46990325 Z = 20.33289892")));

            // add rotate about arbitrary (1,2,3) axis (blue) and compute from original v1
            var myaxis = new Vector3D(10, 20, 30);
            t.RotateAboutAxis(myaxis, 55.3.ToRad());
            var v5 = t.Apply(v1);
            Assert.True(v5.EqualsTol(tol, new Vector3D("X = -149.09823237 Y = -69.43406135 Z = 114.84286438")));
        }
    }
}