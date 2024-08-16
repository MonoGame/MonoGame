// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing four 16-bit signed integer values.
    /// </summary>
    public struct Short4 : IPackedVector<ulong>, IEquatable<Short4>
    {
        ulong packedValue;

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector4"/> value who's components contain the initial values for this structure.
        /// </param>
        public Short4(Vector4 vector)
        {
            packedValue = Pack(ref vector);
        }

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="x">The initial x-component value for this structure.</param>
        /// <param name="y">The initial y-component value for this structure.</param>
        /// <param name="z">The initial z-component value for this structure.</param>
        /// <param name="w">The initial 2-component value for this structure.</param>
        public Short4(float x, float y, float z, float w)
        {
            var vector = new Vector4(x, y, z, w);
            packedValue = Pack(ref vector);
        }

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="a">The value on the left of the inequality operator.</param>
        /// <param name="b">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
        public static bool operator !=(Short4 a, Short4 b)
        {
            return a.PackedValue != b.PackedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether the two values are equal.
        /// </summary>
        /// <param name="a">The value on the left of the equality operator.</param>
        /// <param name="b">The value on the right of the equality operator.</param>
        /// <returns>true if the two values are equal; otherwise, false.</returns>
        public static bool operator ==(Short4 a, Short4 b)
        {
            return a.PackedValue == b.PackedValue;
        }

        /// <inheritdoc />
        public ulong PackedValue
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

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is Short4)
                return this == (Short4)obj;
            return false;
        }

        /// <inheritdoc />
        public bool Equals(Short4 other)
        {
            return this == other;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return packedValue.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return packedValue.ToString("x16");
        }

        static ulong Pack(ref Vector4 vector)
        {
            const long mask = 0xFFFF;
            const long maxPos = 0x7FFF; // Largest two byte positive number 0xFFFF >> 1;
			const float minNeg = ~(int)maxPos; // two's complement

            // clamp the value between min and max values
            var word4 = ((ulong)((int) MathF.Round(MathHelper.Clamp(vector.X, minNeg, maxPos))) & mask);
			var word3 = ((ulong)((int) MathF.Round(MathHelper.Clamp(vector.Y, minNeg, maxPos)) & mask)) << 0x10;
			var word2 = ((ulong)((int) MathF.Round(MathHelper.Clamp(vector.Z, minNeg, maxPos)) & mask)) << 0x20;
			var word1 = ((ulong)((int) MathF.Round(MathHelper.Clamp(vector.W, minNeg, maxPos)) & mask)) << 0x30;

            return word4 | word3 | word2 | word1;
        }

        /// <inheritdoc />
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            packedValue = Pack(ref vector);
        }

        /// <inheritdoc />
        public Vector4 ToVector4()
        {
            return new Vector4(
                (short)(packedValue & 0xFFFF),
                (short)((packedValue >> 0x10) & 0xFFFF),
                (short)((packedValue >> 0x20) & 0xFFFF),
                (short)((packedValue >> 0x30) & 0xFFFF));
        }
    }
}
