namespace SearchAThing.Sci.Tests;

public static class Ext
{

    public static void AssertEqualsTol(this double actual, double tol, double expected, string userMessage = "")
    {
        if (!expected.EqualsTol(tol, actual))
            throw new Xunit.Sdk.AssertActualExpectedException(expected, actual, userMessage);
    }

    public static void AssertEqualsTol(this Vector3D actual, double tol,
        double expectedX, double expectedY, double expectedZ) =>
        actual.AssertEqualsTol(tol, new Vector3D(expectedX, expectedY, expectedZ));

    public static void AssertEqualsTol(this Vector3D actual, double tol, Vector3D expected, string userMessage = "")
    {
        if (!expected.EqualsTol(tol, actual))
            throw new Xunit.Sdk.AssertActualExpectedException(expected, actual, userMessage);
    }
}
