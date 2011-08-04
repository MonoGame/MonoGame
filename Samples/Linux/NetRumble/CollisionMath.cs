#region File Description
//-----------------------------------------------------------------------------
// CollisionMath.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace NetRumble
{
    /// <summary>
    /// A code container for collision-related mathematical functions.
    /// </summary>
    static public class CollisionMath
    {
        #region Helper Types


        /// <summary>
        /// Data defining a circle/line collision result.
        /// </summary>
        /// <remarks>Also used for circle/rectangles.</remarks>
        public struct CircleLineCollisionResult
        {
            public bool Collision;
            public Vector2 Point;
            public Vector2 Normal;
            public float Distance;
        }

        
        #endregion


        #region Collision Methods


        /// <summary>
        /// Determines the point of intersection between two line segments, 
        /// as defined by four points.
        /// </summary>
        /// <param name="a">The first point on the first line segment.</param>
        /// <param name="b">The second point on the first line segment.</param>
        /// <param name="c">The first point on the second line segment.</param>
        /// <param name="d">The second point on the second line segment.</param>
        /// <param name="point">The output value with the interesection, if any.</param>
        /// <remarks>The output parameter "point" is only valid
        /// when the return value is true.</remarks>
        /// <returns>True if intersecting, false otherwise.</returns>
        public static bool LineLineIntersect(Vector2 a, Vector2 b, Vector2 c, 
            Vector2 d, out Vector2 point)
        {
            point = Vector2.Zero;

            double r, s;
            double denominator = (b.X - a.X) * (d.Y - c.Y) - (b.Y - a.Y) * (d.X - c.X);

            // If the denominator in above is zero, AB & CD are colinear
            if (denominator == 0)
            {
                return false;
            }

            double numeratorR = (a.Y - c.Y) * (d.X - c.X) - (a.X - c.X) * (d.Y - c.Y);
            r = numeratorR / denominator;

            double numeratorS = (a.Y - c.Y) * (b.X - a.X) - (a.X - c.X) * (b.Y - a.Y);
            s = numeratorS / denominator;

            // non-intersecting
            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            // find intersection point
            point.X = (float)(a.X + (r * (b.X - a.X)));
            point.Y = (float)(a.Y + (r * (b.Y - a.Y)));

            return true;
        }


        /// <summary>
        /// Determine if two circles intersect or contain each other.
        /// </summary>
        /// <param name="center1">The center of the first circle.</param>
        /// <param name="radius1">The radius of the first circle.</param>
        /// <param name="center2">The center of the second circle.</param>
        /// <param name="radius2">The radius of the second circle.</param>
        /// <returns>True if the circles intersect or contain one another.</returns>
        public static bool CircleCircleIntersect(Vector2 center1, float radius1, 
            Vector2 center2, float radius2)
        {
            Vector2 line = center2 - center1;
            // we use LengthSquared to avoid a costly square-root call
            return (line.LengthSquared() <= (radius1 + radius2) * (radius1 + radius2));
        }


        /// <summary>
        /// Determines if a circle and line segment intersect, and if so, how they do.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="result">The result data for the collision.</param>
        /// <returns>True if a collision occurs, provided for convenience.</returns>
        public static bool CircleRectangleCollide(Vector2 center, float radius,
            Rectangle rectangle, ref CircleLineCollisionResult result)
        {
            float xVal = center.X;
            if (xVal < rectangle.Left) xVal = rectangle.Left;
            if (xVal > rectangle.Right) xVal = rectangle.Right;

            float yVal = center.Y;
            if (yVal < rectangle.Top) yVal = rectangle.Top;
            if (yVal > rectangle.Bottom) yVal = rectangle.Bottom;

            Vector2 direction = new Vector2(center.X-xVal,center.Y-yVal);
            float distance = direction.Length();

            if ((distance > 0) && (distance < radius))
            {
                result.Collision = true;
                result.Distance = radius - distance;
                result.Normal = Vector2.Normalize(direction);
                result.Point = new Vector2(xVal, yVal);
            }
            else
            {
                result.Collision = false;
            }

            return result.Collision;
        }


        /// <summary>
        /// Determines if a circle and line segment intersect, and if so, how they do.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="lineStart">The first point on the line segment.</param>
        /// <param name="lineEnd">The second point on the line segment.</param>
        /// <param name="result">The result data for the collision.</param>
        /// <returns>True if a collision occurs, provided for convenience.</returns>
        public static bool CircleLineCollide(Vector2 center, float radius,
            Vector2 lineStart, Vector2 lineEnd, ref CircleLineCollisionResult result)
        {
            Vector2 AC = center - lineStart;
            Vector2 AB = lineEnd - lineStart;
            float ab2 = AB.LengthSquared();
            if (ab2 <= 0f)
            {
                return false;
            }
            float acab = Vector2.Dot(AC, AB);
            float t = acab / ab2;

            if (t < 0.0f)
                t = 0.0f;
            else if (t > 1.0f)
                t = 1.0f;

            result.Point = lineStart + t * AB;
            result.Normal = center - result.Point;

            float h2 = result.Normal.LengthSquared();
            float r2 = radius * radius;

            if ((h2 > 0) && (h2 <= r2))
            {
                result.Normal.Normalize();
                result.Distance = (radius - (center - result.Point).Length());
                result.Collision = true;
            }
            else
            {
                result.Collision = false;
            }

            return result.Collision;
        }


        #endregion
    }
}
