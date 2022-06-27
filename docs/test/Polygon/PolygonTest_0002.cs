using Xunit;
using System.Linq;

namespace SearchAThing.Sci.Tests
{
    public partial class PolygonTests
    {

        [Fact]
        public void PolygonTest_0002()
        {            
            var tol = 1e-1;

            var ccw = new[]
            {
                new Vector3D(0,0,0),
                new Vector3D(0,200,0),
                new Vector3D(250,200,0),
                new Vector3D(250,0,0),
            }.ToList();

            var cw = ccw.Reversed().ToList();

            Assert.True(cw[0].EqualsTol(tol, 250, 0, 0));
            Assert.True(cw[1].EqualsTol(tol, 250, 200, 0));
            Assert.True(cw[2].EqualsTol(tol, 0, 200, 0));
            Assert.True(cw[3].EqualsTol(tol, 0, 0, 0));

            var ccw_centroid = ccw.XYCentroid(tol);
            var cw_centroid = cw.XYCentroid(tol);

            Assert.True(ccw_centroid.EqualsTol(tol, cw_centroid));
            Assert.True(ccw_centroid.EqualsTol(tol, 125, 100));
        }

    }
}