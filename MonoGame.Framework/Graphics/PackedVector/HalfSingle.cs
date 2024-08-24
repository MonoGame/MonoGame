// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing a single 16-bit floating point
    /// </summary>
    public struct HalfSingle : IPackedVector<UInt16>, IEquatable<HalfSingle>, IPackedVector
    {
        UInt16 packedValue;

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="single">TheThe initial value for this structure.</param>
        public HalfSingle(float single)
        {
            packedValue = HalfTypeHelper.Convert(single);
        }

        /// <inheritdoc />
        public ushort PackedValue
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
        /// Expands the packed representation to a <see cref="Single">System.Single</see>
        /// </summary>
        /// <returns>The expanded value.</returns>
        public float ToSingle()
        {
            return HalfTypeHelper.Convert(this.packedValue);
        }

        /// <inheritdoc />
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            this.packedValue = HalfTypeHelper.Convert(vector.X);
        }

        /// <inheritdoc />
        public Vector4 ToVector4()
        {
            return new Vector4(this.ToSingle(), 0f, 0f, 1f);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == this.GetType())
            {
                return this == (HalfSingle)obj;
            }

            return false;
        }

        /// <inheritdoc />
        public bool Equals(HalfSingle other)
        {
            return this.packedValue == other.packedValue;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ToSingle().ToString();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether the two values are equal.
        /// </summary>
        /// <param name="lhs">The value on the left of the equality operator.</param>
        /// <param name="rhs">The value on the right of the equality operator.</param>
        /// <returns>true if the two values are equal; otherwise, false.</returns>
        public static bool operator ==(HalfSingle lhs, HalfSingle rhs)
        {
            return lhs.packedValue == rhs.packedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="lhs">The value on the left of the inequality operator.</param>
        /// <param name="rhs">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
        public static bool operator !=(HalfSingle lhs, HalfSingle rhs)
        {
            return lhs.packedValue != rhs.packedValue;
        }
    }
}
