// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Describes a 3D point on an integer grid.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Point3 : IEquatable<Point3>
    {
        #region Private Fields

        private static readonly Point3 zero = new Point3(0, 0, 0);
        private static readonly Point3 one = new Point3(1, 1, 1);
        private static readonly Point3 negOne = new Point3(-1, -1, -1);
        private static readonly Point3 unitX = new Point3(1, 0, 0);
        private static readonly Point3 unitY = new Point3(0, 1, 0);
        private static readonly Point3 unitZ = new Point3(0, 0, 1);
        private static readonly Point3 up = new Point3(0, 1, 0);
        private static readonly Point3 down = new Point3(0, -1, 0);
        private static readonly Point3 right = new Point3(1, 0, 0);
        private static readonly Point3 left = new Point3(-1, 0, 0);
        private static readonly Point3 forward = new Point3(0, 0, -1);
        private static readonly Point3 backward = new Point3(0, 0, 1);

        #endregion

        #region Public Fields

        /// <summary>
        /// The x coordinate of this <see cref="Point3"/>.
        /// </summary>
        [DataMember]
        public int X;

        /// <summary>
        /// The y coordinate of this <see cref="Point3"/>.
        /// </summary>
        [DataMember]
        public int Y;

        /// <summary>
        /// The z coordinate of this <see cref="Point3"/>.
        /// </summary>
        [DataMember]
        public int Z;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns a <see cref="Point3"/> with components 0, 0, 0.
        /// </summary>
        public static Point3 Zero
        {
            get { return zero; }
        }

        /// <summary>
        /// Returns a <see cref="Point3"/> with components 1, 1, 1.
        /// </summary>
        public static Point3 One
        {
            get { return one; }
        }

        /// <summary>
        /// Returns a <see cref="Point3"/> with components -1, -1, -1.
        /// </summary>
        public static Point3 NegOne
        {
            get { return negOne; }
        }

        /// <summary>
        /// Returns a <see cref="Point3"/> with components 1, 0, 0.
        /// </summary>
        public static Point3 UnitX
        {
            get { return unitX; }
        }

        /// <summary>
        /// Returns a <see cref="Point3"/> with components 0, 1, 0.
        /// </summary>
        public static Point3 UnitY
        {
            get { return unitY; }
        }

        /// <summary>
        /// Returns a <see cref="Point3"/> with components 0, 0, 1.
        /// </summary>
        public static Point3 UnitZ
        {
            get { return unitZ; }
        }

        /// <summary>
        /// Returns a <see cref="Point3"/> with components 0, 1, 0.
        /// </summary>
        public static Point3 Up
        {
            get { return up; }
        }

        /// <summary>
        /// Returns a <see cref="Point3"/> with components 0, -1, 0.
        /// </summary>
        public static Point3 Down
        {
            get { return down; }
        }

        /// <summary>
        /// Returns a <see cref="Point3"/> with components 1, 0, 0.
        /// </summary>
        public static Point3 Right
        {
            get { return right; }
        }

        /// <summary>
        /// Returns a <see cref="Point3"/> with components -1, 0, 0.
        /// </summary>
        public static Point3 Left
        {
            get { return left; }
        }

        /// <summary>
        /// Returns a <see cref="Point3"/> with components 0, 0, -1.
        /// </summary>
        public static Point3 Forward
        {
            get { return forward; }
        }

        /// <summary>
        /// Returns a <see cref="Point3"/> with components 0, 0, 1.
        /// </summary>
        public static Point3 Backward
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
        /// Constructs a point with X, Y, and Z from three values.
        /// </summary>
        /// <param name="x">The x coordinate in 3D-space.</param>
        /// <param name="y">The y coordinate in 3D-space.</param>
        /// <param name="z">The z coordinate in 3D-space.</param>
        public Point3(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Constructs a point with X, Y, and Z set to the same value.
        /// </summary>
        /// <param name="value">The x, y, and z coordinates in 3D-space.</param>
        public Point3(int value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Adds two points.
        /// </summary>
        /// <param name="value1">Source <see cref="Point3"/> on the left of the add sign.</param>
        /// <param name="value2">Source <see cref="Point3"/> on the right of the add sign.</param>
        /// <returns>Sum of the points.</returns>
        public static Point3 operator +(Point3 value1, Point3 value2)
        {
            return new Point3(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z);
        }

        /// <summary>
        /// Subtracts a <see cref="Point3"/> from a <see cref="Point3"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Point3"/> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="Point3"/> on the right of the sub sign.</param>
        /// <returns>Result of the subtraction.</returns>
        public static Point3 operator -(Point3 value1, Point3 value2)
        {
            return new Point3(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z);
        }

        /// <summary>
        /// Multiplies the components of two points by each other.
        /// </summary>
        /// <param name="value1">Source <see cref="Point3"/> on the left of the mul sign.</param>
        /// <param name="value2">Source <see cref="Point3"/> on the right of the mul sign.</param>
        /// <returns>Result of the multiplication.</returns>
        public static Point3 operator *(Point3 value1, Point3 value2)
        {
            return new Point3(value1.X * value2.X, value1.Y * value2.Y, value1.Z * value2.Z);
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="value">Source <see cref="Point3"/> on the left of the mul sign.</param>
        /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Point3 operator *(Point3 value, int scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
        /// <param name="value">Source <see cref="Point3"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Point3 operator *(int scaleFactor, Point3 value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Divides the components of a <see cref="Point3"/> by the components of another <see cref="Point3"/>.
        /// </summary>
        /// <param name="source">Source <see cref="Point3"/> on the left of the div sign.</param>
        /// <param name="divisor">Divisor <see cref="Point3"/> on the right of the div sign.</param>
        /// <returns>The result of dividing the points.</returns>
        public static Point3 operator /(Point3 source, Point3 divisor)
        {
            return new Point3(source.X / divisor.X, source.Y / divisor.Y, source.Z / divisor.Z);
        }

        /// <summary>
        /// Divides the components of a <see cref="Point3"/> by a specified divisor.
        /// </summary>
        /// <param name="source">Source <see cref="Point3"/> on the left of the div sign.</param>
        /// <param name="divisor">Divisor on the right of the div sign.</param>
        /// <returns>The result of dividing the point.</returns>
        public static Point3 operator /(Point3 source, int divisor)
        {
            return new Point3(source.X / divisor, source.Y / divisor, source.Z / divisor);
        }

        /// <summary>
        /// Compares whether two <see cref="Point3"/> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="Point3"/> instance on the left of the equal sign.</param>
        /// <param name="b"><see cref="Point3"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Point3 a, Point3 b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compares whether two <see cref="Point3"/> instances are not equal.
        /// </summary>
        /// <param name="a"><see cref="Point3"/> instance on the left of the not equal sign.</param>
        /// <param name="b"><see cref="Point3"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
        public static bool operator !=(Point3 a, Point3 b)
        {
            return !a.Equals(b);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Performs point addition on <paramref name="value1"/> and <paramref name="value2"/>.
        /// </summary>
        /// <param name="value1">The first point to add.</param>
        /// <param name="value2">The second point to add.</param>
        /// <returns>The result of the point addition.</returns>
        public static Point3 Add(Point3 value1, Point3 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        /// <summary>
        /// Performs point addition on <paramref name="value1"/> and
        /// <paramref name="value2"/>, storing the result of the
        /// addition in <paramref name="result"/>.
        /// </summary>
        /// <param name="value1">The first point to add.</param>
        /// <param name="value2">The second point to add.</param>
        /// <param name="result">The result of the point addition.</param>
        public static void Add(ref Point3 value1, ref Point3 value2, out Point3 result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
        }

        /// <summary>
        /// Clamps the specified value within a range.
        /// </summary>
        /// <param name="value1">The value to clamp.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <returns>The clamped value.</returns>
        public static Point3 Clamp(Point3 value1, Point3 min, Point3 max)
        {
            return new Point3(
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
        public static void Clamp(ref Point3 value1, ref Point3 min, ref Point3 max, out Point3 result)
        {
            result.X = MathHelper.Clamp(value1.X, min.X, max.X);
            result.Y = MathHelper.Clamp(value1.Y, min.Y, max.Y);
            result.Z = MathHelper.Clamp(value1.Z, min.Z, max.Z);
        }

        /// <summary>
        /// Returns the distance between two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The distance between two points.</returns>
        public static float Distance(Point3 value1, Point3 value2)
        {
            int distsq = DistanceSquared(value1, value2);
            return (float)Math.Sqrt(distsq);
        }

        /// <summary>
        /// Returns the distance between two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <param name="result">The distance between two points as an output parameter.</param>
        public static void Distance(ref Point3 value1, ref Point3 value2, out float result)
        {
            int distsq = DistanceSquared(value1, value2);
            result = (float)Math.Sqrt(distsq);
        }

        /// <summary>
        /// Returns the squared distance between two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The squared distance between two points.</returns>
        public static int DistanceSquared(Point3 value1, Point3 value2)
        {
            return  (value1.X - value2.X) * (value1.X - value2.X) +
                    (value1.Y - value2.Y) * (value1.Y - value2.Y) +
                    (value1.Z - value2.Z) * (value1.Z - value2.Z);
        }

        /// <summary>
        /// Returns the squared distance between two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <param name="result">The squared distance between two points as an output parameter.</param>
        public static void DistanceSquared(ref Point3 value1, ref Point3 value2, out int result)
        {
            result = (value1.X - value2.X) * (value1.X - value2.X) +
                     (value1.Y - value2.Y) * (value1.Y - value2.Y) +
                     (value1.Z - value2.Z) * (value1.Z - value2.Z);
        }

        /// <summary>
        /// Divides the components of a <see cref="Point3"/> by the components of another <see cref="Point3"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Point3"/>.</param>
        /// <param name="value2">Divisor <see cref="Point3"/>.</param>
        /// <returns>The result of dividing the points.</returns>
        public static Point3 Divide(Point3 value1, Point3 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Point3"/> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Point3"/>.</param>
        /// <param name="divisor">Divisor scalar.</param>
        /// <returns>The result of dividing a point by a scalar.</returns>
        public static Point3 Divide(Point3 value1, int divisor)
        {
            value1.X /= divisor;
            value1.Y /= divisor;
            value1.Z /= divisor;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Point3"/> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Point3"/>.</param>
        /// <param name="divisor">Divisor scalar.</param>
        /// <param name="result">The result of dividing a point by a scalar as an output parameter.</param>
        public static void Divide(ref Point3 value1, int divisor, out Point3 result)
        {
            result.X = value1.X / divisor;
            result.Y = value1.Y / divisor;
            result.Z = value1.Z / divisor;
        }

        /// <summary>
        /// Divides the components of a <see cref="Point3"/> by the components of another <see cref="Point3"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Point3"/>.</param>
        /// <param name="value2">Divisor <see cref="Point3"/>.</param>
        /// <param name="result">The result of dividing the points as an output parameter.</param>
        public static void Divide(ref Point3 value1, ref Point3 value2, out Point3 result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
        }

        /// <summary>
        /// Returns a dot product of two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The dot product of two points.</returns>
        public static int Dot(Point3 value1, Point3 value2)
        {
            return value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
        }

        /// <summary>
        /// Returns a dot product of two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <param name="result">The dot product of two points as an output parameter.</param>
        public static void Dot(ref Point3 value1, ref Point3 value2, out int result)
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
            return (obj is Point3) && Equals((Point3)obj);
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Point3"/>.
        /// </summary>
        /// <param name="other">The <see cref="Point3"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Point3 other)
        {
            return ((X == other.X) && (Y == other.Y) && (Z == other.Z));
        }

        /// <summary>
        /// Gets the hash code of this <see cref="Point3"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Point3"/>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Z.GetHashCode();
                return hash;
            }

        }

        /// <summary>
        /// Returns the length of this <see cref="Point3"/>.
        /// </summary>
        /// <returns>The length of this <see cref="Point3"/>.</returns>
        public float Length()
        {
            return (float)Math.Sqrt(LengthSquared());
        }

        /// <summary>
        /// Returns the squared length of this <see cref="Point3"/>.
        /// </summary>
        /// <returns>The squared length of this <see cref="Point3"/>.</returns>
        public int LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z);
        }

        /// <summary>
        /// Creates a new <see cref="Point3"/> that contains a maximal values from the two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The <see cref="Point3"/> with maximal values from the two points.</returns>
        public static Point3 Max(Point3 value1, Point3 value2)
        {
            return new Point3(
                MathHelper.Max(value1.X, value2.X),
                MathHelper.Max(value1.Y, value2.Y),
                MathHelper.Max(value1.Z, value2.Z));
        }

        /// <summary>
        /// Creates a new <see cref="Point3"/> that contains a maximal values from the two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <param name="result">The <see cref="Point3"/> with maximal values from the two points as an output parameter.</param>
        public static void Max(ref Point3 value1, ref Point3 value2, out Point3 result)
        {
            result.X = MathHelper.Max(value1.X, value2.X);
            result.Y = MathHelper.Max(value1.Y, value2.Y);
            result.Z = MathHelper.Max(value1.Z, value2.Z);
        }

        /// <summary>
        /// Creates a new <see cref="Point3"/> that contains a minimal values from the two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The <see cref="Point3"/> with minimal values from the two points.</returns>
        public static Point3 Min(Point3 value1, Point3 value2)
        {
            return new Point3(
                MathHelper.Min(value1.X, value2.X),
                MathHelper.Min(value1.Y, value2.Y),
                MathHelper.Min(value1.Z, value2.Z));
        }

        /// <summary>
        /// Creates a new <see cref="Point3"/> that contains a minimal values from the two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <param name="result">The <see cref="Point3"/> with minimal values from the two points as an output parameter.</param>
        public static void Min(ref Point3 value1, ref Point3 value2, out Point3 result)
        {
            result.X = MathHelper.Min(value1.X, value2.X);
            result.Y = MathHelper.Min(value1.Y, value2.Y);
            result.Z = MathHelper.Min(value1.Z, value2.Z);
        }

        /// <summary>
        /// Creates a new <see cref="Point3"/> that contains a multiplication of two points.
        /// </summary>
        /// <param name="value1">Source <see cref="Point3"/>.</param>
        /// <param name="value2">Source <see cref="Point3"/>.</param>
        /// <returns>The result of the point multiplication.</returns>
        public static Point3 Multiply(Point3 value1, Point3 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="Point3"/> that contains a multiplication of <see cref="Point3"/> and a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Point3"/>.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <returns>The result of the point multiplication with a scalar.</returns>
        public static Point3 Multiply(Point3 value1, int scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            value1.Z *= scaleFactor;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="Point3"/> that contains a multiplication of <see cref="Point3"/> and a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Point3"/>.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
        public static void Multiply(ref Point3 value1, int scaleFactor, out Point3 result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
        }

        /// <summary>
        /// Creates a new <see cref="Point3"/> that contains a multiplication of two points.
        /// </summary>
        /// <param name="value1">Source <see cref="Point3"/>.</param>
        /// <param name="value2">Source <see cref="Point3"/>.</param>
        /// <param name="result">The result of the point multiplication as an output parameter.</param>
        public static void Multiply(ref Point3 value1, ref Point3 value2, out Point3 result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
        }

        /// <summary>
        /// Creates a new <see cref="Point3"/> that contains the specified point inversion.
        /// </summary>
        /// <param name="value">Source <see cref="Point3"/>.</param>
        /// <returns>The result of the point inversion.</returns>
        public static Point3 Negate(Point3 value)
        {
            value = new Point3(-value.X, -value.Y, -value.Z);
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="Point3"/> that contains the specified point inversion.
        /// </summary>
        /// <param name="value">Source <see cref="Point3"/>.</param>
        /// <param name="result">The result of the point inversion as an output parameter.</param>
        public static void Negate(ref Point3 value, out Point3 result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
        }

        /// <summary>
        /// Creates a new <see cref="Point3"/> that contains subtraction of on <see cref="Point3"/> from a another.
        /// </summary>
        /// <param name="value1">Source <see cref="Point3"/>.</param>
        /// <param name="value2">Source <see cref="Point3"/>.</param>
        /// <returns>The result of the point subtraction.</returns>
        public static Point3 Subtract(Point3 value1, Point3 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="Point3"/> that contains subtraction of on <see cref="Point3"/> from a another.
        /// </summary>
        /// <param name="value1">Source <see cref="Point3"/>.</param>
        /// <param name="value2">Source <see cref="Point3"/>.</param>
        /// <param name="result">The result of the point subtraction as an output parameter.</param>
        public static void Subtract(ref Point3 value1, ref Point3 value2, out Point3 result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="Point3"/> in the format:
        /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>]}
        /// </summary>
        /// <returns><see cref="String"/> representation of this <see cref="Point3"/>.</returns>
        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + " Z:" + Z + "}";
        }

        /// <summary>
        /// Gets a <see cref="Vector3"/> representation for this <see cref="Point3"/> object.
        /// </summary>
        /// <returns>A <see cref="Vector3"/> representation for this object.</returns>
        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        /// <summary>
        /// Gets a <see cref="Vector3"/> representation for this <see cref="Point3"/> object implicitly.
        /// </summary>
        /// <returns>A <see cref="Vector3"/> representation for this object.</returns>
        public static implicit operator Vector3(Point3 point)
        {
            return new Vector3(point.X, point.Y, point.Z);
        }

        /// <summary>
        /// Deconstruction method for <see cref="Point3"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Deconstruct(out int x, out int y, out int z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        #endregion
    }
}


