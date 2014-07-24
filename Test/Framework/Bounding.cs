using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
