using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using netDxf.Entities;

namespace SearchAThing
{

    public enum CircleTangentType { Exterior, Interior };

    public static partial class SciToolkit
    {

        /// <summary>
        /// Finds tangent segments between two given circles.
        /// 
        /// returns empty,
        /// or 2 tuple (exterior tangents)
        /// or 4 tuple (two exterior tangents and two interior tangents)
        ///
        /// ref: https://gieseanw.wordpress.com/2012/09/12/finding-external-tangent-points-for-two-circles/        
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