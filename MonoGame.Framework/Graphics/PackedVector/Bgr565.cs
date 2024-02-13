// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing unsigned normalized values ranging from 0 to 1. The x and z components use 5 bits, and the y component uses 6 bits.
    /// </summary>
    public struct Bgr565 : IPackedVector<UInt16>, IEquatable<Bgr565>, IPackedVector
    {
        UInt16 _packedValue;

        private static UInt16 Pack(float x, float y, float z)
        {
            return (UInt16) ((((int) MathF.Round(MathHelper.Clamp(x, 0, 1) * 31.0f) & 0x1F) << 11) |
                (((int) MathF.Round(MathHelper.Clamp(y, 0, 1) * 63.0f) & 0x3F) << 5) |
                ((int) MathF.Round(MathHelper.Clamp(z, 0, 1) * 31.0f) & 0x1F));
        }

        /// <summary>
        /// Creates a new <see cref="Bgr565"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
        /// <param name="z">The initial z-component value.</param>
        public Bgr565(float x, float y, float z)
        {
            _packedValue = Pack(x, y, z);
        }

        /// <summary>
        /// Creates a new <see cref="Bgr565"/> from the specified <see cref="Vector3"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector3"/> whos components contain the initial values
        /// for this <see cref="Bgr565"/>.
        /// </param>
        public Bgr565(Vector3 vector)
        {
            _packedValue = Pack(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="Bgr565"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="Bgr565"/>
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
        /// Expands this <see cref="Bgr565"/> to a <see cref="Vector3"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="Bgr565"/> as a <see cref="Vector3"/>.
        /// </returns>
        public Vector3 ToVector3()
        {
            return new Vector3((float)(((_packedValue >> 11) & 0x1F) * (1.0f / 31.0f)),
                (float)(((_packedValue >> 5) & 0x3F) * (1.0f / 63.0f)),
                (float)((_packedValue & 0x1F) * (1.0f / 31.0f))
                );
        }

        /// <summary>
        /// Sets the packed vector from a Vector4.
        /// </summary>
        /// <param name="vector">Vector containing the components.</param>
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = (UInt16)((((int)(vector.X * 31.0f) & 0x1F) << 11) |
                (((int)(vector.Y * 63.0f) & 0x3F) << 5) |
                ((int)(vector.Z * 31.0f) & 0x1F));
        }

        /// <summary>
        /// Expands this <see cref="Bgr565"/> to a <see cref="Vector4"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="Bgr565"/> as a <see cref="Vector4"/>.
        /// </returns>
        public Vector4 ToVector4()
        {
            return new Vector4(ToVector3(), 1.0f);
        }

        /// <summary>
        /// Returns a value that indicates whether this <see cref="Bgr565"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="Bgr565"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="Bgr565"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj != null && (obj is Bgr565))
                return this == (Bgr565)obj;
            return false;
        }

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="Bgr565"/>
        /// and a specified <see cref="Bgr565"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="Bgr565"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="Bgr565"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(Bgr565 other)
        {
            return _packedValue == other._packedValue;
        }

        /// <summary>
        /// Returns the string representation of this <see cref="Bgr565"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="Bgr565"/> value.
        /// </returns>
        public override string ToString()
        {
            return ToVector3().ToString();
        }

        /// <summary>
        /// Returns the hash code for this <see cref="Bgr565"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="Bgr565"/> value.
        /// </returns>
        public override int GetHashCode()
        {
            return _packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Bgr565"/> values are equal.
        /// </summary>
        /// <param name="lhs">The <see cref="Bgr565"/> on the left of the equality operator.</param>
        /// <param name="rhs">The <see cref="Bgr565"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/> are equal; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Bgr565 lhs, Bgr565 rhs)
        {
            return lhs._packedValue == rhs._packedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Bgr565"/> values are not equal.
        /// </summary>
        /// <param name="lhs">The <see cref="Bgr565"/> on the left of the inequality operator.</param>
        /// <param name="rhs">The <see cref="Bgr565"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/> are different; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Bgr565 lhs, Bgr565 rhs)
        {
            return lhs._packedValue != rhs._packedValue;
        }
    }
}
