// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Provides low-level, allocation-free geometric queries and helper routines for 2D collision detection.
    /// </summary>
    /// <remarks>
    /// This class contains stateless, static methods for intersection tests, containment classification, distance
    /// queries, interval clipping, and projection operations between common 2D primitives such as points, line
    /// segments, rays, circles, axis-aligned bounding boxes (AABBs), oriented bounding boxes (OBBs), capsules,
    /// and convex polygons.
    ///
    /// The APIs are intended to be used as shared building blocks by higher-level geometric types (for example,
    /// bounding volumes and shapes) rather than consumed directly in most application code. Methods are designed
    /// to be deterministic, side-effect free, and suitable for performance-critical use, including tight update
    /// loops and broad-phase or narrow-phase collision detection.
    ///
    /// Many algorithms implemented here are standard results from computational geometry and collision detection
    /// literature, including distance-based tests and separating-axis tests (SAT). Where appropriate, individual
    /// methods include in-code attribution to published sources such as Christer Ericson’s
    /// <em>Real-Time Collision Detection</em>.
    ///
    /// This class is not thread-affine and contains no shared mutable state; all methods are thread-safe provided
    /// the supplied arguments are not mutated concurrently by the caller.
    /// </remarks>
    internal static class Collision2D
    {
        #region Constants

        /// <summary>
        /// A small tolerance value used to account for floating-point imprecision in geometric computations.
        /// </summary>
        /// <remarks>
        /// This constant is used to stabilize comparisons involving distances, projections, and dot products where
        /// exact equality cannot be reliably assumed due to floating-point rounding behavior.
        /// </remarks>
        public const float Epsilon = 1e-6f;

        /// <summary>
        /// The square of <see cref="Epsilon"/>.
        /// </summary>
        /// <remarks>
        /// This value is provided for efficiency when comparing squared distances, avoiding unnecessary square root
        /// operations while maintaining consistent tolerance behavior.
        /// </remarks>
        public const float EpsilonSq = Epsilon * Epsilon;

        #endregion

        #region Helpers

        /// <summary>
        /// Determines whether the specified convex polygon data is structurally valid for use by collision routines.
        /// </summary>
        /// <param name="vertices">
        /// The polygon vertices in 2D space. A valid polygon requires at least three vertices.
        /// </param>
        /// <param name="normals">
        /// The per-edge outward unit normals. The array length must match <paramref name="vertices"/> so that
        /// <c>normals[i]</c> corresponds to the edge from <c>vertices[i]</c> to <c>vertices[(i + 1) % vertices.Length]</c>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="vertices"/> and <paramref name="normals"/> are non-<see langword="null"/>,
        /// <paramref name="vertices"/> contains at least three elements, and both arrays have the same length;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidPolygon(Vector2[] vertices, Vector2[] normals)
        {
            if (vertices == null || normals == null)
                return false;

            int count = vertices.Length;
            return count >= 3 && normals.Length == count;
        }

        /// <summary>
        /// Intersects a parametric interval with a clipping interval and updates the result in place.
        /// </summary>
        /// <param name="tMin">
        /// On input, the lower bound of the parametric interval. On output, the lower bound of the
        /// intersected interval.
        /// </param>
        /// <param name="tMax">
        /// On input, the upper bound of the parametric interval. On output, the upper bound of the
        /// intersected interval.
        /// </param>
        /// <param name="clipMin">The lower bound of the clipping interval.</param>
        /// <param name="clipMax">The upper bound of the clipping interval.</param>
        /// <returns>
        /// <see langword="true"/> if the two intervals overlap after clipping; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method is commonly used in parametric line, ray, and segment intersection algorithms,
        /// where <c>t</c> represents a scalar parameter along a direction vector and successive calls
        /// progressively restrict the valid range.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ClipInterval(ref float tMin, ref float tMax, float clipMin, float clipMax)
        {
            if (clipMin > tMin) tMin = clipMin;
            if (clipMax < tMax) tMax = clipMax;
            return tMin <= tMax;
        }

        /// <summary>
        /// Computes the intersection of two parametric intervals and returns the clipped result.
        /// </summary>
        /// <param name="tEnter">The lower bound of the first parametric interval.</param>
        /// <param name="tExit">The upper bound of the first parametric interval.</param>
        /// <param name="tLower">The lower bound of the clipping interval.</param>
        /// <param name="tUpper">The upper bound of the clipping interval.</param>
        /// <param name="clippedEnter">
        /// When this method returns, contains the lower bound of the intersected interval.
        /// </param>
        /// <param name="clippedExit">
        /// When this method returns, contains the upper bound of the intersected interval.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the intervals overlap and the resulting interval is non-empty;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This overload performs interval clipping without modifying the input parameters and is suitable
        /// for algorithms that require preservation of the original bounds, such as line, ray, or segment
        /// intersection tests.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ClipInterval(float tEnter, float tExit, float tLower, float tUpper, out float clippedEnter, out float clippedExit)
        {
            clippedEnter = MathF.Max(tEnter, tLower);
            clippedExit = MathF.Min(tExit, tUpper);
            return clippedEnter <= clippedExit;
        }

        /// <summary>
        /// Determines whether two scalar intervals overlap or touch within a numeric tolerance.
        /// </summary>
        /// <param name="minA">The lower bound of the first interval.</param>
        /// <param name="maxA">The upper bound of the first interval.</param>
        /// <param name="minB">The lower bound of the second interval.</param>
        /// <param name="maxB">The upper bound of the second interval.</param>
        /// <returns>
        /// <see langword="true"/> if the intervals overlap or touch;otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Intervals that touch at a boundary are considered overlapping, which is required
        /// for separating-axis tests and projection-based intersection checks.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IntervalsOverlap(float minA, float maxA, float minB, float maxB)
        {
            // touch counts as overlap
            if (maxA < minB - Epsilon) return false;
            if (maxB < minA - Epsilon) return false;
            return true;
        }

        #endregion

        #region Containment

        #region Containment (AABB Contains)

        /// <summary>
        /// Determines whether a point is contained within an axis-aligned bounding box (AABB).
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="min">The minimum corner of the AABB.</param>
        /// <param name="max">The maximum corner of the AABB.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the point lies inside the bounding box or on its
        /// boundary; otherwise, <see cref="ContainmentType.Disjoint"/>.
        /// </returns>
        public static ContainmentType ContainsAabbPoint(Vector2 point, Vector2 min, Vector2 max)
        {
            if (point.X < min.X - Epsilon) return ContainmentType.Disjoint;
            if (point.X > max.X + Epsilon) return ContainmentType.Disjoint;
            if (point.Y < min.Y - Epsilon) return ContainmentType.Disjoint;
            if (point.Y > max.Y + Epsilon) return ContainmentType.Disjoint;
            return ContainmentType.Contains;
        }

        /// <summary>
        /// Determines the containment relationship between two axis-aligned bounding boxes (AABBs).
        /// </summary>
        /// <param name="aMin">The minimum corner of the reference AABB.</param>
        /// <param name="aMax">The maximum corner of the reference AABB.</param>
        /// <param name="bMin">The minimum corner of the AABB being tested.</param>
        /// <param name="bMax">The maximum corner of the AABB being tested.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the boxes do not overlap; <see cref="ContainmentType.Contains"/>
        /// if the second box is fully contained within the first; otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// Boundary contact is treated as overlap rather than disjoint.
        /// </remarks>
        public static ContainmentType ContainsAabbAabb(Vector2 aMin, Vector2 aMax, Vector2 bMin, Vector2 bMax)
        {
            // Disjoint (touch counts as NOT disjoint)
            if (bMax.X < aMin.X - Epsilon ||
                bMin.X > aMax.X + Epsilon ||
                bMax.Y < aMin.Y - Epsilon ||
                bMin.Y > aMax.Y + Epsilon)
                return ContainmentType.Disjoint;

            // Contains (inclusive)
            if (bMin.X >= aMin.X - Epsilon &&
                bMax.X <= aMax.X + Epsilon &&
                bMin.Y >= aMin.Y - Epsilon &&
                bMax.Y <= aMax.Y + Epsilon)
                return ContainmentType.Contains;

            // Otherwise, overlap/touch but not fully contained
            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between an axis-aligned bounding box (AABB) and a circle.
        /// </summary>
        /// <param name="boxMin">The minimum corner of the AABB.</param>
        /// <param name="boxMax">The maximum corner of the AABB.</param>
        /// <param name="circleCenter">The center of the circle.</param>
        /// <param name="circleRadius">The radius of the circle. Must be non-negative.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the circle lies entirely outside the box;
        /// <see cref="ContainmentType.Contains"/> if the circle is fully contained within the box;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// Boundary contact is treated as intersection rather than disjoint.
        /// </remarks>
        public static ContainmentType ContainsAabbCircle(Vector2 boxMin, Vector2 boxMax, Vector2 circleCenter, float circleRadius)
        {
            if (circleRadius < 0.0f)
                return ContainmentType.Disjoint;

            // Disjoint check: circle completely outside
            if (circleCenter.X - circleRadius > boxMax.X + Epsilon ||
                circleCenter.X + circleRadius < boxMin.X - Epsilon ||
                circleCenter.Y - circleRadius > boxMax.Y + Epsilon ||
                circleCenter.Y + circleRadius < boxMin.Y - Epsilon)
                return ContainmentType.Disjoint;

            // Contains (inclusive)
            if (circleCenter.X - circleRadius >= boxMin.X - Epsilon &&
                circleCenter.X + circleRadius <= boxMax.X + Epsilon &&
                circleCenter.Y - circleRadius >= boxMin.Y - Epsilon &&
                circleCenter.Y + circleRadius <= boxMax.Y + Epsilon)
                return ContainmentType.Contains;

            // Otherwise, overlap/touch, but not fully contains
            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between an axis-aligned bounding box (AABB) and an oriented bounding box (OBB).
        /// </summary>
        /// <param name="aabbMin">The minimum corner of the AABB.</param>
        /// <param name="aabbMax">The maximum corner of the AABB.</param>
        /// <param name="obbCenter">The center of the OBB.</param>
        /// <param name="obbAxisX">The OBB local X axis direction.</param>
        /// <param name="obbAxisY">The OBB local Y axis direction.</param>
        /// <param name="obbHalfExtents">The half-widths of the OBB along <paramref name="obbAxisX"/> and <paramref name="obbAxisY"/>.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the volumes do not overlap; <see cref="ContainmentType.Contains"/> if the
        /// OBB is fully contained within the AABB; otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// The overlap test is performed first. If the volumes overlap, containment is determined by testing whether all four OBB
        /// corners lie inside the AABB (inclusive).
        /// </remarks>
        public static ContainmentType ContainsAabbObb(Vector2 aabbMin, Vector2 aabbMax, Vector2 obbCenter, Vector2 obbAxisX, Vector2 obbAxisY, Vector2 obbHalfExtents)
        {
            Vector2 aabbCenter = (aabbMin + aabbMax) * 0.5f;
            Vector2 aabbHalf = (aabbMax - aabbMin) * 0.5f;

            // Disjoint check
            if (!IntersectsAabbObb(aabbCenter, aabbHalf, obbCenter, obbAxisX, obbAxisY, obbHalfExtents))
                return ContainmentType.Disjoint;

            // Contains: all 4 OBB corners inside AABB
            Vector2 ex = obbAxisX * obbHalfExtents.X;
            Vector2 ey = obbAxisY * obbHalfExtents.Y;

            Vector2 c0 = obbCenter - ex - ey;
            Vector2 c1 = obbCenter + ex - ey;
            Vector2 c2 = obbCenter + ex + ey;
            Vector2 c3 = obbCenter - ex + ey;

            if (ContainsAabbPoint(c0, aabbMin, aabbMax) == ContainmentType.Contains &&
                ContainsAabbPoint(c1, aabbMin, aabbMax) == ContainmentType.Contains &&
                ContainsAabbPoint(c2, aabbMin, aabbMax) == ContainmentType.Contains &&
                ContainsAabbPoint(c3, aabbMin, aabbMax) == ContainmentType.Contains)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between an axis-aligned bounding box (AABB) and a capsule.
        /// </summary>
        /// <param name="boxMin">The minimum corner of the AABB.</param>
        /// <param name="boxMax">The maximum corner of the AABB.</param>
        /// <param name="capsuleA">The first endpoint of the capsule line segment.</param>
        /// <param name="capsuleB">The second endpoint of the capsule line segment.</param>
        /// <param name="capsuleRadius">The capsule radius. Must be non-negative.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the capsule lies entirely outside the box;
        /// <see cref="ContainmentType.Contains"/> if the capsule is fully contained within the box;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the shapes overlap, full containment is determined by
        /// contracting the AABB by <paramref name="capsuleRadius"/> on all sides and testing whether both capsule
        /// endpoints lie inside the contracted box (inclusive). This corresponds to the condition that the capsule's
        /// swept disk about its segment is contained within the original AABB.
        /// </remarks>
        public static ContainmentType ContainsAabbCapsule(Vector2 boxMin, Vector2 boxMax, Vector2 capsuleA, Vector2 capsuleB, float capsuleRadius)
        {
            if (capsuleRadius < 0.0f)
                return ContainmentType.Disjoint;

            // If they don't even intersect, containment is impossible
            if (!IntersectsAabbCapsule(boxMin, boxMax, capsuleA, capsuleB, capsuleRadius))
                return ContainmentType.Disjoint;

            // For full containment, the capsule's segment must lie inside the AABb
            // contracted (shrunk) by the capsule radius
            Vector2 innerMin = new Vector2(boxMin.X + capsuleRadius, boxMin.Y + capsuleRadius);
            Vector2 innerMax = new Vector2(boxMax.X - capsuleRadius, boxMax.Y - capsuleRadius);

            // If the contracted AABB is invalid, it can't contain any capsule with this radius
            if (innerMin.X > innerMax.X + Epsilon || innerMin.Y > innerMax.Y + Epsilon)
                return ContainmentType.Disjoint;

            // Segment inside convex set
            if (ContainsAabbPoint(capsuleA, innerMin, innerMax) == ContainmentType.Contains &&
                ContainsAabbPoint(capsuleB, innerMin, innerMax) == ContainmentType.Contains)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between an axis-aligned bounding box (AABB) and a convex polygon.
        /// </summary>
        /// <param name="boxMin">The minimum corner of the AABB.</param>
        /// <param name="boxMax">The maximum corner of the AABB.</param>
        /// <param name="vertices">
        /// The polygon vertices in winding order. A valid polygon requires at least three vertices.
        /// </param>
        /// <param name="normals">
        /// The per-edge outward unit normals. The array length must match <paramref name="vertices"/> so that
        /// <c>normals[i]</c> corresponds to the edge from <c>vertices[i]</c> to <c>vertices[(i + 1) % vertices.Length]</c>.
        /// </param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the polygon lies entirely outside the box;
        /// <see cref="ContainmentType.Contains"/> if the polygon is fully contained within the box;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the shapes overlap, containment is determined by verifying
        /// that every polygon vertex lies inside the AABB (inclusive). For a convex polygon, all vertices contained
        /// within a convex set implies the entire polygon is contained.
        /// </remarks>
        public static ContainmentType ContainsAabbConvexPolygon(Vector2 boxMin, Vector2 boxMax, Vector2[] vertices, Vector2[] normals)
        {
            if (!IsValidPolygon(vertices, normals))
                return ContainmentType.Disjoint;

            // If they don't intersect at all, containment is impossible
            if (!IntersectsAabbConvexPolygon(boxMin, boxMax, vertices, normals))
                return ContainmentType.Disjoint;

            // Fast containment check: if all polygon vertices are inside the AABB,
            // then the convex polygon is fully contained
            for (int i = 0; i < vertices.Length; i++)
            {
                if (ContainsAabbPoint(vertices[i], boxMin, boxMax) == ContainmentType.Disjoint)
                    return ContainmentType.Intersects;
            }

            return ContainmentType.Contains;
        }

        #endregion

        #region Containment (Circle Contains)

        /// <summary>
        /// Determines whether a point is contained within a circle.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The circle radius. Must be non-negative.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the point lies inside the circle or on its boundary;
        /// otherwise, <see cref="ContainmentType.Disjoint"/>.
        /// </returns>
        public static ContainmentType ContainsCirclePoint(Vector2 point, Vector2 center, float radius)
        {
            if (radius < 0.0f)
                return ContainmentType.Disjoint;

            // Expand radius by epsilon to improve tests when point lies near boundary.
            float r = radius + Epsilon;
            return Vector2.DistanceSquared(point, center) <= r * r ? ContainmentType.Contains : ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines the containment relationship between a circle and an axis-aligned bounding box (AABB).
        /// </summary>
        /// <param name="circleCenter">The center of the circle.</param>
        /// <param name="circleRadius">The circle radius. Must be non-negative.</param>
        /// <param name="aabbMin">The minimum corner of the AABB.</param>
        /// <param name="aabbMax">The maximum corner of the AABB.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the box lies entirely outside the circle;
        /// <see cref="ContainmentType.Contains"/> if the box is fully contained within the circle;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the shapes overlap, containment is determined by testing
        /// whether all four AABB corners lie inside the circle (inclusive).
        /// </remarks>
        public static ContainmentType ContainsCircleAabb(Vector2 circleCenter, float circleRadius, Vector2 aabbMin, Vector2 aabbMax)
        {
            if (circleRadius < 0f)
                return ContainmentType.Disjoint;

            if (!IntersectsCircleAabb(circleCenter, circleRadius, aabbMin, aabbMax))
                return ContainmentType.Disjoint;

            // Contains if all 4 AABB corners are inside circle
            Vector2 c0 = new Vector2(aabbMin.X, aabbMin.Y);
            Vector2 c1 = new Vector2(aabbMax.X, aabbMin.Y);
            Vector2 c2 = new Vector2(aabbMax.X, aabbMax.Y);
            Vector2 c3 = new Vector2(aabbMin.X, aabbMax.Y);

            if (ContainsCirclePoint(c0, circleCenter, circleRadius) == ContainmentType.Contains &&
                ContainsCirclePoint(c1, circleCenter, circleRadius) == ContainmentType.Contains &&
                ContainsCirclePoint(c2, circleCenter, circleRadius) == ContainmentType.Contains &&
                ContainsCirclePoint(c3, circleCenter, circleRadius) == ContainmentType.Contains)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between two circles.
        /// </summary>
        /// <param name="aCenter">The center of the reference circle.</param>
        /// <param name="aRadius">The radius of the reference circle. Must be non-negative.</param>
        /// <param name="bCenter">The center of the circle being tested.</param>
        /// <param name="bRadius">The radius of the circle being tested. Must be non-negative.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the circles do not overlap;
        /// <see cref="ContainmentType.Contains"/> if the second circle is fully contained within the first;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// The test is performed using squared distances to avoid a square root. Boundary contact is treated as intersection,
        /// and containment tests are inclusive.
        /// </remarks>
        public static ContainmentType ContainsCircleCircle(Vector2 aCenter, float aRadius, Vector2 bCenter, float bRadius)
        {
            if (aRadius < 0.0f || bRadius < 0.0f)
                return ContainmentType.Disjoint;

            Vector2 d = bCenter - aCenter;
            float d2 = d.LengthSquared();

            // Disjoint if centers are father apart than sum of radii
            float sum = aRadius + bRadius + Epsilon;
            if (d2 > sum * sum)
                return ContainmentType.Disjoint;

            // Contains if B fits entirely with A (inclusive)
            float rhs = (aRadius - bRadius) + Epsilon;
            if (rhs >= 0.0f && d2 <= rhs * rhs)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between a circle and an oriented bounding box (OBB).
        /// </summary>
        /// <param name="circleCenter">The center of the circle.</param>
        /// <param name="circleRadius">The circle radius. Must be non-negative.</param>
        /// <param name="obbCenter">The center of the OBB.</param>
        /// <param name="obbAxisX">The OBB local X axis direction.</param>
        /// <param name="obbAxisY">The OBB local Y axis direction.</param>
        /// <param name="obbHalfExtents">The half-widths of the OBB along <paramref name="obbAxisX"/> and <paramref name="obbAxisY"/>.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the OBB lies entirely outside the circle;
        /// <see cref="ContainmentType.Contains"/> if the OBB is fully contained within the circle;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the shapes overlap, containment is determined by testing
        /// whether all four OBB corners lie inside the circle (inclusive).
        /// </remarks>
        public static ContainmentType ContainsCircleObb(Vector2 circleCenter, float circleRadius, Vector2 obbCenter, Vector2 obbAxisX, Vector2 obbAxisY, Vector2 obbHalfExtents)
        {
            if (circleRadius < 0f)
                return ContainmentType.Disjoint;

            // Disjoint check first (touch counts as NOT disjoint)
            if (!IntersectsCircleObb(circleCenter, circleRadius, obbCenter, obbAxisX, obbAxisY, obbHalfExtents))
                return ContainmentType.Disjoint;

            // Contains if all 4 OBB corners are inside circle
            Vector2 ex = obbAxisX * obbHalfExtents.X;
            Vector2 ey = obbAxisY * obbHalfExtents.Y;

            Vector2 c0 = obbCenter - ex - ey;
            Vector2 c1 = obbCenter + ex - ey;
            Vector2 c2 = obbCenter + ex + ey;
            Vector2 c3 = obbCenter - ex + ey;

            if (ContainsCirclePoint(c0, circleCenter, circleRadius) == ContainmentType.Contains &&
                ContainsCirclePoint(c1, circleCenter, circleRadius) == ContainmentType.Contains &&
                ContainsCirclePoint(c2, circleCenter, circleRadius) == ContainmentType.Contains &&
                ContainsCirclePoint(c3, circleCenter, circleRadius) == ContainmentType.Contains)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between a circle and a capsule.
        /// </summary>
        /// <param name="circleCenter">The center of the circle.</param>
        /// <param name="circleRadius">The circle radius. Must be non-negative.</param>
        /// <param name="capsuleA">The first endpoint of the capsule line segment.</param>
        /// <param name="capsuleB">The second endpoint of the capsule line segment.</param>
        /// <param name="capsuleRadius">The capsule radius. Must be non-negative.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the capsule lies entirely outside the circle;
        /// <see cref="ContainmentType.Contains"/> if the capsule is fully contained within the circle;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// The capsule is the Minkowski sum of its segment and a disk of radius <paramref name="capsuleRadius"/>.
        /// After confirming the shapes intersect, containment is determined by checking whether the capsule segment lies
        /// within a circle of radius <c>circleRadius - capsuleRadius</c> (inclusive). This is equivalent to requiring
        /// the maximum distance from <paramref name="circleCenter"/> to the segment to be less than or equal to
        /// <c>circleRadius - capsuleRadius</c>.
        /// </remarks>
        public static ContainmentType ContainsCircleCapsule(Vector2 circleCenter, float circleRadius, Vector2 capsuleA, Vector2 capsuleB, float capsuleRadius)
        {
            if (circleRadius < 0f || capsuleRadius < 0f)
                return ContainmentType.Disjoint;

            if (!IntersectsCircleCapsule(circleCenter, circleRadius, capsuleA, capsuleB, capsuleRadius))
                return ContainmentType.Disjoint;

            float inner = (circleRadius - capsuleRadius) + Epsilon;
            if (inner < 0f)
                return ContainmentType.Intersects;

            float inner2 = inner * inner;

            // For the capsule to be fully contained, both endpoints must be within the inner circle
            if (Vector2.DistanceSquared(circleCenter, capsuleA) <= inner2 &&
                Vector2.DistanceSquared(circleCenter, capsuleB) <= inner2)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between a circle and a convex polygon.
        /// </summary>
        /// <param name="circleCenter">The center of the circle.</param>
        /// <param name="circleRadius">The circle radius. Must be non-negative.</param>
        /// <param name="vertices">
        /// The polygon vertices in winding order. A valid polygon requires at least three vertices.
        /// </param>
        /// <param name="normals">
        /// The per-edge outward unit normals. The array length must match <paramref name="vertices"/> so that
        /// <c>normals[i]</c> corresponds to the edge from <c>vertices[i]</c> to <c>vertices[(i + 1) % vertices.Length]</c>.
        /// </param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the polygon lies entirely outside the circle;
        /// <see cref="ContainmentType.Contains"/> if the polygon is fully contained within the circle;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the shapes overlap, containment is determined by verifying
        /// that every polygon vertex lies inside the circle (inclusive). For a convex polygon, all vertices contained
        /// within a convex set implies the entire polygon is contained.
        /// </remarks>
        public static ContainmentType ContainsCircleConvexPolygon(Vector2 circleCenter, float circleRadius, Vector2[] vertices, Vector2[] normals)
        {
            if (circleRadius < 0f)
                return ContainmentType.Disjoint;

            if (!IsValidPolygon(vertices, normals))
                return ContainmentType.Disjoint;

            if (!IntersectsCircleConvexPolygon(circleCenter, circleRadius, vertices, normals))
                return ContainmentType.Disjoint;

            // Contains: all polygon vertices inside circle
            for (int i = 0; i < vertices.Length; i++)
            {
                if (ContainsCirclePoint(vertices[i], circleCenter, circleRadius) == ContainmentType.Disjoint)
                    return ContainmentType.Intersects;
            }

            return ContainmentType.Contains;
        }

        #endregion

        #region Containment (OBB Contains)

        /// <summary>
        /// Determines whether a point is contained within an oriented bounding box (OBB).
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="center">The center of the OBB.</param>
        /// <param name="axisX">The OBB local X axis direction.</param>
        /// <param name="axisY">The OBB local Y axis direction.</param>
        /// <param name="halfExtents">The half-widths of the OBB along <paramref name="axisX"/> and <paramref name="axisY"/>.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the point lies inside the box or on its boundary;
        /// otherwise, <see cref="ContainmentType.Disjoint"/>.
        /// </returns>
        /// <remarks>
        /// The test projects <c>(point - center)</c> onto the OBB axes and compares the absolute projected distances against
        /// the corresponding half extents. The axes are expected to be orthonormal for the extents to represent distances.
        /// </remarks>
        public static ContainmentType ContainsObbPoint(Vector2 point, Vector2 center, Vector2 axisX, Vector2 axisY, Vector2 halfExtents)
        {
            //  Transform point into OBB local space by projecting it onto axes
            Vector2 d = point - center;

            float distX = Vector2.Dot(d, axisX);
            if (MathF.Abs(distX) > halfExtents.X + Epsilon)
                return ContainmentType.Disjoint;

            float distY = Vector2.Dot(d, axisY);
            if (MathF.Abs(distY) > halfExtents.Y + Epsilon)
                return ContainmentType.Disjoint;

            return ContainmentType.Contains;
        }

        /// <summary>
        /// Determines the containment relationship between an oriented bounding box (OBB) and an axis-aligned bounding box (AABB).
        /// </summary>
        /// <param name="obbCenter">The center of the OBB.</param>
        /// <param name="obbAxisX">The OBB local X axis direction.</param>
        /// <param name="obbAxisY">The OBB local Y axis direction.</param>
        /// <param name="obbHalfExtents">The half-widths of the OBB along <paramref name="obbAxisX"/> and <paramref name="obbAxisY"/>.</param>
        /// <param name="aabbMin">The minimum corner of the AABB.</param>
        /// <param name="aabbMax">The maximum corner of the AABB.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the volumes do not overlap; <see cref="ContainmentType.Contains"/> if the
        /// AABB is fully contained within the OBB; otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// The overlap test is performed first. If the volumes overlap, containment is determined by testing whether all four
        /// AABB corners lie inside the OBB (inclusive).
        /// </remarks>
        public static ContainmentType ContainsObbAabb(Vector2 obbCenter, Vector2 obbAxisX, Vector2 obbAxisY, Vector2 obbHalfExtents, Vector2 aabbMin, Vector2 aabbMax)
        {
            Vector2 aabbCenter = (aabbMin + aabbMax) * 0.5f;
            Vector2 aabbHalf = (aabbMax - aabbMin) * 0.5f;

            // Disjoint check
            if (!IntersectsAabbObb(aabbCenter, aabbHalf, obbCenter, obbAxisX, obbAxisY, obbHalfExtents))
                return ContainmentType.Disjoint;

            // Contains: all 4 AABB corners inside OBB
            Vector2 c0 = new Vector2(aabbMin.X, aabbMin.Y);
            Vector2 c1 = new Vector2(aabbMax.X, aabbMin.Y);
            Vector2 c2 = new Vector2(aabbMax.X, aabbMax.Y);
            Vector2 c3 = new Vector2(aabbMin.X, aabbMax.Y);

            if (ContainsObbPoint(c0, obbCenter, obbAxisX, obbAxisY, obbHalfExtents) == ContainmentType.Contains &&
                ContainsObbPoint(c1, obbCenter, obbAxisX, obbAxisY, obbHalfExtents) == ContainmentType.Contains &&
                ContainsObbPoint(c2, obbCenter, obbAxisX, obbAxisY, obbHalfExtents) == ContainmentType.Contains &&
                ContainsObbPoint(c3, obbCenter, obbAxisX, obbAxisY, obbHalfExtents) == ContainmentType.Contains)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between an oriented bounding box (OBB) and a circle.
        /// </summary>
        /// <param name="boxCenter">The center of the OBB.</param>
        /// <param name="axisX">The OBB local X axis direction.</param>
        /// <param name="axisY">The OBB local Y axis direction.</param>
        /// <param name="halfExtents">The half-widths of the OBB along <paramref name="axisX"/> and <paramref name="axisY"/>.</param>
        /// <param name="circleCenter">The center of the circle.</param>
        /// <param name="circleRadius">The circle radius. Must be non-negative.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the circle lies entirely outside the box;
        /// <see cref="ContainmentType.Contains"/> if the circle is fully contained within the box;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// The circle center is projected into OBB local space. The circle is contained when its local-space extent along each
        /// axis fits within the corresponding half extent, i.e. <c>|dot(d, axisX)| + r &lt;= halfExtents.X</c> and
        /// <c>|dot(d, axisY)| + r &lt;= halfExtents.Y</c>.
        /// </remarks>
        public static ContainmentType ContainsObbCircle(Vector2 boxCenter, Vector2 axisX, Vector2 axisY, Vector2 halfExtents, Vector2 circleCenter, float circleRadius)
        {
            if (circleRadius < 0.0f)
                return ContainmentType.Disjoint;

            // Disjoint check
            if (!IntersectsCircleObb(circleCenter, circleRadius, boxCenter, axisX, axisY, halfExtents))
                return ContainmentType.Disjoint;

            // Containment: circle must fit within OBB extents in local space
            Vector2 d = circleCenter - boxCenter;
            float localX = Vector2.Dot(d, axisX);
            float localY = Vector2.Dot(d, axisY);

            float r = circleRadius + Epsilon;

            if (MathF.Abs(localX) + r <= halfExtents.X &&
                MathF.Abs(localY) + r <= halfExtents.Y)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between two oriented bounding boxes (OBBs).
        /// </summary>
        /// <param name="aCenter">The center of the reference OBB.</param>
        /// <param name="aAxisX">The reference OBB local X axis direction.</param>
        /// <param name="aAxisY">The reference OBB local Y axis direction.</param>
        /// <param name="aHalf">The half-widths of the reference OBB along <paramref name="aAxisX"/> and <paramref name="aAxisY"/>.</param>
        /// <param name="bCenter">The center of the OBB being tested.</param>
        /// <param name="bAxisX">The tested OBB local X axis direction.</param>
        /// <param name="bAxisY">The tested OBB local Y axis direction.</param>
        /// <param name="bHalf">The half-widths of the tested OBB along <paramref name="bAxisX"/> and <paramref name="bAxisY"/>.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the boxes do not overlap; <see cref="ContainmentType.Contains"/> if the
        /// second box is fully contained within the first; otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an overlap test. If the boxes overlap, containment is determined by testing whether all
        /// four corners of the second OBB lie inside the first OBB.
        /// </remarks>
        public static ContainmentType ContainsObbObb(Vector2 aCenter, Vector2 aAxisX, Vector2 aAxisY, Vector2 aHalf, Vector2 bCenter, Vector2 bAxisX, Vector2 bAxisY, Vector2 bHalf)
        {
            if (!IntersectsObbObb(aCenter, aAxisX, aAxisY, aHalf, bCenter, bAxisX, bAxisY, bHalf))
                return ContainmentType.Disjoint;

            // Contains all 4 B corners inside A
            Vector2 bx = bAxisX * bHalf.X;
            Vector2 by = bAxisY * bHalf.Y;

            Vector2 c0 = bCenter - bx - by;
            Vector2 c1 = bCenter + bx - by;
            Vector2 c2 = bCenter + bx + by;
            Vector2 c3 = bCenter - bx + by;

            if (ContainsObbPoint(c0, aCenter, aAxisX, aAxisY, aHalf) == ContainmentType.Contains &&
                ContainsObbPoint(c1, aCenter, aAxisX, aAxisY, aHalf) == ContainmentType.Contains &&
                ContainsObbPoint(c2, aCenter, aAxisX, aAxisY, aHalf) == ContainmentType.Contains &&
                ContainsObbPoint(c3, aCenter, aAxisX, aAxisY, aHalf) == ContainmentType.Contains)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between an oriented bounding box (OBB) and a capsule.
        /// </summary>
        /// <param name="boxCenter">The center of the OBB.</param>
        /// <param name="axisX">The OBB local X axis direction.</param>
        /// <param name="axisY">The OBB local Y axis direction.</param>
        /// <param name="halfExtents">The half-widths of the OBB along <paramref name="axisX"/> and <paramref name="axisY"/>.</param>
        /// <param name="capsuleA">The first endpoint of the capsule line segment.</param>
        /// <param name="capsuleB">The second endpoint of the capsule line segment.</param>
        /// <param name="capsuleRadius">The capsule radius. Must be non-negative.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the capsule lies entirely outside the box;
        /// <see cref="ContainmentType.Contains"/> if the capsule is fully contained within the box;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the shapes overlap, containment is determined by contracting
        /// the OBB half extents by <paramref name="capsuleRadius"/> and testing whether both capsule endpoints lie inside the
        /// contracted OBB.
        /// </remarks>
        public static ContainmentType ContainsObbCapsule(Vector2 boxCenter, Vector2 axisX, Vector2 axisY, Vector2 halfExtents, Vector2 capsuleA, Vector2 capsuleB, float capsuleRadius)
        {
            if (capsuleRadius < 0.0f)
                return ContainmentType.Disjoint;

            if (!IntersectsObbCapsule(boxCenter, axisX, axisY, halfExtents, capsuleA, capsuleB, capsuleRadius))
                return ContainmentType.Disjoint;

            Vector2 innerHalf = new Vector2(halfExtents.X - capsuleRadius, halfExtents.Y - capsuleRadius);

            // If the contracted OBB is invalid, it can't contain a capsule of this radius
            if (innerHalf.X < -Epsilon || innerHalf.Y < -Epsilon)
                return ContainmentType.Intersects;

            if (ContainsObbPoint(capsuleA, boxCenter, axisX, axisY, innerHalf) == ContainmentType.Contains &&
                ContainsObbPoint(capsuleB, boxCenter, axisX, axisY, innerHalf) == ContainmentType.Contains)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between an oriented bounding box (OBB) and a convex polygon.
        /// </summary>
        /// <param name="boxCenter">The center of the OBB.</param>
        /// <param name="axisX">The OBB local X axis direction.</param>
        /// <param name="axisY">The OBB local Y axis direction.</param>
        /// <param name="halfExtents">The half-widths of the OBB along <paramref name="axisX"/> and <paramref name="axisY"/>.</param>
        /// <param name="vertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="normals">
        /// The per-edge outward unit normals. The array length must match <paramref name="vertices"/> so that
        /// <c>normals[i]</c> corresponds to the edge from <c>vertices[i]</c> to <c>vertices[(i + 1) % vertices.Length]</c>.
        /// </param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the polygon lies entirely outside the box;
        /// <see cref="ContainmentType.Contains"/> if the polygon is fully contained within the box;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the shapes overlap, containment is determined by verifying
        /// that every polygon vertex lies inside the OBB.
        /// </remarks>
        public static ContainmentType ContainsObbConvexPolygon(Vector2 boxCenter, Vector2 axisX, Vector2 axisY, Vector2 halfExtents, Vector2[] vertices, Vector2[] normals)
        {
            if (!IsValidPolygon(vertices, normals))
                return ContainmentType.Disjoint;

            if (!IntersectsObbConvexPolygon(boxCenter, axisX, axisY, halfExtents, vertices, normals))
                return ContainmentType.Disjoint;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (ContainsObbPoint(vertices[i], boxCenter, axisX, axisY, halfExtents) == ContainmentType.Disjoint)
                    return ContainmentType.Intersects;
            }

            return ContainmentType.Contains;
        }

        #endregion

        #region Containment (Capsule Contains)

        /// <summary>
        /// Determines whether a point is contained within a capsule.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="a">The first endpoint of the capsule line segment.</param>
        /// <param name="b">The second endpoint of the capsule line segment.</param>
        /// <param name="radius">The capsule radius. Must be non-negative.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the point lies within the capsule or on its boundary; otherwise,
        /// <see cref="ContainmentType.Disjoint"/>.
        /// </returns>
        /// <remarks>
        /// A capsule is the set of points whose distance to the segment <c>[a, b]</c> is less than or equal to
        /// <paramref name="radius"/>. This method compares the squared distance from the point to the segment against
        /// <c>radius^2</c> to avoid a square root.
        /// </remarks>
        public static ContainmentType ContainsCapsulePoint(Vector2 point, Vector2 a, Vector2 b, float radius)
        {
            if (radius < 0.0f)
                return ContainmentType.Disjoint;

            float r = radius + Epsilon;
            float rr = r * r;

            float d2 = DistanceSquaredPointSegment(point, a, b, out _, out _);
            return d2 <= rr ? ContainmentType.Contains : ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines the containment relationship between a capsule and an axis-aligned bounding box (AABB).
        /// </summary>
        /// <param name="capsuleA">The first endpoint of the capsule line segment.</param>
        /// <param name="capsuleB">The second endpoint of the capsule line segment.</param>
        /// <param name="capsuleRadius">The capsule radius. Must be non-negative.</param>
        /// <param name="aabbMin">The minimum corner of the AABB.</param>
        /// <param name="aabbMax">The maximum corner of the AABB.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the box lies entirely outside the capsule;
        /// <see cref="ContainmentType.Contains"/> if the box is fully contained within the capsule;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the shapes overlap, containment is determined by testing
        /// whether all four AABB corners lie inside the capsule.
        /// </remarks>
        public static ContainmentType ContainsCapsuleAabb(Vector2 capsuleA, Vector2 capsuleB, float capsuleRadius, Vector2 aabbMin, Vector2 aabbMax)
        {
            if (capsuleRadius < 0f)
                return ContainmentType.Disjoint;

            if (!IntersectsAabbCapsule(aabbMin, aabbMax, capsuleA, capsuleB, capsuleRadius))
                return ContainmentType.Disjoint;

            Vector2 c0 = new Vector2(aabbMin.X, aabbMin.Y);
            Vector2 c1 = new Vector2(aabbMax.X, aabbMin.Y);
            Vector2 c2 = new Vector2(aabbMax.X, aabbMax.Y);
            Vector2 c3 = new Vector2(aabbMin.X, aabbMax.Y);

            if (ContainsCapsulePoint(c0, capsuleA, capsuleB, capsuleRadius) == ContainmentType.Contains &&
                ContainsCapsulePoint(c1, capsuleA, capsuleB, capsuleRadius) == ContainmentType.Contains &&
                ContainsCapsulePoint(c2, capsuleA, capsuleB, capsuleRadius) == ContainmentType.Contains &&
                ContainsCapsulePoint(c3, capsuleA, capsuleB, capsuleRadius) == ContainmentType.Contains)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between a capsule and a circle.
        /// </summary>
        /// <param name="capsuleA">The first endpoint of the capsule line segment.</param>
        /// <param name="capsuleB">The second endpoint of the capsule line segment.</param>
        /// <param name="capsuleRadius">The capsule radius. Must be non-negative.</param>
        /// <param name="circleCenter">The center of the circle.</param>
        /// <param name="circleRadius">The circle radius. Must be non-negative.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the circle lies entirely outside the capsule;
        /// <see cref="ContainmentType.Contains"/> if the circle is fully contained within the capsule;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the shapes overlap, the circle is contained when its center is
        /// within a distance of <c>capsuleRadius - circleRadius</c> from the capsule segment <c>[capsuleA, capsuleB]</c>.
        /// The test is performed using squared distance to avoid a square root.
        /// </remarks>
        public static ContainmentType ContainsCapsuleCircle(Vector2 capsuleA, Vector2 capsuleB, float capsuleRadius, Vector2 circleCenter, float circleRadius)
        {
            if (capsuleRadius < 0f || circleRadius < 0f)
                return ContainmentType.Disjoint;

            // Disjoint check first (touch counts as NOT disjoint)
            if (!IntersectsCircleCapsule(circleCenter, circleRadius, capsuleA, capsuleB, capsuleRadius))
                return ContainmentType.Disjoint;

            // Contains if the circle fits fully inside the capsule
            float inner = (capsuleRadius - circleRadius) + Epsilon;
            if (inner < 0f)
                return ContainmentType.Intersects;

            float inner2 = inner * inner;
            float d2 = DistanceSquaredPointSegment(circleCenter, capsuleA, capsuleB, out _, out _);

            return d2 <= inner2 ? ContainmentType.Contains : ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between a capsule and an oriented bounding box (OBB).
        /// </summary>
        /// <param name="capsuleA">The first endpoint of the capsule line segment.</param>
        /// <param name="capsuleB">The second endpoint of the capsule line segment.</param>
        /// <param name="capsuleRadius">The capsule radius. Must be non-negative.</param>
        /// <param name="obbCenter">The center of the OBB.</param>
        /// <param name="obbAxisX">The OBB local X axis direction.</param>
        /// <param name="obbAxisY">The OBB local Y axis direction.</param>
        /// <param name="obbHalf">The half-widths of the OBB along <paramref name="obbAxisX"/> and <paramref name="obbAxisY"/>.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the OBB lies entirely outside the capsule;
        /// <see cref="ContainmentType.Contains"/> if the OBB is fully contained within the capsule;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the shapes overlap, containment is determined by testing
        /// whether all four OBB corners lie inside the capsule.
        /// </remarks>
        public static ContainmentType ContainsCapsuleObb(Vector2 capsuleA, Vector2 capsuleB, float capsuleRadius, Vector2 obbCenter, Vector2 obbAxisX, Vector2 obbAxisY, Vector2 obbHalf)
        {
            if (capsuleRadius < 0f)
                return ContainmentType.Disjoint;

            if (!IntersectsObbCapsule(obbCenter, obbAxisX, obbAxisY, obbHalf, capsuleA, capsuleB, capsuleRadius))
                return ContainmentType.Disjoint;

            Vector2 ex = obbAxisX * obbHalf.X;
            Vector2 ey = obbAxisY * obbHalf.Y;

            Vector2 c0 = obbCenter - ex - ey;
            Vector2 c1 = obbCenter + ex - ey;
            Vector2 c2 = obbCenter + ex + ey;
            Vector2 c3 = obbCenter - ex + ey;

            if (ContainsCapsulePoint(c0, capsuleA, capsuleB, capsuleRadius) == ContainmentType.Contains &&
                ContainsCapsulePoint(c1, capsuleA, capsuleB, capsuleRadius) == ContainmentType.Contains &&
                ContainsCapsulePoint(c2, capsuleA, capsuleB, capsuleRadius) == ContainmentType.Contains &&
                ContainsCapsulePoint(c3, capsuleA, capsuleB, capsuleRadius) == ContainmentType.Contains)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between two capsules.
        /// </summary>
        /// <param name="a0">The first endpoint of the reference capsule segment.</param>
        /// <param name="a1">The second endpoint of the reference capsule segment.</param>
        /// <param name="aRadius">The reference capsule radius. Must be non-negative.</param>
        /// <param name="b0">The first endpoint of the capsule segment being tested.</param>
        /// <param name="b1">The second endpoint of the capsule segment being tested.</param>
        /// <param name="bRadius">The tested capsule radius. Must be non-negative.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the capsules do not overlap;
        /// <see cref="ContainmentType.Contains"/> if the second capsule is fully contained within the first;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the capsules overlap, containment requires the segment
        /// endpoints of the second capsule to lie within a distance of <c>aRadius - bRadius</c> from the reference capsule
        /// segment <c>[a0, a1]</c>. The test is performed using squared distances to avoid a square root.
        /// </remarks>
        public static ContainmentType ContainsCapsuleCapsule(Vector2 a0, Vector2 a1, float aRadius, Vector2 b0, Vector2 b1, float bRadius)
        {
            if (aRadius < 0f || bRadius < 0f)
                return ContainmentType.Disjoint;

            if (!IntersectsCapsuleCapsule(a0, a1, aRadius, b0, b1, bRadius))
                return ContainmentType.Disjoint;

            // If B is larger, A can't contain it, but they do intersect
            float inner = (aRadius - bRadius) + Epsilon;
            if (inner < 0f)
                return ContainmentType.Intersects;

            float inner2 = inner * inner;

            float d20 = DistanceSquaredPointSegment(b0, a0, a1, out _, out _);
            if (d20 > inner2)
                return ContainmentType.Intersects;

            float d21 = DistanceSquaredPointSegment(b1, a0, a1, out _, out _);
            if (d21 > inner2)
                return ContainmentType.Intersects;

            return ContainmentType.Contains;
        }

        /// <summary>
        /// Determines the containment relationship between a capsule and a convex polygon.
        /// </summary>
        /// <param name="capsuleA">The first endpoint of the capsule line segment.</param>
        /// <param name="capsuleB">The second endpoint of the capsule line segment.</param>
        /// <param name="capsuleRadius">The capsule radius. Must be non-negative.</param>
        /// <param name="pVertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="pNormals">
        /// The per-edge outward unit normals. The array length must match <paramref name="pVertices"/> so that
        /// <c>pNormals[i]</c> corresponds to the edge from <c>pVertices[i]</c> to <c>pVertices[(i + 1) % pVertices.Length]</c>.
        /// </param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the polygon lies entirely outside the capsule;
        /// <see cref="ContainmentType.Contains"/> if the polygon is fully contained within the capsule;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the shapes overlap, containment is determined by verifying
        /// that every polygon vertex lies inside the capsule.
        /// </remarks>
        public static ContainmentType ContainsCapsuleConvexPolygon(Vector2 capsuleA, Vector2 capsuleB, float capsuleRadius, Vector2[] pVertices, Vector2[] pNormals)
        {
            if (capsuleRadius < 0f)
                return ContainmentType.Disjoint;

            if (!IsValidPolygon(pVertices, pNormals))
                return ContainmentType.Disjoint;

            if (!IntersectsCapsuleConvexPolygon(capsuleA, capsuleB, capsuleRadius, pVertices, pNormals))
                return ContainmentType.Disjoint;

            for (int i = 0; i < pVertices.Length; i++)
            {
                if (ContainsCapsulePoint(pVertices[i], capsuleA, capsuleB, capsuleRadius) == ContainmentType.Disjoint)
                    return ContainmentType.Intersects;
            }

            return ContainmentType.Contains;
        }

        #endregion

        #region Containment (Convex Polygon Contains)

        /// <summary>
        /// Determines whether a point is contained within a convex polygon.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="vertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="normals">
        /// The per-edge outward unit normals. The array length must match <paramref name="vertices"/> so that
        /// <c>normals[i]</c> corresponds to the edge from <c>vertices[i]</c> to <c>vertices[(i + 1) % vertices.Length]</c>.
        /// </param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the point lies inside the polygon or on its boundary; otherwise,
        /// <see cref="ContainmentType.Disjoint"/>.
        /// </returns>
        /// <remarks>
        /// The polygon is treated as the intersection of half-spaces defined by its edges. For each edge normal <c>n</c>,
        /// the point is inside the polygon when <c>dot(n, point) &lt;= dot(n, vertices[i])</c>.
        /// </remarks>
        public static ContainmentType ContainsConvexPolygonPoint(Vector2 point, Vector2[] vertices, Vector2[] normals)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.4.1 "Testing Point in Polygon" (half-space tests; adapted to convex polygon with outward normals)

            if (!IsValidPolygon(vertices, normals))
                return ContainmentType.Disjoint;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 n = normals[i];
                float planeD = Vector2.Dot(n, vertices[i]);

                // Inside halfspace if Dot(n, p) <= planD
                if (Vector2.Dot(n, point) > planeD + Epsilon)
                    return ContainmentType.Disjoint;
            }

            return ContainmentType.Contains;
        }

        /// <summary>
        /// Determines the containment relationship between a convex polygon and an axis-aligned bounding box (AABB).
        /// </summary>
        /// <param name="pVertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="pNormals">
        /// The per-edge outward unit normals. The array length must match <paramref name="pVertices"/> so that
        /// <c>pNormals[i]</c> corresponds to the edge from <c>pVertices[i]</c> to <c>pVertices[(i + 1) % pVertices.Length]</c>.
        /// </param>
        /// <param name="aabbMin">The minimum corner of the AABB.</param>
        /// <param name="aabbMax">The maximum corner of the AABB.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the box lies entirely outside the polygon;
        /// <see cref="ContainmentType.Contains"/> if the box is fully contained within the polygon;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the shapes overlap, containment is determined by testing
        /// whether all four AABB corners lie inside the polygon.
        /// </remarks>
        public static ContainmentType ContainsConvexPolygonAabb(Vector2[] pVertices, Vector2[] pNormals, Vector2 aabbMin, Vector2 aabbMax)
        {
            if (!IsValidPolygon(pVertices, pNormals))
                return ContainmentType.Disjoint;

            Vector2 aabbCenter = (aabbMin + aabbMax) * 0.5f;
            Vector2 aabbHalf = (aabbMax - aabbMin) * 0.5f;

            if (!IntersectsAabbConvexPolygon(aabbCenter, aabbHalf, pVertices, pNormals))
                return ContainmentType.Disjoint;

            // Contains: all AABB corners inside polygon
            Vector2 c0 = new Vector2(aabbMin.X, aabbMin.Y);
            Vector2 c1 = new Vector2(aabbMax.X, aabbMin.Y);
            Vector2 c2 = new Vector2(aabbMax.X, aabbMax.Y);
            Vector2 c3 = new Vector2(aabbMin.X, aabbMax.Y);

            if (ContainsConvexPolygonPoint(c0, pVertices, pNormals) == ContainmentType.Contains &&
                ContainsConvexPolygonPoint(c1, pVertices, pNormals) == ContainmentType.Contains &&
                ContainsConvexPolygonPoint(c2, pVertices, pNormals) == ContainmentType.Contains &&
                ContainsConvexPolygonPoint(c3, pVertices, pNormals) == ContainmentType.Contains)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between a convex polygon and a circle.
        /// </summary>
        /// <param name="pVertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="pNormals">
        /// The per-edge outward unit normals. The array length must match <paramref name="pVertices"/> so that
        /// <c>pNormals[i]</c> corresponds to the edge from <c>pVertices[i]</c> to <c>pVertices[(i + 1) % pVertices.Length]</c>.
        /// </param>
        /// <param name="circleCenter">The center of the circle.</param>
        /// <param name="circleRadius">The circle radius. Must be non-negative.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the circle lies entirely outside the polygon;
        /// <see cref="ContainmentType.Contains"/> if the circle is fully contained within the polygon;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// After confirming the shapes overlap, containment is tested against each polygon half-space. The circle is contained
        /// when, for every edge normal <c>n</c>, <c>dot(n, circleCenter) + circleRadius &lt;= dot(n, pVertices[i])</c>.
        /// This requires <paramref name="pNormals"/> to be outward-facing unit normals.
        /// </remarks>
        public static ContainmentType ContainsConvexPolygonCircle(Vector2[] pVertices, Vector2[] pNormals, Vector2 circleCenter, float circleRadius)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.2 "Testing Sphere Against Plane" (2D reduction: circle vs line)
            // Related: Section 5.2.8 "Testing Sphere Against Polygon" (apply plane test to all polygon edge half-spaces)

            if (circleRadius < 0.0f)
                return ContainmentType.Disjoint;

            if (!IsValidPolygon(pVertices, pNormals))
                return ContainmentType.Disjoint;

            // If they don't intersect at all, containment is impossible
            if (!IntersectsCircleConvexPolygon(circleCenter, circleRadius, pVertices, pNormals))
                return ContainmentType.Disjoint;

            // Contains if the circle is fully inside every polygon halfspace:
            // Dot(n, center) + radius <= planeD
            //
            // NOTE: this assumes normals are outward facing unit normals
            float r = circleRadius + Epsilon;

            for (int i = 0; i < pVertices.Length; i++)
            {
                Vector2 n = pNormals[i];
                float planeD = Vector2.Dot(n, pVertices[i]);

                if (Vector2.Dot(n, circleCenter) + r > planeD)
                    return ContainmentType.Intersects;
            }

            return ContainmentType.Contains;
        }

        /// <summary>
        /// Determines the containment relationship between a convex polygon and a capsule.
        /// </summary>
        /// <param name="pVertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="pNormals">
        /// The per-edge outward unit normals. The array length must match <paramref name="pVertices"/> so that
        /// <c>pNormals[i]</c> corresponds to the edge from <c>pVertices[i]</c> to <c>pVertices[(i + 1) % pVertices.Length]</c>.
        /// </param>
        /// <param name="capsuleA">The first endpoint of the capsule line segment.</param>
        /// <param name="capsuleB">The second endpoint of the capsule line segment.</param>
        /// <param name="capsuleRadius">The capsule radius. Must be non-negative.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the capsule lies entirely outside the polygon;
        /// <see cref="ContainmentType.Contains"/> if the capsule is fully contained within the polygon;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// After confirming the shapes overlap, containment is tested against each polygon half-space. The capsule is
        /// contained when, for every edge normal <c>n</c>, <c>max(dot(n, capsuleA), dot(n, capsuleB)) + capsuleRadius</c>
        /// is less than or equal to <c>dot(n, pVertices[i])</c>. This requires <paramref name="pNormals"/> to be
        /// outward-facing unit normals.
        /// </remarks>
        public static ContainmentType ContainsConvexPolygonCapsule(Vector2[] pVertices, Vector2[] pNormals, Vector2 capsuleA, Vector2 capsuleB, float capsuleRadius)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.2 "Testing Sphere Against Plane" (2D reduction: circle vs line)

            if (capsuleRadius < 0f)
                return ContainmentType.Disjoint;

            if (!IsValidPolygon(pVertices, pNormals))
                return ContainmentType.Disjoint;

            if (!IntersectsCapsuleConvexPolygon(capsuleA, capsuleB, capsuleRadius, pVertices, pNormals))
                return ContainmentType.Disjoint;

            float r = capsuleRadius + Epsilon;

            for (int i = 0; i < pVertices.Length; i++)
            {
                Vector2 n = pNormals[i];
                float planeD = Vector2.Dot(n, pVertices[i]);

                float da = Vector2.Dot(n, capsuleA);
                float db = Vector2.Dot(n, capsuleB);
                float max = da > db ? da : db;

                if (max + r > planeD)
                    return ContainmentType.Intersects;
            }

            return ContainmentType.Contains;
        }

        /// <summary>
        /// Determines the containment relationship between a convex polygon and an oriented bounding box (OBB).
        /// </summary>
        /// <param name="pVertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="pNormals">
        /// The per-edge outward unit normals. The array length must match <paramref name="pVertices"/> so that
        /// <c>pNormals[i]</c> corresponds to the edge from <c>pVertices[i]</c> to <c>pVertices[(i + 1) % pVertices.Length]</c>.
        /// </param>
        /// <param name="obbCenter">The center of the OBB.</param>
        /// <param name="obbAxisX">The OBB local X axis direction.</param>
        /// <param name="obbAxisY">The OBB local Y axis direction.</param>
        /// <param name="obbHalf">The half-widths of the OBB along <paramref name="obbAxisX"/> and <paramref name="obbAxisY"/>.</param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the box lies entirely outside the polygon;
        /// <see cref="ContainmentType.Contains"/> if the box is fully contained within the polygon;
        /// otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the shapes overlap, containment is determined by testing
        /// whether all four OBB corners lie inside the polygon.
        /// </remarks>
        public static ContainmentType ContainsConvexPolygonObb(Vector2[] pVertices, Vector2[] pNormals, Vector2 obbCenter, Vector2 obbAxisX, Vector2 obbAxisY, Vector2 obbHalf)
        {
            if (!IsValidPolygon(pVertices, pNormals))
                return ContainmentType.Disjoint;

            if (!IntersectsObbConvexPolygon(obbCenter, obbAxisX, obbAxisY, obbHalf, pVertices, pNormals))
                return ContainmentType.Disjoint;

            // Contains: all 4 OBB corners inside polygon
            Vector2 ex = obbAxisX * obbHalf.X;
            Vector2 ey = obbAxisY * obbHalf.Y;

            Vector2 c0 = obbCenter - ex - ey;
            Vector2 c1 = obbCenter + ex - ey;
            Vector2 c2 = obbCenter + ex + ey;
            Vector2 c3 = obbCenter - ex + ey;

            if (ContainsConvexPolygonPoint(c0, pVertices, pNormals) == ContainmentType.Contains &&
                ContainsConvexPolygonPoint(c1, pVertices, pNormals) == ContainmentType.Contains &&
                ContainsConvexPolygonPoint(c2, pVertices, pNormals) == ContainmentType.Contains &&
                ContainsConvexPolygonPoint(c3, pVertices, pNormals) == ContainmentType.Contains)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the containment relationship between two convex polygons.
        /// </summary>
        /// <param name="aVertices">The reference polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="aNormals">
        /// The per-edge outward unit normals for the reference polygon. The array length must match <paramref name="aVertices"/> so that
        /// <c>aNormals[i]</c> corresponds to the edge from <c>aVertices[i]</c> to <c>aVertices[(i + 1) % aVertices.Length]</c>.
        /// </param>
        /// <param name="bVertices">The tested polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="bNormals">
        /// The per-edge outward unit normals for the tested polygon. The array length must match <paramref name="bVertices"/> so that
        /// <c>bNormals[i]</c> corresponds to the edge from <c>bVertices[i]</c> to <c>bVertices[(i + 1) % bVertices.Length]</c>.
        /// </param>
        /// <returns>
        /// <see cref="ContainmentType.Disjoint"/> if the polygons do not overlap; <see cref="ContainmentType.Contains"/> if the
        /// second polygon is fully contained within the first; otherwise, <see cref="ContainmentType.Intersects"/>.
        /// </returns>
        /// <remarks>
        /// This method first performs an intersection test. If the polygons overlap, containment is determined by verifying
        /// that every vertex of the second polygon lies inside the first polygon.
        /// </remarks>
        public static ContainmentType ContainsConvexPolygonConvexPolygon(Vector2[] aVertices, Vector2[] aNormals, Vector2[] bVertices, Vector2[] bNormals)
        {
            if (!IsValidPolygon(aVertices, aNormals) || !IsValidPolygon(bVertices, bNormals))
                return ContainmentType.Disjoint;

            // If they don't intersect at all, containment is impossible
            if (!IntersectsConvexPolygonConvexPolygon(aVertices, aNormals, bVertices, bNormals))
                return ContainmentType.Disjoint;

            // A contains B if all of B's vertices are inside A
            for (int i = 0; i < bVertices.Length; i++)
            {
                if (ContainsConvexPolygonPoint(bVertices[i], aVertices, aNormals) == ContainmentType.Disjoint)
                    return ContainmentType.Intersects;
            }

            return ContainmentType.Contains;
        }

        #endregion

        #endregion

        #region Projection

        /// <summary>
        /// Projects a set of points onto an axis and returns the resulting scalar interval.
        /// </summary>
        /// <param name="vertices">The points to project.</param>
        /// <param name="axis">The axis onto which the points are projected.</param>
        /// <param name="min">When this method returns, contains the minimum projection value.</param>
        /// <param name="max">When this method returns, contains the maximum projection value.</param>
        /// <remarks>
        /// The projection of a point <c>v</c> onto <paramref name="axis"/> is computed as <c>dot(v, axis)</c>.
        /// </remarks>
        public static void ProjectOntoAxis(Vector2[] vertices, Vector2 axis, out float min, out float max)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.1 "Separating-axis Test" (2D adaptation)

            min = float.MaxValue;
            max = float.MinValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                float p = Vector2.Dot(vertices[i], axis);
                if (p < min) min = p;
                if (p > max) max = p;
            }
        }

        /// <summary>
        /// Projects an axis-aligned bounding box (AABB) onto an axis and returns the resulting scalar interval.
        /// </summary>
        /// <param name="center">The center of the AABB.</param>
        /// <param name="halfExtents">The half-extents of the AABB along the world X and Y axes.</param>
        /// <param name="axis">The axis onto which the AABB is projected.</param>
        /// <param name="min">When this method returns, contains the minimum projection value.</param>
        /// <param name="max">When this method returns, contains the maximum projection value.</param>
        /// <remarks>
        /// The interval is computed as <c>[dot(center, axis) - r, dot(center, axis) + r]</c>, where
        /// <c>r = halfExtents.X * |axis.X| + halfExtents.Y * |axis.Y|</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ProjectAabbOntoAxis(Vector2 center, Vector2 halfExtents, Vector2 axis, out float min, out float max)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.1 "Separating-axis Test" (2D adaptation)

            // Axis does not need to be normalized as long as both shapes use the same axis
            float c = Vector2.Dot(center, axis);
            float r = halfExtents.X * MathF.Abs(axis.X) + halfExtents.Y * MathF.Abs(axis.Y);

            min = c - r;
            max = c + r;
        }

        /// <summary>
        /// Projects an oriented bounding box (OBB) onto an axis and returns the resulting scalar interval.
        /// </summary>
        /// <param name="center">The center of the OBB.</param>
        /// <param name="axisX">The OBB local X axis direction.</param>
        /// <param name="axisY">The OBB local Y axis direction.</param>
        /// <param name="halfExtents">The half-extents of the OBB along <paramref name="axisX"/> and <paramref name="axisY"/>.</param>
        /// <param name="axis">The axis onto which the OBB is projected.</param>
        /// <param name="min">When this method returns, contains the minimum projection value.</param>
        /// <param name="max">When this method returns, contains the maximum projection value.</param>
        /// <remarks>
        /// The interval is computed as <c>[dot(center, axis) - r, dot(center, axis) + r]</c>, where
        /// <c>r = halfExtents.X * |dot(axisX, axis)| + halfExtents.Y * |dot(axisY, axis)|</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ProjectObbOntoAxis(Vector2 center, Vector2 axisX, Vector2 axisY, Vector2 halfExtents, Vector2 axis, out float min, out float max)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.1 "Separating-axis Test" (2D adaptation)

            float c = Vector2.Dot(center, axis);

            // Radius is sum of projected half-extents onto the axis
            float r = halfExtents.X * MathF.Abs(Vector2.Dot(axisX, axis)) +
                      halfExtents.Y * MathF.Abs(Vector2.Dot(axisY, axis));

            min = c - r;
            max = c + r;
        }

        /// <summary>
        /// Determines whether the projections of two point sets overlap on an axis.
        /// </summary>
        /// <param name="aVerts">The vertices of the first shape.</param>
        /// <param name="bVerts">The vertices of the second shape.</param>
        /// <param name="axis">The axis onto which both point sets are projected.</param>
        /// <returns><see langword="true"/> if the projection intervals overlap; otherwise, <see langword="false"/>.</returns>
        public static bool OverlapOnAxis(Vector2[] aVerts, Vector2[] bVerts, Vector2 axis)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.1 "Separating-axis Test" (2D adaptation)

            ProjectOntoAxis(aVerts, axis, out float minA, out float maxA);
            ProjectOntoAxis(bVerts, axis, out float minB, out float maxB);
            return IntervalsOverlap(minA, maxA, minB, maxB);
        }

        /// <summary>
        /// Determines whether the projections of an axis-aligned bounding box (AABB) and a point set overlap on an axis.
        /// </summary>
        /// <param name="aabbCenter">The center of the AABB.</param>
        /// <param name="aabbHalfExtents">The half-extents of the AABB along the world X and Y axes.</param>
        /// <param name="polygonVertices">The vertices of the shape being tested.</param>
        /// <param name="axis">The axis onto which both shapes are projected.</param>
        /// <returns><see langword="true"/> if the projection intervals overlap; otherwise, <see langword="false"/>.</returns>
        public static bool OverlapOnAxis(Vector2 aabbCenter, Vector2 aabbHalfExtents, Vector2[] polygonVertices, Vector2 axis)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.1 "Separating-axis Test" (2D adaptation)

            ProjectAabbOntoAxis(aabbCenter, aabbHalfExtents, axis, out float minA, out float maxA);
            ProjectOntoAxis(polygonVertices, axis, out float minB, out float maxB);
            return IntervalsOverlap(minA, maxA, minB, maxB);
        }

        #endregion

        #region Distance / Closest Point

        /// <summary>
        /// Computes the closest points between a ray and a line segment and returns the corresponding parameters and squared distance.
        /// </summary>
        /// <param name="rayOrigin">The ray origin.</param>
        /// <param name="rayDirection">
        /// The ray direction vector. The ray is parameterized as <c>rayOrigin + s * rayDirection</c> with <c>s &gt;= 0</c>.
        /// </param>
        /// <param name="segA">The first endpoint of the segment.</param>
        /// <param name="segB">The second endpoint of the segment.</param>
        /// <param name="sRay">
        /// When this method returns, contains the ray parameter <c>s</c> of the closest point on the ray. This value is clamped to
        /// <c>s &gt;= 0</c>.
        /// </param>
        /// <param name="tSeg">
        /// When this method returns, contains the segment parameter <c>t</c> of the closest point on the segment. The segment is
        /// parameterized as <c>segA + t * (segB - segA)</c> with <c>0 &lt;= t &lt;= 1</c>.
        /// </param>
        /// <param name="distanceSquared">When this method returns, contains the squared distance between the closest points.</param>
        public static void ClosestPointRaySegment(Vector2 rayOrigin, Vector2 rayDirection, Vector2 segA, Vector2 segB, out float sRay, out float tSeg, out float distanceSquared)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.1.9 "Closest Points of Two Line Segments" (ray vs segment adaptation)
            // Solve closest points on the supporting lines, then clamp to ray/segment parameter domains.

            Vector2 d1 = rayDirection;
            Vector2 d2 = segB - segA;
            Vector2 r = rayOrigin - segA;

            float a = Vector2.Dot(d1, d1);
            float e = Vector2.Dot(d2, d2);

            // Degenerate ray (treat as point)
            if (a <= Epsilon)
            {
                // Ray is a point at rayOrigin, clamp to segment
                if (e <= Epsilon)
                {
                    tSeg = 0.0f;
                }
                else
                {
                    tSeg = MathHelper.Clamp(Vector2.Dot(-r, d2) / e, 0.0f, 1.0f);
                }

                Vector2 q = segA + tSeg * d2;
                sRay = 0.0f;
                distanceSquared = Vector2.DistanceSquared(rayOrigin, q);
                return;
            }

            // Degenerate segment (treat as point)
            if (e <= Epsilon)
            {
                // Segment is a point at segA, clamp ray to s >= 0
                float c = Vector2.Dot(d1, r);
                sRay = MathF.Max(-c / a, 0.0f);
                tSeg = 0.0f;
                Vector2 p = rayOrigin + sRay * d1;
                distanceSquared = Vector2.DistanceSquared(p, segA);
                return;
            }

            float b = Vector2.Dot(d1, d2);
            float c2 = Vector2.Dot(d1, r);
            float f = Vector2.Dot(d2, r);

            float denom = a * e - b * b;

            float s = 0.0f;
            float t = 0.0f;

            // If not parallel, compute closest point on infinite lines first
            if (MathF.Abs(denom) > Epsilon)
            {
                s = (b * f - c2 * e) / denom;
            }
            else
            {
                // Parallel-ish, pick s=0 initially, we'll clamp below
                s = 0.0f;
            }

            // Clamp s to ray domain [0, +inf]
            if (s < 0.0f)
            {
                s = 0.0f;
                t = MathHelper.Clamp(f / e, 0.0f, 1.0f);
            }
            else
            {
                // Compute t from s
                t = (b * s + f) / e;

                // Clamp t to segment [0,1] and recompute s if needed
                if (t < 0.0f)
                {
                    t = 0.0f;
                    s = MathF.Max(-c2 / a, 0.0f);
                }
                else if (t > 1.0f)
                {
                    t = 1.0f;
                    s = MathF.Max((b - c2) / a, 0.0f);
                }
            }

            Vector2 pClosest = rayOrigin + s * d1;
            Vector2 qClosest = segA + t * d2;

            sRay = s;
            tSeg = t;
            distanceSquared = Vector2.DistanceSquared(pClosest, qClosest);
        }

        /// <summary>
        /// Computes the squared distance from a point to a line segment and returns the closest point on the segment.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <param name="a">The first endpoint of the segment.</param>
        /// <param name="b">The second endpoint of the segment.</param>
        /// <param name="t">
        /// When this method returns, contains the segment parameter of the closest point. The segment is parameterized as
        /// <c>a + t * (b - a)</c> with <c>0 &lt;= t &lt;= 1</c>.
        /// </param>
        /// <param name="closestPoint">When this method returns, contains the closest point on the segment to <paramref name="point"/>.</param>
        /// <returns>The squared distance between <paramref name="point"/> and the segment <c>[a, b]</c>.</returns>
        /// <remarks>
        /// The parameter is computed by projecting <c>(point - a)</c> onto <c>(b - a)</c> and clamping to the segment domain:
        /// <c>t = clamp(dot(point - a, b - a) / dot(b - a, b - a), 0, 1)</c>. Squared distance is returned to avoid a square root.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquaredPointSegment(Vector2 point, Vector2 a, Vector2 b, out float t, out Vector2 closestPoint)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.1.2 "Closest Point on Line Segment to Point"

            Vector2 ab = b - a;
            float denom = Vector2.Dot(ab, ab);

            // Check if segment is degenerate
            if (denom <= EpsilonSq)
            {
                // Segment is degenerate, treat it as a point
                t = 0.0f;
                closestPoint = a;
                Vector2 d = point - a;
                return d.X * d.X + d.Y * d.Y;
            }

            t = Vector2.Dot(point - a, ab) / denom;
            t = MathHelper.Clamp(t, 0.0f, 1.0f);
            closestPoint = a + t * ab;

            Vector2 diff = point - closestPoint;
            return diff.LengthSquared();
        }

        /// <summary>
        /// Computes the squared distance between two line segments and returns the closest points on each segment.
        /// </summary>
        /// <param name="p1">The first endpoint of the first segment.</param>
        /// <param name="q1">The second endpoint of the first segment.</param>
        /// <param name="p2">The first endpoint of the second segment.</param>
        /// <param name="q2">The second endpoint of the second segment.</param>
        /// <param name="s">
        /// When this method returns, contains the parameter of the closest point on the first segment. The first segment is
        /// parameterized as <c>p1 + s * (q1 - p1)</c> with <c>0 &lt;= s &lt;= 1</c>.
        /// </param>
        /// <param name="t">
        /// When this method returns, contains the parameter of the closest point on the second segment. The second segment is
        /// parameterized as <c>p2 + t * (q2 - p2)</c> with <c>0 &lt;= t &lt;= 1</c>.
        /// </param>
        /// <param name="c1">When this method returns, contains the closest point on the first segment.</param>
        /// <param name="c2">When this method returns, contains the closest point on the second segment.</param>
        /// <returns>The squared distance between the two segments.</returns>
        /// <remarks>
        /// Based on the closest-point computation for two segments described in:
        /// <para>
        /// C. Ericson, <i>Real-Time Collision Detection</i>, Morgan Kaufmann, 2005.
        /// </para>
        /// The method solves for the closest points on the infinite lines, then clamps the parameters to the segment domains.
        /// Degenerate segments (zero length) are treated as points. Squared distance is returned to avoid a square root.
        /// </remarks>
        public static float DistanceSquaredSegmentSegment(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2, out float s, out float t, out Vector2 c1, out Vector2 c2)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Derived from Section 5.1.9 "Closest Points of Two Line Segments" (ray/segment domain adaptation)

            Vector2 d1 = q1 - p1;   // Direction vector of segment S1
            Vector2 d2 = q2 - p2;   // Direction vector of segment S2
            Vector2 r = p1 - p2;

            float a = Vector2.Dot(d1, d1);  // Squared length of segment S1
            float e = Vector2.Dot(d2, d2);  // Squared length of segment S2
            float f = Vector2.Dot(d2, r);

            // Default outputs
            s = 0.0f;
            t = 0.0f;

            // Check if either or both segments degenerate into points
            if (a <= Epsilon && e <= Epsilon)
            {
                c1 = p1;
                c2 = p2;
                Vector2 d = c1 - c2;
                return d.LengthSquared();
            }

            if (a <= Epsilon)
            {
                // First segment degenerates into a point
                s = 0.0f;
                t = f / e;
                t = MathHelper.Clamp(t, 0.0f, 1.0f);
            }
            else
            {
                float c = Vector2.Dot(d1, r);

                if (e <= Epsilon)
                {
                    // Second segment degenerates into a point
                    t = 0.0f;
                    s = MathHelper.Clamp(-c / a, 0.0f, 1.0f);
                }
                else
                {
                    // General nondegenerate case starts here
                    float b = Vector2.Dot(d1, d2);
                    float denom = a * e - b * b;    // Always nonnegative

                    // If segments not parallel, compute closest point on L1, to L2 and
                    // clamp to segment s!  Else pick arbitrary s (here 0)
                    if (MathF.Abs(denom) > Epsilon)
                    {
                        s = MathHelper.Clamp((b * f - c * e) / denom, 0.0f, 1.0f);
                    }
                    else
                    {
                        s = 0.0f;
                    }

                    //  Compute point on L2 closest to S1(s) using
                    // t = Dot((P1 + D1*s) - P2,D2) / Dot(D2,D2) = (b*s + f) / e
                    t = (b * s + f) / e;

                    // If t in [0,1] do nothing. Else clamp t, recompute s for
                    // the new value of t using
                    // s = Dot((P2 + D2*t) - P1,D1) / Dot(D1,D1) = (t*b - c) / a
                    // and clamp s to [0,1]
                    if (t < 0.0f)
                    {
                        t = 0.0f;
                        s = MathHelper.Clamp(-c / a, 0.0f, 1.0f);
                    }
                    else if (t > 1.0f)
                    {
                        t = 1.0f;
                        s = MathHelper.Clamp((b - c) / a, 0.0f, 1.0f);
                    }
                }
            }

            c1 = p1 + d1 * s;
            c2 = p2 + d2 * t;
            Vector2 diff = c1 - c2;
            return diff.LengthSquared();
        }

        /// <summary>
        /// Computes the squared distance from a point to an axis-aligned bounding box (AABB).
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <param name="min">The minimum corner of the AABB.</param>
        /// <param name="max">The maximum corner of the AABB.</param>
        /// <returns>The squared distance between <paramref name="point"/> and the AABB.</returns>
        /// <remarks>
        /// The closest point on the AABB is found by clamping each coordinate of <paramref name="point"/> to the range
        /// defined by <paramref name="min"/> and <paramref name="max"/>, then computing the squared distance to that point.
        /// Squared distance is returned to avoid a square root.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquaredPointAabb(Vector2 point, Vector2 min, Vector2 max)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.1.3 "Closest Point on AABB to Point"

            float cx = MathHelper.Clamp(point.X, min.X, max.X);
            float cy = MathHelper.Clamp(point.Y, min.Y, max.Y);

            float dx = point.X - cx;
            float dy = point.Y - cy;

            return dx * dx + dy * dy;
        }

        /// <summary>
        /// Computes the squared distance from a point to an oriented bounding box (OBB).
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <param name="obbCenter">The center of the OBB.</param>
        /// <param name="obbAxisX">The OBB local X axis direction.</param>
        /// <param name="obbAxisY">The OBB local Y axis direction.</param>
        /// <param name="obbHalfExtents">The half-widths of the OBB along <paramref name="obbAxisX"/> and <paramref name="obbAxisY"/>.</param>
        /// <returns>The squared distance between <paramref name="point"/> and the OBB.</returns>
        /// <remarks>
        /// The point is projected into the OBB's local space using dot products with the box axes. The closest point in local
        /// space is obtained by clamping the local coordinates to <c>[-halfExtents, +halfExtents]</c>, and the squared
        /// distance is computed in that local space. Squared distance is returned to avoid a square root.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquaredPointObb(Vector2 point, Vector2 obbCenter, Vector2 obbAxisX, Vector2 obbAxisY, Vector2 obbHalfExtents)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.1.4 "Closest Point on OBB to Point"

            Vector2 d = point - obbCenter;

            // Point in local space
            float x = Vector2.Dot(d, obbAxisX);
            float y = Vector2.Dot(d, obbAxisY);

            float cx = MathHelper.Clamp(x, -obbHalfExtents.X, obbHalfExtents.X);
            float cy = MathHelper.Clamp(y, -obbHalfExtents.Y, obbHalfExtents.Y);

            float dx = x - cx;
            float dy = y - cy;

            return dx * dx + dy * dy;
        }

        /// <summary>
        /// Computes the squared distance from a point to a convex polygon.
        /// </summary>
        /// <param name="p">The point to measure from.</param>
        /// <param name="vertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="normals">
        /// The per-edge outward unit normals. The array length must match <paramref name="vertices"/> so that
        /// <c>normals[i]</c> corresponds to the edge from <c>vertices[i]</c> to <c>vertices[(i + 1) % vertices.Length]</c>.
        /// </param>
        /// <returns>The squared distance between <paramref name="p"/> and the polygon. Returns <c>0</c> when the point is inside.</returns>
        /// <remarks>
        /// If the point lies inside the polygon, the distance is zero. Otherwise, the distance is the minimum squared distance
        /// to any polygon edge segment. Squared distance is returned to avoid a square root.
        /// </remarks>
        public static float DistanceSquaredPointConvexPolygon(Vector2 p, Vector2[] vertices, Vector2[] normals)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Derived from:
            //  - Section 5.2.1 "Separating-axis Test" (convex point containment)
            //  - Section 5.1.2 "Closest Point on Line Segment to Point"
            // 2D reduction of convex polyhedron distance computation

            if (ContainsConvexPolygonPoint(p, vertices, normals) == ContainmentType.Contains)
                return 0.0f;

            float best = float.MaxValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                int j = (i + 1) % vertices.Length;
                float d2 = DistanceSquaredPointSegment(p, vertices[i], vertices[j], out _, out _);
                if (d2 < best) best = d2;
            }

            return best;
        }

        /// <summary>
        /// Computes the squared distance between a line segment and an axis-aligned bounding box (AABB).
        /// </summary>
        /// <param name="a">The first endpoint of the segment.</param>
        /// <param name="b">The second endpoint of the segment.</param>
        /// <param name="min">The minimum corner of the AABB.</param>
        /// <param name="max">The maximum corner of the AABB.</param>
        /// <returns>The squared distance between the segment <c>[a, b]</c> and the AABB. Returns <c>0</c> when they intersect.</returns>
        /// <remarks>
        /// If the segment intersects the AABB, the distance is zero. Otherwise, the distance is the minimum of:
        /// <list type="bullet">
        /// <item><description>The squared distance from each segment endpoint to the AABB.</description></item>
        /// <item><description>The squared distance between the segment and each of the four AABB edge segments.</description></item>
        /// </list>
        /// Squared distance is returned to avoid a square root.
        /// </remarks>
        public static float DistanceSquaredSegmentAabb(Vector2 a, Vector2 b, Vector2 min, Vector2 max)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Derived from:
            //  - Section 5.1.3 "Closest Point on AABB to Point" (endpoint distance)
            //  - Section 5.1.9 "Closest Points of Two Line Segments" (segment-edge distance)
            //  - Parametric clipping against convex regions (segment/AABB intersection test; 2D reduction)

            Vector2 d = b - a;
            float dd = Vector2.Dot(d, d);

            // Check for degenerate segment
            if (dd < EpsilonSq)
            {
                // Segment is degenerate, treat as point
                return DistanceSquaredPointAabb(a, min, max);
            }

            // If the segment intersects the AABB, distance is zero.
            if (ClipLineToAabb(a, d, min, max, 0.0f, 1.0f, out _, out _))
                return 0.0f;

            // Otherwise, minimum of:
            // - endpoints to AABB
            // - segment to each AABB edge segment
            float best = DistanceSquaredPointAabb(a, min, max);
            float db = DistanceSquaredPointAabb(b, min, max);
            if (db < best) best = db;

            // AABB corners
            Vector2 c0 = new Vector2(min.X, min.Y);
            Vector2 c1 = new Vector2(max.X, min.Y);
            Vector2 c2 = new Vector2(max.X, max.Y);
            Vector2 c3 = new Vector2(min.X, max.Y);

            // Distance segment-to-edge
            float dist;

            dist = DistanceSquaredSegmentSegment(a, b, c0, c1, out _, out _, out _, out _);
            if (dist < best) best = dist;

            dist = DistanceSquaredSegmentSegment(a, b, c1, c2, out _, out _, out _, out _);
            if (dist < best) best = dist;

            dist = DistanceSquaredSegmentSegment(a, b, c2, c3, out _, out _, out _, out _);
            if (dist < best) best = dist;

            dist = DistanceSquaredSegmentSegment(a, b, c3, c0, out _, out _, out _, out _);
            if (dist < best) best = dist;

            return best;
        }

        /// <summary>
        /// Computes the squared distance between a line segment and a convex polygon.
        /// </summary>
        /// <param name="a">The first endpoint of the segment.</param>
        /// <param name="b">The second endpoint of the segment.</param>
        /// <param name="vertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="normals">
        /// The per-edge outward unit normals. The array length must match <paramref name="vertices"/> so that
        /// <c>normals[i]</c> corresponds to the edge from <c>vertices[i]</c> to <c>vertices[(i + 1) % vertices.Length]</c>.
        /// </param>
        /// <returns>
        /// The squared distance between the segment <c>[a, b]</c> and the polygon. Returns <c>0</c> when they intersect.
        /// If the polygon data is invalid, returns <see cref="float.MaxValue"/>.
        /// </returns>
        /// <remarks>
        /// If the segment intersects the polygon, the distance is zero. Otherwise, the distance is the minimum squared distance
        /// between the segment and any polygon edge segment. Squared distance is returned to avoid a square root.
        /// </remarks>
        public static float DistanceSquaredSegmentConvexPolygon(Vector2 a, Vector2 b, Vector2[] vertices, Vector2[] normals)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Derived from:
            //  - Section 5.2.1 "Separating Axis Test" (convex polygon half-space representation and clipping)
            //  - Section 5.1.9 "Closest Points of Two Line Segments" (segment–edge distance)
            //  - Section 5.1.2 "Closest Point on Line Segment to Point" (degenerate segment handling)

            if (!IsValidPolygon(vertices, normals))
                return float.MaxValue;

            Vector2 d = b - a;
            float dd = Vector2.Dot(d, d);

            // Check for degenerate segment
            if (dd <= EpsilonSq)
            {
                // Treat segment as point
                return DistanceSquaredPointConvexPolygon(a, vertices, normals);
            }

            // If segment intersects polygon, distance is zero
            if (ClipLineToConvexPolygon(a, d, vertices, normals, 0.0f, 1.0f, out _, out _))
                return 0.0f;

            // Otherwise min distance to edges
            float best = float.MaxValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                int j = (i + 1) % vertices.Length;
                float d2 = DistanceSquaredSegmentSegment(a, b, vertices[i], vertices[j], out _, out _, out _, out _);
                if (d2 < best) best = d2;
            }

            return best;
        }

        #endregion

        #region Parametric Solvers

        /// <summary>
        /// Solves the parametric intersection between a line in implicit form and a parametric line, ray, or segment.
        /// </summary>
        /// <param name="lineNormal">The normal vector of the implicit line.</param>
        /// <param name="lineDistance">The line offset <c>d</c> in the implicit equation <c>dot(lineNormal, x) = d</c>.</param>
        /// <param name="origin">The origin of the parametric line.</param>
        /// <param name="direction">The direction vector of the parametric line.</param>
        /// <param name="t">
        /// When this method returns, contains the parameter <c>t</c> such that <c>origin + t * direction</c> lies on the
        /// implicit line.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a unique solution exists; otherwise, <see langword="false"/> if the parametric line is
        /// parallel to the implicit line.
        /// </returns>
        /// <remarks>
        /// This method solves <c>dot(lineNormal, origin + t * direction) = lineDistance</c>. A unique intersection exists
        /// when <c>dot(lineNormal, direction) != 0</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SolveParametricIntersectionWithImplicitLine(Vector2 lineNormal, float lineDistance, Vector2 origin, Vector2 direction, out float t)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Derived from Section 5.3.1 "Intersecting Segment Against Plane" (2D reduction)
            //
            // Solves the intersection between:
            //   Implicit line: dot(n, x) = d
            //   Parametric line: x = origin + t * direction
            // yielding:
            //   t = (d - dot(n, origin)) / dot(n, direction)

            float denom = Vector2.Dot(lineNormal, direction);
            if (MathF.Abs(denom) < Epsilon)
            {
                t = default;
                return false;
            }

            t = (lineDistance - Vector2.Dot(lineNormal, origin)) / denom;
            return true;
        }

        /// <summary>
        /// Solves the parametric intersection of two 2D lines in point-direction form.
        /// </summary>
        /// <param name="origin1">Origin of the first line.</param>
        /// <param name="direction1">Direction of the first line.</param>
        /// <param name="origin2">Origin of the second line.</param>
        /// <param name="direction2">Direction of the second line.</param>
        /// <param name="t1">
        /// When this method returns <see langword="true"/>, contains the parametric value on the first line.
        /// </param>
        /// <param name="t2">
        /// When this method returns <see langword="true"/>, contains the parametric value on the second line.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the directions are not parallel; otherwise, <see langword="false"/>.
        /// Parallel and collinear cases are treated as no single-point intersection.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SolveParametricIntersection2D(Vector2 origin1, Vector2 direction1, Vector2 origin2, Vector2 direction2, out float t1, out float t2)
        {
            // Standard 2D line intersection in point-direction form.
            // Uses 2D cross product (perp-dot): cross(a,b) = perpDot(a,b).
            // Solve: o1 + t1*d1 = o2 + t2*d2
            // => t1 = cross(o2 - o1, d2) / cross(d1, d2)
            // => t2 = cross(o2 - o1, d1) / cross(d1, d2)

            float cross = Vector2.PerpDot(direction1, direction2);

            // Check if parallel
            if (MathF.Abs(cross) < Epsilon)
            {
                // Parallel or coincident, no intersection
                t1 = default;
                t2 = default;
                return false;
            }

            Vector2 diff = origin2 - origin1;

            // Standard 2D line intersection parameters using perp-dot.
            t1 = Vector2.PerpDot(diff, direction2) / cross;
            t2 = Vector2.PerpDot(diff, direction1) / cross;
            return true;
        }

        #endregion

        #region Clipping & Ray Intervals

        /// <summary>
        /// Clips a parametric line <c>P(t) = origin + t * direction</c> against an axis-aligned bounding box
        /// defined by <paramref name="min"/> and <paramref name="max"/>, restricting the result to the parametric
        /// interval [<paramref name="tLower"/>, <paramref name="tUpper"/>].
        /// </summary>
        /// <param name="origin">The parametric line origin.</param>
        /// <param name="direction">The parametric line direction (not required to be unit length).</param>
        /// <param name="min">Minimum corner of the AABB.</param>
        /// <param name="max">Maximum corner of the AABB.</param>
        /// <param name="tLower">Lower bound for the allowed parameter range (e.g. 0 for rays/segments, -infinity for lines).</param>
        /// <param name="tUpper">Upper bound for the allowed parameter range (e.g. 1 for segments, +infinity for rays/lines).</param>
        /// <param name="tEnter">
        /// When this method returns <see langword="true"/>, contains the entering parameter of the clipped interval,
        /// clamped to [<paramref name="tLower"/>, <paramref name="tUpper"/>].
        /// </param>
        /// <param name="tExit">
        /// When this method returns <see langword="true"/>, contains the exiting parameter of the clipped interval,
        /// clamped to [<paramref name="tLower"/>, <paramref name="tUpper"/>].
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the parametric line overlaps the AABB over any interval within
        /// [<paramref name="tLower"/>, <paramref name="tUpper"/>]; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This is the 2D slab test (Ericson-style). Using different [tLower, tUpper] produces:
        /// <list type="bullet">
        /// <item><description>Infinite line: (-∞, +∞)</description></item>
        /// <item><description>Ray: [0, +∞)</description></item>
        /// <item><description>Segment: [0, 1]</description></item>
        /// </list>
        /// </remarks>
        public static bool ClipLineToAabb(Vector2 origin, Vector2 direction, Vector2 min, Vector2 max, float tLower, float tUpper, out float tEnter, out float tExit)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.3.3 "Intersecting Ray or Segment Against Box" (slab method / parametric interval clipping).
            // Note: This is equivalent in form to Liang–Barsky style parametric line clipping against axis-aligned boundaries.

            float tMin = tLower;
            float tMax = tUpper;

            // X Slab
            if (MathF.Abs(direction.X) < Epsilon)
            {
                // parallel to X planes; must be within slab
                if (origin.X < min.X - Epsilon || origin.X > max.X + Epsilon)
                {
                    tEnter = tExit = default;
                    return false;
                }
            }
            else
            {
                float inv = 1.0f / direction.X;
                float t1 = (min.X - origin.X) * inv;
                float t2 = (max.X - origin.X) * inv;
                if (t1 > t2) (t1, t2) = (t2, t1);

                if (!ClipInterval(ref tMin, ref tMax, t1, t2))
                {
                    tEnter = tExit = default;
                    return false;
                }
            }

            // Y slab
            if (MathF.Abs(direction.Y) < Epsilon)
            {
                // Parallel to Y planes, must be within slab
                if (origin.Y < min.Y - Epsilon || origin.Y > max.Y + Epsilon)
                {
                    tEnter = tExit = default;
                    return false;
                }
            }
            else
            {
                float inv = 1.0f / direction.Y;
                float t1 = (min.Y - origin.Y) * inv;
                float t2 = (max.Y - origin.Y) * inv;
                if (t1 > t2) (t1, t2) = (t2, t1);

                if (!ClipInterval(ref tMin, ref tMax, t1, t2))
                {
                    tEnter = tExit = default;
                    return false;
                }
            }

            tEnter = tMin;
            tExit = tMax;
            return true;
        }

        /// <summary>
        /// Clips a parametric line against a convex polygon expressed as an intersection of half-spaces.
        /// </summary>
        /// <param name="origin">The line origin.</param>
        /// <param name="direction">The line direction vector.</param>
        /// <param name="vertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="normals">
        /// The per-edge outward unit normals. The array length must match <paramref name="vertices"/> so that
        /// <c>normals[i]</c> corresponds to the edge from <c>vertices[i]</c> to <c>vertices[(i + 1) % vertices.Length]</c>.
        /// </param>
        /// <param name="tLower">The lower bound of the line parameter interval to clip.</param>
        /// <param name="tUpper">The upper bound of the line parameter interval to clip.</param>
        /// <param name="tEnter">When this method returns, contains the entering parameter of the clipped interval.</param>
        /// <param name="tExit">When this method returns, contains the exiting parameter of the clipped interval.</param>
        /// <returns>
        /// <see langword="true"/> if the line intersects the polygon within the parameter range <c>[tLower, tUpper]</c>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method incrementally intersects the parameter interval <c>[tLower, tUpper]</c> with the constraints induced by
        /// each polygon edge half-space <c>dot(n, X) &lt;= dot(n, v)</c>. For a segment or ray, pass an appropriate parameter
        /// range (for example, <c>[0, 1]</c> for a segment and <c>[0, +inf)</c> for a ray).
        /// </remarks>
        public static bool ClipLineToConvexPolygon(Vector2 origin, Vector2 direction, Vector2[] vertices, Vector2[] normals, float tLower, float tUpper, out float tEnter, out float tExit)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Derived from Section 5.3.8 "Intersecting Ray or Segment Against Convex Polyhedron" (2D reduction).
            // Also known as Cyrus–Beck clipping (parametric line clipping against a convex polygon via half-space constraints).
            // Half-space: dot(n, X) <= dot(n, v)
            // Parametric: X(t) = origin + t * direction


            if (!IsValidPolygon(vertices, normals))
            {
                tEnter = tExit = default;
                return false;
            }

            float tMin = tLower;
            float tMax = tUpper;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 n = normals[i];
                Vector2 v = vertices[i];

                // Half space: Dot(n, X) <= Dot(n, v)
                float planeD = Vector2.Dot(n, v);
                float dist = planeD - Vector2.Dot(n, origin);
                float denom = Vector2.Dot(n, direction);

                // Parallel to plane
                if (MathF.Abs(denom) < Epsilon)
                {
                    // If outside (dist < 0) no intersection with the convex region
                    if (dist < -Epsilon)
                    {
                        tEnter = tExit = default;
                        return false;
                    }

                    // Otherwise, line is inside/on this half-space; no constraint from edge
                    continue;
                }

                float t = dist / denom;

                // With outward normals:
                // denom > 0 => moving toward increasing Dot(n, X) => exiting the inside half-space
                // denom < 0 => moving inward => entering
                if (denom > 0.0f)
                {
                    if (t < tMax)
                        tMax = t;
                }
                else
                {
                    if (t > tMin)
                        tMin = t;
                }

                if (tMin > tMax)
                {
                    tEnter = tExit = default;
                    return false;
                }
            }

            tEnter = tMin;
            tExit = tMax;
            return true;
        }

        /// <summary>
        /// Computes the parametric intersection interval between a ray and a circle.
        /// </summary>
        /// <param name="origin">The ray origin.</param>
        /// <param name="direction">
        /// The ray direction vector. The ray is parameterized as <c>origin + t * direction</c> with <c>t &gt;= 0</c>.
        /// </param>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The circle radius.</param>
        /// <param name="tMin">
        /// When this method returns, contains the smaller intersection parameter along the supporting line of the ray.
        /// </param>
        /// <param name="tMax">
        /// When this method returns, contains the larger intersection parameter along the supporting line of the ray.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the supporting line intersects the circle; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Based on solving the quadratic equation obtained by substituting the ray equation
        /// <c>x = origin + t * direction</c> into the circle equation <c>|x - center|^2 = radius^2</c>.
        /// The returned parameters are ordered such that <paramref name="tMin"/> is less than or equal to
        /// <paramref name="tMax"/>.
        /// </remarks>
        public static bool RayCircleIntersectionInterval(Vector2 origin, Vector2 direction, Vector2 center, float radius, out float tMin, out float tMax)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.3.2 "Intersecting Ray or Segment Against Sphere" (2D reduction).
            // Solves the quadratic |origin + t*direction - center|^2 = radius^2.

            Vector2 m = origin - center;

            float a = Vector2.Dot(direction, direction);
            if (a <= EpsilonSq)
            {
                tMin = tMax = default;
                return false;
            }

            float b = Vector2.Dot(m, direction);
            float c = Vector2.Dot(m, m) - radius * radius;

            // Ray origin outside circle (c > 0)
            // and ray pointing away from circle (b > 0)
            if (c > 0.0f && b > 0.0f)
            {
                tMin = tMax = default;
                return false;
            }

            float discriminant = b * b - a * c;

            // Negative discriminant means ray misses circle
            if (discriminant < 0.0f)
            {
                tMin = tMax = default;
                return false;
            }

            float sqrtD = MathF.Sqrt(discriminant);
            float invA = 1.0f / a;

            tMin = (-b - sqrtD) * invA;
            tMax = (-b + sqrtD) * invA;

            if (tMin > tMax)
                (tMin, tMax) = (tMax, tMin);

            // Clamp tMin to 0 for ray semantics
            if (tMin < 0.0f)
                tMin = 0.0f;

            return true;
        }

        /// <summary>
        /// Computes the parametric intersection interval between a ray and a capsule.
        /// </summary>
        /// <param name="rayOrigin">The ray origin.</param>
        /// <param name="rayDirection">
        /// The ray direction vector. The ray is parameterized as <c>rayOrigin + t * rayDirection</c> with <c>t &gt;= 0</c>.
        /// </param>
        /// <param name="segA">The first endpoint of the capsule line segment.</param>
        /// <param name="segB">The second endpoint of the capsule line segment.</param>
        /// <param name="radius">The capsule radius.</param>
        /// <param name="tMin">When this method returns, contains the smaller intersection parameter along the supporting line of the ray.</param>
        /// <param name="tMax">When this method returns, contains the larger intersection parameter along the supporting line of the ray.</param>
        /// <returns>
        /// <see langword="true"/> if the supporting line intersects the capsule; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The capsule is treated as the Minkowski sum of the segment <c>[segA, segB]</c> and a disk of radius <paramref name="radius"/>.
        /// The returned interval is computed by finding the closest approach between the ray and the capsule's medial segment and
        /// expanding along the ray by the available radial slack. Degenerate inputs are handled by reducing to point/segment or
        /// ray/circle cases.
        /// </remarks>
        public static bool RayCapsuleIntersectionInterval(Vector2 rayOrigin, Vector2 rayDirection, Vector2 segA, Vector2 segB, float radius, out float tMin, out float tMax)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Capsule as swept sphere / Minkowski sum of segment and disk (concept).
            // Built from these Ericson primitives (2D reductions / domain adaptations):
            // - Section 5.3.2 "Intersecting Ray or Segment Against Sphere"  (ray-circle interval)
            // - Section 5.1.2 "Closest Point on Line Segment to Point"      (point-segment distance; degenerate ray)
            // - Section 5.1.9 "Closest Points of Two Line Segments"         (adapted to closest points of ray vs segment)
            // The returned interval is formed by expanding around the closest-approach parameter using the available radial slack.

            float a = Vector2.Dot(rayDirection, rayDirection);
            float radiusSq = radius * radius;

            // Check for degenerate ray
            if (a <= EpsilonSq)
            {
                // Ray is degenerate, treat as a point
                tMin = tMax = 0.0f;
                float distSq = DistanceSquaredPointSegment(rayOrigin, segA, segB, out _, out _);
                return distSq <= radiusSq;
            }

            Vector2 segDir = segB - segA;

            // Check for degenerate capsule
            if (segDir.LengthSquared() <= EpsilonSq)
            {
                // Capsule is degenerate, treat as a circle
                return RayCircleIntersectionInterval(rayOrigin, rayDirection, segA, radius, out tMin, out tMax);
            }

            // Check if parallel or colinear with capsule medial line segment
            float cross = Vector2.PerpDot(rayDirection, segDir);
            if (MathF.Abs(cross) <= Epsilon)
            {
                // Parallel/colinear case: build interval by projecting segment endpoints
                float invA = 1.0f / a;

                Vector2 toA = segA - rayOrigin;
                float tA = Vector2.Dot(toA, rayDirection) * invA;
                float tB = Vector2.Dot(segB - rayOrigin, rayDirection) * invA;

                // Perpendicular distance from segA to ray line
                Vector2 perp = toA - (Vector2.Dot(toA, rayDirection) * invA) * rayDirection;
                float perpDistSq = Vector2.Dot(perp, perp);

                if (perpDistSq > radiusSq)
                {
                    tMin = tMax = default;
                    return false;
                }

                float minProj = MathF.Min(tA, tB);
                float maxProj = MathF.Max(tA, tB);

                // Expand interval by how far we can move along the ray while staying within radius
                // For non-unit direction: delta = sqrt((R^2 - perp^2) / Dot(d,d))
                float delta = MathF.Sqrt((radiusSq - perpDistSq) * invA);

                tMin = minProj - delta;
                tMax = maxProj + delta;

                // Clamp tMin to 0 for ray semantics
                if (tMin < 0.0f)
                    tMin = 0.0f;
                return true;
            }

            // General case: closest approach to capsule medial line segment
            ClosestPointRaySegment(rayOrigin, rayDirection, segA, segB, out float sRay, out _, out float distSqToAxis);

            if (distSqToAxis > radiusSq)
            {
                tMin = tMax = default;
                return false;
            }

            // Convert "radial slack" into parameter offset along the ray
            float offset = MathF.Sqrt((radiusSq - distSqToAxis) / a);

            tMin = sRay - offset;
            tMax = sRay + offset;

            if (tMin > tMax)
            {
                (tMin, tMax) = (tMax, tMin);
            }

            // Clamp tMin to 0 for ray semantics
            if (tMin < 0.0f)
                tMin = 0.0f;

            return true;
        }

        #endregion

        #region Intersections

        /// <summary>
        /// Determines whether two axis-aligned bounding boxes (AABBs) intersect.
        /// </summary>
        /// <param name="aMin">The minimum corner of the first AABB.</param>
        /// <param name="aMax">The maximum corner of the first AABB.</param>
        /// <param name="bMin">The minimum corner of the second AABB.</param>
        /// <param name="bMax">The maximum corner of the second AABB.</param>
        /// <returns>
        /// <see langword="true"/> if the AABBs overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IntersectsAabbAabb(Vector2 aMin, Vector2 aMax, Vector2 bMin, Vector2 bMax)
        {
            // Exit with no intersection if separated along any axis
            if (aMax.X < bMin.X || aMin.X > bMax.X) return false;
            if (aMax.Y < bMin.Y || aMin.Y > bMax.Y) return false;

            return true;
        }

        /// <summary>
        /// Determines whether an axis-aligned bounding box (AABB) intersects a capsule.
        /// </summary>
        /// <param name="boxMin">The minimum corner of the AABB.</param>
        /// <param name="boxMax">The maximum corner of the AABB.</param>
        /// <param name="capsuleA">The first endpoint of the capsule line segment.</param>
        /// <param name="capsuleB">The second endpoint of the capsule line segment.</param>
        /// <param name="capsuleRadius">The capsule radius.</param>
        /// <returns>
        /// <see langword="true"/> if the capsule overlaps or touches the AABB; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The capsule intersects the AABB when the squared distance between the capsule segment <c>[capsuleA, capsuleB]</c>
        /// and the AABB is less than or equal to <c>capsuleRadius^2</c>.
        /// </remarks>
        public static bool IntersectsAabbCapsule(Vector2 boxMin, Vector2 boxMax, Vector2 capsuleA, Vector2 capsuleB, float capsuleRadius)
        {
            float radiusSq = capsuleRadius * capsuleRadius;
            float distSq = DistanceSquaredSegmentAabb(capsuleA, capsuleB, boxMin, boxMax);
            return distSq <= radiusSq;
        }

        /// <summary>
        /// Determines whether an axis-aligned bounding box (AABB) intersects a convex polygon.
        /// </summary>
        /// <param name="aabbCenter">The center of the AABB.</param>
        /// <param name="aabbHalfExtents">The half-widths of the AABB along the world X and Y axes.</param>
        /// <param name="pVertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="pNormals">
        /// The per-edge outward unit normals. The array length must match <paramref name="pVertices"/> so that
        /// <c>pNormals[i]</c> corresponds to the edge from <c>pVertices[i]</c> to <c>pVertices[(i + 1) % pVertices.Length]</c>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the shapes overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Separating-axis test (SAT) is performed using all polygon edge normals and the two AABB axes. If the projections are
        /// disjoint on any tested axis, the shapes do not intersect.
        /// </remarks>
        public static bool IntersectsAabbConvexPolygon(Vector2 aabbCenter, Vector2 aabbHalfExtents, Vector2[] pVertices, Vector2[] pNormals)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.1 "Separating-axis Test" (2D adaptation)

            if (!IsValidPolygon(pVertices, pNormals))
                return false;

            // Test polygon edge normals
            for (int i = 0; i < pNormals.Length; i++)
            {
                if (!OverlapOnAxis(aabbCenter, aabbHalfExtents, pVertices, pNormals[i]))
                    return false;
            }

            // Test AABB axes
            if (!OverlapOnAxis(aabbCenter, aabbHalfExtents, pVertices, Vector2.UnitX))
                return false;

            if (!OverlapOnAxis(aabbCenter, aabbHalfExtents, pVertices, Vector2.UnitY))
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether an axis-aligned bounding box (AABB) intersects an oriented bounding box (OBB).
        /// </summary>
        /// <param name="aabbCenter">The center of the AABB.</param>
        /// <param name="aabbHalfExtents">The half-widths of the AABB along the world X and Y axes.</param>
        /// <param name="obbCenter">The center of the OBB.</param>
        /// <param name="obbAxisX">The OBB local X axis direction.</param>
        /// <param name="obbAxisY">The OBB local Y axis direction.</param>
        /// <param name="obbHalfExtents">The half-widths of the OBB along <paramref name="obbAxisX"/> and <paramref name="obbAxisY"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the boxes overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Separating-axis test (SAT) is performed using the two AABB axes and the two OBB axes. If the projections are
        /// disjoint on any tested axis, the boxes do not intersect.
        /// </remarks>
        public static bool IntersectsAabbObb(Vector2 aabbCenter, Vector2 aabbHalfExtents, Vector2 obbCenter, Vector2 obbAxisX, Vector2 obbAxisY, Vector2 obbHalfExtents)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.1 "Separating-axis Test" (2D adaptation)

            // AABB axis x
            ProjectAabbOntoAxis(aabbCenter, aabbHalfExtents, Vector2.UnitX, out float minA, out float maxA);
            ProjectObbOntoAxis(obbCenter, obbAxisX, obbAxisY, obbHalfExtents, Vector2.UnitX, out float minB, out float maxB);
            if (!IntervalsOverlap(minA, maxA, minB, maxB)) return false;

            // AABB axis y
            ProjectAabbOntoAxis(aabbCenter, aabbHalfExtents, Vector2.UnitY, out minA, out maxA);
            ProjectObbOntoAxis(obbCenter, obbAxisX, obbAxisY, obbHalfExtents, Vector2.UnitY, out minB, out maxB);
            if (!IntervalsOverlap(minA, maxA, minB, maxB)) return false;

            // OBB axis X
            ProjectAabbOntoAxis(aabbCenter, aabbHalfExtents, obbAxisX, out minA, out maxA);
            ProjectObbOntoAxis(obbCenter, obbAxisX, obbAxisY, obbHalfExtents, obbAxisX, out minB, out maxB);
            if (!IntervalsOverlap(minA, maxA, minB, maxB)) return false;

            // OBB axis Y
            ProjectAabbOntoAxis(aabbCenter, aabbHalfExtents, obbAxisY, out minA, out maxA);
            ProjectObbOntoAxis(obbCenter, obbAxisX, obbAxisY, obbHalfExtents, obbAxisY, out minB, out maxB);
            if (!IntervalsOverlap(minA, maxA, minB, maxB)) return false;

            return true;
        }

        /// <summary>
        /// Determines whether two oriented bounding boxes (OBBs) intersect.
        /// </summary>
        /// <param name="aCenter">The center of the first OBB.</param>
        /// <param name="aAxisX">The first OBB local X axis direction.</param>
        /// <param name="aAxisY">The first OBB local Y axis direction.</param>
        /// <param name="aHalf">The half-widths of the first OBB along <paramref name="aAxisX"/> and <paramref name="aAxisY"/>.</param>
        /// <param name="bCenter">The center of the second OBB.</param>
        /// <param name="bAxisX">The second OBB local X axis direction.</param>
        /// <param name="bAxisY">The second OBB local Y axis direction.</param>
        /// <param name="bHalf">The half-widths of the second OBB along <paramref name="bAxisX"/> and <paramref name="bAxisY"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the boxes overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Separating-axis test (SAT) is performed using the two axes from each box. If the projections are disjoint on any
        /// tested axis, the boxes are disjoint.
        /// </remarks>
        public static bool IntersectsObbObb(Vector2 aCenter, Vector2 aAxisX, Vector2 aAxisY, Vector2 aHalf, Vector2 bCenter, Vector2 bAxisX, Vector2 bAxisY, Vector2 bHalf)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.1 "Separating-axis Test" (2D adaptation)

            // A axis X
            ProjectObbOntoAxis(aCenter, aAxisX, aAxisY, aHalf, aAxisX, out float minA, out float maxA);
            ProjectObbOntoAxis(bCenter, bAxisX, bAxisY, bHalf, aAxisX, out float minB, out float maxB);
            if (!IntervalsOverlap(minA, maxA, minB, maxB)) return false;

            // A axis Y
            ProjectObbOntoAxis(aCenter, aAxisX, aAxisY, aHalf, aAxisY, out minA, out maxA);
            ProjectObbOntoAxis(bCenter, bAxisX, bAxisY, bHalf, aAxisY, out minB, out maxB);
            if (!IntervalsOverlap(minA, maxA, minB, maxB)) return false;

            // B axis X
            ProjectObbOntoAxis(aCenter, aAxisX, aAxisY, aHalf, bAxisX, out minA, out maxA);
            ProjectObbOntoAxis(bCenter, bAxisX, bAxisY, bHalf, bAxisX, out minB, out maxB);
            if (!IntervalsOverlap(minA, maxA, minB, maxB)) return false;

            // B axis Y
            ProjectObbOntoAxis(aCenter, aAxisX, aAxisY, aHalf, bAxisY, out minA, out maxA);
            ProjectObbOntoAxis(bCenter, bAxisX, bAxisY, bHalf, bAxisY, out minB, out maxB);
            if (!IntervalsOverlap(minA, maxA, minB, maxB)) return false;

            return true;
        }

        /// <summary>
        /// Determines whether an oriented bounding box (OBB) intersects a capsule.
        /// </summary>
        /// <param name="obbCenter">The center of the OBB.</param>
        /// <param name="obbAxisX">The OBB local X axis direction.</param>
        /// <param name="obbAxisY">The OBB local Y axis direction.</param>
        /// <param name="obbHalfExtents">The half-widths of the OBB along <paramref name="obbAxisX"/> and <paramref name="obbAxisY"/>.</param>
        /// <param name="capsuleA">The first endpoint of the capsule line segment.</param>
        /// <param name="capsuleB">The second endpoint of the capsule line segment.</param>
        /// <param name="capsuleRadius">The capsule radius.</param>
        /// <returns>
        /// <see langword="true"/> if the capsule overlaps or touches the OBB; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The capsule segment endpoints are transformed into the OBB's local space so that the OBB becomes an axis-aligned
        /// box with extents <c>[-halfExtents, +halfExtents]</c>. The capsule intersects the OBB when the squared distance
        /// between the transformed segment and that box is less than or equal to <c>capsuleRadius^2</c>.
        /// </remarks>
        public static bool IntersectsObbCapsule(Vector2 obbCenter, Vector2 obbAxisX, Vector2 obbAxisY, Vector2 obbHalfExtents, Vector2 capsuleA, Vector2 capsuleB, float capsuleRadius)
        {
            float radiusSq = capsuleRadius * capsuleRadius;

            // Transform capsule segment into OBB local space (OBB becomes AABB)
            Vector2 a = capsuleA - obbCenter;
            Vector2 b = capsuleB - obbCenter;

            Vector2 localA = new Vector2(Vector2.Dot(a, obbAxisX), Vector2.Dot(a, obbAxisY));
            Vector2 localB = new Vector2(Vector2.Dot(b, obbAxisX), Vector2.Dot(b, obbAxisY));

            Vector2 min = -obbHalfExtents;
            Vector2 max = obbHalfExtents;

            float distSq = DistanceSquaredSegmentAabb(localA, localB, min, max);
            return distSq <= radiusSq;
        }

        /// <summary>
        /// Determines whether an oriented bounding box (OBB) intersects a convex polygon.
        /// </summary>
        /// <param name="obbCenter">The center of the OBB.</param>
        /// <param name="obbAxisX">The OBB local X axis direction.</param>
        /// <param name="obbAxisY">The OBB local Y axis direction.</param>
        /// <param name="obbHalfExtents">The half-widths of the OBB along <paramref name="obbAxisX"/> and <paramref name="obbAxisY"/>.</param>
        /// <param name="pVertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="pNormals">
        /// The per-edge outward unit normals. The array length must match <paramref name="pVertices"/> so that
        /// <c>pNormals[i]</c> corresponds to the edge from <c>pVertices[i]</c> to <c>pVertices[(i + 1) % pVertices.Length]</c>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the shapes overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Separating-axis test (SAT) is performed using all polygon edge normals and the two OBB axes. If the projections are
        /// disjoint on any tested axis, the shapes are disjoint.
        /// </remarks>
        public static bool IntersectsObbConvexPolygon(Vector2 obbCenter, Vector2 obbAxisX, Vector2 obbAxisY, Vector2 obbHalfExtents, Vector2[] pVertices, Vector2[] pNormals)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.1 "Separating-axis Test" (2D adaptation)

            if (!IsValidPolygon(pVertices, pNormals))
                return false;

            float minP, maxP, minB, maxB;

            // Test polygon edge normals
            for (int i = 0; i < pVertices.Length; i++)
            {
                Vector2 axis = pNormals[i];

                ProjectOntoAxis(pVertices, axis, out minP, out maxP);
                ProjectObbOntoAxis(obbCenter, obbAxisX, obbAxisY, obbHalfExtents, axis, out minB, out maxB);
                if (!IntervalsOverlap(minP, maxP, minB, maxB)) return false;
            }

            // OBB X Axis
            ProjectOntoAxis(pVertices, obbAxisX, out minP, out maxP);
            ProjectObbOntoAxis(obbCenter, obbAxisX, obbAxisY, obbHalfExtents, obbAxisX, out minB, out maxB);
            if (!IntervalsOverlap(minP, maxP, minB, maxB)) return false;

            // Obb Y Axis
            ProjectOntoAxis(pVertices, obbAxisY, out minP, out maxP);
            ProjectObbOntoAxis(obbCenter, obbAxisX, obbAxisY, obbHalfExtents, obbAxisY, out minB, out maxB);
            if (!IntervalsOverlap(minP, maxP, minB, maxB)) return false;

            return true;
        }

        /// <summary>
        /// Determines whether a capsule intersects a convex polygon.
        /// </summary>
        /// <param name="capsuleA">The first endpoint of the capsule line segment.</param>
        /// <param name="capsuleB">The second endpoint of the capsule line segment.</param>
        /// <param name="capsuleRadius">The capsule radius.</param>
        /// <param name="pVertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="pNormals">
        /// The per-edge outward unit normals. The array length must match <paramref name="pVertices"/> so that
        /// <c>pNormals[i]</c> corresponds to the edge from <c>pVertices[i]</c> to <c>pVertices[(i + 1) % pVertices.Length]</c>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the capsule overlaps or touches the polygon; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The capsule intersects the polygon when the squared distance between the capsule segment <c>[capsuleA, capsuleB]</c>
        /// and the polygon is less than or equal to <c>capsuleRadius^2</c>.
        /// </remarks>
        public static bool IntersectsCapsuleConvexPolygon(Vector2 capsuleA, Vector2 capsuleB, float capsuleRadius, Vector2[] pVertices, Vector2[] pNormals)
        {
            float radiusSq = capsuleRadius * capsuleRadius;
            float d2 = DistanceSquaredSegmentConvexPolygon(capsuleA, capsuleB, pVertices, pNormals);
            return d2 <= radiusSq;
        }

        /// <summary>
        /// Determines whether two circles intersect.
        /// </summary>
        /// <param name="aCenter">The center of the first circle.</param>
        /// <param name="aRadius">The radius of the first circle.</param>
        /// <param name="bCenter">The center of the second circle.</param>
        /// <param name="bRadius">The radius of the second circle.</param>
        /// <returns>
        /// <see langword="true"/> if the circles overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IntersectsCircleCircle(Vector2 aCenter, float aRadius, Vector2 bCenter, float bRadius)
        {
            float r = aRadius + bRadius;
            float sqDist = Vector2.DistanceSquared(aCenter, bCenter);
            return sqDist <= r * r;
        }

        /// <summary>
        /// Determines whether a circle intersects an axis-aligned bounding box (AABB).
        /// </summary>
        /// <param name="cCenter">The center of the circle.</param>
        /// <param name="cRadius">The radius of the circle.</param>
        /// <param name="boxMin">The minimum corner of the AABB.</param>
        /// <param name="boxMax">The maximum corner of the AABB.</param>
        /// <returns>
        /// <see langword="true"/> if the circle overlaps or touches the AABB; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The circle intersects the AABB when the squared distance from the circle center to the box is less than or equal to
        /// <c>cRadius^2</c>.
        /// </remarks>
        public static bool IntersectsCircleAabb(Vector2 cCenter, float cRadius, Vector2 boxMin, Vector2 boxMax)
        {
            float d2 = DistanceSquaredPointAabb(cCenter, boxMin, boxMax);
            return d2 <= cRadius * cRadius;
        }

        /// <summary>
        /// Determines whether a circle intersects an oriented bounding box (OBB).
        /// </summary>
        /// <param name="cCenter">The center of the circle.</param>
        /// <param name="cRadius">The radius of the circle.</param>
        /// <param name="obbCenter">The center of the OBB.</param>
        /// <param name="obbAxisX">The OBB local X axis direction.</param>
        /// <param name="obbAxisY">The OBB local Y axis direction.</param>
        /// <param name="obbHalfExtents">The half-widths of the OBB along <paramref name="obbAxisX"/> and <paramref name="obbAxisY"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the circle overlaps or touches the OBB; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The circle intersects the OBB when the squared distance from the circle center to the box is less than or equal to
        /// <c>cRadius^2</c>.
        /// </remarks>
        public static bool IntersectsCircleObb(Vector2 cCenter, float cRadius, Vector2 obbCenter, Vector2 obbAxisX, Vector2 obbAxisY, Vector2 obbHalfExtents)
        {
            float d2 = DistanceSquaredPointObb(cCenter, obbCenter, obbAxisX, obbAxisY, obbHalfExtents);
            return d2 <= cRadius * cRadius;
        }

        /// <summary>
        /// Determines whether a circle intersects a convex polygon.
        /// </summary>
        /// <param name="cCenter">The center of the circle.</param>
        /// <param name="cRadius">The radius of the circle.</param>
        /// <param name="pVertices">The polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="pNormals">
        /// The per-edge outward unit normals. The array length must match <paramref name="pVertices"/> so that
        /// <c>pNormals[i]</c> corresponds to the edge from <c>pVertices[i]</c> to <c>pVertices[(i + 1) % pVertices.Length]</c>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the circle overlaps or touches the polygon; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The circle intersects the polygon when the squared distance from the circle center to the polygon is less than or
        /// equal to <c>cRadius^2</c>.
        /// </remarks>
        public static bool IntersectsCircleConvexPolygon(Vector2 cCenter, float cRadius, Vector2[] pVertices, Vector2[] pNormals)
        {
            if (!IsValidPolygon(pVertices, pNormals))
                return false;

            float d2 = DistanceSquaredPointConvexPolygon(cCenter, pVertices, pNormals);
            return d2 <= cRadius * cRadius;
        }

        /// <summary>
        /// Determines whether a circle intersects a capsule.
        /// </summary>
        /// <param name="circleCenter">The center of the circle.</param>
        /// <param name="circleRadius">The radius of the circle.</param>
        /// <param name="capsuleA">The first endpoint of the capsule line segment.</param>
        /// <param name="capsuleB">The second endpoint of the capsule line segment.</param>
        /// <param name="capsuleRadius">The capsule radius.</param>
        /// <returns>
        /// <see langword="true"/> if the circle overlaps or touches the capsule; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The circle intersects the capsule when the squared distance from the circle center to the capsule segment
        /// <c>[capsuleA, capsuleB]</c> is less than or equal to <c>(circleRadius + capsuleRadius)^2</c>.
        /// </remarks>
        public static bool IntersectsCircleCapsule(Vector2 circleCenter, float circleRadius, Vector2 capsuleA, Vector2 capsuleB, float capsuleRadius)
        {
            float r = circleRadius + capsuleRadius;
            float d2 = DistanceSquaredPointSegment(circleCenter, capsuleA, capsuleB, out _, out _);
            return d2 <= r * r;
        }

        /// <summary>
        /// Determines whether two capsules intersect.
        /// </summary>
        /// <param name="a0">The first endpoint of the first capsule line segment.</param>
        /// <param name="a1">The second endpoint of the first capsule line segment.</param>
        /// <param name="aRadius">The radius of the first capsule.</param>
        /// <param name="b0">The first endpoint of the second capsule line segment.</param>
        /// <param name="b1">The second endpoint of the second capsule line segment.</param>
        /// <param name="bRadius">The radius of the second capsule.</param>
        /// <returns>
        /// <see langword="true"/> if the capsules overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Two capsules intersect when the squared distance between their medial segments is less than or equal to
        /// <c>(aRadius + bRadius)^2</c>.
        /// </remarks>
        public static bool IntersectsCapsuleCapsule(Vector2 a0, Vector2 a1, float aRadius, Vector2 b0, Vector2 b1, float bRadius)
        {
            float r = aRadius + bRadius;
            float rr = r * r;

            float d2 = DistanceSquaredSegmentSegment(a0, a1, b0, b1, out _, out _, out _, out _);
            return d2 <= rr;
        }

        /// <summary>
        /// Determines whether two convex polygons intersect.
        /// </summary>
        /// <param name="aVertices">The first polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="aNormals">
        /// The per-edge outward unit normals for the first polygon. The array length must match <paramref name="aVertices"/> so that
        /// <c>aNormals[i]</c> corresponds to the edge from <c>aVertices[i]</c> to <c>aVertices[(i + 1) % aVertices.Length]</c>.
        /// </param>
        /// <param name="bVertices">The second polygon vertices in winding order. A valid polygon requires at least three vertices.</param>
        /// <param name="bNormals">
        /// The per-edge outward unit normals for the second polygon. The array length must match <paramref name="bVertices"/> so that
        /// <c>bNormals[i]</c> corresponds to the edge from <c>bVertices[i]</c> to <c>bVertices[(i + 1) % bVertices.Length]</c>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the polygons overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Separating-axis test (SAT) is performed using the edge normals of both polygons. If the projections are disjoint on
        /// any tested axis, the polygons are disjoint.
        /// </remarks>
        public static bool IntersectsConvexPolygonConvexPolygon(Vector2[] aVertices, Vector2[] aNormals, Vector2[] bVertices, Vector2[] bNormals)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.1 "Separating-axis Test" (2D adaptation)

            if (!IsValidPolygon(aVertices, aNormals) || !IsValidPolygon(bVertices, bNormals))
                return false;

            // Test A's axis
            for (int i = 0; i < aVertices.Length; i++)
            {
                if (!OverlapOnAxis(aVertices, bVertices, aNormals[i]))
                    return false;
            }

            // Test B's axis
            for (int i = 0; i < bVertices.Length; i++)
            {
                if (!OverlapOnAxis(aVertices, bVertices, bNormals[i]))
                    return false;
            }

            return true;
        }

        #endregion
    }
}
