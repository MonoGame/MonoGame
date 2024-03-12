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
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector2"/> value who's components contain the initial values for this structure.
        /// </param>
        public NormalizedShort2(Vector2 vector)
		{
            short2Packed = PackInTwo(vector.X, vector.Y);
		}

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="x">The initial x-component value for this structure.</param>
        /// <param name="y">The initial y-component value for this structure.</param>
        public NormalizedShort2(float x, float y)
		{
            short2Packed = PackInTwo(x, y);
		}

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="a">The value on the left of the inequality operator.</param>
        /// <param name="b">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
        public static bool operator !=(NormalizedShort2 a, NormalizedShort2 b)
		{
			return !a.Equals (b);
		}

        /// <summary>
        /// Returns a value that indicates whether the two values are equal.
        /// </summary>
        /// <param name="a">The value on the left of the equality operator.</param>
        /// <param name="b">The value on the right of the equality operator.</param>
        /// <returns>true if the two values are equal; otherwise, false.</returns>
        public static bool operator ==(NormalizedShort2 a, NormalizedShort2 b)
		{
			return a.Equals (b);
		}

        /// <inheritdoc />
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

        /// <inheritdoc />
		public override bool Equals (object obj)
		{
            return (obj is NormalizedShort2) && Equals((NormalizedShort2)obj);
		}

        /// <inheritdoc />
        public bool Equals(NormalizedShort2 other)
		{
            return short2Packed.Equals(other.short2Packed);
		}

        /// <inheritdoc />
		public override int GetHashCode ()
		{
			return short2Packed.GetHashCode();
		}

        /// <inheritdoc />
        public override string ToString ()
		{
            return short2Packed.ToString("X");
		}

        /// <summary>
        /// Expands the packed representation to a <see cref="Vector2"/>.
        /// </summary>
        /// <returns>The expanded value.</returns>
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

        /// <inheritdoc />
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
