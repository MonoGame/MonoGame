// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing four 8-bit signed normalized values, ranging from −1 to 1.
    /// </summary>
    public struct NormalizedByte4 : IPackedVector<uint>, IEquatable<NormalizedByte4>
    {
        private uint _packed;

        /// <summary>
        /// Creates a new <see cref="NormalizedByte4"/> from the specified <see cref="Vector4"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector4"/> whos components contain the initial values
        /// for this <see cref="NormalizedByte4"/>.
        /// </param>
        public NormalizedByte4(Vector4 vector)
        {
            _packed = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <summary>
        /// Creates a new <see cref="NormalizedByte4"/> from the specified component values.
        /// </summary>
        /// <param name="x">The initial x-component value.</param>
        /// <param name="y">The initial y-component value.</param>
        /// <param name="z">The initial z-component value.</param>
        /// <param name="w">The initial w-component value.</param>
        public NormalizedByte4(float x, float y, float z, float w)
        {
            _packed = Pack(x, y, z, w);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="NormalizedByte4"/>
        /// values are not equal.
        /// </summary>
        /// <param name="a">The <see cref="NormalizedByte4"/> on the left of the inequality operator.</param>
        /// <param name="b">The <see cref="NormalizedByte4"/> on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(NormalizedByte4 a, NormalizedByte4 b)
        {
            return a._packed != b._packed;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="NormalizedByte4"/>
        /// values are equal.
        /// </summary>
        /// <param name="a">The <see cref="NormalizedByte4"/> on the left of the equality operator.</param>
        /// <param name="b">The <see cref="NormalizedByte4"/> on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/>
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(NormalizedByte4 a, NormalizedByte4 b)
        {
            return a._packed == b._packed;
        }

        /// <summary>
        /// Gets or Sets the packed representation of this <see cref="NormalizedByte4"/>.
        /// </summary>
        /// <value>
        /// The packed representation of this <see cref="NormalizedByte4"/>
        /// </value>
        public uint PackedValue
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
        /// Returns a value that indicates whether this <see cref="NormalizedByte4"/>
        /// and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="NormalizedByte4"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="NormalizedByte4"/> and
        /// <paramref name="obj"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return  (obj is NormalizedByte4) &&
                    ((NormalizedByte4)obj)._packed == _packed;
        }

        /// <summary>
        /// Returns a value tha indicates whether this <see cref="NormalizedByte4"/>
        /// and a specified <see cref="NormalizedByte4"/> are equal.
        /// </summary>
        /// <param name="other">The other <see cref="NormalizedByte4"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="NormalizedByte4"/> values
        /// are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(NormalizedByte4 other)
        {
            return _packed == other._packed;
        }

        /// <summary>
        /// Returns the hash code for this <see cref="NormalizedByte4"/> value.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer hash code for this <see cref="NormalizedByte4"/> value.
        /// </returns>
        public override int GetHashCode()
        {
            return _packed.GetHashCode();
        }

        /// <summary>
        /// Returns the string representation of this <see cref="NormalizedByte4"/> value.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="NormalizedByte4"/> value.
        /// </returns>
        public override string ToString()
        {
            return _packed.ToString("X");
        }

        private static uint Pack(float x, float y, float z, float w)
        {
            var byte4 = (((uint) MathF.Round(MathHelper.Clamp(x, -1.0f, 1.0f) * 127.0f)) & 0xff) << 0;
            var byte3 = (((uint) MathF.Round(MathHelper.Clamp(y, -1.0f, 1.0f) * 127.0f)) & 0xff) << 8;
            var byte2 = (((uint) MathF.Round(MathHelper.Clamp(z, -1.0f, 1.0f) * 127.0f)) & 0xff) << 16;
            var byte1 = (((uint) MathF.Round(MathHelper.Clamp(w, -1.0f, 1.0f) * 127.0f)) & 0xff) << 24;

            return byte4 | byte3 | byte2 | byte1;
        }

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packed = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <summary>
        /// Expands this <see cref="NormalizedByte4"/> to a <see cref="Vector4"/>
        /// </summary>
        /// <returns>
        /// The expanded <see cref="NormalizedByte4"/> as a <see cref="Vector4"/>.
        /// </returns>
        public Vector4 ToVector4()
        {
            return new Vector4(
                ((sbyte) ((_packed >> 0) & 0xFF)) / 127.0f,
                ((sbyte) ((_packed >> 8) & 0xFF)) / 127.0f,
                ((sbyte) ((_packed >> 16) & 0xFF)) / 127.0f,
                ((sbyte) ((_packed >> 24) & 0xFF)) / 127.0f);
        }
    }
}
