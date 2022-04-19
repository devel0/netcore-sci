using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using netDxf.Entities;
using static System.Math;

namespace SearchAThing
{

    public class CirclesOuterTangentResult
    {
        public Vector3D pa1;
        public Vector3D pa2;
        public Vector3D pb1;
        public Vector3D pb2;

    }

    public static partial class SciToolkit
    {

        /// <summary>
        /// retrieve outer tangent points for two 2d circles
        /// </summary>
        /// <param name="center_a">center of circle A</param>
        /// <param name="radius_a">radius of circle A</param>
        /// <param name="center_b">center of circle B</param>
        /// <param name="radius_b">radius of circle B</param>
        /// <returns>null if invalid</returns>
        public static CirclesOuterTangentResult?
            CirclesOuterTangent(double tol, Vector3D center_a, double radius_a, Vector3D center_b, double radius_b)
        {
            var lAB = new Line3D(center_a, center_b);

            var lAperp = lAB.RotateAboutAxis(center_a.LineV(Vector3D.ZAxis), PI / 2);
            var lBperp = lAB.RotateAboutAxis(center_b.LineV(Vector3D.ZAxis), PI / 2);

            var circleA = new Circle3D(tol, new CoordinateSystem3D(center_a, Vector3D.ZAxis), radius_a);
            var Aips = circleA.Intersect(tol, lAperp).ToList();

            var circleB = new Circle3D(tol, new CoordinateSystem3D(center_b, Vector3D.ZAxis), radius_b);
            var Bips = circleB.Intersect(tol, lBperp).ToList();

            if (Aips.Count == 2 && Bips.Count == 2)
                return new CirclesOuterTangentResult { pa1 = Aips[0], pa2 = Aips[1], pb1 = Bips[0], pb2 = Bips[1] };
            else
                return null;
        }

    }

}