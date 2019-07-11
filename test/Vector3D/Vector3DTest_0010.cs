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
        public void Vector3DTest_0010()
        {
            var tol = 1e-2;

            var vx = -1.1;
            var vy = 2.7;
            var vz = 0.09;
            var v = new Vector3D(vx, vy, vz);

            var lst = v.Coordinates.ToList();
            Assert.True(lst.Count == 3);
            Assert.True(lst[0].EqualsTol(tol, vx));
            Assert.True(lst[1].EqualsTol(tol, vy));
            Assert.True(lst[2].EqualsTol(tol, vz));
        }
    }
}