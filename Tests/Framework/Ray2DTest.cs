// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class Ray2DTest
    {
        [Test]
        public void Constructor()
        {
            var origin = new Vector2(1, 2);
            var direction = new Vector2(0, 1);

            var ray = new Ray2D(origin, direction);

            Assert.AreEqual(origin, ray.Origin);
            Assert.AreEqual(direction, ray.Direction);
        }

        [Test]
        public void CreateFromPoints()
        {
            var start = new Vector2(0, 0);
            var through = new Vector2(10, 0);

            var ray = Ray2D.CreateFromPoints(start, through);

            Assert.AreEqual(start, ray.Origin);
            Assert.AreEqual(new Vector2(1, 0), ray.Direction);
        }

        [Test]
        public void CreateFromPoints_ThrowsWhenPointsTooClose()
        {
            var start = new Vector2(0, 0);
            var through = new Vector2(1e-7f, 0);

            Assert.Throws<ArgumentException>(() => Ray2D.CreateFromPoints(start, through));
        }

        [Test]
        public void GetPoint_AtOrigin()
        {
            var ray = new Ray2D(new Vector2(5, 5), new Vector2(1, 0));

            var point = ray.GetPoint(0);

            Assert.AreEqual(new Vector2(5, 5), point);
        }

        [Test]
        public void GetPoint_ForwardDirection()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));

            var point = ray.GetPoint(10);

            Assert.AreEqual(new Vector2(10, 0), point);
        }

        [Test]
        public void GetPoint_NegativeDistance()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));

            var point = ray.GetPoint(-5);

            Assert.AreEqual(new Vector2(-5, 0), point);
        }

        [Test]
        public void ClosestPoint_PointAhead()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
            var point = new Vector2(10, 5);

            var closest = ray.ClosestPoint(point, out float distanceAlongRay);

            Assert.AreEqual(new Vector2(10, 0), closest);
            Assert.AreEqual(10.0f, distanceAlongRay, 1e-6f);
        }

        [Test]
        public void ClosestPoint_PointBehind()
        {
            var ray = new Ray2D(new Vector2(10, 0), new Vector2(1, 0));
            var point = new Vector2(0, 5);

            var closest = ray.ClosestPoint(point, out float distanceAlongRay);

            Assert.AreEqual(new Vector2(10, 0), closest);
            Assert.AreEqual(0.0f, distanceAlongRay);
        }

        [Test]
        public void DistanceToPoint_PerpendicularDistance()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
            var point = new Vector2(10, 5);

            float distance = ray.DistanceToPoint(point);

            Assert.AreEqual(5.0f, distance, 1e-6f);
        }

        [Test]
        public void DistanceSquaredToPoint_AvoidsSqrt()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
            var point = new Vector2(10, 3);

            float distanceSquared = ray.DistanceSquaredToPoint(point);

            Assert.AreEqual(9.0f, distanceSquared, 1e-6f);
        }

        [Test]
        public void Normalize_CreatesUnitDirection()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(3, 4));

            Ray2D.Normalize(ref ray, out Ray2D normalized);

            Assert.AreEqual(new Vector2(0, 0), normalized.Origin);
            Assert.AreEqual(1.0f, normalized.Direction.Length(), 1e-6f);
        }

        [Test]
        public void IntersectsRay_Crossing()
        {
            var ray1 = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
            var ray2 = new Ray2D(new Vector2(5, -5), new Vector2(0, 1));

            bool intersects = ray1.Intersects(ray2, out float? d1, out float? d2, out Vector2? point);

            Assert.IsTrue(intersects);
            Assert.AreEqual(5.0f, d1, 1e-6f);
            Assert.AreEqual(5.0f, d2, 1e-6f);
            Assert.AreEqual(new Vector2(5, 0), point);
        }

        [Test]
        public void IntersectsRay_Parallel()
        {
            var ray1 = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
            var ray2 = new Ray2D(new Vector2(0, 5), new Vector2(1, 0));

            bool intersects = ray1.Intersects(ray2);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsRay_BehindOrigin()
        {
            var ray1 = new Ray2D(new Vector2(10, 0), new Vector2(1, 0));
            var ray2 = new Ray2D(new Vector2(5, -5), new Vector2(0, 1));

            bool intersects = ray1.Intersects(ray2);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsSegment_WithinBounds()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
            var segment = new LineSegment2D(new Vector2(5, -5), new Vector2(5, 5));

            bool intersects = ray.Intersects(segment, out float? distanceAlongRay, out float? distanceAlongSegment, out Vector2? point);

            Assert.IsTrue(intersects);
            Assert.AreEqual(5.0f, distanceAlongRay, 1e-6f);
            Assert.AreEqual(0.5f, distanceAlongSegment, 1e-6f);
            Assert.AreEqual(new Vector2(5, 0), point);
        }

        [Test]
        public void IntersectsSegment_OutsideBounds()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
            var segment = new LineSegment2D(new Vector2(5, 5), new Vector2(5, 10));

            bool intersects = ray.Intersects(segment);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void Deconstruct()
        {
            var ray = new Ray2D(new Vector2(1, 2), new Vector2(3, 4));

            ray.Deconstruct(out Vector2 origin, out Vector2 direction);

            Assert.AreEqual(ray.Origin, origin);
            Assert.AreEqual(ray.Direction, direction);
        }

        [Test]
        public void IntersectsBoundingBox_HitsBox()
        {
            var ray = new Ray2D(new Vector2(0, 5), new Vector2(1, 0));
            var box = new BoundingBox2D(new Vector2(10, 0), new Vector2(20, 10));

            bool intersects = ray.Intersects(box, out float? tMin, out float? tMax);

            Assert.IsTrue(intersects);
            Assert.AreEqual(10.0f, tMin, 1e-5f);
            Assert.AreEqual(20.0f, tMax, 1e-5f);
        }

        [Test]
        public void IntersectsBoundingBox_MissesBox()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
            var box = new BoundingBox2D(new Vector2(10, 10), new Vector2(20, 20));

            bool intersects = ray.Intersects(box);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBoundingBox_OriginInsideBox()
        {
            var ray = new Ray2D(new Vector2(5, 5), new Vector2(1, 0));
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            bool intersects = ray.Intersects(box, out float? tMin, out float? tMax);

            Assert.IsTrue(intersects);
            Assert.AreEqual(0.0f, tMin);
            Assert.AreEqual(5.0f, tMax, 1e-5f);
        }

        [Test]
        public void IntersectsBoundingBox_PointsAwayFromBox()
        {
            var ray = new Ray2D(new Vector2(0, 5), new Vector2(-1, 0));
            var box = new BoundingBox2D(new Vector2(10, 0), new Vector2(20, 10));

            bool intersects = ray.Intersects(box);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBoundingBox_ParallelToEdgeInside()
        {
            var ray = new Ray2D(new Vector2(5, 5), new Vector2(1, 0));
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(20, 10));

            bool intersects = ray.Intersects(box, out float? tMin, out float? tMax);

            Assert.IsTrue(intersects);
            Assert.AreEqual(0.0f, tMin);
            Assert.AreEqual(15.0f, tMax, 1e-5f);
        }

        [Test]
        public void IntersectsBoundingBox_ParallelToEdgeOutside()
        {
            var ray = new Ray2D(new Vector2(5, 15), new Vector2(1, 0));
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(20, 10));

            bool intersects = ray.Intersects(box);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBoundingBox_HitsCorner()
        {
            var ray = new Ray2D(new Vector2(0, 0), Vector2.Normalize(new Vector2(1, 1)));
            var box = new BoundingBox2D(new Vector2(10, 10), new Vector2(20, 20));

            bool intersects = ray.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingBox_TouchesEdge()
        {
            var ray = new Ray2D(new Vector2(0, 10), new Vector2(1, 0));
            var box = new BoundingBox2D(new Vector2(10, 0), new Vector2(20, 10));

            bool intersects = ray.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingCircle_HitsCircle()
        {
            var ray = new Ray2D(new Vector2(0, 5), new Vector2(1, 0));
            var circle = new BoundingCircle(new Vector2(10, 5), 3);

            bool intersects = ray.Intersects(circle, out float? tMin, out float? tMax);

            Assert.IsTrue(intersects);
            Assert.AreEqual(7.0f, tMin, 1e-5f);
            Assert.AreEqual(13.0f, tMax, 1e-5f);
        }

        [Test]
        public void IntersectsBoundingCircle_MissesCircle()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
            var circle = new BoundingCircle(new Vector2(10, 10), 3);

            bool intersects = ray.Intersects(circle);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBoundingCircle_OriginInsideCircle()
        {
            var ray = new Ray2D(new Vector2(5, 5), new Vector2(1, 0));
            var circle = new BoundingCircle(new Vector2(5, 5), 10);

            bool intersects = ray.Intersects(circle, out float? tMin, out float? tMax);

            Assert.IsTrue(intersects);
            Assert.AreEqual(0.0f, tMin);
            Assert.AreEqual(10.0f, tMax, 1e-5f);
        }

        [Test]
        public void IntersectsBoundingCircle_PointsAwayFromCircle()
        {
            var ray = new Ray2D(new Vector2(0, 5), new Vector2(-1, 0));
            var circle = new BoundingCircle(new Vector2(10, 5), 3);

            bool intersects = ray.Intersects(circle);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBoundingCircle_TangentRay()
        {
            var ray = new Ray2D(new Vector2(0, 5), new Vector2(1, 0));
            var circle = new BoundingCircle(new Vector2(10, 10), 5);

            bool intersects = ray.Intersects(circle, out float? tMin, out float? tMax);

            Assert.IsTrue(intersects);
            Assert.AreEqual(10.0f, tMin, 1e-5f);
            Assert.AreEqual(10.0f, tMax, 1e-5f);
        }

        [Test]
        public void IntersectsBoundingCircle_RayThroughCenter()
        {
            var ray = new Ray2D(new Vector2(0, 5), new Vector2(1, 0));
            var circle = new BoundingCircle(new Vector2(10, 5), 5);

            bool intersects = ray.Intersects(circle, out float? tMin, out float? tMax);

            Assert.IsTrue(intersects);
            Assert.AreEqual(5.0f, tMin, 1e-5f);
            Assert.AreEqual(15.0f, tMax, 1e-5f);
        }
    }
}
