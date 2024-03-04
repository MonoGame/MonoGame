// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	/// <summary>
    /// Packed vector type containing four 16-bit unsigned normalized values ranging from 0 to 1.
	/// </summary>
	public struct Rgba64 : IPackedVector<ulong>, IEquatable<Rgba64>, IPackedVector
	{
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

		private ulong packedValue;

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="x">The initial x-component value for this structure.</param>
        /// <param name="y">The initial y-component value for this structure.</param>
        /// <param name="z">The initial z-component value for this structure.</param>
        /// <param name="w">The initial 2-component value for this structure.</param>
		public Rgba64(float x, float y, float z, float w)
		{
			packedValue = Pack(x, y, z, w);
		}

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector4"/> value who's components contain the initial values for this structure.
        /// </param>
		public Rgba64(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

        /// <inheritdoc />
		public Vector4 ToVector4()
		{
			return new Vector4(
                (float) (((packedValue) & 0xFFFF) / 65535.0f),
                (float) (((packedValue >> 16) & 0xFFFF) / 65535.0f),
                (float) (((packedValue >> 32) & 0xFFFF) / 65535.0f),
                (float) (((packedValue >> 48) & 0xFFFF) / 65535.0f)
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
			return (obj is Rgba64) && Equals((Rgba64) obj);
		}

        /// <inheritdoc />
		public bool Equals(Rgba64 other)
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
		public static bool operator ==(Rgba64 lhs, Rgba64 rhs)
		{
			return lhs.packedValue == rhs.packedValue;
		}

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="lhs">The value on the left of the inequality operator.</param>
        /// <param name="rhs">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
		public static bool operator !=(Rgba64 lhs, Rgba64 rhs)
		{
			return lhs.packedValue != rhs.packedValue;
		}

		private static ulong Pack(float x, float y, float z, float w)
		{
			return (ulong) (
				(((ulong)MathF.Round(MathHelper.Clamp(x * 0xFFFF, 0, 65535f)) ) ) |
				(((ulong)MathF.Round(MathHelper.Clamp(y * 0xFFFF, 0, 65535f)) ) << 16) |
                (((ulong)MathF.Round(MathHelper.Clamp(z * 0xFFFF, 0, 65535f)) ) << 32) |
				(((ulong)MathF.Round(MathHelper.Clamp(w * 0xFFFF, 0, 65535f)) ) << 48)
            );
		}
	}
}
