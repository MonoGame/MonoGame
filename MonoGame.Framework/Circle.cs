// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary> 
    /// Represents a 2D circle. 
    /// Developed by Kris Steele‏ @KrisWD40, Sourced from http://krissteele.net/blogdetails.aspx?id=251
    /// </summary> 
    public struct Circle
    {
        #region Private Fields
        private Vector2 direction;
        private float distanceSquared;

        #endregion

        #region Public Fields

        /// <summary>
        /// Center X position of the circle.  <see cref="Circle"/>.
        /// </summary>
        [DataMember]
        public float X;

        /// <summary>
        /// Center Y position of the circle.  <see cref="Circle"/>.
        /// </summary>
        [DataMember]
        public float Y;

        /// <summary> 
        /// Radius of the circle. 
        /// </summary> 
        [DataMember]
        public float Radius;

        #endregion

        #region Public Properties

        /// <summary>
        /// The center coordinates of this <see cref="Circle"/>.
        /// </summary>
        public Vector2 Location
        {
            get
            {
                return new Vector2(this.X, this.Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>
        /// A <see cref="Point"/> located in the center of this <see cref="Circle"/>.
        /// </summary>
        public Vector2 Center
        {
            get
            {
                return new Vector2(this.X , this.Y);
            }
        }

        #endregion

        #region Internal Properties

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    this.Center.X, "  ",
                    this.Center.Y, "  ",
                    this.Radius, "  ",
                    this.direction, "  ",
                    this.distanceSquared
                    );
            }
        }

        #endregion

        #region Constructors

        /// <summary> 
        /// Constructs a new circle. 
        /// </summary> 
        public Circle(float x, float y, float radius)
        {
            this.distanceSquared = 0f;
            this.direction = Vector2.Zero;
            this.X = x;
            this.Y = y;
            this.Radius = radius;
        }

        /// <summary> 
        /// Constructs a new circle. 
        /// </summary> 
        public Circle(int x, int y, int radius)
        {
            this.distanceSquared = 0f;
            this.direction = Vector2.Zero;
            this.X = x;
            this.Y = y;
            this.Radius = radius;
        }

        /// <summary> 
        /// Constructs a new circle. 
        /// </summary> 
        public Circle(Vector2 position, float radius)
        {
            this.distanceSquared = 0f;
            this.direction = Vector2.Zero;
            this.X = position.X;
            this.Y = position.Y;
            this.Radius = radius;
        }

        /// <summary> 
        /// Constructs a new circle. 
        /// </summary> 
        public Circle(Vector2 position, int radius)
        {
            this.distanceSquared = 0f;
            this.direction = Vector2.Zero;
            this.X = position.X;
            this.Y = position.Y;
            this.Radius = radius;
        }
        #endregion

        #region Operators

        /// <summary>
        /// Compares whether two <see cref="Rectangle"/> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="Rectangle"/> instance on the left of the equal sign.</param>
        /// <param name="b"><see cref="Rectangle"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Circle a, Circle b)
        {
            return ((a.Center == b.Center) && (a.Radius == b.Radius));
        }

        /// <summary>
        /// Compares whether two <see cref="Rectangle"/> instances are not equal.
        /// </summary>
        /// <param name="a"><see cref="Rectangle"/> instance on the left of the not equal sign.</param>
        /// <param name="b"><see cref="Rectangle"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Circle a, Circle b)
        {
            return !(a == b);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets whether or not the provided coordinates lie within the bounds of this <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="x">The x coordinate of the point to check for containment.</param>
        /// <param name="y">The y coordinate of the point to check for containment.</param>
        /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="Rectangle"/>; <c>false</c> otherwise.</returns>
        public bool Contains(int x, int y)
        {
            return isInCircle(this.X, this.Y, this.Radius, x, y);
        }

        /// <summary>
        /// Gets whether or not the provided coordinates lie within the bounds of this <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="x">The x coordinate of the point to check for containment.</param>
        /// <param name="y">The y coordinate of the point to check for containment.</param>
        /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="Rectangle"/>; <c>false</c> otherwise.</returns>
        public bool Contains(float x, float y)
        {
            return isInCircle(this.X, this.Y, this.Radius, x, y);
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">The coordinates to check for inclusion in this <see cref="Rectangle"/>.</param>
        /// <returns><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="Rectangle"/>; <c>false</c> otherwise.</returns>
        public bool Contains(Point value)
        {
            return isInCircle(this.X, this.Y, this.Radius, value.X, value.Y);
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">The coordinates to check for inclusion in this <see cref="Rectangle"/>.</param>
        /// <param name="result"><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="Rectangle"/>; <c>false</c> otherwise. As an output parameter.</param>
        public void Contains(ref Point value, out bool result)
        {
            result = isInCircle(this.X, this.Y, this.Radius, value.X, value.Y);
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">The coordinates to check for inclusion in this <see cref="Rectangle"/>.</param>
        /// <returns><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="Rectangle"/>; <c>false</c> otherwise.</returns>
        public bool Contains(Vector2 value)
        {
            return isInCircle(this.X, this.Y, this.Radius, value.X, value.Y);
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">The coordinates to check for inclusion in this <see cref="Rectangle"/>.</param>
        /// <param name="result"><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="Rectangle"/>; <c>false</c> otherwise. As an output parameter.</param>
        public void Contains(ref Vector2 value, out bool result)
        {
            result = isInCircle(this.X, this.Y, this.Radius, value.X, value.Y);
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="Rectangle"/> lies within the bounds of this <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">The <see cref="Rectangle"/> to check for inclusion in this <see cref="Rectangle"/>.</param>
        /// <returns><c>true</c> if the provided <see cref="Rectangle"/>'s bounds lie entirely inside this <see cref="Rectangle"/>; <c>false</c> otherwise.</returns>
        public bool Contains(Rectangle value)
        {
            return isInCircle(this.X, this.Y, this.Radius, value.X, value.Y) && 
            isInCircle(this.X, this.Y, this.Radius, value.X + value.Width, value.Y) &&
            isInCircle(this.X, this.Y, this.Radius, value.X, value.Y + value.Height) &&
            isInCircle(this.X, this.Y, this.Radius, value.X + value.Width, value.Y + value.Height);
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="Rectangle"/> lies within the bounds of this <see cref="Circle"/>.
        /// </summary>
        /// <param name="value">The <see cref="Rectangle"/> to check for inclusion in this <see cref="Rectangle"/>.</param>
        /// <param name="result"><c>true</c> if the provided <see cref="Rectangle"/>'s bounds lie entirely inside this <see cref="Circle"/>; <c>false</c> otherwise. As an output parameter.</param>
        public void Contains(ref Rectangle value, out bool result)
        {
            result = isInCircle(this.X, this.Y, this.Radius, value.X, value.Y) &&
            isInCircle(this.X, this.Y, this.Radius, value.X + value.Width, value.Y) &&
            isInCircle(this.X, this.Y, this.Radius, value.X, value.Y + value.Height) &&
            isInCircle(this.X, this.Y, this.Radius, value.X + value.Width, value.Y + value.Height);
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="Circle"/> lies within the bounds of this <see cref="Circle"/>.
        /// </summary>
        /// <param name="value">The <see cref="Circle"/> to check for inclusion in this <see cref="Circle"/>.</param>
        /// <returns><c>true</c> if the provided <see cref="Circle"/>'s bounds lie entirely inside this <see cref="Circle"/>; <c>false</c> otherwise.</returns>
        public bool Contains(Circle value)
        {
            this.direction = Center - value.Center;
            //If Distance + value.radius < Radius
            return this.direction.Length() + value.Radius < Radius;

        }

        /// <summary>
        /// Gets whether or not the provided <see cref="Circle"/> lies within the bounds of this <see cref="Circle"/>.
        /// </summary>
        /// <param name="value">The <see cref="Circle"/> to check for inclusion in this <see cref="Circle"/>.</param>
        /// <param name="result"><c>true</c> if the provided <see cref="Circle"/>'s bounds lie entirely inside this <see cref="Circle"/>; <c>false</c> otherwise. As an output parameter.</param>
        public void Contains(ref Circle value, out bool result)
        {
            this.direction = Center - value.Center;
            result = this.direction.Length() + value.Radius < Radius;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return (obj is Circle) && this == ((Circle)obj);
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="other">The <see cref="Rectangle"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Circle other)
        {
            return this == other;
        }

        /// <summary>
        /// Gets the hash code of this <see cref="Circle"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Circle"/>.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Radius.GetHashCode();
        }

        /// <summary> 
        /// Determines if a circle intersects a rectangle. 
        /// </summary> 
        /// <returns>True if the circle and rectangle overlap. False otherwise.</returns> 
        public bool Intersects(Rectangle rectangle)
        {
            Vector2 v = new Vector2(MathHelper.Clamp(Center.X, rectangle.Left, rectangle.Right),
                                    MathHelper.Clamp(Center.Y, rectangle.Top, rectangle.Bottom));

            this.direction = Center - v;
            this.distanceSquared = direction.LengthSquared();

            return ((distanceSquared > 0) && (distanceSquared < Radius * Radius));
        }

        /// <summary> 
        /// Determines if a circle intersects another circle. 
        /// </summary> 
        /// <returns>True if the circle and rectangle overlap. False otherwise.</returns> 
        public bool Intersects(Circle circle)
        {
            this.direction = Center - circle.Center;
            return (direction.Length() < (Radius + circle.Radius));
        }

        /// <summary>
        /// Changes the <see cref="Location"/> of this <see cref="Circle"/>.
        /// </summary>
        /// <param name="offsetX">The x coordinate to add to this <see cref="Circle"/>.</param>
        /// <param name="offsetY">The y coordinate to add to this <see cref="Circle"/>.</param>
        public void Offset(int offsetX, int offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        /// <summary>
        /// Changes the <see cref="Location"/> of this <see cref="Circle"/>.
        /// </summary>
        /// <param name="offsetX">The x coordinate to add to this <see cref="Circle"/>.</param>
        /// <param name="offsetY">The y coordinate to add to this <see cref="Circle"/>.</param>
        public void Offset(float offsetX, float offsetY)
        {
            X += (int)offsetX;
            Y += (int)offsetY;
        }

        /// <summary>
        /// Changes the <see cref="Location"/> of this <see cref="Circle"/>.
        /// </summary>
        /// <param name="amount">The x and y components to add to this <see cref="Circle"/>.</param>
        public void Offset(Point amount)
        {
            X += amount.X;
            Y += amount.Y;
        }

        /// <summary>
        /// Changes the <see cref="Location"/> of this <see cref="Circle"/>.
        /// </summary>
        /// <param name="amount">The x and y components to add to this <see cref="Circle"/>.</param>
        public void Offset(Vector2 amount)
        {
            X += (int)amount.X;
            Y += (int)amount.Y;
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="Circle"/> in the format:
        /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Radius:[<see cref="Radius"/>]}
        /// </summary>
        /// <returns><see cref="String"/> representation of this <see cref="Circle"/>.</returns>
        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + " Radius:" + Radius + "}";
        }

        /// <summary>
        /// Creates a new <see cref="Rectangle"/> that completely contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="Rectangle"/>.</param>
        /// <param name="value2">The second <see cref="Rectangle"/>.</param>
        /// <returns>The union of the two rectangles.</returns>
        public static Circle Union(Circle value1, Circle value2)
        {
            float x = Math.Min(value1.X, value2.X);
            float y = Math.Min(value1.Y, value2.Y);
            return new Circle(x, y, value1.Radius + value2.Radius);

        }

        /// <summary>
        /// Creates a new <see cref="Circle"/> that completely contains two other circles.
        /// </summary>
        /// <param name="value1">The first <see cref="Circle"/>.</param>
        /// <param name="value2">The second <see cref="Circle"/>.</param>
        /// <param name="result">The union of the two rectangles as an output parameter.</param>
        public static void Union(ref Circle value1, ref Circle value2, out Circle result)
        {
            result = new Circle();
            result.X = Math.Min(value1.X, value2.X);
            result.Y = Math.Min(value1.Y, value2.Y);
            result.Radius = value1.Radius + value2.Radius;
        }
        #endregion

        #region Private Methods
        bool isInRectangle(double centerX, double centerY, double radius, double x, double y)
        {
            return x >= centerX - radius && x <= centerX + radius &&
                y >= centerY - radius && y <= centerY + radius;
        }
        bool isInCircle(double centerX, double centerY, double radius, double x, double y)
        {
            if (isInRectangle(this.X, this.Y, this.Radius, x, y))
            {
                double dx = this.X - x;
                double dy = this.Y - y;
                dx *= dx;
                dy *= dy;
                double distanceSquared = dx + dy;
                double radiusSquared = this.Radius * this.Radius;
                return distanceSquared <= radiusSquared;
            }
            return false;
        }
        #endregion
    }
}