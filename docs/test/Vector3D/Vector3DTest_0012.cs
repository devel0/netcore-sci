using Xunit;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {

        [Fact]
        public void Vector3DTest_0012()
        {
            var tol = 1e-2;

            var vx = -1.1;
            var vy = 2.7;
            var vz = 0.09;
            var v = new Vector3D(vx, vy, vz);

            var v2 = new Vector3D(vx, vy, vz);
            Assert.True(v.EqualsTol(tol, v2));
            Assert.False(v.EqualsTol(tol, new Vector3D(2 * vx, vy, vz)));
            Assert.False(v.EqualsTol(tol, new Vector3D(vx, 2 * vy, vz)));
            Assert.False(v.EqualsTol(tol, new Vector3D(vx, vy, 2 * vz)));

            Assert.True(v.EqualsAutoTol(v2));
            Assert.False(v.EqualsAutoTol(new Vector3D(2 * vx, vy, vz)));
            Assert.False(v.EqualsAutoTol(new Vector3D(vx, 2 * vy, vz)));
            Assert.False(v.EqualsAutoTol(new Vector3D(vx, vy, 2 * vz)));

            Assert.True(v.EqualsTol(tol, vx, vy));
            Assert.True(v.EqualsTol(tol, vx, vy, vz));
        }
    }
}