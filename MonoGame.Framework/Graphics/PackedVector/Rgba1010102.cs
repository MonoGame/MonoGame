// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing unsigned normalized values ranging from 0 to 1.
    /// The x, y and z components use 10 bits, and the w component uses 2 bits.
    /// </summary>
    public struct Rgba1010102 : IPackedVector<uint>, IEquatable<Rgba1010102>, IPackedVector
    {
        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="Rgba1010102"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="Rgba1010102"/>
        /// </value>
        public uint PackedValue
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

        private uint packedValue;

        /// <summary>
        /// Creates a new <see cref="Rgba1010102"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
        /// <param name="z">The initial z-component value.</param>
        /// <param name="w">The initial w-component value.</param>
        public Rgba1010102(float x, float y, float z, float w)
        {
            packedValue = Pack(x, y, z, w);
        }

        /// <summary>
        /// Creates a new <see cref="Rgba1010102"/> from the specified <see cref="Vector4"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector4"/> whos components contain the initial values
        /// for this <see cref="Rgba1010102"/>.
        /// </param>
        public Rgba1010102(Vector4 vector)
        {
            packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <summary>
        /// Expands this <see cref="Rgba1010102"/> to a <see cref="Vector4"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="Rgba1010102"/> as a <see cref="Vector4"/>.
        /// </returns>
        public Vector4 ToVector4()
        {
            return new Vector4(
                (float) (((packedValue >> 0) & 0x03FF) / 1023.0f),
                (float) (((packedValue >> 10) & 0x03FF) / 1023.0f),
                (float) (((packedValue >> 20) & 0x03FF) / 1023.0f),
                (float) (((packedValue >> 30) & 0x03) / 3.0f)
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
        /// Returns a value that indicates whether this <see cref="Rgba1010102"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="Rgba1010102"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="Rgba1010102"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is Rgba1010102) && Equals((Rgba1010102) obj);
        }

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="Rgba1010102"/>
        /// and a specified <see cref="Rgba1010102"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="Rgba1010102"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="Rgba1010102"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(Rgba1010102 other)
        {
            return packedValue == other.packedValue;
        }

        /// <summary>
        /// Returns the string representation of this <see cref="Rgba1010102"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="Rgba1010102"/> value.
        /// </returns>
        public override string ToString()
        {
            return ToVector4().ToString();
        }

        /// <summary>
        /// Returns the hash code for this <see cref="Rgba1010102"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="Rgba1010102"/> value.
        /// </returns>
        public override int GetHashCode()
        {
            return packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Rgba1010102"/>
        /// values are equal.
        /// </summary>
        /// <param name="lhs">The <see cref="Rgba1010102"/> on the left of the equality operator.</param>
        /// <param name="rhs">The <see cref="Rgba1010102"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Rgba1010102 lhs, Rgba1010102 rhs)
        {
            return lhs.packedValue == rhs.packedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Rgba1010102"/>
        /// values are not equal.
        /// </summary>
        /// <param name="lhs">The <see cref="Rgba1010102"/> on the left of the inequality operator.</param>
        /// <param name="rhs">The <see cref="Rgba1010102"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Rgba1010102 lhs, Rgba1010102 rhs)
        {
            return lhs.packedValue != rhs.packedValue;
        }

        private static uint Pack(float x, float y, float z, float w)
        {
            return (uint) (
                (((int) MathF.Round(MathHelper.Clamp(x, 0, 1) * 1023.0f) & 0x03FF) << 0) |
                (((int) MathF.Round(MathHelper.Clamp(y, 0, 1) * 1023.0f) & 0x03FF) << 10) |
                (((int) MathF.Round(MathHelper.Clamp(z, 0, 1) * 1023.0f) & 0x03FF) << 20) |
                (((int) MathF.Round(MathHelper.Clamp(w, 0, 1) * 3.0f) & 0x03) << 30)
            );
        }
    }
}
