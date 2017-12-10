using System;
using System.Collections.Generic;
using System.Text;

using Vec3 = Microsoft.Xna.Framework.Vector3;
using Vec4 = Microsoft.Xna.Framework.Vector4;

namespace TextureSquish
{
    struct Sym3x3
    {
        public Sym3x3(float s)
        {
            m_x = new float[6];

            for (int i = 0; i < 6; ++i) m_x[i] = s;
        }

        public float this[int index]
        {
            get { return m_x[index]; }
            set { m_x[index] = value; }
        }

        private readonly float[] m_x;

        public static Sym3x3 ComputeWeightedCovariance(int n, Vec3[] points, float[] weights)
        {
            // compute the centroid
            float total = 0.0f;
            var centroid = Vec3.Zero;

            for (int i = 0; i < n; ++i)
            {
                total += weights[i];
                centroid += weights[i] * points[i];
            }
            if (total > float.Epsilon) centroid /= total;

            // accumulate the covariance matrix
            var covariance = new Sym3x3( 0 );
            for (int i = 0; i < n; ++i)
            {
                Vec3 a = points[i] - centroid;
                Vec3 b = weights[i] * a;

                covariance[0] += a.X * b.X;
                covariance[1] += a.X * b.Y;
                covariance[2] += a.X * b.Z;
                covariance[3] += a.Y * b.Y;
                covariance[4] += a.Y * b.Z;
                covariance[5] += a.Z * b.Z;
            }

            // return it
            return covariance;
        }

        public static Vec3 ComputePrincipleComponent(Sym3x3 matrix)
        {
            Vec4 row0 = new Vec4(matrix[0], matrix[1], matrix[2], 0.0f);
            Vec4 row1 = new Vec4(matrix[1], matrix[3], matrix[4], 0.0f);
            Vec4 row2 = new Vec4(matrix[2], matrix[4], matrix[5], 0.0f);
            Vec4 v = new Vec4(1.0f);
            for (int i = 0; i < 8; ++i)
            {
                // matrix multiply
                Vec4 w = row0 * v.SplatX();
                w = row1.MultiplyAdd(v.SplatY(), w);
                w = row2.MultiplyAdd(v.SplatZ(), w);

                // get max component from xyz in all channels
                Vec4 a = Vec4.Max(w.SplatX(), Vec4.Max(w.SplatY(), w.SplatZ()));

                // divide through and advance
                v = w * a.Reciprocal();
            }

            return v.GetVec3();
        }        
    }
}
