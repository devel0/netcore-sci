using SearchAThing;
using SearchAThing.Util;
using YLScsDrawing.Drawing3d;

namespace SearchAThing.Sci
{

    public class Transform3D
    {        

        Quaternion d;

        public Transform3D()
        {
            //m = sMatrix3D.Identity;
        }

        public void RotateAboutXAxis(double angleRad)
        {
            //m = m * sMatrix3D.CreateFromQuaternion(sQuaternion.CreateFromAxisAngle(sXAxis, (float)angleRad));
            ////m.Rotate(new sQuaternion(sXAxis, angleRad.ToDeg()));
        }

        public void RotateAboutYAxis(double angleRad)
        {
            //m = m * sMatrix3D.CreateFromQuaternion(sQuaternion.CreateFromAxisAngle(sYAxis, (float)angleRad));
            ////m.Rotate(new sQuaternion(sYAxis, angleRad.ToDeg()));
        }

        public void RotateAboutZAxis(double angleRad)
        {
            //m = m * sMatrix3D.CreateFromQuaternion(sQuaternion.CreateFromAxisAngle(sZAxis, (float)angleRad));
            ////m.Rotate(new sQuaternion(sZAxis, angleRad.ToDeg()));
        }

        public void RotateAboutAxis(Vector3D axis, double angleRad)
        {
            //var v = new sVector3D((float)axis.X, (float)axis.Y, (float)axis.Z);
            //m = m * sMatrix3D.CreateFromQuaternion(sQuaternion.CreateFromAxisAngle(v, (float)angleRad));
            ////m.Rotate(new sQuaternion(new sVector3D(axis.X, axis.Y, axis.Z), angleRad.ToDeg()));
        }

        public Vector3D Apply(Vector3D v)
        {
            //return sVector3D.Transform(v.ToSystemVector3D(), m).ToVector3D();
            ////return m.Transform(v.ToSystemVector3D()).ToVector3D();
            return null;
        }

    }

}
