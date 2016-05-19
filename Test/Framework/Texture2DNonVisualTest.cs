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
        public void GetDataException(int rx, int ry, int rw, int rh, int startIndex, int elementsToRead)
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

        [TestCase(2000000)]
        [TestCase(2000)]
        [TestCase(4095)]
        [TestCase(4097)]

        [TestCase(4096)]
        public void SetData1ParameterGoodTest(int arraySize)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/LogoOnly_64px.png"))
            {
                Color[] data = new Color[arraySize];
                Color[] written = new Color[4096];
                for (int i = 0; i < arraySize; i++)
                {
                    data[i] = Color.White;
                }
                Texture2D t = Texture2D.FromStream(_game.GraphicsDevice, reader.BaseStream);
                t.SetData(data);
                t.GetData(written);
                for (int i = 0; i < written.Length; i++)
                {

                    if (i < arraySize)
                    {
                        Assert.AreEqual(255, written[i].R, "Bad color in position:{0};", i);
                        Assert.AreEqual(255, written[i].G, "Bad color in position:{0};", i);
                        Assert.AreEqual(255, written[i].B, "Bad color in position:{0};", i);
                        Assert.AreEqual(255, written[i].A, "Bad color in position:{0};", i);
                    }
                    else
                    {
                        Assert.AreNotEqual(255, written[i].R, "Bad color in position:{0};", i);
                        Assert.AreNotEqual(255, written[i].G, "Bad color in position:{0};", i);
                        Assert.AreNotEqual(255, written[i].B, "Bad color in position:{0};", i);
                        Assert.AreNotEqual(255, written[i].A, "Bad color in position:{0};", i);
                    }
                }
            }
        }
        [TestCase(4200,0,4096)]
        [TestCase(2000,0,4096)]

        [TestCase(4095, 0, 4095)]
        [TestCase(4095, 1, 4095)]
        [TestCase(4096, 1, 4096)]
        [TestCase(4096, 1, 4095)]
        [TestCase(4095, 1, 4096)]
        [TestCase(4096, 1, 4097)]
        [TestCase(4095, 0, 4094)]

        [TestCase(4097, 1, 4097)]
        [TestCase(4098, 1, 4097)]
        [TestCase(4097, 1, 4098)]
        [TestCase(4097, 0, 4097)]
        [TestCase(4096, 0, 4095)]

        [TestCase(4097, 1, 4096)]
        [TestCase(4097, 0, 4096)]
        [TestCase(4096, 0, 4096)]
        public void SetData3ParameterGoodTest(int arraySize, int startIndex, int elements)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/LogoOnly_64px.png"))
            {
                Color[] data = new Color[arraySize];
                Color[] written = new Color[4096];
                Color[] reference = new Color[4096];
                for (int i = 0; i < arraySize; i++)
                {
                    data[i] = Color.White;
                }
                Texture2D t = Texture2D.FromStream(_game.GraphicsDevice, reader.BaseStream);
                t.GetData(reference);
                t.SetData(data, startIndex, elements);
                t.GetData(written);
                for (int i = 0; i < written.Length; i++)
                {
                    if (i < arraySize)
                    {
                        Assert.AreEqual(255, written[i].R, "Bad color in position:{0};", i);
                        Assert.AreEqual(255, written[i].G, "Bad color in position:{0};", i);
                        Assert.AreEqual(255, written[i].B, "Bad color in position:{0};", i);
                        Assert.AreEqual(255, written[i].A, "Bad color in position:{0};", i);
                    }
                    else
                    {
                        Assert.AreEqual(reference[i].R, written[i].R, "Bad color in position:{0};", i);
                        Assert.AreEqual(reference[i].G, written[i].G, "Bad color in position:{0};", i);
                        Assert.AreEqual(reference[i].B, written[i].B, "Bad color in position:{0};", i);
                        Assert.AreEqual(reference[i].A, written[i].A, "Bad color in position:{0};", i);
                    }
                }
            }
        }

        //public void SetData5ParameterGoodTest(int arraySize, int startIndex, int elements)
        //{
        //    using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/LogoOnly_64px.png"))
        //    {
        //        Rectangle area = new Rectangle();
        //        Color[] data = new Color[arraySize];
        //        Color[] written = new Color[4096];
        //        for (int i = 0; i < arraySize; i++)
        //        {
        //            data[i] = Color.White;
        //        }
        //        Texture2D t = Texture2D.FromStream(_game.GraphicsDevice, reader.BaseStream);
        //        t.SetData(0, area, data, startIndex, elements);
        //        t.GetData(0, area, written, 0, elements);
        //        for (int i = 0; i < arraySize; i++)
        //        {
        //            if (i < arraySize)
                    //{
                    //    Assert.AreEqual(255, written[i].R, "Bad color in position:{0};", i);
                    //    Assert.AreEqual(255, written[i].G, "Bad color in position:{0};", i);
                    //    Assert.AreEqual(255, written[i].B, "Bad color in position:{0};", i);
                    //    Assert.AreEqual(255, written[i].A, "Bad color in position:{0};", i);
                    //}
                    //else
                    //{
                    //    Assert.AreNotEqual(255, written[i].R, "Bad color in position:{0};", i);
                    //    Assert.AreNotEqual(255, written[i].G, "Bad color in position:{0};", i);
                    //    Assert.AreNotEqual(255, written[i].B, "Bad color in position:{0};", i);
                    //    Assert.AreNotEqual(255, written[i].A, "Bad color in position:{0};", i);
                    //}
//        }
//    }
//}
[TestFixtureTearDown]
        public void TearDown()
        {
            _game.Dispose();
        }
    }
}
