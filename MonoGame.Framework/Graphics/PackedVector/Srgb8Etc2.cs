// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing unsigned normalized values ranging from 0 to 1.
    /// The x, y and z components use 10 bits, and the w component uses 2 bits.
    /// </summary>
#if XNADESIGNPROVIDED
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    public struct Srgb8Etc2 : IPackedVector<uint>, IEquatable<Srgb8Etc2>, IPackedVector
    {
        /// <inheritdoc />
        public uint PackedValue
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

        private uint _packedValue;

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="x">The initial x-component value for this structure.</param>
        /// <param name="y">The initial y-component value for this structure.</param>
        /// <param name="z">The initial z-component value for this structure.</param>
        /// <param name="w">The initial w-component value for this structure.</param>
        public Srgb8Etc2(float x, float y, float z, float w)
        {
            _packedValue = Pack(x, y, z, w);
        }

        /// <summary>
        /// Initializes a new instance of this structure.
        /// </summary>
        /// <param name="vector">
        /// A <see cref="Vector4"/> value who's components contain the initial values for this structure.
        /// </param>
        public Srgb8Etc2(Vector4 vector)
        {
            _packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <inheritdoc />
        public Vector4 ToVector4()
        {
            return new Vector4(
                (float)(((_packedValue >>  0) & 0xFF) / 255.0f),
                (float)(((_packedValue >>  8) & 0xFF) / 255.0f),
                (float)(((_packedValue >> 16) & 0xFF) / 255.0f),
                1.0f  // Should be fully opaque by default.
            );
        }

        /// <inheritdoc />
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Srgb8Etc2 other && Equals(other);
        }

        /// <inheritdoc />
        public bool Equals(Srgb8Etc2 other)
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
        public static bool operator ==(Srgb8Etc2 lhs, Srgb8Etc2 rhs)
        {
            return lhs._packedValue == rhs._packedValue;
        }

        /// <summary>
        /// Returns a value that indicates whether the two value are not equal.
        /// </summary>
        /// <param name="lhs">The value on the left of the inequality operator.</param>
        /// <param name="rhs">The value on the right of the inequality operator.</param>
        /// <returns>true if the two value are not equal; otherwise, false.</returns>
        public static bool operator !=(Srgb8Etc2 lhs, Srgb8Etc2 rhs)
        {
            return lhs._packedValue != rhs._packedValue;
        }

        private static uint Pack(float x, float y, float z, float _)
        {
            return (uint)(
                (((int)MathF.Round(MathHelper.Clamp(x, 0, 1) * 255.0f) & 0xFF) << 0) |
                (((int)MathF.Round(MathHelper.Clamp(y, 0, 1) * 255.0f) & 0xFF) << 8) |
                (((int)MathF.Round(MathHelper.Clamp(z, 0, 1) * 255.0f) & 0xFF) << 16)
            );
        }
    }
}
