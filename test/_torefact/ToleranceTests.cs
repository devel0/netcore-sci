using Xunit;
using System.Linq;

namespace SearchAThing.Sci.Tests
{
    public class ToleranceTests_torefact
    {

        public ToleranceTests_torefact()
        {
        }

        [Fact]
        public void ToleranceTest_001()
        {
             var dbls = new [] { .1d, .2d, .21d, 1.2d, 1.21d };

             var cmp = new DoubleEqualityComparer(1e-1);

             var q = dbls.Distinct(cmp);

             Assert.True(q.Count() == 3);
        }


    }

}

