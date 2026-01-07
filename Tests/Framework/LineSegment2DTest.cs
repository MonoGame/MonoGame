// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class LineSegment2DTest
    {
        [Test]
        public void Constructor()
        {
            var start = new Vector2(1, 2);
            var end = new Vector2(3, 4);

            var segment = new LineSegment2D(start, end);

            Assert.AreEqual(start, segment.Start);
            Assert.AreEqual(end, segment.End);
        }

        [Test]
        public void Direction_ReturnsVector()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));

            var direction = segment.Direction;

            Assert.AreEqual(new Vector2(10, 0), direction);
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

            Assert.AreEqual(10.0f, length, 1e-6f);
        }


        [Test]
        public void Length_Diagonal()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(3, 4));

            float length = segment.Length;

            Assert.AreEqual(5.0f, length, 1e-6f);
        }

        [Test]
        public void LengthSquared_AvoidsSqrt()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(3, 4));

            float lengthSquared = segment.LengthSquared;

            Assert.AreEqual(25.0f, lengthSquared, 1e-6f);
        }

        [Test]
        public void GetPoint_AtStart()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));

            var point = segment.GetPoint(0);

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

            var point = segment.GetPoint(2.0f);

            Assert.AreEqual(new Vector2(20, 0), point);
        }

        [Test]
        public void ClosestPoint_OnSegment()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));
            var point = new Vector2(5, 5);

            var closest = segment.ClosestPoint(point, out float distanceAlongSegment);

            Assert.AreEqual(new Vector2(5, 0), closest);
            Assert.AreEqual(0.5f, distanceAlongSegment, 1e-6f);
        }

        [Test]
        public void ClosestPoint_BeforeStart()
        {
            var segment = new LineSegment2D(new Vector2(10, 0), new Vector2(20, 0));
            var point = new Vector2(0, 5);

            var closest = segment.ClosestPoint(point, out float distanceAlongSegment);

            Assert.AreEqual(new Vector2(10, 0), closest);
            Assert.AreEqual(0.0f, distanceAlongSegment);
        }

        [Test]
        public void ClosestPoint_AfterEnd()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));
            var point = new Vector2(20, 5);

            var closest = segment.ClosestPoint(point, out float distanceAlongSegment);

            Assert.AreEqual(new Vector2(10, 0), closest);
            Assert.AreEqual(1.0f, distanceAlongSegment);
        }

        [Test]
        public void DistanceToPoint_PerpendicularDistance()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));
            var point = new Vector2(5, 5);

            float distance = segment.DistanceToPoint(point);

            Assert.AreEqual(5.0f, distance, 1e-6f);
        }

        [Test]
        public void DistanceSquaredToPoint_AvoidsSqrt()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));
            var point = new Vector2(5, 3);

            float distanceSquared = segment.DistanceSquaredToPoint(point);

            Assert.AreEqual(9.0f, distanceSquared, 1e-6f);
        }

        [Test]
        public void DistanceToSegment_Parallel()
        {
            var segment1 = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));
            var segment2 = new LineSegment2D(new Vector2(0, 5), new Vector2(10, 5));

            float distance = segment1.DistanceToSegment(segment2);

            Assert.AreEqual(5.0f, distance, 1e-6f);
        }

        [Test]
        public void IntersectsLine_WithinBounds()
        {
            var segment = new LineSegment2D(new Vector2(0, -5), new Vector2(0, 5));
            var line = new Line2D(new Vector2(0, 1), 0);

            bool intersects = segment.Intersects(line, out float? distanceAlongSegment, out Vector2? point);

            Assert.IsTrue(intersects);
            Assert.AreEqual(0.5f, distanceAlongSegment, 1e-6f);
            Assert.AreEqual(new Vector2(0, 0), point);
        }

        [Test]
        public void IntersectsLine_OutsideBounds()
        {
            var segment = new LineSegment2D(new Vector2(0, 5), new Vector2(0, 10));
            var line = new Line2D(new Vector2(0, 1), 0);

            bool intersects = segment.Intersects(line);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsRay_WithinBounds()
        {
            var segment = new LineSegment2D(new Vector2(5, -5), new Vector2(5, 5));
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));

            bool intersects = segment.Intersects(ray, out float? distanceAlongSegment, out float? distanceAlongRay, out Vector2? point);

            Assert.IsTrue(intersects);
            Assert.AreEqual(0.5f, distanceAlongSegment, 1e-6f);
            Assert.AreEqual(5.0f, distanceAlongRay, 1e-6f);
            Assert.AreEqual(new Vector2(5, 0), point);
        }

        [Test]
        public void IntersectsRay_OutsideBounds()
        {
            var segment = new LineSegment2D(new Vector2(5, 5), new Vector2(5, 10));
            var ray = new Ray2D(new Vector2(0, 0), new Vector2(1, 0));

            bool intersects = segment.Intersects(ray);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsSegment_Crossing()
        {
            var segment1 = new LineSegment2D(new Vector2(0, 5), new Vector2(10, 5));
            var segment2 = new LineSegment2D(new Vector2(5, 0), new Vector2(5, 10));

            bool intersects = segment1.Intersects(segment2, out float? d1, out float? d2, out Vector2? point);

            Assert.IsTrue(intersects);
            Assert.AreEqual(0.5f, d1, 1e-6f);
            Assert.AreEqual(0.5f, d2, 1e-6f);
            Assert.AreEqual(new Vector2(5, 5), point);
        }

        [Test]
        public void IntersectsSegment_Parallel()
        {
            var segment1 = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 0));
            var segment2 = new LineSegment2D(new Vector2(0, 5), new Vector2(10, 5));

            bool intersects = segment1.Intersects(segment2);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsSegment_NotCrossing()
        {
            var segment1 = new LineSegment2D(new Vector2(0, 0), new Vector2(5, 0));
            var segment2 = new LineSegment2D(new Vector2(10, -5), new Vector2(10, 5));

            bool intersects = segment1.Intersects(segment2);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void Deconstruct()
        {
            var segment = new LineSegment2D(new Vector2(1, 2), new Vector2(3, 4));

            segment.Deconstruct(out Vector2 start, out Vector2 end);

            Assert.AreEqual(segment.Start, start);
            Assert.AreEqual(segment.End, end);
        }

        [Test]
        public void IntersectsBoundingBox_PassesCompletelyThrough()
        {
            var segment = new LineSegment2D(new Vector2(5, -5), new Vector2(5, 15));
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            bool intersects = segment.Intersects(box, out float? tMin, out float? tMax);

            Assert.IsTrue(intersects);
            Assert.AreEqual(0.25f, tMin, 1e-5f);
            Assert.AreEqual(0.75f, tMax, 1e-5f);
        }

        [Test]
        public void IntersectsBoundingBox_StartsInside()
        {
            var segment = new LineSegment2D(new Vector2(5, 5), new Vector2(15, 5));
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            bool intersects = segment.Intersects(box, out float? tMin, out float? tMax);

            Assert.IsTrue(intersects);
            Assert.AreEqual(0.0f, tMin);
            Assert.AreEqual(0.5f, tMax, 1e-5f);
        }

        [Test]
        public void IntersectsBoundingBox_EndsInside()
        {
            var segment = new LineSegment2D(new Vector2(-5, 5), new Vector2(5, 5));
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            bool intersects = segment.Intersects(box, out float? tMin, out float? tMax);

            Assert.IsTrue(intersects);
            Assert.AreEqual(0.5f, tMin, 1e-5f);
            Assert.AreEqual(1.0f, tMax);
        }

        [Test]
        public void IntersectsBoundingBox_BothEndpointsInside()
        {
            var segment = new LineSegment2D(new Vector2(2, 5), new Vector2(8, 5));
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            bool intersects = segment.Intersects(box, out float? tMin, out float? tMax);

            Assert.IsTrue(intersects);
            Assert.AreEqual(0.0f, tMin);
            Assert.AreEqual(1.0f, tMax);
        }

        [Test]
        public void IntersectsBoundingBox_CompletelyOutside()
        {
            var segment = new LineSegment2D(new Vector2(-5, 5), new Vector2(-2, 5));
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            bool intersects = segment.Intersects(box);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBoundingBox_TouchesEdge()
        {
            var segment = new LineSegment2D(new Vector2(-5, 10), new Vector2(5, 10));
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            bool intersects = segment.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingBox_TouchesCorner()
        {
            var segment = new LineSegment2D(new Vector2(0, 0), new Vector2(10, 10));
            var box = new BoundingBox2D(new Vector2(5, 5), new Vector2(15, 15));

            bool intersects = segment.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBoundingBox_DegenerateSegmentInside()
        {
            var segment = new LineSegment2D(new Vector2(5, 5), new Vector2(5, 5));
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            bool intersects = segment.Intersects(box, out float? tMin, out float? tMax);

            Assert.IsTrue(intersects);
            Assert.AreEqual(0.0f, tMin);
            Assert.AreEqual(0.0f, tMax);
        }

        [Test]
        public void IntersectsBoundingBox_DegenerateSegmentOutside()
        {
            var segment = new LineSegment2D(new Vector2(15, 15), new Vector2(15, 15));
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            bool intersects = segment.Intersects(box);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void GetBounds_HorizontalSegment()
        {
            var segment = new LineSegment2D(new Vector2(5, 10), new Vector2(15, 10));

            var bounds = segment.GetBounds();

            Assert.AreEqual(new Vector2(5, 10), bounds.Min);
            Assert.AreEqual(new Vector2(15, 10), bounds.Max);
        }

        [Test]
        public void GetBounds_VerticalSegment()
        {
            var segment = new LineSegment2D(new Vector2(10, 5), new Vector2(10, 15));

            var bounds = segment.GetBounds();

            Assert.AreEqual(new Vector2(10, 5), bounds.Min);
            Assert.AreEqual(new Vector2(10, 15), bounds.Max);
        }

        [Test]
        public void GetBounds_DiagonalSegment()
        {
            var segment = new LineSegment2D(new Vector2(5, 10), new Vector2(15, 20));

            var bounds = segment.GetBounds();

            Assert.AreEqual(new Vector2(5, 10), bounds.Min);
            Assert.AreEqual(new Vector2(15, 20), bounds.Max);
        }

        [Test]
        public void GetBounds_ReversedEndpoints()
        {
            var segment = new LineSegment2D(new Vector2(15, 20), new Vector2(5, 10));

            var bounds = segment.GetBounds();

            Assert.AreEqual(new Vector2(5, 10), bounds.Min);
            Assert.AreEqual(new Vector2(15, 20), bounds.Max);
        }

        [Test]
        public void GetBounds_DegenerateSegment()
        {
            var segment = new LineSegment2D(new Vector2(10, 15), new Vector2(10, 15));

            var bounds = segment.GetBounds();

            Assert.AreEqual(new Vector2(10, 15), bounds.Min);
            Assert.AreEqual(new Vector2(10, 15), bounds.Max);
        }
    }
}
