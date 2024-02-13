// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing two 8-bit signed normalized values, ranging from −1 to 1.
    /// </summary>
    public struct NormalizedByte2 : IPackedVector<ushort>, IEquatable<NormalizedByte2>
    {
        private ushort _packed;

        /// <summary>
        /// Creates a new <see cref="NormalizedByte2"/> from the specified <see cref="Vector2"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector2"/> whos components contain the initial values
        /// for this <see cref="NormalizedByte2"/>.
        /// </param>
        public NormalizedByte2(Vector2 vector)
        {
            _packed = Pack(vector.X, vector.Y);
        }

        /// <summary>
        /// Creates a new <see cref="NormalizedByte2"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
        public NormalizedByte2(float x, float y)
        {
            _packed = Pack(x, y);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="NormalizedByte2"/>
        /// values are not equal.
        /// </summary>
        /// <param name="a">The <see cref="NormalizedByte2"/> on the left of the inequality operator.</param>
        /// <param name="b">The <see cref="NormalizedByte2"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(NormalizedByte2 a, NormalizedByte2 b)
        {
            return a._packed != b._packed;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="NormalizedByte2"/>
        /// values are equal.
        /// </summary>
        /// <param name="a">The <see cref="NormalizedByte2"/> on the left of the equality operator.</param>
        /// <param name="b">The <see cref="NormalizedByte2"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(NormalizedByte2 a, NormalizedByte2 b)
        {
            return a._packed == b._packed;
        }

        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="NormalizedByte2"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="NormalizedByte2"/>
        /// </value>
        public ushort PackedValue
        {
            get
            {
                return _packed;
            }
            set
            {
                _packed = value;
            }
        }

        /// <summary>
        /// Returns a value that indicates whether this <see cref="NormalizedByte2"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="NormalizedByte2"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="NormalizedByte2"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is NormalizedByte2) &&
                    ((NormalizedByte2)obj)._packed == _packed;
        }

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="NormalizedByte2"/>
        /// and a specified <see cref="NormalizedByte2"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="NormalizedByte2"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="NormalizedByte2"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(NormalizedByte2 other)
        {
            return _packed == other._packed;
        }

        /// <summary>
        /// Returns the hash code for this <see cref="NormalizedByte2"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="NormalizedByte2"/> value.
        /// </returns>
        public override int GetHashCode()
        {
            return _packed.GetHashCode();
        }

        /// <summary>
        /// Returns the string representation of this <see cref="NormalizedByte2"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="NormalizedByte2"/> value.
        /// </returns>
        public override string ToString()
        {
            return _packed.ToString("X");
        }

        private static ushort Pack(float x, float y)
        {
            var byte2 = (((ushort) MathF.Round(MathHelper.Clamp(x, -1.0f, 1.0f) * 127.0f)) & 0xFF) << 0;
            var byte1 = (((ushort) MathF.Round(MathHelper.Clamp(y, -1.0f, 1.0f) * 127.0f)) & 0xFF) << 8;

            return (ushort)(byte2 | byte1);
        }

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packed = Pack(vector.X, vector.Y);
        }

        /// <summary>
        /// Gets the packed vector in Vector4 format.
        /// </summary>
        /// <returns>The packed vector in Vector4 format</returns>
        public Vector4 ToVector4()
        {
            return new Vector4(ToVector2(), 0.0f, 1.0f);
        }

        /// <summary>
        /// Expands this <see cref="NormalizedByte2"/> to a <see cref="Vector2"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="NormalizedByte2"/> as a <see cref="Vector2"/>.
        /// </returns>
        public Vector2 ToVector2()
        {
            return new Vector2(
                ((sbyte) ((_packed >> 0) & 0xFF)) / 127.0f,
                ((sbyte) ((_packed >> 8) & 0xFF)) / 127.0f);
        }
    }
}
