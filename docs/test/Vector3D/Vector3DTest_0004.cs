using Xunit;
using System;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {
        
        [Fact]
        public void Vector3DTest_0004()
        {
            var tol = 1e-3;
            
            var xaxis = Vector3D.XAxis;
            var yaxis = Vector3D.YAxis;
            var zaxis = Vector3D.ZAxis;

            Assert.True(xaxis.EqualsTol(tol, Vector3D.Axis(0)));
            Assert.True(yaxis.EqualsTol(tol, Vector3D.Axis(1)));
            Assert.True(zaxis.EqualsTol(tol, Vector3D.Axis(2)));
            Assert.Throws<ArgumentException>(new Action(() => Vector3D.Axis(3)));
        }

    }
}