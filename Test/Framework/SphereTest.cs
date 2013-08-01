using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Microsoft.Xna.Framework;

namespace MonoGame.Tests.Framework
{
    class SphereTest
    {
        [Test]
        public void BoundingBoxIntersects()
        {
            BoundingBox box = new BoundingBox();
            box.Min = new Vector3(-5, -5, -5);
            box.Max = new Vector3(5, 5, 5);

            BoundingSphere sphere = new BoundingSphere(new Vector3(), 1);

            const int samples = 10;//number of spheres along each axis
            float delta = 10f/samples;

            for (int j = 0; j <=samples ; j++)
            {
                // test top and bottom shallow intersect on x asis
                sphere.Center = new Vector3(-6, -5+j*delta, 5.7f);
                Assert.AreEqual(false, box.Intersects(sphere));
                sphere.Center = new Vector3(-6, -5 + j * delta, -5.7f);
                Assert.AreEqual(false, box.Intersects(sphere));
                for (int i = 0; i <= 10; i++)
                {
                    sphere.Center = new Vector3(-5 + i * delta, -5 + j * delta, 5.7f);
                    Assert.AreEqual(true, box.Intersects(sphere));

                    sphere.Center.Z = -sphere.Center.Z;
                    Assert.AreEqual(true, box.Intersects(sphere));
                }
                sphere.Center = new Vector3(6, -5 + j * delta, 5.7f);
                Assert.AreEqual(false, box.Intersects(sphere));
                sphere.Center = new Vector3(6, -5 + j * delta, -5.7f);
                Assert.AreEqual(false, box.Intersects(sphere));

                // z axis
                sphere.Center = new Vector3(5.7f, -5 + j * delta, -6f);
                Assert.AreEqual(false, box.Intersects(sphere));
                sphere.Center = new Vector3(-5.7f, -5 + j * delta, -6f);
                Assert.AreEqual(false, box.Intersects(sphere));
                for (int i = 0; i <= 10; i++)
                {
                    sphere.Center = new Vector3(5.7f, -5 + j * delta, -5 + i * delta);
                    Assert.AreEqual(true, box.Intersects(sphere));

                    sphere.Center.X = -sphere.Center.X;
                    Assert.AreEqual(true, box.Intersects(sphere));
                }
                sphere.Center = new Vector3(5.7f, -5 + j * delta, 6);
                Assert.AreEqual(false, box.Intersects(sphere));
                sphere.Center = new Vector3(-5.7f, -5 + j * delta, 6);
                Assert.AreEqual(false, box.Intersects(sphere));
            }
        }
    }
}
