// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing unsigned normalized values, ranging from 0 to 1, using 4 bits each for x, y, z, and w.
    /// </summary>
    public struct Bgra4444 : IPackedVector<UInt16>, IEquatable<Bgra4444>, IPackedVector
    {
        UInt16 _packedValue;

        /// <summary>
        /// Creates a new instance of Bgra4444.
        /// </summary>
        /// <param name="x">The x component</param>
        /// <param name="y">The y component</param>
        /// <param name="z">The z component</param>
        /// <param name="w">The w component</param>
        public Bgra4444(float x, float y, float z, float w)
        {
            _packedValue = (UInt16)((((int)(x * 15.0f) & 0x0F) << 12) |
                (((int)(y * 15.0f) & 0x0F) << 8) |
                (((int)(z * 15.0f) & 0x0F) << 4) |
                ((int)(w * 15.0f) & 0x0F));
        }

        /// <summary>
        /// Creates a new instance of Bgra4444.
        /// </summary>
        /// <param name="vector">Vector containing the components for the packed vector.</param>
        public Bgra4444(Vector4 vector)
        {
            _packedValue = (UInt16)((((int)(vector.X * 15.0f) & 0x0F) << 12) |
                (((int)(vector.Y * 15.0f) & 0x0F) << 8) |
                (((int)(vector.Z * 15.0f) & 0x0F) << 4) |
                ((int)(vector.W * 15.0f) & 0x0F));
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
        /// Gets the packed vector in Vector4 format.
        /// </summary>
        /// <returns>The packed vector in Vector4 format</returns>
        public Vector4 ToVector4()
        {
            return new Vector4((float)(((_packedValue >> 12) & 0x0F) * (1.0f / 15.0f)),
                (float)(((_packedValue >> 12) & 0x0F) * (1.0f / 15.0f)),
                (float)(((_packedValue >> 8) & 0x0F) * (1.0f / 15.0f)),
                (float)((_packedValue & 0x0F) * (1.0f / 15.0f))
                );
        }

        /// <summary>
        /// Sets the packed vector from a Vector4.
        /// </summary>
        /// <param name="vector">Vector containing the components.</param>
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = (UInt16)((((int)(vector.X * 15.0f) & 0x0F) << 12) |
                (((int)(vector.Y * 15.0f) & 0x0F) << 8) |
                (((int)(vector.Z * 15.0f) & 0x0F) << 4) |
                ((int)(vector.W * 15.0f) & 0x0F));
        }

        /// <summary>
        /// Gets the packed vector in Vector4 format.
        /// </summary>
        /// <returns>The packed vector in Vector4 format</returns>
        Vector4 IPackedVector.ToVector4()
        {
            return new Vector4((float)(((_packedValue >> 12) & 0x0F) * (1.0f / 15.0f)),
                (float)(((_packedValue >> 12) & 0x0F) * (1.0f / 15.0f)),
                (float)(((_packedValue >> 8) & 0x0F) * (1.0f / 15.0f)),
                (float)((_packedValue & 0x0F) * (1.0f / 15.0f))
                );
        }

        /// <summary>
        /// Compares an object with the packed vector.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>true if the object is equal to the packed vector.</returns>
        public override bool Equals(object obj)
        {
            if (obj != null && (obj is Bgra4444))
                return this == (Bgra4444)obj;
            return false;
        }

        /// <summary>
        /// Compares another Bgra4444 packed vector with the packed vector.
        /// </summary>
        /// <param name="other">The Bgra4444 packed vector to compare.</param>
        /// <returns>true if the packed vectors are equal.</returns>
        public bool Equals(Bgra4444 other)
        {
            return _packedValue == other._packedValue;
        }

        /// <summary>
        /// Gets a string representation of the packed vector.
        /// </summary>
        /// <returns>A string representation of the packed vector.</returns>
        public override string ToString()
        {
            return ToVector4().ToString();
        }

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        /// <returns>The hash code for the packed vector.</returns>
        public override int GetHashCode()
        {
            return _packedValue.GetHashCode();
        }

        public static bool operator ==(Bgra4444 lhs, Bgra4444 rhs)
        {
            return lhs._packedValue == rhs._packedValue;
        }

        public static bool operator !=(Bgra4444 lhs, Bgra4444 rhs)
        {
            return lhs._packedValue != rhs._packedValue;
        }
    }
}
