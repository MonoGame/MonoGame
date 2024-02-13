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
        /// Creates a new <see cref="Short4"/> from the specified <see cref="Vector4"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector4"/> whos components contain the initial values
        /// for this <see cref="Short4"/>.
        /// </param>
        public Short4(Vector4 vector)
        {
            packedValue = Pack(ref vector);
        }

        /// <summary>
        /// Creates a new <see cref="Short4"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
        /// <param name="z">The initial z-component value.</param>
        /// <param name="w">The initial w-component value.</param>
        public Short4(float x, float y, float z, float w)
        {
            var vector = new Vector4(x, y, z, w);
            packedValue = Pack(ref vector);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Short4"/>
        /// values are not equal.
        /// </summary>
        /// <param name="a">The <see cref="Short4"/> on the left of the inequality operator.</param>
        /// <param name="b">The <see cref="Short4"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Short4 a, Short4 b)
        {
            return a.PackedValue != b.PackedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Short4"/>
        /// values are equal.
        /// </summary>
        /// <param name="a">The <see cref="Short4"/> on the left of the equality operator.</param>
        /// <param name="b">The <see cref="Short4"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Short4 a, Short4 b)
        {
            return a.PackedValue == b.PackedValue;
        }

        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="Short4"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="Short4"/>
        /// </value>
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

        /// <summary>
        /// Returns a value that indicates whether this <see cref="Short4"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="Short4"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="Short4"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Short4)
                return this == (Short4)obj;
            return false;
        }

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="Short4"/>
        /// and a specified <see cref="Short4"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="Short4"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="Short4"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(Short4 other)
        {
            return this == other;
        }

        /// <summary>
        /// Returns the hash code for this <see cref="Short4"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="Short4"/> value.
        /// </returns>
        public override int GetHashCode()
        {
            return packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns the string representation of this <see cref="Short4"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="Short4"/> value.
        /// </returns>
        public override string ToString()
        {
            return packedValue.ToString("x16");
        }

        /// <summary>
        /// Packs a vector into a ulong.
        /// </summary>
        /// <param name="vector">The vector containing the values to pack.</param>
        /// <returns>The ulong containing the packed values.</returns>
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

        /// <summary>
        /// Sets the packed representation from a Vector4.
        /// </summary>
        /// <param name="vector">The vector to create the packed representation from.</param>
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            packedValue = Pack(ref vector);
        }

        /// <summary>
        /// Expands this <see cref="Short4"/> to a <see cref="Vector4"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="Short4"/> as a <see cref="Vector4"/>.
        /// </returns>
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
