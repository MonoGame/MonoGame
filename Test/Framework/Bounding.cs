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
    }
}
