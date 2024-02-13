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
        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="Rgba64"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="Rgba64"/>.
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

		private ulong packedValue;

        /// <summary>
        /// Creates a new <see cref="Rgba64"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
        /// <param name="z">The initial z-component value.</param>
        /// <param name="w">The initial w-component value.</param>
		public Rgba64(float x, float y, float z, float w)
		{
			packedValue = Pack(x, y, z, w);
		}

       /// <summary>
        /// Creates a new <see cref="Rgba64"/> from the specified <see cref="Vector4"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector4"/> whos components contain the initial values
        /// for this <see cref="Rgba64"/>.
        /// </param>
		public Rgba64(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

        /// <summary>
        /// Expands this <see cref="Rgba64"/> to a <see cref="Vector4"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="Rgba64"/> as a <see cref="Vector4"/>.
        /// </returns>
		public Vector4 ToVector4()
		{
			return new Vector4(
                (float) (((packedValue) & 0xFFFF) / 65535.0f),
                (float) (((packedValue >> 16) & 0xFFFF) / 65535.0f),
                (float) (((packedValue >> 32) & 0xFFFF) / 65535.0f),
                (float) (((packedValue >> 48) & 0xFFFF) / 65535.0f)
            );
		}

		/// <summary>
		/// Sets the packed vector from a Vector4.
		/// </summary>
		/// <param name="vector">Vector containing the components.</param>
		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

        /// <summary>
        /// Returns a value that indicates whether this <see cref="Rgba64"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="Rgba64"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="Rgba64"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
		public override bool Equals(object obj)
		{
			return (obj is Rgba64) && Equals((Rgba64) obj);
		}

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="Rgba64"/>
        /// and a specified <see cref="Rgba64"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="Rgba64"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="Rgba64"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
		public bool Equals(Rgba64 other)
		{
			return packedValue == other.packedValue;
		}

        /// <summary>
        /// Returns the string representation of this <see cref="Rgba64"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="Rgba64"/> value.
        /// </returns>
		public override string ToString()
		{
			return ToVector4().ToString();
		}

        /// <summary>
        /// Returns the hash code for this <see cref="Rgba64"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="Rgba64"/> value.
        /// </returns>
		public override int GetHashCode()
		{
			return packedValue.GetHashCode();
		}

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Rgba64"/>
        /// values are equal.
        /// </summary>
        /// <param name="lhs">The <see cref="Rgba64"/> on the left of the equality operator.</param>
        /// <param name="rhs">The <see cref="Rgba64"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
		public static bool operator ==(Rgba64 lhs, Rgba64 rhs)
		{
			return lhs.packedValue == rhs.packedValue;
		}

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Rgba64"/>
        /// values are not equal.
        /// </summary>
        /// <param name="lhs">The <see cref="Rgba64"/> on the left of the inequality operator.</param>
        /// <param name="rhs">The <see cref="Rgba64"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
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
