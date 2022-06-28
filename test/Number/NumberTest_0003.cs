using Xunit;
using System.Linq;
using System;
using static System.Math;

namespace SearchAThing.Sci.Tests
{
    public partial class NumberTests
    {

        [Fact]
        public void NumberTest_0003()
        {
            var tolRad = 1e-10;

            // [0,2pi)
            Assert.True(10d.ToRad().NormalizeAngle(tolRad).EqualsTol(tolRad, 10d.ToRad()));
            Assert.True(100d.ToRad().NormalizeAngle(tolRad).EqualsTol(tolRad, 100d.ToRad()));
            Assert.True(190d.ToRad().NormalizeAngle(tolRad).EqualsTol(tolRad, 190d.ToRad()));
            Assert.True(360d.ToRad().NormalizeAngle(tolRad).EqualsTol(tolRad, 0d.ToRad()));
            Assert.True(410d.ToRad().NormalizeAngle(tolRad).EqualsTol(tolRad, 50d.ToRad()));
            Assert.True((-10d).ToRad().NormalizeAngle(tolRad).EqualsTol(tolRad, 350d.ToRad()));
            Assert.True(0d.ToRad().NormalizeAngle(tolRad).EqualsTol(tolRad, 0d.ToRad()));
            Assert.True((-370d).ToRad().NormalizeAngle(tolRad).EqualsTol(tolRad, 350d.ToRad()));

            // [-pi,pi)
            Assert.True(10d.ToRad().NormalizeAngle(tolRad, PI).EqualsTol(tolRad, 10d.ToRad()));
            Assert.True(100d.ToRad().NormalizeAngle(tolRad, PI).EqualsTol(tolRad, 100d.ToRad()));
            Assert.True(190d.ToRad().NormalizeAngle(tolRad, PI).EqualsTol(tolRad, -170d.ToRad()));
            Assert.True(360d.ToRad().NormalizeAngle(tolRad, PI).EqualsTol(tolRad, 0d.ToRad()));
            Assert.True(410d.ToRad().NormalizeAngle(tolRad, PI).EqualsTol(tolRad, 50d.ToRad()));
            Assert.True((-10d).ToRad().NormalizeAngle(tolRad, PI).EqualsTol(tolRad, (-10d).ToRad()));
            Assert.True(0d.ToRad().NormalizeAngle(tolRad, PI).EqualsTol(tolRad, 0d.ToRad()));
            Assert.True((-370d).ToRad().NormalizeAngle(tolRad, PI).EqualsTol(tolRad, (-10d).ToRad()));

            // [-3pi/2,pi/2)
            Assert.True(10d.ToRad().NormalizeAngle(tolRad, PI / 2).EqualsTol(tolRad, 10d.ToRad()));
            Assert.True(100d.ToRad().NormalizeAngle(tolRad, PI / 2).EqualsTol(tolRad, -260d.ToRad()));
            Assert.True(190d.ToRad().NormalizeAngle(tolRad, PI / 2).EqualsTol(tolRad, -170d.ToRad()));
            Assert.True(360d.ToRad().NormalizeAngle(tolRad, PI / 2).EqualsTol(tolRad, 0d.ToRad()));
            Assert.True(410d.ToRad().NormalizeAngle(tolRad, PI / 2).EqualsTol(tolRad, 50d.ToRad()));
            Assert.True((-10d).ToRad().NormalizeAngle(tolRad, PI / 2).EqualsTol(tolRad, (-10d).ToRad()));
            Assert.True(0d.ToRad().NormalizeAngle(tolRad, PI / 2).EqualsTol(tolRad, 0d.ToRad()));
            Assert.True((-370d).ToRad().NormalizeAngle(tolRad, PI / 2).EqualsTol(tolRad, (-10d).ToRad()));

            // [-2pi,0)
            Assert.True(10d.ToRad().NormalizeAngle(tolRad, 0).EqualsTol(tolRad, -350d.ToRad()));
            Assert.True(100d.ToRad().NormalizeAngle(tolRad, 0).EqualsTol(tolRad, -260d.ToRad()));
            Assert.True(190d.ToRad().NormalizeAngle(tolRad, 0).EqualsTol(tolRad, -170d.ToRad()));
            Assert.True(360d.ToRad().NormalizeAngle(tolRad, 0).EqualsTol(tolRad, (-360d).ToRad()));
            Assert.True(410d.ToRad().NormalizeAngle(tolRad, 0).EqualsTol(tolRad, -310d.ToRad()));
            Assert.True((-10d).ToRad().NormalizeAngle(tolRad, 0).EqualsTol(tolRad, (-10d).ToRad()));
            Assert.True(0d.ToRad().NormalizeAngle(tolRad, 0).EqualsTol(tolRad, (-360d).ToRad()));
            Assert.True((-370d).ToRad().NormalizeAngle(tolRad, 0).EqualsTol(tolRad, (-10d).ToRad()));                        

            // misc            
            Assert.True(6.283185286106162d.AngleInRange(5.890752785109231E-06, 0, 2.6901865734990813));
        }

    }
}