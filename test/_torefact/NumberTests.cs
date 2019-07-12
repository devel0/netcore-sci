using Xunit;

namespace SearchAThing.Sci.Tests
{
    public class NumberTests
    {

        public NumberTests()
        {
        }

        [Fact]
        public void WeightedDistributionTest()
        {
            {
                var inputs = new[] { 1d, 2, 3 };
                var res = inputs.WeightedDistribution(3);
                Assert.True(res.EqualsTol(new[] { (1d, .333), (2, .333), (3, .333) }, 1e-3, 1e-3));
            }

            {
                var inputs = new[] { 1d, 2.49, 3 };
                var res = inputs.WeightedDistribution(3);
                Assert.True(res.EqualsTol(new[] { (1d, .333), (2, .169), (3, .497) }, 1e-3, 1e-3));
            }

            {
                var inputs = new[] { 1d, 2, 3 };
                var res = inputs.WeightedDistribution(4);
                Assert.True(res.EqualsTol(new[] { (1d, .333), (1.667, .167), (2.333, .167), (3, .333) }, 1e-3, 1e-3));
            }
        }


    }

}

