using Xunit;
using System.Linq;

using static SearchAThing.SciToolkit;
using SearchAThing;
namespace SearchAThing.Sci.Tests
{
    public partial class ToolkitTests
    {

        [Fact]
        public void Circle2DTest_0001()
        {
            var res = CirclesOuterTangent(
                75, 125, 12.5,
                175, 25, 7)
                .ToList();

            Assert.True(res.Count == 4);

            // var l1 = res[0].pa.LineTo(res[0].pb).QCadScript;
            // var l2 = res[1].pa.LineTo(res[1].pb).QCadScript;
            // var l3 = res[2].pa.LineTo(res[2].pb).QCadScript;
            // var l4 = res[3].pa.LineTo(res[3].pb).QCadScript;

            if (res == null)
            {
                Assert.True(false);
            }
            else
            {
                var pa_1_external = new Vector3D(84.17589787, 133.48839787);
                var pa_2_external = new Vector3D(66.51160213, 115.8241021);

                var pa_1_internal = new Vector3D(84.97315737, 132.53565737);
                var pa_2_internal = new Vector3D(67.46434263, 115.02684263);

                var pb_1_external = new Vector3D(180.13850281, 29.75350281);
                var pb_2_external = new Vector3D(170.24649719, 19.86149719);

                var pb_1_internal = new Vector3D(169.41503187, 20.78003187);
                var pb_2_internal = new Vector3D(179.21996813, 30.58496813);

                var exteriorTagents = res.Where(r => r.type == CircleTangentType.Exterior).ToList();
                var interiorTagents = res.Where(r => r.type == CircleTangentType.Interior).ToList();

                Assert.True(exteriorTagents[0].pa.EqualsTol(1e-1, pa_1_external));
                Assert.True(exteriorTagents[0].pb.EqualsTol(1e-1, pb_1_external));

                Assert.True(exteriorTagents[1].pa.EqualsTol(1e-1, pa_2_external));
                Assert.True(exteriorTagents[1].pb.EqualsTol(1e-1, pb_2_external));

                Assert.True(interiorTagents[0].pa.EqualsTol(1e-1, pa_1_internal));
                Assert.True(interiorTagents[0].pb.EqualsTol(1e-1, pb_1_internal));

                Assert.True(interiorTagents[1].pa.EqualsTol(1e-1, pa_2_internal));
                Assert.True(interiorTagents[1].pb.EqualsTol(1e-1, pb_2_internal));
            }

        }
    }
}