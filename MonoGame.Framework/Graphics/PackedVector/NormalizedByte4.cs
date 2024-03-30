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
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector4"/> value who's components contain the initial values for this structure.
        /// </param>
        public NormalizedByte4(Vector4 vector)
        {
            _packed = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="x">The initial x-component value for this structure.</param>
        /// <param name="y">The initial y-component value for this structure.</param>
        /// <param name="z">The initial z-component value for this structure.</param>
        /// <param name="w">The initial 2-component value for this structure.</param>
        public NormalizedByte4(float x, float y, float z, float w)
        {
            _packed = Pack(x, y, z, w);
        }

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="a">The value on the left of the inequality operator.</param>
        /// <param name="b">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
        public static bool operator !=(NormalizedByte4 a, NormalizedByte4 b)
        {
            return a._packed != b._packed;
        }

        /// <summary>
        /// Returns a value that indicates whether the two values are equal.
        /// </summary>
        /// <param name="a">The value on the left of the equality operator.</param>
        /// <param name="b">The value on the right of the equality operator.</param>
        /// <returns>true if the two values are equal; otherwise, false.</returns>
        public static bool operator ==(NormalizedByte4 a, NormalizedByte4 b)
        {
            return a._packed == b._packed;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return  (obj is NormalizedByte4) &&
                    ((NormalizedByte4)obj)._packed == _packed;
        }

        /// <inheritdoc />
        public bool Equals(NormalizedByte4 other)
        {
            return _packed == other._packed;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _packed.GetHashCode();
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
