// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class BoundingBox2DTest
    {
        #region Constructor Tests

        [Test]
        public void Constructor()
        {
            var min = new Vector2(1, 2);
            var max = new Vector2(10, 20);

            var box = new BoundingBox2D(min, max);

            Assert.AreEqual(min, box.Min);
            Assert.AreEqual(max, box.Max);
        }

        #endregion

        #region Computed Property Tests

        [Test]
        public void Center_ReturnsMiddlePoint()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));

            var center = box.Center;

            Assert.AreEqual(new Vector2(5, 10), center);
        }

        [Test]
        public void Size_ReturnsWidthAndHeight()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));

            var size = box.Size;

            Assert.AreEqual(new Vector2(10, 20), size);
        }

        [Test]
        public void HalfExtents_ReturnsHalfSize()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));

            var halfExtents = box.HalfExtents;

            Assert.AreEqual(new Vector2(5, 10), halfExtents);
        }

        [Test]
        public void Width_ReturnsHorizontalExtent()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));

            float width = box.Width;

            Assert.AreEqual(10.0f, width, Collision2D.Epsilon);
        }

        [Test]
        public void Height_ReturnsVerticalExtent()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));

            float height = box.Height;

            Assert.AreEqual(20.0f, height, Collision2D.Epsilon);
        }

        [Test]
        public void Area_ReturnsWidthTimesHeight()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));

            float area = box.Area;

            Assert.AreEqual(200.0f, area, Collision2D.Epsilon);
        }

        #endregion

        #region Factory Method Tests

        [Test]
        public void CreateFromMinMax()
        {
            var min = new Vector2(1, 2);
            var max = new Vector2(10, 20);

            var box = BoundingBox2D.CreateFromMinMax(min, max);

            Assert.AreEqual(min, box.Min);
            Assert.AreEqual(max, box.Max);
        }

        [Test]
        public void CreateFromCenterAndExtents()
        {
            var center = new Vector2(5, 10);
            var halfExtents = new Vector2(5, 10);

            var box = BoundingBox2D.CreateFromCenterAndExtents(center, halfExtents);

            Assert.AreEqual(new Vector2(0, 0), box.Min);
            Assert.AreEqual(new Vector2(10, 20), box.Max);
        }

        [Test]
        public void CreateFromPositionAndSize()
        {
            var position = new Vector2(1, 2);
            var size = new Vector2(9, 18);

            var box = BoundingBox2D.CreateFromPositionAndSize(position, size);

            Assert.AreEqual(new Vector2(1, 2), box.Min);
            Assert.AreEqual(new Vector2(10, 20), box.Max);
        }

        [Test]
        public void CreateFromPoints_SinglePoint()
        {
            var points = new[] { new Vector2(5, 5) };

            var box = BoundingBox2D.CreateFromPoints(points);

            Assert.AreEqual(new Vector2(5, 5), box.Min);
            Assert.AreEqual(new Vector2(5, 5), box.Max);
        }

        [Test]
        public void CreateFromPoints_MultiplePoints()
        {
            var points = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 5),
                new Vector2(5, 15),
                new Vector2(-5, 3)
            };

            var box = BoundingBox2D.CreateFromPoints(points);

            Assert.AreEqual(new Vector2(-5, 0), box.Min);
            Assert.AreEqual(new Vector2(10, 15), box.Max);
        }

        [Test]
        public void CreateFromPoints_ThrowsWhenNull()
        {
            Assert.Throws<ArgumentNullException>(() => BoundingBox2D.CreateFromPoints(null));
        }

        [Test]
        public void CreateFromPoints_ThrowsWhenEmpty()
        {
            Assert.Throws<ArgumentException>(() => BoundingBox2D.CreateFromPoints(new Vector2[0]));
        }

        [Test]
        public void CreateMerged_EnclosesBoth()
        {
            var box1 = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var box2 = new BoundingBox2D(new Vector2(5, 5), new Vector2(15, 15));

            var merged = BoundingBox2D.CreateMerged(box1, box2);

            Assert.AreEqual(new Vector2(0, 0), merged.Min);
            Assert.AreEqual(new Vector2(15, 15), merged.Max);
        }

        [Test]
        public void CreateMerged_OneBoxContainsOther()
        {
            var box1 = new BoundingBox2D(new Vector2(0, 0), new Vector2(20, 20));
            var box2 = new BoundingBox2D(new Vector2(5, 5), new Vector2(15, 15));

            var merged = BoundingBox2D.CreateMerged(box1, box2);

            Assert.AreEqual(box1.Min, merged.Min);
            Assert.AreEqual(box1.Max, merged.Max);
        }

        #endregion

        #region GetCorners Tests (Type-Specific Method)

        [Test]
        public void GetCorners_ReturnsArray()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));

            var corners = box.GetCorners();

            Assert.AreEqual(4, corners.Length);
            Assert.Contains(new Vector2(0, 0), corners);
            Assert.Contains(new Vector2(10, 0), corners);
            Assert.Contains(new Vector2(10, 20), corners);
            Assert.Contains(new Vector2(0, 20), corners);
        }

        [Test]
        public void GetCorners_FillsExistingArray()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));
            var corners = new Vector2[4];

            box.GetCorners(corners);

            Assert.Contains(new Vector2(0, 0), corners);
            Assert.Contains(new Vector2(10, 0), corners);
            Assert.Contains(new Vector2(10, 20), corners);
            Assert.Contains(new Vector2(0, 20), corners);
        }

        [Test]
        public void GetCorners_ThrowsWhenArrayNull()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));

            Assert.Throws<ArgumentNullException>(() => box.GetCorners(null));
        }

        [Test]
        public void GetCorners_ThrowsWhenArrayTooSmall()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));
            var corners = new Vector2[3];

            Assert.Throws<ArgumentException>(() => box.GetCorners(corners));
        }

        #endregion

        #region Transform Tests

        [Test]
        public void Transform_Translation()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var matrix = Matrix.CreateTranslation(5, 10, 0);

            var transformed = box.Transform(matrix);

            Assert.AreEqual(new Vector2(5, 10), transformed.Min);
            Assert.AreEqual(new Vector2(15, 20), transformed.Max);
        }

        [Test]
        public void Transform_Scale()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var matrix = Matrix.CreateScale(2.0f);

            var transformed = box.Transform(matrix);

            Assert.AreEqual(new Vector2(0, 0), transformed.Min);
            Assert.AreEqual(new Vector2(20, 20), transformed.Max);
        }

        [Test]
        public void Transform_Rotation()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 0));
            var matrix = Matrix.CreateRotationZ(MathHelper.PiOver2);

            var transformed = box.Transform(matrix);

            Assert.AreEqual(0, transformed.Min.X, Collision2D.Epsilon);
            Assert.AreEqual(0, transformed.Min.Y, Collision2D.Epsilon);
            Assert.AreEqual(0, transformed.Max.X, Collision2D.Epsilon);
            Assert.AreEqual(10, transformed.Max.Y, Collision2D.Epsilon);
        }

        [Test]
        public void Translate_OffsetsPosition()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var offset = new Vector2(5, 10);

            var translated = box.Translate(offset);

            Assert.AreEqual(new Vector2(5, 10), translated.Min);
            Assert.AreEqual(new Vector2(15, 20), translated.Max);
        }

        #endregion

        #region ContainsPoint Tests (Delegation Spot Check)

        [Test]
        public void ContainsPoint_Inside()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var point = new Vector2(5, 5);

            var result = box.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_OnBoundary()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var point = new Vector2(10, 5);

            var result = box.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_Outside()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var point = new Vector2(15, 5);

            var result = box.Contains(point);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        #endregion

        #region Deconstruct Test

        [Test]
        public void Deconstruct()
        {
            var box = new BoundingBox2D(new Vector2(1, 2), new Vector2(10, 20));

            var (min, max) = box;

            Assert.AreEqual(new Vector2(1, 2), min);
            Assert.AreEqual(new Vector2(10, 20), max);
        }

        #endregion
    }
}
