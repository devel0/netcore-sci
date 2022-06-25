using Xunit;
using System.Linq;
using System;
using static System.Math;

namespace SearchAThing.Sci.Tests
{    
    
    public static class Ext
    {

        public static void AssertEqualsTol(this double actual, double tol, double expected)
        {
            if (!expected.EqualsTol(tol, actual))
                throw new Xunit.Sdk.EqualException(expected, actual);
        }

        public static void AssertEqualsTol(this Vector3D actual, double tol, Vector3D expected)
        {
            if (!expected.EqualsTol(tol, actual))
                throw new Xunit.Sdk.EqualException(expected, actual);
        }
    }

}