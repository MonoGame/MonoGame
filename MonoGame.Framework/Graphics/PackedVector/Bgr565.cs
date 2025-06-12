// MonoGame - Copyright (C) MonoGame Foundation, Inc
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
            return (UInt16) ((((int) MathF.Round(MathHelper.Clamp(x, 0, 1) * 31.0f) & 0x1F) << 11) |
                (((int) MathF.Round(MathHelper.Clamp(y, 0, 1) * 63.0f) & 0x3F) << 5) |
                ((int) MathF.Round(MathHelper.Clamp(z, 0, 1) * 31.0f) & 0x1F));
        }

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="x">The initial x-component value for this structure.</param>
        /// <param name="y">The initial y-component value for this structure.</param>
        /// <param name="z">The initial z-component value for this structure.</param>
        public Bgr565(float x, float y, float z)
        {
            _packedValue = Pack(x, y, z);
        }

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector3"/> value who's components contain the initial values for this structure.
        /// </param>
        public Bgr565(Vector3 vector)
        {
            _packedValue = Pack(vector.X, vector.Y, vector.Z);
        }

        /// <inheritdoc />
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
        /// Expands the packed representation of this structure to a <see cref="Vector3"/>.
        /// </summary>
        /// <returns>The expanded value.</returns>
        public Vector3 ToVector3()
        {
            return new Vector3((float)(((_packedValue >> 11) & 0x1F) * (1.0f / 31.0f)),
                (float)(((_packedValue >> 5) & 0x3F) * (1.0f / 63.0f)),
                (float)((_packedValue & 0x1F) * (1.0f / 31.0f))
                );
        }

        /// <inheritdoc />
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = (UInt16)((((int)(vector.X * 31.0f) & 0x1F) << 11) |
                (((int)(vector.Y * 63.0f) & 0x3F) << 5) |
                ((int)(vector.Z * 31.0f) & 0x1F));
        }

        /// <inheritdoc />
        public Vector4 ToVector4()
        {
            return new Vector4(ToVector3(), 1.0f);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj != null && (obj is Bgr565))
                return this == (Bgr565)obj;
            return false;
        }

        /// <inheritdoc />
        public bool Equals(Bgr565 other)
        {
            return _packedValue == other._packedValue;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToVector3().ToString();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether the two values are equal.
        /// </summary>
        /// <param name="lhs">The value on the left of the equality operator.</param>
        /// <param name="rhs">The value on the right of the equality operator.</param>
        /// <returns>true if the two values are equal; otherwise, false.</returns>
        public static bool operator ==(Bgr565 lhs, Bgr565 rhs)
        {
            return lhs._packedValue == rhs._packedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="lhs">The value on the left of the inequality operator.</param>
        /// <param name="rhs">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
        public static bool operator !=(Bgr565 lhs, Bgr565 rhs)
        {
            return lhs._packedValue != rhs._packedValue;
        }
    }
}
