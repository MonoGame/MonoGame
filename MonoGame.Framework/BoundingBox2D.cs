// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Represents an axis-aligned bounding box in 2D space, defined by minimum and maximum corner points.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct BoundingBox2D : IEquatable<BoundingBox2D>
    {
        /// <summary>
        /// The number of corners in this bounding box.
        /// </summary>
        public const int CornerCount = 4;

        #region Public Fields

        /// <summary>
        /// The minimum corner position with the smallest X and Y coordinates.
        /// </summary>
        [DataMember]
        public Vector2 Min;

        /// <summary>
        /// The maximum corner position with the largest X and Y coordinates.
        /// </summary>
        [DataMember]
        public Vector2 Max;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the center position of this bounding box in 2D space.
        /// </summary>
        public readonly Vector2 Center => (Min + Max) * 0.5f;

        /// <summary>
        /// Gets the size of this bounding box as width and height.
        /// </summary>
        public readonly Vector2 Size => Max - Min;

        /// <summary>
        /// Gets the half extents of this bounding box, representing the distance from the center to each edge.
        /// </summary>
        public readonly Vector2 HalfExtents => (Max - Min) * 0.5f;

        /// <summary>
        /// Gets the width of this bounding box.
        /// </summary>
        public readonly float Width => Max.X - Min.X;

        /// <summary>
        /// Gets the height of this bounding box.
        /// </summary>
        public readonly float Height => Max.Y - Min.Y;

        /// <summary>
        /// Gets the area enclosed by this bounding box.
        /// </summary>
        public readonly float Area => Width * Height;

        #endregion

        #region Internal Properties

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    "Min( ", Min.ToString(), " )  \r\n",
                    "Max( ", Max.ToString(), " )"
                );
            }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a new <see cref="BoundingBox2D"/> with the specified minimum and maximum corners.
        /// </summary>
        /// <param name="min">The minimum corner, containing the smallest X and Y coordinate values.</param>
        /// <param name="max">The maximum corner, containing the largest X and Y coordinate values.</param>
        public BoundingBox2D(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new <see cref="BoundingBox2D"/> from the specified minimum and maximum corners.
        /// </summary>
        /// <param name="min">The minimum corner, containing the smallest X and Y coordinate values.</param>
        /// <param name="max">The maximum corner, containing the largest X and Y coordinate values.</param>
        /// <returns>
        /// A new <see cref="BoundingBox2D"/> with the specified corners.
        /// </returns>
        public static BoundingBox2D CreateFromMinMax(Vector2 min, Vector2 max)
        {
            return new BoundingBox2D(min, max);
        }

        /// <summary>
        /// Creates a new <see cref="BoundingBox2D"/> from a center position and half extents.
        /// </summary>
        /// <param name="center">The center position of the bounding box in 2D space.</param>
        /// <param name="halfExtents">
        /// The half extents representing the distance from the center to each edge (half-width and half-height).
        /// </param>
        /// <returns>
        /// A new <see cref="BoundingBox2D"/> centered at the specified position with the given extents.
        /// </returns>
        public static BoundingBox2D CreateFromCenterAndExtents(Vector2 center, Vector2 halfExtents)
        {
            return new BoundingBox2D(center - halfExtents, center + halfExtents);
        }

        /// <summary>
        /// Creates a new <see cref="BoundingBox2D"/> from a minimum corner position and size.
        /// </summary>
        /// <param name="position">The minimum corner position in 2D space.</param>
        /// <param name="size">The size as width and height.</param>
        /// <returns>
        /// A new <see cref="BoundingBox2D"/> with the specified position and size.
        /// </returns>
        public static BoundingBox2D CreateFromPositionAndSize(Vector2 position, Vector2 size)
        {
            return new BoundingBox2D(position, position + size);
        }

        /// <summary>
        /// Creates a new <see cref="BoundingBox2D"/> that encloses all specified points.
        /// </summary>
        /// <param name="points">The array of points to enclose within the bounding box.</param>
        /// <returns>
        /// A new <see cref="BoundingBox2D"/> with minimum and maximum corners positioned to contain all the specified points.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="points"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="points"/> is empty.
        /// </exception>
        public static BoundingBox2D CreateFromPoints(Vector2[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            if (points.Length == 0)
            {
                throw new ArgumentException("Cannot create bounding box from empty point array.", nameof(points));
            }

            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].X < min.X) min.X = points[i].X;
                if (points[i].Y < min.Y) min.Y = points[i].Y;
                if (points[i].X > max.X) max.X = points[i].X;
                if (points[i].Y > max.Y) max.Y = points[i].Y;
            }

            return new BoundingBox2D(min, max);
        }

        /// <summary>
        /// Creates a new <see cref="BoundingBox2D"/> that encloses two bounding boxes.
        /// </summary>
        /// <param name="original">The first bounding box to enclose.</param>
        /// <param name="additional">The second bounding box to enclose.</param>
        /// <returns>
        /// A new <see cref="BoundingBox2D"/> that completely contains both input bounding boxes.
        /// </returns>
        public static BoundingBox2D CreateMerged(BoundingBox2D original, BoundingBox2D additional)
        {
            BoundingBox2D result;
            result.Min.X = MathF.Min(original.Min.X, additional.Min.X);
            result.Min.Y = MathF.Min(original.Min.Y, additional.Min.Y);
            result.Max.X = MathF.Max(original.Max.X, additional.Max.X);
            result.Max.Y = MathF.Max(original.Max.Y, additional.Max.Y);
            return result;
        }

        /// <summary>
        /// Gets an array containing the four corner positions of this bounding box.
        /// </summary>
        /// <returns>
        /// An array of 4 corner positions in counter-clockwise order starting from the minimum corner:
        /// bottom-left, bottom-right, top-right, top-left.
        /// </returns>
        public readonly Vector2[] GetCorners()
        {
            return new Vector2[CornerCount]
            {
                new Vector2(Min.X, Min.Y),
                new Vector2(Max.X, Min.Y),
                new Vector2(Max.X, Max.Y),
                new Vector2(Min.X, Max.Y)
            };
        }

        /// <summary>
        /// Writes the four corner positions of this bounding box into an existing array.
        /// </summary>
        /// <param name="corners">
        /// The array to write corner positions into. Must have at least 4 elements.
        /// Corners are written in counter-clockwise order starting from the minimum corner.
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
                throw new ArgumentNullException(nameof(corners));

            if (corners.Length < CornerCount)
                throw new ArgumentException($"Array must have at least {CornerCount} elements", nameof(corners));

            corners[0] = new Vector2(Min.X, Min.Y);
            corners[1] = new Vector2(Max.X, Min.Y);
            corners[2] = new Vector2(Max.X, Max.Y);
            corners[3] = new Vector2(Min.X, Max.Y);
        }

        /// <summary>
        /// Applies a matrix transformation to this bounding box and creates a new axis-aligned bounding box that encloses the result.
        /// </summary>
        /// <param name="matrix">The transformation matrix to apply.</param>
        /// <returns>
        /// A new axis-aligned <see cref="BoundingBox2D"/> that completely contains all corners of this bounding box after transformation.
        /// </returns>
        /// <remarks>
        /// When a transformation includes rotation or skew, the transformed corners of the original box may no longer
        /// be axis-aligned. This method computes a new axis-aligned bounding box that encloses all transformed corners,
        /// which may be larger than the original if rotation is applied.
        /// </remarks>
        public readonly BoundingBox2D Transform(Matrix matrix)
        {
            Vector2 transformedCenter = Vector2.Transform(Center, matrix);

            // For each axis, sum the absolute values of the transformed half-extents.
            // This gives us the new box that bounds the rotated original
            Vector2 newHalfExtents;
            newHalfExtents.X = MathF.Abs(matrix.M11) * HalfExtents.X
                             + MathF.Abs(matrix.M12) * HalfExtents.Y;

            newHalfExtents.Y = MathF.Abs(matrix.M21) * HalfExtents.X
                             + MathF.Abs(matrix.M22) * HalfExtents.Y;

            return CreateFromCenterAndExtents(transformedCenter, newHalfExtents);
        }

        /// <summary>
        /// Creates a new <see cref="BoundingBox2D"/> by translating this bounding box by the specified offset.
        /// </summary>
        /// <param name="translation">The offset to translate the bounding box by in 2D space.</param>
        /// <returns>
        /// A new <see cref="BoundingBox2D"/> at the translated position with the same size.
        /// </returns>
        public readonly BoundingBox2D Translate(Vector2 translation)
        {
            return new BoundingBox2D(Min + translation, Max + translation);
        }

        /// <summary>
        /// Tests whether a point lies inside this bounding box or on its boundary.
        /// </summary>
        /// <param name="point">The point to test in 2D space.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the point is inside or on the boundary;
        /// otherwise, <see cref="ContainmentType.Disjoint"/> if the point is outside.
        /// </returns>
        public readonly ContainmentType Contains(Vector2 point)
        {
            return Collision2D.ContainsAabbPoint(point, Min, Max);
        }

        /// <summary>
        /// Tests whether this bounding box contains, intersects, or is separate from another bounding box.
        /// </summary>
        /// <param name="other">The other bounding box to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the other bounding box is completely inside this one;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingBox2D other)
        {
            return Collision2D.ContainsAabbAabb(Min, Max, other.Min, other.Max);
        }

        /// <summary>
        /// Tests whether this bounding box contains, intersects, or is separate from a circle.
        /// </summary>
        /// <param name="circle">The circle to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the circle is completely inside this bounding box;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingCircle circle)
        {
            return Collision2D.ContainsAabbCircle(Min, Max, circle.Center, circle.Radius);
        }

        /// <summary>
        /// Tests whether this bounding box contains, intersects, or is separate from an oriented bounding box.
        /// </summary>
        /// <param name="obb">The oriented bounding box to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the oriented bounding box is completely inside this bounding box;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(OrientedBoundingBox2D obb)
        {
            return Collision2D.ContainsAabbObb(Min, Max, obb.Center, obb.AxisX, obb.AxisY, obb.HalfExtents);
        }

        /// <summary>
        /// Tests whether this bounding box contains, intersects, or is separate from a capsule.
        /// </summary>
        /// <param name="capsule">The capsule to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the capsule is completely inside this bounding box;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingCapsule2D capsule)
        {
            return Collision2D.ContainsAabbCapsule(Min, Max, capsule.PointA, capsule.PointB, capsule.Radius);
        }

        /// <summary>
        /// Tests whether this bounding box contains, intersects, or is separate from a polygon.
        /// </summary>
        /// <param name="polygon">The polygon to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the polygon is completely inside this bounding box;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingPolygon2D polygon)
        {
            return Collision2D.ContainsAabbConvexPolygon(Min, Max, polygon.Vertices, polygon.Normals);
        }

        /// <summary>
        /// Tests whether this bounding box intersects with another bounding box.
        /// </summary>
        /// <param name="other">The other bounding box to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the bounding boxes overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingBox2D other)
        {
            return Collision2D.IntersectsAabbAabb(Min, Max, other.Min, other.Max);
        }

        /// <summary>
        /// Tests whether this bounding box intersects with a circle.
        /// </summary>
        /// <param name="circle">The circle to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the bounding box and circle overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCircle circle)
        {
            return Collision2D.IntersectsCircleAabb(circle.Center, circle.Radius, Min, Max);
        }

        /// <summary>
        /// Tests whether this bounding box intersects with a capsule.
        /// </summary>
        /// <param name="capsule">The capsule to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the bounding box and capsule overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCapsule2D capsule)
        {
            return Collision2D.IntersectsAabbCapsule(Min, Max, capsule.PointA, capsule.PointB, capsule.Radius);
        }

        /// <summary>
        /// Tests whether this bounding box intersects with an oriented bounding box.
        /// </summary>
        /// <param name="obb">The oriented bounding box to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the boxes overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(OrientedBoundingBox2D obb)
        {
            return Collision2D.IntersectsAabbObb(Center, HalfExtents, obb.Center, obb.AxisX, obb.AxisY, obb.HalfExtents);
        }

        /// <summary>
        /// Tests whether this bounding box intersects with a polygon.
        /// </summary>
        /// <param name="polygon">The polygon to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the box and polygon overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingPolygon2D polygon)
        {
            return Collision2D.IntersectsAabbConvexPolygon(Center, HalfExtents, polygon.Vertices, polygon.Normals);
        }

        /// <summary>
        /// Deconstructs this bounding box into its component values.
        /// </summary>
        /// <param name="min">
        /// When this method returns, contains the minimum corner, containing the smallest X and Y coordinate values.
        /// </param>
        /// <param name="max">
        /// When this method returns, contains the maximum corner, containing the largest X and Y coordinate values.
        /// </param>
        public readonly void Deconstruct(out Vector2 min, out Vector2 max)
        {
            min = Min;
            max = Max;
        }

        /// <inheritdoc/>
        public readonly bool Equals(BoundingBox2D other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            return obj is BoundingBox2D other && Equals(other);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return Min.GetHashCode() ^ Max.GetHashCode();
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{{Min:{Min} Max:{Max}}}";
        }

        /// <summary/>
        public static bool operator ==(BoundingBox2D left, BoundingBox2D right)
        {
            return left.Equals(right);
        }

        /// <summary/>
        public static bool operator !=(BoundingBox2D left, BoundingBox2D right)
        {
            return !left.Equals(right);
        }

        #endregion

    }
}
