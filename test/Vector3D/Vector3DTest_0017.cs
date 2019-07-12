using Xunit;
using System.Linq;
using static System.Math;
using System;
using SearchAThing.Util;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {

        [Fact]
        public void Vector3DTest_0017()
        {
            var tol = 1e-8;

            var v1 = new Vector3D("X = 220.22063137 Y = 217.58532235 Z = 30.72201149");
            var v2 = new Vector3D("X = 41.2984487 Y = 335.41147265 Z = 65.72177141");
            var v1v2angle = v1.AngleRad(tol, v2);

            var dotp = v1.DotProduct(v2);

            // DotProduct implemented as sum or vectors components'product
            // test here with different approach: a b = |a| |b| cos(alfa)
            Assert.True(dotp.EqualsTol(tol, v1.Length * v2.Length * Cos(v1v2angle)));
        }
    }
}