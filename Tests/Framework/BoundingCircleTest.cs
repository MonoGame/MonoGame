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
        [Test]
        public void Constructor()
        {
            var center = new Vector2(10, 20);
            var radius = 5.0f;

            var circle = new BoundingCircle(center, radius);

            Assert.AreEqual(center, circle.Center);
            Assert.AreEqual(radius, circle.Radius);
        }


        [Test]
        public void RadiusSquared_ReturnsSquaredValue()
        {
            var circle = new BoundingCircle(Vector2.Zero, 5.0f);

            float radiusSquared = circle.RadiusSquared;

            Assert.AreEqual(25.0f, radiusSquared);
        }

        [Test]
        public void Diameter_ReturnsTwiceRadius()
        {
            var circle = new BoundingCircle(Vector2.Zero, 5.0f);

            float diameter = circle.Diameter;

            Assert.AreEqual(10.0f, diameter);
        }

        [Test]
        public void Area_ReturnsPiTimesRadiusSquared()
        {
            var circle = new BoundingCircle(Vector2.Zero, 5.0f);

            float area = circle.Area;

            Assert.AreEqual(MathF.PI * 25.0f, area, 1e-5f);
        }

        [Test]
        public void CreateFromPoints_SinglePoint()
        {
            var points = new[] { new Vector2(5, 10) };

            var circle = BoundingCircle.CreateFromPoints(points);

            Assert.AreEqual(new Vector2(5, 10), circle.Center);
            Assert.AreEqual(0.0f, circle.Radius);
        }

        [Test]
        public void CreateFromPoints_TwoPoints()
        {
            var points = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0)
            };

            var circle = BoundingCircle.CreateFromPoints(points);

            Assert.AreEqual(new Vector2(5, 0), circle.Center);
            Assert.AreEqual(5.0f, circle.Radius, 1e-5f);
        }

        [Test]
        public void CreateFromPoints_MultiplePoints()
        {
            var points = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };

            var circle = BoundingCircle.CreateFromPoints(points);

            // Should contain all points
            foreach (var point in points)
            {
                float distSq = Vector2.DistanceSquared(point, circle.Center);
                Assert.LessOrEqual(distSq, circle.RadiusSquared + 1e-5f);
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

            Assert.AreEqual(new Vector2(5, 5), circle.Center);

            // Radius should be half the diagonal: sqrt(5^2 + 5^2) = sqrt(50)
            Assert.AreEqual(MathF.Sqrt(50), circle.Radius, 1e-5f);
        }

[Test]
        public void CreateFromBoundingCapsule2D_HorizontalCapsule()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 3);

            var circle = BoundingCircle.CreateFromBoundingCapsule2D(capsule);

            Assert.AreEqual(new Vector2(5, 0), circle.Center);
            // Radius = (length / 2) + radius = 5 + 3 = 8
            Assert.AreEqual(8.0f, circle.Radius, 1e-5f);
        }

        [Test]
        public void CreateFromBoundingCapsule2D_VerticalCapsule()
        {
            var capsule = new BoundingCapsule2D(new Vector2(5, 0), new Vector2(5, 20), 4);

            var circle = BoundingCircle.CreateFromBoundingCapsule2D(capsule);

            Assert.AreEqual(new Vector2(5, 10), circle.Center);
            // Radius = (length / 2) + radius = 10 + 4 = 14
            Assert.AreEqual(14.0f, circle.Radius, 1e-5f);
        }

        [Test]
        public void CreateFromBoundingCapsule2D_DegenerateCapsule()
        {
            var capsule = new BoundingCapsule2D(new Vector2(5, 5), new Vector2(5, 5), 7);

            var circle = BoundingCircle.CreateFromBoundingCapsule2D(capsule);

            Assert.AreEqual(new Vector2(5, 5), circle.Center);
            // Radius = (length / 2) + radius = 0 + 7 = 7
            Assert.AreEqual(7.0f, circle.Radius, 1e-5f);
        }

        [Test]
        public void CreateFromBoundingCapsule2D_ContainsOriginal()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 10), 5);

            var circle = BoundingCircle.CreateFromBoundingCapsule2D(capsule);

            // Verify that all points on the capsule are within the bounding circle
            // Test endpoints + radius
            float distToA = Vector2.Distance(circle.Center, capsule.PointA);
            float distToB = Vector2.Distance(circle.Center, capsule.PointB);
            Assert.LessOrEqual(distToA + capsule.Radius, circle.Radius + 1e-5f);
            Assert.LessOrEqual(distToB + capsule.Radius, circle.Radius + 1e-5f);
        }

        [Test]
        public void CreateMerged_NonOverlapping()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 5);
            var circle2 = new BoundingCircle(new Vector2(20, 0), 5);

            var merged = BoundingCircle.CreateMerged(circle1, circle2);

            // Should contain both circles
            Assert.AreEqual(new Vector2(10, 0), merged.Center);
            Assert.AreEqual(15.0f, merged.Radius, 1e-5f);
        }

        [Test]
        public void CreateMerged_OneContainsOther()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 10);
            var circle2 = new BoundingCircle(new Vector2(2, 0), 3);

            var merged = BoundingCircle.CreateMerged(circle1, circle2);

            // Should return the larger circle
            Assert.AreEqual(circle1.Center, merged.Center);
            Assert.AreEqual(circle1.Radius, merged.Radius, 1e-5f);
        }

        [Test]
        public void CreateMerged_PartiallyOverlapping()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 5);
            var circle2 = new BoundingCircle(new Vector2(8, 0), 5);

            var merged = BoundingCircle.CreateMerged(circle1, circle2);

            // Verify both circles are contained
            float dist1 = Vector2.Distance(merged.Center, circle1.Center);
            float dist2 = Vector2.Distance(merged.Center, circle2.Center);
            Assert.LessOrEqual(dist1 + circle1.Radius, merged.Radius + 1e-5f);
            Assert.LessOrEqual(dist2 + circle2.Radius, merged.Radius + 1e-5f);
        }

        [Test]
        public void ContainsCircle_Contains()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 10);
            var circle2 = new BoundingCircle(new Vector2(2, 0), 3);

            var result = circle1.Contains(circle2);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsCircle_Intersects()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 10);
            var circle2 = new BoundingCircle(new Vector2(8, 0), 5);

            var result = circle1.Contains(circle2);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        [Test]
        public void ContainsCircle_Disjoint()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 5);
            var circle2 = new BoundingCircle(new Vector2(20, 0), 5);

            var result = circle1.Contains(circle2);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsCircle_Touching()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 5);
            var circle2 = new BoundingCircle(new Vector2(10, 0), 5);

            var result = circle1.Contains(circle2);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        [Test]
        public void ContainsBox_Contains()
        {
            var circle = new BoundingCircle(new Vector2(10, 10), 10);
            var box = new BoundingBox2D(new Vector2(8, 8), new Vector2(12, 12));

            var result = circle.Contains(box);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsBox_Intersects()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5);
            var box = new BoundingBox2D(new Vector2(3, 3), new Vector2(10, 10));

            var result = circle.Contains(box);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        [Test]
        public void ContainsBox_Disjoint()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5);
            var box = new BoundingBox2D(new Vector2(10, 10), new Vector2(20, 20));

            var result = circle.Contains(box);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsPoint_Inside()
        {
            var circle = new BoundingCircle(new Vector2(10, 10), 5);
            var point = new Vector2(10, 10);

            var result = circle.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_OnBoundary()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5);
            var point = new Vector2(5, 0);

            var result = circle.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_Outside()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5);
            var point = new Vector2(10, 0);

            var result = circle.Contains(point);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void IntersectsCircle_Overlapping()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 5);
            var circle2 = new BoundingCircle(new Vector2(8, 0), 5);

            bool intersects = circle1.Intersects(circle2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsCircle_Separated()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 5);
            var circle2 = new BoundingCircle(new Vector2(20, 0), 5);

            bool intersects = circle1.Intersects(circle2);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsCircle_Touching()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 5);
            var circle2 = new BoundingCircle(new Vector2(10, 0), 5);

            bool intersects = circle1.Intersects(circle2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsCircle_OneContainsOther()
        {
            var circle1 = new BoundingCircle(new Vector2(0, 0), 10);
            var circle2 = new BoundingCircle(new Vector2(2, 0), 3);

            bool intersects = circle1.Intersects(circle2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBox_Overlapping()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5);
            var box = new BoundingBox2D(new Vector2(3, 3), new Vector2(10, 10));

            bool intersects = circle.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBox_Separated()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5);
            var box = new BoundingBox2D(new Vector2(10, 10), new Vector2(20, 20));

            bool intersects = circle.Intersects(box);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBox_CircleCenterInsideBox()
        {
            var circle = new BoundingCircle(new Vector2(5, 5), 2);
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            bool intersects = circle.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBox_TouchingEdge()
        {
            var circle = new BoundingCircle(new Vector2(0, 5), 5);
            var box = new BoundingBox2D(new Vector2(5, 0), new Vector2(15, 10));

            bool intersects = circle.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBox_TouchingCorner()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5);
            var box = new BoundingBox2D(new Vector2(MathF.Sqrt(12.5f), MathF.Sqrt(12.5f)), new Vector2(10, 10));

            bool intersects = circle.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void Transform_Translation()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5);
            var matrix = Matrix.CreateTranslation(10, 20, 0);

            var transformed = circle.Transform(matrix);

            Assert.AreEqual(new Vector2(10, 20), transformed.Center);
            Assert.AreEqual(5.0f, transformed.Radius, 1e-5f);
        }

        [Test]
        public void Transform_UniformScale()
        {
            var circle = new BoundingCircle(new Vector2(5, 5), 5);
            var matrix = Matrix.CreateScale(2, 2, 1);

            var transformed = circle.Transform(matrix);

            Assert.AreEqual(new Vector2(10, 10), transformed.Center);
            Assert.AreEqual(10.0f, transformed.Radius, 1e-5f);
        }

        [Test]
        public void Transform_NonUniformScale()
        {
            var circle = new BoundingCircle(new Vector2(0, 0), 5);
            var matrix = Matrix.CreateScale(2, 3, 1);

            var transformed = circle.Transform(matrix);

            // Radius should be scaled by the maximum scale component (3)
            Assert.AreEqual(15.0f, transformed.Radius, 1e-5f);
        }

        [Test]
        public void Transform_Rotation()
        {
            var circle = new BoundingCircle(new Vector2(5, 0), 5);
            var matrix = Matrix.CreateRotationZ(MathHelper.PiOver2);

            var transformed = circle.Transform(matrix);

            // Center should be rotated
            Assert.AreEqual(0.0f, transformed.Center.X, 1e-5f);
            Assert.AreEqual(5.0f, transformed.Center.Y, 1e-5f);

            // Radius should remain the same for pure rotation
            Assert.AreEqual(5.0f, transformed.Radius, 1e-5f);
        }

        [Test]
        public void Translate_OffsetsPosition()
        {
            var circle = new BoundingCircle(new Vector2(10, 10), 5);
            var translation = new Vector2(5, -3);

            var translated = circle.Translate(translation);

            Assert.AreEqual(new Vector2(15, 7), translated.Center);
            Assert.AreEqual(5.0f, translated.Radius);
        }

        [Test]
        public void Deconstruct()
        {
            var circle = new BoundingCircle(new Vector2(10, 20), 5);

            circle.Deconstruct(out Vector2 center, out float radius);

            Assert.AreEqual(circle.Center, center);
            Assert.AreEqual(circle.Radius, radius);
        }

        [Test]
        public void IntersectsBoundingCapsule_Overlapping()
        {
            var circle = new BoundingCircle(new Vector2(5, 5), 5);
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 3);

            bool intersects = circle.Intersects(capsule);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingCapsule_Separated()
        {
            var circle = new BoundingCircle(new Vector2(5, 15), 3);
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 3);

            bool intersects = circle.Intersects(capsule);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBoundingCapsule_CircleAtEndCap()
        {
            var circle = new BoundingCircle(new Vector2(-5, 0), 3);
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);

            bool intersects = circle.Intersects(capsule);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingCapsule_CircleInsideCapsule()
        {
            var circle = new BoundingCircle(new Vector2(5, 0), 2);
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);

            bool intersects = circle.Intersects(capsule);

            Assert.IsTrue(intersects);
        }
    }
}
