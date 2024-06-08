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
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector4"/> value who's components contain the initial values for this structure.
        /// </param>
        public NormalizedShort4(Vector4 vector)
		{
            short4Packed = PackInFour(vector.X, vector.Y, vector.Z, vector.W);
		}

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="x">The initial x-component value for this structure.</param>
        /// <param name="y">The initial y-component value for this structure.</param>
        /// <param name="z">The initial z-component value for this structure.</param>
        /// <param name="w">The initial 2-component value for this structure.</param>
        public NormalizedShort4(float x, float y, float z, float w)
		{
            short4Packed = PackInFour(x, y, z, w);
		}

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="a">The value on the left of the inequality operator.</param>
        /// <param name="b">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
        public static bool operator !=(NormalizedShort4 a, NormalizedShort4 b)
		{
			return !a.Equals (b);
		}

        /// <summary>
        /// Returns a value that indicates whether the two values are equal.
        /// </summary>
        /// <param name="a">The value on the left of the equality operator.</param>
        /// <param name="b">The value on the right of the equality operator.</param>
        /// <returns>true if the two values are equal; otherwise, false.</returns>
        public static bool operator ==(NormalizedShort4 a, NormalizedShort4 b)
		{
			return a.Equals (b);
		}

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return (obj is NormalizedShort4) && Equals((NormalizedShort4)obj);
        }

        /// <inheritdoc />
        public bool Equals(NormalizedShort4 other)
        {
            return short4Packed.Equals(other.short4Packed);
        }

        /// <inheritdoc />
		public override int GetHashCode ()
		{
			return short4Packed.GetHashCode();
		}

        /// <inheritdoc />
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

        /// <inheritdoc />
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
