#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.Globalization;
using System.ComponentModel;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Defines a rectangle.
    /// </summary>	
    public struct Rectangle : IEquatable<Rectangle>
    {
        #region Private Fields

        private static Rectangle emptyRectangle = new Rectangle();

        #endregion Private Fields

        #region Public Fields

        /// <summary>
        /// Specifies the x-coordinate of the rectangle.
        /// </summary>
        public int X;

        /// <summary>
        /// Specifies the y-coordinate of the rectangle.
        /// </summary>
        public int Y;

        /// <summary>
        /// Specifies the width of the rectangle.
        /// </summary>
        public int Width;

        /// <summary>
        /// Specifies the height of the rectangle.
        /// </summary>
        public int Height;

        #endregion Public Fields

        #region Public Properties

        /// <summary>
        /// Returns a <see cref="Rectangle"/> with all of its values set to zero.
        /// </summary>
        public static Rectangle Empty
        {
            get { return emptyRectangle; }
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="Rectangle"/> is empty.
        /// </summary>	
        public bool IsEmpty
        {
            get
            {
                return ((((this.Width == 0) && (this.Height == 0)) && (this.X == 0)) && (this.Y == 0));
            }
        }

        /// <summary>
        /// Returns the x-coordinate of the left side of the rectangle.
        /// </summary>
        public int Left
        {
            get { return this.X; }
        }

        /// <summary>
        /// Returns the x-coordinate of the right side of the rectangle.
        /// </summary>
        public int Right
        {
            get { return (this.X + this.Width); }
        }

        /// <summary>
        /// Returns the y-coordinate of the top of the rectangle.
        /// </summary>
        public int Top
        {
            get { return this.Y; }
        }

        /// <summary>
        /// Returns the y-coordinate of the bottom of the rectangle.
        /// </summary>
        public int Bottom
        {
            get { return (this.Y + this.Height); }
        }

        /// <summary>
        /// Gets or sets the upper-left value of the <see cref="Rectangle"/>.
        /// </summary>	
        public Point Location
        {
            get
            {
                return new Point(this.X, this.Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>
        /// Gets the <see cref="Point"/> that specifies the center of the rectangle.
        /// </summary>
        public Point Center
        {
            get
            {
                // This is incorrect
                // return new Point( (this.X + this.Width) / 2,(this.Y + this.Height) / 2 );
                // What we want is the Center of the rectangle from the X and Y Origins
                return new Point(this.X + (this.Width / 2), this.Y + (this.Height / 2));
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the rectangle.</param>
        /// <param name="y">The y-coordinate of the rectangle.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        public Rectangle(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        #endregion Constructors

        #region Public Operators

        /// <summary>
        /// Compares two rectangles for equality.
        /// </summary>
        /// <param name="a">Source rectangle.</param>
        /// <param name="b">Source rectangle.</param>
        /// <returns><c>true</c> if the rectangles are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Rectangle a, Rectangle b)
        {
            return ((a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height));
        }

        /// <summary>
        /// Compares two rectangles for inequality.
        /// </summary>
        /// <param name="a">Source rectangle.</param>
        /// <param name="b">Source rectangle.</param>
        /// <returns><c>true</c> if the rectangles are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return !(a == b);
        }

        #endregion Public Operators

        #region Public Methods

        /// <summary>
        /// Determines whether this <see cref="Rectangle"/> contains a specified point represented by its x- and y-coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate of the specified point.</param>
        /// <param name="y">The y-coordinate of the specified point.</param>
        /// <returns><c>true</c> if the specified point is contained within this <see cref="Rectangle"/>; <c>false</c> otherwise.</returns>
        public bool Contains(int x, int y)
        {
            return ((((this.X <= x) && (x < (this.X + this.Width))) && (this.Y <= y)) && (y < (this.Y + this.Height)));
        }

        /// <summary>
        /// Determines whether this <see cref="Rectangle"/> contains a specified <see cref="Point"/>.
        /// </summary>
        /// <param name="value">The <see cref="Point"/> to evaluate.</param>
        /// <returns><c>true</c> if the specified <see cref="Point"/> is contained within this <see cref="Rectangle"/>; <c>false</c> otherwise.</returns>
        public bool Contains(Point value)
        {
            return ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Y <= value.Y)) && (value.Y < (this.Y + this.Height)));
        }

        /// <summary>
        /// Determines whether this <see cref="Rectangle"/> contains a specified <see cref="Point"/>.
        /// </summary>
        /// <param name="value">The <see cref="Point"/> to evaluate.</param>
        /// <param name="result">Will contains <c>true</c> if the specified <see cref="Point"/> is contained within this <see cref="Rectangle"/>; <c>false</c> otherwise.</param>
        public void Contains(ref Point value, out bool result)
        {
            if ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Y <= value.Y)) &&
                (value.Y < (this.Y + this.Height)))
                result = true;
            else
                result = false;
        }

        /// <summary>
        /// Determines whether this <see cref="Rectangle"/> entirely contains a specified <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">The <see cref="Rectangle"/> to evaluate.</param>
        /// <returns><c>true</c> if this <see cref="Rectangle"/> entirely contains the specified <see cref="Rectangle"/>; <c>false</c> otherwise.</returns>
        public bool Contains(Rectangle value)
        {
            return ((((this.X <= value.X) && ((value.X + value.Width) <= (this.X + this.Width))) && (this.Y <= value.Y)) && ((value.Y + value.Height) <= (this.Y + this.Height)));
        }

        /// <summary>
        /// Determines whether this <see cref="Rectangle"/> entirely contains a specified <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">The <see cref="Rectangle"/> to evaluate.</param>
        /// <param name="result">Will contains <c>true</c> if this <see cref="Rectangle"/> entirely contains the specified <see cref="Rectangle"/>; <c>false</c> otherwise.</param>
        public void Contains(ref Rectangle value, out bool result)
        {
            if ((((this.X <= value.X) && ((value.X + value.Width) <= (this.X + this.Width))) && (this.Y <= value.Y)) &&
                ((value.Y + value.Height) <= (this.Y + this.Height)))
                result = true;
            else
                result = false;
        }

        /// <summary>
        /// Changes the position of the <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="offsetX">Change in the x-position.</param>
        /// <param name="offsetY">Change in the y-position.</param>
        public void Offset(int offsetX, int offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        /// <summary>
        /// Changes the position of the <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="offset">The values to adjust the position of the <see cref="Rectangle"/> by.</param>
        public void Offset(Point offset)
        {
            X += offset.X;
            Y += offset.Y;
        }

        /// <summary>
        /// Pushes the edges of the <see cref="Rectangle"/> out by the horizontal and vertical values specified.
        /// </summary>
        /// <param name="horizontalValue">Value to push the sides out by.</param>
        /// <param name="verticalValue">Value to push the top and bottom out by.</param>
        public void Inflate(int horizontalValue, int verticalValue)
        {
            X -= horizontalValue;
            Y -= verticalValue;
            Width += horizontalValue * 2;
            Height += verticalValue * 2;
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="other"><see cref="Rectangle"/> to make the comparison with.</param>
        /// <returns><c>true</c> if the current instance is equal to the specified <see cref="Rectangle"/>; <c>false</c> otherwise.</returns>
        public bool Equals(Rectangle other)
        {
            return this == other;
        }

        /// <summary>
        /// Determines whether a specified <see cref="Rectangle"/> intersects with this <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">The <see cref="Rectangle"/> to evaluate.</param>
        /// <returns><c>true</c> if the specified <see cref="Rectangle"/> intersects with this one; <c>false</c> otherwise.</returns>
        public bool Intersects(Rectangle value)
        {
            return value.Left < Right &&
                   Left < value.Right &&
                   value.Top < Bottom &&
                   Top < value.Bottom;
        }

        /// <summary>
        /// Determines whether a specified <see cref="Rectangle"/> intersects with this <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">The <see cref="Rectangle"/> to evaluate.</param>
        /// <param name="result">Will contains <c>true</c> if the specified <see cref="Rectangle"/> intersects with this one; <c>false</c> otherwise.</param>
        public void Intersects(ref Rectangle value, out bool result)
        {
            result = value.Left < Right &&
                     Left < value.Right &&
                     value.Top < Bottom &&
                     Top < value.Bottom;
        }

        /// <summary>
        /// Creates a <see cref="Rectangle"/> defining the area where one rectangle overlaps with another rectangle.
        /// </summary>
        /// <param name="value1">The first <see cref="Rectangle"/> to compare.</param>
        /// <param name="value2">The second <see cref="Rectangle"/> to compare.</param>
        /// <returns>The area where the two parameters overlap.</returns>
        public static Rectangle Intersect(Rectangle value1, Rectangle value2)
        {
            Rectangle rectangle;
            Intersect(ref value1, ref value2, out rectangle);
            return rectangle;
        }

        /// <summary>
        /// Creates a <see cref="Rectangle"/> defining the area where one rectangle overlaps with another rectangle.
        /// </summary>
        /// <param name="value1">The first <see cref="Rectangle"/> to compare.</param>
        /// <param name="value2">The second <see cref="Rectangle"/> to compare.</param>
        /// <param name="result">Will contains the area where the two first parameters overlap.</param>
        public static void Intersect(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
        {
            if (value1.Intersects(value2))
            {
                int right_side = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
                int left_side = Math.Max(value1.X, value2.X);
                int top_side = Math.Max(value1.Y, value2.Y);
                int bottom_side = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
                result = new Rectangle(left_side, top_side, right_side - left_side, bottom_side - top_side);
            }
            else
            {
                result = new Rectangle(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Creates a new <see cref="Rectangle"/> that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="Rectangle"/> to contain.</param>
        /// <param name="value2">The second <see cref="Rectangle"/> to contain.</param>
        /// <returns>The union of the two <see cref="Rectangle"/> parameters.</returns>
        public static Rectangle Union(Rectangle value1, Rectangle value2)
        {
            int x = Math.Min(value1.X, value2.X);
            int y = Math.Min(value1.Y, value2.Y);
            return new Rectangle(x, y,
            Math.Max(value1.Right, value2.Right) - x,
            Math.Max(value1.Bottom, value2.Bottom) - y);
        }

        /// <summary>
        /// Creates a new <see cref="Rectangle"/> that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="Rectangle"/> to contain.</param>
        /// <param name="value2">The second <see cref="Rectangle"/> to contain.</param>
        /// <param name="result">Will contains the union of the two <see cref="Rectangle"/> parameters.</param>
        public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
        {
            result.X = Math.Min(value1.X, value2.X);
            result.Y = Math.Min(value1.Y, value2.Y);
            result.Width = Math.Max(value1.Right, value2.Right) - result.X;
            result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
        }

        #endregion Public Methods

        #region Object overrided methods

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="obj"><see cref="System.Object"/> to make the comparison with.</param>
        /// <returns><c>true</c> if the current instance is equal to the specified object; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return (obj is Rectangle) ? this == ((Rectangle)obj) : false;
        }

        /// <summary>
        /// Retrieves a string representation of the current object.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString()
        {
            return string.Format("{{X:{0} Y:{1} Width:{2} Height:{3}}}", X, Y, Width, Height);
        }

        /// <summary>
        /// Gets the hash code for this object.
        /// </summary>
        /// <returns>Hash code for this object.</returns>
        public override int GetHashCode()
        {
            return (this.X ^ this.Y ^ this.Width ^ this.Height);
        }

        #endregion Object overrided methods
    }
}

