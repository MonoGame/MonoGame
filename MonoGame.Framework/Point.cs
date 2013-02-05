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
    /// Defines a point in 2D space.
    /// </summary>
    public struct Point : IEquatable<Point>
    {
        #region Private Fields

        private static Point zeroPoint = new Point();

        #endregion Private Fields
 
        #region Public Fields
        
        /// <summary>
        /// Specifies the x-coordinate of the <see cref="Point"/>.
        /// </summary>
        public int X;
        
        /// <summary>
        /// Specifies the y-coordinate of the <see cref="Point"/>.
        /// </summary>
        public int Y;

        #endregion Public Fields
 
        #region Properties
        
        /// <summary>
        /// Returns the point (0,0).
        /// </summary>
        public static Point Zero
        {
            get { return zeroPoint; }
        }

        #endregion Properties
 
        #region Constructors
        
        /// <summary>
        /// Initializes a new instance of <see cref="Point"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the <see cref="Point"/>.</param>
        /// <param name="y">The y-coordinate of the <see cref="Point"/>.</param>
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        #endregion Constructors
        
        #region Public Operators
        
        /// <summary>
        /// Determines whether two <see cref="Point"/> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="Point"/> on the left side of the equal sign.</param>
        /// <param name="b"><see cref="Point"/> on the right side of the equal sign.</param>
        /// <returns><c>true</c> if <c>a</c> and <c>b</c> are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Point a, Point b)
        {
            return a.Equals(b);
        }
        
        /// <summary>
        /// Determines whether two <see cref="Point"/> instances are not equal.
        /// </summary>
        /// <param name="a"><see cref="Point"/> on the left side of the equal sign.</param>
        /// <param name="b"><see cref="Point"/> on the right side of the equal sign.</param>
        /// <returns><c>true</c> if <c>a</c> and <c>b</c> are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Point a, Point b)
        {
            return !a.Equals(b);
        }
        
        #endregion Public Operators
        
        #region Public Methods
        
        /// <summary>
        /// Determines whether two <see cref="Point"/> instances are equal.
        /// </summary>
        /// <param name="other">The <see cref="Point"/> to compare this instance to.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Point other)
        {
            return ((X == other.X) && (Y == other.Y));
        }
        
        #endregion Public Methods
        
        #region Object Overrided Methods
        
        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="obj"><see cref="System.Object"/> to make the comparison with.</param>
        /// <returns><c>true</c> if the current instance is equal to the specified object; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return (obj is Point) ? Equals((Point)obj) : false;
        }
        
        /// <summary>
        /// Retrieves a string representation of the current object.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString()
        {
            return string.Format("{{X:{0} Y:{1}}}", X, Y);
        }
        
        /// <summary>
        /// Gets the hash code for this object.
        /// </summary>
        /// <returns>Hash code for this object.</returns>
        public override int GetHashCode()
        {
            return X ^ Y;
        }

        #endregion Object Overrided Methods
    }
}


