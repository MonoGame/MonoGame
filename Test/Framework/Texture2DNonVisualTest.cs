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
        [TestCase(25, 23, 1, 1, 0, 1)]
        [TestCase(25, 23, 1, 1, 1, 1)]
        [TestCase(25, 23, 2, 1, 0, 2)]
        [TestCase(25, 23, 2, 1, 1, 2)]
        [TestCase(25, 23, 1, 2, 0, 2)]
        [TestCase(25, 23, 1, 2, 1, 2)]
        [TestCase(25, 23, 2, 2, 0, 4)]
        [TestCase(25, 23, 2, 2, 1, 4)]

        public void PlatformGetDataWithOffsetTest(int rx, int ry, int rw, int rh, int startIndex, int elementsToRead)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/LogoOnly_64px.png"))
            {
                Rectangle toReadArea = new Rectangle(rx, ry, rw, rh);
                Texture2D t = Texture2D.FromStream(_game.GraphicsDevice, reader.BaseStream);
                Color[] colors = new Color[startIndex + elementsToRead];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.White;
                }
                t.GetData(0, toReadArea, colors, startIndex, elementsToRead);
                for (int i = 0; i < elementsToRead; i++)
                {
                    Assert.AreNotEqual(255, colors[i + startIndex].R, "colors was not overwritten in position {0}", startIndex + i);
                }
            }
        }
        [TestCase(25, 23, 2, 2, 0, 2)]
        [TestCase(25, 23, 2, 2, 1, 2)]
        public void GetDataExceptionTest(int rx, int ry, int rw, int rh, int startIndex, int elementsToRead)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/LogoOnly_64px.png"))
            {
                Rectangle toReadArea = new Rectangle(rx, ry, rw, rh);
                Texture2D t = Texture2D.FromStream(_game.GraphicsDevice, reader.BaseStream);
                Color[] colors = new Color[startIndex + elementsToRead];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.White;
                }
                Assert.Throws<ArgumentException>(() => t.GetData(0, toReadArea, colors, startIndex, elementsToRead));
            }
        }

#if !XNA
        [TestCase(0, 4095, 4096)]
        [TestCase(0, 4095, 4595)]
        [TestCase(1, 4095, 4096)]
        [TestCase(1, 4095, 4097)]
#endif
        [TestCase(0, 4096, 4096)]
        [TestCase(1, 4096, 4097)]
        public void GetDataTintintTest(int startIndex, int elementsToRead, int arraySize)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/LogoOnly_64px.png"))
            {
                Texture2D t = Texture2D.FromStream(_game.GraphicsDevice, reader.BaseStream);
                Color[] colors = new Color[arraySize];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.White;
                }
                t.GetData(colors, startIndex, elementsToRead);
                for (int i = 0; i < startIndex; i++)
                {
                    Assert.AreEqual(255, colors[i].G, "color written to position {0} before starting index", i);
                }
                for (int i = 0; i < elementsToRead; i++)
                {
                    Assert.AreNotEqual(255, colors[i + startIndex].G, "colors found in position {0} was {{R{1},G{2},B{3},A{4}}}", startIndex + i, colors[i + startIndex].R, colors[i + startIndex].G, colors[i + startIndex].B, colors[i + startIndex].A);
                    Assert.True((colors[i + startIndex].A > 0 && colors[i + startIndex].G < 255) || colors[i + startIndex].G == 0);
                }
                for (int i = startIndex + elementsToRead; i < arraySize; i++)
                {
                    Assert.AreEqual(255, colors[i].G, "color written to position {0} after requested data", i);
                }
            }
        }
        [TestCase(1, 4097, 4095)]
        [TestCase(0, 4097, 4096)]
        [TestCase(1, 4097, 4097)]
        [TestCase(1, 4097, 4096)]
        [TestCase(1, 4096, 4095)]
        [TestCase(1, 4096, 4096)]
        [TestCase(1, 4095, 4095)]
#if XNA
        [TestCase(0, 4095, 4096)]
        [TestCase(0, 4095, 4595)]
        [TestCase(1, 4095, 4096)]
        [TestCase(1, 4095, 4097)]
#endif
        public void GetDataTintintExceptionTest(int startIndex, int elementsToRead, int arraySize)
        {
            Console.Error.Write("\n{0},{1},{2}", startIndex, elementsToRead, arraySize);
            using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/LogoOnly_64px.png"))
            {
                Texture2D t = Texture2D.FromStream(_game.GraphicsDevice, reader.BaseStream);
                Color[] colors = new Color[arraySize];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.White;
                }
                Assert.Throws(Is.InstanceOf<Exception>(),()=>t.GetData(colors, startIndex, elementsToRead));
            }
        }
        [TestFixtureTearDown]
        public void TearDown()
        {
            _game.Dispose();
        }
    }
}
