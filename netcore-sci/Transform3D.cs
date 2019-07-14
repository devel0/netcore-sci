
using System.DoubleNumerics;

namespace SearchAThing.Sci
{

    /// <summary>
    /// Use quaternion to append rotate transformations    
    /// </summary>
    /// <remarks>      
    /// [unit test](/test/Transform3D/Transform3DTest_0001.cs)
    /// ![](/test/Transform3D/Transform3DTest_0001.png)
    /// </remarks>
    public class Transform3D
    {

        Quaternion q;
        Vector3 xAxis;
        Vector3 yAxis;
        Vector3 zAxis;

        /// <summary>
        /// instantiate an identity transformation        
        /// </summary>
        /// <remarks>      
        /// [unit test](/test/Transform3D/Transform3D_0001.cs)
        /// </remarks>
        public Transform3D()
        {
            xAxis = new Vector3(1, 0, 0);
            yAxis = new Vector3(0, 1, 0);
            zAxis = new Vector3(0, 0, 1);
            q = Quaternion.Identity;
        }

        /// <summary>
        /// add rotation about X axis of given angle to the current rotation matrix        
        /// </summary>        
        /// <param name="angleRad">rotation angle about X axis</param>
        /// <remarks>      
        /// [unit test](/test/Transform3D/Transform3DTest_0001.cs)
        /// </remarks>
        public void RotateAboutXAxis(double angleRad)
        {
            // https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation
            // q' = q2 * q1
            var q1 = this.q;
            var q2 = Quaternion.CreateFromAxisAngle(xAxis, angleRad);
            this.q = q2 * q1;
        }

        /// <summary>
        /// add rotation about Y axis of given angle to the current rotation matrix        
        /// </summary>
        /// <param name="angleRad">rotation angle about Y axis</param>
        /// <remarks>      
        /// [unit test](/test/Transform3D/Transform3DTest_0001.cs)
        /// </remarks>
        public void RotateAboutYAxis(double angleRad)
        {
            // https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation
            // q' = q2 * q1
            var q1 = this.q;
            var q2 = Quaternion.CreateFromAxisAngle(yAxis, angleRad);
            this.q = q2 * q1;
        }

        /// <summary>
        /// add rotation about z axis of given angle to the current rotation matrix        
        /// </summary>
        /// <param name="angleRad">rotation angle about Z axis</param>
        /// <remarks>      
        /// [unit test](/test/Transform3D/Transform3DTest_0001.cs)
        /// </remarks>
        public void RotateAboutZAxis(double angleRad)
        {
            // https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation
            // q' = q2 * q1
            var q1 = this.q;
            var q2 = Quaternion.CreateFromAxisAngle(zAxis, angleRad);
            this.q = q2 * q1;
        }

        /// <summary>
        /// add rotation about given axis of given angle to the current rotation matrix.
        /// given axis will subjected to normalization        
        /// </summary>        
        /// <param name="axis">custom rotation axis</param>
        /// <param name="angleRad">rotation angle about given axis</param>                                
        /// <remarks>      
        /// [unit test](/test/Transform3D/Transform3DTest_0001.cs)
        /// </remarks>
        public void RotateAboutAxis(Vector3D axis, double angleRad)
        {
            var normalizedAxis = axis.Normalized();

            // https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation
            // q' = q2 * q1            
            var q1 = this.q;
            var q2 = Quaternion.CreateFromAxisAngle(
                new Vector3(normalizedAxis.X, normalizedAxis.Y, normalizedAxis.Z), angleRad);
            this.q = q2 * q1;
        }

        /// <summary>
        /// apply this transformation to given vector returning new one        
        /// </summary>
        /// <param name="v">vector to transform</param>
        /// <remarks>      
        /// [unit test](/test/Transform3D/Transform3DTest_0001.cs)
        /// </remarks>
        public Vector3D Apply(Vector3D v)
        {
            // https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation
            // p' = qpq^(-1)
            //var q_1 = Quaternion.Conjugate(q);//Quaternion.Normalize(q));
            var q_1 = Quaternion.Conjugate(q);
            var p = new Quaternion(v.X, v.Y, v.Z, 0);
            var p_ = q * p * q_1;
            return new Vector3D(p_.X, p_.Y, p_.Z);
        }

    }

}

