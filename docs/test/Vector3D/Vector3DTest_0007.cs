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
        public void Vector3DTest_0007()
        {
            var tol = 1e-2;

            var vx = -1.1;
            var vy = 2.7;
            var vz = 0.09;
            
            Assert.True(new Vector3D(new[] { vx, vy }).EqualsTol(tol, vx, vy, 0));
            Assert.True(new Vector3D(new[] { vx, vy, vz }).EqualsTol(tol, vx, vy, vz));
            Assert.Throws<ArgumentException>(new Action(() => new Vector3D(new[] { 1d, 2, 3, 4 }))); 
        }
    }
}