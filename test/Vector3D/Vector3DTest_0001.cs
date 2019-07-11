using Xunit;
using System.Linq;
using static System.Math;
using System;
using SearchAThing.Util;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {

        /// <summary>
        /// test Vector3D.Zero, Vector3D.XAxis, Vector3D.YAxis, Vector3D.ZAxis, CoordinateSystem3D.WCS
        /// and (IEnumerable Vector3D).IsLinearIndependent
        /// </summary>
        [Fact]
        public void Vector3DTest_0001()
        {
            var zero = Vector3D.Zero;
            var xaxis = Vector3D.XAxis;
            var yaxis = Vector3D.YAxis;
            var zaxis = Vector3D.ZAxis;

            Assert.True(zero.EqualsTol(1e-3, 0, 0, 0));
            Assert.True(xaxis.EqualsTol(1e-3, 1, 0, 0));
            Assert.True(yaxis.EqualsTol(1e-3, 0, 1, 0));
            Assert.True(zaxis.EqualsTol(1e-3, 0, 0, 1));

            Assert.True(new CoordinateSystem3D(zero, xaxis, yaxis, zaxis).Equals(1e-3, CoordinateSystem3D.WCS));

            Assert.True(new[] { xaxis, yaxis, zaxis }.IsLinearIndependent());
            Assert.False(new[] { xaxis, yaxis, new Vector3D(2.2, 1.5, 0) }.IsLinearIndependent());
        }
    }
}