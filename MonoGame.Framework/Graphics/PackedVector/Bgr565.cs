// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing unsigned normalized values ranging from 0 to 1. The x and z components use 5 bits, and the y component uses 6 bits.
    /// </summary>
    public struct Bgr565 : IPackedVector<UInt16>, IEquatable<Bgr565>, IPackedVector
    {
        UInt16 _packedValue;

        private static UInt16 Pack(float x, float y, float z)
        {
            return (UInt16) ((((int) Math.Round(MathHelper.Clamp(x, 0, 1) * 31.0f) & 0x1F) << 11) |
                (((int) Math.Round(MathHelper.Clamp(y, 0, 1) * 63.0f) & 0x3F) << 5) |
                ((int) Math.Round(MathHelper.Clamp(z, 0, 1) * 31.0f) & 0x1F));
        }

        /// <summary>
        /// Creates a new instance of Bgr565.
        /// </summary>
        /// <param name="x">The x component</param>
        /// <param name="y">The y component</param>
        /// <param name="z">The z component</param>
        public Bgr565(float x, float y, float z)
        {
            _packedValue = Pack(x, y, z);
        }

        /// <summary>
        /// Creates a new instance of Bgr565.
        /// </summary>
        /// <param name="vector">Vector containing the components for the packed vector.</param>
        public Bgr565(Vector3 vector)
        {
            _packedValue = Pack(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Gets and sets the packed value.
        /// </summary>
        [CLSCompliant(false)]
        public UInt16 PackedValue
        {
            get
            {
                return _packedValue;
            }
            set
            {
                _packedValue = value;
            }
        }

        /// <summary>
        /// Gets the packed vector in Vector3 format.
        /// </summary>
        /// <returns>The packed vector in Vector3 format</returns>
        public Vector3 ToVector3()
        {
            return new Vector3((float)(((_packedValue >> 11) & 0x1F) * (1.0f / 31.0f)),
                (float)(((_packedValue >> 5) & 0x3F) * (1.0f / 63.0f)),
                (float)((_packedValue & 0x1F) * (1.0f / 31.0f))
                );
        }

        /// <summary>
        /// Sets the packed vector from a Vector4.
        /// </summary>
        /// <param name="vector">Vector containing the components.</param>
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = (UInt16)((((int)(vector.X * 31.0f) & 0x1F) << 11) |
                (((int)(vector.Y * 63.0f) & 0x3F) << 5) |
                ((int)(vector.Z * 31.0f) & 0x1F));
        }

        /// <summary>
        /// Gets the packed vector in Vector4 format.
        /// </summary>
        /// <returns>The packed vector in Vector4 format</returns>
        Vector4 IPackedVector.ToVector4()
        {
            return new Vector4(ToVector3(), 1.0f);
        }

        /// <summary>
        /// Compares an object with the packed vector.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>true if the object is equal to the packed vector.</returns>
        public override bool Equals(object obj)
        {
            if (obj != null && (obj is Bgr565))
                return this == (Bgr565)obj;
            return false;
        }

        /// <summary>
        /// Compares another Bgr565 packed vector with the packed vector.
        /// </summary>
        /// <param name="other">The Bgr565 packed vector to compare.</param>
        /// <returns>true if the packed vectors are equal.</returns>
        public bool Equals(Bgr565 other)
        {
            return _packedValue == other._packedValue;
        }

        /// <summary>
        /// Gets a string representation of the packed vector.
        /// </summary>
        /// <returns>A string representation of the packed vector.</returns>
        public override string ToString()
        {
            return ToVector3().ToString();
        }

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        /// <returns>The hash code for the packed vector.</returns>
        public override int GetHashCode()
        {
            return _packedValue.GetHashCode();
        }

        public static bool operator ==(Bgr565 lhs, Bgr565 rhs)
        {
            return lhs._packedValue == rhs._packedValue;
        }

        public static bool operator !=(Bgr565 lhs, Bgr565 rhs)
        {
            return lhs._packedValue != rhs._packedValue;
        }
    }
}
