using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework
{
    public partial struct Vector2
    {
        public static implicit operator Vector2(System.Numerics.Vector2 v)
        {
            return new Vector2(v.X, v.Y);
        }

        /// <summary>
        /// Returns a <see cref="System.Numerics.Vector2"/>.
        /// </summary>
        public System.Numerics.Vector2 ToNumerics()
        {
            return new System.Numerics.Vector2(this.X, this.Y);
        }
    }

    public partial struct Vector3
    {
        public static implicit operator Vector3(System.Numerics.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        /// <summary>
        /// Returns a <see cref="System.Numerics.Vector3"/>.
        /// </summary>
        public System.Numerics.Vector3 ToNumerics()
        {
            return new System.Numerics.Vector3(this.X, this.Y, this.Z);
        }
    }

    public partial struct Vector4
    {
        public static implicit operator Vector4(System.Numerics.Vector4 v)
        {
            return new Vector4(v.X, v.Y, v.Z, v.W);
        }

        /// <summary>
        /// Returns a <see cref="System.Numerics.Vector4"/>.
        /// </summary>
        public System.Numerics.Vector4 ToNumerics()
        {
            return new System.Numerics.Vector4(this.X, this.Y, this.Z, this.W);
        }
    }

    public partial struct Quaternion
    {
        public static implicit operator Quaternion(System.Numerics.Quaternion v)
        {
            return new Quaternion(v.X, v.Y, v.Z, v.W);
        }

        /// <summary>
        /// Returns a <see cref="System.Numerics.Quaternion"/>.
        /// </summary>
        public System.Numerics.Quaternion ToNumerics()
        {
            return new System.Numerics.Quaternion(this.X, this.Y, this.Z, this.W);
        }
    }

    public partial struct Plane
    {
        public static implicit operator Plane(System.Numerics.Plane p)
        {
            return new Plane(p.Normal, p.D);
        }

        /// <summary>
        /// Returns a <see cref="System.Numerics.Plane"/>.
        /// </summary>
        public System.Numerics.Plane ToNumerics()
        {
            return new System.Numerics.Plane(this.Normal.X, this.Normal.Y, this.Normal.Z, this.D);
        }
    }

    public partial struct Matrix
    {
        public static implicit operator Matrix(System.Numerics.Matrix4x4 m)
        {
            return new Matrix(
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44);
        }

        /// <summary>
        /// Returns a <see cref="System.Numerics.Matrix4x4"/>.
        /// </summary>
        public System.Numerics.Matrix4x4 ToNumerics()
        {
            return new System.Numerics.Matrix4x4(
                this.M11, this.M12, this.M13, this.M14,
                this.M21, this.M22, this.M23, this.M24,
                this.M31, this.M32, this.M33, this.M34,
                this.M41, this.M42, this.M43, this.M44);
        }
    }
}
