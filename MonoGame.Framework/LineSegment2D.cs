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
            Vector2 ab = End - Start;
            Vector2 ac = point - Start;
            Vector2 bc = point - End;

            float e = Vector2.Dot(ac, ab);

            // Handle cases where point projects outside ab
            if (e <= 0.0f)
            {
                return Vector2.Dot(ac, ac);
            }

            float f = Vector2.Dot(ab, ab);

            if (e >= f)
            {
                return Vector2.Dot(bc, bc);
            }

            return Vector2.Dot(ac, ac) - e * e / f;
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
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.1.9 "Closest Points of Two Line Segments"

            const float Epsilon = 1e-6f;

            Vector2 d1 = End - Start;               // Direction vector of segment s!
            Vector2 d2 = other.End - other.Start;   // Direction vector of segment S2
            Vector2 r = Start - other.Start;

            float a = Vector2.Dot(d1, d1);  // Squared length of segment s!, always nonnegative
            float e = Vector2.Dot(d2, d2);  // Squared length of segment S2, always nonnegative
            float f = Vector2.Dot(d2, r);

            // Check if either or both segments degenerate into points
            if (a <= Epsilon && e <= Epsilon)
            {
                distanceAlongSegment1 = distanceAlongSegment2 = 0.0f;
                closestPoint1 = Start;
                closestPoint2 = other.Start;
                return Vector2.Dot(closestPoint1 - closestPoint2, closestPoint1 - closestPoint2);
            }

            if (a <= Epsilon)
            {
                // First segment degenerates into a point
                distanceAlongSegment1 = 0.0f;
                distanceAlongSegment2 = f / e; // s = 0 => t = (b*s + f) / e = f / e
                distanceAlongSegment2 = MathHelper.Clamp(distanceAlongSegment2, 0.0f, 1.0f);
            }
            else
            {
                float c = Vector2.Dot(d1, r);

                if (e <= Epsilon)
                {
                    // Second segment degenerates into a point
                    distanceAlongSegment2 = 0.0f;
                    distanceAlongSegment1 = MathHelper.Clamp(-c / a, 0.0f, 1.0f); // t = 0 => s = (b*t - c) / a = - c / a
                }
                else
                {
                    // The general nondegenerate case stats here
                    float b = Vector2.Dot(d1, d2);
                    float denom = a * e - b * b; // Always nonnegative

                    // If segments not parallel, compute closest point on L1 to L2 and
                    // clamp to segment s!.  Else pick arbitrary s (here 0)
                    if (denom != 0.0f)
                    {
                        distanceAlongSegment1 = MathHelper.Clamp((b * f - c * e) / denom, 0.0f, 1.0f);
                    }
                    else
                    {
                        distanceAlongSegment1 = 0.0f;
                    }

                    // Compute point on L2 closest to S1(s) using
                    // t = Dot((P1 + D1*s) - P2,D2) / Dot(D2,D2) = (b*s + f) / e
                    distanceAlongSegment2 = (b * distanceAlongSegment1 + f) / e;

                    // If t in [0,1] don't. Else clamp t, recompute s for the new value
                    // of t using s = Dot((P2 + D2*t) - P1,D1) / Dot(D1,D1) = (t*b - c) / a
                    // and clamp s to [0, 1]
                    if (distanceAlongSegment2 < 0.0f)
                    {
                        distanceAlongSegment2 = 0.0f;
                        distanceAlongSegment1 = MathHelper.Clamp(-c / a, 0.0f, 1.0f);
                    }
                    else if (distanceAlongSegment2 > 1.0f)
                    {
                        distanceAlongSegment2 = 1.0f;
                        distanceAlongSegment1 = MathHelper.Clamp((b - c) / a, 0.0f, 1.0f);
                    }
                }
            }

            closestPoint1 = Start + d1 * distanceAlongSegment1;
            closestPoint2 = other.Start + d2 * distanceAlongSegment2;
            return Vector2.Dot(closestPoint1 - closestPoint2, closestPoint1 - closestPoint2);
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
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Parametric 2D segment–segment intersection using cross products
            // See section 5.1.9.1 "2D Segment Intersection" (line intersection formulation)

            const float Epsilon = 1e-6f;

            Vector2 d1 = Direction;
            Vector2 d2 = other.Direction;
            Vector2 r = other.Start - Start;

            float d1CrossD2 = d1.X * d2.Y - d1.Y * d2.X;

            // Check if segments are parallel
            if (MathF.Abs(d1CrossD2) < Epsilon)
            {
                distanceAlongSegment1 = distanceAlongSegment2 = null;
                point = null;
                return false;
            }

            float rCrossD2 = r.X * d2.Y - r.Y * d2.X;
            distanceAlongSegment1 = rCrossD2 / d1CrossD2;

            // Validate that intersection is in range [0, 1] of this segment
            if (distanceAlongSegment1 < 0.0f || distanceAlongSegment1 > 1.0f)
            {
                distanceAlongSegment1 = distanceAlongSegment2 = null;
                point = null;
                return false;
            }

            float rCrossD1 = r.X * d1.Y - r.Y * d1.X;
            distanceAlongSegment2 = rCrossD1 / d1CrossD2;

            // Validate that intersection is in range [0, 1] of other segment
            if (distanceAlongSegment2 < 0.0f || distanceAlongSegment2 > 1.0f)
            {
                distanceAlongSegment1 = distanceAlongSegment2 = null;
                point = null;
                return false;
            }

            point = Start + distanceAlongSegment1 * d1;
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
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Parametric intersection of a segment with an axis-aligned box
            // Derived from Section 5.3.3 "Intersecting Ray or Segment Against Box"

            const float Epsilon = 1e-6f;

            Vector2 direction = Direction;
            float segmentLengthSq = direction.LengthSquared();

            // Handle degenerate segment (zero length)
            if (segmentLengthSq < Epsilon * Epsilon)
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

            float segmentLength = MathF.Sqrt(segmentLengthSq);
            Ray2D ray = new Ray2D(Start, direction / segmentLength);

            float? rayTMin;
            float? rayTMax;
            if (ray.Intersects(box, out rayTMin, out rayTMax))
            {
                float segmentTMin = rayTMin.Value / segmentLength;
                float segmentTMax = rayTMax.Value / segmentLength;

                if (segmentTMax < 0.0f || segmentTMin > 1.0f)
                {
                    tMin = tMax = null;
                    return false;
                }

                tMin = MathF.Max(0.0f, segmentTMin);
                tMax = MathF.Min(1.0f, segmentTMax);
                return true;
            }

            tMin = tMax = null;
            return false;
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

            const float Epsilon = 1e-6f;

            Vector2 direction = Direction;
            float segmentLenSq = direction.LengthSquared();

            // Handle degenerate segment (zero length)
            if (segmentLenSq < Epsilon * Epsilon)
            {
                float distSq = Vector2.DistanceSquared(Start, circle.Center);
                if (distSq < circle.Radius * circle.Radius)
                {
                    tSegmentMin = tSegmentMax = 0.0f;
                    return true;
                }

                tSegmentMin = tSegmentMax = null;
                return false;
            }

            float segmentLength = MathF.Sqrt(segmentLenSq);
            Ray2D ray = new Ray2D(Start, direction / segmentLength);

            float? tRayMin;
            float? tRayMax;
            if (ray.Intersects(circle, out tRayMin, out tRayMax))
            {
                float tMin = tRayMin.Value / segmentLength;
                float tMax = tRayMax.Value / segmentLength;

                if (tMax < 0.0f || tMin > 1.0f)
                {
                    tSegmentMin = tSegmentMax = null;
                    return false;
                }

                tSegmentMin = MathF.Max(0.0f, tMin);
                tSegmentMax = MathF.Min(1.0f, tMax);
                return true;
            }

            tSegmentMin = null;
            tSegmentMax = null;
            return false;
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

            const float Epsilon = 1e-6f;

            Vector2 segmentDir = Direction;
            float segmentLengthSq = segmentDir.LengthSquared();

            // Handle degenerate segment (zero length)
            if (segmentLengthSq < Epsilon * Epsilon)
            {
                LineSegment2D capsuleSegment = new LineSegment2D(capsule.PointA, capsule.PointB);
                float distSq = capsuleSegment.DistanceSquaredToPoint(Start);

                if (distSq <= capsule.Radius * capsule.Radius)
                {
                    tMin = tMax = 0.0f;
                    return true;
                }

                tMin = tMax = null;
                return false;
            }

            // Handle degenerate capsule segment (circle)
            float capsuleLengthSq = capsule.Length * capsule.Length;
            if (capsuleLengthSq < Epsilon * Epsilon)
            {
                // Capsule is just a circle at PointA
                BoundingCircle circle = new BoundingCircle(capsule.PointA, capsule.Radius);
                float segmentLength = MathF.Sqrt(segmentLengthSq);
                Ray2D ray = new Ray2D(Start, segmentDir / segmentLength);

                float? rayTMin;
                float? rayTMax;
                if (!ray.Intersects(circle, out rayTMin, out rayTMax))
                {
                    tMin = tMax = null;
                    return false;
                }

                // Convert ray t values to segment t values
                float segmentTMin = rayTMin.Value / segmentLength;
                float segmentTMax = rayTMax.Value / segmentLength;

                // Check if intersection overlaps with segment bounds [0, 1]
                if (segmentTMax < 0.0f || segmentTMin > 1.0f)
                {
                    tMin = tMax = null;
                    return false;
                }

                tMin = MathF.Max(0.0f, segmentTMin);
                tMax = MathF.Min(1.0f, segmentTMax);
                return true;
            }

            // General case: use segment-segment distance
            LineSegment2D capsuleSegment2 = new LineSegment2D(capsule.PointA, capsule.PointB);
            float distSqBetweenSegments = DistanceSquaredToSegment(capsuleSegment2, out _, out _, out _, out _);

            float radiusSq = capsule.Radius * capsule.Radius;

            // Quick rejection if segments are too far apart
            if (distSqBetweenSegments > radiusSq)
            {
                tMin = tMax = null;
                return false;
            }

            // Segments are close enough, solve for intersection points.
            // Cast the segment as a ray and use ray-capsule intersection
            float segmentLength2 = MathF.Sqrt(segmentLengthSq);
            Ray2D ray2 = new Ray2D(Start, segmentDir / segmentLength2);

            float? rayTMin2;
            float? rayTMax2;
            if (!ray2.Intersects(capsule, out rayTMin2, out rayTMax2))
            {
                tMin = tMax = null;
                return false;
            }

            // Convert ray t values to segment t values
            float segmentTMin2 = rayTMin2.Value / segmentLength2;
            float segmentTMax2 = rayTMax2.Value / segmentLength2;

            // Check if intersection overlaps with segment bounds [0, 1]
            if (segmentTMax2 < 0.0f || segmentTMin2 > 1.0f)
            {
                tMin = tMax = null;
                return false;
            }

            // Clamp to segment bounds
            tMin = MathF.Max(0.0f, segmentTMin2);
            tMax = MathF.Min(1.0f, segmentTMax2);
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
