// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{

    /// <summary>
    /// Represents a finite line segment connecting two endpoints in 2D space.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct LineSegment2D : IEquatable<LineSegment2D>
    {
        #region Public Fields

        /// <summary>
        /// The starting point of this line segment in 2D space.
        /// </summary>
        [DataMember]
        public Vector2 Start;

        /// <summary>
        /// The ending point of this line segment in 2D space.
        /// </summary>
        [DataMember]
        public Vector2 End;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the unnormalized direction vector from start to end.
        /// The length of this vector equals the length of the segment.
        /// </summary>
        public readonly Vector2 Direction
        {
            get
            {
                return End - Start;
            }
        }

        /// <summary>
        /// Gets the midpoint of this line segment, located halfway between the start and end points.
        /// </summary>
        public readonly Vector2 Midpoint
        {
            get
            {
                return (Start + End) * 0.5f;
            }
        }

        /// <summary>
        /// Gets the length of this line segment.
        /// For length comparisons, use <see cref="LengthSquared"/> to avoid the square root calculation.
        /// </summary>
        public readonly float Length
        {
            get
            {
                return Vector2.Distance(Start, End);
            }
        }

        /// <summary>
        /// Gets the squared length of this line segment.
        /// </summary>
        /// <remarks>
        /// This property avoids the expensive square root operation, making it efficient for length comparisons.
        /// </remarks>
        public readonly float LengthSquared
        {
            get
            {
                return Vector2.DistanceSquared(Start, End);
            }
        }

        #endregion

        #region Internal Properties

        internal readonly string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    "Start( ", Start.ToString(), " )  \r\n",
                    "End( ", End.ToString(), " )"
                );
            }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a new <see cref="LineSegment2D"/> connecting two specified points.
        /// </summary>
        /// <param name="start">The starting point of the line segment in 2D space.</param>
        /// <param name="end">The ending point of the line segment in 2D space.</param>
        public LineSegment2D(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Computes the smallest axis-aligned bounding box that contains this line segment.
        /// </summary>
        /// <returns>
        /// A <see cref="BoundingBox2D"/> that tightly encloses both endpoints of this segment.
        /// </returns>
        /// <remarks>
        /// The bounding box is computed using the component-wise minimum and maximum of the start and end points.
        /// </remarks>
        public readonly BoundingBox2D GetBounds()
        {
            Vector2 min = Vector2.Min(Start, End);
            Vector2 max = Vector2.Max(Start, End);
            return new BoundingBox2D(min, max);
        }

        /// <summary>
        /// Computes a point along this line segment at the specified parametric distance.
        /// </summary>
        /// <param name="distanceAlongSegment">
        /// The parametric distance along the segment from the start point, where 0 represents the start,
        /// 1 represents the end, and values between 0 and 1 lie on the segment.
        /// </param>
        /// <returns>
        /// The point at position <c>Start + distanceAlongSegment * (End - Start)</c>.
        /// Values outside [0, 1] return points beyond the segment's endpoints along the line defined by the segment.
        /// </returns>
        public readonly Vector2 GetPoint(float distanceAlongSegment)
        {
            return Start + distanceAlongSegment * (End - Start);
        }

        /// <summary>
        /// Computes the closest point on this line segment to a specified point.
        /// </summary>
        /// <param name="point">The point in 2D space to find the closest point to.</param>
        /// <param name="distanceAlongSegment">
        /// When this method returns, contains the parametric distance along the segment to the closest point,
        /// clamped to the range [0, 1] where 0 represents the start and 1 represents the end.
        /// </param>
        /// <returns>
        /// The closest point on the segment. Returns the start if the point projects before the segment,
        /// the end if the point projects beyond the segment, or the perpendicular projection if the point
        /// projects onto the segment.
        /// </returns>
        public readonly Vector2 ClosestPoint(Vector2 point, out float distanceAlongSegment)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.1.2 "Closest Point on Line Segment to Point"

            Vector2 ab = End - Start;

            // Project point onto ab, but deferring divide by Dot(ab, ab)
            distanceAlongSegment = Vector2.Dot(point - Start, ab);
            if (distanceAlongSegment <= 0.0f)
            {
                // point projects outside the [a,b] interval, on the a side; clamp to a
                distanceAlongSegment = 0.0f;
                return Start;
            }

            float denom = Vector2.Dot(ab, ab);
            if (distanceAlongSegment >= denom)
            {
                // point projects outside the [a,b] interval, on the b side; clamp to b
                distanceAlongSegment = 1.0f;
                return End;
            }

            // Point projects inside the [a,b] interval; must do deferred divide now
            distanceAlongSegment /= denom;
            return Start + distanceAlongSegment * ab;
        }

        /// <summary>
        /// Computes the squared distance from a point to the closest point on this line segment.
        /// </summary>
        /// <param name="point">The point in 2D space to measure from.</param>
        /// <returns>
        /// The squared distance.  Returns the distance to <see cref="Start"/> if the point
        /// projects before the segment, distance to <see cref="End"/> if it projects beyond, or the
        /// perpendicular distance if the point projects onto the segment.
        /// </returns>
        /// <remarks>
        /// This method returns squared distance to avoid the expensive square root operation,
        /// making it efficient for distance comparisons.
        /// </remarks>
        public readonly float DistanceSquaredToPoint(Vector2 point)
        {
            return Collision2D.DistanceSquaredPointSegment(point, Start, End, out _, out _);
        }

        /// <summary>
        /// Computes the distance from a point to the closest point on this line segment.
        /// </summary>
        /// <param name="point">The point in 2D space to measure from.</param>
        /// <returns>
        /// The distance. Returns the distance to <see cref="Start"/> if the point
        /// projects before the segment, distance to <see cref="End"/> if it projects beyond, or the
        /// perpendicular distance if the point projects onto the segment.
        /// </returns>
        /// <remarks>
        /// This method performs a square root operation. For distance comparisons,
        /// use <see cref="DistanceSquaredToPoint"/> instead to avoid the computational cost.
        /// </remarks>
        public readonly float DistanceToPoint(Vector2 point)
        {
            return MathF.Sqrt(DistanceSquaredToPoint(point));
        }

        /// <summary>
        /// Computes the squared distance between this line segment and another line segment.
        /// </summary>
        /// <param name="other">The other line segment to measure distance to.</param>
        /// <param name="distanceAlongSegment1">
        /// When this method returns, contains the parametric distance along this segment to the closest point,
        /// in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// </param>
        /// <param name="distanceAlongSegment2">
        /// When this method returns, contains the parametric distance along the other segment to the closest point,
        /// in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// </param>
        /// <param name="closestPoint1">
        /// When this method returns, contains the closest point on this segment to the other segment.
        /// </param>
        /// <param name="closestPoint2">
        /// When this method returns, contains the closest point on the other segment to this segment.
        /// </param>
        /// <returns>
        /// The squared distance in between the two closest points on the segments.
        /// </returns>
        /// <remarks>
        /// This method returns squared distance to avoid the expensive square root operation,
        /// making it efficient for distance comparisons.
        /// </remarks>
        public readonly float DistanceSquaredToSegment(LineSegment2D other, out float distanceAlongSegment1, out float distanceAlongSegment2, out Vector2 closestPoint1, out Vector2 closestPoint2)
        {
            return Collision2D.DistanceSquaredSegmentSegment(Start, End, other.Start, other.End,
                                                             out distanceAlongSegment1, out distanceAlongSegment2,
                                                             out closestPoint1, out closestPoint2);
        }


        /// <summary>
        /// Computes the distance between this line segment and another line segment.
        /// </summary>
        /// <param name="other">The other line segment to measure distance to.</param>
        /// <returns>
        /// The shortest distance between any two points on the segments.
        /// </returns>
        /// <remarks>
        /// This method performs a square root operation. For distance comparisons,
        /// use <see cref="DistanceSquaredToSegment"/> instead to avoid the computational cost.
        /// </remarks>
        public readonly float DistanceToSegment(LineSegment2D other)
        {
            return MathF.Sqrt(DistanceSquaredToSegment(other, out _, out _, out _, out _));
        }

        /// <summary>
        /// Tests if this line segment intersects with a line.
        /// </summary>
        /// <param name="line">The line to test against.</param>
        /// <param name="distanceAlongSegment">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this segment
        /// to the intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="point">
        /// When this method returns <see langword="true"/>, contains the point where the segment and line intersect.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the line within its bounds;
        /// otherwise, <see langword="false"/> if they are parallel or the intersection point lies outside the segment.
        /// </returns>
        public readonly bool Intersects(Line2D line, out float? distanceAlongSegment, out Vector2? point)
        {
            return line.Intersects(this, out distanceAlongSegment, out point);
        }

        /// <summary>
        /// Tests if this line segment intersects with a line.
        /// </summary>
        /// <param name="line">The line to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the line within its bounds;
        /// otherwise, <see langword="false"/> if they are parallel or the intersection point lies outside the segment.
        /// </returns>
        public readonly bool Intersects(Line2D line)
        {
            return line.Intersects(this);
        }

        /// <summary>
        /// Tests if this line segment intersects with a ray.
        /// </summary>
        /// <param name="ray">The ray to test against.</param>
        /// <param name="distanceAlongSegment">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this segment
        /// to the intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="distanceAlongRay">
        /// When this method returns <see langword="true"/>, contains the parametric distance along the ray
        /// to the intersection point.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="point">
        /// When this method returns <see langword="true"/>, contains the point where the segment and ray intersect.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the ray within the segment's bounds and the ray's forward
        /// direction; otherwise, <see langword="false"/> if they are parallel, the intersection is outside the segment,
        /// or behind the ray's origin.
        /// </returns>
        public readonly bool Intersects(Ray2D ray, out float? distanceAlongSegment, out float? distanceAlongRay, out Vector2? point)
        {
            return ray.Intersects(this, out distanceAlongRay, out distanceAlongSegment, out point);
        }

        /// <summary>
        /// Tests if this line segment intersects with a ray.
        /// </summary>
        /// <param name="ray">The ray to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the ray within the segment's bounds and the ray's forward
        /// direction; otherwise, <see langword="false"/> if they are parallel, the intersection is outside the segment,
        /// or behind the ray's origin.
        /// </returns>
        public readonly bool Intersects(Ray2D ray)
        {
            return ray.Intersects(this);
        }

        /// <summary>
        /// Tests if this line segment intersects with another line segment.
        /// </summary>
        /// <param name="other">The other line segment to test against.</param>
        /// <param name="distanceAlongSegment1">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this segment
        /// to the intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="distanceAlongSegment2">
        /// When this method returns <see langword="true"/>, contains the parametric distance along the other segment
        /// to the intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="point">
        /// When this method returns <see langword="true"/>, contains the point where the segments intersect.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if both segments intersect within their bounds;
        /// otherwise, <see langword="false"/> if they are parallel or if the intersection point lies outside either segment.
        /// </returns>
        public readonly bool Intersects(LineSegment2D other, out float? distanceAlongSegment1, out float? distanceAlongSegment2, out Vector2? point)
        {
            if (!Collision2D.SolveParametricIntersection2D(Start, Direction, other.Start, other.Direction, out float t1, out float t2))
            {
                distanceAlongSegment1 = distanceAlongSegment2 = null;
                point = null;
                return false;
            }

            // Clamp to segment bounds [0,1]
            if (t1 < 0.0f || t1 > 1.0f || t2 < 0.0f || t2 > 1.0f)
            {
                distanceAlongSegment1 = distanceAlongSegment2 = null;
                point = null;
                return false;
            }

            distanceAlongSegment1 = t1;
            distanceAlongSegment2 = t2;
            point = Start + t1 * Direction;
            return true;
        }

        /// <summary>
        /// Tests if this line segment intersects with another line segment.
        /// </summary>
        /// <param name="other">The other line segment to test against.</param>
        /// <returns>
        /// <see langword="true"/> if both segments intersect within their bounds;
        /// otherwise, <see langword="false"/> if they are parallel or if the intersection point lies outside either segment.
        /// </returns>
        public readonly bool Intersects(LineSegment2D other)
        {
            return Intersects(other, out _, out _, out _);
        }

        /// <summary>
        /// Tests if this line segment intersects with an axis-aligned bounding box and computes the parametric distances to the intersection points.
        /// </summary>
        /// <param name="box">The bounding box to test against.</param>
        /// <param name="tMin">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this segment
        /// to the entry intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="tMax">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this segment
        /// to the exit intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the bounding box; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// For degenerate segments (zero length), returns <see langword="true"/> with tMin = tMax = 0
        /// if the start point is inside the bounding box.
        /// </remarks>
        public readonly bool Intersects(BoundingBox2D box, out float? tMin, out float? tMax)
        {
            Vector2 d = Direction;
            float dd = Vector2.Dot(d, d);

            // Handle degenerate segment (zero length)
            if (dd < Collision2D.Epsilon * Collision2D.Epsilon)
            {
                bool inside = Start.X >= box.Min.X && Start.X <= box.Max.X &&
                              Start.Y >= box.Min.Y && Start.Y <= box.Max.Y;

                if (inside)
                {
                    tMin = tMax = 0.0f;
                    return true;
                }

                tMin = tMax = null;
                return false;
            }

            if (!Collision2D.ClipLineToAabb(Start, d, box.Min, box.Max, 0.0f, 1.0f, out float tEnter, out float tExit))
            {
                tMin = tMax = null;
                return false;
            }

            tMin = tEnter;
            tMax = tExit;
            return true;
        }

        /// <summary>
        /// Tests if this line segment intersects with an axis-aligned bounding box.
        /// </summary>
        /// <param name="box">The bounding box to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the bounding box; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingBox2D box)
        {
            return Intersects(box, out _, out _);
        }

        /// <summary>
        /// Tests if this line segment intersects with a circle and computes the parametric distances to the intersection points.
        /// </summary>
        /// <param name="circle">The circle to test against.</param>
        /// <param name="tSegmentMin">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this segment
        /// to the entry intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="tSegmentMax">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this segment
        /// to the exit intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the circle; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// For degenerate segments (zero length), returns <see langword="true"/> with tSegmentMin = tSegmentMax = 0
        /// if the start point is inside the circle.
        /// </remarks>
        public readonly bool Intersects(BoundingCircle circle, out float? tSegmentMin, out float? tSegmentMax)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Parametric intersection of a segment with a circle (2D reduction)
            // Derived from Section 5.3.2 "Intersecting Ray or Segment Against Sphere"

            Vector2 d = Direction;
            float dd = Vector2.Dot(d, d);

            // Handle degenerate segment (zero length)
            if (dd <= Collision2D.Epsilon * Collision2D.Epsilon)
            {
                float distSq = Vector2.DistanceSquared(Start, circle.Center);
                if (distSq <= circle.Radius * circle.Radius)
                {
                    tSegmentMin = tSegmentMax = 0.0f;
                    return true;
                }

                tSegmentMin = tSegmentMax = null;
                return false;
            }

            if (!Collision2D.RayCircleIntersectionInterval(Start, d, circle.Center, circle.Radius, out float tMin, out float tMax))
            {
                tSegmentMin = tSegmentMax = null;
                return false;
            }

            // Clip ray interval to segment interval [0,1]
            if (!Collision2D.ClipInterval(tMin, tMax, 0.0f, 1.0f, out float enter, out float exit))
            {
                tSegmentMin = tSegmentMax = null;
                return false;
            }

            tSegmentMin = enter;
            tSegmentMax = exit;
            return true;
        }

        /// <summary>
        /// Tests if this line segment intersects with a circle.
        /// </summary>
        /// <param name="circle">The circle to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the circle; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCircle circle)
        {
            return Intersects(circle, out _, out _);
        }

        /// <summary>
        /// Deconstructs this line segment into its component values.
        /// </summary>
        /// <param name="start">
        /// When this method returns, contains the starting point of this line segment in 2D space.
        /// </param>
        /// <param name="end">
        /// When this method returns, contains the ending point of this line segment in 2D space.
        /// </param>
        public readonly void Deconstruct(out Vector2 start, out Vector2 end)
        {
            start = Start;
            end = End;
        }

        /// <summary>
        /// Tests if this line segment intersects with a capsule and computes the parametric distances to the intersection points.
        /// </summary>
        /// <param name="capsule">The capsule to test against.</param>
        /// <param name="tMin">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this segment
        /// to the entry intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="tMax">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this segment
        /// to the exit intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the capsule; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// For degenerate segments (zero length), returns <see langword="true"/> with tMin = tMax = 0
        /// if the start point is inside the capsule.
        /// </remarks>
        public readonly bool Intersects(BoundingCapsule2D capsule, out float? tMin, out float? tMax)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Parametric intersection of a segment with a capsule
            // Derived from Section 5.1.9 "Closest Points of Two Line Segments"
            // and Section 5.3.7 "Intersecting Ray or Segment Against Cylinder"

            Vector2 d = Direction;
            float dd = Vector2.Dot(d, d);
            float radiusSq = capsule.Radius * capsule.Radius;

            // Check for degenerate segment
            if (dd <= Collision2D.Epsilon * Collision2D.Epsilon)
            {
                // Segment is degenerate, treat as point
                float distSq = Collision2D.DistanceSquaredPointSegment(Start, capsule.PointA, capsule.PointB, out _, out _);
                if (distSq <= radiusSq)
                {
                    tMin = tMax = 0.0f;
                    return true;
                }

                tMin = tMax = null;
                return false;
            }

            // Get intersection interval against the infinite parametric line P(t)=Start + t * d
            if (!Collision2D.RayCapsuleIntersectionInterval(Start, d, capsule.PointA, capsule.PointB, capsule.Radius, out float t0, out float t1))
            {
                tMin = tMax = null;
                return false;
            }

            if (!Collision2D.ClipInterval(t0, t1, 0.0f, 1.0f, out float enter, out float exit))
            {
                tMin = tMax = null;
                return false;
            }

            tMin = enter;
            tMax = exit;
            return true;
        }

        /// <summary>
        /// Tests if this line segment intersects with a capsule.
        /// </summary>
        /// <param name="capsule">The capsule to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the capsule; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCapsule2D capsule)
        {
            return Intersects(capsule, out _, out _);
        }

        /// <summary>
        /// Tests if this line segment intersects with an oriented bounding box and computes the parametric distances to the intersection points.
        /// </summary>
        /// <param name="obb">The oriented bounding box to test against.</param>
        /// <param name="tMin">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this segment
        /// to the entry intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="tMax">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this segment
        /// to the exit intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the box; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// For degenerate segments (zero length), returns <see langword="true"/> with tMin = tMax = 0
        /// if the start point is inside the box.
        /// </remarks>
        public readonly bool Intersects(OrientedBoundingBox2D obb, out float? tMin, out float? tMax)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Parametric intersection of a segment with an oriented box
            // Derived from Section 5.3.3 "Intersecting Ray or Segment Against Box"

            Vector2 d = Direction;
            float dd = Vector2.Dot(d, d);
            Vector2 diff = Start - obb.Center;

            // Check for degenerate segment (zero length)
            if (dd <= Collision2D.Epsilon * Collision2D.Epsilon)
            {
                // Segment is degenerate, treat it as a point
                float x = Vector2.Dot(diff, obb.AxisX);
                float y = Vector2.Dot(diff, obb.AxisY);

                bool inside = MathF.Abs(x) <= obb.HalfExtents.X &&
                              MathF.Abs(y) <= obb.HalfExtents.Y;

                if (inside)
                {
                    tMin = tMax = 0.0f;
                    return true;
                }

                tMin = tMax = null;
                return false;
            }

            // Transform segment start and direction into OBB local space
            Vector2 localOrigin = new Vector2(
                Vector2.Dot(diff, obb.AxisX),
                Vector2.Dot(diff, obb.AxisY)
            );

            Vector2 localDirection = new Vector2(
                Vector2.Dot(d, obb.AxisX),
                Vector2.Dot(d, obb.AxisY)
            );

            // Intersect the parametric line:
            // P(T) = localOrigin + t * localDirection
            // with local AABB [-halfExtent, +halfExtent]
            if (!Collision2D.ClipLineToAabb(localOrigin, localDirection, -obb.HalfExtents, obb.HalfExtents, 0.0f, 1.0f, out float enter, out float exit))
            {
                tMin = tMax = null;
                return false;
            }


            tMin = enter;
            tMax = exit;
            return true;
        }

        /// <summary>
        /// Tests if this line segment intersects with an oriented bounding box.
        /// </summary>
        /// <param name="obb">The oriented bounding box to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the box; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(OrientedBoundingBox2D obb)
        {
            return Intersects(obb, out _, out _);
        }

        /// <summary>
        /// Tests if this line segment intersects with a polygon and computes the parametric distances to the intersection points.
        /// </summary>
        /// <param name="polygon">The polygon to test against.</param>
        /// <param name="tMin">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this segment
        /// to the entry intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="tMax">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this segment
        /// to the exit intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="point">
        /// When this method returns <see langword="true"/>, contains the intersection point corresponding to <paramref name="tMin"/>.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the polygon; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// For degenerate segments (zero length), returns <see langword="true"/> with tMin = tMax = 0
        /// if the start point is inside the polygon.
        /// </remarks>
        public readonly bool Intersects(BoundingPolygon2D polygon, out float? tMin, out float? tMax, out Vector2? point)
        {
            Vector2 d = Direction;
            float dd = Vector2.Dot(d, d);

            // Check for degenerate segment
            if (dd <= Collision2D.Epsilon * Collision2D.Epsilon)
            {
                // Segment is degenerate, treat as point
                if (polygon.Contains(Start) == ContainmentType.Contains)
                {
                    tMin = tMax = 0.0f;
                    point = Start;
                    return true;
                }

                tMin = tMax = null;
                point = null;
                return false;
            }

            // Clip the segment's parametric line against the polygon
            if (!Collision2D.ClipLineToConvexPolygon(Start, d, polygon.Vertices, polygon.Normals, 0.0f, 1.0f, out float t0, out float t1))
            {
                tMin = tMax = null;
                point = null;
                return false;
            }

            tMin = t0;
            tMax = t1;
            point = Start + d * t0;
            return true;
        }

        /// <summary>
        /// Tests if this line segment intersects with a polygon.
        /// </summary>
        /// <param name="polygon">The polygon to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the segment intersects the polygon; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingPolygon2D polygon)
        {
            return Intersects(polygon, out _, out _, out _);
        }

        /// <inheritdoc/>
        public override readonly bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is LineSegment2D other && Equals(other);
        }

        /// <inheritdoc/>
        public readonly bool Equals(LineSegment2D other)
        {
            return Start.Equals(other.Start)
                   && End.Equals(other.End);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"LineSegment2D {{ Start: {Start}, End: {End}, Length: {Length:F3} }}";
        }

        /// <summary/>
        public static bool operator ==(LineSegment2D left, LineSegment2D right)
        {
            return left.Equals(right);
        }

        /// <summary/>
        public static bool operator !=(LineSegment2D left, LineSegment2D right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}
