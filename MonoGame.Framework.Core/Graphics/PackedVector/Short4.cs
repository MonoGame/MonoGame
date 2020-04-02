// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing four 16-bit signed integer values.
    /// </summary>
    public struct Short4 : IPackedVector<ulong>, IEquatable<Short4>
    {
        ulong packedValue;

        /// <summary>
        /// Initializes a new instance of the Short4 class.
        /// </summary>
        /// <param name="vector">A vector containing the initial values for the components of the Short4 structure.</param>
        public Short4(Vector4 vector)
        {
            packedValue = Pack(ref vector);
        }

        /// <summary>
        /// Initializes a new instance of the Short4 class.
        /// </summary>
        /// <param name="x">Initial value for the x component.</param>
        /// <param name="y">Initial value for the y component.</param>
        /// <param name="z">Initial value for the z component.</param>
        /// <param name="w">Initial value for the w component.</param>
        public Short4(float x, float y, float z, float w)
        {
            var vector = new Vector4(x, y, z, w);
            packedValue = Pack(ref vector);
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are different.
        /// </summary>
        /// <param name="a">The object to the left of the equality operator.</param>
        /// <param name="b">The object to the right of the equality operator.</param>
        /// <returns>true if the objects are different; false otherwise.</returns>
        public static bool operator !=(Short4 a, Short4 b)
        {
            return a.PackedValue != b.PackedValue;
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are the same.
        /// </summary>
        /// <param name="a">The object to the left of the equality operator.</param>
        /// <param name="b">The object to the right of the equality operator.</param>
        /// <returns>true if the objects are the same; false otherwise.</returns>
        public static bool operator ==(Short4 a, Short4 b)
        {
            return a.PackedValue == b.PackedValue;
        }

        /// <summary>
        /// Directly gets or sets the packed representation of the value.
        /// </summary>
        /// <value>The packed representation of the value.</value>
        [CLSCompliant(false)]
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
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object with which to make the comparison.</param>
        /// <returns>true if the current instance is equal to the specified object; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Short4)
                return this == (Short4)obj;
            return false;
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="other">The object with which to make the comparison.</param>
        /// <returns>true if the current instance is equal to the specified object; false otherwise.</returns>
        public bool Equals(Short4 other)
        {
            return this == other;
        }

        /// <summary>
        /// Gets the hash code for the current instance.
        /// </summary>
        /// <returns>Hash code for the instance.</returns>
        public override int GetHashCode()
        {
            return packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of the current instance.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString()
        {
            return packedValue.ToString("x16");
        }

        /// <summary>
        /// Packs a vector into a ulong.
        /// </summary>
        /// <param name="vector">The vector containing the values to pack.</param>
        /// <returns>The ulong containing the packed values.</returns>
        static ulong Pack(ref Vector4 vector)
        {
            const long mask = 0xFFFF;
            const long maxPos = 0x7FFF; // Largest two byte positive number 0xFFFF >> 1;
			const float minNeg = ~(int)maxPos; // two's complement

            // clamp the value between min and max values
            var word4 = ((ulong)((int) Math.Round(MathHelper.Clamp(vector.X, minNeg, maxPos))) & mask);
			var word3 = ((ulong)((int) Math.Round(MathHelper.Clamp(vector.Y, minNeg, maxPos)) & mask)) << 0x10;
			var word2 = ((ulong)((int) Math.Round(MathHelper.Clamp(vector.Z, minNeg, maxPos)) & mask)) << 0x20;
			var word1 = ((ulong)((int) Math.Round(MathHelper.Clamp(vector.W, minNeg, maxPos)) & mask)) << 0x30;

            return word4 | word3 | word2 | word1;
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
        /// Expands the packed representation into a Vector4.
        /// </summary>
        /// <returns>The expanded vector.</returns>
        public Vector4 ToVector4()
        {
            return new Vector4(
                (short)(packedValue & 0xFFFF),
                (short)((packedValue >> 0x10) & 0xFFFF),
                (short)((packedValue >> 0x20) & 0xFFFF),
                (short)((packedValue >> 0x30) & 0xFFFF));
        }
    }
}
