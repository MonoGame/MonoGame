// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing four 16-bit signed normalized values, ranging from −1 to 1.
    /// </summary>
    public struct NormalizedShort4 : IPackedVector<ulong>, IEquatable<NormalizedShort4>
	{
		private ulong short4Packed;

        /// <summary>
        /// Creates a new <see cref="NormalizedShort4"/> from the specified <see cref="Vector4"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector4"/> whos components contain the initial values
        /// for this <see cref="NormalizedShort4"/>.
        /// </param>
        public NormalizedShort4(Vector4 vector)
		{
            short4Packed = PackInFour(vector.X, vector.Y, vector.Z, vector.W);
		}

        /// <summary>
        /// Creates a new <see cref="NormalizedShort4"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
        /// <param name="z">The initial z-component value.</param>
        /// <param name="w">The initial w-component value.</param>
        public NormalizedShort4(float x, float y, float z, float w)
		{
            short4Packed = PackInFour(x, y, z, w);
		}

        /// <summary>
        /// Returns a value that indicates whether two <see cref="NormalizedShort4"/>
        /// values are not equal.
        /// </summary>
        /// <param name="a">The <see cref="NormalizedShort4"/> on the left of the inequality operator.</param>
        /// <param name="b">The <see cref="NormalizedShort4"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(NormalizedShort4 a, NormalizedShort4 b)
		{
			return !a.Equals (b);
		}

        /// <summary>
        /// Returns a value that indicates whether two <see cref="NormalizedShort4"/>
        /// values are equal.
        /// </summary>
        /// <param name="a">The <see cref="NormalizedShort4"/> on the left of the equality operator.</param>
        /// <param name="b">The <see cref="NormalizedShort4"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(NormalizedShort4 a, NormalizedShort4 b)
		{
			return a.Equals (b);
		}

        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="NormalizedShort4"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="NormalizedShort4"/>
        /// </value>
        public ulong PackedValue
        {
            get
            {
                return short4Packed;
            }
            set
            {
                short4Packed = value;
            }
		}

        /// <summary>
        /// Returns a value that indicates whether this <see cref="NormalizedShort4"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="NormalizedShort4"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="NormalizedShort4"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is NormalizedShort4) && Equals((NormalizedShort4)obj);
        }

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="NormalizedShort4"/>
        /// and a specified <see cref="NormalizedShort4"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="NormalizedShort4"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="NormalizedShort4"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(NormalizedShort4 other)
        {
            return short4Packed.Equals(other.short4Packed);
        }

        /// <summary>
        /// Returns the hash code for this <see cref="NormalizedShort4"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="NormalizedShort4"/> value.
        /// </returns>
		public override int GetHashCode ()
		{
			return short4Packed.GetHashCode();
		}

        /// <summary>
        /// Returns the string representation of this <see cref="NormalizedShort4"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="NormalizedShort4"/> value.
        /// </returns>
		public override string ToString ()
		{
            return short4Packed.ToString("X");
		}

        private static ulong PackInFour(float vectorX, float vectorY, float vectorZ, float vectorW)
		{
			const long mask = 0xFFFF;
            const long maxPos = 0x7FFF;
            const long minNeg = -maxPos;

			// clamp the value between min and max values
            var word4 = (ulong)((int)MathF.Round(MathHelper.Clamp(vectorX * maxPos, minNeg, maxPos)) & mask);
            var word3 = (ulong)((int)MathF.Round(MathHelper.Clamp(vectorY * maxPos, minNeg, maxPos)) & mask) << 0x10;
            var word2 = (ulong)((int)MathF.Round(MathHelper.Clamp(vectorZ * maxPos, minNeg, maxPos)) & mask) << 0x20;
            var word1 = (ulong)((int)MathF.Round(MathHelper.Clamp(vectorW * maxPos, minNeg, maxPos)) & mask) << 0x30;

			return (word4 | word3 | word2 | word1);
		}

		void IPackedVector.PackFromVector4 (Vector4 vector)
		{
            short4Packed = PackInFour(vector.X, vector.Y, vector.Z, vector.W);
		}

        /// <summary>
        /// Expands this <see cref="NormalizedShort4"/> to a <see cref="Vector4"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="NormalizedShort4"/> as a <see cref="Vector4"/>.
        /// </returns>
		public Vector4 ToVector4 ()
		{
            const float maxVal = 0x7FFF;

			var v4 = new Vector4 ();
            v4.X = ((short)((short4Packed >> 0x00) & 0xFFFF)) / maxVal;
            v4.Y = ((short)((short4Packed >> 0x10) & 0xFFFF)) / maxVal;
            v4.Z = ((short)((short4Packed >> 0x20) & 0xFFFF)) / maxVal;
            v4.W = ((short)((short4Packed >> 0x30) & 0xFFFF)) / maxVal;
			return v4;
		}
	}
}
