// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing four 16-bit floating-point values.
    /// </summary>
    public struct HalfVector4 : IPackedVector<ulong>, IPackedVector, IEquatable<HalfVector4>
    {
        ulong packedValue;

        /// <summary>
        /// Creates a new <see cref="HalfVector4"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
        /// <param name="z">The initial z-component value.</param>
        /// <param name="w">The initial w-component value.</param>
        public HalfVector4(float x, float y, float z, float w)
        {
            var vector = new Vector4(x, y, z, w);
            packedValue = PackHelper(ref vector);
        }

        /// <summary>
        /// Creates a new <see cref="HalfVector4"/> from the specified <see cref="Vector4"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector4"/> whos components contain the initial values
        /// for this <see cref="HalfVector4"/>.
        /// </param>
        public HalfVector4(Vector4 vector)
        {
            packedValue = PackHelper(ref vector);
        }

        /// <summary>
        /// Sets the packed representation from a Vector4.
        /// </summary>
        /// <param name="vector">The vector to create the packed representation from.</param>
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            packedValue = PackHelper(ref vector);
        }

        /// <summary>
        /// Packs a vector into a ulong.
        /// </summary>
        /// <param name="vector">The vector containing the values to pack.</param>
        /// <returns>The ulong containing the packed values.</returns>
        static ulong PackHelper(ref Vector4 vector)
        {
            ulong num4 = (ulong)HalfTypeHelper.Convert(vector.X);
            ulong num3 = ((ulong)HalfTypeHelper.Convert(vector.Y) << 0x10);
            ulong num2 = ((ulong)HalfTypeHelper.Convert(vector.Z) << 0x20);
            ulong num1 = ((ulong)HalfTypeHelper.Convert(vector.W) << 0x30);
            return num4 | num3 | num2 | num1;
        }

        /// <summary>
        /// Expands this <see cref="HalfVector4"/> to a <see cref="Vector4"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="HalfVector4"/> as a <see cref="Vector4"/>.
        /// </returns>
        public Vector4 ToVector4()
        {
            return new Vector4(
                HalfTypeHelper.Convert((ushort)packedValue),
                HalfTypeHelper.Convert((ushort)(packedValue >> 0x10)),
                HalfTypeHelper.Convert((ushort)(packedValue >> 0x20)),
                HalfTypeHelper.Convert((ushort)(packedValue >> 0x30)));
        }

        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="HalfVector4"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="HalfVector4"/>
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

        /// <summary>
        /// Returns the string representation of this <see cref="HalfVector4"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="HalfVector4"/> value.
        /// </returns>
        public override string ToString()
        {
            return ToVector4().ToString();
        }

        /// <summary>
        /// Returns the hash code for this <see cref="HalfVector4"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="HalfVector4"/> value.
        /// </returns>
        public override int GetHashCode()
        {
            return packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether this <see cref="HalfVector4"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="HalfVector4"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="HalfVector4"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return ((obj is HalfVector4) && Equals((HalfVector4)obj));
        }

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="HalfVector4"/>
        /// and a specified <see cref="HalfVector4"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="HalfVector4"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="HalfVector4"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(HalfVector4 other)
        {
            return packedValue.Equals(other.packedValue);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="HalfVector4"/>
        /// values are equal.
        /// </summary>
        /// <param name="a">The <see cref="HalfVector4"/> on the left of the equality operator.</param>
        /// <param name="b">The <see cref="HalfVector4"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(HalfVector4 a, HalfVector4 b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="HalfVector4"/>
        /// values are not equal.
        /// </summary>
        /// <param name="a">The <see cref="HalfVector4"/> on the left of the inequality operator.</param>
        /// <param name="b">The <see cref="HalfVector4"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(HalfVector4 a, HalfVector4 b)
        {
            return !a.Equals(b);
        }
    }
}
