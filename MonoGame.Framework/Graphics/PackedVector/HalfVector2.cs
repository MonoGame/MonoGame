// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing two 16-bit floating-point values.
    /// </summary>
    public struct HalfVector2 : IPackedVector<uint>, IPackedVector, IEquatable<HalfVector2>
    {
        private uint packedValue;

        /// <summary>
        /// Creates a new <see cref="HalfVector2"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
        public HalfVector2(float x, float y)
        {
            this.packedValue = PackHelper(x, y);
        }

        /// <summary>
        /// Creates a new <see cref="HalfVector2"/> from the specified <see cref="Vector2"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector2"/> whos components contain the initial values
        /// for this <see cref="HalfVector2"/>.
        /// </param>
        public HalfVector2(Vector2 vector)
        {
            this.packedValue = PackHelper(vector.X, vector.Y);
        }

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            this.packedValue = PackHelper(vector.X, vector.Y);
        }

        private static uint PackHelper(float vectorX, float vectorY)
        {
            uint num2 = HalfTypeHelper.Convert(vectorX);
            uint num = (uint)(HalfTypeHelper.Convert(vectorY) << 0x10);
            return (num2 | num);
        }

        /// <summary>
        /// Expands this <see cref="HalfVector2"/> to a <see cref="Vector2"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="HalfVector2"/> as a <see cref="Vector2"/>.
        /// </returns>
        public Vector2 ToVector2()
        {
            Vector2 vector;
            vector.X = HalfTypeHelper.Convert((ushort)this.packedValue);
            vector.Y = HalfTypeHelper.Convert((ushort)(this.packedValue >> 0x10));
            return vector;
        }

        /// <summary>
        /// Gets the packed vector in Vector4 format.
        /// </summary>
        /// <returns>The packed vector in Vector4 format</returns>
        public Vector4 ToVector4()
        {
            Vector2 vector = this.ToVector2();
            return new Vector4(vector.X, vector.Y, 0f, 1f);
        }

        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="HalfVector2"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="HalfVector2"/>
        /// </value>
        public uint PackedValue
        {
            get
            {
                return this.packedValue;
            }
            set
            {
                this.packedValue = value;
            }
        }

        /// <summary>
        /// Returns the string representation of this <see cref="HalfVector2"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="HalfVector2"/> value.
        /// </returns>
        public override string ToString()
        {
            return this.ToVector2().ToString();
        }

        /// <summary>
        /// Returns the hash code for this <see cref="HalfVector2"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="HalfVector2"/> value.
        /// </returns>
        public override int GetHashCode()
        {
            return this.packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether this <see cref="HalfVector2"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="HalfVector2"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="HalfVector2"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return ((obj is HalfVector2) && this.Equals((HalfVector2)obj));
        }

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="HalfVector2"/>
        /// and a specified <see cref="HalfVector2"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="HalfVector2"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="HalfVector2"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(HalfVector2 other)
        {
            return this.packedValue.Equals(other.packedValue);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="HalfVector2"/>
        /// values are equal.
        /// </summary>
        /// <param name="a">The <see cref="HalfVector2"/> on the left of the equality operator.</param>
        /// <param name="b">The <see cref="HalfVector2"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(HalfVector2 a, HalfVector2 b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="HalfVector2"/>
        /// values are not equal.
        /// </summary>
        /// <param name="a">The <see cref="HalfVector2"/> on the left of the inequality operator.</param>
        /// <param name="b">The <see cref="HalfVector2"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(HalfVector2 a, HalfVector2 b)
        {
            return !a.Equals(b);
        }
    }
}
