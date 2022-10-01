// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Describes a 3D-vector.
    /// </summary>
#if XNADESIGNPROVIDED
    [System.ComponentModel.TypeConverter(typeof(Microsoft.Xna.Framework.Design.Vector3IntTypeConverter))]
#endif
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Vector3Int : IEquatable<Vector3Int>
    {
        #region Private Fields

        private static readonly Vector3Int zero = new Vector3Int(0f, 0f, 0f);
        private static readonly Vector3Int one = new Vector3Int(1f, 1f, 1f);
        private static readonly Vector3Int unitX = new Vector3Int(1f, 0f, 0f);
        private static readonly Vector3Int unitY = new Vector3Int(0f, 1f, 0f);
        private static readonly Vector3Int unitZ = new Vector3Int(0f, 0f, 1f);
        private static readonly Vector3Int up = new Vector3Int(0f, 1f, 0f);
        private static readonly Vector3Int down = new Vector3Int(0f, -1f, 0f);
        private static readonly Vector3Int right = new Vector3Int(1f, 0f, 0f);
        private static readonly Vector3Int left = new Vector3Int(-1f, 0f, 0f);
        private static readonly Vector3Int forward = new Vector3Int(0f, 0f, -1f);
        private static readonly Vector3Int backward = new Vector3Int(0f, 0f, 1f);

        #endregion

        #region Public Fields

        /// <summary>
        /// The x coordinate of this <see cref="Vector3Int"/>.
        /// </summary>
        [DataMember]
        public int X;

        /// <summary>
        /// The y coordinate of this <see cref="Vector3Int"/>.
        /// </summary>
        [DataMember]
        public int Y;

        /// <summary>
        /// The z coordinate of this <see cref="Vector3Int"/>.
        /// </summary>
        [DataMember]
        public int Z;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns a <see cref="Vector3Int"/> with components 0, 0, 0.
        /// </summary>
        public static Vector3Int Zero
        {
            get { return zero; }
        }

        /// <summary>
        /// Returns a <see cref="Vector3Int"/> with components 1, 1, 1.
        /// </summary>
        public static Vector3Int One
        {
            get { return one; }
        }

        /// <summary>
        /// Returns a <see cref="Vector3Int"/> with components 1, 0, 0.
        /// </summary>
        public static Vector3Int UnitX
        {
            get { return unitX; }
        }

        /// <summary>
        /// Returns a <see cref="Vector3Int"/> with components 0, 1, 0.
        /// </summary>
        public static Vector3Int UnitY
        {
            get { return unitY; }
        }

        /// <summary>
        /// Returns a <see cref="Vector3Int"/> with components 0, 0, 1.
        /// </summary>
        public static Vector3Int UnitZ
        {
            get { return unitZ; }
        }

        /// <summary>
        /// Returns a <see cref="Vector3Int"/> with components 0, 1, 0.
        /// </summary>
        public static Vector3Int Up
        {
            get { return up; }
        }

        /// <summary>
        /// Returns a <see cref="Vector3Int"/> with components 0, -1, 0.
        /// </summary>
        public static Vector3Int Down
        {
            get { return down; }
        }

        /// <summary>
        /// Returns a <see cref="Vector3Int"/> with components 1, 0, 0.
        /// </summary>
        public static Vector3Int Right
        {
            get { return right; }
        }

        /// <summary>
        /// Returns a <see cref="Vector3Int"/> with components -1, 0, 0.
        /// </summary>
        public static Vector3Int Left
        {
            get { return left; }
        }

        /// <summary>
        /// Returns a <see cref="Vector3Int"/> with components 0, 0, -1.
        /// </summary>
        public static Vector3Int Forward
        {
            get { return forward; }
        }

        /// <summary>
        /// Returns a <see cref="Vector3Int"/> with components 0, 0, 1.
        /// </summary>
        public static Vector3Int Backward
        {
            get { return backward; }
        }

        #endregion

        #region Internal Properties

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    this.X.ToString(), "  ",
                    this.Y.ToString(), "  ",
                    this.Z.ToString()
                );
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a 3d vector with X, Y and Z from three values.
        /// </summary>
        /// <param name="x">The x coordinate in 3d-space.</param>
        /// <param name="y">The y coordinate in 3d-space.</param>
        /// <param name="z">The z coordinate in 3d-space.</param>
        public Vector3Int(float x, float y, float z)
            : this((int) x, (int) y, (int)z)
        {

        }

        public Vector3Int(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        /// <summary>
        /// Constructs a 3d vector with X, Y and Z set to the same value.
        /// </summary>
        /// <param name="value">The x, y and z coordinates in 3d-space.</param>
        public Vector3Int(int value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
        }

        /// <summary>
        /// Constructs a 3d vector with X, Y from <see cref="Vector2"/> and Z from a scalar.
        /// </summary>
        /// <param name="value">The x and y coordinates in 3d-space.</param>
        /// <param name="z">The z coordinate in 3d-space.</param>
        public Vector3Int(Vector2Int value, int z)
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Z = z;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs vector addition on <paramref name="value1"/> and <paramref name="value2"/>.
        /// </summary>
        /// <param name="value1">The first vector to add.</param>
        /// <param name="value2">The second vector to add.</param>
        /// <returns>The result of the vector addition.</returns>
        public static Vector3Int Add(Vector3Int value1, Vector3Int value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        /// <summary>
        /// Performs vector addition on <paramref name="value1"/> and
        /// <paramref name="value2"/>, storing the result of the
        /// addition in <paramref name="result"/>.
        /// </summary>
        /// <param name="value1">The first vector to add.</param>
        /// <param name="value2">The second vector to add.</param>
        /// <param name="result">The result of the vector addition.</param>
        public static void Add(ref Vector3Int value1, ref Vector3Int value2, out Vector3Int result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 3d-triangle.
        /// </summary>
        /// <param name="value1">The first vector of 3d-triangle.</param>
        /// <param name="value2">The second vector of 3d-triangle.</param>
        /// <param name="value3">The third vector of 3d-triangle.</param>
        /// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 3d-triangle.</param>
        /// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 3d-triangle.</param>
        /// <returns>The cartesian translation of barycentric coordinates.</returns>
        public static Vector3Int Barycentric(Vector3Int value1, Vector3Int value2, Vector3Int value3, float amount1, float amount2)
        {
            return new Vector3Int(
                MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2),
                MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2),
                MathHelper.Barycentric(value1.Z, value2.Z, value3.Z, amount1, amount2));
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 3d-triangle.
        /// </summary>
        /// <param name="value1">The first vector of 3d-triangle.</param>
        /// <param name="value2">The second vector of 3d-triangle.</param>
        /// <param name="value3">The third vector of 3d-triangle.</param>
        /// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 3d-triangle.</param>
        /// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 3d-triangle.</param>
        /// <param name="result">The cartesian translation of barycentric coordinates as an output parameter.</param>
        public static void Barycentric(ref Vector3Int value1, ref Vector3Int value2, ref Vector3Int value3, float amount1, float amount2, out Vector3Int result)
        {
            result.X = (int)MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2);
            result.Y = (int)MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2);
            result.Z = (int)MathHelper.Barycentric(value1.Z, value2.Z, value3.Z, amount1, amount2);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains CatmullRom interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector in interpolation.</param>
        /// <param name="value2">The second vector in interpolation.</param>
        /// <param name="value3">The third vector in interpolation.</param>
        /// <param name="value4">The fourth vector in interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The result of CatmullRom interpolation.</returns>
        public static Vector3Int CatmullRom(Vector3Int value1, Vector3Int value2, Vector3Int value3, Vector3Int value4, float amount)
        {
            return new Vector3Int(
                MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount),
                MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount),
                MathHelper.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount));
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains CatmullRom interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector in interpolation.</param>
        /// <param name="value2">The second vector in interpolation.</param>
        /// <param name="value3">The third vector in interpolation.</param>
        /// <param name="value4">The fourth vector in interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <param name="result">The result of CatmullRom interpolation as an output parameter.</param>
        public static void CatmullRom(ref Vector3Int value1, ref Vector3Int value2, ref Vector3Int value3, ref Vector3Int value4, float amount, out Vector3Int result)
        {
            result.X = (int)MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount);
            result.Y = (int)MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount);
            result.Z = (int)MathHelper.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount);
        }

        

        /// <summary>
        /// Clamps the specified value within a range.
        /// </summary>
        /// <param name="value1">The value to clamp.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <returns>The clamped value.</returns>
        public static Vector3Int Clamp(Vector3Int value1, Vector3Int min, Vector3Int max)
        {
            return new Vector3Int(
                MathHelper.Clamp(value1.X, min.X, max.X),
                MathHelper.Clamp(value1.Y, min.Y, max.Y),
                MathHelper.Clamp(value1.Z, min.Z, max.Z));
        }

        /// <summary>
        /// Clamps the specified value within a range.
        /// </summary>
        /// <param name="value1">The value to clamp.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <param name="result">The clamped value as an output parameter.</param>
        public static void Clamp(ref Vector3Int value1, ref Vector3Int min, ref Vector3Int max, out Vector3Int result)
        {
            result.X = MathHelper.Clamp(value1.X, min.X, max.X);
            result.Y = MathHelper.Clamp(value1.Y, min.Y, max.Y);
            result.Z = MathHelper.Clamp(value1.Z, min.Z, max.Z);
        }

        /// <summary>
        /// Computes the cross product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The cross product of two vectors.</returns>
        public static Vector3Int Cross(Vector3Int vector1, Vector3Int vector2)
        {
            Cross(ref vector1, ref vector2, out vector1);
            return vector1;
        }

        /// <summary>
        /// Computes the cross product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <param name="result">The cross product of two vectors as an output parameter.</param>
        public static void Cross(ref Vector3Int vector1, ref Vector3Int vector2, out Vector3Int result)
        {
            var x = vector1.Y * vector2.Z - vector2.Y * vector1.Z;
            var y = -(vector1.X * vector2.Z - vector2.X * vector1.Z);
            var z = vector1.X * vector2.Y - vector2.X * vector1.Y;
            result.X = x;
            result.Y = y;
            result.Z = z;
        }

        /// <summary>
        /// Returns the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between two vectors.</returns>
        public static float Distance(Vector3Int value1, Vector3Int value2)
        {
            float result;
            DistanceSquared(ref value1, ref value2, out result);
            return (float)Math.Sqrt(result);
        }

        /// <summary>
        /// Returns the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The distance between two vectors as an output parameter.</param>
        public static void Distance(ref Vector3Int value1, ref Vector3Int value2, out float result)
        {
            DistanceSquared(ref value1, ref value2, out result);
            result = (float)Math.Sqrt(result);
        }

        /// <summary>
        /// Returns the squared distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The squared distance between two vectors.</returns>
        public static float DistanceSquared(Vector3Int value1, Vector3Int value2)
        {
            return (value1.X - value2.X) * (value1.X - value2.X) +
                    (value1.Y - value2.Y) * (value1.Y - value2.Y) +
                    (value1.Z - value2.Z) * (value1.Z - value2.Z);
        }

        /// <summary>
        /// Returns the squared distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The squared distance between two vectors as an output parameter.</param>
        public static void DistanceSquared(ref Vector3Int value1, ref Vector3Int value2, out float result)
        {
            result = (value1.X - value2.X) * (value1.X - value2.X) +
                     (value1.Y - value2.Y) * (value1.Y - value2.Y) +
                     (value1.Z - value2.Z) * (value1.Z - value2.Z);
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector3Int"/> by the components of another <see cref="Vector3Int"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/>.</param>
        /// <param name="value2">Divisor <see cref="Vector3Int"/>.</param>
        /// <returns>The result of dividing the vectors.</returns>
        public static Vector3Int Divide(Vector3Int value1, Vector3Int value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector3Int"/> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/>.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        public static Vector3Int Divide(Vector3Int value1, float divider)
        {
            float factor = 1 / divider;
            value1.X = (int)((float)value1.X * factor);
            value1.Y = (int)((float)value1.Y * factor);
            value1.Z = (int)((float)value1.Z * factor);
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector3Int"/> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/>.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <param name="result">The result of dividing a vector by a scalar as an output parameter.</param>
        public static void Divide(ref Vector3Int value1, float divider, out Vector3Int result)
        {
            float factor = 1 / divider;
            result.X = (int)((float)value1.X * factor);
            result.Y = (int)((float)value1.Y * factor);
            result.Z = (int)((float)value1.Z * factor);
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector3Int"/> by the components of another <see cref="Vector3Int"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/>.</param>
        /// <param name="value2">Divisor <see cref="Vector3Int"/>.</param>
        /// <param name="result">The result of dividing the vectors as an output parameter.</param>
        public static void Divide(ref Vector3Int value1, ref Vector3Int value2, out Vector3Int result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
        }

        /// <summary>
        /// Returns a dot product of two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The dot product of two vectors.</returns>
        public static float Dot(Vector3Int value1, Vector3Int value2)
        {
            return value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
        }

        /// <summary>
        /// Returns a dot product of two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The dot product of two vectors as an output parameter.</param>
        public static void Dot(ref Vector3Int value1, ref Vector3Int value2, out float result)
        {
            result = value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Vector3Int))
                return false;

            var other = (Vector3Int)obj;
            return X == other.X &&
                    Y == other.Y &&
                    Z == other.Z;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Vector3Int"/>.
        /// </summary>
        /// <param name="other">The <see cref="Vector3Int"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Vector3Int other)
        {
            return X == other.X &&
                    Y == other.Y &&
                    Z == other.Z;
        }

        

        /// <summary>
        /// Gets the hash code of this <see cref="Vector3Int"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Vector3Int"/>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains hermite spline interpolation.
        /// </summary>
        /// <param name="value1">The first position vector.</param>
        /// <param name="tangent1">The first tangent vector.</param>
        /// <param name="value2">The second position vector.</param>
        /// <param name="tangent2">The second tangent vector.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The hermite spline interpolation vector.</returns>
        public static Vector3Int Hermite(Vector3Int value1, Vector3Int tangent1, Vector3Int value2, Vector3Int tangent2, float amount)
        {
            return new Vector3Int(MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount),
                               MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount),
                               MathHelper.Hermite(value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount));
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains hermite spline interpolation.
        /// </summary>
        /// <param name="value1">The first position vector.</param>
        /// <param name="tangent1">The first tangent vector.</param>
        /// <param name="value2">The second position vector.</param>
        /// <param name="tangent2">The second tangent vector.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <param name="result">The hermite spline interpolation vector as an output parameter.</param>
        public static void Hermite(ref Vector3Int value1, ref Vector3Int tangent1, ref Vector3Int value2, ref Vector3Int tangent2, float amount, out Vector3Int result)
        {
            result.X = (int)MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
            result.Y = (int)MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
            result.Z = (int)MathHelper.Hermite(value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount);
        }

        /// <summary>
        /// Returns the length of this <see cref="Vector3Int"/>.
        /// </summary>
        /// <returns>The length of this <see cref="Vector3Int"/>.</returns>
        public float Length()
        {
            return (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
        }

        /// <summary>
        /// Returns the squared length of this <see cref="Vector3Int"/>.
        /// </summary>
        /// <returns>The squared length of this <see cref="Vector3Int"/>.</returns>
        public float LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains linear interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <returns>The result of linear interpolation of the specified vectors.</returns>
        public static Vector3Int Lerp(Vector3Int value1, Vector3Int value2, float amount)
        {
            return new Vector3Int(
                MathHelper.Lerp(value1.X, value2.X, amount),
                MathHelper.Lerp(value1.Y, value2.Y, amount),
                MathHelper.Lerp(value1.Z, value2.Z, amount));
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains linear interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
        public static void Lerp(ref Vector3Int value1, ref Vector3Int value2, float amount, out Vector3Int result)
        {
            result.X = (int)MathHelper.Lerp(value1.X, value2.X, amount);
            result.Y = (int)MathHelper.Lerp(value1.Y, value2.Y, amount);
            result.Z = (int)MathHelper.Lerp(value1.Z, value2.Z, amount);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains linear interpolation of the specified vectors.
        /// Uses <see cref="MathHelper.LerpPrecise"/> on MathHelper for the interpolation.
        /// Less efficient but more precise compared to <see cref="Vector3Int.Lerp(Vector3Int, Vector3Int, float)"/>.
        /// See remarks section of <see cref="MathHelper.LerpPrecise"/> on MathHelper for more info.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <returns>The result of linear interpolation of the specified vectors.</returns>
        public static Vector3Int LerpPrecise(Vector3Int value1, Vector3Int value2, float amount)
        {
            return new Vector3Int(
                MathHelper.LerpPrecise(value1.X, value2.X, amount),
                MathHelper.LerpPrecise(value1.Y, value2.Y, amount),
                MathHelper.LerpPrecise(value1.Z, value2.Z, amount));
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains linear interpolation of the specified vectors.
        /// Uses <see cref="MathHelper.LerpPrecise"/> on MathHelper for the interpolation.
        /// Less efficient but more precise compared to <see cref="Vector3Int.Lerp(ref Vector3Int, ref Vector3Int, float, out Vector3Int)"/>.
        /// See remarks section of <see cref="MathHelper.LerpPrecise"/> on MathHelper for more info.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
        public static void LerpPrecise(ref Vector3Int value1, ref Vector3Int value2, float amount, out Vector3Int result)
        {
            result.X = (int)MathHelper.LerpPrecise(value1.X, value2.X, amount);
            result.Y = (int)MathHelper.LerpPrecise(value1.Y, value2.Y, amount);
            result.Z = (int)MathHelper.LerpPrecise(value1.Z, value2.Z, amount);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a maximal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The <see cref="Vector3Int"/> with maximal values from the two vectors.</returns>
        public static Vector3Int Max(Vector3Int value1, Vector3Int value2)
        {
            return new Vector3Int(
                MathHelper.Max(value1.X, value2.X),
                MathHelper.Max(value1.Y, value2.Y),
                MathHelper.Max(value1.Z, value2.Z));
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a maximal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The <see cref="Vector3Int"/> with maximal values from the two vectors as an output parameter.</param>
        public static void Max(ref Vector3Int value1, ref Vector3Int value2, out Vector3Int result)
        {
            result.X = MathHelper.Max(value1.X, value2.X);
            result.Y = MathHelper.Max(value1.Y, value2.Y);
            result.Z = MathHelper.Max(value1.Z, value2.Z);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a minimal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The <see cref="Vector3Int"/> with minimal values from the two vectors.</returns>
        public static Vector3Int Min(Vector3Int value1, Vector3Int value2)
        {
            return new Vector3Int(
                MathHelper.Min(value1.X, value2.X),
                MathHelper.Min(value1.Y, value2.Y),
                MathHelper.Min(value1.Z, value2.Z));
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a minimal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The <see cref="Vector3Int"/> with minimal values from the two vectors as an output parameter.</param>
        public static void Min(ref Vector3Int value1, ref Vector3Int value2, out Vector3Int result)
        {
            result.X = MathHelper.Min(value1.X, value2.X);
            result.Y = MathHelper.Min(value1.Y, value2.Y);
            result.Z = MathHelper.Min(value1.Z, value2.Z);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a multiplication of two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/>.</param>
        /// <param name="value2">Source <see cref="Vector3Int"/>.</param>
        /// <returns>The result of the vector multiplication.</returns>
        public static Vector3Int Multiply(Vector3Int value1, Vector3Int value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a multiplication of <see cref="Vector3Int"/> and a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/>.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <returns>The result of the vector multiplication with a scalar.</returns>
        public static Vector3Int Multiply(Vector3Int value1, float scaleFactor)
        {
            value1.X = (int)((float)value1.X * scaleFactor);
            value1.Y = (int)((float)value1.Y * scaleFactor);
            value1.Z = (int)((float)value1.Z * scaleFactor);
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a multiplication of <see cref="Vector3Int"/> and a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/>.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
        public static void Multiply(ref Vector3Int value1, float scaleFactor, out Vector3Int result)
        {
            result.X = (int)((float)value1.X * scaleFactor);
            result.Y = (int)((float)value1.Y * scaleFactor);
            result.Z = (int)((float)value1.Z * scaleFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a multiplication of two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/>.</param>
        /// <param name="value2">Source <see cref="Vector3Int"/>.</param>
        /// <param name="result">The result of the vector multiplication as an output parameter.</param>
        public static void Multiply(ref Vector3Int value1, ref Vector3Int value2, out Vector3Int result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains the specified vector inversion.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3Int"/>.</param>
        /// <returns>The result of the vector inversion.</returns>
        public static Vector3Int Negate(Vector3Int value)
        {
            value = new Vector3Int(-value.X, -value.Y, -value.Z);
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains the specified vector inversion.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3Int"/>.</param>
        /// <param name="result">The result of the vector inversion as an output parameter.</param>
        public static void Negate(ref Vector3Int value, out Vector3Int result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
        }

        /// <summary>
        /// Turns this <see cref="Vector3Int"/> to a unit vector with the same direction.
        /// </summary>
        public void Normalize()
        {
            float factor = (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
            factor = 1f / factor;
            X = (int)((float)X * factor);
            Y = (int)((float)Y * factor);
            Z = (int)((float)Z * factor);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a normalized values from another vector.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3Int"/>.</param>
        /// <returns>Unit vector.</returns>
        public static Vector3Int Normalize(Vector3Int value)
        {
            float factor = (float)Math.Sqrt((value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z));
            factor = 1f / factor;
            return new Vector3Int(value.X * factor, value.Y * factor, value.Z * factor);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a normalized values from another vector.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3Int"/>.</param>
        /// <param name="result">Unit vector as an output parameter.</param>
        public static void Normalize(ref Vector3Int value, out Vector3Int result)
        {
            float factor = (float)Math.Sqrt((value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z));
            factor = 1f / factor;
            result.X = (int)((float)value.X * factor);
            result.Y = (int)((float)value.Y * factor);
            result.Z = (int)((float)value.Z * factor);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains reflect vector of the given vector and normal.
        /// </summary>
        /// <param name="vector">Source <see cref="Vector3Int"/>.</param>
        /// <param name="normal">Reflection normal.</param>
        /// <returns>Reflected vector.</returns>
        public static Vector3Int Reflect(Vector3Int vector, Vector3Int normal)
        {
            // I is the original array
            // N is the normal of the incident plane
            // R = I - (2 * N * ( DotProduct[ I,N] ))
            Vector3Int reflectedVector;
            // inline the dotProduct here instead of calling method
            float dotProduct = ((vector.X * normal.X) + (vector.Y * normal.Y)) + (vector.Z * normal.Z);
            reflectedVector.X = (int)((float)vector.X - (2.0f * normal.X) * dotProduct);
            reflectedVector.Y = (int)((float)vector.Y - (2.0f * normal.Y) * dotProduct);
            reflectedVector.Z = (int)((float)vector.Z - (2.0f * normal.Z) * dotProduct);

            return reflectedVector;
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains reflect vector of the given vector and normal.
        /// </summary>
        /// <param name="vector">Source <see cref="Vector3Int"/>.</param>
        /// <param name="normal">Reflection normal.</param>
        /// <param name="result">Reflected vector as an output parameter.</param>
        public static void Reflect(ref Vector3Int vector, ref Vector3Int normal, out Vector3Int result)
        {
            // I is the original array
            // N is the normal of the incident plane
            // R = I - (2 * N * ( DotProduct[ I,N] ))

            // inline the dotProduct here instead of calling method
            float dotProduct = ((vector.X * normal.X) + (vector.Y * normal.Y)) + (vector.Z * normal.Z);
            result.X = (int)((float)vector.X - (2.0f * normal.X) * dotProduct);
            result.Y = (int)((float)vector.Y - (2.0f * normal.Y) * dotProduct);
            result.Z = (int)((float)vector.Z - (2.0f * normal.Z) * dotProduct);
        }

        /// <summary>
        /// Round the members of this <see cref="Vector3Int"/> towards the nearest integer value.
        /// </summary>
        public void Round()
        {
            X = (int)Math.Round((float)X);
            Y = (int)Math.Round((float)Y);
            Z = (int)Math.Round((float)Z);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains members from another vector rounded to the nearest integer value.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3Int"/>.</param>
        /// <returns>The rounded <see cref="Vector3Int"/>.</returns>
        public static Vector3Int Round(Vector3Int value)
        {
            value.X = (int)Math.Round((float)value.X);
            value.Y = (int)Math.Round((float)value.Y);
            value.Z = (int)Math.Round((float)value.Z);
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains members from another vector rounded to the nearest integer value.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3Int"/>.</param>
        /// <param name="result">The rounded <see cref="Vector3Int"/>.</param>
        public static void Round(ref Vector3Int value, out Vector3Int result)
        {
            result.X = (int)Math.Round((float)value.X);
            result.Y = (int)Math.Round((float)value.Y);
            result.Z = (int)Math.Round((float)value.Z);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains cubic interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/>.</param>
        /// <param name="value2">Source <see cref="Vector3Int"/>.</param>
        /// <param name="amount">Weighting value.</param>
        /// <returns>Cubic interpolation of the specified vectors.</returns>
        public static Vector3Int SmoothStep(Vector3Int value1, Vector3Int value2, float amount)
        {
            return new Vector3Int(
                MathHelper.SmoothStep(value1.X, value2.X, amount),
                MathHelper.SmoothStep(value1.Y, value2.Y, amount),
                MathHelper.SmoothStep(value1.Z, value2.Z, amount));
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains cubic interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/>.</param>
        /// <param name="value2">Source <see cref="Vector3Int"/>.</param>
        /// <param name="amount">Weighting value.</param>
        /// <param name="result">Cubic interpolation of the specified vectors as an output parameter.</param>
        public static void SmoothStep(ref Vector3Int value1, ref Vector3Int value2, float amount, out Vector3Int result)
        {
            result.X = (int)MathHelper.SmoothStep(value1.X, value2.X, amount);
            result.Y = (int)MathHelper.SmoothStep(value1.Y, value2.Y, amount);
            result.Z = (int)MathHelper.SmoothStep(value1.Z, value2.Z, amount);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains subtraction of on <see cref="Vector3Int"/> from a another.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/>.</param>
        /// <param name="value2">Source <see cref="Vector3Int"/>.</param>
        /// <returns>The result of the vector subtraction.</returns>
        public static Vector3Int Subtract(Vector3Int value1, Vector3Int value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains subtraction of on <see cref="Vector3Int"/> from a another.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/>.</param>
        /// <param name="value2">Source <see cref="Vector3Int"/>.</param>
        /// <param name="result">The result of the vector subtraction as an output parameter.</param>
        public static void Subtract(ref Vector3Int value1, ref Vector3Int value2, out Vector3Int result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="Vector3Int"/> in the format:
        /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Z:[<see cref="Z"/>]}
        /// </summary>
        /// <returns>A <see cref="String"/> representation of this <see cref="Vector3Int"/>.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(32);
            sb.Append("{X:");
            sb.Append(this.X);
            sb.Append(" Y:");
            sb.Append(this.Y);
            sb.Append(" Z:");
            sb.Append(this.Z);
            sb.Append("}");
            return sb.ToString();
        }

        #region Transform

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a transformation of 3d-vector by the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="position">Source <see cref="Vector3Int"/>.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <returns>Transformed <see cref="Vector3Int"/>.</returns>
        public static Vector3Int Transform(Vector3Int position, Matrix matrix)
        {
            Transform(ref position, ref matrix, out position);
            return position;
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a transformation of 3d-vector by the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="position">Source <see cref="Vector3Int"/>.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="result">Transformed <see cref="Vector3Int"/> as an output parameter.</param>
        public static void Transform(ref Vector3Int position, ref Matrix matrix, out Vector3Int result)
        {
            var x = (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41;
            var y = (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42;
            var z = (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43;
            result.X = (int)x;
            result.Y = (int)y;
            result.Z = (int)z;
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a transformation of 3d-vector by the specified <see cref="Quaternion"/>, representing the rotation.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3Int"/>.</param>
        /// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
        /// <returns>Transformed <see cref="Vector3Int"/>.</returns>
        public static Vector3Int Transform(Vector3Int value, Quaternion rotation)
        {
            Vector3Int result;
            Transform(ref value, ref rotation, out result);
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a transformation of 3d-vector by the specified <see cref="Quaternion"/>, representing the rotation.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3Int"/>.</param>
        /// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
        /// <param name="result">Transformed <see cref="Vector3Int"/> as an output parameter.</param>
        public static void Transform(ref Vector3Int value, ref Quaternion rotation, out Vector3Int result)
        {
            float x = 2 * (rotation.Y * value.Z - rotation.Z * value.Y);
            float y = 2 * (rotation.Z * value.X - rotation.X * value.Z);
            float z = 2 * (rotation.X * value.Y - rotation.Y * value.X);

            result.X = (int)((float)value.X + x * rotation.W + (rotation.Y * z - rotation.Z * y));
            result.Y = (int)((float)value.Y + y * rotation.W + (rotation.Z * x - rotation.X * z));
            result.Z = (int)((float)value.Z + z * rotation.W + (rotation.X * y - rotation.Y * x));
        }

        /// <summary>
        /// Apply transformation on vectors within array of <see cref="Vector3Int"/> by the specified <see cref="Matrix"/> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="destinationArray">Destination array.</param>
        /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector3Int"/> should be written.</param>
        /// <param name="length">The number of vectors to be transformed.</param>
        public static void Transform(Vector3Int[] sourceArray, int sourceIndex, ref Matrix matrix, Vector3Int[] destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");
            if (sourceArray.Length < sourceIndex + length)
                throw new ArgumentException("Source array length is lesser than sourceIndex + length");
            if (destinationArray.Length < destinationIndex + length)
                throw new ArgumentException("Destination array length is lesser than destinationIndex + length");

            // TODO: Are there options on some platforms to implement a vectorized version of this?

            for (var i = 0; i < length; i++)
            {
                var position = sourceArray[sourceIndex + i];
                destinationArray[destinationIndex + i] =
                    new Vector3Int(
                        (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41,
                        (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42,
                        (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43);
            }
        }

        /// <summary>
        /// Apply transformation on vectors within array of <see cref="Vector3Int"/> by the specified <see cref="Quaternion"/> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
        /// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
        /// <param name="destinationArray">Destination array.</param>
        /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector3Int"/> should be written.</param>
        /// <param name="length">The number of vectors to be transformed.</param>
        public static void Transform(Vector3Int[] sourceArray, int sourceIndex, ref Quaternion rotation, Vector3Int[] destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");
            if (sourceArray.Length < sourceIndex + length)
                throw new ArgumentException("Source array length is lesser than sourceIndex + length");
            if (destinationArray.Length < destinationIndex + length)
                throw new ArgumentException("Destination array length is lesser than destinationIndex + length");

            // TODO: Are there options on some platforms to implement a vectorized version of this?

            for (var i = 0; i < length; i++)
            {
                var position = sourceArray[sourceIndex + i];

                float x = 2 * (rotation.Y * position.Z - rotation.Z * position.Y);
                float y = 2 * (rotation.Z * position.X - rotation.X * position.Z);
                float z = 2 * (rotation.X * position.Y - rotation.Y * position.X);

                destinationArray[destinationIndex + i] =
                    new Vector3Int(
                        position.X + x * rotation.W + (rotation.Y * z - rotation.Z * y),
                        position.Y + y * rotation.W + (rotation.Z * x - rotation.X * z),
                        position.Z + z * rotation.W + (rotation.X * y - rotation.Y * x));
            }
        }

        /// <summary>
        /// Apply transformation on all vectors within array of <see cref="Vector3Int"/> by the specified <see cref="Matrix"/> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="destinationArray">Destination array.</param>
        public static void Transform(Vector3Int[] sourceArray, ref Matrix matrix, Vector3Int[] destinationArray)
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");
            if (destinationArray.Length < sourceArray.Length)
                throw new ArgumentException("Destination array length is lesser than source array length");

            // TODO: Are there options on some platforms to implement a vectorized version of this?

            for (var i = 0; i < sourceArray.Length; i++)
            {
                var position = sourceArray[i];
                destinationArray[i] =
                    new Vector3Int(
                        (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41,
                        (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42,
                        (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43);
            }
        }

        /// <summary>
        /// Apply transformation on all vectors within array of <see cref="Vector3Int"/> by the specified <see cref="Quaternion"/> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
        /// <param name="destinationArray">Destination array.</param>
        public static void Transform(Vector3Int[] sourceArray, ref Quaternion rotation, Vector3Int[] destinationArray)
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");
            if (destinationArray.Length < sourceArray.Length)
                throw new ArgumentException("Destination array length is lesser than source array length");

            // TODO: Are there options on some platforms to implement a vectorized version of this?

            for (var i = 0; i < sourceArray.Length; i++)
            {
                var position = sourceArray[i];

                float x = 2 * (rotation.Y * position.Z - rotation.Z * position.Y);
                float y = 2 * (rotation.Z * position.X - rotation.X * position.Z);
                float z = 2 * (rotation.X * position.Y - rotation.Y * position.X);

                destinationArray[i] =
                    new Vector3Int(
                        position.X + x * rotation.W + (rotation.Y * z - rotation.Z * y),
                        position.Y + y * rotation.W + (rotation.Z * x - rotation.X * z),
                        position.Z + z * rotation.W + (rotation.X * y - rotation.Y * x));
            }
        }

        #endregion

        #region TransformNormal

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a transformation of the specified normal by the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="normal">Source <see cref="Vector3Int"/> which represents a normal vector.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <returns>Transformed normal.</returns>
        public static Vector3Int TransformNormal(Vector3Int normal, Matrix matrix)
        {
            TransformNormal(ref normal, ref matrix, out normal);
            return normal;
        }

        /// <summary>
        /// Creates a new <see cref="Vector3Int"/> that contains a transformation of the specified normal by the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="normal">Source <see cref="Vector3Int"/> which represents a normal vector.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="result">Transformed normal as an output parameter.</param>
        public static void TransformNormal(ref Vector3Int normal, ref Matrix matrix, out Vector3Int result)
        {
            var x = (normal.X * matrix.M11) + (normal.Y * matrix.M21) + (normal.Z * matrix.M31);
            var y = (normal.X * matrix.M12) + (normal.Y * matrix.M22) + (normal.Z * matrix.M32);
            var z = (normal.X * matrix.M13) + (normal.Y * matrix.M23) + (normal.Z * matrix.M33);
            result.X = (int)x;
            result.Y = (int)y;
            result.Z = (int)z;
        }

        /// <summary>
        /// Apply transformation on normals within array of <see cref="Vector3Int"/> by the specified <see cref="Matrix"/> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="destinationArray">Destination array.</param>
        /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector3Int"/> should be written.</param>
        /// <param name="length">The number of normals to be transformed.</param>
        public static void TransformNormal(Vector3Int[] sourceArray,
         int sourceIndex,
         ref Matrix matrix,
         Vector3Int[] destinationArray,
         int destinationIndex,
         int length)
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");
            if (sourceArray.Length < sourceIndex + length)
                throw new ArgumentException("Source array length is lesser than sourceIndex + length");
            if (destinationArray.Length < destinationIndex + length)
                throw new ArgumentException("Destination array length is lesser than destinationIndex + length");

            for (int x = 0; x < length; x++)
            {
                var normal = sourceArray[sourceIndex + x];

                destinationArray[destinationIndex + x] =
                     new Vector3Int(
                        (normal.X * matrix.M11) + (normal.Y * matrix.M21) + (normal.Z * matrix.M31),
                        (normal.X * matrix.M12) + (normal.Y * matrix.M22) + (normal.Z * matrix.M32),
                        (normal.X * matrix.M13) + (normal.Y * matrix.M23) + (normal.Z * matrix.M33));
            }
        }

        /// <summary>
        /// Apply transformation on all normals within array of <see cref="Vector3Int"/> by the specified <see cref="Matrix"/> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="destinationArray">Destination array.</param>
        public static void TransformNormal(Vector3Int[] sourceArray, ref Matrix matrix, Vector3Int[] destinationArray)
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");
            if (destinationArray.Length < sourceArray.Length)
                throw new ArgumentException("Destination array length is lesser than source array length");

            for (var i = 0; i < sourceArray.Length; i++)
            {
                var normal = sourceArray[i];

                destinationArray[i] =
                    new Vector3Int(
                        (normal.X * matrix.M11) + (normal.Y * matrix.M21) + (normal.Z * matrix.M31),
                        (normal.X * matrix.M12) + (normal.Y * matrix.M22) + (normal.Z * matrix.M32),
                        (normal.X * matrix.M13) + (normal.Y * matrix.M23) + (normal.Z * matrix.M33));
            }
        }

        #endregion

        /// <summary>
        /// Deconstruction method for <see cref="Vector3Int"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Deconstruct(out float x, out float y, out float z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Compares whether two <see cref="Vector3Int"/> instances are equal.
        /// </summary>
        /// <param name="value1"><see cref="Vector3Int"/> instance on the left of the equal sign.</param>
        /// <param name="value2"><see cref="Vector3Int"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Vector3Int value1, Vector3Int value2)
        {
            return value1.X == value2.X
                && value1.Y == value2.Y
                && value1.Z == value2.Z;
        }

        /// <summary>
        /// Compares whether two <see cref="Vector3Int"/> instances are not equal.
        /// </summary>
        /// <param name="value1"><see cref="Vector3Int"/> instance on the left of the not equal sign.</param>
        /// <param name="value2"><see cref="Vector3Int"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
        public static bool operator !=(Vector3Int value1, Vector3Int value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/> on the left of the add sign.</param>
        /// <param name="value2">Source <see cref="Vector3Int"/> on the right of the add sign.</param>
        /// <returns>Sum of the vectors.</returns>
        public static Vector3Int operator +(Vector3Int value1, Vector3Int value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        /// <summary>
        /// Inverts values in the specified <see cref="Vector3Int"/>.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3Int"/> on the right of the sub sign.</param>
        /// <returns>Result of the inversion.</returns>
        public static Vector3Int operator -(Vector3Int value)
        {
            value = new Vector3Int(-value.X, -value.Y, -value.Z);
            return value;
        }

        /// <summary>
        /// Subtracts a <see cref="Vector3Int"/> from a <see cref="Vector3Int"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="Vector3Int"/> on the right of the sub sign.</param>
        /// <returns>Result of the vector subtraction.</returns>
        public static Vector3Int operator -(Vector3Int value1, Vector3Int value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of two vectors by each other.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/> on the left of the mul sign.</param>
        /// <param name="value2">Source <see cref="Vector3Int"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication.</returns>
        public static Vector3Int operator *(Vector3Int value1, Vector3Int value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3Int"/> on the left of the mul sign.</param>
        /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector3Int operator *(Vector3Int value, float scaleFactor)
        {
            value.X = (int)((float)value.X * scaleFactor);
            value.Y = (int)((float)value.Y * scaleFactor);
            value.Z = (int)((float)value.Z * scaleFactor);
            return value;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
        /// <param name="value">Source <see cref="Vector3Int"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector3Int operator *(float scaleFactor, Vector3Int value)
        {
            value.X = (int)((float)value.X * scaleFactor);
            value.Y = (int)((float)value.Y * scaleFactor);
            value.Z = (int)((float)value.Z * scaleFactor);
            return value;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector3Int"/> by the components of another <see cref="Vector3Int"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/> on the left of the div sign.</param>
        /// <param name="value2">Divisor <see cref="Vector3Int"/> on the right of the div sign.</param>
        /// <returns>The result of dividing the vectors.</returns>
        public static Vector3Int operator /(Vector3Int value1, Vector3Int value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector3Int"/> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3Int"/> on the left of the div sign.</param>
        /// <param name="divider">Divisor scalar on the right of the div sign.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        public static Vector3Int operator /(Vector3Int value1, float divider)
        {
            float factor = 1 / divider;
            value1.X = (int)((float)value1.X * factor);
            value1.Y = (int)((float)value1.Y * factor);
            value1.Z = (int)((float)value1.Z * factor);
            return value1;
        }

        #endregion
    }
}
