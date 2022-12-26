namespace SearchAThing
{

	public static partial class SciExt
    {

        /// <summary>
        /// Return the min distance between two adiacent number
        /// given from all of the given ordered set of numbers.        
        /// </summary>                
        /// <returns>0 if empty set or 1 element. min distance otherwise.</returns>
        public static double MinDistance(this IEnumerable<double> orderedNumbers)
        {
            var min = double.MaxValue;
            var en = orderedNumbers.GetEnumerator();
            if (!en.MoveNext()) return 0.0;
            var x = en.Current;
            if (!en.MoveNext()) return 0.0;

            do
            {
                var y = en.Current;
                var dst = Math.Abs(y - x);
                if (dst < min) min = dst;
                x = y;
            }
            while (en.MoveNext());

            return min;
        }

    }

}
