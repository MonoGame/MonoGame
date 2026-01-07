// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class Line2DTest
    {
        [Test]
        public void Constructor()
        {
            var normal = new Vector2(0, 1);
            var distance = 5.0f;

            var line = new Line2D(normal, distance);

            Assert.AreEqual(normal, line.Normal);
            Assert.AreEqual(distance, line.Distance);
        }

        [Test]
        public void CreateFromPointAndNormal()
        {
            var point = new Vector2(0, 5);
            var normal = new Vector2(0, 1);

            var line = Line2D.CreateFromPointAndNormal(point, normal);

            Assert.AreEqual(new Vector2(0, 1), line.Normal);
            Assert.AreEqual(5.0f, line.Distance, 1e-6f);
        }

        [Test]
        public void CreateFromTwoPoints()
        {
            var p1 = new Vector2(0, 0);
            var p2 = new Vector2(10, 0);

            var line = Line2D.CreateFromTwoPoints(p1, p2);

            // Line through (0,0) and (10,0) has normal (0,1) or (0,-1)
            Assert.AreEqual(0, MathF.Abs(line.Normal.X), 1e-6f);
            Assert.AreEqual(1, MathF.Abs(line.Normal.Y), 1e-6f);
        }

        [Test]
        public void CreateFromTwoPoints_ThrowsWhenPointsTooClose()
        {
            var p1 = new Vector2(0, 0);
            var p2 = new Vector2(1e-7f, 0);

            Assert.Throws<ArgumentException>(() => Line2D.CreateFromTwoPoints(p1, p2));
        }

        [Test]
        public void CreateFromPointAndDirection()
        {
            var point = new Vector2(5, 0);
            var direction = new Vector2(1, 0);

            var line = Line2D.CreateFromPointAndDirection(point, direction);

            // Direction (1,0) gives normal (0,1) or (0,-1)
            Assert.AreEqual(0, MathF.Abs(line.Normal.X), 1e-6f);
            Assert.AreEqual(1, MathF.Abs(line.Normal.Y), 1e-6f);
        }

        [Test]
        public void DistanceToPoint_PointOnLine()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var point = new Vector2(0, 5);

            float distance = line.DistanceToPoint(point);

            Assert.AreEqual(0.0f, distance, 1e-6f);
        }

        [Test]
        public void DistanceToPoint_PointAboveLine()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var point = new Vector2(0, 8);

            float distance = line.DistanceToPoint(point);

            Assert.AreEqual(3.0f, distance, 1e-6f);
        }

        [Test]
        public void DistanceToPoint_PointBelowLine()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var point = new Vector2(0, 2);

            float distance = line.DistanceToPoint(point);

            Assert.AreEqual(-3.0f, distance, 1e-6f);
        }

        [Test]
        public void ClosestPoint_ReturnsProjection()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var point = new Vector2(10, 8);

            var closest = line.ClosestPoint(point, out float distanceAlongLine);

            Assert.AreEqual(new Vector2(10, 5), closest);

            // For normal (0,1), the line's direction vector is (-1,0)
            // Distance from reference point (0,5) to (10,5) along (-1,0) is -10
            Assert.AreEqual(-10.0f, distanceAlongLine, 1e-6f);
        }

        [Test]
        public void ClosestPoint_ParametricDistance_VerticalLine()
        {
            var line = new Line2D(new Vector2(1, 0), 3);
            var point = new Vector2(3, 10);

            var closest = line.ClosestPoint(point, out float distanceAlongLine);

            Assert.AreEqual(new Vector2(3, 10), closest);

            // For normal (1,0), the line's direction vector is (0,1)
            // Distance from reference point (3,0) to (3,10) along (0,1) is 10
            Assert.AreEqual(10.0f, distanceAlongLine, 1e-5f);
        }

        [Test]
        public void Normalize_Static_CreatesUnitNormal()
        {
            var line = new Line2D(new Vector2(3, 4), 10);

            var normalized = Line2D.Normalize(line);

            Assert.AreEqual(1.0f, normalized.Normal.Length(), 1e-6f);
            Assert.AreEqual(2.0f, normalized.Distance, 1e-6f);
        }

        [Test]
        public void Normalize_StaticRef_CreatesUnitNormal()
        {
            var line = new Line2D(new Vector2(3, 4), 10);

            Line2D.Normalize(ref line, out Line2D result);

            Assert.AreEqual(1.0f, result.Normal.Length(), 1e-6f);
            Assert.AreEqual(2.0f, result.Distance, 1e-6f);
        }

        [Test]
        public void Normalize_Instance_ModifiesInPlace()
        {
            var line = new Line2D(new Vector2(3, 4), 10);

            line.Normalize();

            Assert.AreEqual(1.0f, line.Normal.Length(), 1e-6f);
            Assert.AreEqual(2.0f, line.Distance, 1e-6f);
        }

        [Test]
        public void Normalize_AlreadyNormalized_RemainsUnchanged()
        {
            var line = new Line2D(new Vector2(0, 1), 5);

            var normalized = Line2D.Normalize(line);

            Assert.AreEqual(line.Normal, normalized.Normal);
            Assert.AreEqual(line.Distance, normalized.Distance);
        }

        [Test]
        public void Normalize_Static_PreservesGeometricLine()
        {
            var line = new Line2D(new Vector2(0, 2), 10);
            var testPoint = new Vector2(5, 5);

            var normalized = Line2D.Normalize(line);

            // Distance to point should be the same for both representations
            float originalDistance = line.DistanceToPoint(testPoint);
            float normalizedDistance = normalized.DistanceToPoint(testPoint);
            Assert.AreEqual(originalDistance, normalizedDistance, 1e-5f);
        }

        [Test]
        public void IntersectsLine_Perpendicular()
        {
            var line1 = new Line2D(new Vector2(1, 0), 0);
            var line2 = new Line2D(new Vector2(0, 1), 0);

            bool intersects = line1.Intersects(line2, out Vector2? point);

            Assert.IsTrue(intersects);
            Assert.AreEqual(Vector2.Zero, point);
        }

        [Test]
        public void IntersectsLine_Parallel()
        {
            var line1 = new Line2D(new Vector2(0, 1), 5);
            var line2 = new Line2D(new Vector2(0, 1), 10);

            bool intersects = line1.Intersects(line2);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsRay_ForwardDirection()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(0, 1));

            bool intersects = line.Intersects(ray, out float? distanceAlongRay, out Vector2? point);

            Assert.IsTrue(intersects);
            Assert.AreEqual(5.0f, distanceAlongRay, 1e-6f);
            Assert.AreEqual(new Vector2(0, 5), point);
        }

        [Test]
        public void IntersectsRay_BehindRayOrigin()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var ray = new Ray2D(new Vector2(0, 10), new Vector2(0, 1));

            bool intersects = line.Intersects(ray);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsSegment_WithinBounds()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(0, 10));

            bool intersects = line.Intersects(segment, out float? distanceAlongSegment, out Vector2? point);

            Assert.IsTrue(intersects);
            Assert.AreEqual(0.5f, distanceAlongSegment, 1e-6f);
            Assert.AreEqual(new Vector2(0, 5), point);
        }

        [Test]
        public void IntersectsSegment_OutsideBounds()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var segment = new LineSegment2D(new Vector2(0, 10), new Vector2(0, 20));

            bool intersects = line.Intersects(segment);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void Deconstruct()
        {
            var line = new Line2D(new Vector2(1, 0), 5.0f);

            line.Deconstruct(out Vector2 normal, out float distance);

            Assert.AreEqual(line.Normal, normal);
            Assert.AreEqual(line.Distance, distance);
        }

        [Test]
        public void IntersectsBoundingBox2D_PassesThrough()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            bool intersects = line.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingBox2D_MissesCompletely()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var box = new BoundingBox2D(new Vector2(0, 10), new Vector2(10, 20));

            bool intersects = line.Intersects(box);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBoundingBox2D_TouchesEdge()
        {
            var line = new Line2D(new Vector2(0, 1), 10);
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            bool intersects = line.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingBox2D_TouchesCorner()
        {
            var line = Line2D.CreateFromTwoPoints(new Vector2(0, 0), new Vector2(10, 10));
            var box = new BoundingBox2D(new Vector2(5, 5), new Vector2(15, 15));

            bool intersects = line.Intersects(box);

            Assert.IsTrue(intersects);
        }


        [Test]
        public void IntersectsBoundingBox2D_HorizontalLine()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var box = new BoundingBox2D(new Vector2(-5, 0), new Vector2(5, 10));

            bool intersects = line.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingBox2D_VerticalLine()
        {
            var line = new Line2D(new Vector2(1, 0), 5);
            var box = new BoundingBox2D(new Vector2(0, -5), new Vector2(10, 5));

            bool intersects = line.Intersects(box);

            Assert.IsTrue(intersects);
        }
    }
}
