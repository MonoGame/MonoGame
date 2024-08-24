// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing unsigned normalized values ranging from 0 to 1.
    /// The x , y and z components use 5 bits, and the w component uses 1 bit.
    /// </summary>
    public struct Bgra5551 : IPackedVector<UInt16>, IEquatable<Bgra5551>, IPackedVector
    {
        /// <inheritdoc />
        public UInt16 PackedValue
        {
            get
            {
                return packedValue;
            }
            set
            {
                packedValue = value;
            }
        }

        private UInt16 packedValue;

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="x">The initial x-component value for this structure.</param>
        /// <param name="y">The initial y-component value for this structure.</param>
        /// <param name="z">The initial z-component value for this structure.</param>
        /// <param name="w">The initial 2-component value for this structure.</param>
        public Bgra5551(float x, float y, float z, float w)
        {
            packedValue = Pack(x, y, z, w);
        }

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector4"/> value who's components contain the initial values for this structure.
        /// </param>
        public Bgra5551(Vector4 vector)
        {
            packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <inheritdoc />
        public Vector4 ToVector4()
        {
            return new Vector4(
                (float) (((packedValue >> 10) & 0x1F) / 31.0f),
                (float) (((packedValue >> 5) & 0x1F) / 31.0f),
                (float) (((packedValue >> 0) & 0x1F) / 31.0f),
                (float) ((packedValue >> 15)& 0x01)
            );
        }

        /// <inheritdoc />
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return (obj is Bgra5551) && Equals((Bgra5551) obj);
        }

        /// <inheritdoc />
        public bool Equals(Bgra5551 other)
        {
            return packedValue == other.packedValue;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToVector4().ToString();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether the two values are equal.
        /// </summary>
        /// <param name="lhs">The value on the left of the equality operator.</param>
        /// <param name="rhs">The value on the right of the equality operator.</param>
        /// <returns>true if the two values are equal; otherwise, false.</returns>
        public static bool operator ==(Bgra5551 lhs, Bgra5551 rhs)
        {
            return lhs.packedValue == rhs.packedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="lhs">The value on the left of the inequality operator.</param>
        /// <param name="rhs">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
        public static bool operator !=(Bgra5551 lhs, Bgra5551 rhs)
        {
            return lhs.packedValue != rhs.packedValue;
        }

        private static UInt16 Pack(float x, float y, float z, float w)
        {
            return (UInt16) (
                (((int) MathF.Round(MathHelper.Clamp(x, 0, 1) * 31.0f) & 0x1F) << 10) |
                (((int) MathF.Round(MathHelper.Clamp(y, 0, 1) * 31.0f) & 0x1F) << 5) |
                (((int) MathF.Round(MathHelper.Clamp(z, 0, 1) * 31.0f) & 0x1F) << 0) |
                ((((int) MathF.Round(MathHelper.Clamp(w, 0, 1)) & 0x1) << 15))
            );
        }
    }
}
