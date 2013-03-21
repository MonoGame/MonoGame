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

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Describe integer based rectangle.
    /// </summary>
    public struct Rectangle : IEquatable<Rectangle>
    { 
        private static Rectangle emptyRectangle = new Rectangle();

        /// <summary>
        /// X coordinate of the <see cref="Rectangle"/>.
        /// </summary>
        public int X;
        
        /// <summary>
        /// Y coordinate of the <see cref="Rectangle"/>.
        /// </summary>
        public int Y;

        /// <summary>
        /// Width of the <see cref="Rectangle"/>.
        /// </summary>
        public int Width;

        /// <summary>
        /// Height of the <see cref="Rectangle"/>.
        /// </summary>
        public int Height;
        
        /// <summary>
        /// Returns empty <see cref="Rectangle"/>.
        /// </summary>
        public static Rectangle Empty
        {
            get { return emptyRectangle; }
        }

        /// <summary>
        /// Gets X coordinate of left-top corner of the <see cref="Rectangle"/>.
        /// </summary>
        public int Left
        {
            get { return this.X; }
        }

        /// <summary>
        /// Gets X coordinate of right-bottom corner of the <see cref="Rectangle"/>.
        /// </summary>
        public int Right
        {
            get { return (this.X + this.Width); }
        }

        /// <summary>
        /// Gets Y coordinate of left-top corner of the <see cref="Rectangle"/>.
        /// </summary>
        public int Top
        {
            get { return this.Y; }
        }

        /// <summary>
        /// Gets Y coordinate of right-bottom corner of the <see cref="Rectangle"/>.
        /// </summary>
        public int Bottom
        {
            get { return (this.Y + this.Height); }
        }

        /// <summary>
        /// Gets or sets the left-top corner of the rectangle.
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
        /// Gets the center of the rectangle as <see cref="Point"/>.
        /// </summary>
        public Point Center
        {
            get
            {
                return new Point(this.X + (this.Width / 2), this.Y + (this.Height / 2));
            }
        }
   
        /// <summary>
        /// Creates a new instance of <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public Rectangle(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Compares whether two <see cref="Rectangle"/> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="Rectangle"/> instance on the left of the equal sign.</param>
        /// <param name="b"><see cref="Rectangle"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Rectangle a, Rectangle b)
        {
            return ((a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height));
        }

        /// <summary>
        /// Compares whether two <see cref="Rectangle"/> instances are not equal.
        /// </summary>
        /// <param name="a"><see cref="Rectangle"/> instance on the left of the not equal sign.</param>
        /// <param name="b"><see cref="Rectangle"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines if coordinates are inside <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns><c>true</c> if coordinates are inside <see cref="Rectangle"/>; <c>false</c> otherwise.</returns>
        public bool Contains(int x, int y)
        {
            return ((((this.X <= x) && (x < (this.X + this.Width))) && (this.Y <= y)) && (y < (this.Y + this.Height)));
        }

        /// <summary>
        /// Determines if coordinates are inside <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">Coordinates.</param>
        /// <returns><c>true</c> if coordinates are inside <see cref="Rectangle"/>; <c>false</c> otherwise.</returns>
        public bool Contains(Point value)
        {
            return ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Y <= value.Y)) && (value.Y < (this.Y + this.Height)));
        }

        /// <summary>
        /// Determines if other <see cref="Rectangle"/> are inside <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">Other <see cref="Rectangle"/>.</param>
        /// <returns><c>true</c> if other <see cref="Rectangle"/> are inside <see cref="Rectangle"/>; <c>false</c> otherwise.</returns>
        public bool Contains(Rectangle value)
        {
            return ((((this.X <= value.X) && ((value.X + value.Width) <= (this.X + this.Width))) && (this.Y <= value.Y)) && ((value.Y + value.Height) <= (this.Y + this.Height)));
        }
          
        /// <summary>
        /// Shifts position of <see cref="Rectangle"/> by offset.
        /// </summary>
        /// <param name="offset">Offset.</param>
        public void Offset(Point offset)
        {
            X += offset.X;
            Y += offset.Y;
        }

        /// <summary>
        /// Shifts position of <see cref="Rectangle"/> by offset.
        /// </summary> 
        /// <param name="offsetX">Offset X coordinate.</param>
        /// <param name="offsetY">Offset Y coordinate.</param>
        public void Offset(int offsetX, int offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }
          
        /// <summary>
        /// Pushes the edges of the <see cref="Rectangle"/> out by the horizontal and vertical values specified.
        /// </summary>
        /// <param name="horizontalValue">Value to push the sides out by.</param>
        /// <param name="verticalValue">Value to push the top and bottom out by.</param>
        /// <remarks>Each corner of the <see cref="Rectangle"/> is pushed away from the center of the <see cref="Rectangle"/> by the specified amounts. This results in the width and height of the <see cref="Rectangle"/> increasing by twice the values provided.</remarks>
        public void Inflate(int horizontalValue, int verticalValue)
        {
            X -= horizontalValue;
            Y -= verticalValue;
            Width += horizontalValue * 2;
            Height += verticalValue * 2;
        }

        /// <summary>
        /// Returns <c>true</c> if <see cref="Rectangle"/> is empty; <c>false</c> otherwise.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return ((((this.Width == 0) && (this.Height == 0)) && (this.X == 0)) && (this.Y == 0));
            }
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="other">The <see cref="Rectangle"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Rectangle other)
        {
            return this == other;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified object.
        /// </summary>
        /// <param name="obj">The <see cref="Rectangle"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return (obj is Rectangle) ? this == ((Rectangle)obj) : false;
        }

        /// <summary>
        /// Converts the values of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of the color value of this instance.</returns>
        public override string ToString()
        {
            return string.Format("{{X:{0} Y:{1} Width:{2} Height:{3}}}", X, Y, Width, Height);
        }

        /// <summary>
        /// Gets the hash code for <see cref="Rectangle"/> instance.
        /// </summary>
        /// <returns>Hash code of the object.</returns>
        public override int GetHashCode()
        {
            return (this.X ^ this.Y ^ this.Width ^ this.Height);
        }

        /// <summary>
        /// Determines whether this <see cref="Rectangle"/> intersects with other <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">Other <see cref="Rectangle"/>.</param>
        /// <returns><c>true</c> if this <see cref="Rectangle"/> intersects with other <see cref="Rectangle"/>; <c>false</c> otherwise.</returns>
        public bool Intersects(Rectangle value)
        {
            return value.Left < Right &&
                   Left < value.Right &&
                   value.Top < Bottom &&
                   Top < value.Bottom;
        }

        /// <summary>
        /// Determines whether <see cref="Rectangle"/> intersects with other <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">Other <see cref="Rectangle"/>.</param>
        /// <param name="result">Output <c>true</c> if <see cref="Rectangle"/> intersects with other <see cref="Rectangle"/>; <c>false</c> otherwise.</param>
        public void Intersects(ref Rectangle value, out bool result)
        {
            result = value.Left < Right &&
                     Left < value.Right &&
                     value.Top < Bottom &&
                     Top < value.Bottom;
        }

        /// <summary>
        /// Calculates intersection result of two rectangles. Returns empty rectangle if no intersection happens.
        /// </summary>
        /// <param name="value1">First <see cref="Rectangle"/>.</param>
        /// <param name="value2">Second <see cref="Rectangle"/>.</param>
        /// <returns>Intersection result.</returns>
        public static Rectangle Intersect(Rectangle value1, Rectangle value2)
        {
            Rectangle rectangle;
            Intersect(ref value1, ref value2, out rectangle);
            return rectangle;
        }

        /// <summary>
        /// Calculates intersection result of two rectangles. Returns empty rectangle if no intersection happens.
        /// </summary>
        /// <param name="value1">First <see cref="Rectangle"/>.</param>
        /// <param name="value2">Second <see cref="Rectangle"/>.</param>
        /// <param name="result">Output intersection result.</param>
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
        /// Creates <see cref="Rectangle"/> which contains two rectangles.
        /// </summary>
        /// <param name="value1">First <see cref="Rectangle"/>.</param>
        /// <param name="value2">Second <see cref="Rectangle"/>.</param>
        /// <returns>Union <see cref="Rectangle"/>.</returns>
        public static Rectangle Union(Rectangle value1, Rectangle value2)
        {
            int x = Math.Min(value1.X, value2.X);
            int y = Math.Min(value1.Y, value2.Y);
            return new Rectangle(x, y,
                                 Math.Max(value1.Right, value2.Right) - x,
                                 Math.Max(value1.Bottom, value2.Bottom) - y);
        }

        /// <summary>
        /// Creates <see cref="Rectangle"/> which contains two rectangles.
        /// </summary>
        /// <param name="value1">First <see cref="Rectangle"/>.</param>
        /// <param name="value2">Second <see cref="Rectangle"/>.</param>
        /// <param name="result">Output union <see cref="Rectangle"/>.</param>
        public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
        {
            result.X = Math.Min(value1.X, value2.X);
            result.Y = Math.Min(value1.Y, value2.Y);
            result.Width = Math.Max(value1.Right, value2.Right) - result.X;
            result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
        } 
    }
}
