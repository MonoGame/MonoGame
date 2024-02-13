// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing two 16-bit unsigned normalized values ranging from 0 to 1.
    /// </summary>
    public struct Rg32 : IPackedVector<uint>, IEquatable<Rg32>, IPackedVector
    {
        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="Rg32"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="Rg32"/>
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
        /// Creates a new <see cref="Rg32"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
        public Rg32(float x, float y)
        {
            packedValue = Pack(x, y);
        }

        /// <summary>
        /// Creates a new <see cref="Rg32"/> from the specified <see cref="Vector2"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector2"/> whos components contain the initial values
        /// for this <see cref="Rg32"/>.
        /// </param>
        public Rg32(Vector2 vector)
        {
            packedValue = Pack(vector.X, vector.Y);
        }

        /// <summary>
        /// Expands this <see cref="Rg32"/> to a <see cref="Vector2"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="Rg32"/> as a <see cref="Vector2"/>.
        /// </returns>
        public Vector2 ToVector2()
        {
            return new Vector2(
                (float) ((packedValue & 0xFFFF) / 65535.0f),
                (float)(((packedValue >> 16) & 0xFFFF) / 65535.0f)
            );
        }

        /// <summary>
        /// Sets the packed vector from a Vector4.
        /// </summary>
        /// <param name="vector">Vector containing the components.</param>
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            packedValue = Pack(vector.X, vector.Y);
        }

        /// <summary>
        /// Expands this <see cref="Rg32"/> to a <see cref="Vector4"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="Rg32"/> as a <see cref="Vector4"/>.
        /// </returns>
        public Vector4 ToVector4()
        {
            return new Vector4(ToVector2(), 0.0f, 1.0f);
        }

        /// <summary>
        /// Returns a value that indicates whether this <see cref="Rg32"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="Rg32"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="Rg32"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is Rg32) && Equals((Rg32) obj);
        }

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="Rg32"/>
        /// and a specified <see cref="Rg32"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="Rg32"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="Rg32"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(Rg32 other)
        {
            return packedValue == other.packedValue;
        }

        /// <summary>
        /// Returns the string representation of this <see cref="Rg32"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="Rg32"/> value.
        /// </returns>
        public override string ToString()
        {
            return ToVector2().ToString();
        }

        /// <summary>
        /// Returns the hash code for this <see cref="Rg32"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="Rg32"/> value.
        /// </returns>
        public override int GetHashCode()
        {
            return packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Rg32"/> values
        /// are equal.
        /// </summary>
        /// <param name="lhs">The <see cref="Rg32"/> on the left of the equality operator.</param>
        /// <param name="rhs">The <see cref="Rg32"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Rg32 lhs, Rg32 rhs)
        {
            return lhs.packedValue == rhs.packedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Rg32"/> values
        /// are not equal.
        /// </summary>
        /// <param name="lhs">The <see cref="Rg32"/> on the left of the inequality operator.</param>
        /// <param name="rhs">The <see cref="Rg32"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Rg32 lhs, Rg32 rhs)
        {
            return lhs.packedValue != rhs.packedValue;
        }

        private static uint Pack(float x, float y)
        {
            return (uint) (
                (((int) MathF.Round(MathHelper.Clamp(x, 0, 1) * 65535.0f) & 0xFFFF) ) |
                (((int) MathF.Round(MathHelper.Clamp(y, 0, 1) * 65535.0f) & 0xFFFF) << 16)
            );
        }
    }
}
