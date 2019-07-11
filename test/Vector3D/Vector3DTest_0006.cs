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
        public void Vector3DTest_0006()
        {
            var tol = 1e-2;
                      
            Assert.True(new Vector3D().EqualsTol(tol, Vector3D.Zero)); 
        }
    }
}