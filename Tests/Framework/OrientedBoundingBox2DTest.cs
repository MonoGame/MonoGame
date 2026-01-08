// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using NUnit.Framework;
using System;

namespace MonoGame.Tests.Framework
{
    class OrientedBoundingBox2DTest
    {
        [Test]
        public void Constructor()
        {
            var center = new Vector2(10, 20);
            var axisX = new Vector2(1, 0);
            var axisY = new Vector2(0, 1);
            var halfExtents = new Vector2(5, 3);

            var obb = new OrientedBoundingBox2D(center, axisX, axisY, halfExtents);

            Assert.AreEqual(center, obb.Center);
            Assert.AreEqual(axisX, obb.AxisX);
            Assert.AreEqual(axisY, obb.AxisY);
            Assert.AreEqual(halfExtents, obb.HalfExtents);
        }

        [Test]
        public void Width_ReturnsTwiceHalfExtentX()
        {
            var obb = new OrientedBoundingBox2D(Vector2.Zero, Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));

            float width = obb.Width;

            Assert.AreEqual(10.0f, width);
        }

        [Test]
        public void Height_ReturnsTwiceHalfExtentY()
        {
            var obb = new OrientedBoundingBox2D(Vector2.Zero, Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));

            float height = obb.Height;

            Assert.AreEqual(6.0f, height);
        }

        [Test]
        public void Rotation_ReturnsZeroForAlignedBox()
        {
            var obb = new OrientedBoundingBox2D(Vector2.Zero, Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));

            float rotation = obb.Rotation;

            Assert.AreEqual(0.0f, rotation, 1e-5f);
        }

        [Test]
        public void Rotation_Returns90DegreesForRotatedBox()
        {
            var obb = new OrientedBoundingBox2D(Vector2.Zero, Vector2.UnitY, new Vector2(-1, 0), new Vector2(5, 3));

            float rotation = obb.Rotation;

            Assert.AreEqual(MathHelper.PiOver2, rotation, 1e-5f);
        }

        [Test]
        public void Area_CalculatesCorrectly()
        {
            var obb = new OrientedBoundingBox2D(Vector2.Zero, Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));

            float area = obb.Area;

            Assert.AreEqual(60.0f, area);
        }

        [Test]
        public void CreateFromRotation_ZeroRotation()
        {
            var center = new Vector2(10, 20);
            var halfExtents = new Vector2(5, 3);

            var obb = OrientedBoundingBox2D.CreateFromRotation(center, 0.0f, halfExtents);

            Assert.AreEqual(center, obb.Center);
            Assert.AreEqual(new Vector2(1, 0), obb.AxisX);
            Assert.AreEqual(new Vector2(0, 1), obb.AxisY);
            Assert.AreEqual(halfExtents, obb.HalfExtents);
        }

        [Test]
        public void CreateFromRotation_90Degrees()
        {
            var center = new Vector2(10, 20);
            var halfExtents = new Vector2(5, 3);

            var obb = OrientedBoundingBox2D.CreateFromRotation(center, MathHelper.PiOver2, halfExtents);

            Assert.AreEqual(center, obb.Center);
            Assert.AreEqual(0.0f, obb.AxisX.X, 1e-5f);
            Assert.AreEqual(1.0f, obb.AxisX.Y, 1e-5f);
            Assert.AreEqual(-1.0f, obb.AxisY.X, 1e-5f);
            Assert.AreEqual(0.0f, obb.AxisY.Y, 1e-5f);
            Assert.AreEqual(halfExtents, obb.HalfExtents);
        }

        [Test]
        public void CreateFromRotation_45Degrees()
        {
            var center = new Vector2(10, 20);
            var halfExtents = new Vector2(5, 3);
            float rotation = MathHelper.PiOver4;

            var obb = OrientedBoundingBox2D.CreateFromRotation(center, rotation, halfExtents);

            Assert.AreEqual(center, obb.Center);
            float expectedCos = MathF.Cos(rotation);
            float expectedSin = MathF.Sin(rotation);
            Assert.AreEqual(expectedCos, obb.AxisX.X, 1e-5f);
            Assert.AreEqual(expectedSin, obb.AxisX.Y, 1e-5f);
        }

        [Test]
        public void CreateFromBoundingBox2D()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));

            var obb = OrientedBoundingBox2D.CreateFromBoundingBox2D(box);

            Assert.AreEqual(box.Center, obb.Center);
            Assert.AreEqual(Vector2.UnitX, obb.AxisX);
            Assert.AreEqual(Vector2.UnitY, obb.AxisY);
            Assert.AreEqual(box.HalfExtents, obb.HalfExtents);
        }

        [Test]
        public void CreateMerged_NonOverlapping()
        {
            var obb1 = new OrientedBoundingBox2D(new Vector2(0, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(2, 2));
            var obb2 = new OrientedBoundingBox2D(new Vector2(10, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(2, 2));

            var merged = OrientedBoundingBox2D.CreateMerged(obb1, obb2);

            Assert.AreEqual(ContainmentType.Contains, merged.Contains(obb1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(obb2));
        }

        [Test]
        public void CreateMerged_Overlapping()
        {
            var obb1 = new OrientedBoundingBox2D(new Vector2(0, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var obb2 = new OrientedBoundingBox2D(new Vector2(5, 5), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));

            var merged = OrientedBoundingBox2D.CreateMerged(obb1, obb2);

            Assert.AreEqual(ContainmentType.Contains, merged.Contains(obb1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(obb2));
        }

        [Test]
        public void CreateMerged_DifferentRotations()
        {
            var obb1 = OrientedBoundingBox2D.CreateFromRotation(new Vector2(0, 0), 0, new Vector2(5, 2));
            var obb2 = OrientedBoundingBox2D.CreateFromRotation(new Vector2(10, 0), MathHelper.PiOver4, new Vector2(5, 2));

            var merged = OrientedBoundingBox2D.CreateMerged(obb1, obb2);

            Assert.AreEqual(ContainmentType.Contains, merged.Contains(obb1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(obb2));
        }

        [Test]
        public void GetCorners_ReturnsArrayOfFour()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));

            var corners = obb.GetCorners();

            Assert.AreEqual(4, corners.Length);
        }

        [Test]
        public void GetCorners_AlignedBox()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));

            var corners = obb.GetCorners();

            Assert.AreEqual(new Vector2(5, 7), corners[0]);   // Top-left
            Assert.AreEqual(new Vector2(15, 7), corners[1]);  // Top-right
            Assert.AreEqual(new Vector2(15, 13), corners[2]); // Bottom-right
            Assert.AreEqual(new Vector2(5, 13), corners[3]);  // Bottom-left
        }

        [Test]
        public void GetCorners_FillArray()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));
            var corners = new Vector2[4];

            obb.GetCorners(corners);

            Assert.AreEqual(new Vector2(5, 7), corners[0]);
            Assert.AreEqual(new Vector2(15, 7), corners[1]);
            Assert.AreEqual(new Vector2(15, 13), corners[2]);
            Assert.AreEqual(new Vector2(5, 13), corners[3]);
        }

        [Test]
        public void GetCorners_ThrowsWhenArrayNull()
        {
            var obb = new OrientedBoundingBox2D(Vector2.Zero, Vector2.UnitX, Vector2.UnitY, Vector2.One);

            Assert.Throws<ArgumentNullException>(() => obb.GetCorners(null));
        }

        [Test]
        public void GetCorners_ThrowsWhenArrayTooSmall()
        {
            var obb = new OrientedBoundingBox2D(Vector2.Zero, Vector2.UnitX, Vector2.UnitY, Vector2.One);
            var corners = new Vector2[3];

            Assert.Throws<ArgumentException>(() => obb.GetCorners(corners));
        }

        [Test]
        public void ContainsOBB_Contains()
        {
            var obb1 = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(10, 10));
            var obb2 = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(3, 3));

            var result = obb1.Contains(obb2);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsOBB_Intersects()
        {
            var obb1 = new OrientedBoundingBox2D(new Vector2(0, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var obb2 = new OrientedBoundingBox2D(new Vector2(7, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));

            var result = obb1.Contains(obb2);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        [Test]
        public void ContainsOBB_Disjoint()
        {
            var obb1 = new OrientedBoundingBox2D(new Vector2(0, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var obb2 = new OrientedBoundingBox2D(new Vector2(20, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));

            var result = obb1.Contains(obb2);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsCircle_Contains()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(10, 10));
            var circle = new BoundingCircle(new Vector2(10, 10), 3);

            var result = obb.Contains(circle);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsCircle_Intersects()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var circle = new BoundingCircle(new Vector2(8, 10), 5);

            var result = obb.Contains(circle);

            Assert.AreEqual(ContainmentType.Intersects, result);
        }

        [Test]
        public void ContainsCircle_Disjoint()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var circle = new BoundingCircle(new Vector2(30, 10), 5);

            var result = obb.Contains(circle);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsPoint_Inside()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));
            var point = new Vector2(10, 10);

            var result = obb.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_OnBoundary()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));
            var point = new Vector2(15, 10);

            var result = obb.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_Outside()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));
            var point = new Vector2(20, 20);

            var result = obb.Contains(point);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsPoint_RotatedBox()
        {
            var obb = OrientedBoundingBox2D.CreateFromRotation(new Vector2(10, 10), MathHelper.PiOver4, new Vector2(5, 3));
            var point = new Vector2(10, 10);

            var result = obb.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void IntersectsOBB_Overlapping()
        {
            var obb1 = new OrientedBoundingBox2D(new Vector2(0, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var obb2 = new OrientedBoundingBox2D(new Vector2(7, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));

            bool intersects = obb1.Intersects(obb2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsOBB_Separated()
        {
            var obb1 = new OrientedBoundingBox2D(new Vector2(0, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var obb2 = new OrientedBoundingBox2D(new Vector2(20, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));

            bool intersects = obb1.Intersects(obb2);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsOBB_Touching()
        {
            var obb1 = new OrientedBoundingBox2D(new Vector2(0, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var obb2 = new OrientedBoundingBox2D(new Vector2(10, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));

            bool intersects = obb1.Intersects(obb2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsOBB_DifferentRotations()
        {
            var obb1 = OrientedBoundingBox2D.CreateFromRotation(new Vector2(0, 0), 0, new Vector2(5, 2));
            var obb2 = OrientedBoundingBox2D.CreateFromRotation(new Vector2(5, 0), MathHelper.PiOver4, new Vector2(5, 2));

            bool intersects = obb1.Intersects(obb2);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsCircle_Overlapping()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var circle = new BoundingCircle(new Vector2(8, 10), 5);

            bool intersects = obb.Intersects(circle);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsCircle_Separated()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var circle = new BoundingCircle(new Vector2(30, 10), 5);

            bool intersects = obb.Intersects(circle);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsBox_Overlapping()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var box = new BoundingBox2D(new Vector2(8, 8), new Vector2(12, 12));

            bool intersects = obb.Intersects(box);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsBox_Separated()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var box = new BoundingBox2D(new Vector2(30, 30), new Vector2(40, 40));

            bool intersects = obb.Intersects(box);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsCapsule_Overlapping()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var capsule = new BoundingCapsule2D(new Vector2(8, 8), new Vector2(12, 12), 2);

            bool intersects = obb.Intersects(capsule);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void IntersectsCapsule_Separated()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var capsule = new BoundingCapsule2D(new Vector2(30, 30), new Vector2(40, 40), 2);

            bool intersects = obb.Intersects(capsule);

            Assert.IsFalse(intersects);
        }

        [Test]
        public void IntersectsCapsule_CapsuleThroughBox()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 5));
            var capsule = new BoundingCapsule2D(new Vector2(10, 0), new Vector2(10, 20), 2);

            bool intersects = obb.Intersects(capsule);

            Assert.IsTrue(intersects);
        }

        [Test]
        public void Transform_Translation()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));
            var matrix = Matrix.CreateTranslation(5, 10, 0);

            var transformed = obb.Transform(matrix);

            Assert.AreEqual(new Vector2(15, 20), transformed.Center);
            Assert.AreEqual(Vector2.UnitX, transformed.AxisX);
            Assert.AreEqual(Vector2.UnitY, transformed.AxisY);
            Assert.AreEqual(new Vector2(5, 3), transformed.HalfExtents);
        }

        [Test]
        public void Transform_UniformScale()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));
            var matrix = Matrix.CreateScale(2, 2, 1);

            var transformed = obb.Transform(matrix);

            Assert.AreEqual(new Vector2(20, 20), transformed.Center);
            Assert.AreEqual(new Vector2(10, 6), transformed.HalfExtents);
        }

        [Test]
        public void Transform_NonUniformScale()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));
            var matrix = Matrix.CreateScale(2, 3, 1);

            var transformed = obb.Transform(matrix);

            Assert.AreEqual(new Vector2(20, 30), transformed.Center);
            Assert.AreEqual(new Vector2(10, 9), transformed.HalfExtents);
        }

        [Test]
        public void Transform_Rotation()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 0), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));
            var matrix = Matrix.CreateRotationZ(MathHelper.PiOver2);

            var transformed = obb.Transform(matrix);

            Assert.AreEqual(0.0f, transformed.Center.X, 1e-5f);
            Assert.AreEqual(10.0f, transformed.Center.Y, 1e-5f);
            Assert.AreEqual(0.0f, transformed.AxisX.X, 1e-5f);
            Assert.AreEqual(1.0f, transformed.AxisX.Y, 1e-5f);
        }

        [Test]
        public void Translate_OffsetsPosition()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));
            var translation = new Vector2(5, -3);

            var translated = obb.Translate(translation);

            Assert.AreEqual(new Vector2(15, 7), translated.Center);
            Assert.AreEqual(Vector2.UnitX, translated.AxisX);
            Assert.AreEqual(Vector2.UnitY, translated.AxisY);
            Assert.AreEqual(new Vector2(5, 3), translated.HalfExtents);
        }

        [Test]
        public void Deconstruct()
        {
            var obb = new OrientedBoundingBox2D(new Vector2(10, 10), Vector2.UnitX, Vector2.UnitY, new Vector2(5, 3));

            obb.Deconstruct(out Vector2 center, out Vector2 axisX, out Vector2 axisY, out Vector2 halfExtents);

            Assert.AreEqual(obb.Center, center);
            Assert.AreEqual(obb.AxisX, axisX);
            Assert.AreEqual(obb.AxisY, axisY);
            Assert.AreEqual(obb.HalfExtents, halfExtents);
        }
    }
}
