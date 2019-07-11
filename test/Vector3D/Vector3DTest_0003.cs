using Xunit;
using System;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {

        /// <summary>
        /// test Vector3D 
        /// </summary>
        [Fact]
        public void Vector3DTest_0003()
        {
            var tol = 1e-2;

            var vx = -1.1;
            var vy = 2.7;
            var vz = 0.09;
            var v = new Vector3D(vx, vy, vz);

            Assert.True(v.GetOrd(0).EqualsTol(tol, vx));
            Assert.True(v.GetOrd(1).EqualsTol(tol, vy));
            Assert.True(v.GetOrd(2).EqualsTol(tol, vz));
            Assert.Throws<ArgumentException>(new Action(() => v.GetOrd(3)));

            Assert.True(v.GetOrd(OrdIdx.X).EqualsTol(tol, vx));
            Assert.True(v.GetOrd(OrdIdx.Y).EqualsTol(tol, vy));
            Assert.True(v.GetOrd(OrdIdx.Z).EqualsTol(tol, vz));
            Assert.Throws<ArgumentException>(new Action(() => v.GetOrd(3)));             
        }
    }
}