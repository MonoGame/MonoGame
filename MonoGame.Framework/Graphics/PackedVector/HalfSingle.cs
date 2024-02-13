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
        /// Creates a new <see cref="HalfSingle"/> initialized with the specified value.
        /// </summary>
        /// <param name="single">The initial value of the <see cref="HalfSingle"/></param>
        public HalfSingle(float single)
        {
            packedValue = HalfTypeHelper.Convert(single);
        }

        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="HalfSingle"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="HalfSingle"/>
        /// </value>
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
        /// Expands this <see cref="HalfSingle"/> to a <see cref="Single"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="HalfSingle"/> as a <see cref="Single"/>.
        /// </returns>
        public float ToSingle()
        {
            return HalfTypeHelper.Convert(this.packedValue);
        }

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            this.packedValue = HalfTypeHelper.Convert(vector.X);
        }

        /// <summary>
        /// Gets the packed vector in Vector4 format.
        /// </summary>
        /// <returns>The packed vector in Vector4 format</returns>
        public Vector4 ToVector4()
        {
            return new Vector4(this.ToSingle(), 0f, 0f, 1f);
        }

        /// <summary>
        /// Returns a value that indicates whether this <see cref="HalfSingle"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="HalfSingle"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="HalfSingle"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == this.GetType())
            {
                return this == (HalfSingle)obj;
            }

            return false;
        }

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="HalfSingle"/>
        /// and a specified <see cref="HalfSingle"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="HalfSingle"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="HalfSingle"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(HalfSingle other)
        {
            return this.packedValue == other.packedValue;
        }

        /// <summary>
        /// Returns the string representation of this <see cref="HalfSingle"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="HalfSingle"/> value.
        /// </returns>
        public override string ToString()
        {
            return this.ToSingle().ToString();
        }

        /// <summary>
        /// Returns the hash code for this <see cref="HalfSingle"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="HalfSingle"/> value.
        /// </returns>
        public override int GetHashCode()
        {
            return this.packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="HalfSingle"/>
        /// values are equal.
        /// </summary>
        /// <param name="lhs">The <see cref="HalfSingle"/> on the left of the equality operator.</param>
        /// <param name="rhs">The <see cref="HalfSingle"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(HalfSingle lhs, HalfSingle rhs)
        {
            return lhs.packedValue == rhs.packedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="HalfSingle"/>
        /// values are not equal.
        /// </summary>
        /// <param name="lhs">The <see cref="HalfSingle"/> on the left of the inequality operator.</param>
        /// <param name="rhs">The <see cref="HalfSingle"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(HalfSingle lhs, HalfSingle rhs)
        {
            return lhs.packedValue != rhs.packedValue;
        }
    }
}
