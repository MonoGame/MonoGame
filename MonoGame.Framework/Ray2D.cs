// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Represents a semi-infinite ray in 2D space starting at an origin point and extending in a direction.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Ray2D : IEquatable<Ray2D>
    {
        #region Public Fields

        /// <summary>
        /// The direction vector defining which way this ray extends from its origin.
        /// Should be normalized for accurate distance calculations.
        /// </summary>
        [DataMember]
        public Vector2 Direction;

        /// <summary>
        /// The starting point of this ray in 2D space.
        /// </summary>
        [DataMember]
        public Vector2 Origin;

        #endregion

        #region Internal Properties

        internal readonly string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    "Origin( ", Origin.ToString(), " )  \r\n",
                    "Direction( ", Direction.ToString(), " )"
                );
            }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a new <see cref="Ray2D"/> with the specified origin and direction.
        /// </summary>
        /// <param name="origin">The starting point of the ray in 2D space.</param>
        /// <param name="direction">
        /// The direction vector defining which way the ray extends. Should be normalized for accurate distance calculations.
        /// </param>
        public Ray2D(Vector2 origin, Vector2 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        #endregion

        #region Public Methods

        #endregion

        /// <summary>
        /// Creates a <see cref="Ray2D"/> from a starting point toward a target point.
        /// </summary>
        /// <param name="start">The starting point of the ray in 2D space.</param>
        /// <param name="through">A target point the ray should pass through.</param>
        /// <returns>
        /// A new <see cref="Ray2D"/> starting at the specified point with a unit direction vector
        /// pointing toward the target point.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the points are too close to define a valid direction.
        /// </exception>
        public static Ray2D CreateFromPoints(Vector2 start, Vector2 through)
        {
            const float Epsilon = 1e-6f;

            Vector2 direction = through - start;
            float lengthSq = direction.LengthSquared();

            if (lengthSq < Epsilon * Epsilon)
            {
                throw new ArgumentException("Points must be distinct to define a ray direction");
            }

            return new Ray2D(start, Vector2.Normalize(direction));
        }

        /// <summary>
        /// Computes a point along this ray at the specified parametric distance.
        /// </summary>
        /// <param name="distanceAlongRay">The parametric distance along the ray from the origin.</param>
        /// <returns>
        /// The point at position <c>Origin + distanceAlongRay * Direction</c>.
        /// </returns>
        /// <remarks>
        /// Negative values of <paramref name="distanceAlongRay"/> return points behind the ray's origin.
        /// </remarks>
        public readonly Vector2 GetPoint(float distanceAlongRay)
        {
            return Origin + distanceAlongRay * Direction;
        }

        /// <summary>
        /// Computes the closest point on this ray to a specified point.
        /// </summary>
        /// <param name="point">The point in 2D space to find the closest point to.</param>
        /// <param name="distanceAlongRay">
        /// When this method returns, contains the parametric distance along the ray to the closest point.
        /// Will be clamped to zero or greater, as rays only extend forward from the origin.
        /// </param>
        /// <returns>
        /// The closest point on the ray. If the point projects behind the ray's origin, returns the origin itself.
        /// </returns>
        public readonly Vector2 ClosestPoint(Vector2 point, out float distanceAlongRay)
        {
            const float Epsilon = 1e-6f;

            Vector2 ab = Direction;

            float denom = Vector2.Dot(ab, ab);
            if (denom <= Epsilon)
            {
                // Direction of ray is effectively zero, so it's just a point.
                distanceAlongRay = 0.0f;
                return Origin;
            }

            // Project point ab, but deferring divide by Dot(ab, ab)
            distanceAlongRay = Vector2.Dot(point - Origin, ab);
            if (distanceAlongRay <= 0.0f)
            {
                // point projects before the ray origin, clamp to origin
                distanceAlongRay = 0.0f;
                return Origin;
            }

            // Point projects after the ray origin, must do deferred divide
            distanceAlongRay /= denom;
            return Origin + distanceAlongRay * ab;
        }

        /// <summary>
        /// Computes the squared distance from a point to the closest point on this ray.
        /// </summary>
        /// <param name="point">The point in 2D space to measure from.</param>
        /// <returns>
        /// The squared distance. Returns the distance to the origin if the point
        /// projects behind the ray's starting point, or the perpendicular distance if the point
        /// projects onto the ray.
        /// </returns>
        /// <remarks>
        /// This method returns squared distance to avoid the expensive square root operation,
        /// making it efficient for distance comparisons.
        /// </remarks>
        public readonly float DistanceSquaredToPoint(Vector2 point)
        {
            Vector2 closestPoint = ClosestPoint(point, out _);
            return Vector2.DistanceSquared(point, closestPoint);
        }

        /// <summary>
        /// Computes the distance from a point to the closest point on this ray.
        /// </summary>
        /// <param name="point">The point in 2D space to measure from.</param>
        /// <returns>
        /// The distance. Returns the distance to the origin if the point
        /// projects behind the ray's starting point, or the perpendicular distance if the point
        /// projects onto the ray.
        /// </returns>
        /// <remarks>
        /// This method performs a square root operation. For distance comparisons,
        /// use <see cref="DistanceSquaredToPoint"/> instead to avoid the computational cost.
        /// </remarks>
        public readonly float DistanceToPoint(Vector2 point)
        {
            float distSq = DistanceSquaredToPoint(point);
            return MathF.Sqrt(distSq);
        }

        /// <summary>
        /// Returns a normalized representation of the specified ray with a unit direction vector.
        /// </summary>
        /// <param name="value">The ray to normalize.</param>
        /// <param name="result">
        /// When this method returns, contains a <see cref="Ray2D"/> with the same origin but a unit direction vector.
        /// </param>
        public static void Normalize(ref Ray2D value, out Ray2D result)
        {
            result = new Ray2D(value.Origin, Vector2.Normalize(value.Direction));
        }

        /// <summary>
        /// Returns a normalized representation of the specified ray with a unit direction vector.
        /// </summary>
        /// <param name="value">The ray to normalize.</param>
        /// <returns>
        /// A new <see cref="Ray2D"/> with the same origin but a unit direction vector.
        /// </returns>
        public static Ray2D Normalize(Ray2D value)
        {
            Ray2D result;
            Normalize(ref value, out result);
            return result;
        }

        /// <summary>
        /// Normalizes this ray's direction vector to unit length.
        /// </summary>
        /// <remarks>
        /// After normalization, the <see cref="Direction"/> will be a unit vector while
        /// <see cref="Origin"/> remains unchanged.
        /// </remarks>
        public void Normalize()
        {
            Direction.Normalize();
        }

        /// <summary>
        /// Tests if this ray intersects with a line.
        /// </summary>
        /// <param name="line">The line to test against.</param>
        /// <param name="distanceAlongRay">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this ray
        /// to the intersection point.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="point">
        /// When this method returns <see langword="true"/>, contains the point where the ray and line intersect.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the ray and line intersect in the ray's forward direction;
        /// otherwise, <see langword="false"/> if they are parallel or the intersection point is behind the ray's origin.
        /// </returns>
        public readonly bool Intersects(Line2D line, out float? distanceAlongRay, out Vector2? point)
        {
            return line.Intersects(this, out distanceAlongRay, out point);
        }

        /// <summary>
        /// Tests if this ray intersects with a line.
        /// </summary>
        /// <param name="line">The line to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the ray and line intersect in the ray's forward direction;
        /// otherwise, <see langword="false"/> if they are parallel or the intersection point is behind the ray's origin.
        /// </returns>
        public readonly bool Intersects(Line2D line)
        {
            return line.Intersects(this);
        }

        /// <summary>
        /// Tests if this ray intersects with another ray.
        /// </summary>
        /// <param name="other">The other ray to test against.</param>
        /// <param name="distanceAlongRay1">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this ray
        /// to the intersection point.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="distanceAlongRay2">
        /// When this method returns <see langword="true"/>, contains the parametric distance along the other ray
        /// to the intersection point.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="point">
        /// When this method returns <see langword="true"/>, contains the point where the rays intersect.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if both rays intersect in their forward directions;
        /// otherwise, <see langword="false"/> if they are parallel or if the intersection point is behind either ray's origin.
        /// </returns>
        public readonly bool Intersects(Ray2D other, out float? distanceAlongRay1, out float? distanceAlongRay2, out Vector2? point)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Parametric intersection of two rays using 2D cross products
            // Derived from Section 5.1.9.1 "2D Segment Intersection" (line–line formulation)

            const float Epsilon = 1e-6f;

            // Check if rays are parallel
            float cross = Vector2.PerpDot(Direction, other.Direction);
            if (MathF.Abs(cross) < Epsilon)
            {
                distanceAlongRay1 = distanceAlongRay2 = null;
                point = null;
                return false;
            }

            Vector2 diff = other.Origin - Origin;

            // Solve for t1 (parameter on this ray)
            distanceAlongRay1 = Vector2.PerpDot(diff, other.Direction) / cross;

            // Only intersections in forward direction.  Negative direction indicates
            // intersection would have happened behind origin
            if (distanceAlongRay1 < 0.0f)
            {
                distanceAlongRay1 = distanceAlongRay2 = null;
                point = null;
                return false;
            }

            // Solve for t2 (parameter on other ray)
            distanceAlongRay2 = Vector2.PerpDot(diff, Direction) / cross;

            // Only intersections in forward direction.  Negative direction indicates
            // intersection would have happened behind the origin
            if (distanceAlongRay2 < 0.0f)
            {
                distanceAlongRay1 = distanceAlongRay2 = null;
                point = null;
                return false;
            }

            point = Origin + distanceAlongRay1 * Direction;
            return true;
        }

        /// <summary>
        /// Tests if this ray intersects with another ray.
        /// </summary>
        /// <param name="other">The other ray to test against.</param>
        /// <returns>
        /// <see langword="true"/> if both rays intersect in their forward directions;
        /// otherwise, <see langword="false"/> if they are parallel or if the intersection point is behind either ray's origin.
        /// </returns>
        public readonly bool Intersects(Ray2D other)
        {
            return Intersects(other, out _, out _, out _);
        }

        /// <summary>
        /// Tests if this ray intersects with a line segment.
        /// </summary>
        /// <param name="segment">The line segment to test against.</param>
        /// <param name="distanceAlongRay">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this ray
        /// to the intersection point.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="distanceAlongSegment">
        /// When this method returns <see langword="true"/>, contains the parametric distance along the segment
        /// to the intersection point, in the range [0, 1] where 0 represents the start and 1 represents the end.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="point">
        /// When this method returns <see langword="true"/>, contains the point where the ray and segment intersect.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the ray intersects the segment in its forward direction and within the segment's bounds;
        /// otherwise, <see langword="false"/> if they are parallel, the intersection is outside the segment, or behind the ray's origin.
        /// </returns>
        public readonly bool Intersects(LineSegment2D segment, out float? distanceAlongRay, out float? distanceAlongSegment, out Vector2? point)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Parametric intersection of a ray with a line segment using 2D cross products
            // Derived from Section 5.1.9.1 "2D Segment Intersection" (line–line formulation)

            const float Epsilon = 1e-6f;

            Vector2 segmentDir = segment.End - segment.Start;

            // Check if ray and segment are parallel
            float cross = Vector2.PerpDot(Direction, segmentDir);
            if (MathF.Abs(cross) < Epsilon)
            {
                // Parallel or colinear
                distanceAlongRay = distanceAlongSegment = null;
                point = null;
                return false;
            }

            Vector2 diff = segment.Start - Origin;

            // Solve for segment parameter s
            distanceAlongSegment = Vector2.PerpDot(diff, Direction) / cross;

            // Intersection only if within segment bounds [0,1]
            if (distanceAlongSegment < 0.0f || distanceAlongSegment > 1.0f)
            {
                distanceAlongRay = distanceAlongSegment = null;
                point = null;
                return false;
            }

            // Solve for ray parameter t
            distanceAlongRay = Vector2.PerpDot(diff, segmentDir) / cross;

            // Intersection only if in forward direction. Negative direction indicates
            // intersection would have happened behind ray origin
            if (distanceAlongRay < 0.0f)
            {
                distanceAlongSegment = distanceAlongRay = null;
                point = null;
                return false;
            }

            point = Origin + distanceAlongRay * Direction;
            return true;
        }

        /// <summary>Tests if this <see cref="Ray2D"/> intersects a <see cref="LineSegment2D"/>.</summary>
        /// <param name="segment">The <see cref="LineSegment2D"/> to test against.</param>
        /// <returns>
        /// <see langword="true"/> if this ray and <paramref name="segment"/> intersect; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(LineSegment2D segment)
        {
            return Intersects(segment, out _, out _, out _);
        }

        /// <summary>
        /// Tests if this ray intersects with an axis-aligned bounding box and computes the parametric distances to the intersection points.
        /// </summary>
        /// <param name="box">The bounding box to test against.</param>
        /// <param name="tRayMin">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this ray
        /// to the entry intersection point, where the intersection point equals <c>Origin + tRayMin * Direction</c>.
        /// If the ray origin is inside the bounding box, this will be <c>0</c>.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="tRayMax">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this ray
        /// to the exit intersection point, where the intersection point equals <c>Origin + tRayMax * Direction</c>.
        /// This is always greater than or equal to <paramref name="tRayMin"/>.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the ray intersects the bounding box in its forward direction;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingBox2D box, out float? tRayMin, out float? tRayMax)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Parametric intersection of a ray with an axis-aligned box (2D reduction)
            // Derived from Section 5.3.3 "Intersecting Ray or Segment Against Box"

            const float Epsilon = 1e-6f;

            float tMin = float.MinValue;
            float tMax = float.MaxValue;

            // X-axis slab
            if (MathF.Abs(Direction.X) < Epsilon)
            {
                // Ray is parallel to the x slab planes
                if (Origin.X < box.Min.X || Origin.X > box.Max.X)
                {
                    tRayMin = tRayMax = null;
                    return false;
                }
            }
            else
            {
                // Compute intersection t values with the near and far X planes
                float ood = 1.0f / Direction.X;
                float t1 = (box.Min.X - Origin.X) * ood;
                float t2 = (box.Max.X - Origin.X) * ood;

                // Make t1 be intersection with near plane and t2 with far plane
                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                tMin = MathF.Max(tMin, t1);
                tMax = MathF.Min(tMax, t2);

                // Is slab intersection empty?
                if (tMin > tMax)
                {
                    tRayMin = tRayMax = null;
                    return false;
                }
            }

            // Y-axis slab
            if (MathF.Abs(Direction.Y) < Epsilon)
            {
                // Ray is parallel to the x slab planes
                if (Origin.Y < box.Min.Y || Origin.Y > box.Max.Y)
                {
                    tRayMin = tRayMax = null;
                    return false;
                }
            }
            else
            {
                // Compute intersection t values with the near and far X planes
                float ood = 1.0f / Direction.Y;
                float t1 = (box.Min.Y - Origin.Y) * ood;
                float t2 = (box.Max.Y - Origin.Y) * ood;

                // Make t1 be intersection with near plane and t2 with far plane
                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                tMin = MathF.Max(tMin, t1);
                tMax = MathF.Min(tMax, t2);

                // Is slab intersection empty?
                if (tMin > tMax)
                {
                    tRayMin = tRayMax = null;
                    return false;
                }
            }

            // If ray origin is inside box (tMin < 0), return 0
            if (tMin < 0.0f && tMax > 0.0f)
            {
                tRayMin = 0.0f;
                tRayMax = tMax;
                return true;
            }

            // Ensure intersection is in forward direction
            if (tMax < 0.0f)
            {
                tRayMin = tRayMax = null;
                return false;
            }

            tRayMin = MathF.Max(0.0f, tMin);
            tRayMax = tMax;
            return true;
        }

        /// <summary>
        /// Tests if this ray intersects with an axis-aligned bounding box.
        /// </summary>
        /// <param name="box">The bounding box to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the ray intersects the bounding box in its forward direction;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingBox2D box)
        {
            return Intersects(box, out _, out _);
        }

        /// <summary>
        /// Tests if this ray intersects with a circle and computes the parametric distances to the intersection points.
        /// </summary>
        /// <param name="circle">The circle to test against.</param>
        /// <param name="tRayMin">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this ray
        /// to the first intersection point, where the intersection point equals <c>Origin + tRayMin * Direction</c>.
        /// If the ray origin is inside the circle, this will be <c>0</c>.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="tRayMax">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this ray
        /// to the second intersection point, where the intersection point equals <c>Origin + tRayMax * Direction</c>.
        /// This is always greater than or equal to <paramref name="tRayMin"/>.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the ray intersects the circle in its forward direction;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCircle circle, out float? tRayMin, out float? tRayMax)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Parametric intersection of a ray with a circle (2D reduction)
            // Derived from Section 5.3.2 "Intersecting Ray or Segment Against Sphere"

            Vector2 m = Origin - circle.Center;

            float b = Vector2.Dot(m, Direction);
            float c = Vector2.Dot(m, m) - circle.Radius * circle.Radius;

            // Ray origin outside circle (c > 0)
            // and ray pointing away from circle (b > 0)
            if (c > 0.0f && b > 0.0f)
            {
                tRayMin = tRayMax = null;
                return false;
            }

            float discriminant = b * b - c;

            // Negative discriminant means ray misses circle
            if (discriminant < 0.0f)
            {
                tRayMin = tRayMax = null;
                return false;
            }

            float sqrtDiscriminant = MathF.Sqrt(discriminant);
            float tMin = -b - sqrtDiscriminant;
            float tMax = -b + sqrtDiscriminant;

            // If ray origin is inside circle (tMin < 0), return 0
            if (tMin < 0.0f && tMax > 0.0f)
            {
                tRayMin = 0.0f;
                tRayMax = tMax;
                return true;
            }

            // If both intersections are behind ray origin, no valid intersection
            if (tMax < 0.0f)
            {
                tRayMin = tRayMax = null;
                return false;
            }

            tRayMin = MathF.Max(0.0f, tMin);
            tRayMax = tMax;
            return true;
        }

        /// <summary>
        /// Tests if this ray intersects with a circle.
        /// </summary>
        /// <param name="circle">The circle to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the ray intersects the circle in its forward direction;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCircle circle)
        {
            return Intersects(circle, out _, out _);
        }

        /// <summary>
        /// Tests if this ray intersects with a capsule and computes the parametric distances to the intersection points.
        /// </summary>
        /// <param name="capsule">The capsule to test against.</param>
        /// <param name="tRayMin">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this ray
        /// to the entry intersection point, where the intersection point equals Origin + tRayMin * Direction.
        /// If the ray origin is inside the capsule, this will be 0.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <param name="tRayMax">
        /// When this method returns <see langword="true"/>, contains the parametric distance along this ray
        /// to the exit intersection point, where the intersection point equals Origin + tRayMax * Direction.
        /// This is always greater than or equal to <paramref name="tRayMin"/>.
        /// When this method returns <see langword="false"/>, contains <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the ray intersects the capsule in its forward direction;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCapsule2D capsule, out float? tRayMin, out float? tRayMax)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Parametric intersection of a ray with a capsule (2D reduction)
            // Derived from Section 5.1.9 "Closest Points of Two Line Segments" and Section 5.3.7 "Intersecting Ray or Segment Against Cylinder"

            const float Epsilon = 1e-6f;

            Vector2 d1 = Direction;
            Vector2 d2 = capsule.PointB - capsule.PointA;
            Vector2 r = Origin - capsule.PointA;

            float a = Vector2.Dot(d1, d1);
            float e = Vector2.Dot(d2, d2);
            float f = Vector2.Dot(d2, r);

            // Handle degenerate capsule (segment is a point, just a circe).
            if (e <= Epsilon * Epsilon)
            {
                // capsule degenerates to a circle at Point A
                BoundingCircle circle = new BoundingCircle(capsule.PointA, capsule.Radius);
                return Intersects(circle, out tRayMin, out tRayMax);
            }

            float c = Vector2.Dot(d1, r);
            float b = Vector2.Dot(d1, d2);
            float denom = a * e - b * b;

            float radiusSq = capsule.Radius * capsule.Radius;

            // Check if ray and segment are parallel
            if (MathF.Abs(denom) < Epsilon)
            {
                // ray and segment are parallel
                // Compute perpendicular distance between the parallel lines
                Vector2 toSegment = capsule.PointA - Origin;
                float projectionOnRay = Vector2.Dot(toSegment, d1);
                Vector2 perpComponent = toSegment - projectionOnRay * d1;
                float perpDistanceSq = perpComponent.LengthSquared();

                if (perpDistanceSq > radiusSq)
                {
                    // Too far apart perpendicularly
                    tRayMin = tRayMax = null;
                    return false;
                }

                // Parallel and close enough, project capsule endpoints onto ray.
                Vector2 toA = capsule.PointA - Origin;
                Vector2 toB = capsule.PointB - Origin;
                float projA = Vector2.Dot(toA, d1);
                float projB = Vector2.Dot(toB, d1);

                // Find the interval on the ray where it's within radius of the capsule.
                float minProj = MathF.Min(projA, projB);
                float maxProj = MathF.Max(projA, projB);

                float parallelOffset = MathF.Sqrt(radiusSq - perpDistanceSq);

                tRayMin = minProj - parallelOffset;
                tRayMax = maxProj + parallelOffset;
                return true;
            }

            // General case: ray and segment are not parallel
            float s;
            float t;

            // Compute closest point on infinite lines
            s = (b * f - c * e) / denom;

            // Since this is a ray, clamp s to [0, positiveInfinity]
            if (s < 0.0f)
            {
                s = 0.0f;
                t = MathHelper.Clamp(f / e, 0.0f, 1.0f);
            }
            else
            {
                // Compute t corresponding to s
                t = (b * s + f) / e;

                // Clamp to to segment range
                if (t < 0.0f)
                {
                    t = 0.0f;
                    s = MathF.Max(-c / a, 0.0f);
                }
                else if (t > 1.0f)
                {
                    t = 1.0f;
                    s = MathF.Max((b - c) / a, 0.0f);
                }
            }

            // Compute closest points
            Vector2 closestOnRay = Origin + s * d1;
            Vector2 closestOnSegment = capsule.PointA + t * d2;

            // Check if closest points are within capsule radius
            Vector2 separation = closestOnRay - closestOnSegment;
            float distanceSq = separation.LengthSquared();

            if (distanceSq > radiusSq)
            {
                // Ray doesn't intersect capsule
                tRayMin = tRayMax = null;
                return false;
            }

            // Compute intersection parameters using Pythagorean theorem
            // The ray intersects a sphere of radius R at distance s from origin
            // We need to find how far along the ray from point s the entry and exit are
            float offset = MathF.Sqrt(radiusSq - distanceSq);

            float tMin = s - offset;
            float tMax = s + offset;

            // If ray origin is inside capsule (tMin < 0), return 0
            if (tMin < 0.0f && tMax > 0.0f)
            {
                tRayMin = 0.0f;
                tRayMax = tMax;
                return true;
            }

            // If both intersections are behind ray origin, no valid intersection
            if (tMax < 0.0f)
            {
                tRayMin = tRayMax = null;
                return false;
            }

            tRayMin = MathF.Max(0.0f, tMin);
            tRayMax = tMax;
            return true;
        }


        /// <summary>
        /// Tests if this ray intersects with a capsule.
        /// </summary>
        /// <param name="capsule">The capsule to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the ray intersects the capsule in its forward direction;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCapsule2D capsule)
        {
            return Intersects(capsule, out _, out _);
        }

        /// <summary>
        /// Deconstructs this ray into its component values.
        /// </summary>
        /// <param name="origin">
        /// When this method returns, contains the starting point of this ray in 2D space.
        /// </param>
        /// <param name="direction">
        /// When this method returns, contains the direction vector defining which way this ray extends.
        /// </param>
        public readonly void Deconstruct(out Vector2 origin, out Vector2 direction)
        {
            origin = Origin;
            direction = Direction;
        }

        /// <inheritdoc/>
        public readonly bool Equals(Ray2D other)
        {
            return Origin.Equals(other.Origin) && Direction.Equals(other.Direction);
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            return (obj is Ray2D other) && Equals(other);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return Origin.GetHashCode() ^ Direction.GetHashCode();
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return "{{Origin:" + Origin.ToString() + " Direction:" + Direction.ToString() + "}}";
        }

        /// <summary/>
        public static bool operator ==(Ray2D left, Ray2D right)
        {
            return left.Equals(right);
        }

        /// <summary/>
        public static bool operator !=(Ray2D left, Ray2D right)
        {
            return !left.Equals(right);
        }
    }
}
