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
            Vector2 direction = p2 - p1;
            float lengthSquared = direction.LengthSquared();

            if (lengthSquared < Collision2D.Epsilon * Collision2D.Epsilon)
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

            // This line is represented in implicit form:
            //      Dot(Normal, X) = Distance
            // We need to calculate X, as any point X that satisfies this
            // equation lies on the line.
            Vector2 n = Normal;
            float nn = Vector2.Dot(n, n);
            if (nn <= Collision2D.Epsilon)
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
            float length = value.Normal.Length();
            if (length < Collision2D.Epsilon)
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
            float length = Normal.Length();
            if (length > Collision2D.Epsilon)
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

            // Check if lines are parallel
            float cross = Normal.X * other.Normal.Y - Normal.Y * other.Normal.X;
            if (MathF.Abs(cross) < Collision2D.Epsilon)
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
            if (!Collision2D.SolveParametricIntersectionWithImplicitLine(Normal, Distance, ray.Origin, ray.Direction, out float t))
            {
                // Parallel or coincident
                distanceAlongRay = null;
                point = null;
                return false;
            }

            // Ray only intersects in forward direction
            if (t < 0.0f)
            {
                distanceAlongRay = null;
                point = null;
                return false;
            }

            distanceAlongRay = t;
            point = ray.Origin + t * ray.Direction;
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
            Vector2 ab = segment.End - segment.Start;

            if (!Collision2D.SolveParametricIntersectionWithImplicitLine(Normal, Distance, segment.Start, ab, out float t))
            {
                distanceAlongSegment = null;
                point = null;
                return false;
            }

            // Check if the intersection is within the segment bounds
            if (t < 0.0f || t > 1.0f)
            {
                distanceAlongSegment = null;
                point = null;
                return false;
            }

            distanceAlongSegment = t;
            point = segment.Start + t * ab;
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
        /// Tests if this line intersects with an axis-aligned bounding box.
        /// </summary>
        /// <param name="box">The bounding box to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the line passes through or touches the bounding box;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingBox2D box)
        {
            Vector2 n = Normal;
            float nn = Vector2.Dot(n, n);

            // Check for degenerate line
            if(nn <= Collision2D.Epsilon * Collision2D.Epsilon)
                return false;

            // Point on the line: a = n * (d / Dot(n,n))
            Vector2 origin = n * (Distance / nn);

            // Direction along the line (perpendicular to normal)
            Vector2 dir = new Vector2(-n.Y, n.X);

            return Collision2D.ClipLineToAabb(origin, dir, box.Min, box.Max, float.MinValue, float.MaxValue, out _, out _);
        }

        /// <summary>
        /// Tests if this line intersects with a circle.
        /// </summary>
        /// <param name="circle">The circle to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the line passes through or is tangent to the circle;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCircle circle)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Distance‑based intersection of implicit line with circle
            // Derived from Section 5.2.2 "Testing Sphere Against Plane" (2D reduction)

            // Compute signed distance from circle center to line
            float signedDist = DistanceToPoint(circle.Center);

            // Line intersects circle if perpendicular distance is within radius
            return MathF.Abs(signedDist) <= circle.Radius;
        }

        /// <summary>
        /// Tests if this line intersects with a capsule.
        /// </summary>
        /// <param name="capsule">The capsule to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the line passes through or is tangent to the capsule;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCapsule2D capsule)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Intersection of an implicit line with a 2D capsule (line segment swept by a circle)
            // Derived from Section 4.5.1 "Sphere-swept Volume Intersection" and Section 5.2.2 "Testing Sphere Against Plane"

            // Compute signed distance to capsule endpoints
            float signedDistA = DistanceToPoint(capsule.PointA);
            float signedDistB = DistanceToPoint(capsule.PointB);

            // Get absolute distances for comparison
            float distToA = MathF.Abs(signedDistA);
            float distToB = MathF.Abs(signedDistB);

            // minimum distance starts as the closer of the two end points
            float minDistSq = MathF.Min(distToA * distToA, distToB * distToB);

            // Check if capsule's medial segment is not degenerate
            Vector2 segmentDir = capsule.PointB - capsule.PointA;
            float segmentLenSq = segmentDir.LengthSquared();

            if (segmentLenSq > Collision2D.Epsilon * Collision2D.Epsilon)
            {
                // If endpoints are on opposite sides of the line, the segment crosses it
                if (signedDistA * signedDistB <= 0.0f)
                {
                    minDistSq = 0.0f;
                }
            }

            // Line intersects capsule if minimum distance is within radius
            return minDistSq <= capsule.Radius * capsule.Radius;
        }

        /// <summary>
        /// Tests if this line intersects with an oriented bounding box.
        /// </summary>
        /// <param name="obb">The oriented bounding box to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the line passes through or touches the box;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(OrientedBoundingBox2D obb)
        {
            Vector2 n = Normal;
            float nn = Vector2.Dot(n, n);

            // Handle degenerate line
            if(nn <= Collision2D.Epsilon * Collision2D.Epsilon)
                return false;

            // Get a world space point and direction for the line
            Vector2 a = n * (Distance / nn);
            Vector2 dir = new Vector2(-n.Y, n.X);

            // Transform line into OBB local space
            Vector2 diff = a - obb.Center;
            Vector2 localOrigin = new Vector2(Vector2.Dot(diff, obb.AxisX), Vector2.Dot(diff, obb.AxisY));
            Vector2 localDirection = new Vector2(Vector2.Dot(dir, obb.AxisX), Vector2.Dot(dir, obb.AxisY));

            // Local OBB is just ABB [-halfExtents, +halfExtents]
            return Collision2D.ClipLineToAabb(localOrigin, localDirection, -obb.HalfExtents, obb.HalfExtents, float.MinValue, float.MaxValue, out _, out _);
        }

        /// <summary>
        /// Tests if this line intersects with a polygon.
        /// </summary>
        /// <param name="polygon">The polygon to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the line passes through or is tangent to the polygon;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingPolygon2D polygon)
        {
            Vector2 n = Normal;
            float nn = Vector2.Dot(n, n);

            // Check for degenerate line
            if(nn <= Collision2D.Epsilon * Collision2D.Epsilon)
                return false;

            // A point on the line: a = n * (d / Dot(n,n))
            Vector2 a = n * (Distance / nn);

            // A direction along the line (perpendicular to n)
            Vector2 dir = new Vector2(-n.Y, n.X);

            // Clip infinite line against polygon half-spaces
            return Collision2D.ClipLineToConvexPolygon(a, dir, polygon.Vertices, polygon.Normals, float.MinValue, float.MaxValue, out _, out _);
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
            return Normal.Equals(other.Normal)
                   && Math.Abs(Distance - other.Distance) < Collision2D.Epsilon;
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
