using Xunit;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {
        
        [Fact]
        public void Vector3DTest_0005()
        {
            var tol = 1e-2;

            var vx = -1.1;
            var vy = 2.7;
            var vz = 0.09;
            var v = new Vector3D(vx, vy, vz);
            
            Assert.True(vx.EqualsTol(tol, v.X));
            Assert.True(vy.EqualsTol(tol, v.Y));
            Assert.True(vz.EqualsTol(tol, v.Z));
        }
    }
}