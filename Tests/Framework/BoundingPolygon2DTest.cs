// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class BoundingPolygon2DTest
    {
        #region Constructor Tests

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
                new Vector2(10, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(-0.707f, 0.707f)
            };

            var polygon = new BoundingPolygon2D(vertices, normals);

            Assert.AreEqual(3, polygon.VertexCount);
        }

        [Test]
        public void Constructor_WithNormals_ThrowsWhenNullVertices()
        {
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1)
            };

            Assert.Throws<ArgumentNullException>(() => new BoundingPolygon2D(null, normals));
        }

        [Test]
        public void Constructor_WithNormals_ThrowsWhenNullNormals()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10)
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
                new Vector2(10, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0)
            };

            Assert.Throws<ArgumentException>(() => new BoundingPolygon2D(vertices, normals));
        }

        #endregion

        #region Computed Property Tests

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

            Assert.AreEqual(4, polygon.VertexCount);
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

            Assert.AreEqual(5, centroid.X, Collision2D.Epsilon);
            Assert.AreEqual(10.0f / 3.0f, centroid.Y, Collision2D.Epsilon);
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

            Assert.AreEqual(5, centroid.X, Collision2D.Epsilon);
            Assert.AreEqual(5, centroid.Y, Collision2D.Epsilon);
        }

        [Test]
        public void Area_TriangleReturnsCorrectValue()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(5, 10)
            };

            var polygon = new BoundingPolygon2D(vertices);

            float area = polygon.Area;

            Assert.AreEqual(50.0f, area, Collision2D.Epsilon);
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

            Assert.AreEqual(100.0f, area, Collision2D.Epsilon);
        }

        #endregion

        #region Factory Method Tests

        [Test]
        public void CreateFromVertices()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10)
            };

            var polygon = BoundingPolygon2D.CreateFromVertices(vertices);

            Assert.AreEqual(3, polygon.VertexCount);
        }

        [Test]
        public void CreateRegular_Triangle()
        {
            var center = new Vector2(5, 5);
            var radius = 10.0f;
            var sides = 3;

            var polygon = BoundingPolygon2D.CreateRegular(center, radius, sides);

            Assert.AreEqual(3, polygon.VertexCount);

            for (int i = 0; i < 3; i++)
            {
                float distance = Vector2.Distance(center, polygon.Vertices[i]);
                Assert.AreEqual(radius, distance, Collision2D.Epsilon);
            }
        }

        [Test]
        public void CreateRegular_Square()
        {
            var center = new Vector2(0, 0);
            var radius = 10.0f;
            var sides = 4;

            var polygon = BoundingPolygon2D.CreateRegular(center, radius, sides);

            Assert.AreEqual(4, polygon.VertexCount);
        }

        [Test]
        public void CreateRegular_Hexagon()
        {
            var center = new Vector2(0, 0);
            var radius = 10.0f;
            var sides = 6;

            var polygon = BoundingPolygon2D.CreateRegular(center, radius, sides);

            Assert.AreEqual(6, polygon.VertexCount);
        }

        [Test]
        public void CreateRegular_WithRotation()
        {
            var center = new Vector2(0, 0);
            var radius = 10.0f;
            var sides = 4;
            var rotation = MathHelper.PiOver4;

            var polygon = BoundingPolygon2D.CreateRegular(center, radius, sides, rotation);

            Assert.AreEqual(4, polygon.VertexCount);

            float angle = MathHelper.PiOver4;
            float expectedX = radius * MathF.Cos(angle);
            float expectedY = radius * MathF.Sin(angle);

            Assert.AreEqual(expectedX, polygon.Vertices[0].X, Collision2D.Epsilon);
            Assert.AreEqual(expectedY, polygon.Vertices[0].Y, Collision2D.Epsilon);
        }

        [Test]
        public void CreateRegular_ThrowsWhenTooFewSides()
        {
            var center = new Vector2(0, 0);
            var radius = 10.0f;
            var sides = 2;

            Assert.Throws<ArgumentException>(() => BoundingPolygon2D.CreateRegular(center, radius, sides));
        }

        [Test]
        public void CreateFromBoundingBox2D()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            var polygon = BoundingPolygon2D.CreateFromBoundingBox2D(box);

            Assert.AreEqual(4, polygon.VertexCount);

            Assert.Contains(new Vector2(0, 0), polygon.Vertices);
            Assert.Contains(new Vector2(10, 0), polygon.Vertices);
            Assert.Contains(new Vector2(10, 10), polygon.Vertices);
            Assert.Contains(new Vector2(0, 10), polygon.Vertices);
        }

        [Test]
        public void CreateMerged_NonOverlapping()
        {
            var vertices1 = new[]
            {
                new Vector2(0, 0),
                new Vector2(5, 0),
                new Vector2(5, 5),
                new Vector2(0, 5)
            };
            var polygon1 = new BoundingPolygon2D(vertices1);

            var vertices2 = new[]
            {
                new Vector2(10, 10),
                new Vector2(15, 10),
                new Vector2(15, 15),
                new Vector2(10, 15)
            };
            var polygon2 = new BoundingPolygon2D(vertices2);

            var merged = BoundingPolygon2D.CreateMerged(polygon1, polygon2);

            Assert.AreEqual(ContainmentType.Contains, merged.Contains(polygon1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(polygon2));
        }

        [Test]
        public void CreateMerged_Overlapping()
        {
            var vertices1 = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var polygon1 = new BoundingPolygon2D(vertices1);

            var vertices2 = new[]
            {
                new Vector2(5, 5),
                new Vector2(15, 5),
                new Vector2(15, 15),
                new Vector2(5, 15)
            };
            var polygon2 = new BoundingPolygon2D(vertices2);

            var merged = BoundingPolygon2D.CreateMerged(polygon1, polygon2);

            Assert.AreEqual(ContainmentType.Contains, merged.Contains(polygon1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(polygon2));
        }

        #endregion

        #region Transform Tests

        [Test]
        public void Transform_Translation()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);
            var matrix = Matrix.CreateTranslation(5, 10, 0);

            var transformed = polygon.Transform(matrix);

            Assert.AreEqual(new Vector2(5, 10), transformed.Vertices[0]);
            Assert.AreEqual(new Vector2(15, 10), transformed.Vertices[1]);
            Assert.AreEqual(new Vector2(15, 20), transformed.Vertices[2]);
        }

        [Test]
        public void Transform_UniformScale()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);
            var matrix = Matrix.CreateScale(2.0f);

            var transformed = polygon.Transform(matrix);

            Assert.AreEqual(new Vector2(0, 0), transformed.Vertices[0]);
            Assert.AreEqual(new Vector2(20, 0), transformed.Vertices[1]);
            Assert.AreEqual(new Vector2(20, 20), transformed.Vertices[2]);
        }

        [Test]
        public void Transform_Rotation()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(0, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);
            var matrix = Matrix.CreateRotationZ(MathHelper.PiOver2);

            var transformed = polygon.Transform(matrix);

            Assert.AreEqual(0, transformed.Vertices[0].X, Collision2D.Epsilon);
            Assert.AreEqual(0, transformed.Vertices[0].Y, Collision2D.Epsilon);
            Assert.AreEqual(0, transformed.Vertices[1].X, Collision2D.Epsilon);
            Assert.AreEqual(10, transformed.Vertices[1].Y, Collision2D.Epsilon);
            Assert.AreEqual(-10, transformed.Vertices[2].X, Collision2D.Epsilon);
            Assert.AreEqual(0, transformed.Vertices[2].Y, Collision2D.Epsilon);
        }

        [Test]
        public void Translate_OffsetsPosition()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);
            var offset = new Vector2(5, 10);

            var translated = polygon.Translate(offset);

            Assert.AreEqual(new Vector2(5, 10), translated.Vertices[0]);
            Assert.AreEqual(new Vector2(15, 10), translated.Vertices[1]);
            Assert.AreEqual(new Vector2(15, 20), translated.Vertices[2]);
        }

        #endregion

        #region ContainsPoint Tests (Delegation Spot Check)

        [Test]
        public void ContainsPoint_Inside()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);
            var point = new Vector2(5, 5);

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
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);
            var point = new Vector2(15, 5);

            var result = polygon.Contains(point);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        #endregion

        #region Deconstruct Test

        [Test]
        public void Deconstruct()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10)
            };
            var polygon = new BoundingPolygon2D(vertices);

            var (v, n) = polygon;

            Assert.AreEqual(vertices, v);
            Assert.IsNotNull(n);
        }

        #endregion
    }
}
