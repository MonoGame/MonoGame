// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class BoundingCapsule2DTest
    {
        [Test]
        public void Constructor()
        {
            var pointA = new Vector2(0, 0);
            var pointB = new Vector2(10, 0);
            var radius = 5.0f;

            var capsule = new BoundingCapsule2D(pointA, pointB, radius);

            Assert.AreEqual(pointA, capsule.PointA);
            Assert.AreEqual(pointB, capsule.PointB);
            Assert.AreEqual(radius, capsule.Radius);
        }

        [Test]
        public void Center_ReturnsMidpoint()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);

            var center = capsule.Center;

            Assert.AreEqual(new Vector2(5, 0), center);
        }

        [Test]
        public void Length_ReturnsDistance()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);

            float length = capsule.Length;

            Assert.AreEqual(10.0f, length, 1e-5f);
        }

        [Test]
        public void LengthSquared_ReturnsSquaredDistance()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);

            float lengthSquared = capsule.LengthSquared;

            Assert.AreEqual(100.0f, lengthSquared, 1e-5f);
        }

        [Test]
        public void Direction_ReturnsUnitVector()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);

            var direction = capsule.Direction;

            Assert.AreEqual(new Vector2(1, 0), direction);
            Assert.AreEqual(1.0f, direction.Length(), 1e-5f);
        }

        [Test]
        public void Direction_DegenerateCapsule()
        {
            var capsule = new BoundingCapsule2D(new Vector2(5, 5), new Vector2(5, 5), 5);

            var direction = capsule.Direction;

            Assert.AreEqual(Vector2.Zero, direction);
        }

        [Test]
        public void Area_CalculatesCorrectly()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);

            float area = capsule.Area;

            // Area = (length * 2 * radius) + (PI * radius^2)
            // Area = (10 * 10) + (PI * 25) = 100 + 78.54 = 178.54
            Assert.AreEqual(100.0f + MathF.PI * 25.0f, area, 1e-5f);
        }

        [Test]
        public void CreateFromCenterAndDirection()
        {
            var center = new Vector2(5, 5);
            var direction = new Vector2(1, 0);
            var length = 10.0f;
            var radius = 3.0f;

            var capsule = BoundingCapsule2D.CreateFromCenterAndDirection(center, direction, length, radius);

            Assert.AreEqual(new Vector2(0, 5), capsule.PointA);
            Assert.AreEqual(new Vector2(10, 5), capsule.PointB);
            Assert.AreEqual(radius, capsule.Radius);
        }

        [Test]
        public void CreateFromCenterAndDirection_UnnormalizedDirection()
        {
            var center = new Vector2(5, 5);
            var direction = new Vector2(3, 0);
            var length = 10.0f;
            var radius = 3.0f;

            var capsule = BoundingCapsule2D.CreateFromCenterAndDirection(center, direction, length, radius);

            Assert.AreEqual(new Vector2(0, 5), capsule.PointA);
            Assert.AreEqual(new Vector2(10, 5), capsule.PointB);
        }

        [Test]
        public void CreateFromSegment()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));
            var radius = 5.0f;

            var capsule = BoundingCapsule2D.CreateFromSegment(segment, radius);

            Assert.AreEqual(segment.Start, capsule.PointA);
            Assert.AreEqual(segment.End, capsule.PointB);
            Assert.AreEqual(radius, capsule.Radius);
        }

        [Test]
        public void CreateMerged_NonOverlapping()
        {
            var capsule1 = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(5, 0), 2);
            var capsule2 = new BoundingCapsule2D(new Vector2(20, 0), new Vector2(25, 0), 2);

            var merged = BoundingCapsule2D.CreateMerged(capsule1, capsule2);

            // Should contain both capsules
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(capsule1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(capsule2));
        }

        [Test]
        public void CreateMerged_OneContainsOther()
        {
            var capsule1 = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(20, 0), 5);
            var capsule2 = new BoundingCapsule2D(new Vector2(8, 0), new Vector2(12, 0), 2);

            var merged = BoundingCapsule2D.CreateMerged(capsule1, capsule2);

            // Should approximately match the larger capsule
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(capsule1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(capsule2));
        }

        [Test]
        public void CreateMerged_PartiallyOverlapping()
        {
            var capsule1 = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 3);
            var capsule2 = new BoundingCapsule2D(new Vector2(8, 0), new Vector2(18, 0), 3);

            var merged = BoundingCapsule2D.CreateMerged(capsule1, capsule2);

            // Should contain both capsules
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(capsule1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(capsule2));
        }

        [Test]
        public void ContainsBoundingCapsule2D__Contains()
        {
            var capsule1 = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(20, 0), 10);
            var capsule2 = new BoundingCapsule2D(new Vector2(8, 0), new Vector2(12, 0), 3);

            var result = capsule1.Contains(capsule2);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsBoundingCapsule2D__Intersects()
        {
            var capsule1 = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var capsule2 = new BoundingCapsule2D(new Vector2(8, 0), new Vector2(18, 0), 5);

            var result = capsule1.Contains(capsule2);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        [Test]
        public void ContainsBoundingCapsule2D__Disjoint()
        {
            var capsule1 = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 3);
            var capsule2 = new BoundingCapsule2D(new Vector2(20, 0), new Vector2(30, 0), 3);

            var result = capsule1.Contains(capsule2);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsBoundingCircle_Contains()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var circle = new BoundingCircle(new Vector2(5, 0), 2);

            var result = capsule.Contains(circle);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsBoundingCircle_Intersects()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var circle = new BoundingCircle(new Vector2(5, 5), 3);

            var result = capsule.Contains(circle);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        [Test]
        public void ContainsBoundingCircle_Disjoint()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 3);
            var circle = new BoundingCircle(new Vector2(5, 10), 3);

            var result = capsule.Contains(circle);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsPoint_Inside()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var point = new Vector2(5, 3);

            var result = capsule.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_OnBoundary()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var point = new Vector2(5, 5);

            var result = capsule.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_Outside()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 3);
            var point = new Vector2(5, 10);

            var result = capsule.Contains(point);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsPoint_AtEndCap()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var point = new Vector2(-3, 0);

            var result = capsule.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void IntersectsBoundingCapsule2D_Overlapping()
        {
            var capsule1 = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var capsule2 = new BoundingCapsule2D(new Vector2(8, 0), new Vector2(18, 0), 5);

            bool intersects = capsule1.Intersects(capsule2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingCapsule2D_Separated()
        {
            var capsule1 = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 3);
            var capsule2 = new BoundingCapsule2D(new Vector2(20, 0), new Vector2(30, 0), 3);

            bool intersects = capsule1.Intersects(capsule2);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBoundingCapsule2D_Touching()
        {
            var capsule1 = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var capsule2 = new BoundingCapsule2D(new Vector2(15, 0), new Vector2(25, 0), 5);

            bool intersects = capsule1.Intersects(capsule2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingCapsule2D_Perpendicular()
        {
            var capsule1 = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 3);
            var capsule2 = new BoundingCapsule2D(new Vector2(5, -5), new Vector2(5, 5), 3);

            bool intersects = capsule1.Intersects(capsule2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingCircle_Overlapping()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var circle = new BoundingCircle(new Vector2(5, 5), 3);

            bool intersects = capsule.Intersects(circle);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingCircle_Separated()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 3);
            var circle = new BoundingCircle(new Vector2(5, 10), 3);

            bool intersects = capsule.Intersects(circle);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBoundingCircle_CircleAtEndCap()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var circle = new BoundingCircle(new Vector2(-5, 5), 3);

            bool intersects = capsule.Intersects(circle);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingBox2D__Overlapping()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var box = new BoundingBox2D(new Vector2(3, 3), new Vector2(7, 7));

            bool intersects = capsule.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingBox2D__Separated()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 3);
            var box = new BoundingBox2D(new Vector2(5, 10), new Vector2(15, 20));

            bool intersects = capsule.Intersects(box);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBoundingBox2D__CapsuleThroughBox()
        {
            var capsule = new BoundingCapsule2D(new Vector2(5, -10), new Vector2(5, 20), 2);
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            bool intersects = capsule.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void Transform_Translation()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var matrix = Matrix.CreateTranslation(5, 10, 0);

            var transformed = capsule.Transform(matrix);

            Assert.AreEqual(new Vector2(5, 10), transformed.PointA);
            Assert.AreEqual(new Vector2(15, 10), transformed.PointB);
            Assert.AreEqual(5.0f, transformed.Radius, 1e-5f);
        }

        [Test]
        public void Transform_UniformScale()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var matrix = Matrix.CreateScale(2, 2, 1);

            var transformed = capsule.Transform(matrix);

            Assert.AreEqual(new Vector2(0, 0), transformed.PointA);
            Assert.AreEqual(new Vector2(20, 0), transformed.PointB);
            Assert.AreEqual(10.0f, transformed.Radius, 1e-5f);
        }

        [Test]
        public void Transform_NonUniformScale()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var matrix = Matrix.CreateScale(2, 3, 1);

            var transformed = capsule.Transform(matrix);

            Assert.AreEqual(new Vector2(0, 0), transformed.PointA);
            Assert.AreEqual(new Vector2(20, 0), transformed.PointB);

            // Radius should be scaled by maximum scale component (3)
            Assert.AreEqual(15.0f, transformed.Radius, 1e-5f);
        }

        [Test]
        public void Transform_Rotation()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var matrix = Matrix.CreateRotationZ(MathHelper.PiOver2);

            var transformed = capsule.Transform(matrix);

            Assert.AreEqual(0.0f, transformed.PointA.X, 1e-5f);
            Assert.AreEqual(0.0f, transformed.PointA.Y, 1e-5f);
            Assert.AreEqual(0.0f, transformed.PointB.X, 1e-5f);
            Assert.AreEqual(10.0f, transformed.PointB.Y, 1e-5f);
            Assert.AreEqual(5.0f, transformed.Radius, 1e-5f);
        }

        [Test]
        public void Translate_OffsetsPosition()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);
            var translation = new Vector2(5, -3);

            var translated = capsule.Translate(translation);

            Assert.AreEqual(new Vector2(5, -3), translated.PointA);
            Assert.AreEqual(new Vector2(15, -3), translated.PointB);
            Assert.AreEqual(5.0f, translated.Radius);
        }

        [Test]
        public void Deconstruct()
        {
            var capsule = new BoundingCapsule2D(new Vector2(0, 0), new Vector2(10, 0), 5);

            capsule.Deconstruct(out Vector2 pointA, out Vector2 pointB, out float radius);

            Assert.AreEqual(capsule.PointA, pointA);
            Assert.AreEqual(capsule.PointB, pointB);
            Assert.AreEqual(capsule.Radius, radius);
        }
    }
}
