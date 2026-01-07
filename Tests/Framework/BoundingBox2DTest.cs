// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using NUnit.Framework;
using System;

namespace MonoGame.Tests.Framework
{
    class BoundingBox2DTest
    {
        [Test]
        public void Constructor()
        {
            var min = new Vector2(10, 20);
            var max = new Vector2(30, 40);

            var box = new BoundingBox2D(min, max);

            Assert.AreEqual(min, box.Min);
            Assert.AreEqual(max, box.Max);
        }

        [Test]
        public void Center_ReturnsMiddlePoint()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(20, 10));

            var center = box.Center;

            Assert.AreEqual(new Vector2(10, 5), center);
        }

        [Test]
        public void Size_ReturnsWidthAndHeight()
        {
            var box = new BoundingBox2D(new Vector2(10, 20), new Vector2(30, 50));

            var size = box.Size;

            Assert.AreEqual(new Vector2(20, 30), size);
        }

        [Test]
        public void HalfExtents_ReturnsHalfSize()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(20, 10));

            var halfExtents = box.HalfExtents;

            Assert.AreEqual(new Vector2(10, 5), halfExtents);
        }

        [Test]
        public void Width_ReturnsHorizontalExtent()
        {
            var box = new BoundingBox2D(new Vector2(10, 20), new Vector2(30, 50));

            float width = box.Width;

            Assert.AreEqual(20.0f, width);
        }

        [Test]
        public void Height_ReturnsVerticalExtent()
        {
            var box = new BoundingBox2D(new Vector2(10, 20), new Vector2(30, 50));

            float height = box.Height;

            Assert.AreEqual(30.0f, height);
        }

        [Test]
        public void Area_ReturnsWidthTimesHeight()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 5));

            float area = box.Area;

            Assert.AreEqual(50.0f, area);
        }

        [Test]
        public void CreateFromMinMax()
        {
            var min = new Vector2(5, 10);
            var max = new Vector2(15, 20);

            var box = BoundingBox2D.CreateFromMinMax(min, max);

            Assert.AreEqual(min, box.Min);
            Assert.AreEqual(max, box.Max);
        }

        [Test]
        public void CreateFromCenterAndExtents()
        {
            var center = new Vector2(10, 10);
            var halfExtents = new Vector2(5, 5);

            var box = BoundingBox2D.CreateFromCenterAndExtents(center, halfExtents);

            Assert.AreEqual(new Vector2(5, 5), box.Min);
            Assert.AreEqual(new Vector2(15, 15), box.Max);
            Assert.AreEqual(center, box.Center);
        }

        [Test]
        public void CreateFromPositionAndSize()
        {
            var position = new Vector2(10, 20);
            var size = new Vector2(30, 40);

            var box = BoundingBox2D.CreateFromPositionAndSize(position, size);

            Assert.AreEqual(position, box.Min);
            Assert.AreEqual(new Vector2(40, 60), box.Max);
        }

        [Test]
        public void CreateFromPoints_SinglePoint()
        {
            var points = new[] { new Vector2(5, 10) };

            var box = BoundingBox2D.CreateFromPoints(points);

            Assert.AreEqual(new Vector2(5, 10), box.Min);
            Assert.AreEqual(new Vector2(5, 10), box.Max);
        }

        [Test]
        public void CreateFromPoints_MultiplePoints()
        {
            var points = new[]
            {
                new Vector2(0, 0),
                new Vector2(10, 5),
                new Vector2(-5, 15),
                new Vector2(20, -10)
            };

            var box = BoundingBox2D.CreateFromPoints(points);

            Assert.AreEqual(new Vector2(-5, -10), box.Min);
            Assert.AreEqual(new Vector2(20, 15), box.Max);
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
        public void CreateMerged_EnclosesBoths()
        {
            var box1 = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var box2 = new BoundingBox2D(new Vector2(5, 5), new Vector2(15, 20));

            var merged = BoundingBox2D.CreateMerged(box1, box2);

            Assert.AreEqual(new Vector2(0, 0), merged.Min);
            Assert.AreEqual(new Vector2(15, 20), merged.Max);
        }

        [Test]
        public void CreateMerged_OneBoxContainsOther()
        {
            var box1 = new BoundingBox2D(new Vector2(0, 0), new Vector2(20, 20));
            var box2 = new BoundingBox2D(new Vector2(5, 5), new Vector2(10, 10));

            var merged = BoundingBox2D.CreateMerged(box1, box2);

            Assert.AreEqual(box1.Min, merged.Min);
            Assert.AreEqual(box1.Max, merged.Max);
        }

        [Test]
        public void GetCorners_ReturnsArray()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));

            var corners = box.GetCorners();

            Assert.AreEqual(4, corners.Length);
            Assert.AreEqual(new Vector2(0, 0), corners[0]);
            Assert.AreEqual(new Vector2(10, 0), corners[1]);
            Assert.AreEqual(new Vector2(10, 20), corners[2]);
            Assert.AreEqual(new Vector2(0, 20), corners[3]);
        }

        [Test]
        public void GetCorners_FillsExistingArray()
        {
            var box = new BoundingBox2D(new Vector2(5, 10), new Vector2(15, 30));
            var corners = new Vector2[4];

            box.GetCorners(corners);

            Assert.AreEqual(new Vector2(5, 10), corners[0]);
            Assert.AreEqual(new Vector2(15, 10), corners[1]);
            Assert.AreEqual(new Vector2(15, 30), corners[2]);
            Assert.AreEqual(new Vector2(5, 30), corners[3]);
        }

        [Test]
        public void GetCorners_ThrowsWhenArrayNull()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));

            Assert.Throws<ArgumentNullException>(() => box.GetCorners(null));
        }

        [Test]
        public void GetCorners_ThrowsWhenArrayTooSmall()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var corners = new Vector2[3];

            Assert.Throws<ArgumentException>(() => box.GetCorners(corners));
        }

        [Test]
        public void Transform_Translation()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var matrix = Matrix.CreateTranslation(5, 10, 0);

            var transformed = box.Transform(matrix);

            Assert.AreEqual(new Vector2(10, 15), transformed.Center);
            Assert.AreEqual(new Vector2(5, 5), transformed.HalfExtents);
        }

        [Test]
        public void Transform_Scale()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var matrix = Matrix.CreateScale(2, 3, 1);

            var transformed = box.Transform(matrix);

            Assert.AreEqual(new Vector2(10, 15), transformed.Center);
            Assert.AreEqual(new Vector2(10, 15), transformed.HalfExtents);
        }

        [Test]
        public void Transform_Rotation()
        {
            var box = new BoundingBox2D(new Vector2(-5, -5), new Vector2(5, 5));
            var matrix = Matrix.CreateRotationZ(MathHelper.PiOver4);

            var transformed = box.Transform(matrix);

            // Rotated box should be larger to contain all corners
            Assert.AreEqual(Vector2.Zero, transformed.Center);
            float expectedHalfExtent = 5.0f * MathF.Sqrt(2);
            Assert.AreEqual(expectedHalfExtent, transformed.HalfExtents.X, 1e-5f);
            Assert.AreEqual(expectedHalfExtent, transformed.HalfExtents.Y, 1e-5f);
        }

        [Test]
        public void Translate_OffsetsPosition()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var translation = new Vector2(5, -3);

            var translated = box.Translate(translation);

            Assert.AreEqual(new Vector2(5, -3), translated.Min);
            Assert.AreEqual(new Vector2(15, 7), translated.Max);
            Assert.AreEqual(box.Size, translated.Size);
        }

        [Test]
        public void ContainsBox_Disjoint()
        {
            var box1 = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var box2 = new BoundingBox2D(new Vector2(20, 20), new Vector2(30, 30));

            var result = box1.Contains(box2);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsBox_Intersects()
        {
            var box1 = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var box2 = new BoundingBox2D(new Vector2(5, 5), new Vector2(15, 15));

            var result = box1.Contains(box2);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        [Test]
        public void ContainsBox_Contains()
        {
            var box1 = new BoundingBox2D(new Vector2(0, 0), new Vector2(20, 20));
            var box2 = new BoundingBox2D(new Vector2(5, 5), new Vector2(15, 15));

            var result = box1.Contains(box2);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsBox_TouchingEdge()
        {
            var box1 = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var box2 = new BoundingBox2D(new Vector2(10, 0), new Vector2(20, 10));

            var result = box1.Contains(box2);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

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

        [Test]
        public void Intersects_Overlapping()
        {
            var box1 = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var box2 = new BoundingBox2D(new Vector2(5, 5), new Vector2(15, 15));

            bool intersects = box1.Intersects(box2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void Intersects_Separated()
        {
            var box1 = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var box2 = new BoundingBox2D(new Vector2(20, 20), new Vector2(30, 30));

            bool intersects = box1.Intersects(box2);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void Intersects_TouchingEdge()
        {
            var box1 = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 10));
            var box2 = new BoundingBox2D(new Vector2(10, 0), new Vector2(20, 10));

            bool intersects = box1.Intersects(box2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void Intersects_OneContainsOther()
        {
            var box1 = new BoundingBox2D(new Vector2(0, 0), new Vector2(20, 20));
            var box2 = new BoundingBox2D(new Vector2(5, 5), new Vector2(15, 15));

            bool intersects = box1.Intersects(box2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void Deconstruct()
        {
            var box = new BoundingBox2D(new Vector2(1, 2), new Vector2(3, 4));

            box.Deconstruct(out Vector2 min, out Vector2 max);

            Assert.AreEqual(box.Min, min);
            Assert.AreEqual(box.Max, max);
        }
    }
}
