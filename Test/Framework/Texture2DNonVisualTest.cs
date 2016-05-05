// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    public class Texture2DNonVisualTest
    {
        Texture2D _texture;
        TestGameBase _game;
        [SetUp]
        public void Setup()
        {
            _game = new TestGameBase();
            var graphicsDeviceManager = new GraphicsDeviceManager(_game);
#if XNA
            graphicsDeviceManager.ApplyChanges();
#else
            graphicsDeviceManager.CreateDevice();
#endif
        }

#if !XNA
        [TestCase("Assets/Textures/LogoOnly_64px.bmp")]
        [TestCase("Assets/Textures/LogoOnly_64px.dds")]
        [TestCase("Assets/Textures/LogoOnly_64px.tif")]
#endif
        [TestCase("Assets/Textures/LogoOnly_64px.gif")]
        [TestCase("Assets/Textures/LogoOnly_64px.jpg")]
        [TestCase("Assets/Textures/LogoOnly_64px.png")]
        public void FromStreamShouldWorkTest(string filename)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader(filename))
            {
                Assert.DoesNotThrow(() => _texture = Texture2D.FromStream(_game.GraphicsDevice, reader.BaseStream));
            }
            Assert.NotNull(_texture);
            try
            {

                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(filename);
                System.Drawing.GraphicsUnit gu = System.Drawing.GraphicsUnit.Pixel;
                System.Drawing.RectangleF rf = bitmap.GetBounds(ref gu);
                Rectangle rt = _texture.Bounds;
                Assert.AreEqual((int) rf.Bottom, rt.Bottom);
                Assert.AreEqual((int) rf.Left, rt.Left);
                Assert.AreEqual((int) rf.Right, rt.Right);
                Assert.AreEqual((int) rf.Top, rt.Top);
                bitmap.Dispose();
            }//The dds file test case can't be checked with System.Drawing because it does not understand this format
            catch { }
            _texture.Dispose();
            _texture = null;
        }

#if XNA
        [TestCase("Assets/Textures/LogoOnly_64px.bmp")]
        [TestCase("Assets/Textures/LogoOnly_64px.dds")]
        [TestCase("Assets/Textures/LogoOnly_64px.tif")]
#endif
        [TestCase("Assets/Textures/LogoOnly_64px.tga")]
        [TestCase("Assets/Textures/SampleCube64DXT1Mips.dds")]
        public void FromStreamShouldFailTest(string filename)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader(filename))
            {
                Assert.Throws<InvalidOperationException>(() => _texture = Texture2D.FromStream(_game.GraphicsDevice, reader.BaseStream));
            }
        }
        [TestFixtureTearDown]
        public void TearDown()
        {
            _game.Dispose();
        }
        [Test]
        public void TransparentTexture()
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/Transparent.png"))
            {
                Texture2D t = Texture2D.FromStream(_game.GraphicsDevice, reader.BaseStream);
                Color[] data = new Color[1];
                t.GetData<Color>(0, new Rectangle(10, 10, 1, 1), data, 0, 1);
                Color expected = new Color(0, 0, 0, 0);
                //Console.Error.WriteLine("\nR: {0}, G: {1}, B: {2}, A: {3};", data[0].R, data[0].G, data[0].B, data[0].A);
                Assert.AreEqual(expected.R, data[0].R,"Red");
                Assert.AreEqual(expected.G, data[0].G,"Green");
                Assert.AreEqual(expected.B, data[0].B,"Blue");
                Assert.AreEqual(expected.A, data[0].A,"Alpha");
            }
        }
        [Test]
        public void NearlyTransparentTexture()
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/NearlyTransparent.png"))
            {
                Texture2D t = Texture2D.FromStream(_game.GraphicsDevice, reader.BaseStream);
                Color[] data = new Color[1];
                t.GetData<Color>(0, new Rectangle(10, 10, 1, 1), data, 0, 1);
                Color expected = new Color(254, 128, 63, 129);
                Console.Error.WriteLine("\nFound\nR: {0}, G: {1}, B: {2}, A: {3};", data[0].R, data[0].G, data[0].B, data[0].A);
                Console.Error.WriteLine("Expected\nR: {0}, G: {1}, B: {2}, A: {3};", expected.R, expected.G, expected.B, expected.A);
                Assert.AreEqual(expected.R, data[0].R, "Red");
                Assert.AreEqual(expected.G, data[0].G,"Green");
                Assert.AreEqual(expected.B, data[0].B,"Blue");
                Assert.AreEqual(expected.A, data[0].A,"Alpha");
            }
        }
    }
}
