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
        #region Constructor Tests

        [Test]
        public void Constructor()
        {
            var origin = new Vector2(1, 2);
            var direction = new Vector2(3, 4);

            var ray = new Ray2D(origin, direction);

            Assert.AreEqual(origin, ray.Origin);
            Assert.AreEqual(direction, ray.Direction);
        }

        #endregion

        #region Factory Method Tests

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

        #endregion

        #region GetPoint Tests (Type-Specific Utility)

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
            var ray = new Ray2D(new Vector2(10, 0), new Vector2(1, 0));

            var point = ray.GetPoint(-5);

            Assert.AreEqual(new Vector2(5, 0), point);
        }

        #endregion

        #region Distance and Projection Tests (Delegation Spot Checks)

        [Test]
        public void ClosestPoint_PointAhead()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
            var point = new Vector2(5, 3);

            var closest = ray.ClosestPoint(point, out float distanceAlongRay);

            Assert.AreEqual(new Vector2(5, 0), closest);
        }

        [Test]
        public void ClosestPoint_PointBehind()
        {
            var ray = new Ray2D(new Vector2(10, 0), new Vector2(1, 0));
            var point = new Vector2(5, 3);

            var closest = ray.ClosestPoint(point, out float distanceAlongRay);

            Assert.AreEqual(new Vector2(10, 0), closest);
        }

        [Test]
        public void DistanceToPoint_PerpendicularDistance()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
            var point = new Vector2(5, 3);

            float distance = ray.DistanceToPoint(point);

            Assert.AreEqual(3.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceSquaredToPoint_AvoidsSqrt()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));
            var point = new Vector2(5, 3);

            float distanceSquared = ray.DistanceSquaredToPoint(point);

            Assert.AreEqual(9.0f, distanceSquared, Collision2D.Epsilon);
        }

        #endregion

        #region Normalize Tests

        [Test]
        public void Normalize_Static_CreatesUnitDirection()
        {
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(3, 4));

            var normalized = Ray2D.Normalize(ray);

            Assert.AreEqual(1.0f, normalized.Direction.Length(), Collision2D.Epsilon);
            Assert.AreEqual(ray.Origin, normalized.Origin);
        }

        [Test]
        public void Normalize_Instance_ModifiesInPlace()
        {
            var ray = new Ray2D(new Vector2(5, 5), new Vector2(3, 4));

            ray.Normalize();

            Assert.AreEqual(1.0f, ray.Direction.Length(), Collision2D.Epsilon);
            Assert.AreEqual(new Vector2(5, 5), ray.Origin);
        }

        #endregion

        #region Deconstruct Test

        [Test]
        public void Deconstruct()
        {
            var ray = new Ray2D(new Vector2(1, 2), new Vector2(3, 4));

            var (origin, direction) = ray;

            Assert.AreEqual(new Vector2(1, 2), origin);
            Assert.AreEqual(new Vector2(3, 4), direction);
        }

        #endregion
    }
}
