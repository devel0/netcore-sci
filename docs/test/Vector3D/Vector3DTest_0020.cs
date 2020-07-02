using Xunit;
using static System.Math;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {

        [Fact]
        public void Vector3DTest_0020()
        {
            var tol = 1e-8;

            var l1 = new Line3D(
                new Vector3D("X = 3.48476398 Y = 0.45842167 Z = -3.02893918"),
                new Vector3D("X = 0.87006261 Y = 1.11330977 Z = -6.76496666"));

            var l2 = new Line3D(
                new Vector3D("X = 3.35169396 Y = 2.13433388 Z = -6.89816430"),
                new Vector3D("X = 2.79946081 Y = 0.43600438 Z = -7.76172167"));

            var l1l2ang = l1.V.AngleRad(tol, l2.V);
            var radtol = tol.RadTol(Min(l1.Length / 2, l2.Length / 2));
            Assert.True(l1l2ang.EqualsTol(radtol, 1.17088752));
        }
    }
}