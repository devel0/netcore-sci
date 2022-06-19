using System;
using System.Collections.Generic;
using System.Linq;
using SearchAThing;

namespace SearchAThing
{

    /// <summary>
    /// region of a plane3d delimited by a set of coplanar points
    /// </summary>
    public class Plane3DRegion
    {

        /// <summary>
        /// plane info, contains CS
        /// </summary>
        public Plane3D Plane { get; private set; }

        /// <summary>
        /// (coplanar) points delimiting region of plane3d in WCS coordinates
        /// </summary>
        public IReadOnlyList<Vector3D> Points { get; private set; }

        /// <summary>
        /// (coplanar) points delimiting region of plane3d in CS coordinates ( Z = 0 )
        /// </summary>        
        public IReadOnlyList<Vector3D> CSPoints { get; private set; }

        /// <summary>
        /// build plane3d region based upong given list of pts ( at least 3 non colinear points required )
        /// </summary>
        /// <param name="tol">length tolerance</param>
        /// <param name="pts">region points</param>
        public Plane3DRegion(double tol, IReadOnlyList<Vector3D> pts)
        {
            if (pts.Count < 3) throw new Exception($"at least 3 pts required for Plane3DRegion");
            var o = pts[0];
            Vector3D? a = null, b = null;
            int i = 1;
            while (i < pts.Count)
            {
                if (!pts[i].EqualsTol(tol, o))
                {
                    a = pts[i];
                    ++i;
                    break;
                }
                ++i;
            }
            if (a == null) throw new Exception($"can't find 2 non colinear pts for Plane3DRegion");
            var v1 = a - o;
            var alpha = 0d;
            while (i < pts.Count)
            {
                var v2 = pts[i] - o;
                var ialpha = v1.AngleRad(tol, v2);
                if (ialpha > alpha)
                {
                    b = pts[i];
                    alpha = ialpha;
                }
                ++i;
            }
            if (b == null) throw new Exception($"can't find 3 non colinear pts for Plane3DRegion");

            var cs = new CoordinateSystem3D(o, v1.CrossProduct(b - o));
            Plane = new Plane3D(cs);

            Points = pts;

            CSPoints = Points.Select(w => w.ToUCS(Plane.CS)).ToList();
        }

        /// <summary>
        /// states if this plane3d region contains given point
        /// </summary>
        /// <param name="tol">length tolerance</param>
        /// <param name="p">point to check if contained</param>
        /// <returns>true if given point is in plane3d region</returns>
        public bool Contains(double tol, Vector3D p)
        {
            var cp = p.ToUCS(Plane.CS);

            return cp.Z.EqualsTol(tol, 0) && CSPoints.ContainsPoint(tol, cp);
        }

        /// <summary>
        /// states if given ray line intersect this plane3d region
        /// </summary>
        /// <param name="tol">length tolerance</param>
        /// <param name="ray">ray to test if intersect plane3d region</param>
        /// <returns>intersection point or null if not</returns>
        public Vector3D? Intersect(double tol, Line3D ray)
        {
            var ip = ray.Intersect(tol, Plane);
            if (ip == null || !Contains(tol, ip)) return null;

            return ip;
        }

    }

}
