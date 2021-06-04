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

            // 
            {
                var c = new Circle3D(1e-3,
                        CoordinateSystem3D.WCS.Move(
                            new Vector3D(
                                250.21546070748141,
                                -926.81833345210805)
                        ), 125);

                var l = new Line3D(
                    new Vector3D(0, -960), new Vector3D(1, 0, 0),
                    Line3DConstructMode.PointAndVector);

                var q = c.Intersect(1e-3, l).ToList();

                Assert.True(q.Count == 2);
                Assert.True(q[0].EqualsTol(1e-3, 370.731, -960, 0));
                Assert.True(q[1].EqualsTol(1e-3, 129.7, -960, 0));
            }
        }
    }
}