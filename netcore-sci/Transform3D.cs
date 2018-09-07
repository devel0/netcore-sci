
using SearchAThing;
using SearchAThing.Util;
using YLScsDrawing.Drawing3d;

namespace SearchAThing.Sci
{

    public class Transform3D
    {        

        Quaternion q;
        Vector3d xAxis;
        Vector3d yAxis;
        Vector3d zAxis;

        public Transform3D()
        {
            xAxis = new Vector3d(1,0,0);
            yAxis = new Vector3d(0,1,0);
            zAxis = new Vector3d(0,0,1);
            q = new Quaternion(1,0,0,0);
            //m = sMatrix3D.Identity;
        }

        public void RotateAboutXAxis(double angleRad)
        {
            var q = new Quaternion();
            q.FromAxisAngle(xAxis, angleRad);
            this.q = q * this.q;
            //m = m * sMatrix3D.CreateFromQuaternion(sQuaternion.CreateFromAxisAngle(sXAxis, (float)angleRad));
            ////m.Rotate(new sQuaternion(sXAxis, angleRad.ToDeg()));
        }

        public void RotateAboutYAxis(double angleRad)
        {
            var q = new Quaternion();
            q.FromAxisAngle(yAxis, angleRad);
            this.q = q * this.q;
            //m = m * sMatrix3D.CreateFromQuaternion(sQuaternion.CreateFromAxisAngle(sYAxis, (float)angleRad));
            ////m.Rotate(new sQuaternion(sYAxis, angleRad.ToDeg()));
        }

        public void RotateAboutZAxis(double angleRad)
        {
            var q = new Quaternion();
            q.FromAxisAngle(zAxis, angleRad);
            this.q = q * this.q;
            //m = m * sMatrix3D.CreateFromQuaternion(sQuaternion.CreateFromAxisAngle(sZAxis, (float)angleRad));
            ////m.Rotate(new sQuaternion(sZAxis, angleRad.ToDeg()));
        }

        public void RotateAboutAxis(Vector3D axis, double angleRad)
        {
            var v = new Vector3d(axis.X, axis.Y, axis.Z);
            var q = new Quaternion();
            q.FromAxisAngle(v, angleRad);
            this.q = q * this.q;
            //var v = new sVector3D((float)axis.X, (float)axis.Y, (float)axis.Z);
            //m = m * sMatrix3D.CreateFromQuaternion(sQuaternion.CreateFromAxisAngle(v, (float)angleRad));
            ////m.Rotate(new sQuaternion(new sVector3D(axis.X, axis.Y, axis.Z), angleRad.ToDeg()));
        }

        public Vector3D Apply(Vector3D v)
        {            
            var p = new Point3d(v.X, v.Y, v.Z);
            var pr = q.Rotate(p);
            return new Vector3D(pr.X, pr.Y, pr.Z);            
        }

    }

}

