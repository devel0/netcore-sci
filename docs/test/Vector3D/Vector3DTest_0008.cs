using Xunit;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {
        
        [Fact]
        public void Vector3DTest_0008()
        {
            var tol = 1e-2;

            var vx = -1.1;
            var vy = 2.7;
            var vz = 0.09;
                        
            Assert.True(new Vector3D(vx, vy, vz).EqualsTol(tol, vx, vy, vz));
            Assert.True(new Vector3D(vx, vy).EqualsTol(tol, vx, vy, 0));
        }
    }
}