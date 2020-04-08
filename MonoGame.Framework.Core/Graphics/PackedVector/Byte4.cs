// MonoGame - Copyright (C) The MonoGame Team
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
        /// Initializes a new instance of the Byte4 class.
        /// </summary>
        /// <param name="vector">A vector containing the initial values for the components of the Byte4 structure.</param>
        public Byte4(Vector4 vector)
        {
            packedValue = Pack(ref vector);
        }

        /// <summary>
        /// Initializes a new instance of the Byte4 class.
        /// </summary>
        /// <param name="x">Initial value for the x component.</param>
        /// <param name="y">Initial value for the y component.</param>
        /// <param name="z">Initial value for the z component.</param>
        /// <param name="w">Initial value for the w component.</param>
        public Byte4(float x, float y, float z, float w)
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
        public static bool operator !=(Byte4 a, Byte4 b)
        {
            return a.PackedValue != b.PackedValue;
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are the same.
        /// </summary>
        /// <param name="a">The object to the left of the equality operator.</param>
        /// <param name="b">The object to the right of the equality operator.</param>
        /// <returns>true if the objects are the same; false otherwise.</returns>
        public static bool operator ==(Byte4 a, Byte4 b)
        {
            return a.PackedValue == b.PackedValue;
        }

        /// <summary>
        /// Directly gets or sets the packed representation of the value.
        /// </summary>
        /// <value>The packed representation of the value.</value>
        [CLSCompliant(false)]
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
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object with which to make the comparison.</param>
        /// <returns>true if the current instance is equal to the specified object; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Byte4)
                return this == (Byte4)obj;
            return false;
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="other">The object with which to make the comparison.</param>
        /// <returns>true if the current instance is equal to the specified object; false otherwise.</returns>
        public bool Equals(Byte4 other)
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
            var byte4 = (uint) Math.Round(MathHelper.Clamp(vector.X, min, max)) & 0xFF;
            var byte3 = ((uint) Math.Round(MathHelper.Clamp(vector.Y, min, max)) & 0xFF) << 0x8;
            var byte2 = ((uint) Math.Round(MathHelper.Clamp(vector.Z, min, max)) & 0xFF) << 0x10;
            var byte1 = ((uint) Math.Round(MathHelper.Clamp(vector.W, min, max)) & 0xFF) << 0x18;

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
        /// Expands the packed representation into a Vector4.
        /// </summary>
        /// <returns>The expanded vector.</returns>
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

