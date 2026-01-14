// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class Collision2DTest
    {
        #region Helper Methods

        #region IsValidPolygon Tests

        [Test]
        public void IsValidPolygon_ValidPolygon_ReturnsTrue()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1)
            };

            var normals = new[]
            {
                new Vector2(0, -1),
                Vector2.Normalize(new Vector2(1, 1)),
                new Vector2(-1, 0)
            };

            bool result = Collision2D.IsValidPolygon(vertices, normals);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsValidPolygon_NullVertices_ReturnsFalse()
        {
            var normals = new[] { Vector2.UnitX, Vector2.UnitY, -Vector2.UnitX };

            bool result = Collision2D.IsValidPolygon(null, normals);

            Assert.IsFalse(result);
        }

        [Test]
        public void IsValidPolygon_NullNormals_ReturnsFalse()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1)
            };

            bool result = Collision2D.IsValidPolygon(vertices, null);

            Assert.IsFalse(result);
        }

        [Test]
        public void IsValidPolygon_MismatchedArrayLengths_ReturnsFalse()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1)
            };
            var normals = new[]
            {
                Vector2.UnitX,
                Vector2.UnitY
            };

            bool result = Collision2D.IsValidPolygon(vertices, normals);

            Assert.IsFalse(result);
        }

        [Test]
        public void IsValidPolygon_LessThanThreeVertices_ReturnsFalse()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0)
            };
            var normals = new[]
            {
                Vector2.UnitX,
                Vector2.UnitY
            };

            bool result = Collision2D.IsValidPolygon(vertices, normals);

            Assert.IsFalse(result);
        }

        #endregion

        #region ClipInterval Tests (ref overload)

        [Test]
        public void ClipInterval_OverlappingIntervals_ReturnsTrue()
        {
            float tMin = 0.0f;
            float tMax = 10.0f;
            float clipMin = 3.0f;
            float clipMax = 8.0f;

            bool result = Collision2D.ClipInterval(ref tMin, ref tMax, clipMin, clipMax);

            Assert.IsTrue(result);
            Assert.AreEqual(3.0f, tMin);
            Assert.AreEqual(8.0f, tMax);
        }

        [Test]
        public void ClipInterval_NonOverlappingIntervals_ReturnsFalse()
        {
            float tMin = 0.0f;
            float tMax = 5.0f;
            float clipMin = 10.0f;
            float clipMax = 15.0f;

            bool result = Collision2D.ClipInterval(ref tMin, ref tMax, clipMin, clipMax);

            Assert.IsFalse(result);
        }

        [Test]
        public void ClipInterval_TouchingAtBoundary_ReturnsTrue()
        {
            float tMin = 0.0f;
            float tMax = 5.0f;
            float clipMin = 5.0f;
            float clipMax = 10.0f;

            bool result = Collision2D.ClipInterval(ref tMin, ref tMax, clipMin, clipMax);

            Assert.IsTrue(result);
            Assert.AreEqual(5.0f, tMin);
            Assert.AreEqual(5.0f, tMax);
        }

        [Test]
        public void ClipInterval_ClippingWiderThanOriginal_KeepsOriginalBounds()
        {
            float tMin = 3.0f;
            float tMax = 7.0f;
            float clipMin = 0.0f;
            float clipMax = 10.0f;

            bool result = Collision2D.ClipInterval(ref tMin, ref tMax, clipMin, clipMax);

            Assert.IsTrue(result);
            Assert.AreEqual(3.0f, tMin);
            Assert.AreEqual(7.0f, tMax);
        }

        [Test]
        public void ClipInterval_NegativeIntervals_WorksCorrectly()
        {
            float tMin = -10.0f;
            float tMax = -5.0f;
            float clipMin = -8.0f;
            float clipMax = -3.0f;

            bool result = Collision2D.ClipInterval(ref tMin, ref tMax, clipMin, clipMax);

            Assert.IsTrue(result);
            Assert.AreEqual(-8.0f, tMin);
            Assert.AreEqual(-5.0f, tMax);
        }

        #endregion

        #region ClipInterval Tests (out overload)

        [Test]
        public void ClipIntervalOut_OverlappingIntervals_ReturnsClippedValues()
        {
            float tEnter = 0.0f;
            float tExit = 10.0f;
            float tLower = 3.0f;
            float tUpper = 8.0f;

            bool result = Collision2D.ClipInterval(tEnter, tExit, tLower, tUpper, out float clippedEnter, out float clippedExit);

            Assert.IsTrue(result);
            Assert.AreEqual(3.0f, clippedEnter);
            Assert.AreEqual(8.0f, clippedExit);
        }

        [Test]
        public void ClipIntervalOut_NonOverlappingIntervals_ReturnsFalse()
        {
            float tEnter = 0.0f;
            float tExit = 5.0f;
            float tLower = 10.0f;
            float tUpper = 15.0f;

            bool result = Collision2D.ClipInterval(tEnter, tExit, tLower, tUpper, out float clippedEnter, out float clippedExit);

            Assert.IsFalse(result);
        }

        [Test]
        public void ClipIntervalOut_TouchingAtBoundary_ReturnsTrue()
        {
            float tEnter = 0.0f;
            float tExit = 5.0f;
            float tLower = 5.0f;
            float tUpper = 10.0f;

            bool result = Collision2D.ClipInterval(tEnter, tExit, tLower, tUpper, out float clippedEnter, out float clippedExit);

            Assert.IsTrue(result);
            Assert.AreEqual(5.0f, clippedEnter);
            Assert.AreEqual(5.0f, clippedExit);
        }

        [Test]
        public void ClipIntervalOut_ClippingWiderThanOriginal_ReturnsOriginal()
        {
            float tEnter = 3.0f;
            float tExit = 7.0f;
            float tLower = 0.0f;
            float tUpper = 10.0f;

            bool result = Collision2D.ClipInterval(tEnter, tExit, tLower, tUpper, out float clippedEnter, out float clippedExit);

            Assert.IsTrue(result);
            Assert.AreEqual(3.0f, clippedEnter);
            Assert.AreEqual(7.0f, clippedExit);
        }

        #endregion

        #region IntervalsOverlap Tests

        [Test]
        public void IntervalsOverlap_Overlapping_ReturnsTrue()
        {
            float minA = 0.0f;
            float maxA = 10.0f;
            float minB = 5.0f;
            float maxB = 15.0f;

            bool result = Collision2D.IntervalsOverlap(minA, maxA, minB, maxB);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntervalsOverlap_Separated_ReturnsFalse()
        {
            float minA = 0.0f;
            float maxA = 5.0f;
            float minB = 10.0f;
            float maxB = 15.0f;

            bool result = Collision2D.IntervalsOverlap(minA, maxA, minB, maxB);

            Assert.IsFalse(result);
        }

        [Test]
        public void IntervalsOverlap_TouchingAtPoint_ReturnsTrue()
        {
            float minA = 0.0f;
            float maxA = 5.0f;
            float minB = 5.0f;
            float maxB = 10.0f;

            bool result = Collision2D.IntervalsOverlap(minA, maxA, minB, maxB);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntervalsOverlap_NegativeIntervals_WorksCorrectly()
        {
            float minA = -10.0f;
            float maxA = -5.0f;
            float minB = -8.0f;
            float maxB = -3.0f;

            bool result = Collision2D.IntervalsOverlap(minA, maxA, minB, maxB);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntervalsOverlap_NearlyTouching_ReturnsFalse()
        {
            float minA = 0.0f;
            float maxA = 5.0f;
            float minB = 5.0f + Collision2D.Epsilon * 2.0f;
            float maxB = 10.0f;

            bool result = Collision2D.IntervalsOverlap(minA, maxA, minB, maxB);

            Assert.IsFalse(result);
        }

        #endregion

        #endregion

        #region Containment - AABB Methods

        #region ContainsAabbPoint Tests

        [Test]
        public void ContainsAabbPoint_PointInside_ReturnsContains()
        {
            var point = new Vector2(5, 5);
            var min = new Vector2(0, 0);
            var max = new Vector2(10, 10);

            var result = Collision2D.ContainsAabbPoint(point, min, max);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsAabbPoint_PointOutside_ReturnsDisjoint()
        {
            var point = new Vector2(15, 5);
            var min = new Vector2(0, 0);
            var max = new Vector2(10, 10);

            var result = Collision2D.ContainsAabbPoint(point, min, max);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsAabbPoint_PointOnBoundary_ReturnsContains()
        {
            var point = new Vector2(10, 5);
            var min = new Vector2(0, 0);
            var max = new Vector2(10, 10);

            var result = Collision2D.ContainsAabbPoint(point, min, max);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        #endregion

        #region ContainsAabbAabb Tests

        [Test]
        public void ContainsAabbAabb_CompletelyContains_ReturnsContains()
        {
            var aMin = new Vector2(0, 0);
            var aMax = new Vector2(10, 10);
            var bMin = new Vector2(2, 2);
            var bMax = new Vector2(8, 8);

            var result = Collision2D.ContainsAabbAabb(aMin, aMax, bMin, bMax);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsAabbAabb_NoOverlap_ReturnsDisjoint()
        {
            var aMin = new Vector2(0, 0);
            var aMax = new Vector2(10, 10);
            var bMin = new Vector2(15, 15);
            var bMax = new Vector2(20, 20);

            var result = Collision2D.ContainsAabbAabb(aMin, aMax, bMin, bMax);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsAabbAabb_PartialOverlap_ReturnsIntersects()
        {
            var aMin = new Vector2(0, 0);
            var aMax = new Vector2(10, 10);
            var bMin = new Vector2(5, 5);
            var bMax = new Vector2(15, 15);

            var result = Collision2D.ContainsAabbAabb(aMin, aMax, bMin, bMax);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsAabbCircle Tests

        [Test]
        public void ContainsAabbCircle_CircleCompletelyInside_ReturnsContains()
        {
            var boxMin = new Vector2(0, 0);
            var boxMax = new Vector2(10, 10);
            var circleCenter = new Vector2(5, 5);
            var circleRadius = 2.0f;

            var result = Collision2D.ContainsAabbCircle(boxMin, boxMax, circleCenter, circleRadius);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsAabbCircle_CircleCompletelyOutside_ReturnsDisjoint()
        {
            var boxMin = new Vector2(0, 0);
            var boxMax = new Vector2(10, 10);
            var circleCenter = new Vector2(20, 20);
            var circleRadius = 3.0f;

            var result = Collision2D.ContainsAabbCircle(boxMin, boxMax, circleCenter, circleRadius);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsAabbCircle_CirclePartiallyInside_ReturnsIntersects()
        {
            var boxMin = new Vector2(0, 0);
            var boxMax = new Vector2(10, 10);
            var circleCenter = new Vector2(5, 5);
            var circleRadius = 7.0f;

            var result = Collision2D.ContainsAabbCircle(boxMin, boxMax, circleCenter, circleRadius);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsAabbObb Tests

        [Test]
        public void ContainsAabbObb_ObbCompletelyInside_ReturnsContains()
        {
            var aabbMin = new Vector2(0, 0);
            var aabbMax = new Vector2(10, 10);
            var obbCenter = new Vector2(5, 5);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(2, 2);

            var result = Collision2D.ContainsAabbObb(aabbMin, aabbMax, obbCenter, obbAxisX, obbAxisY, obbHalfExtents);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsAabbObb_ObbCompletelyOutside_ReturnsDisjoint()
        {
            var aabbMin = new Vector2(0, 0);
            var aabbMax = new Vector2(10, 10);
            var obbCenter = new Vector2(20, 20);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(2, 2);

            var result = Collision2D.ContainsAabbObb(aabbMin, aabbMax, obbCenter, obbAxisX, obbAxisY, obbHalfExtents);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsAabbObb_ObbPartiallyInside_ReturnsIntersects()
        {
            var aabbMin = new Vector2(0, 0);
            var aabbMax = new Vector2(10, 10);
            var obbCenter = new Vector2(8, 8);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(5, 5);

            var result = Collision2D.ContainsAabbObb(aabbMin, aabbMax, obbCenter, obbAxisX, obbAxisY, obbHalfExtents);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsAabbCapsule Tests

        [Test]
        public void ContainsAabbCapsule_CapsuleCompletelyInside_ReturnsContains()
        {
            var boxMin = new Vector2(0, 0);
            var boxMax = new Vector2(10, 10);
            var capsuleA = new Vector2(3, 5);
            var capsuleB = new Vector2(7, 5);
            var capsuleRadius = 1.0f;

            var result = Collision2D.ContainsAabbCapsule(boxMin, boxMax, capsuleA, capsuleB, capsuleRadius);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsAabbCapsule_CapsuleCompletelyOutside_ReturnsDisjoint()
        {
            var boxMin = new Vector2(0, 0);
            var boxMax = new Vector2(10, 10);
            var capsuleA = new Vector2(15, 15);
            var capsuleB = new Vector2(20, 20);
            var capsuleRadius = 1.0f;

            var result = Collision2D.ContainsAabbCapsule(boxMin, boxMax, capsuleA, capsuleB, capsuleRadius);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsAabbCapsule_CapsulePartiallyInside_ReturnsIntersects()
        {
            var boxMin = new Vector2(0, 0);
            var boxMax = new Vector2(10, 10);
            var capsuleA = new Vector2(5, 5);
            var capsuleB = new Vector2(15, 5);
            var capsuleRadius = 1.0f;

            var result = Collision2D.ContainsAabbCapsule(boxMin, boxMax, capsuleA, capsuleB, capsuleRadius);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsAabbConvexPolygon Tests

        [Test]
        public void ContainsAabbConvexPolygon_PolygonCompletelyInside_ReturnsContains()
        {
            var boxMin = new Vector2(0, 0);
            var boxMax = new Vector2(10, 10);
            var vertices = new[]
            {
                new Vector2(3, 3),
                new Vector2(7, 3),
                new Vector2(7, 7),
                new Vector2(3, 7)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsAabbConvexPolygon(boxMin, boxMax, vertices, normals);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsAabbConvexPolygon_PolygonCompletelyOutside_ReturnsDisjoint()
        {
            var boxMin = new Vector2(0, 0);
            var boxMax = new Vector2(10, 10);
            var vertices = new[]
            {
                new Vector2(15, 15),
                new Vector2(20, 15),
                new Vector2(20, 20),
                new Vector2(15, 20)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsAabbConvexPolygon(boxMin, boxMax, vertices, normals);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsAabbConvexPolygon_PolygonPartiallyInside_ReturnsIntersects()
        {
            var boxMin = new Vector2(0, 0);
            var boxMax = new Vector2(10, 10);
            var vertices = new[]
            {
                new Vector2(5, 5),
                new Vector2(15, 5),
                new Vector2(15, 15),
                new Vector2(5, 15)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsAabbConvexPolygon(boxMin, boxMax, vertices, normals);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #endregion

        #region Containment - Circle Methods

        #region ContainsCirclePoint Tests

        [Test]
        public void ContainsCirclePoint_PointInside_ReturnsContains()
        {
            var point = new Vector2(3, 0);
            var center = new Vector2(0, 0);
            var radius = 5.0f;

            var result = Collision2D.ContainsCirclePoint(point, center, radius);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsCirclePoint_PointOutside_ReturnsDisjoint()
        {
            var point = new Vector2(10, 0);
            var center = new Vector2(0, 0);
            var radius = 5.0f;

            var result = Collision2D.ContainsCirclePoint(point, center, radius);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsCirclePoint_PointOnCircumference_ReturnsContains()
        {
            var point = new Vector2(5, 0);
            var center = new Vector2(0, 0);
            var radius = 5.0f;

            var result = Collision2D.ContainsCirclePoint(point, center, radius);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        #endregion

        #region ContainsCircleAabb Tests

        [Test]
        public void ContainsCircleAabb_AabbCompletelyInside_ReturnsContains()
        {
            var circleCenter = new Vector2(10, 10);
            var circleRadius = 10.0f;
            var aabbMin = new Vector2(8, 8);
            var aabbMax = new Vector2(12, 12);

            var result = Collision2D.ContainsCircleAabb(circleCenter, circleRadius, aabbMin, aabbMax);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsCircleAabb_AabbCompletelyOutside_ReturnsDisjoint()
        {
            var circleCenter = new Vector2(0, 0);
            var circleRadius = 5.0f;
            var aabbMin = new Vector2(20, 20);
            var aabbMax = new Vector2(25, 25);

            var result = Collision2D.ContainsCircleAabb(circleCenter, circleRadius, aabbMin, aabbMax);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsCircleAabb_AabbPartiallyInside_ReturnsIntersects()
        {
            var circleCenter = new Vector2(0, 0);
            var circleRadius = 5.0f;
            var aabbMin = new Vector2(3, 3);
            var aabbMax = new Vector2(8, 8);

            var result = Collision2D.ContainsCircleAabb(circleCenter, circleRadius, aabbMin, aabbMax);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsCircleCircle Tests

        [Test]
        public void ContainsCircleCircle_CompletelyContains_ReturnsContains()
        {
            var aCenter = new Vector2(0, 0);
            var aRadius = 10.0f;
            var bCenter = new Vector2(2, 0);
            var bRadius = 3.0f;

            var result = Collision2D.ContainsCircleCircle(aCenter, aRadius, bCenter, bRadius);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsCircleCircle_Separated_ReturnsDisjoint()
        {
            var aCenter = new Vector2(0, 0);
            var aRadius = 5.0f;
            var bCenter = new Vector2(20, 0);
            var bRadius = 3.0f;

            var result = Collision2D.ContainsCircleCircle(aCenter, aRadius, bCenter, bRadius);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsCircleCircle_PartialOverlap_ReturnsIntersects()
        {
            var aCenter = new Vector2(0, 0);
            var aRadius = 5.0f;
            var bCenter = new Vector2(7, 0);
            var bRadius = 4.0f;

            var result = Collision2D.ContainsCircleCircle(aCenter, aRadius, bCenter, bRadius);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsCircleObb Tests

        [Test]
        public void ContainsCircleObb_ObbCompletelyInside_ReturnsContains()
        {
            var circleCenter = new Vector2(10, 10);
            var circleRadius = 10.0f;
            var obbCenter = new Vector2(10, 10);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(2, 2);

            var result = Collision2D.ContainsCircleObb(circleCenter, circleRadius, obbCenter, obbAxisX, obbAxisY, obbHalfExtents);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsCircleObb_ObbCompletelyOutside_ReturnsDisjoint()
        {
            var circleCenter = new Vector2(0, 0);
            var circleRadius = 5.0f;
            var obbCenter = new Vector2(20, 20);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(2, 2);

            var result = Collision2D.ContainsCircleObb(circleCenter, circleRadius, obbCenter, obbAxisX, obbAxisY, obbHalfExtents);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsCircleObb_ObbPartiallyInside_ReturnsIntersects()
        {
            var circleCenter = new Vector2(0, 0);
            var circleRadius = 5.0f;
            var obbCenter = new Vector2(4, 4);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(3, 3);

            var result = Collision2D.ContainsCircleObb(circleCenter, circleRadius, obbCenter, obbAxisX, obbAxisY, obbHalfExtents);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsCircleCapsule Tests

        [Test]
        public void ContainsCircleCapsule_CapsuleCompletelyInside_ReturnsContains()
        {
            var circleCenter = new Vector2(10, 10);
            var circleRadius = 10.0f;
            var capsuleA = new Vector2(8, 10);
            var capsuleB = new Vector2(12, 10);
            var capsuleRadius = 1.0f;

            var result = Collision2D.ContainsCircleCapsule(circleCenter, circleRadius, capsuleA, capsuleB, capsuleRadius);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsCircleCapsule_CapsuleCompletelyOutside_ReturnsDisjoint()
        {
            var circleCenter = new Vector2(0, 0);
            var circleRadius = 5.0f;
            var capsuleA = new Vector2(20, 20);
            var capsuleB = new Vector2(25, 25);
            var capsuleRadius = 1.0f;

            var result = Collision2D.ContainsCircleCapsule(circleCenter, circleRadius, capsuleA, capsuleB, capsuleRadius);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsCircleCapsule_CapsulePartiallyInside_ReturnsIntersects()
        {
            var circleCenter = new Vector2(0, 0);
            var circleRadius = 5.0f;
            var capsuleA = new Vector2(3, 0);
            var capsuleB = new Vector2(10, 0);
            var capsuleRadius = 1.0f;

            var result = Collision2D.ContainsCircleCapsule(circleCenter, circleRadius, capsuleA, capsuleB, capsuleRadius);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsCircleConvexPolygon Tests

        [Test]
        public void ContainsCircleConvexPolygon_PolygonCompletelyInside_ReturnsContains()
        {
            var circleCenter = new Vector2(10, 10);
            var circleRadius = 10.0f;
            var vertices = new[]
            {
                new Vector2(9, 9),
                new Vector2(11, 9),
                new Vector2(11, 11),
                new Vector2(9, 11)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsCircleConvexPolygon(circleCenter, circleRadius, vertices, normals);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsCircleConvexPolygon_PolygonCompletelyOutside_ReturnsDisjoint()
        {
            var circleCenter = new Vector2(0, 0);
            var circleRadius = 5.0f;
            var vertices = new[]
            {
                new Vector2(20, 20),
                new Vector2(25, 20),
                new Vector2(25, 25),
                new Vector2(20, 25)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsCircleConvexPolygon(circleCenter, circleRadius, vertices, normals);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsCircleConvexPolygon_PolygonPartiallyInside_ReturnsIntersects()
        {
            var circleCenter = new Vector2(0, 0);
            var circleRadius = 5.0f;
            var vertices = new[]
            {
                new Vector2(3, 3),
                new Vector2(8, 3),
                new Vector2(8, 8),
                new Vector2(3, 8)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsCircleConvexPolygon(circleCenter, circleRadius, vertices, normals);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #endregion

        #region Containment - OBB Methods

        #region ContainsObbPoint Tests

        [Test]
        public void ContainsObbPoint_PointInside_ReturnsContains()
        {
            var point = new Vector2(5, 5);
            var center = new Vector2(5, 5);
            var axisX = Vector2.UnitX;
            var axisY = Vector2.UnitY;
            var halfExtents = new Vector2(3, 3);

            var result = Collision2D.ContainsObbPoint(point, center, axisX, axisY, halfExtents);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsObbPoint_PointOutside_ReturnsDisjoint()
        {
            var point = new Vector2(15, 15);
            var center = new Vector2(5, 5);
            var axisX = Vector2.UnitX;
            var axisY = Vector2.UnitY;
            var halfExtents = new Vector2(3, 3);

            var result = Collision2D.ContainsObbPoint(point, center, axisX, axisY, halfExtents);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsObbPoint_PointOnEdge_ReturnsContains()
        {
            var point = new Vector2(8, 5);
            var center = new Vector2(5, 5);
            var axisX = Vector2.UnitX;
            var axisY = Vector2.UnitY;
            var halfExtents = new Vector2(3, 3);

            var result = Collision2D.ContainsObbPoint(point, center, axisX, axisY, halfExtents);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        #endregion

        #region ContainsObbObb Tests

        [Test]
        public void ContainsObbObb_CompletelyContains_ReturnsContains()
        {
            var aCenter = new Vector2(5, 5);
            var aAxisX = Vector2.UnitX;
            var aAxisY = Vector2.UnitY;
            var aHalf = new Vector2(5, 5);
            var bCenter = new Vector2(5, 5);
            var bAxisX = Vector2.UnitX;
            var bAxisY = Vector2.UnitY;
            var bHalf = new Vector2(2, 2);

            var result = Collision2D.ContainsObbObb(aCenter, aAxisX, aAxisY, aHalf, bCenter, bAxisX, bAxisY, bHalf);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsObbObb_Separated_ReturnsDisjoint()
        {
            var aCenter = new Vector2(0, 0);
            var aAxisX = Vector2.UnitX;
            var aAxisY = Vector2.UnitY;
            var aHalf = new Vector2(2, 2);
            var bCenter = new Vector2(20, 20);
            var bAxisX = Vector2.UnitX;
            var bAxisY = Vector2.UnitY;
            var bHalf = new Vector2(2, 2);

            var result = Collision2D.ContainsObbObb(aCenter, aAxisX, aAxisY, aHalf, bCenter, bAxisX, bAxisY, bHalf);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsObbObb_PartialOverlap_ReturnsIntersects()
        {
            var aCenter = new Vector2(5, 5);
            var aAxisX = Vector2.UnitX;
            var aAxisY = Vector2.UnitY;
            var aHalf = new Vector2(5, 5);
            var bCenter = new Vector2(8, 8);
            var bAxisX = Vector2.UnitX;
            var bAxisY = Vector2.UnitY;
            var bHalf = new Vector2(5, 5);

            var result = Collision2D.ContainsObbObb(aCenter, aAxisX, aAxisY, aHalf, bCenter, bAxisX, bAxisY, bHalf);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsObbCapsule Tests

        [Test]
        public void ContainsObbCapsule_CapsuleCompletelyInside_ReturnsContains()
        {
            var boxCenter = new Vector2(10, 10);
            var axisX = Vector2.UnitX;
            var axisY = Vector2.UnitY;
            var halfExtents = new Vector2(5, 5);
            var capsuleA = new Vector2(8, 10);
            var capsuleB = new Vector2(12, 10);
            var capsuleRadius = 1.0f;

            var result = Collision2D.ContainsObbCapsule(boxCenter, axisX, axisY, halfExtents, capsuleA, capsuleB, capsuleRadius);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsObbCapsule_CapsuleCompletelyOutside_ReturnsDisjoint()
        {
            var boxCenter = new Vector2(0, 0);
            var axisX = Vector2.UnitX;
            var axisY = Vector2.UnitY;
            var halfExtents = new Vector2(5, 5);
            var capsuleA = new Vector2(20, 20);
            var capsuleB = new Vector2(25, 25);
            var capsuleRadius = 1.0f;

            var result = Collision2D.ContainsObbCapsule(boxCenter, axisX, axisY, halfExtents, capsuleA, capsuleB, capsuleRadius);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsObbCapsule_CapsulePartiallyInside_ReturnsIntersects()
        {
            var boxCenter = new Vector2(5, 5);
            var axisX = Vector2.UnitX;
            var axisY = Vector2.UnitY;
            var halfExtents = new Vector2(5, 5);
            var capsuleA = new Vector2(5, 5);
            var capsuleB = new Vector2(15, 5);
            var capsuleRadius = 1.0f;

            var result = Collision2D.ContainsObbCapsule(boxCenter, axisX, axisY, halfExtents, capsuleA, capsuleB, capsuleRadius);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsObbConvexPolygon Tests

        [Test]
        public void ContainsObbConvexPolygon_PolygonCompletelyInside_ReturnsContains()
        {
            var boxCenter = new Vector2(10, 10);
            var axisX = Vector2.UnitX;
            var axisY = Vector2.UnitY;
            var halfExtents = new Vector2(5, 5);
            var vertices = new[]
            {
                new Vector2(8, 8),
                new Vector2(12, 8),
                new Vector2(12, 12),
                new Vector2(8, 12)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsObbConvexPolygon(boxCenter, axisX, axisY, halfExtents, vertices, normals);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsObbConvexPolygon_PolygonCompletelyOutside_ReturnsDisjoint()
        {
            var boxCenter = new Vector2(0, 0);
            var axisX = Vector2.UnitX;
            var axisY = Vector2.UnitY;
            var halfExtents = new Vector2(5, 5);
            var vertices = new[]
            {
                new Vector2(20, 20),
                new Vector2(25, 20),
                new Vector2(25, 25),
                new Vector2(20, 25)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsObbConvexPolygon(boxCenter, axisX, axisY, halfExtents, vertices, normals);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsObbConvexPolygon_PolygonPartiallyInside_ReturnsIntersects()
        {
            var boxCenter = new Vector2(5, 5);
            var axisX = Vector2.UnitX;
            var axisY = Vector2.UnitY;
            var halfExtents = new Vector2(5, 5);
            var vertices = new[]
            {
                new Vector2(5, 5),
                new Vector2(15, 5),
                new Vector2(15, 15),
                new Vector2(5, 15)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsObbConvexPolygon(boxCenter, axisX, axisY, halfExtents, vertices, normals);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #endregion

        #region Containment - Capsule Methods

        #region ContainsCapsulePoint Tests

        [Test]
        public void ContainsCapsulePoint_PointInside_ReturnsContains()
        {
            var point = new Vector2(5, 1);
            var a = new Vector2(0, 0);
            var b = new Vector2(10, 0);
            var radius = 2.0f;

            var result = Collision2D.ContainsCapsulePoint(point, a, b, radius);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsCapsulePoint_PointOutside_ReturnsDisjoint()
        {
            var point = new Vector2(5, 10);
            var a = new Vector2(0, 0);
            var b = new Vector2(10, 0);
            var radius = 2.0f;

            var result = Collision2D.ContainsCapsulePoint(point, a, b, radius);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsCapsulePoint_PointOnBoundary_ReturnsContains()
        {
            var point = new Vector2(5, 2);
            var a = new Vector2(0, 0);
            var b = new Vector2(10, 0);
            var radius = 2.0f;

            var result = Collision2D.ContainsCapsulePoint(point, a, b, radius);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        #endregion

        #region ContainsCapsuleObb Tests

        [Test]
        public void ContainsCapsuleObb_ObbCompletelyInside_ReturnsContains()
        {
            var capsuleA = new Vector2(0, 5);
            var capsuleB = new Vector2(20, 5);
            var capsuleRadius = 5.0f;
            var obbCenter = new Vector2(10, 5);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalf = new Vector2(2, 2);

            var result = Collision2D.ContainsCapsuleObb(capsuleA, capsuleB, capsuleRadius, obbCenter, obbAxisX, obbAxisY, obbHalf);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsCapsuleObb_ObbCompletelyOutside_ReturnsDisjoint()
        {
            var capsuleA = new Vector2(0, 0);
            var capsuleB = new Vector2(10, 0);
            var capsuleRadius = 2.0f;
            var obbCenter = new Vector2(20, 20);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalf = new Vector2(2, 2);

            var result = Collision2D.ContainsCapsuleObb(capsuleA, capsuleB, capsuleRadius, obbCenter, obbAxisX, obbAxisY, obbHalf);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsCapsuleObb_ObbPartiallyInside_ReturnsIntersects()
        {
            var capsuleA = new Vector2(0, 5);
            var capsuleB = new Vector2(10, 5);
            var capsuleRadius = 3.0f;
            var obbCenter = new Vector2(8, 5);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalf = new Vector2(5, 5);

            var result = Collision2D.ContainsCapsuleObb(capsuleA, capsuleB, capsuleRadius, obbCenter, obbAxisX, obbAxisY, obbHalf);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsCapsuleCapsule Tests

        [Test]
        public void ContainsCapsuleCapsule_CompletelyContains_ReturnsContains()
        {
            var a0 = new Vector2(0, 0);
            var a1 = new Vector2(20, 0);
            var aRadius = 5.0f;
            var b0 = new Vector2(8, 0);
            var b1 = new Vector2(12, 0);
            var bRadius = 2.0f;

            var result = Collision2D.ContainsCapsuleCapsule(a0, a1, aRadius, b0, b1, bRadius);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsCapsuleCapsule_Separated_ReturnsDisjoint()
        {
            var a0 = new Vector2(0, 0);
            var a1 = new Vector2(10, 0);
            var aRadius = 2.0f;
            var b0 = new Vector2(20, 20);
            var b1 = new Vector2(30, 20);
            var bRadius = 2.0f;

            var result = Collision2D.ContainsCapsuleCapsule(a0, a1, aRadius, b0, b1, bRadius);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsCapsuleCapsule_PartialOverlap_ReturnsIntersects()
        {
            var a0 = new Vector2(0, 0);
            var a1 = new Vector2(10, 0);
            var aRadius = 3.0f;
            var b0 = new Vector2(8, 0);
            var b1 = new Vector2(15, 0);
            var bRadius = 2.0f;

            var result = Collision2D.ContainsCapsuleCapsule(a0, a1, aRadius, b0, b1, bRadius);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsCapsuleConvexPolygon Tests

        [Test]
        public void ContainsCapsuleConvexPolygon_PolygonCompletelyInside_ReturnsContains()
        {
            var capsuleA = new Vector2(0, 10);
            var capsuleB = new Vector2(20, 10);
            var capsuleRadius = 8.0f;
            var vertices = new[]
            {
                new Vector2(8, 8),
                new Vector2(12, 8),
                new Vector2(12, 12),
                new Vector2(8, 12)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsCapsuleConvexPolygon(capsuleA, capsuleB, capsuleRadius, vertices, normals);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsCapsuleConvexPolygon_PolygonCompletelyOutside_ReturnsDisjoint()
        {
            var capsuleA = new Vector2(0, 0);
            var capsuleB = new Vector2(10, 0);
            var capsuleRadius = 2.0f;
            var vertices = new[]
            {
                new Vector2(20, 20),
                new Vector2(25, 20),
                new Vector2(25, 25),
                new Vector2(20, 25)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsCapsuleConvexPolygon(capsuleA, capsuleB, capsuleRadius, vertices, normals);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsCapsuleConvexPolygon_PolygonPartiallyInside_ReturnsIntersects()
        {
            var capsuleA = new Vector2(0, 5);
            var capsuleB = new Vector2(10, 5);
            var capsuleRadius = 3.0f;
            var vertices = new[]
            {
                new Vector2(5, 5),
                new Vector2(15, 5),
                new Vector2(15, 15),
                new Vector2(5, 15)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsCapsuleConvexPolygon(capsuleA, capsuleB, capsuleRadius, vertices, normals);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #endregion

        #region Containment - ConvexPolygon Methods

        #region ContainsConvexPolygonPoint Tests

        [Test]
        public void ContainsConvexPolygonPoint_PointInside_ReturnsContains()
        {
            var point = new Vector2(5, 5);
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsConvexPolygonPoint(point, vertices, normals);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsConvexPolygonPoint_PointOutside_ReturnsDisjoint()
        {
            var point = new Vector2(15, 15);
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsConvexPolygonPoint(point, vertices, normals);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsConvexPolygonPoint_PointOnEdge_ReturnsContains()
        {
            var point = new Vector2(10, 5);
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsConvexPolygonPoint(point, vertices, normals);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        #endregion

        #region ContainsConvexPolygonCapsule Tests

        [Test]
        public void ContainsConvexPolygonCapsule_CapsuleCompletelyInside_ReturnsContains()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(20, 0),
                new Vector2(20, 20),
                new Vector2(0, 20)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            var capsuleA = new Vector2(8, 10);
            var capsuleB = new Vector2(12, 10);
            var capsuleRadius = 2.0f;

            var result = Collision2D.ContainsConvexPolygonCapsule(vertices, normals, capsuleA, capsuleB, capsuleRadius);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsConvexPolygonCapsule_CapsuleCompletelyOutside_ReturnsDisjoint()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            var capsuleA = new Vector2(20, 20);
            var capsuleB = new Vector2(25, 25);
            var capsuleRadius = 1.0f;

            var result = Collision2D.ContainsConvexPolygonCapsule(vertices, normals, capsuleA, capsuleB, capsuleRadius);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsConvexPolygonCapsule_CapsulePartiallyInside_ReturnsIntersects()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            var capsuleA = new Vector2(5, 5);
            var capsuleB = new Vector2(15, 5);
            var capsuleRadius = 1.0f;

            var result = Collision2D.ContainsConvexPolygonCapsule(vertices, normals, capsuleA, capsuleB, capsuleRadius);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsConvexPolygonObb Tests

        [Test]
        public void ContainsConvexPolygonObb_ObbCompletelyInside_ReturnsContains()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(20, 0),
                new Vector2(20, 20),
                new Vector2(0, 20)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            var obbCenter = new Vector2(10, 10);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalf = new Vector2(3, 3);

            var result = Collision2D.ContainsConvexPolygonObb(vertices, normals, obbCenter, obbAxisX, obbAxisY, obbHalf);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsConvexPolygonObb_ObbCompletelyOutside_ReturnsDisjoint()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            var obbCenter = new Vector2(20, 20);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalf = new Vector2(2, 2);

            var result = Collision2D.ContainsConvexPolygonObb(vertices, normals, obbCenter, obbAxisX, obbAxisY, obbHalf);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsConvexPolygonObb_ObbPartiallyInside_ReturnsIntersects()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            var obbCenter = new Vector2(8, 8);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalf = new Vector2(5, 5);

            var result = Collision2D.ContainsConvexPolygonObb(vertices, normals, obbCenter, obbAxisX, obbAxisY, obbHalf);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #region ContainsConvexPolygonConvexPolygon Tests

        [Test]
        public void ContainsConvexPolygonConvexPolygon_CompletelyContains_ReturnsContains()
        {
            var aVertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(20, 0),
                new Vector2(20, 20),
                new Vector2(0, 20)
            };
            var aNormals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            var bVertices = new[]
            {
                new Vector2(5, 5),
                new Vector2(15, 5),
                new Vector2(15, 15),
                new Vector2(5, 15)
            };
            var bNormals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsConvexPolygonConvexPolygon(aVertices, aNormals, bVertices, bNormals);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsConvexPolygonConvexPolygon_Separated_ReturnsDisjoint()
        {
            var aVertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var aNormals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            var bVertices = new[]
            {
                new Vector2(20, 20),
                new Vector2(30, 20),
                new Vector2(30, 30),
                new Vector2(20, 30)
            };
            var bNormals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsConvexPolygonConvexPolygon(aVertices, aNormals, bVertices, bNormals);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsConvexPolygonConvexPolygon_PartialOverlap_ReturnsIntersects()
        {
            var aVertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var aNormals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            var bVertices = new[]
            {
                new Vector2(5, 5),
                new Vector2(15, 5),
                new Vector2(15, 15),
                new Vector2(5, 15)
            };
            var bNormals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            var result = Collision2D.ContainsConvexPolygonConvexPolygon(aVertices, aNormals, bVertices, bNormals);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        #endregion

        #endregion

        #region Projection Methods

        #region ProjectOntoAxis Tests

        [Test]
        public void ProjectOntoAxis_AlignedWithAxis_ReturnsCorrectInterval()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var axis = Vector2.UnitX;

            Collision2D.ProjectOntoAxis(vertices, axis, out float min, out float max);

            Assert.AreEqual(0.0f, min, Collision2D.Epsilon);
            Assert.AreEqual(10.0f, max, Collision2D.Epsilon);
        }

        [Test]
        public void ProjectOntoAxis_PerpendicularToAxis_ReturnsNarrowInterval()
        {
            var vertices = new[]
            {
                new Vector2(5, 0),
                new Vector2(5, 10)
            };
            var axis = Vector2.UnitX;

            Collision2D.ProjectOntoAxis(vertices, axis, out float min, out float max);

            Assert.AreEqual(5.0f, min, Collision2D.Epsilon);
            Assert.AreEqual(5.0f, max, Collision2D.Epsilon);
        }

        [Test]
        public void ProjectOntoAxis_DiagonalAxis_ReturnsCorrectInterval()
        {
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var axis = Vector2.Normalize(new Vector2(1, 1));

            Collision2D.ProjectOntoAxis(vertices, axis, out float min, out float max);

            float expected = 10.0f / MathF.Sqrt(2);
            Assert.AreEqual(0.0f, min, Collision2D.Epsilon);
            Assert.AreEqual(expected * 2, max, Collision2D.Epsilon);
        }

        #endregion

        #region ProjectAabbOntoAxis Tests

        [Test]
        public void ProjectAabbOntoAxis_AlignedAxis_ReturnsExtents()
        {
            var center = new Vector2(5, 5);
            var halfExtents = new Vector2(5, 5);
            var axis = Vector2.UnitX;

            Collision2D.ProjectAabbOntoAxis(center, halfExtents, axis, out float min, out float max);

            Assert.AreEqual(0.0f, min, Collision2D.Epsilon);
            Assert.AreEqual(10.0f, max, Collision2D.Epsilon);
        }

        [Test]
        public void ProjectAabbOntoAxis_DiagonalAxis_ReturnsCorrectInterval()
        {
            var center = new Vector2(5, 5);
            var halfExtents = new Vector2(5, 5);
            var axis = Vector2.Normalize(new Vector2(1, 1));

            Collision2D.ProjectAabbOntoAxis(center, halfExtents, axis, out float min, out float max);

            float centerProj = 5.0f * MathF.Sqrt(2);
            float radius = 5.0f * MathF.Sqrt(2);

            Assert.AreEqual(centerProj - radius, min, Collision2D.Epsilon);
            Assert.AreEqual(centerProj + radius, max, Collision2D.Epsilon);
        }

        #endregion

        #region ProjectObbOntoAxis Tests

        [Test]
        public void ProjectObbOntoAxis_AxisAlignedBox_MatchesAabbResult()
        {
            var center = new Vector2(5, 5);
            var axisX = Vector2.UnitX;
            var axisY = Vector2.UnitY;
            var halfExtents = new Vector2(5, 5);
            var projAxis = Vector2.UnitX;

            Collision2D.ProjectObbOntoAxis(center, axisX, axisY, halfExtents, projAxis, out float min, out float max);

            Assert.AreEqual(0.0f, min, Collision2D.Epsilon);
            Assert.AreEqual(10.0f, max, Collision2D.Epsilon);
        }

        [Test]
        public void ProjectObbOntoAxis_RotatedBox_ReturnsCorrectInterval()
        {
            var center = new Vector2(0, 0);
            var axisX = Vector2.Normalize(new Vector2(1, 1));
            var axisY = Vector2.Normalize(new Vector2(-1, 1));
            var halfExtents = new Vector2(5, 5);
            var projAxis = Vector2.UnitX;

            Collision2D.ProjectObbOntoAxis(center, axisX, axisY, halfExtents, projAxis, out float min, out float max);

            Assert.AreEqual(min, -max, Collision2D.Epsilon);
        }

        #endregion

        #endregion

        #region Distance Calculations

        #region DistanceSquaredPointSegment Tests

        [Test]
        public void DistanceSquaredPointSegment_PointOnSegment_ReturnsZero()
        {
            var point = new Vector2(5, 0);
            var a = new Vector2(0, 0);
            var b = new Vector2(10, 0);

            float distance = Collision2D.DistanceSquaredPointSegment(point, a, b, out float t, out Vector2 closestPoint);

            Assert.AreEqual(0.0f, distance, Collision2D.Epsilon);
            Assert.AreEqual(0.5f, t, Collision2D.Epsilon);
            Assert.AreEqual(point, closestPoint);
        }

        [Test]
        public void DistanceSquaredPointSegment_ClosestAtEndpoint_ClampsParameter()
        {
            var point = new Vector2(15, 5);
            var a = new Vector2(0, 0);
            var b = new Vector2(10, 0);

            float distance = Collision2D.DistanceSquaredPointSegment(point, a, b, out float t, out Vector2 closestPoint);

            Assert.AreEqual(50.0f, distance, Collision2D.Epsilon);
            Assert.AreEqual(1.0f, t);
            Assert.AreEqual(b, closestPoint);
        }

        [Test]
        public void DistanceSquaredPointSegment_PerpendicularDistance_ReturnsCorrectValue()
        {
            var point = new Vector2(5, 3);
            var a = new Vector2(0, 0);
            var b = new Vector2(10, 0);

            float distance = Collision2D.DistanceSquaredPointSegment(point, a, b, out float t, out Vector2 closestPoint);

            Assert.AreEqual(9.0f, distance, Collision2D.Epsilon);
            Assert.AreEqual(0.5f, t, Collision2D.Epsilon);
            Assert.AreEqual(new Vector2(5, 0), closestPoint);
        }

        [Test]
        public void DistanceSquaredPointSegment_DegenerateSegment_TreatsAsPoint()
        {
            var point = new Vector2(5, 5);
            var a = new Vector2(0, 0);
            var b = new Vector2(0, 0);

            float distance = Collision2D.DistanceSquaredPointSegment(point, a, b, out float t, out Vector2 closestPoint);

            Assert.AreEqual(50.0f, distance, Collision2D.Epsilon);
            Assert.AreEqual(0.0f, t);
            Assert.AreEqual(a, closestPoint);
        }

        #endregion

        #region DistanceSquaredSegmentSegment Tests

        [Test]
        public void DistanceSquaredSegmentSegment_IntersectingSegments_ReturnsZero()
        {
            var p1 = new Vector2(0, 5);
            var q1 = new Vector2(10, 5);
            var p2 = new Vector2(5, 0);
            var q2 = new Vector2(5, 10);

            float distance = Collision2D.DistanceSquaredSegmentSegment(p1, q1, p2, q2, out float s, out float t, out Vector2 c1, out Vector2 c2);

            Assert.AreEqual(0.0f, distance, Collision2D.Epsilon);
            Assert.AreEqual(new Vector2(5, 5), c1);
            Assert.AreEqual(new Vector2(5, 5), c2);
        }

        [Test]
        public void DistanceSquaredSegmentSegment_ParallelSegments_ReturnsCorrectDistance()
        {
            var p1 = new Vector2(0, 0);
            var q1 = new Vector2(10, 0);
            var p2 = new Vector2(0, 5);
            var q2 = new Vector2(10, 5);

            float distance = Collision2D.DistanceSquaredSegmentSegment(p1, q1, p2, q2, out float s, out float t, out Vector2 c1, out Vector2 c2);

            Assert.AreEqual(25.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceSquaredSegmentSegment_SkewSegments_ReturnsCorrectDistance()
        {
            var p1 = new Vector2(0, 0);
            var q1 = new Vector2(5, 0);
            var p2 = new Vector2(0, 2);
            var q2 = new Vector2(0, 5);

            float distance = Collision2D.DistanceSquaredSegmentSegment(p1, q1, p2, q2, out float s, out float t, out Vector2 c1, out Vector2 c2);

            Assert.AreEqual(4.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceSquaredSegmentSegment_DegenerateSegments_HandlesCorrectly()
        {
            var p1 = new Vector2(0, 0);
            var q1 = new Vector2(0, 0);
            var p2 = new Vector2(3, 4);
            var q2 = new Vector2(3, 4);

            float distance = Collision2D.DistanceSquaredSegmentSegment(p1, q1, p2, q2, out float s, out float t, out Vector2 c1, out Vector2 c2);

            Assert.AreEqual(25.0f, distance, Collision2D.Epsilon);
        }

        #endregion

        #region DistanceSquaredPointAabb Tests

        [Test]
        public void DistanceSquaredPointAabb_PointInside_ReturnsZero()
        {
            var point = new Vector2(5, 5);
            var min = new Vector2(0, 0);
            var max = new Vector2(10, 10);

            float distance = Collision2D.DistanceSquaredPointAabb(point, min, max);

            Assert.AreEqual(0.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceSquaredPointAabb_ClosestToCorner_ReturnsCorrectDistance()
        {
            var point = new Vector2(15, 15);
            var min = new Vector2(0, 0);
            var max = new Vector2(10, 10);

            float distance = Collision2D.DistanceSquaredPointAabb(point, min, max);

            Assert.AreEqual(50.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceSquaredPointAabb_ClosestToEdge_ReturnsCorrectDistance()
        {
            var point = new Vector2(5, 15);
            var min = new Vector2(0, 0);
            var max = new Vector2(10, 10);

            float distance = Collision2D.DistanceSquaredPointAabb(point, min, max);

            Assert.AreEqual(25.0f, distance, Collision2D.Epsilon);
        }

        #endregion

        #region DistanceSquaredPointObb Tests

        [Test]
        public void DistanceSquaredPointObb_PointInside_ReturnsZero()
        {
            var point = new Vector2(5, 5);
            var center = new Vector2(5, 5);
            var axisX = Vector2.UnitX;
            var axisY = Vector2.UnitY;
            var halfExtents = new Vector2(3, 3);

            float distance = Collision2D.DistanceSquaredPointObb(point, center, axisX, axisY, halfExtents);

            Assert.AreEqual(0.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceSquaredPointObb_PointOutside_ReturnsCorrectDistance()
        {
            var point = new Vector2(10, 5);
            var center = new Vector2(5, 5);
            var axisX = Vector2.UnitX;
            var axisY = Vector2.UnitY;
            var halfExtents = new Vector2(2, 2);

            float distance = Collision2D.DistanceSquaredPointObb(point, center, axisX, axisY, halfExtents);

            Assert.AreEqual(9.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceSquaredPointObb_RotatedBox_ReturnsCorrectDistance()
        {
            var point = new Vector2(10, 0);
            var center = new Vector2(0, 0);
            var axisX = Vector2.Normalize(new Vector2(1, 1));
            var axisY = Vector2.Normalize(new Vector2(-1, 1));
            var halfExtents = new Vector2(2, 2);

            float distance = Collision2D.DistanceSquaredPointObb(point, center, axisX, axisY, halfExtents);

            Assert.Greater(distance, 0.0f);
        }

        #endregion

        #region DistanceSquaredPointConvexPolygon Tests

        [Test]
        public void DistanceSquaredPointConvexPolygon_PointInside_ReturnsZero()
        {
            var point = new Vector2(5, 5);
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            float distance = Collision2D.DistanceSquaredPointConvexPolygon(point, vertices, normals);

            Assert.AreEqual(0.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceSquaredPointConvexPolygon_ClosestToEdge_ReturnsCorrectDistance()
        {
            var point = new Vector2(5, -3);
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            float distance = Collision2D.DistanceSquaredPointConvexPolygon(point, vertices, normals);

            Assert.AreEqual(9.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceSquaredPointConvexPolygon_ClosestToVertex_ReturnsCorrectDistance()
        {
            var point = new Vector2(-3, -3);
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            float distance = Collision2D.DistanceSquaredPointConvexPolygon(point, vertices, normals);

            Assert.AreEqual(18.0f, distance, Collision2D.Epsilon);
        }

        #endregion

        #region DistanceSquaredSegmentAabb Tests

        [Test]
        public void DistanceSquaredSegmentAabb_SegmentInsideBox_ReturnsZero()
        {
            var a = new Vector2(3, 3);
            var b = new Vector2(7, 7);
            var min = new Vector2(0, 0);
            var max = new Vector2(10, 10);

            float distance = Collision2D.DistanceSquaredSegmentAabb(a, b, min, max);

            Assert.AreEqual(0.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceSquaredSegmentAabb_SegmentIntersectsBox_ReturnsZero()
        {
            var a = new Vector2(-5, 5);
            var b = new Vector2(15, 5);
            var min = new Vector2(0, 0);
            var max = new Vector2(10, 10);

            float distance = Collision2D.DistanceSquaredSegmentAabb(a, b, min, max);

            Assert.AreEqual(0.0f, distance, Collision2D.Epsilon);
        }


        [Test]
        public void DistanceSquaredSegmentAabb_SegmentOutsideBox_ReturnsCorrectDistance()
        {
            var a = new Vector2(15, 5);
            var b = new Vector2(20, 5);
            var min = new Vector2(0, 0);
            var max = new Vector2(10, 10);

            float distance = Collision2D.DistanceSquaredSegmentAabb(a, b, min, max);

            Assert.AreEqual(25.0f, distance, Collision2D.Epsilon);
        }

        #endregion

        #region DistanceSquaredSegmentConvexPolygon Tests

        [Test]
        public void DistanceSquaredSegmentConvexPolygon_SegmentInside_ReturnsZero()
        {
            var a = new Vector2(3, 3);
            var b = new Vector2(7, 7);
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            float distance = Collision2D.DistanceSquaredSegmentConvexPolygon(a, b, vertices, normals);

            Assert.AreEqual(0.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceSquaredSegmentConvexPolygon_SegmentIntersects_ReturnsZero()
        {
            var a = new Vector2(-5, 5);
            var b = new Vector2(15, 5);
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            float distance = Collision2D.DistanceSquaredSegmentConvexPolygon(a, b, vertices, normals);

            Assert.AreEqual(0.0f, distance, Collision2D.Epsilon);
        }

        [Test]
        public void DistanceSquaredSegmentConvexPolygon_SegmentOutside_ReturnsCorrectDistance()
        {
            var a = new Vector2(15, 5);
            var b = new Vector2(20, 5);
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            float distance = Collision2D.DistanceSquaredSegmentConvexPolygon(a, b, vertices, normals);

            Assert.AreEqual(25.0f, distance, Collision2D.Epsilon);
        }

        #endregion

        #region ClosestPointRaySegment Tests

        [Test]
        public void ClosestPointRaySegment_RayIntersectsSegment_ReturnsZeroDistance()
        {
            var rayOrigin = new Vector2(0, 5);
            var rayDirection = Vector2.UnitX;
            var segA = new Vector2(5, 0);
            var segB = new Vector2(5, 10);

            Collision2D.ClosestPointRaySegment(rayOrigin, rayDirection, segA, segB, out float sRay, out float tSeg, out float distanceSquared);

            Assert.AreEqual(0.0f, distanceSquared, Collision2D.Epsilon);
            Assert.AreEqual(5.0f, sRay, Collision2D.Epsilon);
            Assert.AreEqual(0.5f, tSeg, Collision2D.Epsilon);
        }

        [Test]
        public void ClosestPointRaySegment_RayParallelToSegment_ReturnsCorrectDistance()
        {
            var rayOrigin = new Vector2(0, 0);
            var rayDirection = Vector2.UnitX;
            var segA = new Vector2(0, 5);
            var segB = new Vector2(10, 5);

            Collision2D.ClosestPointRaySegment(rayOrigin, rayDirection, segA, segB, out float sRay, out float tSeg, out float distanceSquared);

            Assert.AreEqual(25.0f, distanceSquared, Collision2D.Epsilon);
        }

        [Test]
        public void ClosestPointRaySegment_RayMissesSegment_ReturnsCorrectDistance()
        {
            var rayOrigin = new Vector2(0, 0);
            var rayDirection = Vector2.UnitX;
            var segA = new Vector2(5, 5);
            var segB = new Vector2(10, 5);

            Collision2D.ClosestPointRaySegment(rayOrigin, rayDirection, segA, segB, out float sRay, out float tSeg, out float distanceSquared);

            Assert.Greater(distanceSquared, 0.0f);
        }

        #endregion

        #endregion

        #region Parametric Solvers

        #region SolveParametricIntersection2D Tests

        [Test]
        public void SolveParametricIntersection2D_PerpendicularLines_FindsIntersection()
        {
            var origin1 = new Vector2(0, 5);
            var direction1 = Vector2.UnitX;
            var origin2 = new Vector2(5, 0);
            var direction2 = Vector2.UnitY;

            bool result = Collision2D.SolveParametricIntersection2D(origin1, direction1, origin2, direction2, out float t1, out float t2);

            Assert.IsTrue(result);
            Assert.AreEqual(5.0f, t1, Collision2D.Epsilon);
            Assert.AreEqual(5.0f, t2, Collision2D.Epsilon);
        }

        [Test]
        public void SolveParametricIntersection2D_ParallelLines_ReturnsFalse()
        {
            var origin1 = new Vector2(0, 0);
            var direction1 = Vector2.UnitX;
            var origin2 = new Vector2(0, 5);
            var direction2 = Vector2.UnitX;

            bool result = Collision2D.SolveParametricIntersection2D(origin1, direction1, origin2, direction2, out float t1, out float t2);

            Assert.IsFalse(result);
        }

        [Test]
        public void SolveParametricIntersection2D_CollinearLines_ReturnsFalse()
        {
            var origin1 = new Vector2(0, 0);
            var direction1 = Vector2.UnitX;
            var origin2 = new Vector2(5, 0);
            var direction2 = Vector2.UnitX;

            bool result = Collision2D.SolveParametricIntersection2D(origin1, direction1, origin2, direction2, out float t1, out float t2);

            Assert.IsFalse(result);
        }

        [Test]
        public void SolveParametricIntersection2D_LinesAtAnAngle_FindsIntersection()
        {
            var origin1 = new Vector2(0, 0);
            var direction1 = new Vector2(1, 0);
            var origin2 = new Vector2(0, 0);
            var direction2 = new Vector2(0, 1);

            bool result = Collision2D.SolveParametricIntersection2D(origin1, direction1, origin2, direction2, out float t1, out float t2);

            Assert.IsTrue(result);
            Assert.AreEqual(0.0f, t1, Collision2D.Epsilon);
            Assert.AreEqual(0.0f, t2, Collision2D.Epsilon);
        }

        #endregion

        #region SolveParametricIntersectionWithImplicitLine Tests

        [Test]
        public void SolveParametricIntersectionWithImplicitLine_Intersecting_FindsParameter()
        {
            var lineNormal = Vector2.UnitY;
            var lineDistance = 5.0f;
            var origin = new Vector2(0, 0);
            var direction = Vector2.UnitY;

            bool result = Collision2D.SolveParametricIntersectionWithImplicitLine(lineNormal, lineDistance, origin, direction, out float t);

            Assert.IsTrue(result);
            Assert.AreEqual(5.0f, t, Collision2D.Epsilon);
        }

        [Test]
        public void SolveParametricIntersectionWithImplicitLine_Parallel_ReturnsFalse()
        {
            var lineNormal = Vector2.UnitY;
            var lineDistance = 5.0f;
            var origin = new Vector2(0, 0);
            var direction = Vector2.UnitX;

            bool result = Collision2D.SolveParametricIntersectionWithImplicitLine(lineNormal, lineDistance, origin, direction, out float t);

            Assert.IsFalse(result);
        }

        [Test]
        public void SolveParametricIntersectionWithImplicitLine_Perpendicular_FindsParameter()
        {
            var lineNormal = Vector2.UnitX;
            var lineDistance = 10.0f;
            var origin = new Vector2(0, 0);
            var direction = Vector2.UnitX;

            bool result = Collision2D.SolveParametricIntersectionWithImplicitLine(lineNormal, lineDistance, origin, direction, out float t);

            Assert.IsTrue(result);
            Assert.AreEqual(10.0f, t, Collision2D.Epsilon);
        }

        #endregion

        #endregion

        #region Clipping Methods

        #region ClipLineToAabb Tests

        [Test]
        public void ClipLineToAabb_LineCompletelyInside_ReturnsFullInterval()
        {
            var origin = new Vector2(5, 5);
            var direction = Vector2.UnitX;
            var min = new Vector2(0, 0);
            var max = new Vector2(20, 20);
            float tLower = 0.0f;
            float tUpper = 10.0f;

            bool result = Collision2D.ClipLineToAabb(origin, direction, min, max, tLower, tUpper, out float tEnter, out float tExit);

            Assert.IsTrue(result);
            Assert.AreEqual(0.0f, tEnter, Collision2D.Epsilon);
            Assert.AreEqual(10.0f, tExit, Collision2D.Epsilon);
        }

        [Test]
        public void ClipLineToAabb_LinePartiallyInside_ReturnsClippedInterval()
        {
            var origin = new Vector2(-5, 5);
            var direction = Vector2.UnitX;
            var min = new Vector2(0, 0);
            var max = new Vector2(10, 10);
            float tLower = 0.0f;
            float tUpper = 20.0f;

            bool result = Collision2D.ClipLineToAabb(origin, direction, min, max, tLower, tUpper, out float tEnter, out float tExit);

            Assert.IsTrue(result);
            Assert.AreEqual(5.0f, tEnter, Collision2D.Epsilon);
            Assert.AreEqual(15.0f, tExit, Collision2D.Epsilon);
        }

        [Test]
        public void ClipLineToAabb_LineMissesBox_ReturnsFalse()
        {
            var origin = new Vector2(0, 20);
            var direction = Vector2.UnitX;
            var min = new Vector2(0, 0);
            var max = new Vector2(10, 10);
            float tLower = 0.0f;
            float tUpper = 10.0f;

            bool result = Collision2D.ClipLineToAabb(origin, direction, min, max, tLower, tUpper, out float tEnter, out float tExit);

            Assert.IsFalse(result);
        }

        [Test]
        public void ClipLineToAabb_LineThroughCorner_ReturnsInterval()
        {
            var origin = new Vector2(0, 0);
            var direction = Vector2.Normalize(new Vector2(1, 1));
            var min = new Vector2(5, 5);
            var max = new Vector2(15, 15);
            float tLower = 0.0f;
            float tUpper = 50.0f;

            bool result = Collision2D.ClipLineToAabb(origin, direction, min, max, tLower, tUpper, out float tEnter, out float tExit);

            Assert.IsTrue(result);
        }

        #endregion

        #region ClipLineToConvexPolygon Tests

        [Test]
        public void ClipLineToConvexPolygon_LineCompletelyInside_ReturnsFullInterval()
        {
            var origin = new Vector2(5, 5);
            var direction = Vector2.UnitX;
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(20, 0),
                new Vector2(20, 20),
                new Vector2(0, 20)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            float tLower = 0.0f;
            float tUpper = 10.0f;

            bool result = Collision2D.ClipLineToConvexPolygon(origin, direction, vertices, normals, tLower, tUpper, out float tEnter, out float tExit);

            Assert.IsTrue(result);
            Assert.AreEqual(0.0f, tEnter, Collision2D.Epsilon);
            Assert.AreEqual(10.0f, tExit, Collision2D.Epsilon);
        }

        [Test]
        public void ClipLineToConvexPolygon_LinePartiallyInside_ReturnsClippedInterval()
        {
            var origin = new Vector2(-5, 5);
            var direction = Vector2.UnitX;
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            float tLower = 0.0f;
            float tUpper = 20.0f;

            bool result = Collision2D.ClipLineToConvexPolygon(origin, direction, vertices, normals, tLower, tUpper, out float tEnter, out float tExit);

            Assert.IsTrue(result);
            Assert.AreEqual(5.0f, tEnter, Collision2D.Epsilon);
            Assert.AreEqual(15.0f, tExit, Collision2D.Epsilon);
        }

        [Test]
        public void ClipLineToConvexPolygon_LineMissesPolygon_ReturnsFalse()
        {
            var origin = new Vector2(0, 20);
            var direction = Vector2.UnitX;
            var vertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            float tLower = 0.0f;
            float tUpper = 10.0f;

            bool result = Collision2D.ClipLineToConvexPolygon(origin, direction, vertices, normals, tLower, tUpper, out float tEnter, out float tExit);

            Assert.IsFalse(result);
        }

        #endregion

        #endregion

        #region Ray Interval Methods

        #region RayCircleIntersectionInterval Tests

        [Test]
        public void RayCircleIntersectionInterval_TwoIntersections_ReturnsBothParameters()
        {
            var origin = new Vector2(-10, 0);
            var direction = Vector2.UnitX;
            var center = new Vector2(0, 0);
            var radius = 5.0f;

            bool result = Collision2D.RayCircleIntersectionInterval(origin, direction, center, radius, out float tMin, out float tMax);

            Assert.IsTrue(result);
            Assert.AreEqual(5.0f, tMin, Collision2D.Epsilon);
            Assert.AreEqual(15.0f, tMax, Collision2D.Epsilon);
        }

        [Test]
        public void RayCircleIntersectionInterval_Tangent_ReturnsSinglePoint()
        {
            var origin = new Vector2(-10, 5);
            var direction = Vector2.UnitX;
            var center = new Vector2(0, 0);
            var radius = 5.0f;

            bool result = Collision2D.RayCircleIntersectionInterval(origin, direction, center, radius, out float tMin, out float tMax);

            Assert.IsTrue(result);
            Assert.AreEqual(tMin, tMax, Collision2D.Epsilon);
        }

        [Test]
        public void RayCircleIntersectionInterval_Miss_ReturnsFalse()
        {
            var origin = new Vector2(-10, 10);
            var direction = Vector2.UnitX;
            var center = new Vector2(0, 0);
            var radius = 5.0f;

            bool result = Collision2D.RayCircleIntersectionInterval(origin, direction, center, radius, out float tMin, out float tMax);

            Assert.IsFalse(result);
        }

        [Test]
        public void RayCircleIntersectionInterval_OriginInside_ReturnsIntervalFromZero()
        {
            var origin = new Vector2(0, 0);
            var direction = Vector2.UnitX;
            var center = new Vector2(0, 0);
            var radius = 5.0f;

            bool result = Collision2D.RayCircleIntersectionInterval(origin, direction, center, radius, out float tMin, out float tMax);

            Assert.IsTrue(result);
            Assert.AreEqual(0.0f, tMin, Collision2D.Epsilon);
            Assert.AreEqual(5.0f, tMax, Collision2D.Epsilon);
        }

        #endregion

        #region RayCapsuleIntersectionInterval Tests

        [Test]
        public void RayCapsuleIntersectionInterval_HitsCylinder_ReturnsInterval()
        {
            var rayOrigin = new Vector2(-10, 0);
            var rayDirection = Vector2.UnitX;
            var segA = new Vector2(0, -5);
            var segB = new Vector2(0, 5);
            var radius = 3.0f;

            bool result = Collision2D.RayCapsuleIntersectionInterval(rayOrigin, rayDirection, segA, segB, radius, out float tMin, out float tMax);

            Assert.IsTrue(result);
            Assert.Greater(tMax, tMin);
        }

        [Test]
        public void RayCapsuleIntersectionInterval_HitsEndCap_ReturnsInterval()
        {
            var rayOrigin = new Vector2(-10, 10);
            var rayDirection = Vector2.UnitX;
            var segA = new Vector2(0, 10);
            var segB = new Vector2(10, 10);
            var radius = 3.0f;

            bool result = Collision2D.RayCapsuleIntersectionInterval(rayOrigin, rayDirection, segA, segB, radius, out float tMin, out float tMax);

            Assert.IsTrue(result);
        }

        [Test]
        public void RayCapsuleIntersectionInterval_Miss_ReturnsFalse()
        {
            var rayOrigin = new Vector2(-10, 20);
            var rayDirection = Vector2.UnitX;
            var segA = new Vector2(0, 0);
            var segB = new Vector2(10, 0);
            var radius = 3.0f;

            bool result = Collision2D.RayCapsuleIntersectionInterval(rayOrigin, rayDirection, segA, segB, radius, out float tMin, out float tMax);

            Assert.IsFalse(result);
        }

        [Test]
        public void RayCapsuleIntersectionInterval_OriginInside_ReturnsIntervalFromZero()
        {
            var rayOrigin = new Vector2(5, 5);
            var rayDirection = Vector2.UnitX;
            var segA = new Vector2(0, 5);
            var segB = new Vector2(10, 5);
            var radius = 3.0f;

            bool result = Collision2D.RayCapsuleIntersectionInterval(rayOrigin, rayDirection, segA, segB, radius, out float tMin, out float tMax);

            Assert.IsTrue(result);
            Assert.AreEqual(0.0f, tMin, Collision2D.Epsilon);
        }

        [Test]
        public void RayCapsuleIntersectionInterval_DegenerateCapsule_BehavesLikeCircle()
        {
            var rayOrigin = new Vector2(-10, 0);
            var rayDirection = Vector2.UnitX;
            var segA = new Vector2(0, 0);
            var segB = new Vector2(0, 0);
            var radius = 5.0f;

            bool result = Collision2D.RayCapsuleIntersectionInterval(rayOrigin, rayDirection, segA, segB, radius, out float tMin, out float tMax);

            Assert.IsTrue(result);
            Assert.AreEqual(5.0f, tMin, Collision2D.Epsilon);
            Assert.AreEqual(15.0f, tMax, Collision2D.Epsilon);
        }

        #endregion

        #endregion

        #region Overlap Methods

        #region OverlapOnAxis Tests

        [Test]
        public void OverlapOnAxis_TwoPolygonsOverlapping_ReturnsTrue()
        {
            var aVerts = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var bVerts = new[]
            {
                new Vector2(5, 5),
                new Vector2(15, 5),
                new Vector2(15, 15),
                new Vector2(5, 15)
            };
            var axis = Vector2.UnitX;

            bool result = Collision2D.OverlapOnAxis(aVerts, bVerts, axis);

            Assert.IsTrue(result);
        }

        [Test]
        public void OverlapOnAxis_TwoPolygonsSeparated_ReturnsFalse()
        {
            var aVerts = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var bVerts = new[]
            {
                new Vector2(20, 20),
                new Vector2(30, 20),
                new Vector2(30, 30),
                new Vector2(20, 30)
            };
            var axis = Vector2.UnitX;

            bool result = Collision2D.OverlapOnAxis(aVerts, bVerts, axis);

            Assert.IsFalse(result);
        }

        [Test]
        public void OverlapOnAxis_PolygonsTouching_ReturnsTrue()
        {
            var aVerts = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var bVerts = new[]
            {
                new Vector2(10, 0),
                new Vector2(20, 0),
                new Vector2(20, 10),
                new Vector2(10, 10)
            };
            var axis = Vector2.UnitX;

            bool result = Collision2D.OverlapOnAxis(aVerts, bVerts, axis);

            Assert.IsTrue(result);
        }

        #endregion

        #region OverlapOnAxisAabbPolygon Tests

        [Test]
        public void OverlapOnAxisAabbPolygon_Overlapping_ReturnsTrue()
        {
            var aabbCenter = new Vector2(5, 5);
            var aabbHalfExtents = new Vector2(5, 5);
            var polygonVertices = new[]
            {
                new Vector2(8, 8),
                new Vector2(15, 8),
                new Vector2(15, 15),
                new Vector2(8, 15)
            };
            var axis = Vector2.UnitX;

            bool result = Collision2D.OverlapOnAxis(aabbCenter, aabbHalfExtents, polygonVertices, axis);

            Assert.IsTrue(result);
        }

        [Test]
        public void OverlapOnAxisAabbPolygon_Separated_ReturnsFalse()
        {
            var aabbCenter = new Vector2(5, 5);
            var aabbHalfExtents = new Vector2(5, 5);
            var polygonVertices = new[]
            {
                new Vector2(20, 20),
                new Vector2(30, 20),
                new Vector2(30, 30),
                new Vector2(20, 30)
            };
            var axis = Vector2.UnitX;

            bool result = Collision2D.OverlapOnAxis(aabbCenter, aabbHalfExtents, polygonVertices, axis);

            Assert.IsFalse(result);
        }

        #endregion

        #endregion

        #region Intersection Methods

        #region IntersectsAabbAabb Tests

        [Test]
        public void IntersectsAabbAabb_Overlapping_ReturnsTrue()
        {
            var aMin = new Vector2(0, 0);
            var aMax = new Vector2(10, 10);
            var bMin = new Vector2(5, 5);
            var bMax = new Vector2(15, 15);

            bool result = Collision2D.IntersectsAabbAabb(aMin, aMax, bMin, bMax);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsAabbAabb_Separated_ReturnsFalse()
        {
            var aMin = new Vector2(0, 0);
            var aMax = new Vector2(10, 10);
            var bMin = new Vector2(20, 20);
            var bMax = new Vector2(30, 30);

            bool result = Collision2D.IntersectsAabbAabb(aMin, aMax, bMin, bMax);

            Assert.IsFalse(result);
        }

        #endregion

        #region IntersectsAabbCapsule Tests

        [Test]
        public void IntersectsAabbCapsule_Overlapping_ReturnsTrue()
        {
            var boxMin = new Vector2(0, 0);
            var boxMax = new Vector2(10, 10);
            var capsuleA = new Vector2(5, 5);
            var capsuleB = new Vector2(15, 5);
            var capsuleRadius = 2.0f;

            bool result = Collision2D.IntersectsAabbCapsule(boxMin, boxMax, capsuleA, capsuleB, capsuleRadius);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsAabbCapsule_Separated_ReturnsFalse()
        {
            var boxMin = new Vector2(0, 0);
            var boxMax = new Vector2(10, 10);
            var capsuleA = new Vector2(20, 20);
            var capsuleB = new Vector2(30, 30);
            var capsuleRadius = 2.0f;

            bool result = Collision2D.IntersectsAabbCapsule(boxMin, boxMax, capsuleA, capsuleB, capsuleRadius);

            Assert.IsFalse(result);
        }

        #endregion

        #region IntersectsAabbConvexPolygon Tests

        [Test]
        public void IntersectsAabbConvexPolygon_Overlapping_ReturnsTrue()
        {
            var aabbCenter = new Vector2(5, 5);
            var aabbHalfExtents = new Vector2(5, 5);
            var vertices = new[]
            {
                new Vector2(8, 8),
                new Vector2(15, 8),
                new Vector2(15, 15),
                new Vector2(8, 15)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            bool result = Collision2D.IntersectsAabbConvexPolygon(aabbCenter, aabbHalfExtents, vertices, normals);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsAabbConvexPolygon_Separated_ReturnsFalse()
        {
            var aabbCenter = new Vector2(5, 5);
            var aabbHalfExtents = new Vector2(5, 5);
            var vertices = new[]
            {
                new Vector2(20, 20),
                new Vector2(30, 20),
                new Vector2(30, 30),
                new Vector2(20, 30)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            bool result = Collision2D.IntersectsAabbConvexPolygon(aabbCenter, aabbHalfExtents, vertices, normals);

            Assert.IsFalse(result);
        }

        #endregion

        #region IntersectsAabbObb Tests

        [Test]
        public void IntersectsAabbObb_Overlapping_ReturnsTrue()
        {
            var aabbCenter = new Vector2(5, 5);
            var aabbHalfExtents = new Vector2(5, 5);
            var obbCenter = new Vector2(8, 8);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(3, 3);

            bool result = Collision2D.IntersectsAabbObb(aabbCenter, aabbHalfExtents, obbCenter, obbAxisX, obbAxisY, obbHalfExtents);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsAabbObb_Separated_ReturnsFalse()
        {
            var aabbCenter = new Vector2(5, 5);
            var aabbHalfExtents = new Vector2(5, 5);
            var obbCenter = new Vector2(20, 20);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(3, 3);

            bool result = Collision2D.IntersectsAabbObb(aabbCenter, aabbHalfExtents, obbCenter, obbAxisX, obbAxisY, obbHalfExtents);

            Assert.IsFalse(result);
        }

        #endregion

        #region IntersectsCircleCircle Tests

        [Test]
        public void IntersectsCircleCircle_Overlapping_ReturnsTrue()
        {
            var aCenter = new Vector2(0, 0);
            var aRadius = 5.0f;
            var bCenter = new Vector2(8, 0);
            var bRadius = 5.0f;

            bool result = Collision2D.IntersectsCircleCircle(aCenter, aRadius, bCenter, bRadius);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsCircleCircle_Separated_ReturnsFalse()
        {
            var aCenter = new Vector2(0, 0);
            var aRadius = 5.0f;
            var bCenter = new Vector2(20, 0);
            var bRadius = 5.0f;

            bool result = Collision2D.IntersectsCircleCircle(aCenter, aRadius, bCenter, bRadius);

            Assert.IsFalse(result);
        }

        [Test]
        public void IntersectsCircleCircle_Touching_ReturnsTrue()
        {
            var aCenter = new Vector2(0, 0);
            var aRadius = 5.0f;
            var bCenter = new Vector2(10, 0);
            var bRadius = 5.0f;

            bool result = Collision2D.IntersectsCircleCircle(aCenter, aRadius, bCenter, bRadius);

            Assert.IsTrue(result);
        }

        #endregion

        #region IntersectsCircleAabb Tests

        [Test]
        public void IntersectsCircleAabb_Overlapping_ReturnsTrue()
        {
            var circleCenter = new Vector2(5, 5);
            var circleRadius = 5.0f;
            var boxMin = new Vector2(0, 0);
            var boxMax = new Vector2(10, 10);

            bool result = Collision2D.IntersectsCircleAabb(circleCenter, circleRadius, boxMin, boxMax);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsCircleAabb_Separated_ReturnsFalse()
        {
            var circleCenter = new Vector2(0, 0);
            var circleRadius = 5.0f;
            var boxMin = new Vector2(20, 20);
            var boxMax = new Vector2(30, 30);

            bool result = Collision2D.IntersectsCircleAabb(circleCenter, circleRadius, boxMin, boxMax);

            Assert.IsFalse(result);
        }

        #endregion

        #region IntersectsCircleObb Tests

        [Test]
        public void IntersectsCircleObb_Overlapping_ReturnsTrue()
        {
            var circleCenter = new Vector2(5, 5);
            var circleRadius = 5.0f;
            var obbCenter = new Vector2(8, 8);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(3, 3);

            bool result = Collision2D.IntersectsCircleObb(circleCenter, circleRadius, obbCenter, obbAxisX, obbAxisY, obbHalfExtents);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsCircleObb_Separated_ReturnsFalse()
        {
            var circleCenter = new Vector2(0, 0);
            var circleRadius = 5.0f;
            var obbCenter = new Vector2(20, 20);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(3, 3);

            bool result = Collision2D.IntersectsCircleObb(circleCenter, circleRadius, obbCenter, obbAxisX, obbAxisY, obbHalfExtents);

            Assert.IsFalse(result);
        }

        #endregion

        #region IntersectsCircleConvexPolygon Tests

        [Test]
        public void IntersectsCircleConvexPolygon_Overlapping_ReturnsTrue()
        {
            var circleCenter = new Vector2(5, 5);
            var circleRadius = 5.0f;
            var vertices = new[]
            {
                new Vector2(8, 8),
                new Vector2(15, 8),
                new Vector2(15, 15),
                new Vector2(8, 15)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            bool result = Collision2D.IntersectsCircleConvexPolygon(circleCenter, circleRadius, vertices, normals);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsCircleConvexPolygon_Separated_ReturnsFalse()
        {
            var circleCenter = new Vector2(0, 0);
            var circleRadius = 5.0f;
            var vertices = new[]
            {
                new Vector2(20, 20),
                new Vector2(30, 20),
                new Vector2(30, 30),
                new Vector2(20, 30)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            bool result = Collision2D.IntersectsCircleConvexPolygon(circleCenter, circleRadius, vertices, normals);

            Assert.IsFalse(result);
        }

        #endregion

        #region IntersectsCircleCapsule Tests

        [Test]
        public void IntersectsCircleCapsule_Overlapping_ReturnsTrue()
        {
            var circleCenter = new Vector2(5, 5);
            var circleRadius = 5.0f;
            var capsuleA = new Vector2(8, 8);
            var capsuleB = new Vector2(15, 8);
            var capsuleRadius = 2.0f;

            bool result = Collision2D.IntersectsCircleCapsule(circleCenter, circleRadius, capsuleA, capsuleB, capsuleRadius);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsCircleCapsule_Separated_ReturnsFalse()
        {
            var circleCenter = new Vector2(0, 0);
            var circleRadius = 5.0f;
            var capsuleA = new Vector2(20, 20);
            var capsuleB = new Vector2(30, 30);
            var capsuleRadius = 2.0f;

            bool result = Collision2D.IntersectsCircleCapsule(circleCenter, circleRadius, capsuleA, capsuleB, capsuleRadius);

            Assert.IsFalse(result);
        }

        #endregion

        #region IntersectsObbObb Tests

        [Test]
        public void IntersectsObbObb_Overlapping_ReturnsTrue()
        {
            var aCenter = new Vector2(5, 5);
            var aAxisX = Vector2.UnitX;
            var aAxisY = Vector2.UnitY;
            var aHalf = new Vector2(5, 5);
            var bCenter = new Vector2(8, 8);
            var bAxisX = Vector2.UnitX;
            var bAxisY = Vector2.UnitY;
            var bHalf = new Vector2(3, 3);

            bool result = Collision2D.IntersectsObbObb(aCenter, aAxisX, aAxisY, aHalf, bCenter, bAxisX, bAxisY, bHalf);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsObbObb_Separated_ReturnsFalse()
        {
            var aCenter = new Vector2(0, 0);
            var aAxisX = Vector2.UnitX;
            var aAxisY = Vector2.UnitY;
            var aHalf = new Vector2(5, 5);
            var bCenter = new Vector2(20, 20);
            var bAxisX = Vector2.UnitX;
            var bAxisY = Vector2.UnitY;
            var bHalf = new Vector2(3, 3);

            bool result = Collision2D.IntersectsObbObb(aCenter, aAxisX, aAxisY, aHalf, bCenter, bAxisX, bAxisY, bHalf);

            Assert.IsFalse(result);
        }

        #endregion

        #region IntersectsObbCapsule Tests

        [Test]
        public void IntersectsObbCapsule_Overlapping_ReturnsTrue()
        {
            var obbCenter = new Vector2(5, 5);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(5, 5);
            var capsuleA = new Vector2(8, 8);
            var capsuleB = new Vector2(15, 8);
            var capsuleRadius = 2.0f;

            bool result = Collision2D.IntersectsObbCapsule(obbCenter, obbAxisX, obbAxisY, obbHalfExtents, capsuleA, capsuleB, capsuleRadius);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsObbCapsule_Separated_ReturnsFalse()
        {
            var obbCenter = new Vector2(0, 0);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(5, 5);
            var capsuleA = new Vector2(20, 20);
            var capsuleB = new Vector2(30, 30);
            var capsuleRadius = 2.0f;

            bool result = Collision2D.IntersectsObbCapsule(obbCenter, obbAxisX, obbAxisY, obbHalfExtents, capsuleA, capsuleB, capsuleRadius);

            Assert.IsFalse(result);
        }

        #endregion

        #region IntersectsObbConvexPolygon Tests

        [Test]
        public void IntersectsObbConvexPolygon_Overlapping_ReturnsTrue()
        {
            var obbCenter = new Vector2(5, 5);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(5, 5);
            var vertices = new[]
            {
                new Vector2(8, 8),
                new Vector2(15, 8),
                new Vector2(15, 15),
                new Vector2(8, 15)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            bool result = Collision2D.IntersectsObbConvexPolygon(obbCenter, obbAxisX, obbAxisY, obbHalfExtents, vertices, normals);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsObbConvexPolygon_Separated_ReturnsFalse()
        {
            var obbCenter = new Vector2(0, 0);
            var obbAxisX = Vector2.UnitX;
            var obbAxisY = Vector2.UnitY;
            var obbHalfExtents = new Vector2(5, 5);
            var vertices = new[]
            {
                new Vector2(20, 20),
                new Vector2(30, 20),
                new Vector2(30, 30),
                new Vector2(20, 30)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            bool result = Collision2D.IntersectsObbConvexPolygon(obbCenter, obbAxisX, obbAxisY, obbHalfExtents, vertices, normals);

            Assert.IsFalse(result);
        }

        #endregion

        #region IntersectsCapsuleCapsule Tests

        [Test]
        public void IntersectsCapsuleCapsule_Overlapping_ReturnsTrue()
        {
            var a0 = new Vector2(0, 0);
            var a1 = new Vector2(10, 0);
            var aRadius = 3.0f;
            var b0 = new Vector2(8, 0);
            var b1 = new Vector2(15, 0);
            var bRadius = 3.0f;

            bool result = Collision2D.IntersectsCapsuleCapsule(a0, a1, aRadius, b0, b1, bRadius);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsCapsuleCapsule_Separated_ReturnsFalse()
        {
            var a0 = new Vector2(0, 0);
            var a1 = new Vector2(10, 0);
            var aRadius = 2.0f;
            var b0 = new Vector2(20, 20);
            var b1 = new Vector2(30, 30);
            var bRadius = 2.0f;

            bool result = Collision2D.IntersectsCapsuleCapsule(a0, a1, aRadius, b0, b1, bRadius);

            Assert.IsFalse(result);
        }

        #endregion

        #region IntersectsCapsuleConvexPolygon Tests

        [Test]
        public void IntersectsCapsuleConvexPolygon_Overlapping_ReturnsTrue()
        {
            var capsuleA = new Vector2(5, 5);
            var capsuleB = new Vector2(15, 5);
            var capsuleRadius = 3.0f;
            var vertices = new[]
            {
                new Vector2(8, 8),
                new Vector2(15, 8),
                new Vector2(15, 15),
                new Vector2(8, 15)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            bool result = Collision2D.IntersectsCapsuleConvexPolygon(capsuleA, capsuleB, capsuleRadius, vertices, normals);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsCapsuleConvexPolygon_Separated_ReturnsFalse()
        {
            var capsuleA = new Vector2(0, 0);
            var capsuleB = new Vector2(10, 0);
            var capsuleRadius = 2.0f;
            var vertices = new[]
            {
                new Vector2(20, 20),
                new Vector2(30, 20),
                new Vector2(30, 30),
                new Vector2(20, 30)
            };
            var normals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            bool result = Collision2D.IntersectsCapsuleConvexPolygon(capsuleA, capsuleB, capsuleRadius, vertices, normals);

            Assert.IsFalse(result);
        }

        #endregion

        #region IntersectsConvexPolygonConvexPolygon Tests

        [Test]
        public void IntersectsConvexPolygonConvexPolygon_Overlapping_ReturnsTrue()
        {
            var aVertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var aNormals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            var bVertices = new[]
            {
                new Vector2(5, 5),
                new Vector2(15, 5),
                new Vector2(15, 15),
                new Vector2(5, 15)
            };
            var bNormals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            bool result = Collision2D.IntersectsConvexPolygonConvexPolygon(aVertices, aNormals, bVertices, bNormals);

            Assert.IsTrue(result);
        }

        [Test]
        public void IntersectsConvexPolygonConvexPolygon_Separated_ReturnsFalse()
        {
            var aVertices = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            var aNormals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };
            var bVertices = new[]
            {
                new Vector2(20, 20),
                new Vector2(30, 20),
                new Vector2(30, 30),
                new Vector2(20, 30)
            };
            var bNormals = new[]
            {
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0)
            };

            bool result = Collision2D.IntersectsConvexPolygonConvexPolygon(aVertices, aNormals, bVertices, bNormals);

            Assert.IsFalse(result);
        }

        #endregion

        #endregion
    }
}
