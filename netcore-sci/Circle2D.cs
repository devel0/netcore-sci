using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;

namespace SearchAThing
{

    public enum CircleTangentType { Exterior, Interior };

    public static partial class SciToolkit
    {

        /// <summary>
        /// create circle 2D by given two points and radius
        /// </summary>
        /// <param name="tol">length tolerance</param>
        /// <param name="radius">circle radius</param>
        /// <param name="p1">circle first point (z not considered)</param>
        /// <param name="p2">circle second point (z not considered)</param>
        /// <returns>circle centers</returns>
        public static Vector3D[] Circle2D(double tol, double radius, Vector3D p1, Vector3D p2)
        {
            if (radius.LessThanTol(tol, 0)) throw new ArgumentException("negative radius");

            if (radius.EqualsTol(tol, 0))
            {
                if (p1.EqualsTol(tol, p2))
                    return new[] { p1 };

                throw new ArgumentException("no circles");
            }
            if (p1.EqualsTol(tol, p2))
                throw new ArgumentException($"infinite number of circles");

            var sqDst = p1.SquaredDistance(p2);
            var sqDiam = 4d * radius * radius;
            if (sqDst.GreatThanTol(tol, sqDiam)) throw new ArgumentException("points too far");

            var midpt = (p1 + p2) / 2;
            if (sqDst.EqualsTol(tol, sqDiam)) return new[] { midpt };

            var d = Sqrt(radius * radius - sqDst / 4);
            var dst = Sqrt(sqDst);
            var o = d * (p2 - p1) / dst;

            var c1 = new Vector3D(midpt.X - o.Y, midpt.Y + o.X);
            var c2 = new Vector3D(midpt.X + o.Y, midpt.Y - o.X);

            if (c1.EqualsTol(tol, c2))
                return new[] { c1 };
            else
                return new[] { c1, c2 };
        }

        /// <summary>
        /// Finds tangent segments between two given circles.
        /// 
        /// returns empty,
        /// or 2 tuple (exterior tangents)
        /// or 4 tuple (two exterior tangents and two interior tangents)
        ///
        /// ref: https://en.wikibooks.org/wiki/Algorithm_Implementation/Geometry/Tangents_between_two_circles
        /// </summary>        
        public static IEnumerable<(CircleTangentType type, Vector3D pa, Vector3D pb)>
            CirclesOuterTangent(
                double x1, double y1, double r1,
                double x2, double y2, double r2)
        {
            var d_sq = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
            if (d_sq <= (r1 - r2) * (r1 - r2)) yield break;

            double d = Math.Sqrt(d_sq);
            double vx = (x2 - x1) / d;
            double vy = (y2 - y1) / d;

            int i = 0;

            for (int sign1 = +1; sign1 >= -1; sign1 -= 2)
            {
                double c = (r1 - sign1 * r2) / d;

                if (c * c > 1.0) continue;
                double h = Math.Sqrt(Math.Max(0.0, 1.0 - c * c));

                for (int sign2 = +1; sign2 >= -1; sign2 -= 2)
                {
                    double nx = vx * c - sign2 * h * vy;
                    double ny = vy * c + sign2 * h * vx;

                    yield return (
                        i < 2 ? CircleTangentType.Exterior : CircleTangentType.Interior,
                        new Vector3D(x1 + r1 * nx, y1 + r1 * ny),
                        new Vector3D(x2 + sign1 * r2 * nx, y2 + sign1 * r2 * ny));

                    ++i;
                }
            }
        }

    }

}