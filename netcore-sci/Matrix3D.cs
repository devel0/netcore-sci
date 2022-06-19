namespace SearchAThing
{

    public class Matrix3D
    {

        double[,] data;

        /// <summary>
        /// empty matrix
        /// </summary>
        public Matrix3D()
        {
            data = new double[3, 3];
        }

        /// <summary>
        /// Init matrix using terms m00 m01 m02 - m10 m11 m12 - m20 m21 m22
        /// </summary>        
        public Matrix3D(double[] terms)
        {
            data = new double[3, 3];

            int r = 0; int c = 0;
            for (int i = 0; i < terms.Length; ++i)
            {
                data[r, c] = terms[i];
                ++c;
                if (c > 2) { c = 0; ++r; }
            }
        }

        public Matrix3D CopyVectorAsRow(Vector3D v, int rowIdx)
        {
            data[rowIdx, 0] = v.X;
            data[rowIdx, 1] = v.Y;
            data[rowIdx, 2] = v.Z;

            return this;
        }

        public Matrix3D CopyVectorAsColumn(Vector3D v, int colIdx)
        {
            data[0, colIdx] = v.X;
            data[1, colIdx] = v.Y;
            data[2, colIdx] = v.Z;

            return this;
        }

        public static Matrix3D FromVectorsAsRows(Vector3D v1, Vector3D v2, Vector3D v3)
        {
            var m = new Matrix3D();

            m.CopyVectorAsRow(v1, 0);
            m.CopyVectorAsRow(v2, 1);
            m.CopyVectorAsRow(v3, 2);

            return m;
        }

        public static Matrix3D FromVectorsAsColumns(Vector3D v1, Vector3D v2, Vector3D v3)
        {
            var m = new Matrix3D();

            m.CopyVectorAsColumn(v1, 0);
            m.CopyVectorAsColumn(v2, 1);
            m.CopyVectorAsColumn(v3, 2);

            return m;
        }

        public bool EqualsTol(double tol, Matrix3D other)
        {
            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    if (!data[r, c].EqualsTol(tol, other.data[r, c])) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// swap row, cols
        /// </summary>        
        public Matrix3D Transpose()
        {
            var m = new Matrix3D();

            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    m.data[r, c] = data[c, r];
                }
            }
            return m;
        }

        /// <summary>
        /// Matrix determinant
        /// http://mathcentral.uregina.ca/QQ/database/QQ.09.06/h/suud1.html
        /// </summary>        
        public double Determinant()
        {
            return
                data[0, 0] * (data[1, 1] * data[2, 2] - data[1, 2] * data[2, 1]) -
                data[0, 1] * (data[1, 0] * data[2, 2] - data[1, 2] * data[2, 0]) +
                data[0, 2] * (data[1, 0] * data[2, 1] - data[1, 1] * data[2, 0]);
        }

        /// <summary>
        /// Matrix of minors
        /// http://www.mathsisfun.com/algebra/matrix-inverse-minors-cofactors-adjugate.html
        /// </summary>        
        public Matrix3D Minor()
        {
            var d = data;
            var res = new Matrix3D();
            var rd = res.data;

            rd[0, 0] = d[1, 1] * d[2, 2] - d[1, 2] * d[2, 1];
            rd[0, 1] = d[1, 0] * d[2, 2] - d[1, 2] * d[2, 0];
            rd[0, 2] = d[1, 0] * d[2, 1] - d[1, 1] * d[2, 0];
            rd[1, 0] = d[0, 1] * d[2, 2] - d[0, 2] * d[2, 1];
            rd[1, 1] = d[0, 0] * d[2, 2] - d[0, 2] * d[2, 0];
            rd[1, 2] = d[0, 0] * d[2, 1] - d[0, 1] * d[2, 0];
            rd[2, 0] = d[0, 1] * d[1, 2] - d[0, 2] * d[1, 1];
            rd[2, 1] = d[0, 0] * d[1, 2] - d[0, 2] * d[1, 0];
            rd[2, 2] = d[0, 0] * d[1, 1] - d[0, 1] * d[1, 0];

            return res;
        }

        /// <summary>
        /// Matrix of cofactors
        /// http://www.mathsisfun.com/algebra/matrix-inverse-minors-cofactors-adjugate.html
        /// </summary>        
        public Matrix3D Cofactor()
        {
            var m = Minor();
            double sign = 1.0;
            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    m.data[r, c] = m.data[r, c] * sign;
                    sign *= -1.0;
                }
            }
            return m;
        }

        /// <summary>
        /// Adjoint matrix
        /// http://www.mathsisfun.com/algebra/matrix-inverse-minors-cofactors-adjugate.html
        /// </summary>        
        public Matrix3D Adjoint() => Cofactor().Transpose();

        /// <summary>
        /// Inverse matrix
        /// http://www.mathsisfun.com/algebra/matrix-inverse-minors-cofactors-adjugate.html
        /// </summary>        
        public Matrix3D Inverse() => Adjoint() / Determinant();

        /// <summary>
        /// Solve linear system of eq represented by this matrix
        /// defined (a,b,c) known terms.        
        /// </summary>        
        public Vector3D Solve(double a, double b, double c) => Solve(new Vector3D(a, b, c));

        /// <summary>
        /// Solve linear system of eq represented by this matrix
        /// defined n known term.
        /// Ax = B -> x = A^(-1)B
        /// </summary>
        public Vector3D Solve(Vector3D n) => Inverse() * n;

        #region operators

        /// <summary>
        /// sum
        /// </summary>        
        public static Matrix3D operator +(Matrix3D a, Matrix3D b)
        {
            var res = new Matrix3D();

            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    res.data[r, c] = a.data[r, c] + b.data[r, c];
                }
            }

            return res;
        }

        /// <summary>
        /// sub
        /// </summary>        
        public static Matrix3D operator -(Matrix3D a, Matrix3D b)
        {
            var res = new Matrix3D();

            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    res.data[r, c] = a.data[r, c] - b.data[r, c];
                }
            }

            return res;
        }

        /// <summary>
        /// neg
        /// </summary>        
        public static Matrix3D operator -(Matrix3D m)
        {
            var res = new Matrix3D();

            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    res.data[r, c] = -m.data[r, c];
                }
            }

            return res;
        }

        /// <summary>
        /// scalar multiply
        /// </summary>        
        public static Matrix3D operator *(double s, Matrix3D m)
        {
            var res = new Matrix3D();

            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    res.data[r, c] = m.data[r, c] * s;
                }
            }

            return res;
        }

        /// <summary>
        /// scalar multiply
        /// </summary>        
        public static Matrix3D operator *(Matrix3D m, double s)
        {
            var res = new Matrix3D();

            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    res.data[r, c] = m.data[r, c] * s;
                }
            }

            return res;
        }

        /// <summary>
        /// scalar div
        /// </summary>        
        public static Matrix3D operator /(double s, Matrix3D m)
        {
            var res = new Matrix3D();

            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    res.data[r, c] = s / m.data[r, c];
                }
            }

            return res;
        }

        /// <summary>
        /// scalar div
        /// </summary>        
        public static Matrix3D operator /(Matrix3D m, double s)
        {
            var res = new Matrix3D();

            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    res.data[r, c] = m.data[r, c] / s;
                }
            }

            return res;
        }

        /// <summary>
        /// indexed matrix component [row,col]
        /// </summary>        
        public double this[int r, int c] => data[r, c];

        /// <summary>
        /// matrix * vector as column -> vector
        /// 3x3 x 3x1 -> 3x1
        /// </summary>        
        public static Vector3D operator *(Matrix3D m, Vector3D v)
        {
            var res = new double[3];

            for (int r = 0; r < 3; ++r)
            {
                var s = 0.0;
                for (int c = 0; c < 3; ++c)
                {
                    s += m.data[r, c] * v[c];
                }
                res[r] = s;
            }

            return new Vector3D(res[0], res[1], res[2]);
        }

        /// <summary>
        /// vector as row * matrix -> vector
        /// 1x3 * 3x3 -> 1x3
        /// </summary>        
        public static Vector3D operator *(Vector3D v, Matrix3D m)
        {
            var res = new double[3];

            for (int c = 0; c < 3; ++c)
            {
                var s = 0.0;
                for (int r = 0; r < 3; ++r)
                {
                    s += v[r] * m.data[r, c];
                }
                res[c] = s;
            }

            return new Vector3D(res[0], res[1], res[2]);
        }

        #endregion

    }

}
