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
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="x">The initial x-component value for this structure.</param>
        /// <param name="y">The initial y-component value for this structure.</param>
        /// <param name="z">The initial z-component value for this structure.</param>
        /// <param name="w">The initial 2-component value for this structure.</param>
        public HalfVector4(float x, float y, float z, float w)
        {
            var vector = new Vector4(x, y, z, w);
            packedValue = PackHelper(ref vector);
        }

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector4"/> value who's components contain the initial values for this structure.
        /// </param>
        public HalfVector4(Vector4 vector)
        {
            packedValue = PackHelper(ref vector);
        }

        /// <inheritdoc />
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            packedValue = PackHelper(ref vector);
        }

        static ulong PackHelper(ref Vector4 vector)
        {
            ulong num4 = (ulong)HalfTypeHelper.Convert(vector.X);
            ulong num3 = ((ulong)HalfTypeHelper.Convert(vector.Y) << 0x10);
            ulong num2 = ((ulong)HalfTypeHelper.Convert(vector.Z) << 0x20);
            ulong num1 = ((ulong)HalfTypeHelper.Convert(vector.W) << 0x30);
            return num4 | num3 | num2 | num1;
        }

        /// <inheritdoc />
        public Vector4 ToVector4()
        {
            return new Vector4(
                HalfTypeHelper.Convert((ushort)packedValue),
                HalfTypeHelper.Convert((ushort)(packedValue >> 0x10)),
                HalfTypeHelper.Convert((ushort)(packedValue >> 0x20)),
                HalfTypeHelper.Convert((ushort)(packedValue >> 0x30)));
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override string ToString()
        {
            return ToVector4().ToString();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return packedValue.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ((obj is HalfVector4) && Equals((HalfVector4)obj));
        }

        /// <inheritdoc />
        public bool Equals(HalfVector4 other)
        {
            return packedValue.Equals(other.packedValue);
        }

        /// <summary>
        /// Returns a value that indicates whether the two values are equal.
        /// </summary>
        /// <param name="a">The value on the left of the equality operator.</param>
        /// <param name="b">The value on the right of the equality operator.</param>
        /// <returns>true if the two values are equal; otherwise, false.</returns>
        public static bool operator ==(HalfVector4 a, HalfVector4 b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="a">The value on the left of the inequality operator.</param>
        /// <param name="b">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
        public static bool operator !=(HalfVector4 a, HalfVector4 b)
        {
            return !a.Equals(b);
        }
    }
}
