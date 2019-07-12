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
        public void Vector3DTest_0015()
        {
            var tol = 1e-8;
            
            var v1 = new Vector3D("X = 64.34945471 Y = 108.89733154 Z = -34.78875667");
            var v2 = new Vector3D("X = 260.87876324 Y = 240.36185984 Z = 107.99814397");
            var l = new Line3D(v1, v2);

            var p = new Vector3D("X = 56.846351 Y = 179.41412262 Z = 50.06555907");
            var dst = p.Distance(tol, l);

            Assert.True(dst.EqualsTol(tol, 83.85942231));
        }
    }
}