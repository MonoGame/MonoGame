// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class LineSegment2DTest
    {
        #region Constructor Tests

        [Test]
        public void Constructor()
        {
            var start = new Vector2(1, 2);
            var end = new Vector2(3, 4);

            var segment = new LineSegment2D(start, end);

            Assert.AreEqual(start, segment.Start);
            Assert.AreEqual(end, segment.End);
        }

        #endregion

        #region Computed Property Tests

        [Test]
        public void Direction_ReturnsVector()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(3, 4));

            var direction = segment.Direction;

            Assert.AreEqual(new Vector2(3, 4), direction);
        }

        [Test]
        public void Midpoint_ReturnsCenterPoint()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 10));

            var midpoint = segment.Midpoint;

            Assert.AreEqual(new Vector2(5, 5), midpoint);
        }

        [Test]
        public void Length_Horizontal()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));

            float length = segment.Length;

            Assert.AreEqual(10.0f, length, Collision2D.Epsilon);
        }

        [Test]
        public void Length_Diagonal()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(3, 4));

            float length = segment.Length;

            Assert.AreEqual(5.0f, length, Collision2D.Epsilon);
        }

        [Test]
        public void LengthSquared_AvoidsSqrt()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(3, 4));

            float lengthSquared = segment.LengthSquared;

            Assert.AreEqual(25.0f, lengthSquared, Collision2D.Epsilon);
        }

        #endregion

        #region GetPoint Tests (Type-Specific Utility)

        [Test]
        public void GetPoint_AtStart()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));

            var point = segment.GetPoint(0.0f);

            Assert.AreEqual(new Vector2(0, 0), point);
        }

        [Test]
        public void GetPoint_AtMidpoint()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));

            var point = segment.GetPoint(0.5f);

            Assert.AreEqual(new Vector2(5, 0), point);
        }

        [Test]
        public void GetPoint_AtEnd()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));

            var point = segment.GetPoint(1.0f);

            Assert.AreEqual(new Vector2(10, 0), point);
        }

        [Test]
        public void GetPoint_BeyondEnd()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));

            var point = segment.GetPoint(1.5f);

            Assert.AreEqual(new Vector2(15, 0), point);
        }

        #endregion

        #region GetBounds Tests (Type-Specific Method)

        [Test]
        public void GetBounds_HorizontalSegment()
        {
            var segment = new LineSegment2D(new Vector2(2, 5), new Vector2(8, 5));

            var bounds = segment.GetBounds();

            Assert.AreEqual(new Vector2(2, 5), bounds.Min);
            Assert.AreEqual(new Vector2(8, 5), bounds.Max);
        }

        [Test]
        public void GetBounds_VerticalSegment()
        {
            var segment = new LineSegment2D(new Vector2(5, 2), new Vector2(5, 8));

            var bounds = segment.GetBounds();

            Assert.AreEqual(new Vector2(5, 2), bounds.Min);
            Assert.AreEqual(new Vector2(5, 8), bounds.Max);
        }

        [Test]
        public void GetBounds_DiagonalSegment()
        {
            var segment = new LineSegment2D(new Vector2(1, 2), new Vector2(9, 7));

            var bounds = segment.GetBounds();

            Assert.AreEqual(new Vector2(1, 2), bounds.Min);
            Assert.AreEqual(new Vector2(9, 7), bounds.Max);
        }

        [Test]
        public void GetBounds_ReversedEndpoints()
        {
            var segment = new LineSegment2D(new Vector2(9, 7), new Vector2(1, 2));

            var bounds = segment.GetBounds();

            Assert.AreEqual(new Vector2(1, 2), bounds.Min);
            Assert.AreEqual(new Vector2(9, 7), bounds.Max);
        }

        [Test]
        public void GetBounds_DegenerateSegment()
        {
            var segment = new LineSegment2D(new Vector2(5, 5), new Vector2(5, 5));

            var bounds = segment.GetBounds();

            Assert.AreEqual(new Vector2(5, 5), bounds.Min);
            Assert.AreEqual(new Vector2(5, 5), bounds.Max);
        }

        #endregion

        #region Distance and Projection Tests (Delegation Spot Checks)

        [Test]
        public void ClosestPoint_OnSegment()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));
            var point = new Vector2(5, 3);

            var closest = segment.ClosestPoint(point, out float distanceAlongSegment);

            Assert.AreEqual(new Vector2(5, 0), closest);
            Assert.AreEqual(0.5f, distanceAlongSegment, Collision2D.Epsilon);
        }

        [Test]
        public void ClosestPoint_BeforeStart()
        {
            var segment = new LineSegment2D(new Vector2(10, 0), new Vector2(20, 0));
            var point = new Vector2(5, 3);

            var closest = segment.ClosestPoint(point, out float distanceAlongSegment);

            Assert.AreEqual(new Vector2(10, 0), closest);
            Assert.AreEqual(0.0f, distanceAlongSegment, Collision2D.Epsilon);
        }

        [Test]
        public void ClosestPoint_AfterEnd()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));
            var point = new Vector2(15, 3);

            var closest = segment.ClosestPoint(point, out float distanceAlongSegment);

            Assert.AreEqual(new Vector2(10, 0), closest);
        }

        [Test]
        public void DistanceToPoint_PerpendicularDistance()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));
            var point = new Vector2(5, 3);

            float distance = segment.DistanceToPoint(point);

            Assert.AreEqual(3.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceSquaredToPoint_AvoidsSqrt()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));
            var point = new Vector2(5, 3);

            float distanceSquared = segment.DistanceSquaredToPoint(point);

            Assert.AreEqual(9.0f, distanceSquared, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceToSegment_Parallel()
        {
            var segment1 = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));
            var segment2 = new LineSegment2D(new Vector2(0, 5), new Vector2(10, 5));

            float distance = segment1.DistanceToSegment(segment2);

            Assert.AreEqual(5.0f, distance, Collision2D.Epsilon);
        }

        #endregion

        #region Deconstruct Test

        [Test]
        public void Deconstruct()
        {
            var segment = new LineSegment2D(new Vector2(1, 2), new Vector2(3, 4));

            var (start, end) = segment;

            Assert.AreEqual(new Vector2(1, 2), start);
            Assert.AreEqual(new Vector2(3, 4), end);
        }

        #endregion
    }
}
