using Xunit;
using System.Linq;
using System;
using netDxf;

namespace SearchAThing.Sci.Tests
{
    public partial class Line3DTests
    {

        [Fact]
        public void Line3DTest_0004()
        {
            var tol = 1e-8;

            var l1 = new Vector3D(10, 0, 0).LineTo(new Vector3D(20, 0, 0));
            var l2 = new Vector3D(15, 0, 0).LineTo(new Vector3D(18, 0, 0));
            var l3 = new Vector3D(20, 0, 0).LineTo(new Vector3D(25, 0, 0));

            {
                var i1_2 = l1.GeomIntersect(tol, l2,
                    thisSegmentMode: GeomSegmentMode.Infinite, otherSegmentMode: GeomSegmentMode.Infinite);
                var q = i1_2.First().GeomEquals(tol, l1);
                Assert.True(i1_2.Count() == 1 && i1_2.First().GeomEquals(tol, l2));
            }            

            {
                var i1_3 = l1.GeomIntersect(tol, l3,
                    thisSegmentMode: GeomSegmentMode.FromTo, otherSegmentMode: GeomSegmentMode.FromTo);
                Assert.True(i1_3.Count() == 1 && i1_3.First().GeomEquals(tol, new Vector3D(20, 0, 0)));            
            }

            // if need to test an infinite line intersect with a semiline
            // test two infinite line then checks test with colinear scalar offset if the resulting point inside semiline

        }

    }
}