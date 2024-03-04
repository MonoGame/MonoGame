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
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="x">The initial x-component value for this structure.</param>
        /// <param name="y">The initial y-component value for this structure.</param>
        /// <param name="z">The initial z-component value for this structure.</param>
        /// <param name="w">The initial 2-component value for this structure.</param>
        public Bgra4444(float x, float y, float z, float w)
        {
            _packedValue = Pack(x, y, z, w);
        }

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector4"/> value who's components contain the initial values for this structure.
        /// </param>
        public Bgra4444(Vector4 vector)
        {
            _packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public Vector4 ToVector4()
        {
            const float maxVal = 1 / 15.0f;

            return new Vector4( ((_packedValue >> 8) & 0x0F) * maxVal,
                                ((_packedValue >> 4) & 0x0F) * maxVal,
                                (_packedValue & 0x0F) * maxVal,
                                ((_packedValue >> 12) & 0x0F) * maxVal);
        }

        /// <inheritdoc />
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj != null && (obj is Bgra4444))
                return this == (Bgra4444)obj;
            return false;
        }

        /// <inheritdoc />
        public bool Equals(Bgra4444 other)
        {
            return _packedValue == other._packedValue;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToVector4().ToString();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether the two values are equal.
        /// </summary>
        /// <param name="lhs">The value on the left of the equality operator.</param>
        /// <param name="rhs">The value on the right of the equality operator.</param>
        /// <returns>true if the two values are equal; otherwise, false.</returns>
        public static bool operator ==(Bgra4444 lhs, Bgra4444 rhs)
        {
            return lhs._packedValue == rhs._packedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="lhs">The value on the left of the inequality operator.</param>
        /// <param name="rhs">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
        public static bool operator !=(Bgra4444 lhs, Bgra4444 rhs)
        {
            return lhs._packedValue != rhs._packedValue;
        }
    }
}
