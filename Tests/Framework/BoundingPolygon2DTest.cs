// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using NUnit.Framework;
using System;

namespace MonoGame.Tests.Framework
{
    class BoundingPolygon2DTest
    {
        [Test]
        public void Constructor_Triangle()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(5, 10)
            };

            var polygon = new BoundingPolygon2D(vertices);

            Assert.AreEqual(3, polygon.VertexCount);
            Assert.AreEqual(vertices, polygon.Vertices);
            Assert.IsNotNull(polygon.Normals);
            Assert.AreEqual(3, polygon.Normals.Length);
        }

        [Test]
        public void Constructor_Square()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };

            var polygon = new BoundingPolygon2D(vertices);

            Assert.AreEqual(4, polygon.VertexCount);
        }

        [Test]
        public void Constructor_ThrowsWhenNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BoundingPolygon2D(null));
        }

        [Test]
        public void Constructor_ThrowsWhenTooFewVertices()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0)
            };

            Assert.Throws<ArgumentException>(() => new BoundingPolygon2D(vertices));
        }

        [Test]
        public void Constructor_WithNormals()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(5, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(0.707f, 0.707f),
                new Vector2(-0.707f, 0.707f)
            };

            var polygon = new BoundingPolygon2D(vertices, normals);

            Assert.AreEqual(vertices, polygon.Vertices);
            Assert.AreEqual(normals, polygon.Normals);
        }

        [Test]
        public void Constructor_WithNormals_ThrowsWhenNullVertices()
        {
            var normals = new[] { Vector2.UnitX };
            Assert.Throws<ArgumentNullException>(() => new BoundingPolygon2D(null, normals));
        }

        [Test]
        public void Constructor_WithNormals_ThrowsWhenNullNormals()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(5, 10)
            };
            Assert.Throws<ArgumentNullException>(() => new BoundingPolygon2D(vertices, null));
        }

        [Test]
        public void Constructor_WithNormals_ThrowsWhenLengthMismatch()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(5, 10)
            };
            var normals = new[]
            {
                Vector2.UnitX,
                Vector2.UnitY
            };

            Assert.Throws<ArgumentException>(() => new BoundingPolygon2D(vertices, normals));
        }

        [Test]
        public void VertexCount_ReturnsCorrectCount()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);

            int count = polygon.VertexCount;

            Assert.AreEqual(4, count);
        }

        [Test]
        public void Centroid_TriangleReturnsCorrectValue()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(5, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);

            var centroid = polygon.Centroid;

            Assert.AreEqual(5.0f, centroid.X, 1e-5f);
            Assert.AreEqual(10.0f / 3.0f, centroid.Y, 1e-5f);
        }

        [Test]
        public void Centroid_SquareReturnsCenter()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);

            var centroid = polygon.Centroid;

            Assert.AreEqual(new Vector2(5, 5), centroid);
        }

        [Test]
        public void Area_TriangleReturnsCorrectValue()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(0, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);

            float area = polygon.Area;

            Assert.AreEqual(50.0f, area, 1e-5f);
        }

        [Test]
        public void Area_SquareReturnsCorrectValue()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);

            float area = polygon.Area;

            Assert.AreEqual(100.0f, area, 1e-5f);
        }

        [Test]
        public void CreateFromVertices()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(5, 10)
            };

            var polygon = BoundingPolygon2D.CreateFromVertices(vertices);

            Assert.AreEqual(3, polygon.VertexCount);
            Assert.AreEqual(vertices, polygon.Vertices);
        }

        [Test]
        public void CreateRegular_Triangle()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 10, 3);

            Assert.AreEqual(3, polygon.VertexCount);
        }

        [Test]
        public void CreateRegular_Square()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 10, 4);

            Assert.AreEqual(4, polygon.VertexCount);
        }

        [Test]
        public void CreateRegular_Hexagon()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 10, 6);

            Assert.AreEqual(6, polygon.VertexCount);
        }

        [Test]
        public void CreateRegular_WithRotation()
        {
            var polygon = BoundingPolygon2D.CreateRegular(new Vector2(5, 5), 10, 4, MathHelper.PiOver4);

            Assert.AreEqual(4, polygon.VertexCount);
            Assert.AreEqual(new Vector2(5, 5), polygon.Centroid);
        }

        [Test]
        public void CreateRegular_ThrowsWhenTooFewSides()
        {
            Assert.Throws<ArgumentException>(() => BoundingPolygon2D.CreateRegular(Vector2.Zero, 10, 2));
        }

        [Test]
        public void CreateFromBoundingBox2D()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));

            var polygon = BoundingPolygon2D.CreateFromBoundingBox2D(box);

            Assert.AreEqual(4, polygon.VertexCount);
            Assert.AreEqual(new Vector2(5, 10), polygon.Centroid);
            Assert.AreEqual(200.0f, polygon.Area, 1e-5f);
        }

        [Test]
        public void CreateMerged_NonOverlapping()
        {
            var poly1 = BoundingPolygon2D.CreateRegular(new Vector2(0, 0), 5, 4);
            var poly2 = BoundingPolygon2D.CreateRegular(new Vector2(20, 0), 5, 4);

            var merged = BoundingPolygon2D.CreateMerged(poly1, poly2);

            // Should contain both polygons
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(poly1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(poly2));
        }

        [Test]
        public void CreateMerged_Overlapping()
        {
            var poly1 = BoundingPolygon2D.CreateRegular(new Vector2(0, 0), 5, 4);
            var poly2 = BoundingPolygon2D.CreateRegular(new Vector2(5, 0), 5, 4);

            var merged = BoundingPolygon2D.CreateMerged(poly1, poly2);

            // Should contain both polygons
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(poly1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(poly2));
        }

        [Test]
        public void ContainsPolygon_Contains()
        {
            var outer = BoundingPolygon2D.CreateRegular(Vector2.Zero, 10, 6);
            var inner = BoundingPolygon2D.CreateRegular(Vector2.Zero, 3, 4);

            var result = outer.Contains(inner);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPolygon_Intersects()
        {
            var poly1 = BoundingPolygon2D.CreateRegular(new Vector2(0, 0), 5, 4);
            var poly2 = BoundingPolygon2D.CreateRegular(new Vector2(7, 0), 5, 4);

            var result = poly1.Contains(poly2);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        [Test]
        public void ContainsPolygon_Disjoint()
        {
            var poly1 = BoundingPolygon2D.CreateRegular(new Vector2(0, 0), 5, 4);
            var poly2 = BoundingPolygon2D.CreateRegular(new Vector2(20, 0), 5, 4);

            var result = poly1.Contains(poly2);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsCircle_Contains()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 10, 6);
            var circle = new BoundingCircle(Vector2.Zero, 3);

            var result = polygon.Contains(circle);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsCircle_Intersects()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 10, 6);
            var circle = new BoundingCircle(new Vector2(8, 0), 5);

            var result = polygon.Contains(circle);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        [Test]
        public void ContainsCircle_Disjoint()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 5, 4);
            var circle = new BoundingCircle(new Vector2(20, 0), 3);

            var result = polygon.Contains(circle);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsPoint_Inside()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 10, 4);
            var point = new Vector2(3, 3);

            var result = polygon.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_OnBoundary()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);
            var point = new Vector2(10, 5);

            var result = polygon.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_Outside()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 5, 4);
            var point = new Vector2(20, 20);

            var result = polygon.Contains(point);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void IntersectsPolygon_Overlapping()
        {
            var poly1 = BoundingPolygon2D.CreateRegular(new Vector2(0, 0), 5, 4);
            var poly2 = BoundingPolygon2D.CreateRegular(new Vector2(7, 0), 5, 4);

            bool intersects = poly1.Intersects(poly2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsPolygon_Separated()
        {
            var poly1 = BoundingPolygon2D.CreateRegular(new Vector2(0, 0), 5, 4);
            var poly2 = BoundingPolygon2D.CreateRegular(new Vector2(20, 0), 5, 4);

            bool intersects = poly1.Intersects(poly2);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsPolygon_Touching()
        {
            var poly1 = BoundingPolygon2D.CreateRegular(new Vector2(0, 0), 5, 4);
            var poly2 = BoundingPolygon2D.CreateRegular(new Vector2(10, 0), 5, 4);

            bool intersects = poly1.Intersects(poly2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsCircle_Overlapping()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 10, 6);
            var circle = new BoundingCircle(new Vector2(8, 0), 5);

            bool intersects = polygon.Intersects(circle);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsCircle_Separated()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 5, 4);
            var circle = new BoundingCircle(new Vector2(20, 0), 3);

            bool intersects = polygon.Intersects(circle);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBox_Overlapping()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 10, 6);
            var box = new BoundingBox2D(new Vector2(-5, -5), new Vector2(5, 5));

            bool intersects = polygon.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBox_Separated()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 5, 4);
            var box = new BoundingBox2D(new Vector2(20, 20), new Vector2(30, 30));

            bool intersects = polygon.Intersects(box);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsOBB_Overlapping()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 10, 6);
            var obb = new OrientedBoundingBox2D(new Vector2(5, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));

            bool intersects = polygon.Intersects(obb);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsOBB_Separated()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 5, 4);
            var obb = new OrientedBoundingBox2D(new Vector2(20, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));

            bool intersects = polygon.Intersects(obb);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsCapsule_Overlapping()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 10, 6);
            var capsule = new BoundingCapsule2D(new Vector2(-5, 0), new Vector2(5, 0), 3);

            bool intersects = polygon.Intersects(capsule);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsCapsule_Separated()
        {
            var polygon = BoundingPolygon2D.CreateRegular(Vector2.Zero, 5, 4);
            var capsule = new BoundingCapsule2D(new Vector2(20, 0), new Vector2(30, 0), 3);

            bool intersects = polygon.Intersects(capsule);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void Transform_Translation()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);
            var matrix = Matrix.CreateTranslation(5, 10, 0);

            var transformed = polygon.Transform(matrix);

            Assert.AreEqual(new Vector2(10, 15), transformed.Centroid);
        }

        [Test]
        public void Transform_UniformScale()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);
            var matrix = Matrix.CreateScale(2, 2, 1);

            var transformed = polygon.Transform(matrix);

            Assert.AreEqual(new Vector2(10, 10), transformed.Centroid);
            Assert.AreEqual(400.0f, transformed.Area, 1e-5f);
        }

        [Test]
        public void Transform_Rotation()
        {
            var vertices = new[]
            {
                new Vector2(10, 0),
                new Vector2(20, 0),
                new Vector2(20, 10),
                new Vector2(10, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);
            var matrix = Matrix.CreateRotationZ(MathHelper.PiOver2);

            var transformed = polygon.Transform(matrix);

            Assert.AreEqual(-5.0f, transformed.Centroid.X, 1e-5f);
            Assert.AreEqual(15.0f, transformed.Centroid.Y, 1e-5f);
        }

        [Test]
        public void Translate_OffsetsPosition()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);
            var translation = new Vector2(5, -3);

            var translated = polygon.Translate(translation);

            Assert.AreEqual(new Vector2(10, 2), translated.Centroid);
            Assert.AreEqual(100.0f, translated.Area, 1e-5f);
        }

        [Test]
        public void Deconstruct()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(5, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);

            polygon.Deconstruct(out Vector2[] outVertices, out Vector2[] outNormals);

            Assert.AreEqual(polygon.Vertices, outVertices);
            Assert.AreEqual(polygon.Normals, outNormals);
        }
    }
}
