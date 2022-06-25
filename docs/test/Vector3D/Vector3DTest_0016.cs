using Xunit;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {

        [Fact]
        public void Vector3DTest_0016()
        {
            var tol = 1e-8;
            
            var v1 = new Vector3D("X = 112.74086295 Y = 79.20170949 Z = 30.72201149");
            var v2 = new Vector3D("X = 289.42452382 Y = 320.02012677 Z = 65.72177141");
            
            var dst = v1.Distance2D(v2);

            Assert.True(dst.EqualsTol(tol, 298.68147938));
        }
    }
}