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
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector4"/> value who's components contain the initial values for this structure.
        /// </param>
        public Byte4(Vector4 vector)
        {
            packedValue = Pack(ref vector);
        }

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="x">The initial x-component value for this structure.</param>
        /// <param name="y">The initial y-component value for this structure.</param>
        /// <param name="z">The initial z-component value for this structure.</param>
        /// <param name="w">The initial 2-component value for this structure.</param>
        public Byte4(float x, float y, float z, float w)
        {
            var vector = new Vector4(x, y, z, w);
            packedValue = Pack(ref vector);
        }

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="a">The value on the left of the inequality operator.</param>
        /// <param name="b">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
        public static bool operator !=(Byte4 a, Byte4 b)
        {
            return a.PackedValue != b.PackedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether the two values are equal.
        /// </summary>
        /// <param name="a">The value on the left of the equality operator.</param>
        /// <param name="b">The value on the right of the equality operator.</param>
        /// <returns>true if the two values are equal; otherwise, false.</returns>
        public static bool operator ==(Byte4 a, Byte4 b)
        {
            return a.PackedValue == b.PackedValue;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is Byte4)
                return this == (Byte4)obj;
            return false;
        }

        /// <inheritdoc />
        public bool Equals(Byte4 other)
        {
            return this == other;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return packedValue.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return packedValue.ToString("x8");
        }

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

        /// <inheritdoc />
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            packedValue = Pack(ref vector);
        }

        /// <inheritdoc />
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

