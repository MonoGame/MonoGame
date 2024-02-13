// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing unsigned normalized values, ranging from 0 to 1, using 4 bits each for x, y, z, and w.
    /// </summary>
    public struct Bgra4444 : IPackedVector<UInt16>, IEquatable<Bgra4444>
    {
        UInt16 _packedValue;

        private static UInt16 Pack(float x, float y, float z, float w)
        {
            return (UInt16) ((((int) MathF.Round(MathHelper.Clamp(w, 0, 1) * 15.0f) & 0x0F) << 12) |
                (((int) MathF.Round(MathHelper.Clamp(x, 0, 1) * 15.0f) & 0x0F) << 8) |
                (((int) MathF.Round(MathHelper.Clamp(y, 0, 1) * 15.0f) & 0x0F) << 4) |
                ((int) MathF.Round(MathHelper.Clamp(z, 0, 1) * 15.0f) & 0x0F));
        }

        /// <summary>
        /// Creates a new <see cref="Bgra4444"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
        /// <param name="z">The initial z-component value.</param>
        /// <param name="w">The initial w-component value.</param>
        public Bgra4444(float x, float y, float z, float w)
        {
            _packedValue = Pack(x, y, z, w);
        }

        /// <summary>
        /// Creates a new <see cref="Bgra4444"/> from the specified <see cref="Vector4"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector4"/> whos components contain the initial values
        /// for this <see cref="Bgra4444"/>.
        /// </param>
        public Bgra4444(Vector4 vector)
        {
            _packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="Bgra4444"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="Bgra4444"/>
        /// </value>
        public UInt16 PackedValue
        {
            get
            {
                return _packedValue;
            }
            set
            {
                _packedValue = value;
            }
        }

        /// <summary>
        /// Expands this <see cref="Bgra4444"/> to a <see cref="Vector4"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="Bgra4444"/> as a <see cref="Vector4"/>.
        /// </returns>
        public Vector4 ToVector4()
        {
            const float maxVal = 1 / 15.0f;

            return new Vector4( ((_packedValue >> 8) & 0x0F) * maxVal,
                                ((_packedValue >> 4) & 0x0F) * maxVal,
                                (_packedValue & 0x0F) * maxVal,
                                ((_packedValue >> 12) & 0x0F) * maxVal);
        }

        /// <summary>
        /// Sets the packed vector from a Vector4.
        /// </summary>
        /// <param name="vector">Vector containing the components.</param>
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <summary>
        /// Returns a value that indicates whether this <see cref="Bgra4444"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="Bgra4444"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="Bgra4444"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj != null && (obj is Bgra4444))
                return this == (Bgra4444)obj;
            return false;
        }

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="Bgra4444"/>
        /// and a specified <see cref="Bgra4444"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="Bgra4444"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="Bgra4444"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(Bgra4444 other)
        {
            return _packedValue == other._packedValue;
        }

        /// <summary>
        /// Returns the string representation of this <see cref="Bgra4444"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="Bgra4444"/> value.
        /// </returns>
        public override string ToString()
        {
            return ToVector4().ToString();
        }

        /// <summary>
        /// Returns the hash code for this <see cref="Bgra4444"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="Bgra4444"/> value.
        /// </returns>
        public override int GetHashCode()
        {
            return _packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Bgra4444"/> values are equal.
        /// </summary>
        /// <param name="lhs">The <see cref="Bgra4444"/> on the left of the equality operator.</param>
        /// <param name="rhs">The <see cref="Bgra4444"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/> are equal; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Bgra4444 lhs, Bgra4444 rhs)
        {
            return lhs._packedValue == rhs._packedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Bgra4444"/> values are not equal.
        /// </summary>
        /// <param name="lhs">The <see cref="Bgra4444"/> on the left of the inequality operator.</param>
        /// <param name="rhs">The <see cref="Bgra4444"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/> are different; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Bgra4444 lhs, Bgra4444 rhs)
        {
            return lhs._packedValue != rhs._packedValue;
        }
    }
}
