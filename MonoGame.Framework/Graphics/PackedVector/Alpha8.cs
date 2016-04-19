// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Packed vector type containing a single 8 bit normalized W values that is ranging from 0 to 1.
    /// </summary>
    public struct Alpha8 : IPackedVector<byte>, IEquatable<Alpha8>, IPackedVector
    {
        private byte packedValue;

        /// <summary>
        /// Gets and sets the packed value.
        /// </summary>
        [CLSCompliant(false)]
        public byte PackedValue
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

        /// <summary>
        /// Creates a new instance of Alpha8.
        /// </summary>
        /// <param name="alpha">The alpha component</param>
        public Alpha8(float alpha)
        {
            packedValue = Pack(alpha);
        }

        /// <summary>
        /// Gets the packed vector in float format.
        /// </summary>
        /// <returns>The packed vector in Vector3 format</returns>
        public float ToAlpha()
        {
            return (float) (packedValue / 255.0f);
        }

        /// <summary>
        /// Sets the packed vector from a Vector4.
        /// </summary>
        /// <param name="vector">Vector containing the components.</param>
        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            packedValue = Pack(vector.W);
        }

        /// <summary>
        /// Gets the packed vector in Vector4 format.
        /// </summary>
        /// <returns>The packed vector in Vector4 format</returns>
        Vector4 IPackedVector.ToVector4()
        {
            return new Vector4(
                0.0f,
                0.0f,
                0.0f,
                (float) (packedValue / 255.0f)
            );
        }

        /// <summary>
        /// Compares an object with the packed vector.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if the object is equal to the packed vector.</returns>
        public override bool Equals(object obj)
        {
            return (obj is Alpha8) && Equals((Alpha8) obj);
        }

        /// <summary>
        /// Compares another Alpha8 packed vector with the packed vector.
        /// </summary>
        /// <param name="other">The Alpha8 packed vector to compare.</param>
        /// <returns>True if the packed vectors are equal.</returns>
        public bool Equals(Alpha8 other)
        {
            return packedValue == other.packedValue;
        }

        /// <summary>
        /// Gets a string representation of the packed vector.
        /// </summary>
        /// <returns>A string representation of the packed vector.</returns>
        public override string ToString()
        {
            return (packedValue / 255.0f).ToString();
        }

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        /// <returns>The hash code for the packed vector.</returns>
        public override int GetHashCode()
        {
            return packedValue.GetHashCode();
        }

        public static bool operator ==(Alpha8 lhs, Alpha8 rhs)
        {
            return lhs.packedValue == rhs.packedValue;
        }

        public static bool operator !=(Alpha8 lhs, Alpha8 rhs)
        {
            return lhs.packedValue != rhs.packedValue;
        }

        private static byte Pack(float alpha)
        {
            return (byte) Math.Round(
                MathHelper.Clamp(alpha, 0, 1) * 255.0f
            );
        }
    }
}
