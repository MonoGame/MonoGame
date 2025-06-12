// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing unsigned normalized values ranging from 0 to 1.
    /// The x, y and z components use 10 bits, and the w component uses 2 bits.
    /// </summary>
    public struct Rgba1010102 : IPackedVector<uint>, IEquatable<Rgba1010102>, IPackedVector
    {
        /// <inheritdoc />
        public uint PackedValue
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

        private uint packedValue;

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="x">The initial x-component value for this structure.</param>
        /// <param name="y">The initial y-component value for this structure.</param>
        /// <param name="z">The initial z-component value for this structure.</param>
        /// <param name="w">The initial 2-component value for this structure.</param>
        public Rgba1010102(float x, float y, float z, float w)
        {
            packedValue = Pack(x, y, z, w);
        }

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector4"/> value who's components contain the initial values for this structure.
        /// </param>
        public Rgba1010102(Vector4 vector)
        {
            packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <inheritdoc />
        public Vector4 ToVector4()
        {
            return new Vector4(
                (float) (((packedValue >> 0) & 0x03FF) / 1023.0f),
                (float) (((packedValue >> 10) & 0x03FF) / 1023.0f),
                (float) (((packedValue >> 20) & 0x03FF) / 1023.0f),
                (float) (((packedValue >> 30) & 0x03) / 3.0f)
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
            return (obj is Rgba1010102) && Equals((Rgba1010102) obj);
        }

        /// <inheritdoc />
        public bool Equals(Rgba1010102 other)
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
        public static bool operator ==(Rgba1010102 lhs, Rgba1010102 rhs)
        {
            return lhs.packedValue == rhs.packedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="lhs">The value on the left of the inequality operator.</param>
        /// <param name="rhs">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
        public static bool operator !=(Rgba1010102 lhs, Rgba1010102 rhs)
        {
            return lhs.packedValue != rhs.packedValue;
        }

        private static uint Pack(float x, float y, float z, float w)
        {
            return (uint) (
                (((int) MathF.Round(MathHelper.Clamp(x, 0, 1) * 1023.0f) & 0x03FF) << 0) |
                (((int) MathF.Round(MathHelper.Clamp(y, 0, 1) * 1023.0f) & 0x03FF) << 10) |
                (((int) MathF.Round(MathHelper.Clamp(z, 0, 1) * 1023.0f) & 0x03FF) << 20) |
                (((int) MathF.Round(MathHelper.Clamp(w, 0, 1) * 3.0f) & 0x03) << 30)
            );
        }
    }
}
