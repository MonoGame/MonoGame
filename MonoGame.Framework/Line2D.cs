// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Represents an infinite line in 2D space that extends in both directions without bounds.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Line2D : IEquatable<Line2D>
    {
        #region Public Fields

        /// <summary>
        /// The signed perpendicular distance from the origin to this line along its normal direction.
        /// Positive values indicate the origin is on the opposite side of the line from the normal.
        /// </summary>
        [DataMember]
        public float Distance;

        /// <summary>
        /// The unit normal vector perpendicular to this line, used with <see cref="Distance"/>
        /// to define the line's position and orientation.
        /// </summary>
        [DataMember]
        public Vector2 Normal;

        #endregion

        #region Internal Properties

        internal readonly string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    "Normal( ", Normal.ToString(), " )  \r\n",
                    "Distance( ", Distance.ToString(), " )"
                );
            }
        }

        #endregion

        #region  Public Constructors

        /// <summary>
        /// Creates a new <see cref="Line2D"/> with the specified normal and distance from the origin.
        /// </summary>
        /// <param name="normal">The unit normal vector perpendicular to the line.</param>
        /// <param name="distance">The perpendicular distance from the origin to the line along its normal direction.</param>
        public Line2D(Vector2 normal, float distance)
        {
            Normal = normal;
            Distance = distance;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a <see cref="Line2D"/> that passes through a specified point with a given normal direction.
        /// </summary>
        /// <param name="point">A point in 2D space that the line passes through.</param>
        /// <param name="normal">
        /// The normal vector perpendicular to the line. This vector will be normalized automatically.
        /// </param>
        /// <returns>
        /// A new <see cref="Line2D"/> with a unit normal passing through the specified point.
        /// </returns>
        public static Line2D CreateFromPointAndNormal(Vector2 point, Vector2 normal)
        {
            Vector2 n = Vector2.Normalize(normal);
            float d = Vector2.Dot(n, point);
            return new Line2D(n, d);
        }

        /// <summary>
        /// Creates a <see cref="Line2D"/> that passes through two specified points.
        /// </summary>
        /// <param name="p1">The first point in 2D space.</param>
        /// <param name="p2">The second point in 2D space, must be distinct from the first.</param>
        /// <returns>
        /// A new <see cref="Line2D"/> passing through both points. The line's normal is oriented
        /// 90 degrees counter-clockwise from the direction vector p1 to p2.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the two points are too close to define a unique line.
        /// </exception>
        public static Line2D CreateFromTwoPoints(Vector2 p1, Vector2 p2)
        {
            const float Epsilon = 1e-6f;

            Vector2 direction = p2 - p1;
            float lengthSquared = direction.LengthSquared();

            if (lengthSquared < Epsilon * Epsilon)
            {
                throw new ArgumentException("Points must be distinct to define a line.");
            }

            // Normal is perpendicular to direction (rotate 90deg CCW)
            Vector2 normal = new Vector2(-direction.Y, direction.X);
            return CreateFromPointAndNormal(p1, normal);
        }

        /// <summary>
        /// Creates a <see cref="Line2D"/> that passes through a specified point and extends in a given direction.
        /// </summary>
        /// <param name="point">A point in 2D space that the line passes through.</param>
        /// <param name="direction">
        /// The direction vector the line extends along. This vector will be normalized automatically.
        /// </param>
        /// <returns>
        /// A new <see cref="Line2D"/> with a unit normal perpendicular to the direction, passing through the specified point.
        /// </returns>
        public static Line2D CreateFromPointAndDirection(Vector2 point, Vector2 direction)
        {
            // Normal is perpendicular to direction (rotate 90deg CCW)
            Vector2 normal = Vector2.Normalize(new Vector2(-direction.Y, direction.X));
            float distance = Vector2.Dot(normal, point);
            return new Line2D(normal, distance);
        }

        /// <summary>
        /// Computes the signed perpendicular distance from a point to this line.
        /// </summary>
        /// <param name="point">The point in 2D space to measure from.</param>
        /// <returns>
        /// The signed distance from the point to the line. Positive values indicate the point is on
        /// the same side as the normal vector, negative values indicate the opposite side, and zero
        /// indicates the point lies on the line.
        /// </returns>
        public readonly float DistanceToPoint(Vector2 point)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.1.1 "Closest Point on Plane To Point"
            // Adapted from 3D plane to 2D line using implicit line equation

            return Vector2.Dot(Normal, point) - Distance;
        }

        /// <summary>
        /// Computes the closest point on this line to a specified point.
        /// </summary>
        /// <param name="point">The point in 2D space to project onto the line.</param>
        /// <param name="distanceAlongLine">
        /// When this method returns, contains the parametric distance along the line's direction vector
        /// to the closest point, where <c>distanceAlongLine = 0</c> corresponds to the point on the line
        /// closest to the origin.
        /// </param>
        /// <returns>
        /// The point on the line closest to the specified point, computed as the perpendicular projection
        /// of the point onto the line.
        /// </returns>
        public readonly Vector2 ClosestPoint(Vector2 point, out float distanceAlongLine)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.1.2 "Closest Point on Line Segment to Point"
            // Applied to an infinite line (no clamping of t), as described by Ericson.

            const float Epsilon = 1e-6f;

            // This line is represented in implicit form:
            //      Dot(Normal, X) = Distance
            // We need to calculate X, as any point X that satisfies this
            // equation lies on the line.
            Vector2 n = Normal;
            float nn = Vector2.Dot(n, n);
            if (nn <= Epsilon)
            {
                // Degenerate line, normal has no meaningful direction
                // Treat line as a single point
                distanceAlongLine = 0.0f;
                return Vector2.Zero;
            }

            // Compute a specific point 'a' on the line by scaling the normal.
            // Since Dot(n, n * (Distance / Dot(N, n))) = Distance
            // this point satisfies the line equation
            Vector2 a = n * (Distance / nn);

            // Compute a direction vector 'ab' that lies along the line.
            // In 2D a direction perpendicular to the normal is given by (-n.Y, n.X)
            Vector2 ab = new Vector2(-n.Y, n.X);

            float denom = Vector2.Dot(ab, ab);
            distanceAlongLine = Vector2.Dot(point - a, ab);
            distanceAlongLine /= denom;

            return a + distanceAlongLine * ab;
        }

        /// <summary>
        /// Returns a normalized representation of the specified line with a unit normal vector.
        /// </summary>
        /// <param name="line">The line to normalize.</param>
        /// <returns>
        /// A new <see cref="Line2D"/> representing the same geometric line with a unit normal vector.
        /// </returns>
        public static Line2D Normalize(Line2D line)
        {
            Line2D result;
            Normalize(ref line, out result);
            return result;
        }

        /// <summary>
        /// Returns a normalized representation of the specified line with a unit normal vector.
        /// </summary>
        /// <param name="value">The line to normalize.</param>
        /// <param name="result">
        /// When this method returns, contains a <see cref="Line2D"/> representing the same geometric line
        /// with a unit normal vector and proportionally adjusted distance.
        /// </param>
        public static void Normalize(ref Line2D value, out Line2D result)
        {
            const float Epsilon = 1e-6f;

            float length = value.Normal.Length();
            if (length < Epsilon)
            {
                result = value;
                return;
            }

            result = new Line2D(value.Normal / length, value.Distance / length);
        }

        /// <summary>
        /// Normalizes this line's representation by ensuring the normal vector has unit length.
        /// </summary>
        /// <remarks>
        /// After normalization, the <see cref="Normal"/> will be a unit vector and <see cref="Distance"/>
        /// will be adjusted proportionally to maintain the same geometric line.
        /// </remarks>
        public void Normalize()
        {
            const float Epsilon = 1e-6f;

            float length = Normal.Length();
            if (length > Epsilon)
            {
                Normal /= length;
                Distance /= length;
            }
        }

        /// <summary>
        /// Tests if this line intersects with another line.
        /// </summary>
        /// <param name="other">The other line to test against.</param>
        /// <param name="point">
        /// When this method returns <see langword="true"/>, contains the point where the lines intersect.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/> indicating
        /// the lines are parallel or coincident.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the lines intersect at a single point; otherwise, <see langword="false"/>
        /// if the lines are parallel or coincident.
        /// </returns>
        public readonly bool Intersects(Line2D other, out Vector2? point)
        {
            // Use implicit line representation and Cramer's rule
            // to solve a 2D line-line intersection
            const float Epsilon = 1e-6f;

            // Check if lines are parallel
            float cross = Normal.X * other.Normal.Y - Normal.Y * other.Normal.X;
            if (MathF.Abs(cross) < Epsilon)
            {
                // Lines are parallel or coincident
                point = null;
                return false;
            }

            float x = (Distance * other.Normal.Y - other.Distance * Normal.Y) / cross;
            float y = (other.Distance * Normal.X - Distance * other.Normal.X) / cross;
            point = new Vector2(x, y);
            return true;
        }

        /// <summary>
        /// Tests if this line intersects with another line.
        /// </summary>
        /// <param name="other">The other line to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the lines intersect at a single point; otherwise, <see langword="false"/>
        /// if the lines are parallel or coincident.
        /// </returns>
        public readonly bool Intersects(Line2D other)
        {
            return Intersects(other, out _);
        }

        /// <summary>
        /// Tests if this line intersects with a ray.
        /// </summary>
        /// <param name="ray">The ray to test against.</param>
        /// <param name="distanceAlongRay">
        /// When this method returns <see langword="true"/>, contains the parametric distance along the ray
        /// to the intersection point.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="point">
        /// When this method returns <see langword="true"/>, contains the point where the line and ray intersect.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the line and ray intersect in the ray's forward direction;
        /// otherwise, <see langword="false"/> if they are parallel or the intersection point is behind the ray's origin.
        /// </returns>
        public readonly bool Intersects(Ray2D ray, out float? distanceAlongRay, out Vector2? point)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Parametric intersection of a ray with an implicit line
            // Derived from Section 5.3.1 "Intersecting Segment Against Plane" (2D reduction)

            const float Epsilon = 1e-6f;

            // Check if ray is parallel to the line
            float denom = Vector2.Dot(Normal, ray.Direction);
            if (MathF.Abs(denom) < Epsilon)
            {
                // Parallel or coincident
                distanceAlongRay = null;
                point = null;
                return false;
            }

            // Compute parametric distance along ray
            float distance = Distance - Vector2.Dot(Normal, ray.Origin);
            distanceAlongRay = distance / denom;

            // Ray only intersects in forward direction
            if (distanceAlongRay < 0.0f)
            {
                distanceAlongRay = null;
                point = null;
                return false;
            }

            point = ray.Origin + distanceAlongRay * ray.Direction;
            return true;
        }

        /// <summary>
        /// Tests if this line intersects with a ray.
        /// </summary>
        /// <param name="ray">The ray to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the line and ray intersect in the ray's forward direction;
        /// otherwise, <see langword="false"/> if they are parallel or the intersection point is behind the ray's origin.
        /// </returns>
        public readonly bool Intersects(Ray2D ray)
        {
            return Intersects(ray, out _, out _);
        }

        /// <summary>
        /// Tests if this line intersects with a line segment.
        /// </summary>
        /// <param name="segment">The line segment to test against.</param>
        /// <param name="distanceAlongSegment">
        /// When this method returns <see langword="true"/>, contains the parametric distance along the segment
        /// to the intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="point">
        /// When this method returns <see langword="true"/>, contains the point where the line and segment intersect.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the line intersects the segment within its bounds;
        /// otherwise, <see langword="false"/> if they are parallel or the intersection point lies outside the segment.
        /// </returns>
        public readonly bool Intersects(LineSegment2D segment, out float? distanceAlongSegment, out Vector2? point)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Parametric intersection of a line segment with an implicit line
            // Derived from Section 5.3.1 "Intersecting Segment Against Plane" (2D reduction)

            const float Epsilon = 1e-6f;

            Vector2 ab = segment.End - segment.Start;
            float denom = Vector2.Dot(Normal, ab);

            // Check if segment is parallel to the line
            if (MathF.Abs(denom) < Epsilon)
            {
                distanceAlongSegment = null;
                point = null;
                return false;
            }

            // Compute parametric distance along segment
            float signedDistance = Distance - Vector2.Dot(Normal, segment.Start);
            distanceAlongSegment = signedDistance / denom;

            // Check if the intersection is within the segment bounds
            if (distanceAlongSegment < 0.0f || distanceAlongSegment > 1.0f)
            {
                distanceAlongSegment = null;
                point = null;
                return false;
            }

            point = segment.Start + distanceAlongSegment * ab;
            return true;
        }


        /// <summary>
        /// Tests if this line intersects with a line segment.
        /// </summary>
        /// <param name="segment">The line segment to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the line intersects the segment within its bounds;
        /// otherwise, <see langword="false"/> if they are parallel or the intersection point lies outside the segment.
        /// </returns>
        public readonly bool Intersects(LineSegment2D segment)
        {
            return Intersects(segment, out _, out _);
        }

        /// <summary>
        /// Deconstructs this line into its component values.
        /// </summary>
        /// <param name="normal">
        /// When this method returns, contains the unit normal vector perpendicular to this line.
        /// </param>
        /// <param name="distance">
        /// When this method returns, contains the signed perpendicular distance from the origin
        /// to this line along its normal direction.
        /// </param>
        public readonly void Deconstruct(out Vector2 normal, out float distance)
        {
            normal = Normal;
            distance = Distance;
        }

        /// <inheritdoc/>
        public readonly bool Equals(Line2D other)
        {
            const float Epsilon = 1e-6f;

            return Normal.Equals(other.Normal)
                   && Math.Abs(Distance - other.Distance) < Epsilon;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            return obj is Line2D other && Equals(other);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return Normal.GetHashCode() ^ Distance.GetHashCode();
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return "{Normal:" + Normal.ToString() + " Distance:" + Distance.ToString() + "}";
        }

        /// <summary/>
        public static bool operator ==(Line2D left, Line2D right)
        {
            return left.Equals(right);
        }

        /// <summary/>
        public static bool operator !=(Line2D left, Line2D right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}
