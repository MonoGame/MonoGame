// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Represents a capsule bounding volume in 2D space, formed by sweeping a circle along a line segment.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundingCapsule2D : IEquatable<BoundingCapsule2D>
    {
        #region Public Fields

        /// <summary>
        /// The first endpoint of the capsule's central line segment in 2D space.
        /// </summary>
        [DataMember]
        public Vector2 PointA;

        /// <summary>
        /// The second endpoint of the capsule's central line segment in 2D space.
        /// </summary>
        [DataMember]
        public Vector2 PointB;

        /// <summary>
        /// The radius of this capsule, defining the circular caps and the width of the swept region.
        /// </summary>
        [DataMember]
        public float Radius;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the center of this capsule in 2D space, located at the midpoint between the two endpoints.
        /// </summary>
        public readonly Vector2 Center => (PointA + PointB) * 0.5f;

        /// <summary>
        /// Gets the length of this capsule's central line segment.
        /// </summary>
        public readonly float Length
        {
            get
            {
                return MathF.Sqrt(LengthSquared);
            }
        }

        /// <summary>
        /// Gets the squared length of this capsule's central line segment, useful for distance comparisons without square root calculations.
        /// </summary>
        public readonly float LengthSquared
        {
            get
            {
                Vector2 diff = PointB - PointA;
                return diff.X * diff.X + diff.Y * diff.Y;
            }
        }

        /// <summary>
        /// Gets the unit direction vector from PointA toward PointB, defining this capsule's orientation.
        /// </summary>
        /// <remarks>
        /// Returns <see cref="Vector2.Zero"/> when the endpoints are identical (degenerate capsule).
        /// </remarks>
        public readonly Vector2 Direction
        {
            get
            {
                Vector2 dir = PointB - PointA;
                float lengthSquared = dir.X * dir.X + dir.Y * dir.Y;

                if (lengthSquared < Collision2D.Epsilon * Collision2D.Epsilon)
                {
                    return Vector2.Zero;
                }

                return dir / MathF.Sqrt(lengthSquared);
            }
        }

        /// <summary>
        /// Gets the total area enclosed by this capsule.
        /// </summary>
        /// <remarks>
        /// Calculated as the area of the central rectangle plus the area of the two semicircular caps,
        /// which together form a complete circle.
        /// </remarks>
        public readonly float Area
        {
            get
            {
                float length = Length;
                return length * (2.0f * Radius) + MathF.PI * Radius * Radius;
            }
        }

        #endregion

        #region Internal Properties

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    "PointA( ", PointA.ToString(), " )  \r\n",
                    "PointB( ", PointB.ToString(), " )  \r\n",
                    "Radius( ", Radius.ToString(), " )"
                );
            }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a new <see cref="BoundingCapsule2D"/> with the specified endpoints and radius.
        /// </summary>
        /// <param name="pointA">The first endpoint of the capsule's central line segment in 2D space.</param>
        /// <param name="pointB">The second endpoint of the capsule's central line segment in 2D space.</param>
        /// <param name="radius">The radius of the capsule. Should be non-negative.</param>
        public BoundingCapsule2D(Vector2 pointA, Vector2 pointB, float radius)
        {
            PointA = pointA;
            PointB = pointB;
            Radius = radius;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new <see cref="BoundingCapsule2D"/> from a center point, direction, length, and radius.
        /// </summary>
        /// <param name="center">The center point of the capsule in 2D space.</param>
        /// <param name="direction">
        /// The direction vector defining the capsule's orientation. Will be normalized automatically.
        /// </param>
        /// <param name="length">The length of the capsule's central line segment.</param>
        /// <param name="radius">The radius of the capsule.</param>
        /// <returns>
        /// A new <see cref="BoundingCapsule2D"/> centered at the specified point and oriented along the given direction.
        /// </returns>
        public static BoundingCapsule2D CreateFromCenterAndDirection(Vector2 center, Vector2 direction, float length, float radius)
        {
            // Check if the direction needs to be normalized and normalize it.
            float lengthSq = direction.LengthSquared();
            Vector2 normalizedDir = Vector2.Zero;
            if (lengthSq >= Collision2D.Epsilon * Collision2D.Epsilon)
            {
                normalizedDir = direction / MathF.Sqrt(lengthSq);
            }

            Vector2 halfExtent = normalizedDir * (length * 0.5f);

            return new BoundingCapsule2D(
                center - halfExtent,
                center + halfExtent,
                radius
            );
        }

        /// <summary>
        /// Creates a new <see cref="BoundingCapsule2D"/> from a line segment with the specified radius.
        /// </summary>
        /// <param name="segment">The line segment defining the capsule's central axis.</param>
        /// <param name="radius">The radius of the capsule.</param>
        /// <returns>
        /// A new <see cref="BoundingCapsule2D"/> following the line segment with circular caps at each end.
        /// </returns>
        public static BoundingCapsule2D CreateFromSegment(LineSegment2D segment, float radius)
        {
            return new BoundingCapsule2D(segment.Start, segment.End, radius);
        }

        /// <summary>
        /// Creates a <see cref="BoundingCapsule2D"/> that encloses two capsules.
        /// </summary>
        /// <param name="original">The first capsule to enclose.</param>
        /// <param name="additional">The second capsule to enclose.</param>
        /// <returns>
        /// A <see cref="BoundingCapsule2D"/> that completely contains both input capsules.
        /// </returns>
        /// <remarks>
        /// The merged capsule is constructed by finding the two most distant endpoints among both input capsules
        /// to form the new central segment, then computing the radius needed to enclose both original capsules.
        /// </remarks>
        public static BoundingCapsule2D CreateMerged(BoundingCapsule2D original, BoundingCapsule2D additional)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 4.5 "Sphere-Swept Volumes" and Section 6.5.2 "Merging Two Spheres"
            // Derived capsule merge by expanding a medial segment to enclose both capsules

            Vector2[] points = new Vector2[4]
            {
                original.PointA,
                original.PointB,
                additional.PointA,
                additional.PointB
            };

            // Find the most distant points to form the new medial segment
            float maxDistSq = 0.0f;
            int pointIndex1 = 0;
            int pointIndex2 = 1;

            for (int i = 0; i < points.Length; i++)
            {
                for (int j = i + 1; j < points.Length; j++)
                {
                    float distSq = Vector2.DistanceSquared(points[i], points[j]);

                    if (distSq > maxDistSq)
                    {
                        maxDistSq = distSq;
                        pointIndex1 = i;
                        pointIndex2 = j;
                    }
                }
            }

            Vector2 newPointA = points[pointIndex1];
            Vector2 newPointB = points[pointIndex2];

            // Compute the maximum radius needed to contain both capsules
            // For each endpoint of the original capsules that isn't part of the new medial segment,
            // compute the perpendicular distance to the new medial segment and add the capsule radius
            float newRadius = MathF.Max(original.Radius, additional.Radius);
            LineSegment2D newSegment = new LineSegment2D(newPointA, newPointB);

            float[] radii = new float[]
            {
                original.Radius,
                original.Radius,
                additional.Radius,
                additional.Radius
            };

            for (int i = 0; i < points.Length; i++)
            {
                float distToSegment = newSegment.DistanceToPoint(points[i]);
                float requiredRadius = distToSegment + radii[i];
                newRadius = MathF.Max(newRadius, requiredRadius);
            }

            return new BoundingCapsule2D(newPointA, newPointB, newRadius);
        }

        /// <summary>
        /// Tests whether a point lies inside this capsule or on its boundary.
        /// </summary>
        /// <param name="point">The point to test in 2D space.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the point is inside or on the boundary;
        /// otherwise, <see cref="ContainmentType.Disjoint"/> if the point is outside.
        /// </returns>
        public readonly ContainmentType Contains(Vector2 point)
        {
            float rr = Radius * Radius;
            float d2 = Collision2D.DistanceSquaredPointSegment(point, PointA, PointB, out _, out _);

            if (d2 <= rr)
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether this capsule contains, intersects, or is separate from an axis-aligned bounding box.
        /// </summary>
        /// <param name="aabb">The circle to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the axis-aligned bounding box is completely inside this capsule;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingBox2D aabb)
        {
            return Collision2D.ContainsCapsuleAabb(PointA, PointB, Radius, aabb.Min, aabb.Max);
        }

        /// <summary>
        /// Tests whether this capsule contains, intersects, or is separate from a circle.
        /// </summary>
        /// <param name="circle">The circle to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the circle is completely inside this capsule;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingCircle circle)
        {
            return Collision2D.ContainsCapsuleCircle(PointA, PointB, Radius, circle.Center, circle.Radius);
        }

        /// <summary>
        /// Tests whether this capsule contains, intersects, or is separate from an oriented bounding box.
        /// </summary>
        /// <param name="obb">The oriented bounding box to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the oriented bounding box is completely inside this capsule;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(OrientedBoundingBox2D obb)
        {
            return Collision2D.ContainsCapsuleObb(PointA, PointB, Radius, obb.Center, obb.AxisX, obb.AxisY, obb.HalfExtents);
        }

        /// <summary>
        /// Tests whether this capsule contains, intersects, or is separate from another capsule.
        /// </summary>
        /// <param name="other">The other capsule to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the other capsule is completely inside this one;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingCapsule2D other)
        {
            return Collision2D.ContainsCapsuleCapsule(PointA, PointB, Radius, other.PointA, other.PointB, other.Radius);
        }

        /// <summary>
        /// Tests whether this capsule contains, intersects, or is separate from a polygon.
        /// </summary>
        /// <param name="polygon">The circle to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the polygon is completely inside this capsule;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingPolygon2D polygon)
        {
            return Collision2D.ContainsCapsuleConvexPolygon(PointA, PointB, Radius, polygon.Vertices, polygon.Normals);
        }

        /// <summary>
        /// Tests whether this capsule intersects with another capsule.
        /// </summary>
        /// <param name="other">The other capsule to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the capsules overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCapsule2D other)
        {
            return Collision2D.IntersectsCapsuleCapsule(PointA, PointB, Radius, other.PointA, other.PointB, other.Radius);
        }

        /// <summary>
        /// Tests whether this capsule intersects with a circle.
        /// </summary>
        /// <param name="circle">The circle to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the capsule and circle overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCircle circle)
        {
            return Collision2D.IntersectsCircleCapsule(circle.Center, circle.Radius, PointA, PointB, Radius);
        }

        /// <summary>
        /// Tests whether this capsule intersects with an axis-aligned bounding box.
        /// </summary>
        /// <param name="box">The bounding box to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the capsule and bounding box overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingBox2D box)
        {
            return Collision2D.IntersectsAabbCapsule(box.Min, box.Max, PointA, PointB, Radius);
        }

        /// <summary>
        /// Tests whether this capsule intersects with an oriented bounding box.
        /// </summary>
        /// <param name="obb">The oriented bounding box to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the capsule and box overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(OrientedBoundingBox2D obb)
        {
            return Collision2D.IntersectsObbCapsule(obb.Center, obb.AxisX, obb.AxisY, obb.HalfExtents, PointA, PointB, Radius);
        }

        /// <summary>
        /// Tests whether this capsule intersects with a polygon.
        /// </summary>
        /// <param name="polygon">The polygon to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the capsule and polygon overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingPolygon2D polygon)
        {
            return Collision2D.IntersectsCapsuleConvexPolygon(PointA, PointB, Radius, polygon.Vertices, polygon.Normals);
        }

        /// <summary>
        /// Applies a matrix transformation to this capsule and creates a new transformed capsule.
        /// </summary>
        /// <param name="matrix">The transformation matrix to apply.</param>
        /// <returns>
        /// A new <see cref="BoundingCapsule2D"/> with endpoints transformed by the matrix and the radius
        /// scaled by the maximum scale factor to ensure the transformed shape remains enclosed.
        /// </returns>
        /// <remarks>
        /// The radius is scaled by the maximum of the X and Y scale components of the transformation matrix.
        /// This ensures that the resulting capsule fully encloses the transformed original capsule, even if
        /// the transformation includes non-uniform scaling or rotation.
        /// </remarks>
        public readonly BoundingCapsule2D Transform(Matrix matrix)
        {
            Vector2 transformedA = Vector2.Transform(PointA, matrix);
            Vector2 transformedB = Vector2.Transform(PointB, matrix);

            // Scale radius by maximum scale component
            float scaleX = MathF.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
            float scaleY = MathF.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22);
            float transformedRadius = Radius * MathF.Max(scaleX, scaleY);

            return new BoundingCapsule2D(transformedA, transformedB, transformedRadius);
        }

        /// <summary>
        /// Creates a new <see cref="BoundingCapsule2D"/> by translating this capsule by the specified offset.
        /// </summary>
        /// <param name="translation">The offset to translate the capsule by in 2D space.</param>
        /// <returns>
        /// A new <see cref="BoundingCapsule2D"/> at the translated position with the same radius and orientation.
        /// </returns>
        public readonly BoundingCapsule2D Translate(Vector2 translation)
        {
            return new BoundingCapsule2D(
                PointA + translation,
                PointB + translation,
                Radius
            );
        }

        /// <summary>
        /// Deconstructs this capsule into its component values.
        /// </summary>
        /// <param name="pointA">
        /// When this method returns, contains the first endpoint of this capsule's central line segment in 2D space.
        /// </param>
        /// <param name="pointB">
        /// When this method returns, contains the second endpoint of this capsule's central line segment in 2D space.
        /// </param>
        /// <param name="radius">
        /// When this method returns, contains the radius of this capsule.
        /// </param>
        public readonly void Deconstruct(out Vector2 pointA, out Vector2 pointB, out float radius)
        {
            pointA = PointA;
            pointB = PointB;
            radius = Radius;
        }

        /// <inheritdoc/>
        public readonly bool Equals(BoundingCapsule2D other)
        {
            return PointA.Equals(other.PointA)
                   && PointB.Equals(other.PointB)
                   && Radius.Equals(other.Radius);
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            return obj is BoundingCapsule2D other && Equals(other);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return PointA.GetHashCode() ^
                   PointB.GetHashCode() ^
                   Radius.GetHashCode();
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{{PointA:{PointA} PointB:{PointB} Radius:{Radius:F2}}}";
        }

        /// <summary/>
        public static bool operator ==(BoundingCapsule2D left, BoundingCapsule2D right)
        {
            return left.Equals(right);
        }

        /// <summary/>
        public static bool operator !=(BoundingCapsule2D left, BoundingCapsule2D right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}
