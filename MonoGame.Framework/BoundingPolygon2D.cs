// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{

    /// <summary>
    /// Represents a convex polygon bounding volume in 2D space.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundingPolygon2D : IEquatable<BoundingPolygon2D>
    {
        #region Public Fields

        /// <summary>
        /// The vertices of this polygon in counter-clockwise order in 2D space.
        /// </summary>
        /// <remarks>
        /// The counter-clockwise winding order is used to derive outward-facing edge normals for collision detection.
        /// </remarks>
        [DataMember]
        public Vector2[] Vertices;

        /// <summary>
        /// The outward-facing unit normals for each edge of this polygon.
        /// </summary>
        /// <remarks>
        /// These normals are precomputed to for collision detection.
        /// Each normal at index i corresponds to the edge from Vertices[i] to Vertices[(i + 1) % Vertices.Length].
        /// </remarks>
        [DataMember]
        public Vector2[] Normals;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the number of vertices in this polygon.
        /// </summary>
        public readonly int VertexCount => Vertices?.Length ?? 0;

        /// <summary>
        /// Gets the geometric centroid of this polygon in 2D space.
        /// </summary>
        /// <remarks>
        /// Computed as the arithmetic mean of all vertex positions. This represents the geometric center
        /// and does not account for mass distribution. For physics simulations requiring a true center of mass
        /// based on vertex weights, a separate calculation should be performed.
        /// </remarks>
        public readonly Vector2 Centroid
        {
            get
            {
                if (Vertices == null || Vertices.Length == 0)
                {
                    return Vector2.Zero;
                }

                Vector2 sum = Vector2.Zero;
                for (int i = 0; i < Vertices.Length; i++)
                {
                    sum += Vertices[i];
                }

                return sum / Vertices.Length;
            }
        }

        /// <summary>
        /// Gets the area enclosed by this polygon.
        /// </summary>
        public readonly float Area
        {
            get
            {
                if (Vertices == null || Vertices.Length == 0)
                {
                    return 0.0f;
                }

                float area = 0.0f;
                int n = Vertices.Length;

                for (int i = 0; i < n; i++)
                {
                    int j = (i + 1) % n;
                    area += Vertices[i].X * Vertices[j].Y;
                    area -= Vertices[j].X * Vertices[i].Y;
                }

                return MathF.Abs(area) * 0.5f;
            }
        }

        #endregion

        #region Internal Properties

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    "Vertices( ", VertexCount.ToString(), " )  \r\n",
                    "Area( ", Area.ToString("F2"), " )"
                );
            }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a new <see cref="BoundingPolygon2D"/> with the specified vertices.
        /// </summary>
        /// <param name="vertices">
        /// The vertices of the polygon in counter-clockwise order in 2D space. Must contain at least three vertices.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="vertices"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="vertices"/> contains fewer than three elements.
        /// </exception>
        /// <remarks>
        /// Edge normals are computed automatically from the vertices and cached for collision detection.
        /// </remarks>
        public BoundingPolygon2D(Vector2[] vertices)
        {
            if (vertices == null)
            {
                throw new ArgumentNullException(nameof(vertices));
            }

            if (vertices.Length < 3)
            {
                throw new ArgumentException("Polygon must have at least 3 vertices", nameof(vertices));
            }

            Vertices = vertices;
            Normals = ComputeNormals(vertices);
        }


        /// <summary>
        /// Creates a new <see cref="BoundingPolygon2D"/> with the specified vertices and precomputed edge normals.
        /// </summary>
        /// <param name="vertices">
        /// The vertices of the polygon in counter-clockwise order in 2D space. Must contain at least three vertices.
        /// </param>
        /// <param name="normals">
        /// The outward-facing unit normals for each polygon edge. Must have the same length as <paramref name="vertices"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="vertices"/> or <paramref name="normals"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="vertices"/> contains fewer than three elements,
        /// or when the length of <paramref name="normals"/> does not match the length of <paramref name="vertices"/>.
        /// </exception>
        /// <remarks>
        /// This constructor allows precomputed normals to be provided, avoiding recomputation when the data is already available.
        /// </remarks>
        public BoundingPolygon2D(Vector2[] vertices, Vector2[] normals)
        {
            if (vertices == null)
            {
                throw new ArgumentNullException(nameof(vertices));
            }

            if (normals == null)
            {
                throw new ArgumentNullException(nameof(normals));
            }

            if (vertices.Length < 3)
            {
                throw new ArgumentException("Polygon must have at least 3 vertices", nameof(vertices));
            }

            if (vertices.Length != normals.Length)
            {
                throw new ArgumentException("Normals array must have same length as vertices array");
            }

            Vertices = vertices;
            Normals = normals;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new <see cref="BoundingPolygon2D"/> from the specified vertices.
        /// </summary>
        /// <param name="vertices">
        /// The vertices of the polygon in counter-clockwise order in 2D space. Must contain at least three vertices.
        /// </param>
        /// <returns>
        /// A new <see cref="BoundingPolygon2D"/> with edge normals computed automatically.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="vertices"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="vertices"/> contains fewer than three elements.
        /// </exception>
        public static BoundingPolygon2D CreateFromVertices(Vector2[] vertices)
        {
            return new BoundingPolygon2D(vertices);
        }

        /// <summary>
        /// Creates a new <see cref="BoundingPolygon2D"/> representing a regular polygon with equal side lengths and angles.
        /// </summary>
        /// <param name="center">The center position of the polygon in 2D space.</param>
        /// <param name="radius">The distance from the center to each vertex.</param>
        /// <param name="sides">The number of sides of the polygon. Must be at least three.</param>
        /// <param name="rotation">
        /// The rotation angle of the polygon in radians, measured counter-clockwise from the positive world X-axis.
        /// </param>
        /// <returns>
        /// A new <see cref="BoundingPolygon2D"/> with vertices in counter-clockwise order and edge normals computed automatically.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="sides"/> is less than three.
        /// </exception>
        public static BoundingPolygon2D CreateRegular(Vector2 center, float radius, int sides, float rotation = 0.0f)
        {
            if (sides < 3)
            {
                throw new ArgumentException("Regular polygon must have at least 3 sides", nameof(sides));
            }

            Vector2[] vertices = new Vector2[sides];
            float angleStep = MathHelper.TwoPi / sides;

            for (int i = 0; i < sides; i++)
            {
                float angle = i * angleStep + rotation;
                vertices[i] = center + new Vector2(
                    MathF.Cos(angle) * radius,
                    MathF.Sin(angle) * radius
                );
            }

            return new BoundingPolygon2D(vertices);
        }

        /// <summary>
        /// Creates a new <see cref="BoundingPolygon2D"/> from a bounding box.
        /// </summary>
        /// <param name="box">The bounding box to convert to a polygon.</param>
        /// <returns>
        /// A new <see cref="BoundingPolygon2D"/> with four vertices in counter-clockwise order
        /// representing the bounding box as a rectangular polygon.
        /// </returns>
        public static BoundingPolygon2D CreateFromBoundingBox2D(BoundingBox2D box)
        {
            Vector2[] vertices = new Vector2[]
            {
                new Vector2(box.Min.X, box.Min.Y),  // Top-left
                new Vector2(box.Max.X, box.Min.Y),  // Top-right
                new Vector2(box.Max.X, box.Max.Y),  // Bottom-right
                new Vector2(box.Min.X, box.Max.Y)   // Bottom-left
            };

            return new BoundingPolygon2D(vertices);
        }

        /// <summary>
        /// Creates a <see cref="BoundingPolygon2D"/> that encloses two polygons.
        /// </summary>
        /// <param name="original">The first polygon to enclose.</param>
        /// <param name="additional">The second polygon to enclose.</param>
        /// <returns>
        /// A new <see cref="BoundingPolygon2D"/> that completely contains both input polygons.
        /// </returns>
        /// <remarks>
        /// Computes the convex hull of the combined vertex sets from both polygons,
        /// producing the minimal enclosing convex polygon.
        /// </remarks>
        public static BoundingPolygon2D CreateMerged(BoundingPolygon2D original, BoundingPolygon2D additional)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 4.2.3 "Computing the Convex Hull"
            // Merges polygons by computing the convex hull of the combined vertex set.

            if (original.Vertices == null || original.Vertices.Length == 0)
            {
                return additional;
            }

            if (additional.Vertices == null || additional.Vertices.Length == 0)
            {
                return original;
            }

            // Combine all vertices
            Vector2[] allVertices = new Vector2[original.Vertices.Length + additional.Vertices.Length];
            Array.Copy(original.Vertices, 0, allVertices, 0, original.Vertices.Length);
            Array.Copy(additional.Vertices, 0, allVertices, original.Vertices.Length, additional.Vertices.Length);

            // Compute convex hull using Graham scan
            return new BoundingPolygon2D(ComputeConvexHull(allVertices));
        }

        /// <summary>
        /// Tests whether this polygon contains, intersects, or is separate from another polygon.
        /// </summary>
        /// <param name="polygon">The other polygon to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the other polygon is completely inside this one;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingPolygon2D polygon)
        {
            if (polygon.Vertices == null || polygon.Vertices.Length == 0)
            {
                return ContainmentType.Disjoint;
            }

            // Check if all vertices of the other polygon are inside this polygon
            bool allInside = true;
            for (int i = 0; i < polygon.Vertices.Length; i++)
            {
                if (Contains(polygon.Vertices[i]) != ContainmentType.Contains)
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
            if (Intersects(polygon))
            {
                return ContainmentType.Intersects;
            }

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether this polygon contains, intersects, or is separate from a circle.
        /// </summary>
        /// <param name="circle">The circle to test against.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the circle is completely inside this polygon;
        /// <see cref="ContainmentType.Intersects"/> if they partially overlap;
        /// or <see cref="ContainmentType.Disjoint"/> if they do not touch.
        /// </returns>
        public readonly ContainmentType Contains(BoundingCircle circle)
        {
            // Check if circle center is inside and all points on circle boundary are inside
            if (Contains(circle.Center) != ContainmentType.Contains)
            {
                if (Intersects(circle))
                {
                    return ContainmentType.Intersects;
                }

                return ContainmentType.Disjoint;
            }

            // Center is inside, check if circle extends beyond any edge
            float minDistance = float.MaxValue;

            for (int i = 0; i < Vertices.Length; i++)
            {
                int j = (i + 1) % Vertices.Length;
                Vector2 edge = Vertices[j] - Vertices[i];
                Vector2 toCenter = circle.Center - Vertices[i];

                // Project center onto edge
                float edgeLengthSq = edge.LengthSquared();
                float t = Math.Clamp(Vector2.Dot(toCenter, edge) / edgeLengthSq, 0.0f, 1.0f);
                Vector2 closestPoint = Vertices[i] + t * edge;

                float distance = Vector2.Distance(circle.Center, closestPoint);
                minDistance = MathF.Min(minDistance, distance);
            }

            if (minDistance >= circle.Radius)
            {
                return ContainmentType.Contains;
            }

            if (Intersects(circle))
            {
                return ContainmentType.Intersects;
            }

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether a point lies inside this polygon or on its boundary.
        /// </summary>
        /// <param name="point">The point to test in 2D space.</param>
        /// <returns>
        /// <see cref="ContainmentType.Contains"/> if the point is inside or on the boundary;
        /// otherwise, <see cref="ContainmentType.Disjoint"/> if the point is outside.
        /// </returns>
        public readonly ContainmentType Contains(Vector2 point)
        {
            if (Vertices == null || Vertices.Length < 3)
            {
                return ContainmentType.Disjoint;
            }

            // Use edge normal test
            // Point is inside if it is on the correct side of all edges
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vector2 toPoint = point - Vertices[i];
                float projection = Vector2.Dot(toPoint, Normals[i]);

                if (projection > 0.0f)
                {
                    return ContainmentType.Disjoint;
                }
            }

            return ContainmentType.Contains;
        }

        /// <summary>
        /// Tests whether this polygon intersects with a circle.
        /// </summary>
        /// <param name="circle">The circle to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the polygon and circle overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCircle circle)
        {
            return circle.Intersects(this);
        }

        /// <summary>
        /// Tests whether this polygon intersects with an axis-aligned bounding box.
        /// </summary>
        /// <param name="box">The bounding box to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the polygon and bounding box overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingBox2D box)
        {
            return box.Intersects(this);
        }

        /// <summary>
        /// Tests whether this polygon intersects with an oriented bounding box.
        /// </summary>
        /// <param name="obb">The oriented bounding box to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the polygon and box overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(OrientedBoundingBox2D obb)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.1 "Separating-axis Test"

            if (Vertices == null || VertexCount < 3)
                return false;

            // Treat OBB as a convex polygon
            Vector2[] obbCorners = obb.GetCorners();

            // Test polygon edge normals
            for (int i = 0; i < VertexCount; i++)
            {
                if (!OverlapOnAxis(this, obbCorners, Normals[i]))
                    return false;
            }

            // Test OBB local axes
            if (!OverlapOnAxis(this, obbCorners, obb.AxisX))
                return false;

            if (!OverlapOnAxis(this, obbCorners, obb.AxisY))
                return false;

            return true;
        }

        /// <summary>
        /// Tests whether this polygon intersects with capsule.
        /// </summary>
        /// <param name="capsule">The capsule to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the polygon and capsule overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingCapsule2D capsule)
        {
            return capsule.Intersects(this);
        }

        /// <summary>
        /// Tests whether this polygon intersects with another polygon.
        /// </summary>
        /// <param name="polygon">The other polygon to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the polygons overlap or touch; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Intersects(BoundingPolygon2D polygon)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 5.2.1 "Separating-axis Test"
            // Adapted for 2D convex polygon intersection using SAT

            if (Vertices == null || polygon.Vertices == null)
            {
                return false;
            }

            // Separating Axis Theorem
            // Test all edge normals from both polygons

            // Test this polygon's normals
            for (int i = 0; i < Vertices.Length; i++)
            {
                if (!OverlapOnAxis(this, polygon.Vertices, Normals[i]))
                {
                    return false;
                }
            }

            // Test other polygon's normals
            for (int i = 0; i < polygon.Vertices.Length; i++)
            {
                if (!OverlapOnAxis(this, polygon.Vertices, polygon.Normals[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Applies a matrix transformation to this polygon and creates a new transformed polygon.
        /// </summary>
        /// <param name="matrix">The transformation matrix to apply.</param>
        /// <returns>
        /// A new <see cref="BoundingPolygon2D"/> with vertices transformed by the matrix
        /// and edge normals recomputed from the transformed geometry.
        /// </returns>
        public readonly BoundingPolygon2D Transform(Matrix matrix)
        {
            if (Vertices == null || Vertices.Length == 0)
            {
                return this;
            }

            Vector2[] transformedVertices = new Vector2[Vertices.Length];

            for (int i = 0; i < Vertices.Length; i++)
            {
                transformedVertices[i] = Vector2.Transform(Vertices[i], matrix);
            }

            return new BoundingPolygon2D(transformedVertices);
        }

        /// <summary>
        /// Creates a new <see cref="BoundingPolygon2D"/> by translating this polygon by the specified offset.
        /// </summary>
        /// <param name="translation">The offset to translate the polygon by in 2D space.</param>
        /// <returns>
        /// A new <see cref="BoundingPolygon2D"/> at the translated position with the same shape and orientation.
        /// </returns>
        public readonly BoundingPolygon2D Translate(Vector2 translation)
        {
            if (Vertices == null || Vertices.Length == 0)
            {
                return this;
            }

            Vector2[] translatedVertices = new Vector2[Vertices.Length];

            for (int i = 0; i < Vertices.Length; i++)
            {
                translatedVertices[i] = Vertices[i] + translation;
            }

            // Normals don't change with translation
            return new BoundingPolygon2D(translatedVertices, Normals);
        }

        /// <summary>
        /// Deconstructs this polygon into its component arrays.
        /// </summary>
        /// <param name="vertices">
        /// When this method returns, contains the vertices of this polygon in counter-clockwise order in 2D space.
        /// </param>
        /// <param name="normals">
        /// When this method returns, contains the outward-facing unit normals for each edge of this polygon.
        /// </param>
        public readonly void Deconstruct(out Vector2[] vertices, out Vector2[] normals)
        {
            vertices = Vertices;
            normals = Normals;
        }

        /// <inheritdoc/>
        public readonly bool Equals(BoundingPolygon2D other)
        {
            if (VertexCount != other.VertexCount)
            {
                return false;
            }

            if (Vertices == null && other.Vertices == null)
            {
                return true;
            }

            if (Vertices == null || other.Vertices == null)
            {
                return false;
            }

            for (int i = 0; i < VertexCount; i++)
            {
                if (!Vertices[i].Equals(other.Vertices[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override readonly bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is BoundingPolygon2D other && Equals(other);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            if (Vertices == null || Vertices.Length == 0)
            {
                return 0;
            }

            HashCode hash = new HashCode();
            for (int i = 0; i < Vertices.Length; i++)
            {
                hash.Add(Vertices[i]);
            }

            return hash.ToHashCode();
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{{Vertices:{VertexCount} Area:{Area:F2}}}";
        }

        /// <summary/>
        public static bool operator ==(BoundingPolygon2D left, BoundingPolygon2D right)
        {
            return left.Equals(right);
        }

        /// <summary/>
        public static bool operator !=(BoundingPolygon2D left, BoundingPolygon2D right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region Private Helper Methods

        private static Vector2[] ComputeConvexHull(Vector2[] points)
        {
            // C. Ericson, Real-Time Collision Detection, Morgan Kaufmann, 2005
            // Section 4.2.3 "Computing the Convex Hull"
            // Graham scan algorithm adapted for 2D Vector2 input

            const float Epsilon = 1e-6f;

            if (points.Length < 3)
                return points;

            // Find the point with the lowest Y coordinate
            // and if tied, the lowest x coordinate as well
            int minIndex = 0;
            for (int i = 1; i < points.Length; i++)
            {
                if (points[i].Y < points[minIndex].Y ||
                   (points[i].Y == points[minIndex].Y && points[i].X < points[minIndex].X))
                {
                    minIndex = i;
                }
            }

            Vector2 pivot = points[minIndex];

            // Sort points by polar angle with respect to pivot
            Vector2[] sorted = new Vector2[points.Length];
            Array.Copy(points, sorted, points.Length);

            Array.Sort(sorted, (a, b) =>
            {
                if (a.Equals(pivot)) return -1;
                if (b.Equals(pivot)) return 1;

                float angleA = MathF.Atan2(a.Y - pivot.Y, a.X - pivot.X);
                float angleB = MathF.Atan2(b.Y - pivot.Y, b.X - pivot.X);

                if (MathF.Abs(angleA - angleB) < Epsilon)
                {
                    // Same angle, closer point comes first
                    float distA = Vector2.DistanceSquared(pivot, a);
                    float distB = Vector2.DistanceSquared(pivot, b);
                    return distA.CompareTo(distB);
                }

                return angleA.CompareTo(angleB);
            });

            // Graham scan
            Vector2[] hull = new Vector2[sorted.Length];
            int hullSize = 0;

            for (int i = 0; i < sorted.Length; i++)
            {
                // Remove points that make a right turn
                while (hullSize >= 2 && Orientation(hull[hullSize - 2], hull[hullSize - 1], sorted[i]) <= 0)
                {
                    hullSize--;
                }

                hull[hullSize++] = sorted[i];
            }

            // Resize the actual hull size
            Vector2[] result = new Vector2[hullSize];
            Array.Copy(hull, result, hullSize);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Orientation(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        private static Vector2[] ComputeNormals(Vector2[] vertices)
        {
            const float Epsilon = 1e-6f;

            int n = vertices.Length;
            Vector2[] normals = new Vector2[n];

            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                Vector2 edge = vertices[j] - vertices[i];

                // Perpendicular to edge (rotated 90 degrees clockwise for outward normal)
                normals[i] = new Vector2(edge.Y, -edge.X);

                // Normalize
                float length = normals[i].Length();
                if (length > Epsilon)
                {
                    normals[i] /= length;
                }
            }

            return normals;
        }

        private static bool OverlapOnAxis(BoundingPolygon2D polygon, Vector2[] otherVerts, Vector2 axis)
        {
            float min1 = float.MaxValue;
            float max1 = float.MinValue;
            float min2 = float.MaxValue;
            float max2 = float.MinValue;

            for (int i = 0; i < polygon.VertexCount; i++)
            {
                float p = Vector2.Dot(polygon.Vertices[i], axis);
                min1 = MathF.Min(min1, p);
                max1 = MathF.Max(max1, p);
            }

            for (int i = 0; i < otherVerts.Length; i++)
            {
                float p = Vector2.Dot(otherVerts[i], axis);
                min2 = MathF.Min(min2, p);
                max2 = MathF.Max(max2, p);
            }

            return !(max1 < min2 || max2 < min1);
        }

        #endregion
    }
}
