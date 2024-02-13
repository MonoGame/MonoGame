// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing two 16-bit signed normalized values, ranging from −1 to 1.
    /// </summary>
	public struct NormalizedShort2 : IPackedVector<uint>, IEquatable<NormalizedShort2>
	{
		private uint short2Packed;

        /// <summary>
        /// Creates a new <see cref="NormalizedShort2"/> from the specified <see cref="Vector2"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector2"/> whos components contain the initial values
        /// for this <see cref="NormalizedShort2"/>.
        /// </param>
        public NormalizedShort2(Vector2 vector)
		{
            short2Packed = PackInTwo(vector.X, vector.Y);
		}

        /// <summary>
        /// Creates a new <see cref="NormalizedShort2"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
        public NormalizedShort2(float x, float y)
		{
            short2Packed = PackInTwo(x, y);
		}

        /// <summary>
        /// Returns a value that indicates whether two <see cref="NormalizedShort2"/>
        /// values are not equal.
        /// </summary>
        /// <param name="a">The <see cref="NormalizedShort2"/> on the left of the inequality operator.</param>
        /// <param name="b">The <see cref="NormalizedShort2"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(NormalizedShort2 a, NormalizedShort2 b)
		{
			return !a.Equals (b);
		}

        /// <summary>
        /// Returns a value that indicates whether two <see cref="NormalizedShort2"/>
        /// values are equal.
        /// </summary>
        /// <param name="a">The <see cref="NormalizedShort2"/> on the left of the equality operator.</param>
        /// <param name="b">The <see cref="NormalizedShort2"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(NormalizedShort2 a, NormalizedShort2 b)
		{
			return a.Equals (b);
		}

        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="NormalizedShort2"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="NormalizedShort2"/>
        /// </value>
        public uint PackedValue
        {
            get
            {
                return short2Packed;
            }
            set
            {
                short2Packed = value;
            }
		}

        /// <summary>
        /// Returns a value that indicates whether this <see cref="NormalizedShort2"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="NormalizedShort2"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="NormalizedShort2"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
		public override bool Equals (object obj)
		{
            return (obj is NormalizedShort2) && Equals((NormalizedShort2)obj);
		}

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="NormalizedShort2"/>
        /// and a specified <see cref="NormalizedShort2"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="NormalizedShort2"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="NormalizedShort2"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(NormalizedShort2 other)
		{
            return short2Packed.Equals(other.short2Packed);
		}

        /// <summary>
        /// Returns the hash code for this <see cref="NormalizedShort2"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="NormalizedShort2"/> value.
        /// </returns>
		public override int GetHashCode ()
		{
			return short2Packed.GetHashCode();
		}

        /// <summary>
        /// Returns the string representation of this <see cref="NormalizedShort2"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="NormalizedShort2"/> value.
        /// </returns>
		public override string ToString ()
		{
            return short2Packed.ToString("X");
		}

        /// <summary>
        /// Expands this <see cref="NormalizedShort2"/> to a <see cref="Vector2"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="NormalizedShort2"/> as a <see cref="Vector2"/>.
        /// </returns>
		public Vector2 ToVector2 ()
		{
            const float maxVal = 0x7FFF;

			var v2 = new Vector2 ();
            v2.X = ((short)(short2Packed & 0xFFFF)) / maxVal;
            v2.Y = (short)(short2Packed >> 0x10) / maxVal;
			return v2;
		}

		private static uint PackInTwo (float vectorX, float vectorY)
		{
			const float maxPos = 0x7FFF;
            const float minNeg = -maxPos;

			// clamp the value between min and max values
            // Round rather than truncate.
            var word2 = (uint)((int)MathHelper.Clamp(MathF.Round(vectorX * maxPos), minNeg, maxPos) & 0xFFFF);
            var word1 = (uint)(((int)MathHelper.Clamp(MathF.Round(vectorY * maxPos), minNeg, maxPos) & 0xFFFF) << 0x10);

			return (word2 | word1);
		}

		void IPackedVector.PackFromVector4 (Vector4 vector)
		{
            short2Packed = PackInTwo(vector.X, vector.Y);
		}

        /// <summary>
        /// Expands this <see cref="NormalizedShort2"/> to a <see cref="Vector4"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="NormalizedShort2"/> as a <see cref="Vector4"/>.
        /// </returns>
		public Vector4 ToVector4 ()
		{
            const float maxVal = 0x7FFF;

			var v4 = new Vector4 (0,0,0,1);
            v4.X = ((short)((short2Packed >> 0x00) & 0xFFFF)) / maxVal;
            v4.Y = ((short)((short2Packed >> 0x10) & 0xFFFF)) / maxVal;
			return v4;
		}
	}
}
