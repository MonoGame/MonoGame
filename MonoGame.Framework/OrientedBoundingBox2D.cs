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
    /// Represents an oriented bounding box in 2D space
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    [StructLayout(LayoutKind.Sequential)]
    public struct OrientedBoundingBox2D : IEquatable<OrientedBoundingBox2D>
    {
        /// <summary>
        /// The number of corners in this oriented bounding box.
        /// </summary>
        public const int CornerCount = 4;

        #region Public Fields

        /// <summary>
        /// The center position of this oriented bounding box in 2D space.
        /// </summary>
        [DataMember]
        public Vector2 Center;

        /// <summary>
        /// The unit vector defining this box's local X-axis direction.
        /// </summary>
        [DataMember]
        public Vector2 AxisX;

        /// <summary>
        /// The unit vector defining this box's local Y-axis direction, perpendicular to the X-axis.
        /// </summary>
        [DataMember]
        public Vector2 AxisY;

        /// <summary>
        /// The half extents of this box, representing the distance from the center to each edge along the local axes.
        /// </summary>
        [DataMember]
        public Vector2 HalfExtents;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the width of this box along its local X-axis.
        /// </summary>
        public readonly float Width => HalfExtents.X * 2.0f;

        /// <summary>
        /// Gets the height of this box along its local Y-axis.
        /// </summary>
        public readonly float Height => HalfExtents.Y * 2.0f;

        /// <summary>
        /// Gets the rotation angle of this box in radians, measured counter-clockwise from the positive world X-axis.
        /// </summary>
        public readonly float Rotation => MathF.Atan2(AxisX.Y, AxisX.X);

        /// <summary>
        /// Gets the area enclosed by this box in.
        /// </summary>
        public readonly float Area => Width * Height;

        #endregion

        #region Internal Properties

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    "Center( ", Center.ToString(), " )  \r\n",
                    "Rotation( ", Rotation.ToString("F2"), " rad )  \r\n",
                    "Size( ", Width.ToString("F2"), " x ", Height.ToString("F2"), " )"
                );
            }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a new <see cref="OrientedBoundingBox2D"/> with the specified center, orientation, and extents.
        /// </summary>
        /// <param name="center">The center position of the box in 2D space.</param>
        /// <param name="axisX">
        /// The unit vector defining the local X-axis direction. Should be normalized for accurate calculations.
        /// </param>
        /// <param name="axisY">
        /// The unit vector defining the local Y-axis direction. Should be normalized and perpendicular to <paramref name="axisX"/>.
        /// </param>
        /// <param name="halfExtents">
        /// The half extents, representing the distance from the center to each edge along the local axes.
        /// </param>
        public OrientedBoundingBox2D(Vector2 center, Vector2 axisX, Vector2 axisY, Vector2 halfExtents)
        {
            Center = center;
            AxisX = axisX;
            AxisY = axisY;
            HalfExtents = halfExtents;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new <see cref="OrientedBoundingBox2D"/> from a center, rotation angle, and extents.
        /// </summary>
        /// <param name="center">The center position of the box in 2D space.</param>
        /// <param name="rotation">
        /// The rotation angle in radians, measured counter-clockwise from the positive world X-axis.
        /// </param>
        /// <param name="halfExtents">
        /// The half extents, representing the distance from the center to each edge along the local axes.
        /// </param>
        /// <returns>
        /// A new <see cref="OrientedBoundingBox2D"/> with the specified center, rotation, and extents.
        /// </returns>
        public static OrientedBoundingBox2D CreateFromRotation(Vector2 center, float rotation, Vector2 halfExtents)
        {
            float cos = MathF.Cos(rotation);
            float sin = MathF.Sin(rotation);

            return new OrientedBoundingBox2D(
                center,
                new Vector2(cos, sin),
                new Vector2(-sin, cos),
                halfExtents
            );
        }

        /// <summary>
        /// Creates a new <see cref="OrientedBoundingBox2D"/> from an axis-aligned bounding box with zero rotation.
        /// </summary>
        /// <param name="box">The axis-aligned bounding box to convert.</param>
        /// <returns>
        /// A new <see cref="OrientedBoundingBox2D"/> with the same center and extents as the input box,
        /// aligned with the world axes.
        /// </returns>
        public static OrientedBoundingBox2D CreateFromBoundingBox2D(BoundingBox2D box)
        {
            return new OrientedBoundingBox2D(
                box.Center,
                new Vector2(1, 0),
                new Vector2(0, 1),
                box.HalfExtents
            );
        }

        /// <summary>
        /// Creates an <see cref="OrientedBoundingBox2D"/> that encloses two oriented bounding boxes.
        /// </summary>
        /// <param name="original">The first oriented bounding box to enclose.</param>
        /// <param name="additional">The second oriented bounding box to enclose.</param>
        /// <returns>
        /// A new <see cref="OrientedBoundingBox2D"/> that completely contains both input boxes.
        /// </returns>
        /// <remarks>
        /// Uses Principal Component Analysis (PCA) with power iteration to compute optimal axes for the merged box.
        /// The resulting box may not be the absolute minimum volume, but provides a good approximation that aligns
        /// with the principal direction of the combined corners.
        /// </remarks>
        public static OrientedBoundingBox2D CreateMerged(OrientedBoundingBox2D original, OrientedBoundingBox2D additional)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 6.5.3 "Merging Two OBBs", Section 4.4.2 "PCA-based OBBs"
            // Derived PCA-based OBB merge using power iteration in place of the
            // Jacobi eigenvalue method described in the book.

            const float Epsilon = 1e-6f;
            const int Iterations = 4;

            // Get all corners from both rectangles
            // Computing inline instead of calling GetCorners to avoid multiple array allocations and array copies
            Vector2 extX1 = original.AxisX * original.HalfExtents.X;
            Vector2 extY1 = original.AxisY * original.HalfExtents.Y;
            Vector2 extX2 = additional.AxisX * additional.HalfExtents.X;
            Vector2 extY2 = additional.AxisY * additional.HalfExtents.Y;
            Vector2[] allCorners = new Vector2[]
            {
                original.Center - extX1 - extY1,
                original.Center + extX1 - extY1,
                original.Center + extX1 + extY1,
                original.Center - extX1 + extY1,

                additional.Center - extX2 - extY2,
                additional.Center + extX2 - extY2,
                additional.Center + extX2 + extY2,
                additional.Center - extX2 + extY2,
            };

            // Compute the centroid
            Vector2 centroid = Vector2.Zero;
            for (int i = 0; i < allCorners.Length; i++)
            {
                centroid += allCorners[i];
            }
            centroid /= allCorners.Length;

            // Compute covariance matrix
            float cxx = 0;
            float cxy = 0;
            float cyy = 0;
            for (int i = 0; i < allCorners.Length; i++)
            {
                Vector2 p = allCorners[i] - centroid;
                cxx += p.X * p.X;
                cxy += p.X * p.Y;
                cyy += p.Y * p.Y;
            }

            // Find eigenvector of largest eigenvalue using power iteration.
            // to get the primary axis of the OBB
            Vector2 axisX = Vector2.UnitX;
            for (int iter = 0; iter < Iterations; iter++)
            {
                float newX = cxx * axisX.X + cxy * axisX.Y;
                float newY = cxy * axisX.X + cyy * axisX.Y;
                axisX = new Vector2(newX, newY);

                // Only sqrt if necessary
                float lenSq = axisX.LengthSquared();
                if (lenSq > Epsilon * Epsilon)
                {
                    axisX /= MathF.Sqrt(lenSq);
                }
            }

            // Compute perpendicular axis
            Vector2 axisY = new Vector2(-axisX.Y, axisX.X);

            // Project all points onto both axes to find extents
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < allCorners.Length; i++)
            {
                Vector2 p = allCorners[i] - centroid;
                float projX = Vector2.Dot(p, axisX);
                float projY = Vector2.Dot(p, axisY);

                minX = MathF.Min(minX, projX);
                maxX = MathF.Max(maxX, projX);
                minY = MathF.Min(minY, projY);
                maxY = MathF.Max(maxY, projY);
            }

            // Compute center and extents in the new OBB's local space
            float centerOffsetX = (minX + maxX) * 0.5f;
            float centerOffsetY = (minY + maxY) * 0.5f;
            Vector2 center = centroid + axisX * centerOffsetX + axisY * centerOffsetY;

            Vector2 halfExtents = new Vector2(
                (maxX - minX) * 0.5f,
                (maxY - minY) * 0.5f
            );

            return new OrientedBoundingBox2D(center, axisX, axisY, halfExtents);
        }

        /// <summary>
        /// Gets an array containing the four corner positions of this oriented bounding box.
        /// </summary>
        /// <returns>
        /// An array of 4 corner positions in order: top-left, top-right, bottom-right, bottom-left,
        /// relative to the box's local orientation.
        /// </returns>
        public readonly Vector2[] GetCorners()
        {
            Vector2 extX = AxisX * HalfExtents.X;
            Vector2 extY = AxisY * HalfExtents.Y;

            return new Vector2[]
            {
                Center - extX - extY,   // Top-left
                Center + extX - extY,   // Top-right
                Center + extX + extY,   // Bottom-right
                Center - extX + extY    // Bottom-left
            };
        }

        /// <summary>
        /// Writes the four corner positions of this oriented bounding box into an existing array.
        /// </summary>
        /// <param name="corners">
        /// The array to write corner positions into. Must have at least 4 elements.
        /// Corners are written in order: top-left, top-right, bottom-right, bottom-left.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="corners"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="corners"/> has fewer than 4 elements.
        /// </exception>
        public readonly void GetCorners(Vector2[] corners)
        {
            if (corners == null)
            {
                throw new ArgumentNullException(nameof(corners));
            }

            if (corners.Length < CornerCount)
            {
                throw new ArgumentException($"Array must have at least {CornerCount} elements", nameof(corners));
            }

            Vector2 extX = AxisX * HalfExtents.X;
            Vector2 extY = AxisY * HalfExtents.Y;

            corners[0] = Center - extX - extY;   // Top-left
            corners[1] = Center + extX - extY;   // Top-right
            corners[2] = Center + extX + extY;   // Bottom-right
            corners[3] = Center - extX + extY;   // Bottom-left
        }

        /// <inheritdoc/>
        public readonly bool Equals(OrientedBoundingBox2D other)
        {
            return Center.Equals(other.Center)
                   && AxisX.Equals(other.AxisX)
                   && AxisY.Equals(other.AxisY)
                   && HalfExtents.Equals(other.HalfExtents);
        }

        /// <summary>
        /// Tests whether this oriented bounding box contains, intersects, or is separate from another oriented bounding box.
        /// </summary>
        /// <param name="other">The other oriented bounding box to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the other box is completely inside this one;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(OrientedBoundingBox2D other)
        {
            // Use Separating Axis Theorem to test containment
            // If all corners of the other OBB are inside this OBB, it's contained
            Vector2[] corners = other.GetCorners();
            bool allInside = true;

            for (int i = 0; i < corners.Length; i++)
            {
                if (Contains(corners[i]) != ContainmentType.Contains)
                {
                    allInside = false;
                    break;
                }
            }

            if (allInside)
            {
                return ContainmentType.Contains;
            }

            // Check for intersection
            if (Intersects(other))
            {
                return ContainmentType.Intersects;
            }

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether this oriented bounding box contains, intersects, or is separate from a circle.
        /// </summary>
        /// <param name="circle">The circle to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the circle is completely inside this box;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingCircle circle)
        {
            //Transform circle center to OBB local space
            Vector2 diff = circle.Center - Center;
            Vector2 localCenter = new Vector2(
                Vector2.Dot(diff, AxisX),
                Vector2.Dot(diff, AxisY)
            );

            // Check if circle (as an AABB in local space) is fully inside
            if (MathF.Abs(localCenter.X) + circle.Radius <= HalfExtents.X &&
               MathF.Abs(localCenter.Y) + circle.Radius <= HalfExtents.Y)
            {
                return ContainmentType.Contains;
            }

            // Check for intersection
            if (Intersects(circle))
            {
                return ContainmentType.Intersects;
            }

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether a point lies inside this oriented bounding box or on its boundary.
        /// </summary>
        /// <param name="point">The point to test in 2D space.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the point is inside or on the boundary;
        /// otherwise, <see cref="ContainmentType.Disjoint"/> if the point is outside.
        /// </returns>
        public readonly ContainmentType Contains(Vector2 point)
        {
            // Transform point to OBB local space
            Vector2 diff = point - Center;
            float projX = Vector2.Dot(diff, AxisX);
            float projY = Vector2.Dot(diff, AxisY);

            // Check if within extents
            if (MathF.Abs(projX) <= HalfExtents.X && MathF.Abs(projY) <= HalfExtents.Y)
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether this oriented bounding box intersects with a circle.
        /// </summary>
        /// <param name="circle">The circle to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the box and circle overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCircle circle)
        {
            return circle.Intersects(this);
        }

        /// <summary>
        /// Tests whether this oriented bounding box intersects with another oriented bounding box.
        /// </summary>
        /// <param name="other">The other oriented bounding box to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the boxes overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(OrientedBoundingBox2D other)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.1 "Separating-axis Test", Section 4.4 "Oriented Bounding Boxes"

            if (!OverlapOnAxis(this, other, AxisX))
            {
                return false;
            }

            if (!OverlapOnAxis(this, other, AxisY))
            {
                return false;
            }

            if (!OverlapOnAxis(this, other, other.AxisX))
            {
                return false;
            }

            if (!OverlapOnAxis(this, other, other.AxisY))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tests whether this oriented bounding box intersects with an axis-aligned bounding box.
        /// </summary>
        /// <param name="box">The bounding box to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the boxes overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingBox2D box)
        {
            return box.Intersects(this);
        }

        /// <summary>
        /// Tests whether this oriented bounding box intersects with a capsule.
        /// </summary>
        /// <param name="capsule">The capsule to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the box and capsule overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCapsule2D capsule)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // OBB-Capsule test: Find minimum distance between OBB and capsule segment
            // If distance <= capsule radius, they intersect

            float radiusSq = capsule.Radius * capsule.Radius;

            // Transform capsule segment into OBB local space
            Vector2 a = capsule.PointA - Center;
            Vector2 b = capsule.PointB - Center;

            Vector2 localA = new Vector2(
                Vector2.Dot(a, AxisX),
                Vector2.Dot(a, AxisY));

            Vector2 localB = new Vector2(
                Vector2.Dot(b, AxisX),
                Vector2.Dot(b, AxisY));

            // Check if either capsule endpoint is inside the OBB
            bool aInside = localA.X >= -HalfExtents.X && localA.X <= HalfExtents.X &&
                           localA.Y >= -HalfExtents.Y && localA.Y <= HalfExtents.Y;

            bool bInside = localB.X >= -HalfExtents.X && localB.X <= HalfExtents.X &&
                           localB.Y >= -HalfExtents.Y && localB.Y <= HalfExtents.Y;

            if (aInside || bInside)
                return true;

            // Create a LineSegment2D for the capsule in local space
            LineSegment2D capsuleSegment = new LineSegment2D(localA, localB);

            // Check if any OBB corner is within capsule radius of the segment
            Vector2[] corners = new Vector2[]
            {
                new Vector2(-HalfExtents.X, -HalfExtents.Y),
                new Vector2(HalfExtents.X, -HalfExtents.Y),
                new Vector2(HalfExtents.X, HalfExtents.Y),
                new Vector2(-HalfExtents.X, HalfExtents.Y)
            };

            for (int i = 0; i < 4; i++)
            {
                float distSq = capsuleSegment.DistanceSquaredToPoint(corners[i]);
                if (distSq <= radiusSq)
                    return true;
            }

            // Check distance from capsule segment to each OBB edge
            Vector2[][] edges = new Vector2[][]
            {
                new Vector2[] { corners[0], corners[1] }, // Bottom edge
                new Vector2[] { corners[1], corners[2] }, // Right edge
                new Vector2[] { corners[2], corners[3] }, // Top edge
                new Vector2[] { corners[3], corners[0] }  // Left edge
            };

            for (int i = 0; i < 4; i++)
            {
                LineSegment2D edge = new LineSegment2D(edges[i][0], edges[i][1]);
                float distSq = capsuleSegment.DistanceSquaredToSegment(edge, out _, out _, out _, out _);
                if (distSq <= radiusSq)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Tests whether this oriented bounding box intersects with a polygon.
        /// </summary>
        /// <param name="polygon">The polygon to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the box and polygon overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingPolygon2D polygon)
        {
            return polygon.Intersects(this);
        }

        /// <summary>
        /// Applies a matrix transformation to this oriented bounding box and creates a new transformed box.
        /// </summary>
        /// <param name="matrix">The transformation matrix to apply.</param>
        /// <returns>
        /// A new <see cref="OrientedBoundingBox2D"/> with the center transformed by the matrix,
        /// the axes rotated, and the extents scaled appropriately.
        /// </returns>
        /// <remarks>
        /// The transformation applies to all components: the center is transformed as a point,
        /// the local axes are rotated and normalized, and the half extents are scaled by the
        /// corresponding scale factors extracted from the transformation.
        /// </remarks>
        public readonly OrientedBoundingBox2D Transform(Matrix matrix)
        {
            const float Epsilon = 1e-6f;

            Vector2 transformedCenter = Vector2.Transform(Center, matrix);

            // Transform the axes (rotation and scale)
            Vector2 transformedAxisX = Vector2.TransformNormal(AxisX, matrix);
            Vector2 transformedAxisY = Vector2.TransformNormal(AxisY, matrix);

            // Extract scale from transformed axes
            float scaleX = transformedAxisX.Length();
            float scaleY = transformedAxisY.Length();

            // Normalize axes
            transformedAxisX = scaleX > Epsilon ? transformedAxisX / scaleX : Vector2.UnitX;
            transformedAxisY = scaleY > Epsilon ? transformedAxisY / scaleY : Vector2.UnitY;

            // Scale the extents
            Vector2 transformedExtents = new Vector2(
                HalfExtents.X * scaleX,
                HalfExtents.Y * scaleY
            );

            return new OrientedBoundingBox2D(
                transformedCenter,
                transformedAxisX,
                transformedAxisY,
                transformedExtents
            );
        }

        /// <summary>
        /// Creates a new <see cref="OrientedBoundingBox2D"/> by translating this box by the specified offset.
        /// </summary>
        /// <param name="translation">The offset to translate the box by in 2D space.</param>
        /// <returns>
        /// A new <see cref="OrientedBoundingBox2D"/> at the translated position with the same orientation and extents.
        /// </returns>
        public readonly OrientedBoundingBox2D Translate(Vector2 translation)
        {
            return new OrientedBoundingBox2D(
                Center + translation,
                AxisX,
                AxisY,
                HalfExtents
            );
        }

        /// <summary>
        /// Deconstructs this oriented bounding box into its component values.
        /// </summary>
        /// <param name="center">
        /// When this method returns, contains the center position of this box in 2D space.
        /// </param>
        /// <param name="axisX">
        /// When this method returns, contains the unit vector defining this box's local X-axis direction.
        /// </param>
        /// <param name="axisY">
        /// When this method returns, contains the unit vector defining this box's local Y-axis direction.
        /// </param>
        /// <param name="halfExtents">
        /// When this method returns, contains the half extents of this box in world units.
        /// </param>
        public readonly void Deconstruct(out Vector2 center, out Vector2 axisX, out Vector2 axisY, out Vector2 halfExtents)
        {
            center = Center;
            axisX = AxisX;
            axisY = AxisY;
            halfExtents = HalfExtents;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            return obj is OrientedBoundingBox2D other && Equals(other);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return Center.GetHashCode() ^
                   AxisX.GetHashCode() ^
                   AxisY.GetHashCode() ^
                   HalfExtents.GetHashCode();
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{{Center:{Center} Rotation:{Rotation:F2} Size:{Width:F2}x{Height:F2}}}";
        }

        /// <summary/>
        public static bool operator ==(OrientedBoundingBox2D left, OrientedBoundingBox2D right)
        {
            return left.Equals(right);
        }

        /// <summary/>
        public static bool operator !=(OrientedBoundingBox2D left, OrientedBoundingBox2D right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region Separating Axis Theorem Helper Methods

        private static bool OverlapOnAxis(OrientedBoundingBox2D obb1, OrientedBoundingBox2D obb2, Vector2 axis)
        {
            // Project both OBBs onto the axis and check for overlap
            float min1, max1, min2, max2;

            ProjectOntoAxis(obb1, axis, out min1, out max1);
            ProjectOntoAxis(obb2, axis, out min2, out max2);

            // Check if intervals overlap
            return !(max1 < min2 || max2 < min1);
        }

        private static void ProjectOntoAxis(OrientedBoundingBox2D obb, Vector2 axis, out float min, out float max)
        {
            // Project center
            float centerProj = Vector2.Dot(obb.Center, axis);

            // Project extents
            float extentProj = MathF.Abs(Vector2.Dot(obb.AxisX * obb.HalfExtents.X, axis)) +
                               MathF.Abs(Vector2.Dot(obb.AxisY * obb.HalfExtents.Y, axis));

            min = centerProj - extentProj;
            max = centerProj + extentProj;
        }

        #endregion
    }
}
