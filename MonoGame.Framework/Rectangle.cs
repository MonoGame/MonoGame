// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    [DataContract]
    public struct Rectangle : IEquatable<Rectangle>
    {
        #region Private Fields

        private static Rectangle emptyRectangle = new Rectangle();

        #endregion Private Fields

        #region Public Fields

        /// <summary>
        /// The x coordinate of the top-left corner of this <see>Rectangle</see>.
        /// </summary>
        [DataMember]
        public int X;

        /// <summary>
        /// The y coordinate of the top-left corner of this <see>Rectangle</see>.
        /// </summary>
        [DataMember]
        public int Y;

        /// <summary>
        /// The width of this <see>Rectangle</see>.
        /// </summary>
        [DataMember]
        public int Width;

        /// <summary>
        /// The height of this <see>Rectangle</see>.
        /// </summary>
        [DataMember]
        public int Height;

        #endregion Public Fields

        #region Public Properties

        /// <summary>
        /// Returns a <see>Rectangle</see> with X=0, Y=0, Width=0, and Height=0.
        /// </summary>
        public static Rectangle Empty
        {
            get { return emptyRectangle; }
        }

        /// <summary>
        /// Returns the x coordinate of the left edge of this <see>Rectangle</see>.
        /// </summary>
        public int Left
        {
            get { return this.X; }
        }

        /// <summary>
        /// Returns the x coordinate of the right edge of this <see>Rectangle</see>.
        /// </summary>
        public int Right
        {
            get { return (this.X + this.Width); }
        }

        /// <summary>
        /// Returns the y coordinate of the top edge of this <see>Rectangle</see>.
        /// </summary>
        public int Top
        {
            get { return this.Y; }
        }

        /// <summary>
        /// Returns the y coordinate of the bottom edge of this <see>Rectangle</see>.
        /// </summary>
        public int Bottom
        {
            get { return (this.Y + this.Height); }
        }

        #endregion Public Properties

        #region Constructors

        public Rectangle(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        #endregion Constructors

        #region Public Methods

        public static bool operator ==(Rectangle a, Rectangle b)
        {
            return ((a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height));
        }

		public bool Contains(int x, int y)
        {
            return ((((this.X <= x) && (x < (this.X + this.Width))) && (this.Y <= y)) && (y < (this.Y + this.Height)));
        }

        public bool Contains(float x, float y)
        {
            return ((((this.X <= x) && (x < (this.X + this.Width))) && (this.Y <= y)) && (y < (this.Y + this.Height)));
        }
		
        public bool Contains(Point value)
        {
            return ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Y <= value.Y)) && (value.Y < (this.Y + this.Height)));
        }

        public bool Contains(Vector2 value)
        {
            return ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Y <= value.Y)) && (value.Y < (this.Y + this.Height)));
        }

        public bool Contains(Rectangle value)
        {
            return ((((this.X <= value.X) && ((value.X + value.Width) <= (this.X + this.Width))) && (this.Y <= value.Y)) && ((value.Y + value.Height) <= (this.Y + this.Height)));
        }

        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return !(a == b);
        }

        public void Offset(Point offset)
        {
            X += offset.X;
            Y += offset.Y;
        }

        public void Offset(int offsetX, int offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }
		
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
		
		public Point Center
		{
			get 
			{
				return new Point(this.X + (this.Width / 2), this.Y + (this.Height / 2));
			}
		}
           
        public void Inflate(int horizontalValue, int verticalValue)
        {
            X -= horizontalValue;
            Y -= verticalValue;
            Width += horizontalValue * 2;
            Height += verticalValue * 2;
        }
		
		public bool IsEmpty
        {
            get
            {
                return ((((this.Width == 0) && (this.Height == 0)) && (this.X == 0)) && (this.Y == 0));
            }
        }

        public bool Equals(Rectangle other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return (obj is Rectangle) ? this == ((Rectangle)obj) : false;
        }

        /// <remarks>
        /// Returns a String representation of this Rectangle in the format:
        /// X:[x] Y:[y] Width:[width] Height:[height]
        /// </remarks>
        public override string ToString()
        {
            return string.Format("{{X:{0} Y:{1} Width:{2} Height:{3}}}", X, Y, Width, Height);
        }

        public override int GetHashCode()
        {
            return (this.X ^ this.Y ^ this.Width ^ this.Height);
        }

        public bool Intersects(Rectangle value)
        {
            return value.Left < Right       && 
                   Left       < value.Right && 
                   value.Top  < Bottom      &&
                   Top        < value.Bottom;            
        }

        public void Intersects(ref Rectangle value, out bool result)
        {
            result = value.Left < Right       && 
                     Left       < value.Right && 
                     value.Top  < Bottom      &&
                     Top        < value.Bottom;
        }

        public static Rectangle Intersect(Rectangle value1, Rectangle value2)
        {
            Rectangle rectangle;
            Intersect(ref value1, ref value2, out rectangle);
            return rectangle;
        }

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
		
		public static Rectangle Union(Rectangle value1, Rectangle value2)
		{
			int x = Math.Min (value1.X, value2.X);
			int y = Math.Min (value1.Y, value2.Y);
			return new Rectangle(x, y,
			                     Math.Max (value1.Right, value2.Right) - x,
				                     Math.Max (value1.Bottom, value2.Bottom) - y);
		}
		
		public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
		{
			result.X = Math.Min (value1.X, value2.X);
			result.Y = Math.Min (value1.Y, value2.Y);
			result.Width = Math.Max (value1.Right, value2.Right) - result.X;
			result.Height = Math.Max (value1.Bottom, value2.Bottom) - result.Y;
		}
				
        #endregion Public Methods
    }
}
