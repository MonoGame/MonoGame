// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class OrientedBoundingBox2DTest
    {
        #region Constructor Tests

        [Test]
        public void Constructor()
        {
            var center = new Vector2(5, 10);
            var axisX = new Vector2(1, 0);
            var axisY = new Vector2(0, 1);
            var halfExtents = new Vector2(3, 4);

            var obb = new OrientedBoundingBox2D(center, axisX, axisY, halfExtents);

            Assert.AreEqual(center, obb.Center);
            Assert.AreEqual(axisX, obb.AxisX);
            Assert.AreEqual(axisY, obb.AxisY);
            Assert.AreEqual(halfExtents, obb.HalfExtents);
        }

        #endregion

        #region Computed Property Tests

        [Test]
        public void Width_ReturnsTwiceHalfExtentX()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );

            float width = obb.Width;

            Assert.AreEqual(10.0f, width, Collision2D.Epsilon);
        }

        [Test]
        public void Height_ReturnsTwiceHalfExtentY()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );

            float height = obb.Height;

            Assert.AreEqual(6.0f, height, Collision2D.Epsilon);
        }

        [Test]
        public void Rotation_ReturnsZeroForAlignedBox()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );

            float rotation = obb.Rotation;

            Assert.AreEqual(0.0f, rotation, Collision2D.Epsilon);
        }

        [Test]
        public void Rotation_Returns90DegreesForRotatedBox()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0),
                new Vector2(5, 3)
            );

            float rotation = obb.Rotation;

            Assert.AreEqual(MathHelper.PiOver2, rotation, Collision2D.Epsilon);
        }

        [Test]
        public void Area_CalculatesCorrectly()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );

            float area = obb.Area;

            Assert.AreEqual(60.0f, area, Collision2D.Epsilon);
        }

        #endregion

        #region Factory Method Tests

        [Test]
        public void CreateFromRotation_ZeroRotation()
        {
            var center = new Vector2(5, 5);
            var halfExtents = new Vector2(3, 2);
            var rotation = 0.0f;

            var obb = OrientedBoundingBox2D.CreateFromRotation(center, rotation, halfExtents);

            Assert.AreEqual(center, obb.Center);
            Assert.AreEqual(1, obb.AxisX.X, Collision2D.Epsilon);
            Assert.AreEqual(0, obb.AxisX.Y, Collision2D.Epsilon);
            Assert.AreEqual(0, obb.AxisY.X, Collision2D.Epsilon);
            Assert.AreEqual(1, obb.AxisY.Y, Collision2D.Epsilon);
            Assert.AreEqual(halfExtents, obb.HalfExtents);
        }

        [Test]
        public void CreateFromRotation_90Degrees()
        {
            var center = new Vector2(5, 5);
            var halfExtents = new Vector2(3, 2);
            var rotation = MathHelper.PiOver2;

            var obb = OrientedBoundingBox2D.CreateFromRotation(center, rotation, halfExtents);

            Assert.AreEqual(center, obb.Center);
            Assert.AreEqual(0, obb.AxisX.X, Collision2D.Epsilon);
            Assert.AreEqual(1, obb.AxisX.Y, Collision2D.Epsilon);
            Assert.AreEqual(-1, obb.AxisY.X, Collision2D.Epsilon);
            Assert.AreEqual(0, obb.AxisY.Y, Collision2D.Epsilon);
            Assert.AreEqual(halfExtents, obb.HalfExtents);
        }

        [Test]
        public void CreateFromRotation_45Degrees()
        {
            var center = new Vector2(5, 5);
            var halfExtents = new Vector2(3, 2);
            var rotation = MathHelper.PiOver4;

            var obb = OrientedBoundingBox2D.CreateFromRotation(center, rotation, halfExtents);

            Assert.AreEqual(center, obb.Center);
            Assert.AreEqual(halfExtents, obb.HalfExtents);

            float cos45 = MathF.Cos(MathHelper.PiOver4);
            float sin45 = MathF.Sin(MathHelper.PiOver4);
            Assert.AreEqual(cos45, obb.AxisX.X, Collision2D.Epsilon);
            Assert.AreEqual(sin45, obb.AxisX.Y, Collision2D.Epsilon);
            Assert.AreEqual(-sin45, obb.AxisY.X, Collision2D.Epsilon);
            Assert.AreEqual(cos45, obb.AxisY.Y, Collision2D.Epsilon);
        }

        [Test]
        public void CreateFromBoundingBox2D()
        {
            var box = new BoundingBox2D(new Vector2(0, 0), new Vector2(10, 20));

            var obb = OrientedBoundingBox2D.CreateFromBoundingBox2D(box);

            Assert.AreEqual(new Vector2(5, 10), obb.Center);
            Assert.AreEqual(new Vector2(1, 0), obb.AxisX);
            Assert.AreEqual(new Vector2(0, 1), obb.AxisY);
            Assert.AreEqual(new Vector2(5, 10), obb.HalfExtents);
        }

        [Test]
        public void CreateMerged_NonOverlapping()
        {
            var obb1 = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(2, 2)
            );
            var obb2 = new OrientedBoundingBox2D(
                new Vector2(10, 10),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(2, 2)
            );

            var merged = OrientedBoundingBox2D.CreateMerged(obb1, obb2);

            Assert.AreEqual(ContainmentType.Contains, merged.Contains(obb1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(obb2));
        }

        [Test]
        public void CreateMerged_Overlapping()
        {
            var obb1 = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 5)
            );
            var obb2 = new OrientedBoundingBox2D(
                new Vector2(5, 5),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(3, 3)
            );

            var merged = OrientedBoundingBox2D.CreateMerged(obb1, obb2);

            Assert.AreEqual(ContainmentType.Contains, merged.Contains(obb1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(obb2));
        }

        [Test]
        public void CreateMerged_DifferentRotations()
        {
            var obb1 = OrientedBoundingBox2D.CreateFromRotation(new Vector2(0, 0), 0, new Vector2(3, 2));
            var obb2 = OrientedBoundingBox2D.CreateFromRotation(new Vector2(5, 5), MathHelper.PiOver4, new Vector2(3, 2));

            var merged = OrientedBoundingBox2D.CreateMerged(obb1, obb2);

            Assert.AreEqual(ContainmentType.Contains, merged.Contains(obb1));
            Assert.AreEqual(ContainmentType.Contains, merged.Contains(obb2));
        }

        #endregion

        #region GetCorners Tests (Type-Specific Method)

        [Test]
        public void GetCorners_ReturnsArrayOfFour()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );

            var corners = obb.GetCorners();

            Assert.AreEqual(4, corners.Length);
        }

        [Test]
        public void GetCorners_AlignedBox()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );

            var corners = obb.GetCorners();

            Assert.Contains(new Vector2(-5, -3), corners);
            Assert.Contains(new Vector2(5, -3), corners);
            Assert.Contains(new Vector2(5, 3), corners);
            Assert.Contains(new Vector2(-5, 3), corners);
        }

        [Test]
        public void GetCorners_FillArray()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );
            var corners = new Vector2[4];

            obb.GetCorners(corners);

            Assert.Contains(new Vector2(-5, -3), corners);
            Assert.Contains(new Vector2(5, -3), corners);
            Assert.Contains(new Vector2(5, 3), corners);
            Assert.Contains(new Vector2(-5, 3), corners);
        }

        [Test]
        public void GetCorners_ThrowsWhenArrayNull()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );

            Assert.Throws<ArgumentNullException>(() => obb.GetCorners(null));
        }

        [Test]
        public void GetCorners_ThrowsWhenArrayTooSmall()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );
            var corners = new Vector2[3];

            Assert.Throws<ArgumentException>(() => obb.GetCorners(corners));
        }

        #endregion

        #region Transform Tests

        [Test]
        public void Transform_Translation()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );
            var matrix = Matrix.CreateTranslation(10, 20, 0);

            var transformed = obb.Transform(matrix);

            Assert.AreEqual(new Vector2(10, 20), transformed.Center);
            Assert.AreEqual(obb.AxisX, transformed.AxisX);
            Assert.AreEqual(obb.AxisY, transformed.AxisY);
            Assert.AreEqual(obb.HalfExtents, transformed.HalfExtents);
        }

        [Test]
        public void Transform_UniformScale()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );
            var matrix = Matrix.CreateScale(2.0f);

            var transformed = obb.Transform(matrix);

            Assert.AreEqual(new Vector2(0, 0), transformed.Center);
            Assert.AreEqual(new Vector2(10, 6), transformed.HalfExtents);
        }

        [Test]
        public void Transform_NonUniformScale()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );
            var matrix = Matrix.CreateScale(2.0f, 3.0f, 1.0f);

            var transformed = obb.Transform(matrix);

            Assert.AreEqual(0, transformed.Center.X, Collision2D.Epsilon);
            Assert.AreEqual(0, transformed.Center.Y, Collision2D.Epsilon);
            Assert.AreEqual(10, transformed.HalfExtents.X, Collision2D.Epsilon);
            Assert.AreEqual(9, transformed.HalfExtents.Y, Collision2D.Epsilon);
        }

        [Test]
        public void Transform_Rotation()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(5, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(3, 2)
            );
            var matrix = Matrix.CreateRotationZ(MathHelper.PiOver2);

            var transformed = obb.Transform(matrix);

            Assert.AreEqual(0, transformed.Center.X, Collision2D.Epsilon);
            Assert.AreEqual(5, transformed.Center.Y, Collision2D.Epsilon);

            Assert.AreEqual(0, transformed.AxisX.X, Collision2D.Epsilon);
            Assert.AreEqual(1, transformed.AxisX.Y, Collision2D.Epsilon);
            Assert.AreEqual(-1, transformed.AxisY.X, Collision2D.Epsilon);
            Assert.AreEqual(0, transformed.AxisY.Y, Collision2D.Epsilon);

            Assert.AreEqual(new Vector2(3, 2), transformed.HalfExtents);
        }

        [Test]
        public void Translate_OffsetsPosition()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(5, 5),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(3, 2)
            );
            var offset = new Vector2(10, 15);

            var translated = obb.Translate(offset);

            Assert.AreEqual(new Vector2(15, 20), translated.Center);
            Assert.AreEqual(obb.AxisX, translated.AxisX);
            Assert.AreEqual(obb.AxisY, translated.AxisY);
            Assert.AreEqual(obb.HalfExtents, translated.HalfExtents);
        }

        #endregion

        #region ContainsPoint Tests (Delegation Spot Check)

        [Test]
        public void ContainsPoint_Inside()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );
            var point = new Vector2(2, 1);

            var result = obb.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_OnBoundary()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );
            var point = new Vector2(5, 0);

            var result = obb.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        [Test]
        public void ContainsPoint_Outside()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(5, 3)
            );
            var point = new Vector2(10, 0);

            var result = obb.Contains(point);

            Assert.AreEqual(ContainmentType.Disjoint, result);
        }

        [Test]
        public void ContainsPoint_RotatedBox()
        {
            var obb = OrientedBoundingBox2D.CreateFromRotation(
                new Vector2(0, 0),
                MathHelper.PiOver4,
                new Vector2(5, 3)
            );
            var point = new Vector2(0, 0);

            var result = obb.Contains(point);

            Assert.AreEqual(ContainmentType.Contains, result);
        }

        #endregion

        #region Deconstruct Test

        [Test]
        public void Deconstruct()
        {
            var obb = new OrientedBoundingBox2D(
                new Vector2(5, 10),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(3, 4)
            );

            var (center, axisX, axisY, halfExtents) = obb;

            Assert.AreEqual(new Vector2(5, 10), center);
            Assert.AreEqual(new Vector2(1, 0), axisX);
            Assert.AreEqual(new Vector2(0, 1), axisY);
            Assert.AreEqual(new Vector2(3, 4), halfExtents);
        }

        #endregion
    }
}
