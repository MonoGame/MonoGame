// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using NUnit.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    public class Texture3DNonVisualTest
    {
        Texture3D t;
        Color[] reference;
        const int w=50, h=50, d=50, a = w * d * h;
        private Game _game;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            reference = new Color[a];
            _game = new Game();
            var graphicsDeviceManager = new GraphicsDeviceManager(_game);
            graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            graphicsDeviceManager.ApplyChanges();

            t = new Texture3D(_game.GraphicsDevice, w, h, d, false, SurfaceFormat.Color);
            for (int layer = 0; layer < d; layer++)
            {
                for (int i = 0; i < w * h; i++)
                {
                    reference[layer * w * h + i] = new Color(layer * 5, layer * 5, layer * 5, layer * 5);
                }
            }
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            _game.Dispose();
            t.Dispose();
        }

        [SetUp]
        public void TestSetUp()
        {
            t.SetData(reference);
        }

        [Test]
        public void ZeroSizeShouldFailTest()
        {
            Texture3D texture;
            var gd = _game.GraphicsDevice;
            Assert.Throws<ArgumentOutOfRangeException>(() => texture = new Texture3D(gd, 0, 1, 1, false, SurfaceFormat.Color));
            Assert.Throws<ArgumentOutOfRangeException>(() => texture = new Texture3D(gd, 1, 0, 1, false, SurfaceFormat.Color));
            Assert.Throws<ArgumentOutOfRangeException>(() => texture = new Texture3D(gd, 1, 1, 0, false, SurfaceFormat.Color));
            Assert.Throws<ArgumentOutOfRangeException>(() => texture = new Texture3D(gd, 0, 0, 1, false, SurfaceFormat.Color));
            Assert.Throws<ArgumentOutOfRangeException>(() => texture = new Texture3D(gd, 1, 0, 0, false, SurfaceFormat.Color));
            Assert.Throws<ArgumentOutOfRangeException>(() => texture = new Texture3D(gd, 0, 1, 0, false, SurfaceFormat.Color));
            Assert.Throws<ArgumentOutOfRangeException>(() => texture = new Texture3D(gd, 0, 0, 0, false, SurfaceFormat.Color));
        }

        [Test]
        public void SetData1ParameterTest()
        {
            Color[] written = new Color[a];
            t.GetData(written);
            Assert.AreEqual(reference, written);
        }

        [TestCase(a, 0, a)]
        [TestCase(a + 1, 0, a)]
        [TestCase(a + 1, 1, a)]
        public void SetData3ParametersSuccessTest(int arrayLength, int startIndex, int elementCount)
        {
            Color[] write = new Color[arrayLength];
            Color[] written = new Color[a];
            for (int i = startIndex; i < arrayLength; i++)
            {
                write[i] = new Color(23, 23, 23, 23);
            }
            t.SetData(write, startIndex, elementCount);
            t.GetData(written);
            for (int i = 0; i < a; i++)
            {
                if (i < startIndex)
                    Assert.AreNotEqual(reference[i], written[i], string.Format("Color written from before startIndex"));
                else if (i < elementCount + startIndex)
                    Assert.AreEqual(write[i + startIndex], written[i], string.Format("bad color in position {0}", i));
                else
                    Assert.AreEqual(reference[i], written[i], string.Format("Color written after elementCount"));
            }

        }

        [TestCase(a, 0, a - 1)]
        [TestCase(a - 1, 0, a)]
        [TestCase(a, 1, a)]
        [TestCase(a, 0, a + 1)]
        [TestCase(a + 1, 1, a + 1)]
        public void SetData3ParametersExceptionTest(int arrayLength, int startIndex, int elementCount)
        {
            Color[] write = new Color[arrayLength];
            Color[] written = new Color[a];
            for (int i = startIndex; i < arrayLength; i++)
            {
                write[i] = new Color(23, 23, 23, 23);
            }
            Assert.Throws(Is.InstanceOf<Exception>(), () =>t.SetData(write, startIndex, elementCount));
        }

        [TestCase((w - 2) * (h - 2) * (d - 2), 0, (w - 2) * (h - 2) * (d - 2), 1, 1, 1, w - 2, h - 2, d - 2)]
        [TestCase(a, 0, a, 0, 0, 0, w, h, d)]
        [TestCase(a + 1, 1, a, 0, 0, 0, w, h, d)]
        public void SetData9ParametersSuccessTest(int arrayLength, int startIndex, int elementCount, int x, int y, int z, int w, int h, int d)
        {
            Color[] write = new Color[arrayLength];
            Color[] written = new Color[Texture3DNonVisualTest.w * Texture3DNonVisualTest.h * Texture3DNonVisualTest.d];
            for (int i = startIndex; i < arrayLength; i++)
            {
                write[i] = new Color(23, 23, 23, 23);
            }
            t.SetData(0, x, y, x + w, y + h, z, z + d, write, startIndex, elementCount);
            t.GetData(written);
            int cx, cy, cz;
            for (int i = 0, j = 0; i < Texture3DNonVisualTest.w * Texture3DNonVisualTest.h * Texture3DNonVisualTest.d; i++)
            {
                cx = i % Texture3DNonVisualTest.w;
                cy = (i / Texture3DNonVisualTest.w) % Texture3DNonVisualTest.h;
                cz = ((i / Texture3DNonVisualTest.w) / Texture3DNonVisualTest.h) % Texture3DNonVisualTest.d;
                if (cx >= x && cx < w + x && cy >= y && cy < h + y && cz >= z && cz < d + z)
                    Assert.AreEqual(write[startIndex + j++], written[i], string.Format("bad color in position x:{0};y:{1};z:{2};i:{3}", cx, cy, cz, i));
                else
                    Assert.AreEqual(reference[i], written[i], string.Format("bad color in position x:{0};y:{1};z:{2};i:{3}, outside requested area", cx, cy, cz, i));
            }
        }

        [TestCase(a, 0, a, -1, -1, -1, w + 1, h + 1, d + 1)]
        [TestCase(a, 1, a, 0, 0, 0, w, h, d)]
        public void SetData9ParametersExceptionTest(int arrayLength, int startIndex, int elementCount, int x, int y, int z, int w, int h, int d)
        {
            Color[] write = new Color[arrayLength];
            for (int i = startIndex; i < arrayLength; i++)
            {
                write[i] = new Color(23, 23, 23, 23);
            }
            Assert.Throws(Is.InstanceOf<Exception>(), () => t.SetData(0, x, y, x + w, y + h, z, z + d, write, startIndex, elementCount));
        }

        [Test]
        public void NullDeviceShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
            {                
                var texture = new Texture3D(null, 16, 16, 16, false, SurfaceFormat.Color);
                texture.Dispose();
            });
            GC.GetTotalMemory(true); // collect uninitialized Texture
        }
    }
}
