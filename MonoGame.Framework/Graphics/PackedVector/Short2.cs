// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing two 16-bit signed integer values.
    /// </summary>
	public struct Short2 : IPackedVector<uint>, IEquatable<Short2>
	{
		private uint _short2Packed;

        /// <summary>
        /// Creates a new <see cref="Short2"/> from the specified <see cref="Vector2"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector2"/> whos components contain the initial values
        /// for this <see cref="Short2"/>.
        /// </param>
		public Short2 (Vector2 vector)
		{
			_short2Packed = PackInTwo (vector.X, vector.Y);
		}

        /// <summary>
        /// Creates a new <see cref="Short2"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
		public Short2 (Single x,Single y)
		{
			_short2Packed = PackInTwo (x, y);
		}

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Short2"/>
        /// values are not equal.
        /// </summary>
        /// <param name="a">The <see cref="Short2"/> on the left of the inequality operator.</param>
        /// <param name="b">The <see cref="Short2"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
		public static bool operator !=(Short2 a, Short2 b)
		{
			return a.PackedValue != b.PackedValue;
		}

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Short2"/>
        /// values are equal.
        /// </summary>
        /// <param name="a">The <see cref="Short2"/> on the left of the equality operator.</param>
        /// <param name="b">The <see cref="Short2"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
		public static bool operator ==(Short2 a, Short2 b)
		{
			return a.PackedValue == b.PackedValue;
		}

        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="Short2"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="Short2"/>
        /// </value>
		public uint PackedValue
        {
			get
            {
				return _short2Packed;
			}
			set
            {
				_short2Packed = value;
			}
		}

        /// <summary>
        /// Returns a value that indicates whether this <see cref="Short2"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="Short2"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="Short2"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
		public override bool Equals (object obj)
		{
            if (obj is Short2)
                return this == (Short2)obj;
            return false;
		}

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="Short2"/>
        /// and a specified <see cref="Short2"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="Short2"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="Short2"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
		public bool Equals (Short2 other)
		{
            return this == other;
		}

        /// <summary>
        /// Returns the hash code for this <see cref="Short2"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="Short2"/> value.
        /// </returns>
		public override int GetHashCode ()
		{
			return _short2Packed.GetHashCode();
		}

        /// <summary>
        /// Returns the string representation of this <see cref="Short2"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="Short2"/> value.
        /// </returns>
		public override string ToString ()
		{
            return _short2Packed.ToString("x8");
		}

        /// <summary>
        /// Expands this <see cref="Short2"/> to a <see cref="Vector2"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="Short2"/> as a <see cref="Vector2"/>.
        /// </returns>
		public Vector2 ToVector2 ()
		{
			var v2 = new Vector2 ();
			v2.X = (short)(_short2Packed & 0xFFFF);
			v2.Y = (short)(_short2Packed >> 0x10);
			return v2;
		}

		private static uint PackInTwo (float vectorX, float vectorY)
		{
			const float maxPos = 0x7FFF; // Largest two byte positive number 0xFFFF >> 1;
			const float minNeg = ~(int)maxPos; // two's complement

            // clamp the value between min and max values
            var word2 = ((uint) MathF.Round(MathHelper.Clamp(vectorX, minNeg, maxPos)) & 0xFFFF);
            var word1 = (((uint) MathF.Round(MathHelper.Clamp(vectorY, minNeg, maxPos)) & 0xFFFF) << 0x10);

            return (word2 | word1);
		}

		void IPackedVector.PackFromVector4 (Vector4 vector)
		{
			_short2Packed = Short2.PackInTwo (vector.X, vector.Y);
		}

        /// <summary>
        /// Gets the packed vector in Vector4 format.
        /// </summary>
        /// <returns>The packed vector in Vector4 format</returns>
		public Vector4 ToVector4 ()
		{
			var v4 = new Vector4 (0,0,0,1);
			v4.X = (short)(_short2Packed & 0xFFFF);
			v4.Y = (short)(_short2Packed >> 0x10);
			return v4;
		}
	}
}
