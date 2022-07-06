using Xunit;
using System.Linq;
using System;
using netDxf;
using netDxf.Entities;

namespace SearchAThing.Sci.Tests
{
    public partial class LoopTests
    {

        [Fact]
        public void LoopTest_0005()
        {            
            var tol = 1e-1;

            // green and yellow are two adjacent rectangles

            var loopGreen = new Loop(tol, new[]
            {
                new Line3D(4,6,0, 4,6,10),
                new Line3D(4,6,10, -1,6,10),
                new Line3D(-1,6,10, -1,6,0),
                new Line3D(-1,6,0, 4,6,0)
            });
            var loopYellow = new Loop(tol, new[]
            {
                new Line3D(4,6,0, 4,6,10),
                new Line3D(4,6,10, 11,6,10),
                new Line3D(11,6,10, 11,6,0),
                new Line3D(11,6,0, 4,6,0)
            });

            var gyInts = loopGreen.Boolean(tol, loopYellow).ToList();

            Assert.True(gyInts.Count == 0);                         
        }

    }

}