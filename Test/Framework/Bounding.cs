// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    class Bounding
    {
        [Test]
        public void BoundingSphereTests()
        {
            var zeroPoint = BoundingSphere.CreateFromPoints( new[] {Vector3.Zero} );
            Assert.AreEqual(new BoundingSphere(), zeroPoint);

            var onePoint = BoundingSphere.CreateFromPoints(new[] { Vector3.One });
            Assert.AreEqual(new BoundingSphere(Vector3.One, 0), onePoint);

            var twoPoint = BoundingSphere.CreateFromPoints(new[] { Vector3.Zero, Vector3.One });
            Assert.AreEqual(new BoundingSphere(new Vector3(0.5f, 0.5f, 0.5f), 0.8660254f), twoPoint);

            var threePoint = BoundingSphere.CreateFromPoints(new[] { new Vector3(0, 0, 0), new Vector3(-1, 0, 0), new Vector3(1, 1, 1) });
            Assert.That(new BoundingSphere(new Vector3(0, 0.5f, 0.5f), 1.224745f), Is.EqualTo(threePoint).Using(BoundingSphereComparer.Epsilon));

            var eightPointTestInput = new Vector3[]
            {
                new Vector3(54.58071f, 124.9063f, 56.0016f),
                new Vector3(54.52138f, 124.9063f, 56.13985f),
                new Vector3(54.52208f, 124.8235f, 56.14014f),
                new Vector3(54.5814f, 124.8235f, 56.0019f),
                new Vector3(1145.415f, 505.913f, -212.5173f),
                new Vector3(611.4731f, 505.9535f, 1031.893f),
                new Vector3(617.7462f, -239.7422f, 1034.584f),
                new Vector3(1151.687f, -239.7035f, -209.8246f)
            };
            var eightPoint = BoundingSphere.CreateFromPoints(eightPointTestInput);
            for (int i = 0; i < eightPointTestInput.Length; i++)
            {
                Assert.That(eightPoint.Contains(eightPointTestInput[i]) != ContainmentType.Disjoint);
            }

            Assert.Throws<ArgumentException>(() => BoundingSphere.CreateFromPoints(new Vector3[] {}));
        }

        [Test]
        public void BoundingBoxContainsBoundingSphere()
        {
            var testSphere = new BoundingSphere(Vector3.Zero, 1);
            var testBox = new BoundingBox(-Vector3.One, Vector3.One);

            Assert.AreEqual(testBox.Contains(testSphere), ContainmentType.Contains);

            testSphere.Center -= Vector3.One;

            Assert.AreEqual(testBox.Contains(testSphere), ContainmentType.Intersects);

            testSphere.Center -= Vector3.One;

            Assert.AreEqual(testBox.Contains(testSphere), ContainmentType.Disjoint);
        }

        [Test]
        public void BoundingFrustumToBoundingBoxTests()
        {
            var view = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 100);
            var testFrustum = new BoundingFrustum(view * projection);

            var bbox1 = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            Assert.That(testFrustum.Contains(bbox1), Is.EqualTo(ContainmentType.Contains));
            Assert.That(testFrustum.Intersects(bbox1), Is.True);

            var bbox2 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(1000, 1000, 1000));
            Assert.That(testFrustum.Contains(bbox2), Is.EqualTo(ContainmentType.Intersects));
            Assert.That(testFrustum.Intersects(bbox2), Is.True);

            var bbox3 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(0, 0, 0));
            Assert.That(testFrustum.Contains(bbox3), Is.EqualTo(ContainmentType.Intersects));
            Assert.That(testFrustum.Intersects(bbox3), Is.True);

            var bbox4 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(-500, -500, -500));
            Assert.That(testFrustum.Contains(bbox4), Is.EqualTo(ContainmentType.Disjoint));
            Assert.That(testFrustum.Intersects(bbox4), Is.False);
        }

        [Test]
        public void BoundingFrustumToBoundingFrustumTests()
        {
            var view = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 100);
            var testFrustum = new BoundingFrustum(view * projection);

            // Same frustum.
            Assert.That(testFrustum.Contains(testFrustum), Is.EqualTo(ContainmentType.Contains));
            Assert.That(testFrustum.Intersects(testFrustum), Is.True);

            var otherFrustum = new BoundingFrustum(Matrix.Identity);

            // Smaller frustum contained entirely inside.
            var view2 = Matrix.CreateLookAt(new Vector3(0, 0, 4), Vector3.Zero, Vector3.Up);
            var projection2 = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 50);
            otherFrustum.Matrix = view2 * projection2;

            Assert.That(testFrustum.Contains(otherFrustum), Is.EqualTo(ContainmentType.Contains));
            Assert.That(testFrustum.Intersects(otherFrustum), Is.True);

            // Same size frustum, pointing in the same direction and offset by a small amount.
            otherFrustum.Matrix = view2 * projection;

            Assert.That(testFrustum.Contains(otherFrustum), Is.EqualTo(ContainmentType.Intersects));
            Assert.That(testFrustum.Intersects(otherFrustum), Is.True);

            // Same size frustum, pointing in the opposite direction and not overlapping.
            var view3 = Matrix.CreateLookAt(new Vector3(0, 0, 6), new Vector3(0, 0, 7), Vector3.Up);
            otherFrustum.Matrix = view3 * projection;

            Assert.That(testFrustum.Contains(otherFrustum), Is.EqualTo(ContainmentType.Disjoint));
            Assert.That(testFrustum.Intersects(otherFrustum), Is.False);

            // Larger frustum, entirely containing test frustum.
            var view4 = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up);
            var projection4 = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 1000);
            otherFrustum.Matrix = view4 * projection4;

            Assert.That(testFrustum.Contains(otherFrustum), Is.EqualTo(ContainmentType.Intersects));
            Assert.That(testFrustum.Intersects(otherFrustum), Is.True);

            var bf =
                new BoundingFrustum(Matrix.CreateLookAt(new Vector3(0, 1, 1), new Vector3(0, 0, 0), Vector3.Up) *
                                    Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                        1.3f, 0.1f, 1000.0f));
            var ray = new Ray(new Vector3(0, 0.5f, 0.5f), new Vector3(0, 0, 0));
            var ray2 = new Ray(new Vector3(0, 1.0f, 1.0f), new Vector3(0, 0, 0));
            var value = bf.Intersects(ray);
            var value2 = bf.Intersects(ray2);
            Assert.AreEqual(0.0f, value);
            Assert.AreEqual(null, value2);
        }

        [TestCase(0, 10, 80, 90, 0)]
        [TestCase(0, 50, 40, 90, 10)]
        [TestCase(0, 50, 40, 45, 5)]
        [TestCase(0, 100, 0, 20, 20)]
        [TestCase(80, 90, 0, 10, 0)]
        [TestCase(40, 90, 0, 50, 10)]
        [TestCase(40, 45, 0, 50, 5)]
        [TestCase(0, 20, 0, 100, 20)]
        public void LineOverlapCalculatedCorrectly1(float min1, float max1, float min2, float max2, float overlap)
        {
            var line1 = new Line1D(min1, max1);
            var line2 = new Line1D(min2, max2);
            var calculatedOverlap = line1.OverlapSize(line2);

            Assert.That(calculatedOverlap, Is.EqualTo(overlap));
        }

        [TestCase(20, 0, 0, false, 0, 0, 0)]
        [TestCase(10, 8, 0, false, 0, 0, 0)]
        [TestCase(-20, 0, 0, false, 0, 0, 0)]
        [TestCase(-10, -8, 0, false, 0, 0, 0)]
        [TestCase(5, 0, 0, true, 5, 0, 0)]
        [TestCase(9, 0, 0, true, 1, 0, 0)]
        [TestCase(9, 8, 0, true, 1, 0, 0)]
        [TestCase(-5, 0, 0, true, -5, 0, 0)]
        [TestCase(-9, 0, 0, true, -1, 0, 0)]
        [TestCase(-9, -8, 0, true, -1, 0, 0)]
        [TestCase(6, 3, 9, true, 0, 0, 1)]
        public void IntersectionWithSatBoundingBoxWithBoundingBox(float x, float y, float z, bool hasCollision, float separationX, float separationY, float separationZ)
        {
            var distance = new Vector3(x, y, z);
            var boundingBox1 = new BoundingBox(new Vector3(-5), new Vector3(5));
            var boundingBox2 = new BoundingBox(new Vector3(-5) + distance, new Vector3(5) + distance);

            var collisionResult = boundingBox1.IntersectsWithSat(boundingBox2);
            var separation = new Vector3(separationX, separationY, separationZ);

            Assert.That(collisionResult, Is.Not.Null);
            Assert.That(collisionResult.Intersects, Is.EqualTo(hasCollision));
            Assert.That(collisionResult.Separtion, Is.EqualTo(separation));
        }
    }
}
