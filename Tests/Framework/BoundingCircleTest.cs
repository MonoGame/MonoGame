// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class BoundingCircleTest
    {
        #region Constructor Tests

        [Test]
        public void Constructor()
        {
            var center = new Vector2(5, 10);
            var radius = 15.0f;

            var circle = new BoundingCircle(center, radius);

            Assert.AreEqual(center, circle.Center);
            Assert.AreEqual(radius, circle.Radius);
        }

        #endregion

        #region Computed Property Tests

        [Test]
        public void RadiusSquared_ReturnsSquaredValue()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5.0f);

            float radiusSquared = circle.RadiusSquared;

            Assert.AreEqual(25.0f, radiusSquared, Collision2D.Epsilon);
        }

        [Test]
        public void Diameter_ReturnsTwiceRadius()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5.0f);

            float diameter = circle.Diameter;

            Assert.AreEqual(10.0f, diameter, Collision2D.Epsilon);
        }

        [Test]
        public void Area_ReturnsPiTimesRadiusSquared()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5.0f);

            float area = circle.Area;

            Assert.AreEqual(MathF.PI * 25.0f, area, Collision2D.Epsilon);
        }

        #endregion

        #region Factory Method Tests

        [Test]
        public void CreateFromPoints_SinglePoint()
        {
            var points = new[] { new Vector2(5, 5) };

            var circle = BoundingCircle.CreateFromPoints(points);

            Assert.AreEqual(new Vector2(5, 5), circle.Center);
            Assert.AreEqual(0.0f, circle.Radius, Collision2D.Epsilon);
        }

        [Test]
        public void CreateFromPoints_TwoPoints()
        {
            var points = new[] { new Vector2(0, 0), new Vector2(10, 0) };

            var circle = BoundingCircle.CreateFromPoints(points);

            Assert.AreEqual(new Vector2(5, 0), circle.Center);
            Assert.AreEqual(5.0f, circle.Radius, Collision2D.Epsilon);
        }

        [Test]
        public void CreateFromPoints_MultiplePoints()
        {
            var points = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(0, 10),
                new Vector2(10, 10)
            };

            var circle = BoundingCircle.CreateFromPoints(points);

            foreach (var point in points)
            {
                float distance = Vector2.Distance(circle.Center, point);
                Assert.LessOrEqual(distance, circle.Radius + Collision2D.Epsilon);
            }
        }

        [Test]
        public void CreateFromPoints_ThrowsWhenNull()
        {
            Assert.Throws<ArgumentNullException>(() => BoundingCircle.CreateFromPoints(null));
        }

        [Test]
        public void CreateFromPoints_ThrowsWhenEmpty()
        {
            Assert.Throws<ArgumentException>(() => BoundingCircle.CreateFromPoints(new Vector2[0]));
        }

        [Test]
        public void CreateFromBoundingBox2D()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            var circle = BoundingCircle.CreateFromBoundingBox2D(box);

            Assert.IsTrue(circle.Contains(new Vector2(0, 0)) != ContainmentType.Disjoint);
            Assert.IsTrue(circle.Contains(new Vector2(10, 10)) != ContainmentType.Disjoint);
        }

        [Test]
        public void CreateFromBoundingCapsule2D_HorizontalCapsule()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 5), new Vector2(10, 5), 3.0f);

            var circle = BoundingCircle.CreateFromBoundingCapsule2D(capsule);

            Assert.IsTrue(circle.Contains(new Vector2(-3, 5)) != ContainmentType.Disjoint);
            Assert.IsTrue(circle.Contains(new Vector2(13, 5)) != ContainmentType.Disjoint);
        }

        [Test]
        public void CreateFromBoundingCapsule2D_VerticalCapsule()
        {
            var capsule = new BoundingCapsule2D(new Vector2(5, 0), new Vector2(5, 10), 3.0f);

            var circle = BoundingCircle.CreateFromBoundingCapsule2D(capsule);

            Assert.IsTrue(circle.Contains(new Vector2(5, -3)) != ContainmentType.Disjoint);
            Assert.IsTrue(circle.Contains(new Vector2(5, 13)) != ContainmentType.Disjoint);
        }

        [Test]
        public void CreateFromBoundingCapsule2D_DegenerateCapsule()
        {
            var capsule = new BoundingCapsule2D(new Vector2(5, 5), new Vector2(5, 5), 3.0f);

            var circle = BoundingCircle.CreateFromBoundingCapsule2D(capsule);

            Assert.AreEqual(new Vector2(5, 5), circle.Center);
            Assert.AreEqual(3.0f, circle.Radius, Collision2D.Epsilon);
        }

        [Test]
        public void CreateFromBoundingCapsule2D_ContainsOriginal()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 10), 2.0f);

            var circle = BoundingCircle.CreateFromBoundingCapsule2D(capsule);

            Assert.AreEqual(ContainmentType.Contains, circle.Contains(capsule));
        }

        [Test]
        public void CreateMerged_NonOverlapping()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 5.0f);
            var circle2 = new BoundingCircle(new Vector2(20, 0), 5.0f);

            var merged = BoundingCircle.CreateMerged(circle1, circle2);

            Assert.AreEqual(ContainmentType.Contains, merged.Contains(circle1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(circle2));
        }

        [Test]
        public void CreateMerged_OneContainsOther()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 10.0f);
            var circle2 = new BoundingCircle(new Vector2(2, 2), 3.0f);

            var merged = BoundingCircle.CreateMerged(circle1, circle2);

            Assert.AreEqual(circle1.Center, merged.Center);
            Assert.AreEqual(circle1.Radius, merged.Radius, 0.1f);
        }

        [Test]
        public void CreateMerged_PartiallyOverlapping()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 5.0f);
            var circle2 = new BoundingCircle(new Vector2(8, 0), 5.0f);

            var merged = BoundingCircle.CreateMerged(circle1, circle2);

            Assert.AreEqual(ContainmentType.Contains, merged.Contains(circle1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(circle2));
        }

        #endregion

        #region Transform Tests

        [Test]
        public void Transform_Translation()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5.0f);
            var matrix = Matrix.CreateTranslation(10, 20, 0);

            var transformed = circle.Transform(matrix);

            Assert.AreEqual(new Vector2(10, 20), transformed.Center);
            Assert.AreEqual(5.0f, transformed.Radius, Collision2D.Epsilon);
        }

        [Test]
        public void Transform_UniformScale()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5.0f);
            var matrix = Matrix.CreateScale(2.0f);

            var transformed = circle.Transform(matrix);

            Assert.AreEqual(new Vector2(0, 0), transformed.Center);
            Assert.AreEqual(10.0f, transformed.Radius, Collision2D.Epsilon);
        }

        [Test]
        public void Transform_NonUniformScale()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5.0f);
            var matrix = Matrix.CreateScale(2.0f, 3.0f, 1.0f);

            var transformed = circle.Transform(matrix);

            Assert.GreaterOrEqual(transformed.Radius, 10.0f);
        }

        [Test]
        public void Transform_Rotation()
        {
            var circle = new BoundingCircle(new Vector2(5, 0), 3.0f);
            var matrix = Matrix.CreateRotationZ(MathHelper.PiOver2);

            var transformed = circle.Transform(matrix);

            Assert.AreEqual(0, transformed.Center.X, Collision2D.Epsilon);
            Assert.AreEqual(5, transformed.Center.Y, Collision2D.Epsilon);
            Assert.AreEqual(3.0f, transformed.Radius, Collision2D.Epsilon);
        }

        [Test]
        public void Translate_OffsetsPosition()
        {
            var circle = new BoundingCircle(new Vector2(5, 5), 3.0f);
            var offset = new Vector2(10, 15);

            var translated = circle.Translate(offset);

            Assert.AreEqual(new Vector2(15, 20), translated.Center);
            Assert.AreEqual(3.0f, translated.Radius, Collision2D.Epsilon);
        }

        #endregion

        #region ContainsPoint Tests (Delegation Spot Check)

        [Test]
        public void ContainsPoint_Inside()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 10.0f);
            var point = new Vector2(5, 0);

            var result = circle.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_OnBoundary()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 10.0f);
            var point = new Vector2(10, 0);

            var result = circle.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_Outside()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 10.0f);
            var point = new Vector2(15, 0);

            var result = circle.Contains(point);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        #endregion

        #region Deconstruct Test

        [Test]
        public void Deconstruct()
        {
            var circle = new BoundingCircle(new Vector2(5, 10), 15.0f);

            var (center, radius) = circle;

            Assert.AreEqual(new Vector2(5, 10), center);
            Assert.AreEqual(15.0f, radius);
        }

        #endregion
    }
}
