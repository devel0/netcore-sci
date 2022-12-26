namespace SearchAThing
{

    /// <summary>
    /// Quaternion implementation using doubles for purpose of Vector3D.RotateAboutAxis and Vector3D.RotateAs
    /// references:
    /// - http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/index.htm
    /// - https://www.3dgep.com/understanding-quaternions/
    /// - http://www.ncsa.illinois.edu/People/kindr/emtc/quaternions/
    /// </summary>
    /// <remarks>      
    /// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Transform3D/Transform3DTest_0001.cs)
    /// </remarks>
    public class DQuaternion
    {

        public Vector3D v { get; private set; }

        double s;

        /// <summary>
        /// direct construct quaternion q=[s, v]
        /// </summary>
        public DQuaternion(double s, Vector3D v)
        {
            this.s = s;
            this.v = v;
        }

        /// <summary>
        /// build quaternion from axis and angle.
        /// axis will be subjected to normalization.
        /// </summary>        
        public DQuaternion(Vector3D axis, double alphaRad)
        {
            // general form of quaternion:
            // q = [s, v] = [cos(0.5 * alpha), sin(0.5 * alpha) * axis]
            s = Cos(alphaRad / 2);
            v = Sin(alphaRad / 2) * axis.Normalized();
        }

        /// <summary>
        /// Identity qi = [1, nullvector]
        /// </summary>
        public static DQuaternion Identity => new DQuaternion(1, Vector3D.Zero);

        /// <summary>        
        /// Conjugate
        /// q* = [s, -v]        
        /// </summary>
        public DQuaternion Conjugate() => new DQuaternion(s, -v);

        /// <summary>
        /// Multiply
        /// [sa, va] * [sb, vb] = [sa * sb - va * vb, va x vb + sa * vb + sb * va]
        /// </summary>
        public static DQuaternion operator *(DQuaternion qa, DQuaternion qb) =>
            new DQuaternion(
                // sa * sb - va * vb
                qa.s * qb.s - qa.v.DotProduct(qb.v),
                // va x vb + sa * vb + sb * va
                qa.v.CrossProduct(qb.v) + qa.s * qb.v + qb.s * qa.v);

    }

}
