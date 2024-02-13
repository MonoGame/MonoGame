// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing a single 8 bit normalized W values that is ranging from 0 to 1.
    /// </summary>
    public struct Alpha8 : IPackedVector<byte>, IEquatable<Alpha8>, IPackedVector
    {
        private byte packedValue;

        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="Alpha8"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="Alpha8"/>
        /// </value>
        public byte PackedValue
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
        /// Creates a new <see cref="Alpha8"/> initialized with the specified value.
        /// </summary>
        /// <param name="alpha">The initial value of the <see cref="Alpha8"/></param>
        public Alpha8(float alpha)
        {
            packedValue = Pack(alpha);
        }

        /// <summary>
        /// Expands this <see cref="Alpha8"/> to a <see cref="Single"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="Alpha8"/> as a <see cref="Single"/>.
        /// </returns>
        public float ToAlpha()
        {
            return (float) (packedValue / 255.0f);
        }

        /// <summary>
        /// Sets the packed vector from a Vector4.
        /// </summary>
        /// <param name="vector">Vector containing the components.</param>
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            packedValue = Pack(vector.W);
        }

        /// <summary>
        /// Expands this <see cref="Alpha8"/> to a <see cref="Vector4"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="Alpha8"/> as a <see cref="Vector4"/>.
        /// </returns>
        public Vector4 ToVector4()
        {
            return new Vector4(
                0.0f,
                0.0f,
                0.0f,
                (float) (packedValue / 255.0f)
            );
        }

        /// <summary>
        /// Returns a value that indicates whether this <see cref="Alpha8"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="Alpha8"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="Alpha8"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is Alpha8) && Equals((Alpha8) obj);
        }

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="Alpha8"/>
        /// and a specified <see cref="Alpha8"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="Alpha8"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="Alpha8"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(Alpha8 other)
        {
            return packedValue == other.packedValue;
        }

        /// <summary>
        /// Returns the string representation of this <see cref="Alpha8"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="Alpha8"/> value.
        /// </returns>
        public override string ToString()
        {
            return (packedValue / 255.0f).ToString();
        }

        /// <summary>
        /// Returns the hash code for this <see cref="Alpha8"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="Alpha8"/> value.
        /// </returns>
        public override int GetHashCode()
        {
            return packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Alpha8"/>
        /// values are equal.
        /// </summary>
        /// <param name="lhs">The <see cref="Alpha8"/> on the left of the equality operator.</param>
        /// <param name="rhs">The <see cref="Alpha8"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Alpha8 lhs, Alpha8 rhs)
        {
            return lhs.packedValue == rhs.packedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Alpha8"/>
        /// values are not equal.
        /// </summary>
        /// <param name="lhs">The <see cref="Alpha8"/> on the left of the inequality operator.</param>
        /// <param name="rhs">The <see cref="Alpha8"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Alpha8 lhs, Alpha8 rhs)
        {
            return lhs.packedValue != rhs.packedValue;
        }

        private static byte Pack(float alpha)
        {
            return (byte) MathF.Round(
                MathHelper.Clamp(alpha, 0, 1) * 255.0f
            );
        }
    }
}
