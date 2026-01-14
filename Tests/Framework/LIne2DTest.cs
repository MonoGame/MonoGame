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
        #region Constructor Tests

        [Test]
        public void Constructor()
        {
            var normal = new Vector2(0, 1);
            var distance = 5.0f;

            var line = new Line2D(normal, distance);

            Assert.AreEqual(normal, line.Normal);
            Assert.AreEqual(distance, line.Distance);
        }

        #endregion

        #region Factory Method Tests

        [Test]
        public void CreateFromPointAndNormal()
        {
            var point = new Vector2(0, 5);
            var normal = new Vector2(0, 1);

            var line = Line2D.CreateFromPointAndNormal(point, normal);

            Assert.AreEqual(new Vector2(0, 1), line.Normal);
            Assert.AreEqual(5.0f, line.Distance, Collision2D.Epsilon);
        }

        [Test]
        public void CreateFromTwoPoints()
        {
            var p1 = new Vector2(0, 0);
            var p2 = new Vector2(10, 0);

            var line = Line2D.CreateFromTwoPoints(p1, p2);

            Assert.AreEqual(0, MathF.Abs(line.Normal.X), Collision2D.Epsilon);
            Assert.AreEqual(1, MathF.Abs(line.Normal.Y), Collision2D.Epsilon);
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

            Assert.AreEqual(0, MathF.Abs(line.Normal.X), Collision2D.Epsilon);
            Assert.AreEqual(1, MathF.Abs(line.Normal.Y), Collision2D.Epsilon);
        }

        #endregion

        #region Distance and Projection Tests (Delegation Spot Checks)

        [Test]
        public void DistanceToPoint_PointOnLine()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var point = new Vector2(0, 5);

            float distance = line.DistanceToPoint(point);

            Assert.AreEqual(0.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceToPoint_PointOffLine()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var point = new Vector2(0, 8);

            float distance = line.DistanceToPoint(point);

            Assert.AreEqual(3.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void ClosestPoint_ReturnsProjection()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var point = new Vector2(3, 8);

            var closest = line.ClosestPoint(point, out float distanceAlongLine);

            Assert.AreEqual(new Vector2(3, 5), closest);
        }

        #endregion

        #region Normalize Tests

        [Test]
        public void Normalize_Static_CreatesUnitNormal()
        {
            var line = new Line2D(new Vector2(3, 4), 10);

            var normalized = Line2D.Normalize(line);

            Assert.AreEqual(1.0f, normalized.Normal.Length(), Collision2D.Epsilon);
        }

        [Test]
        public void Normalize_StaticRef_CreatesUnitNormal()
        {
            var line = new Line2D(new Vector2(3, 4), 10);

            Line2D.Normalize(ref line, out var normalized);

            Assert.AreEqual(1.0f, normalized.Normal.Length(), Collision2D.Epsilon);
        }

        [Test]
        public void Normalize_Instance_ModifiesInPlace()
        {
            var line = new Line2D(new Vector2(3, 4), 10);

            line.Normalize();

            Assert.AreEqual(1.0f, line.Normal.Length(), Collision2D.Epsilon);
        }

        [Test]
        public void Normalize_AlreadyNormalized_RemainsUnchanged()
        {
            var line = new Line2D(new Vector2(0, 1), 5);
            var original = line;

            line.Normalize();

            Assert.AreEqual(original.Normal, line.Normal);
            Assert.AreEqual(original.Distance, line.Distance);
        }

        #endregion

        #region Deconstruct Test

        [Test]
        public void Deconstruct()
        {
            var line = new Line2D(new Vector2(0, 1), 5);

            var (normal, distance) = line;

            Assert.AreEqual(new Vector2(0, 1), normal);
            Assert.AreEqual(5.0f, distance);
        }

        #endregion
    }
}
