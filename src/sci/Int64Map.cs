namespace SearchAThing.Sci;

/// <summary>
/// Scan a given domain of doubles, determine the midpoint ( Origin )
/// and using the given tolerance it tests for integrity in conversion between values
/// from double to Int64 and vice-versa.
/// It can generate a Int64MapExceptionRange.
/// </summary>
public class Int64Map
{

    public double Origin { get; private set; }
    public double Tolerance { get; private set; }

    /// <summary>
    /// use small tolerance to avoid lost of precision
    /// Note: too small tolerance can generate Int64MapExceptionRange
    /// </summary>        
    public Int64Map(double tol, IEnumerable<double> domainValues, bool selfCheckTolerance = true)
    {
        var min = double.MaxValue;
        var max = double.MinValue;
        Tolerance = tol;

        foreach (var x in domainValues)
        {
            if (x < min) min = x;
            if (x > max) max = x;
        }

        Origin = (min + max) / 2;

        // self check
        if (selfCheckTolerance)
        {
            foreach (var x in domainValues)
            {
                var fromint = FromInt64(ToInt64(x));

                if (!fromint.EqualsTol(tol, x)) throw new Int64MapExceptionRange($"can't fit given domain within Int64 types. Try bigger tolerance.");
            }
        }
    }

    public Int64 ToInt64(double x) => (Int64)((x - Origin) / Tolerance);
    public double FromInt64(Int64 x) => (((double)x) * Tolerance) + Origin;

}

public class Int64MapExceptionRange : Exception
{

    public Int64MapExceptionRange(string msg) : base(msg)
    {
    }

}
