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
