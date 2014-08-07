// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    [DataContract]
    public struct Point : IEquatable<Point>
    {
        #region Private Fields

        private static Point zeroPoint = new Point();

        #endregion Private Fields

        #region Public Fields

        [DataMember]
        public int X;

        [DataMember]
        public int Y;

        #endregion Public Fields

        #region Properties

        /// <summary>
        /// Returns a <see>Point</see> with coordinates 0, 0.
        /// </summary>
        public static Point Zero
        {
            get { return zeroPoint; }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Creates a <see>Point</see> with the provided coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the <see>Point</see> to create.</param>
        /// <param name="y">The y coordinate of the <see>Point</see> to create.</param>
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        #endregion Constructors

        #region Operators

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X+b.X,a.Y+b.Y);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X-b.X,a.Y-b.Y);
        }

        public static Point operator *(Point a, Point b)
        {
            return new Point(a.X*b.X,a.Y*b.Y);
        }

        public static Point operator /(Point a, Point b)
        {
            return new Point(a.X/b.X,a.Y/b.Y);
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !a.Equals(b);
        }

        #endregion

        #region Public methods

        public bool Equals(Point other)
        {
            return ((X == other.X) && (Y == other.Y));
        }
        
        public override bool Equals(object obj)
        {
            return (obj is Point) ? Equals((Point)obj) : false;
        }

        public override int GetHashCode()
        {
            return X ^ Y;
        }

        /// <summary>
        /// Returns a String representation of this Point in the format:
        /// X:[x] Y:[y]
        /// </summary>
        public override string ToString()
        {
            return string.Format("{{X:{0} Y:{1}}}", X, Y);
        }

        #endregion
    }
}


