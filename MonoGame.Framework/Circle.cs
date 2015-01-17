// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Describes a 2D-circle. 
	/// </summary>
	[DataContract]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct Circle : IEquatable<Circle>
	{
		#region Private Fields

		private static Circle emptyCircle = new Circle();

		#endregion

		#region Public Fields

		/// <summary>
		/// The point representing the center of this <see cref="Circle"/>.
		/// </summary>
		[DataMember]
		public Vector2 Center;

		/// <summary>
		/// The radius from the center of this <see cref="Circle"/>.
		/// </summary>
		[DataMember]
		public float Radius;

		#endregion

		#region Public Properties

		/// <summary>
		/// Returns a <see cref="Circle"/> with Point = Vector2.Zero and Radius= 0.
		/// </summary>
		public static Circle Empty
		{
			get { return emptyCircle; }
		}

		/// <summary>
		/// Returns the x coordinate of the far left point of this <see cref="Circle"/>.
		/// </summary>
		public float Left
		{
			get { return (this.Center.X - this.Radius); }
		}

		/// <summary>
		/// Returns the x coordinate of the far right point of this <see cref="Circle"/>.
		/// </summary>
		public float Right
		{
			get { return (this.Center.X + this.Radius); }
		}

		/// <summary>
		/// Returns the y coordinate of the far top point of this <see cref="Circle"/>.
		/// </summary>
		public float Top
		{
			get { return (this.Center.Y - this.Radius); }
		}

		/// <summary>
		/// Returns the y coordinate of the far bottom point of this <see cref="Circle"/>.
		/// </summary>
		public float Bottom
		{
			get { return (this.Center.Y + this.Radius); }
		}

		/// <summary>
		/// The center coordinates of this <see cref="Circle"/>.
		/// </summary>
		public Point Location
		{
			get
			{
				return this.Center.ToPoint();
			}
			set
			{
				this.Center.X = value.X;
				this.Center.Y = value.Y;
			}
		}

		/// <summary>
		/// Returns the diameter of this <see cref="Circle"/>
		/// </summary>
		public float Diameter
		{
			get { return (this.Radius * 2.0f); }
		}

		/// <summary>
		/// Returns the Circumference of this <see cref="Circle"/>
		/// </summary>
		public float Circumference
		{
			get { return (2.0f * MathHelper.Pi * Radius); }
		}

		/// <summary>
		/// Whether or not this <see cref="Circle"/> has a <see cref="Center"/> and
		/// <see cref="Radius"/> of 0.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return ((this.Radius == 0) && (this.Center == Vector2.Zero));
			}
		}

		#endregion

		#region Internal Properties

		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					this.Center.ToString(), "  ",
					this.Radius
					);
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new instance of <see cref="Circle"/> struct, with the specified
		/// position, and radius
		/// </summary>
		/// <param name="center">The position of the center of the created <see cref="Circle"/>.</param>
		/// <param name="radius">The radius of the created <see cref="Circle"/>.</param>
		public Circle(Vector2 center, float radius)
		{
			this.Center = center;
			this.Radius = radius;
		}

		#endregion

		#region Operators

		/// <summary>
		/// Compares whether two <see cref="Circle"/> instances are equal.
		/// </summary>
		/// <param name="a"><see cref="Circle"/> instance on the left of the equal sign.</param>
		/// <param name="b"><see cref="Circle"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(Circle a, Circle b)
		{
			return ((a.Center == b.Center) && (a.Radius == b.Radius));
		}

		/// <summary>
		/// Compares whether two <see cref="Circle"/> instances are not equal.
		/// </summary>
		/// <param name="a"><see cref="Circle"/> instance on the left of the not equal sign.</param>
		/// <param name="b"><see cref="Circle"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=(Circle a, Circle b)
		{
			return !(a == b);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the point at the edge of this <see cref="Circle"/> from the provided angle
		/// </summary>
		/// <param name="angle">an angle in radians</param>
		/// <returns><see cref="Vector2"/> representing the point on this <see cref="Circle"/>'s surface at the specified angle</returns>
		public Vector2 GetPointAlongEdge(float angle)
		{
			return new Vector2(Center.X + (Radius * (float)Math.Cos((double)angle)),
							   Center.Y + (Radius * (float)Math.Sin((double)angle)));
		}


		/// <summary>
		/// Gets whether or not the provided coordinates lie within the bounds of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="x">The x coordinate of the point to check for containment.</param>
		/// <param name="y">The y coordinate of the point to check for containment.</param>
		/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="Circle"/>; <c>false</c> otherwise.</returns>
		public bool Contains(float x, float y)
		{
			return ((new Vector2(x, y) - Center).Length() <= Radius);
		}
		
		/// <summary>
		/// Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="Circle"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="Circle"/>; <c>false</c> otherwise.</returns>
		public bool Contains(Point value)
		{
			return ((value.ToVector2() - Center).Length() <= Radius);
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="Circle"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="Circle"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Contains(ref Point value, out bool result)
		{
			result = ((value.ToVector2() - Center).Length() <= Radius);
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="Circle"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="Circle"/>; <c>false</c> otherwise.</returns>
		public bool Contains(Vector2 value)
		{
			return ((value - Center).Length() <= Radius);
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="Circle"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="Circle"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Contains(ref Vector2 value, out bool result)
		{
			result = ((value - Center).Length() <= Radius);
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Circle"/> lies within the bounds of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="value">The <see cref="Circle"/> to check for inclusion in this <see cref="Circle"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="Circle"/>'s center lie entirely inside this <see cref="Circle"/>; <c>false</c> otherwise.</returns>
		public bool Contains(Circle value)
		{
			Vector2 distanceOfCenter = value.Center - Center;
			float radii = Radius - value.Radius;

			return ((distanceOfCenter.X * distanceOfCenter.X) + (distanceOfCenter.Y * distanceOfCenter.Y) <= Math.Abs(radii * radii));
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Circle"/> lies within the bounds of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="value">The <see cref="Circle"/> to check for inclusion in this <see cref="Circle"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="Circle"/>'s center lie entirely inside this <see cref="Circle"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Contains(ref Circle value, out bool result)
		{
			Vector2 distanceOfCenter = value.Center - Center;
			float radii = Radius - value.Radius;

			result = ((distanceOfCenter.X * distanceOfCenter.X) + (distanceOfCenter.Y * distanceOfCenter.Y) <= Math.Abs(radii * radii));
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
		/// Compares whether current instance is equal to specified <see cref="Circle"/>.
		/// </summary>
		/// <param name="other">The <see cref="Circle"/> to compare.</param>
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
			return (Center.GetHashCode() ^ Radius.GetHashCode());
		}

		/// <summary>
		/// Adjusts the size of this <see cref="Circle"/> by specified radius amount. 
		/// </summary>
		/// <param name="radiusAmount">Value to adjust the radius by.</param>
		public void Inflate(float radiusAmount)
		{
			Center -= new Vector2(radiusAmount);
			Radius += radiusAmount * 2;
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="Circle"/> intersects with this <see cref="Circle"/>.
		/// </summary>
		/// <param name="value">Other <see cref="Circle"/>.</param>
		/// <returns><c>true</c> if other <see cref="Circle"/> intersects with this <see cref="Circle"/>; <c>false</c> otherwise.</returns>
		public bool Intersects(Circle value)
		{
			Vector2 distanceOfCenter = value.Center - Center;
			float radii = Radius + value.Radius;

			return ((distanceOfCenter.X * distanceOfCenter.X) + (distanceOfCenter.Y * distanceOfCenter.Y) < (radii * radii));
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="Circle"/> intersects with this <see cref="Circle"/>.
		/// </summary>
		/// <param name="value">Other <see cref="Circle"/>.</param>
		/// <param name="result"><c>true</c> if other <see cref="Circle"/> intersects with this <see cref="Circle"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Intersects(ref Circle value, out bool result)
		{
			Vector2 distanceOfCenter = value.Center - Center;
			float radii = Radius + value.Radius;

			result = ((distanceOfCenter.X * distanceOfCenter.X) + (distanceOfCenter.Y * distanceOfCenter.Y) < (radii * radii));
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="Rectangle"/> intersects with this <see cref="Circle"/>.
		/// </summary>
		/// <param name="value">Other <see cref="Rectangle"/>.</param>
		/// <returns><c>true</c> if other <see cref="Rectangle"/> intersects with this <see cref="Circle"/>; <c>false</c> otherwise.</returns>
		public bool Intersects(Rectangle value)
		{
			Vector2 distance = new Vector2(Math.Abs(Center.X - value.X), Math.Abs(Center.Y - value.Y));

			if (distance.X > (value.Width / 2.0f + Radius))  
				return false;
			if (distance.Y > (value.Height / 2.0f + Radius)) 
				return false;

			if (distance.X <= (value.Width / 2.0f))
				return true;
			if (distance.Y <= (value.Height / 2.0f))
				return true;


			float distanceOfCorners = ((distance.X - value.Width / 2.0f) * (distance.X - value.Width / 2.0f) +
									   (distance.Y - value.Height / 2.0f) * (distance.Y - value.Height / 2.0f));

			return (distanceOfCorners <= (Radius * Radius));
		}

        /// <summary>
        /// Gets whether or not a specified <see cref="Rectangle"/> intersects with this <see cref="Circle"/>.
        /// </summary>
        /// <param name="value">Other <see cref="Rectangle"/>.</param>
        /// <param name="result"><c>true</c> if other <see cref="Rectangle"/> intersects with this <see cref="Circle"/>; <c>false</c> otherwise. As an output parameter.</param>
        public void Intersects(ref Rectangle value, out bool result)
        {
            Vector2 distance = new Vector2(Math.Abs(Center.X - value.X), Math.Abs(Center.Y - value.Y));

            if (distance.X > (value.Width / 2.0f + Radius))
                result = false;
            if (distance.Y > (value.Height / 2.0f + Radius))
                result = false;

            if (distance.X <= (value.Width / 2.0f))
                result = true;
            if (distance.Y <= (value.Height / 2.0f))
                result = true;


            float distanceOfCorners = ((distance.X - value.Width / 2.0f) * (distance.X - value.Width / 2.0f) +
                                       (distance.Y - value.Height / 2.0f) * (distance.Y - value.Height / 2.0f));

            result = (distanceOfCorners <= (Radius * Radius));
        }

		/*
		/// <summary>
		/// Creates a new <see cref="Circle"/> that contains overlapping region of two other circles.
		/// </summary>
		/// <param name="value1">The first <see cref="Circle"/>.</param>
		/// <param name="value2">The second <see cref="Circle"/>.</param>
		/// <returns>Overlapping region of the two circles.</returns>
		public static Circle Intersect(Circle value1, Circle value2)
		{
			Circle circle;
			Intersect(ref value1, ref value2, out circle);
			return circle;
		}
		*/

		/*
		/// <summary>
		/// Creates a new <see cref="Circle"/> that contains overlapping region of two other circles.
		/// </summary>
		/// <param name="value1">The first <see cref="Circle"/>.</param>
		/// <param name="value2">The second <see cref="Circle"/>.</param>
		/// <param name="result">Overlapping region of the two circles as an output parameter.</param>
		public static void Intersect(ref Circle value1, ref Circle value2, out Circle result)
		{
			if (value1.Intersects(value2))
			{
				throw new NotImplementedException();
			}
			else
			{
				result = new Circle(Vector2.Zero, 0.0f);
			}
		}
		*/

		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="offsetX">The x coordinate to add to this <see cref="Circle"/>.</param>
		/// <param name="offsetY">The y coordinate to add to this <see cref="Circle"/>.</param>
		public void Offset(float offsetX, float offsetY)
		{
			Center.X += (int)offsetX;
			Center.Y += (int)offsetY;
		}

		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="amount">The x and y components to add to this <see cref="Circle"/>.</param>
		public void Offset(Point amount)
		{
			Center.X += amount.X;
			Center.Y += amount.Y;
		}

		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="amount">The x and y components to add to this <see cref="Circle"/>.</param>
		public void Offset(Vector2 amount)
		{
			Center.X += amount.X;
			Center.Y += amount.Y;
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="Circle"/> in the format:
		/// {Center:[<see cref="Center"/>] Radius:[<see cref="Radius"/>]}
		/// </summary>
		/// <returns><see cref="String"/> representation of this <see cref="Circle"/>.</returns>
		public override string ToString()
		{
			return "{Center:" + Center.ToString() + " Radius:" + Radius + "}";
		}

		/// <summary>
		/// Creates a <see cref="Rectangle"/> large enough to fit this <see cref="Circle"/>  
		/// </summary>
		/// <returns><see cref="Rectangle"/> which contains this <see cref="Circle"/></returns>
		public Rectangle ToRectangle()
		{
			return new Rectangle((int)(Center.X - (Radius / 2.0f)),
								 (int)(Center.Y - (Radius / 2.0f)),
								 (int)Radius,
								 (int)Radius);
		}

		/*
		/// <summary>
		/// Creates a new <see cref="Circle"/> that completely contains two other circles.
		/// </summary>
		/// <param name="value1">The first <see cref="Circle"/>.</param>
		/// <param name="value2">The second <see cref="Circle"/>.</param>
		/// <returns>The union of the two circles.</returns>
		public static Circle Union(Circle value1, Circle value2)
		{
			throw new NotImplementedException();
		}
		*/

		/*
		/// <summary>
		/// Creates a new <see cref="Circle"/> that completely contains two other circles.
		/// </summary>
		/// <param name="value1">The first <see cref="Circle"/>.</param>
		/// <param name="value2">The second <see cref="Circle"/>.</param>
		/// <param name="result">The union of the two circles as an output parameter.</param>
		public static void Union(ref Circle value1, ref Circle value2, out Circle result)
		{
			throw new NotImplementedException();
		}
		*/
				
		#endregion
	}
}
