// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Describes a 2D point on an integer grid.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Point : IEquatable<Point>
    {
        #region Private Fields

        private static readonly Point zero = new Point(0, 0);
        private static readonly Point one = new Point(1, 1);
        private static readonly Point negOne = new Point(-1, -1);
        private static readonly Point unitX = new Point(1, 0);
        private static readonly Point unitY = new Point(0, 1);
        private static readonly Point up = new Point(0, 1);
        private static readonly Point down = new Point(0, -1);
        private static readonly Point right = new Point(1, 0);
        private static readonly Point left = new Point(-1, 0);

        #endregion

        #region Public Fields

        /// <summary>
        /// The x coordinate of this <see cref="Point"/>.
        /// </summary>
        [DataMember]
        public int X;

        /// <summary>
        /// The y coordinate of this <see cref="Point"/>.
        /// </summary>
        [DataMember]
        public int Y;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns a <see cref="Point"/> with components 0, 0.
        /// </summary>
        public static Point Zero
        {
            get { return zero; }
        }

        /// <summary>
        /// Returns a <see cref="Point"/> with components 1, 1.
        /// </summary>
        public static Point One
        {
            get { return one; }
        }

        /// <summary>
        /// Returns a <see cref="Point"/> with components -1, -1.
        /// </summary>
        public static Point NegOne
        {
            get { return negOne; }
        }

        /// <summary>
        /// Returns a <see cref="Point"/> with components 1, 0.
        /// </summary>
        public static Point UnitX
        {
            get { return unitX; }
        }

        /// <summary>
        /// Returns a <see cref="Point"/> with components 0, 1.
        /// </summary>
        public static Point UnitY
        {
            get { return unitY; }
        }

        /// <summary>
        /// Returns a <see cref="Point"/> with components 0, 1.
        /// </summary>
        public static Point Up
        {
            get { return up; }
        }

        /// <summary>
        /// Returns a <see cref="Point"/> with components 0, -1.
        /// </summary>
        public static Point Down
        {
            get { return down; }
        }

        /// <summary>
        /// Returns a <see cref="Point"/> with components 1, 0.
        /// </summary>
        public static Point Right
        {
            get { return right; }
        }

        /// <summary>
        /// Returns a <see cref="Point"/> with components -1, 0.
        /// </summary>
        public static Point Left
        {
            get { return left; }
        }

        #endregion

        #region Internal Properties

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    this.X.ToString(), "  ",
                    this.Y.ToString()
                );
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a point with X and Y from two values.
        /// </summary>
        /// <param name="x">The x coordinate in 2D-space.</param>
        /// <param name="y">The y coordinate in 2D-space.</param>
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Constructs a point with X and Y set to the same value.
        /// </summary>
        /// <param name="value">The x and y coordinates in 2D-space.</param>
        public Point(int value)
        {
            this.X = value;
            this.Y = value;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Adds two points.
        /// </summary>
        /// <param name="value1">Source <see cref="Point"/> on the left of the add sign.</param>
        /// <param name="value2">Source <see cref="Point"/> on the right of the add sign.</param>
        /// <returns>Sum of the points.</returns>
        public static Point operator +(Point value1, Point value2)
        {
            return new Point(value1.X + value2.X, value1.Y + value2.Y);
        }

        /// <summary>
        /// Subtracts a <see cref="Point"/> from a <see cref="Point"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Point"/> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="Point"/> on the right of the sub sign.</param>
        /// <returns>Result of the subtraction.</returns>
        public static Point operator -(Point value1, Point value2)
        {
            return new Point(value1.X - value2.X, value1.Y - value2.Y);
        }

        /// <summary>
        /// Multiplies the components of two points by each other.
        /// </summary>
        /// <param name="value1">Source <see cref="Point"/> on the left of the mul sign.</param>
        /// <param name="value2">Source <see cref="Point"/> on the right of the mul sign.</param>
        /// <returns>Result of the multiplication.</returns>
        public static Point operator *(Point value1, Point value2)
        {
            return new Point(value1.X * value2.X, value1.Y * value2.Y);
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="value">Source <see cref="Point"/> on the left of the mul sign.</param>
        /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Point operator *(Point value, int scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
        /// <param name="value">Source <see cref="Point"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Point operator *(int scaleFactor, Point value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Divides the components of a <see cref="Point"/> by the components of another <see cref="Point"/>.
        /// </summary>
        /// <param name="source">Source <see cref="Point"/> on the left of the div sign.</param>
        /// <param name="divisor">Divisor <see cref="Point"/> on the right of the div sign.</param>
        /// <returns>The result of dividing the points.</returns>
        public static Point operator /(Point source, Point divisor)
        {
            return new Point(source.X / divisor.X, source.Y / divisor.Y);
        }

        /// <summary>
        /// Divides the components of a <see cref="Point"/> by a specified divisor.
        /// </summary>
        /// <param name="source">Source <see cref="Point"/> on the left of the div sign.</param>
        /// <param name="divisor">Divisor on the right of the div sign.</param>
        /// <returns>The result of dividing the point.</returns>
        public static Point operator /(Point source, int divisor)
        {
            return new Point(source.X / divisor, source.Y / divisor);
        }

        /// <summary>
        /// Compares whether two <see cref="Point"/> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="Point"/> instance on the left of the equal sign.</param>
        /// <param name="b"><see cref="Point"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Point a, Point b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compares whether two <see cref="Point"/> instances are not equal.
        /// </summary>
        /// <param name="a"><see cref="Point"/> instance on the left of the not equal sign.</param>
        /// <param name="b"><see cref="Point"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
        public static bool operator !=(Point a, Point b)
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
        public static Point Add(Point value1, Point value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
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
        public static void Add(ref Point value1, ref Point value2, out Point result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
        }

        /// <summary>
        /// Clamps the specified value within a range.
        /// </summary>
        /// <param name="value1">The value to clamp.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <returns>The clamped value.</returns>
        public static Point Clamp(Point value1, Point min, Point max)
        {
            return new Point(
                MathHelper.Clamp(value1.X, min.X, max.X),
                MathHelper.Clamp(value1.Y, min.Y, max.Y));
        }

        /// <summary>
        /// Clamps the specified value within a range.
        /// </summary>
        /// <param name="value1">The value to clamp.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <param name="result">The clamped value as an output parameter.</param>
        public static void Clamp(ref Point value1, ref Point min, ref Point max, out Point result)
        {
            result.X = MathHelper.Clamp(value1.X, min.X, max.X);
            result.Y = MathHelper.Clamp(value1.Y, min.Y, max.Y);
        }

        /// <summary>
        /// Returns the distance between two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The distance between two points.</returns>
        public static float Distance(Point value1, Point value2)
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
        public static void Distance(ref Point value1, ref Point value2, out float result)
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
        public static int DistanceSquared(Point value1, Point value2)
        {
            int x = value1.X - value2.X, y = value1.Y - value2.Y;
            return (x * x) + (y * y);
        }

        /// <summary>
        /// Returns the squared distance between two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <param name="result">The squared distance between two points as an output parameter.</param>
        public static void DistanceSquared(ref Point value1, ref Point value2, out int result)
        {
            int x = value1.X - value2.X, y = value1.Y - value2.Y;
            result = (x * x) + (y * y);
        }

        /// <summary>
        /// Divides the components of a <see cref="Point"/> by the components of another <see cref="Point"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Point"/>.</param>
        /// <param name="value2">Divisor <see cref="Point"/>.</param>
        /// <returns>The result of dividing the points.</returns>
        public static Point Divide(Point value1, Point value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Point"/> by the components of another <see cref="Point"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Point"/>.</param>
        /// <param name="value2">Divisor <see cref="Point"/>.</param>
        /// <param name="result">The result of dividing the points as an output parameter.</param>
        public static void Divide(ref Point value1, ref Point value2, out Point result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
        }

        /// <summary>
        /// Divides the components of a <see cref="Point"/> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Point"/>.</param>
        /// <param name="divisor">Divisor scalar.</param>
        /// <returns>The result of dividing a point by a scalar.</returns>
        public static Point Divide(Point value1, int divisor)
        {
            value1.X /= divisor;
            value1.Y /= divisor;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Point"/> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Point"/>.</param>
        /// <param name="divisor">Divisor scalar.</param>
        /// <param name="result">The result of dividing a point by a scalar as an output parameter.</param>
        public static void Divide(ref Point value1, int divisor, out Point result)
        {
            result.X = value1.X / divisor;
            result.Y = value1.Y / divisor;
        }

        /// <summary>
        /// Returns a dot product of two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The dot product of two points.</returns>
        public static int Dot(Point value1, Point value2)
        {
            return (value1.X * value2.X) + (value1.Y * value2.Y);
        }

        /// <summary>
        /// Returns a dot product of two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <param name="result">The dot product of two points as an output parameter.</param>
        public static void Dot(ref Point value1, ref Point value2, out int result)
        {
            result = (value1.X * value2.X) + (value1.Y * value2.Y);
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return (obj is Point) && Equals((Point)obj);
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Point"/>.
        /// </summary>
        /// <param name="other">The <see cref="Point"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Point other)
        {
            return ((X == other.X) && (Y == other.Y));
        }

        /// <summary>
        /// Gets the hash code of this <see cref="Point"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Point"/>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                return hash;
            }

        }

        /// <summary>
        /// Returns the length of this <see cref="Point"/>.
        /// </summary>
        /// <returns>The length of this <see cref="Point"/>.</returns>
        public float Length()
        {
            return (float)Math.Sqrt(LengthSquared());
        }

        /// <summary>
        /// Returns the squared length of this <see cref="Point"/>.
        /// </summary>
        /// <returns>The squared length of this <see cref="Point"/>.</returns>
        public int LengthSquared()
        {
            return (X * X) + (Y * Y);
        }

        /// <summary>
        /// Creates a new <see cref="Point"/> that contains a maximal values from the two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The <see cref="Point"/> with maximal values from the two points.</returns>
        public static Point Max(Point value1, Point value2)
        {
            return new Point(value1.X > value2.X ? value1.X : value2.X,
                               value1.Y > value2.Y ? value1.Y : value2.Y);
        }

        /// <summary>
        /// Creates a new <see cref="Point"/> that contains a maximal values from the two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <param name="result">The <see cref="Point"/> with maximal values from the two points as an output parameter.</param>
        public static void Max(ref Point value1, ref Point value2, out Point result)
        {
            result.X = value1.X > value2.X ? value1.X : value2.X;
            result.Y = value1.Y > value2.Y ? value1.Y : value2.Y;
        }

        /// <summary>
        /// Creates a new <see cref="Point"/> that contains a minimal values from the two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The <see cref="Point"/> with minimal values from the two points.</returns>
        public static Point Min(Point value1, Point value2)
        {
            return new Point(value1.X < value2.X ? value1.X : value2.X,
                               value1.Y < value2.Y ? value1.Y : value2.Y);
        }

        /// <summary>
        /// Creates a new <see cref="Point"/> that contains a minimal values from the two points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <param name="result">The <see cref="Point"/> with minimal values from the two points as an output parameter.</param>
        public static void Min(ref Point value1, ref Point value2, out Point result)
        {
            result.X = value1.X < value2.X ? value1.X : value2.X;
            result.Y = value1.Y < value2.Y ? value1.Y : value2.Y;
        }

        /// <summary>
        /// Creates a new <see cref="Point"/> that contains a multiplication of two points.
        /// </summary>
        /// <param name="value1">Source <see cref="Point"/>.</param>
        /// <param name="value2">Source <see cref="Point"/>.</param>
        /// <returns>The result of the point multiplication.</returns>
        public static Point Multiply(Point value1, Point value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="Point"/> that contains a multiplication of two points.
        /// </summary>
        /// <param name="value1">Source <see cref="Point"/>.</param>
        /// <param name="value2">Source <see cref="Point"/>.</param>
        /// <param name="result">The result of the point multiplication as an output parameter.</param>
        public static void Multiply(ref Point value1, ref Point value2, out Point result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
        }

        /// <summary>
        /// Creates a new <see cref="Point"/> that contains a multiplication of <see cref="Point"/> and a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Point"/>.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <returns>The result of the point multiplication with a scalar.</returns>
        public static Point Multiply(Point value1, int scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="Point"/> that contains a multiplication of <see cref="Point"/> and a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Point"/>.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
        public static void Multiply(ref Point value1, int scaleFactor, out Point result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
        }

        /// <summary>
        /// Creates a new <see cref="Point"/> that contains the specified point inversion.
        /// </summary>
        /// <param name="value">Source <see cref="Point"/>.</param>
        /// <returns>The result of the point inversion.</returns>
        public static Point Negate(Point value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="Point"/> that contains the specified point inversion.
        /// </summary>
        /// <param name="value">Source <see cref="Point"/>.</param>
        /// <param name="result">The result of the point inversion as an output parameter.</param>
        public static void Negate(ref Point value, out Point result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="Point"/> in the format:
        /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>]}
        /// </summary>
        /// <returns><see cref="String"/> representation of this <see cref="Point"/>.</returns>
        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + "}";
        }

        /// <summary>
        /// Gets a <see cref="Vector2"/> representation for this <see cref="Point"/> object.
        /// </summary>
        /// <returns>A <see cref="Vector2"/> representation for this object.</returns>
        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        /// <summary>
        /// Gets a <see cref="Vector2"/> representation for this <see cref="Point"/> object implicitly.
        /// </summary>
        /// <returns>A <see cref="Vector2"/> representation for this object.</returns>
        public static implicit operator Vector2(Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        /// <summary>
        /// Deconstruction method for <see cref="Point"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        #endregion
    }
}


