using static System.Math;
using System.Collections.Generic;
using System.Linq;
using SearchAThing;
using System.Runtime.CompilerServices;
using LinqStatistics;
using System.Text;
using MathNet.Numerics.Interpolation;
using System;

namespace SearchAThing
{

    /// <summary>
    /// statistical info about number set
    /// </summary>
    public struct NumbersStatNfo
    {
        public int Count;
        public double Average;
        public double Median;
        public double? Mode;
        public double Variance;
        public double StandardDeviation;
        public double VarianceP;
        public double StandardDeviationP;
        public double Range;
        public double Similarity;

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Count: {Count}");
            sb.AppendLine($"Average: {Average}");
            sb.AppendLine($"Median: {Median}");
            sb.AppendLine($"Mode: {Mode}");
            sb.AppendLine($"Variance: {Variance}");
            sb.AppendLine($"StandardDeviation: {StandardDeviation}");
            sb.AppendLine($"VarianceP: {VarianceP}");
            sb.AppendLine($"StandardDeviationP: {StandardDeviationP}");
            sb.AppendLine($"Range: {Range}");
            sb.AppendLine($"Range: {Similarity}");

            return sb.ToString();
        }
    }

    public static partial class SciExt
    {

        /// <summary>
        /// format number with given decimals and total length aligning right
        /// </summary>
        /// <param name="n">number to format</param>
        /// <param name="dec">decimals round</param>
        /// <param name="size">string size</param>
        /// <returns>string representing given number formatted with given decimals and total length aligning right</returns>
        public static string Fmt(this float n, int dec, int size) =>
            string.Format("{0," + size.ToString() + ":0." + "0".Repeat(dec) + "}", n);

        /// <summary>
        /// format number with given decimals and total length aligning right
        /// </summary>
        /// <param name="n">number to format</param>
        /// <param name="dec">decimals round</param>
        /// <param name="size">string size</param>
        /// <returns>string representing given number formatted with given decimals and total length aligning right</returns>
        public static string Fmt(this double n, int dec, int size) =>
            string.Format("{0," + size.ToString() + ":0." + "0".Repeat(dec) + "}", n);

        /// <summary>
        /// retrieve angle between from and to given;
        /// angles will subjected to normalization [0,2pi) and angle from can be greather than to
        /// </summary>
        /// <param name="angleFrom">angle from</param>
        /// <param name="angleTo">angle to</param>
        /// <param name="tol_rad">angle tolerance (rad)</param>
        /// <param name="normalizeAngles">actuate [0,2PI) angle normalization</param>
        public static double Angle(this double angleFrom, double tol_rad, double angleTo, bool normalizeAngles = true)
        {
            if (normalizeAngles)
            {
                angleFrom = angleFrom.NormalizeAngle(tol_rad);
                angleTo = angleTo.NormalizeAngle(tol_rad);
            }

            if (angleFrom > angleTo)
                return angleTo + (2 * PI - angleFrom);
            else
                return angleTo - angleFrom;
        }

        /// <summary>
        /// Normalize given angle(rad) into [maxRad-2PI,maxRad) range.
        /// </summary>
        /// <param name="angle_rad">angle(rad) to normalize</param>
        /// <param name="tol_rad">tolerance over rad</param>
        /// <param name="maxRadExcluded">normalization range (excluded) max value ( minimum will computed as max-2PI )</param>
        /// <returns>angle normalized</returns>
        /// <remarks>
        /// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Number/NumberTest_0003.cs)
        /// </remarks>
        public static double NormalizeAngle(this double angle_rad, double tol_rad, double maxRadExcluded = 2 * PI)
        {
            if (angle_rad.GreatThanOrEqualsTol(tol_rad, maxRadExcluded - 2 * PI) &&
                angle_rad.LessThanTol(tol_rad, maxRadExcluded))
                return angle_rad;

            var n = (int)(angle_rad / (2 * PI)).MRound(tol_rad);

            var excess = (n != 0) ? (n.Sign() * 2 * PI) : 0;

            var res = (angle_rad - excess).MRound(tol_rad);

            if (res < 0) res += 2 * PI;

            if (res >= maxRadExcluded) res = -2 * PI + res;

            return res;
        }

        /// <summary>
        /// retrieve min,max w/single sweep
        /// </summary>        
        public static (double min, double max) MinMax(this IEnumerable<double> input)
        {
            double? _min = null;
            double? _max = null;

            foreach (var x in input)
            {
                if (_min.HasValue) _min = Min(_min.Value, x); else _min = x;
                if (_max.HasValue) _max = Max(_max.Value, x); else _max = x;
            }

            if (_min == null) throw new Exception($"empty input set");

            return (_min!.Value, _max!.Value);
        }

        /// <summary>
        /// retrieve given input set ordered with only distinct values after comparing through tolerance
        /// in this case result set contains only values from the input set (default) or rounding to given tol if maintain_original_values is false;
        /// if keep_ends true (default) min and max already exists at begin/end of returned sequence
        /// </summary>        
        public static List<double> Thin(this IEnumerable<double> input, double tol, bool keep_ends = true, bool maintain_original_values = true)
        {
            var res = new List<double>();

            var dcmp = new DoubleEqualityComparer(tol);

            if (maintain_original_values)
                res = input.Distinct(dcmp).OrderBy(w => w).ToList();
            else
                res = input.Select(w => w.MRound(tol)).Distinct(dcmp).OrderBy(w => w).ToList();

            var minmax = input.MinMax();

            if (!res.First().EqualsTol(tol, minmax.min)) res.Insert(0, minmax.min);
            if (!res.Last().EqualsTol(tol, minmax.max)) res.Add(minmax.max);

            return res;
        }

        /// <summary>
        /// compares two list tuples
        /// </summary>        
        public static bool EqualsTol(this IEnumerable<(double, double)> tuple_list1, IEnumerable<(double, double)> tuple_list2, double tol1, double tol2)
        {
            var en1 = tuple_list1.GetEnumerator();
            var en2 = tuple_list2.GetEnumerator();

            while (en1.MoveNext())
            {
                var a = en1.Current;
                if (!en2.MoveNext()) return false;// diff cnt
                var b = en2.Current;

                if (!a.Item1.EqualsTol(tol1, b.Item1) || !a.Item2.EqualsTol(tol2, b.Item2)) return false;
            }

            if (en2.MoveNext()) return false; // diff cnt

            return true;
        }

        /// <summary>
        /// retrieve a list of N pairs (value,presence)
        /// with value between min and max of inputs and presence between 0..1 that represents the percent of presence of the value
        /// 
        /// examples:
        ///
        /// inputs = ( 1, 2, 3 ), N = 3
        /// results: ( (1, .33), (2, .33), (3, .33) )
        /// 
        /// inputs = ( 1, 2.49, 3), N = 3
        /// results: ( (1, .33), (2, .169), (3, .497) )
        /// 
        /// inputs = ( 1, 2, 3), N = 4
        /// results: ( (1, .33), (1.6, .16), (2.3, .16), (3, .33) )
        /// 
        /// </summary>        
        public static (double off, double weight)[] WeightedDistribution(this IEnumerable<double> inputs, int N)
        {
            var input_cnt = inputs.Count();
            var minmax = inputs.MinMax();
            var min = minmax.min;
            var dst = minmax.max - minmax.min;
            var step = dst / (N - 1);
            var half_step = step / 2;

            var wcnt = new (double off, double w_hits)[N];
            {
                var pos = min;
                for (int i = 0; i < N; ++i, pos += step)
                {
                    wcnt[i].off = pos;
                }
            }

            var sorted_inputs = inputs.OrderBy(w => w).ToList();

            var w_hits_sum = 0d;

            for (int si = 0; si < sorted_inputs.Count; ++si)
            {
                var x = sorted_inputs[si];

                var widx_left = (int)((x - min) / step);

                var wleft_off = wcnt[widx_left].off;
                var wleft_hit = 1d - (x - wleft_off) / step;
                w_hits_sum += wleft_hit;
                wcnt[widx_left].w_hits += wleft_hit;

                if (widx_left < N - 1)
                {
                    var wright_off = wcnt[widx_left + 1].off;
                    var wright_hig = 1d - (wright_off - x) / step;
                    w_hits_sum += wright_hig;
                    wcnt[widx_left + 1].w_hits += wright_hig;
                }
            }

            // normalize
            for (int i = 0; i < N; ++i) wcnt[i].w_hits /= w_hits_sum;

            return wcnt;
        }

        /// <summary>
        /// Test two numbers for similarity; the factor
        /// of similarity f = (max(x,y)-min(x,y)) / min(abs(x), abs(y)).
        /// Special cases:
        /// - if x=y=0 returns 0
        /// - if x=0 xor y=0 returns max(abs(x),abs(y)))/2
        /// - if sign(x) != sign(y) returns StDevP(x,y)
        /// </summary>
        /// <param name="x">first number</param>
        /// <param name="y">second number</param>
        /// <returns>similarity factor</returns>
        public static double Similarity(this double x, double y)
        {
            if (x == 0 && y == 0) return 0;
            if ((x == 0 && y != 0) || (x != 0 && y == 0)) return Max(Math.Abs(x), Math.Abs(y)) / 2;
            if (Math.Sign(x) != Math.Sign(y)) return new[] { x, y }.StandardDeviationP();

            var a = Min(Math.Abs(x), Math.Abs(y));
            var m = Min(x, y);
            var M = Max(x, y);
            var d = M - m;
            var f = d / a;

            return f;
        }

        /// <summary>
        /// Compute some stat info about given number set using [LinqStatistics](https://github.com/dkackman/LinqStatistics)        
        /// </summary>        
        /// <returns>a tuple containing stat informations about given number set</returns>
        public static NumbersStatNfo StatNfos(this IEnumerable<double> numbers) =>
            new NumbersStatNfo
            {
                Count = numbers.Count(),
                Average = numbers.Average(),
                Median = numbers.Mean(),
                Mode = numbers.Mode(),
                Variance = numbers.Variance(),
                StandardDeviation = numbers.StandardDeviation(),
                VarianceP = numbers.VarianceP(),
                StandardDeviationP = numbers.StandardDeviationP(),
                Range = numbers.Range()
            };

    }

    public static partial class SciToolkit
    {
        /// <summary>
        /// (MathNet.Numerics bookmark function)
        /// "Create a linear spline interpolation from an unsorted set of (x,y) value pairs."
        /// then invoke Interpolate with x value to retrieve interpolated y value.
        /// <para>
        /// For more interpolator see https://numerics.mathdotnet.com/api/MathNet.Numerics.Interpolation/index.htm
        /// </para>
        /// </summary>     
        /// <remarks>      
        /// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Number/NumberTest_0004.cs)        
        /// </remarks>
        public static IInterpolation LinearSplineInterpolate(IEnumerable<double> x, IEnumerable<double> y) =>
            LinearSpline.Interpolate(x, y);

        /// <summary>
        /// span a range of doubles from start to end ( optionally included ) stepping with given inc
        /// </summary>
        /// <param name="tol">measure tolerance</param>
        /// <param name="start">start pos</param>
        /// <param name="end">end pos</param>
        /// <param name="inc">increment</param>
        /// <param name="includeEnd">if true end can included in result set</param>
        /// <returns>enumeration of discrete range items</returns>
        public static IEnumerable<double> Range(double tol, double start, double end, double inc, bool includeEnd = false)
        {
            if (start.LessThanTol(tol, end))
            {
                if (inc < 0) throw new ArgumentException($"invalid negative inc when start less than end");
            }
            else if (start.GreatThanTol(tol, end))
            {
                if (inc > 0) throw new ArgumentException($"invalid positive inc when start less than end");
            }

            double x = start;
            while (x.LessThanTol(tol, end) || (includeEnd ? x.EqualsTol(tol, end) : false))
            {
                yield return x;
                x += inc;
            }
        }
    }

}
