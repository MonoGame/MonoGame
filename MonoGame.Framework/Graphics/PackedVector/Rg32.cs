// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing two 16-bit unsigned normalized values ranging from 0 to 1.
    /// </summary>
    public struct Rg32 : IPackedVector<uint>, IEquatable<Rg32>, IPackedVector
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
        public Rg32(float x, float y)
        {
            packedValue = Pack(x, y);
        }

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector2"/> value who's components contain the initial values for this structure.
        /// </param>
        public Rg32(Vector2 vector)
        {
            packedValue = Pack(vector.X, vector.Y);
        }

        /// <summary>
        /// Expands the packed representation to a <see cref="Vector2"/>.
        /// </summary>
        /// <returns>The expanded value.</returns>
        public Vector2 ToVector2()
        {
            return new Vector2(
                (float) ((packedValue & 0xFFFF) / 65535.0f),
                (float)(((packedValue >> 16) & 0xFFFF) / 65535.0f)
            );
        }

        /// <inheritdoc />
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            packedValue = Pack(vector.X, vector.Y);
        }

        /// <inheritdoc />
        public Vector4 ToVector4()
        {
            return new Vector4(ToVector2(), 0.0f, 1.0f);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return (obj is Rg32) && Equals((Rg32) obj);
        }

        /// <inheritdoc />
        public bool Equals(Rg32 other)
        {
            return packedValue == other.packedValue;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToVector2().ToString();
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
        public static bool operator ==(Rg32 lhs, Rg32 rhs)
        {
            return lhs.packedValue == rhs.packedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="lhs">The value on the left of the inequality operator.</param>
        /// <param name="rhs">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
        public static bool operator !=(Rg32 lhs, Rg32 rhs)
        {
            return lhs.packedValue != rhs.packedValue;
        }

        private static uint Pack(float x, float y)
        {
            return (uint) (
                (((int) MathF.Round(MathHelper.Clamp(x, 0, 1) * 65535.0f) & 0xFFFF) ) |
                (((int) MathF.Round(MathHelper.Clamp(y, 0, 1) * 65535.0f) & 0xFFFF) << 16)
            );
        }
    }
}
