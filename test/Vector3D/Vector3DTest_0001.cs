using Xunit;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {

        /// <summary>
        /// test Vector3D.Zero, Vector3D.XAxis, Vector3D.YAxis, Vector3D.ZAxis, CoordinateSystem3D.WCS,
        /// (IEnumerable Vector3D).IsLinearIndependent, X, Y, Z
        /// </summary>
        [Fact]
        public void Vector3DTest_0001()
        {
            var tol = 1e-3;

            var zero = Vector3D.Zero;
            var xaxis = Vector3D.XAxis;
            var yaxis = Vector3D.YAxis;
            var zaxis = Vector3D.ZAxis;

            Assert.True(zero.EqualsTol(tol, 0, 0, 0));
            Assert.True(xaxis.EqualsTol(tol, 1, 0, 0));
            Assert.True(yaxis.EqualsTol(tol, 0, 1, 0));
            Assert.True(zaxis.EqualsTol(tol, 0, 0, 1));
            
            Assert.True(new CoordinateSystem3D(zero, xaxis, yaxis, zaxis).Equals(tol, CoordinateSystem3D.WCS));

            Assert.True(new[] { xaxis, yaxis, zaxis }.IsLinearIndependent());
            Assert.False(new[] { xaxis, yaxis, new Vector3D(2.2, 1.5, 0) }.IsLinearIndependent());
        }

    }
}