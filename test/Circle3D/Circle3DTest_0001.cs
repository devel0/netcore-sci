using Xunit;
using System.Linq;

namespace SearchAThing.Sci.Tests
{
    public partial class Arc3DTests
    {

        [Fact]
        public void Circle3DTest_0001()
        {
            // 2 pts
            {
                var tol = 1e-4;

                var c1 = new Circle3D(
                    new Vector3D(12652.2, -847.9),
                    new Vector3D(12527.2, -722.9),
                    new Vector3D(12402.2, -847.9));

                var c2 = new Circle3D(
                    new Vector3D(13357.5, 112.1),
                    new Vector3D(12397.5, 1072.1),
                    new Vector3D(11437.5, 112.1));

                var qInt = c1.Intersect(tol, c2).ToList();

                Assert.True(qInt.Count == 2);
                Assert.True(qInt.Any(w => w.EqualsTol(tol, 12402.20000052949, -847.888494720131, 0)));
                Assert.True(qInt.Any(w => w.EqualsTol(tol, 12647.715463548095, -814.7183326852199, 0)));
            }

            // 1 pt
            {
                var tol = 1e-4;

                var c1 = new Circle3D(
                    tol,
                    CoordinateSystem3D.WCS.Move(new Vector3D(12397.5233, 112.1186)),
                    960);

                var c2 = new Circle3D(
                    tol,
                    CoordinateSystem3D.WCS.Move(new Vector3D(13535.4631, -465.6793)),
                    316.22776602);

                var qInt = c1.Intersect(tol, c2).ToList();

                Assert.True(qInt.Count == 1);
                Assert.True(qInt.Any(w => w.EqualsTol(tol, 13253.500741174445, -322.5106905459212, 0)));
            }

            // 0 pt
            {
                var tol = 1e-4;

                var c1 = new Circle3D(
                    tol,
                    CoordinateSystem3D.WCS.Move(new Vector3D(12397.5233, 112.1186)),
                    960);

                var c2 = new Circle3D(
                    tol,
                    CoordinateSystem3D.WCS.Move(new Vector3D(13535.4631 + 1, -465.6793)),
                    316.22776602);

                var qInt = c1.Intersect(tol, c2).ToList();

                Assert.True(qInt.Count == 0);
            }
        }
    }
}