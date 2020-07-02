using System.Collections.Generic;

namespace SearchAThing
{

    public class DoubleEqualityComparer : IEqualityComparer<double>
    {

        double tol;

        public DoubleEqualityComparer(double _tol)
        {
            tol = _tol;
        }

        public bool Equals(double x, double y)
        {
            return x.EqualsTol(tol, y);
        }

        public int GetHashCode(double obj)
        {
            return 0;
        }
    }

    public static partial class SciExt
    {



    }

}
