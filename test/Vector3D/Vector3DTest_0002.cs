using Xunit;
using System.Linq;
using static System.Math;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {

        /// <summary>
        /// test Geometry implementation for Vector3D
        /// </summary>
        [Fact]
        public void Vector3DTest_0002()
        {
            var tol = 1e-2;

            var vx = -1.1;
            var vy = 2.7;
            var vz = 0.09;
            var v = new Vector3D(vx, vy, vz);

            Assert.True(v.GeomFrom.EqualsTol(tol, vx, vy, vz));
            Assert.True(v.GeomFrom.EqualsTol(tol, v.GeomTo));

            var vertexes = v.Vertexes;
            Assert.True(vertexes.Count() == 1 && vertexes.First().EqualsTol(tol, v));

            var len = v.Length;
            Assert.True(len.EqualsTol(tol, Sqrt(vx * vx + vy * vy + vz * vz)));

            var dxfEnt = v.DxfEntity;
            Assert.True(dxfEnt.Type == netDxf.Entities.EntityType.Point);
            {
                var pos = ((netDxf.Entities.Point)dxfEnt).Position;
                Assert.True(pos.X.EqualsTol(tol, v.X));
                Assert.True(pos.Y.EqualsTol(tol, v.Y));
                Assert.True(pos.Z.EqualsTol(tol, v.Z));
            }

            {
                var en = v.Divide(2);
                Assert.True(en.Count() == 1 && en.First().EqualsTol(tol, v));
            }

            var bbox = v.BBox(tol);
            Assert.True(bbox.Min.EqualsTol(tol, bbox.Max) && bbox.Min.EqualsTol(tol, v));
        }
    }
}