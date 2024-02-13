// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing four 8-bit unsigned integer values, ranging from 0 to 255.
    /// </summary>
    public struct Byte4 : IPackedVector<uint>, IEquatable<Byte4>, IPackedVector
    {
        uint packedValue;

        /// <summary>
        /// Creates a new <see cref="Byte4"/> from the specified <see cref="Vector4"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector4"/> whos components contain the initial values
        /// for this <see cref="Byte4"/>.
        /// </param>
        public Byte4(Vector4 vector)
        {
            packedValue = Pack(ref vector);
        }

        /// <summary>
        /// Creates a new <see cref="Byte4"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
        /// <param name="z">The initial z-component value.</param>
        /// <param name="w">The initial w-component value.</param>
        public Byte4(float x, float y, float z, float w)
        {
            var vector = new Vector4(x, y, z, w);
            packedValue = Pack(ref vector);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Byte4"/>
        /// values are not equal.
        /// </summary>
        /// <param name="a">The <see cref="Byte4"/> on the left of the inequality operator.</param>
        /// <param name="b">The <see cref="Byte4"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Byte4 a, Byte4 b)
        {
            return a.PackedValue != b.PackedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Byte4"/>
        /// values are equal.
        /// </summary>
        /// <param name="a">The <see cref="Byte4"/> on the left of the equality operator.</param>
        /// <param name="b">The <see cref="Byte4"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Byte4 a, Byte4 b)
        {
            return a.PackedValue == b.PackedValue;
        }

        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="Byte4"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="Byte4"/>
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

        /// <summary>
        /// Returns a value that indicates whether this <see cref="Byte4"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="Byte4"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="Byte4"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Byte4)
                return this == (Byte4)obj;
            return false;
        }

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="Byte4"/>
        /// and a specified <see cref="Byte4"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="Byte4"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="Byte4"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(Byte4 other)
        {
            return this == other;
        }

        /// <summary>
        /// Returns the hash code for this <see cref="Byte4"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="Byte4"/> value.
        /// </returns>
        public override int GetHashCode()
        {
            return packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns the string representation of this <see cref="Byte4"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="Byte4"/> value.
        /// </returns>
        public override string ToString()
        {
            return packedValue.ToString("x8");
        }

        /// <summary>
        /// Packs a vector into a uint.
        /// </summary>
        /// <param name="vector">The vector containing the values to pack.</param>
        /// <returns>The ulong containing the packed values.</returns>
        static uint Pack(ref Vector4 vector)
        {
            const float max = 255.0f;
            const float min = 0.0f;

            // clamp the value between min and max values
            var byte4 = (uint) MathF.Round(MathHelper.Clamp(vector.X, min, max)) & 0xFF;
            var byte3 = ((uint) MathF.Round(MathHelper.Clamp(vector.Y, min, max)) & 0xFF) << 0x8;
            var byte2 = ((uint) MathF.Round(MathHelper.Clamp(vector.Z, min, max)) & 0xFF) << 0x10;
            var byte1 = ((uint) MathF.Round(MathHelper.Clamp(vector.W, min, max)) & 0xFF) << 0x18;

            return byte4 | byte3 | byte2 | byte1;
        }

        /// <summary>
        /// Sets the packed representation from a Vector4.
        /// </summary>
        /// <param name="vector">The vector to create the packed representation from.</param>
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            packedValue = Pack(ref vector);
        }

        /// <summary>
        /// Expands this <see cref="Byte4"/> to a <see cref="Vector4"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="Byte4"/> as a <see cref="Vector4"/>.
        /// </returns>
        public Vector4 ToVector4()
        {
            return new Vector4(
                (float)(packedValue & 0xFF),
                (float)((packedValue >> 0x8) & 0xFF),
                (float)((packedValue >> 0x10) & 0xFF),
                (float)((packedValue >> 0x18) & 0xFF));
        }
    }
}

