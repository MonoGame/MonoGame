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
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector2"/> value who's components contain the initial values for this structure.
        /// </param>
		public Short2 (Vector2 vector)
		{
			_short2Packed = PackInTwo (vector.X, vector.Y);
		}

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="x">The initial x-component value for this structure.</param>
        /// <param name="y">The initial y-component value for this structure.</param>
		public Short2 (Single x,Single y)
		{
			_short2Packed = PackInTwo (x, y);
		}

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="a">The value on the left of the inequality operator.</param>
        /// <param name="b">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
		public static bool operator !=(Short2 a, Short2 b)
		{
			return a.PackedValue != b.PackedValue;
		}

        /// <summary>
        /// Returns a value that indicates whether the two values are equal.
        /// </summary>
        /// <param name="a">The value on the left of the equality operator.</param>
        /// <param name="b">The value on the right of the equality operator.</param>
        /// <returns>true if the two values are equal; otherwise, false.</returns>
		public static bool operator ==(Short2 a, Short2 b)
		{
			return a.PackedValue == b.PackedValue;
		}

        /// <inheritdoc />
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

        /// <inheritdoc />
		public override bool Equals (object obj)
		{
            if (obj is Short2)
                return this == (Short2)obj;
            return false;
		}

        /// <inheritdoc />
		public bool Equals (Short2 other)
		{
            return this == other;
		}

        /// <inheritdoc />
		public override int GetHashCode ()
		{
			return _short2Packed.GetHashCode();
		}

        /// <inheritdoc />
		public override string ToString ()
		{
            return _short2Packed.ToString("x8");
		}

        /// <summary>
        /// Expands the packed representation to a <see cref="Vector2"/>.
        /// </summary>
        /// <returns>The expanded value.</returns>
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

        /// <inheritdoc />
        void IPackedVector.PackFromVector4 (Vector4 vector)
		{
			_short2Packed = Short2.PackInTwo (vector.X, vector.Y);
		}

        /// <inheritdoc />
		public Vector4 ToVector4 ()
		{
			var v4 = new Vector4 (0,0,0,1);
			v4.X = (short)(_short2Packed & 0xFFFF);
			v4.Y = (short)(_short2Packed >> 0x10);
			return v4;
		}
	}
}
