using static System.Math;
using System.Collections.Generic;
using System.Linq;
using SearchAThing;

namespace SearchAThing
{

    public static partial class SciExt
    {

        /// <summary>
        /// retrieve angle between from and to given;
        /// angles will subjected to normalization [0,2pi) and angle from can be greather than to
        /// </summary>
        /// <param name="angleFrom">angle from</param>
        /// <param name="angleTo">angle to</param>
        public static double Angle(this double angleFrom, double tol_rad, double angleTo, bool normalizeAngles = true)
        {
            if (normalizeAngles)
            {
                angleFrom = angleFrom.NormalizeAngle2PI(tol_rad);
                angleTo = angleTo.NormalizeAngle2PI(tol_rad);
            }
            
            if (angleFrom > angleTo)
                return angleTo + (2 * PI - angleFrom);
            else
                return angleTo - angleFrom;
        }

        /// <summary>
        /// ensure given angle in [0,2*PI] range
        /// </summary>        
        public static double NormalizeAngle2PI(this double angle_rad, double tol_rad)
        {
            var n = (int)(angle_rad / (2 * PI)).MRound(tol_rad);

            var excess = (n != 0) ? (n.Sign() * 2 * PI) : 0;

            return angle_rad - excess;
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

            return (_min.Value, _max.Value);
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

    }

}
