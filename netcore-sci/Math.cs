using AngouriMath;

namespace SearchAThing
{

    public static partial class SciExt
    {

        /// <summary>
        /// expensive simplify of entity expression
        /// </summary>
        /// <param name="expr">expression to simplify</param>
        /// <param name="level">level of simplification</param>
        /// <returns>simplified expression</returns>
        public static Entity VeryExpensiveSimplify(this Entity expr, int level)
        {
            Entity ExpensiveSimplify(Entity expr)
            {
                return expr.Replace(x => x.Simplify());
            }

            if (level <= 0)
                return expr;
            return VeryExpensiveSimplify(ExpensiveSimplify(expr), level - 1);
        }

        /// <summary>
        /// simplify expression (expensive) testing what shorter between simplify and VeryExpensiveSimplify
        /// </summary>
        /// <param name="expr">expression to simplify</param>
        /// <returns>simplified expression</returns>
        public static Entity SmartSimplify(this Entity expr)
        {
            var q1 = expr.Simplify().ToString();
            var q2 = expr.VeryExpensiveSimplify(2).ToString();

            if (q2.Length < q1.Length)
                return q2;
            else
                return q1;
        }

    }

}
