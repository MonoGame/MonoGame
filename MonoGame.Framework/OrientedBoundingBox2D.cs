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
                if (lenSq > Collision2D.Epsilon * Collision2D.Epsilon)
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
        /// Tests whether a point lies inside this oriented bounding box or on its boundary.
        /// </summary>
        /// <param name="point">The point to test in 2D space.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the point is inside or on the boundary;
        /// otherwise, <see cref="ContainmentType.Disjoint"/> if the point is outside.
        /// </returns>
        public readonly ContainmentType Contains(Vector2 point)
        {
            return Collision2D.ContainsObbPoint(point, Center, AxisX, AxisY, HalfExtents);
        }

        /// <summary>
        /// Tests whether this oriented bounding box contains, intersects, or is separate from a bounding box.
        /// </summary>
        /// <param name="aabb">The bounding box to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the bounding box is completely inside this box;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingBox2D aabb)
        {
            return Collision2D.ContainsObbAabb(Center, AxisX, AxisY, HalfExtents, aabb.Min, aabb.Max);
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
            return Collision2D.ContainsObbCircle(Center, AxisX, AxisY, HalfExtents, circle.Center, circle.Radius);
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
            return Collision2D.ContainsObbObb(Center, AxisX, AxisY, HalfExtents, other.Center, other.AxisX, other.AxisY, other.HalfExtents);
        }

        /// <summary>
        /// Tests whether this oriented bounding box contains, intersects, or is separate from a capsule.
        /// </summary>
        /// <param name="capsule">The capsule to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the capsule is completely inside this box;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingCapsule2D capsule)
        {
            return Collision2D.ContainsObbCapsule(Center, AxisX, AxisY, HalfExtents, capsule.PointA, capsule.PointB, capsule.Radius);
        }

        /// <summary>
        /// Tests whether this oriented bounding box contains, intersects, or is separate from a polygon.
        /// </summary>
        /// <param name="polygon">The polygon to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the polygon is completely inside this box;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingPolygon2D polygon)
        {
            return Collision2D.ContainsObbConvexPolygon(Center, AxisX, AxisY, HalfExtents, polygon.Vertices, polygon.Normals);
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
            return Collision2D.IntersectsCircleObb(circle.Center, circle.Radius, Center, AxisX, AxisY, HalfExtents);
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
            return Collision2D.IntersectsObbObb(Center, AxisX, AxisY, HalfExtents, other.Center, other.AxisX, other.AxisY, other.HalfExtents);
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
            return Collision2D.IntersectsAabbObb(box.Center, box.HalfExtents, Center, AxisX, AxisY, HalfExtents);
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
            return Collision2D.IntersectsObbCapsule(Center, AxisX, AxisY, HalfExtents, capsule.PointA, capsule.PointB, capsule.Radius);
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
            return Collision2D.IntersectsObbConvexPolygon(Center, AxisX, AxisY, HalfExtents, polygon.Vertices, polygon.Normals);
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
            Vector2 transformedCenter = Vector2.Transform(Center, matrix);

            // Transform the axes (rotation and scale)
            Vector2 transformedAxisX = Vector2.TransformNormal(AxisX, matrix);
            Vector2 transformedAxisY = Vector2.TransformNormal(AxisY, matrix);

            // Extract scale from transformed axes
            float scaleX = transformedAxisX.Length();
            float scaleY = transformedAxisY.Length();

            // Normalize axes
            transformedAxisX = scaleX > Collision2D.Epsilon ? transformedAxisX / scaleX : Vector2.UnitX;
            transformedAxisY = scaleY > Collision2D.Epsilon ? transformedAxisY / scaleY : Vector2.UnitY;

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
    }
}
