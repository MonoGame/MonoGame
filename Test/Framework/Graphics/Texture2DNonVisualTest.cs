﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    internal class Texture2DNonVisualTest : GraphicsDeviceTestFixtureBase
    {
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
            Texture2D texture = null;
            using (System.IO.StreamReader reader = new System.IO.StreamReader(filename))
            {
                Assert.DoesNotThrow(() => texture = Texture2D.FromStream(gd, reader.BaseStream));
            }
            Assert.NotNull(texture);
            try
            {

                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(filename);
                System.Drawing.GraphicsUnit gu = System.Drawing.GraphicsUnit.Pixel;
                System.Drawing.RectangleF rf = bitmap.GetBounds(ref gu);
                Rectangle rt = texture.Bounds;
                Assert.AreEqual((int) rf.Bottom, rt.Bottom);
                Assert.AreEqual((int) rf.Left, rt.Left);
                Assert.AreEqual((int) rf.Right, rt.Right);
                Assert.AreEqual((int) rf.Top, rt.Top);
                bitmap.Dispose();
            }//The dds file test case can't be checked with System.Drawing because it does not understand this format
            catch { }
            texture.Dispose();
            texture = null;
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
                Assert.Throws<InvalidOperationException>(() => Texture2D.FromStream(gd, reader.BaseStream));
            }
        }

        [Test]
        public void FromStreamArgumentNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => Texture2D.FromStream(gd, (Stream) null));
#if !XNA
            // XNA misses this check and throws a NullReferenceException
            Assert.Throws<ArgumentNullException>(() => Texture2D.FromStream(null, new MemoryStream()));
#endif
        }

        [Test]
        public void ZeroSizeShouldFailTest()
        {
            Texture2D texture;
            Assert.Throws<ArgumentOutOfRangeException>(() => texture = new Texture2D(gd, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => texture = new Texture2D(gd, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => texture = new Texture2D(gd, 0, 0));
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
                Texture2D t = Texture2D.FromStream(gd, reader.BaseStream);
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

                t.Dispose();
            }
        }
        [TestCase(25, 23, 2, 2, 0, 2)]
        [TestCase(25, 23, 2, 2, 1, 2)]
        public void GetDataException(int rx, int ry, int rw, int rh, int startIndex, int elementsToRead)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/LogoOnly_64px.png"))
            {
                Rectangle toReadArea = new Rectangle(rx, ry, rw, rh);
                Texture2D t = Texture2D.FromStream(gd, reader.BaseStream);
                Color[] colors = new Color[startIndex + elementsToRead];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.White;
                }
                Assert.Throws<ArgumentException>(() => t.GetData(0, toReadArea, colors, startIndex, elementsToRead));

                t.Dispose();
            }
        }

        [TestCase(4096)]
        public void SetData1ParameterGoodTest(int arraySize)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/LogoOnly_64px.png"))
            {
                Color[] data = new Color[arraySize];
                Color[] reference = new Color[4096];
                Color[] written = new Color[4096];
                for (int i = 0; i < arraySize; i++)
                {
                    data[i] = Color.White;
                }
                Texture2D t = Texture2D.FromStream(gd, reader.BaseStream);
                t.GetData(reference);
                t.SetData(data);
                t.GetData(written);
                for (int i = 0; i < written.Length; i++)
                {
                    if (i < arraySize)
                    {
                        Assert.AreEqual(255, written[i].R, "Bad color written in position:{0};", i);
                        Assert.AreEqual(255, written[i].G, "Bad color written in position:{0};", i);
                        Assert.AreEqual(255, written[i].B, "Bad color written in position:{0};", i);
                        Assert.AreEqual(255, written[i].A, "Bad color written in position:{0};", i);
                    }
                    else
                    {
                        Assert.AreEqual(reference[i].R, written[i].R, "Color written in position:{0}; beyond array data", i);
                        Assert.AreEqual(reference[i].G, written[i].G, "Color written in position:{0}; beyond array data", i);
                        Assert.AreEqual(reference[i].B, written[i].B, "Color written in position:{0}; beyond array data", i);
                        Assert.AreEqual(reference[i].A, written[i].A, "Color written in position:{0}; beyond array data", i);
                    }
                }

                t.Dispose();
            }
        }

        [TestCase(2000)]
        [TestCase(4095)]
        [TestCase(2000000)]
        [TestCase(4097)]
        public void SetData1ParameterExceptionTest(int arraySize)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/LogoOnly_64px.png"))
            {
                Color[] data = new Color[arraySize];
                Color[] reference = new Color[4096];
                Color[] written = new Color[4096];
                for (int i = 0; i < arraySize; i++)
                {
                    data[i] = Color.White;
                }
                Texture2D t = Texture2D.FromStream(gd, reader.BaseStream);
                t.GetData(reference);
                Assert.Throws(Is.InstanceOf<Exception>(), () => t.SetData(data));
                t.GetData(written);
                for (int i = 0; i < written.Length; i++)
                {
                    Assert.AreEqual(reference[i].R, written[i].R, "Bad color written in position:{0};", i);
                    Assert.AreEqual(reference[i].G, written[i].G, "Bad color written in position:{0};", i);
                    Assert.AreEqual(reference[i].B, written[i].B, "Bad color written in position:{0};", i);
                    Assert.AreEqual(reference[i].A, written[i].A, "Bad color written in position:{0};", i);
                }
                t.Dispose();
            }
        }

        [TestCase(SurfaceFormat.HalfSingle, (short)(160 << 8 + 120))]
        [TestCase(SurfaceFormat.Vector4, (long)(200 << 48 + 180 << 32 + 160 << 16 + 120))]
        [TestCase(SurfaceFormat.Vector2, (float)(200 << 48 + 180 << 32 + 160 << 16 + 120))]
        [TestCase(SurfaceFormat.Color, (float)(200 << 24 + 180 << 16 + 160 << 8 + 120))]
        [TestCase(SurfaceFormat.Color, (byte)150)]
        [TestCase(SurfaceFormat.Color, (short)(160 << 8 + 120))]
        [TestCase(SurfaceFormat.Single, (byte)150)]
        [TestCase(SurfaceFormat.Single, (short)(160 << 8 + 120))]
        [TestCase(SurfaceFormat.Single, (float)(200 << 24 + 180 << 16 + 160 << 8 + 120))]
        public void SetDataFormatTest<TBuffer>(SurfaceFormat format, TBuffer value) where TBuffer : struct
        {
            const int textureSize = 16;

            var surfaceFormatSize = GetFormatSize(format);
            var textureSizeBytes = textureSize * surfaceFormatSize;

            var tSizeBytes = Marshal.SizeOf(typeof(TBuffer));
            var bufferSize = textureSizeBytes / tSizeBytes;

            var buffer = new TBuffer[bufferSize];
            for (var i = 0; i < bufferSize; i++)
                buffer[i] = value;

            var t = new Texture2D(gd, textureSize, 1, false, format);
            t.SetData(buffer);

            var buffer2 = new TBuffer[bufferSize];
            t.GetData(buffer2);

            for (var i = 0; i < buffer.Length; i++)
                Assert.AreEqual(buffer[i], buffer2[i]);

            t.Dispose();
        }

        [TestCase(SurfaceFormat.Color, (long)0)]
        [TestCase(SurfaceFormat.HalfSingle, (float)0)]
        public void SetDataFormatFailingTestTBufferTooLarge<TBuffer>(SurfaceFormat format, TBuffer value) where TBuffer : struct
        {
            const int textureSize = 16;

            var surfaceFormatSize = GetFormatSize(format);
            var textureSizeBytes = textureSize * surfaceFormatSize;

            var tSizeBytes = Marshal.SizeOf(typeof(TBuffer));
            var bufferSize = textureSizeBytes / tSizeBytes;

            var buffer = new TBuffer[bufferSize];
            for (var i = 0; i < bufferSize; i++)
                buffer[i] = value;

            var t = new Texture2D(gd, textureSize, 1, false, format);
            Assert.Throws<ArgumentException>(() => t.SetData(buffer));

            t.Dispose();
        }

        [Test]
        public void SetDataFormatFailingTestModTBufferNotZero()
        {
            const int textureSize = 12;
            var format = SurfaceFormat.Vector4;
            var value = new Vector3(20, 15, 18);

            var surfaceFormatSize = GetFormatSize(format);
            var textureSizeBytes = textureSize * surfaceFormatSize;

            var tSizeBytes = 12;
            var bufferSize = textureSizeBytes / tSizeBytes;

            var buffer = new Vector3[bufferSize];
            for (var i = 0; i < bufferSize; i++)
                buffer[i] = value;

            var t = new Texture2D(gd, textureSize, 1, false, format);
            Assert.Throws<ArgumentException>(() => t.SetData(buffer));

            t.Dispose();
        }

        public static int GetFormatSize(SurfaceFormat surfaceFormat)
        {
            switch (surfaceFormat)
            {
                case SurfaceFormat.Alpha8:
                    return 1;
                case SurfaceFormat.HalfSingle:
                    return 2;
                case SurfaceFormat.Single:
                case SurfaceFormat.Color:
                    return 4;
                case SurfaceFormat.Vector2:
                    return 8;
                case SurfaceFormat.Vector4:
                    return 16;
                default:
                    throw new ArgumentException();
            }
        }

        [TestCase(4200, 0, 4096)]
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
                Texture2D t = Texture2D.FromStream(gd, reader.BaseStream);
                t.GetData(reference);
                t.SetData(data, startIndex, elements);
                t.GetData(written);
                for (int i = 0; i < written.Length; i++)
                {
                    if (i < arraySize)
                    {
                        Assert.AreEqual(255, written[i].R, "Bad color written in position:{0};", i);
                        Assert.AreEqual(255, written[i].G, "Bad color written in position:{0};", i);
                        Assert.AreEqual(255, written[i].B, "Bad color written in position:{0};", i);
                        Assert.AreEqual(255, written[i].A, "Bad color written in position:{0};", i);
                    }
                    else
                    {
                        Assert.AreEqual(reference[i].R, written[i].R, "Color written in position:{0}; beyond array data", i);
                        Assert.AreEqual(reference[i].G, written[i].G, "Color written in position:{0}; beyond array data", i);
                        Assert.AreEqual(reference[i].B, written[i].B, "Color written in position:{0}; beyond array data", i);
                        Assert.AreEqual(reference[i].A, written[i].A, "Color written in position:{0}; beyond array data", i);
                    }
                }

                t.Dispose();
            }
        }

        [TestCase(2000, 0, 4096)]
        [TestCase(4095, 0, 4095)]
        [TestCase(4095, 1, 4095)]
        [TestCase(4096, 1, 4096)]
        [TestCase(4096, 1, 4095)]
        [TestCase(4095, 1, 4096)]
        [TestCase(4096, 1, 4097)]
        [TestCase(4095, 0, 4094)]
        [TestCase(4097, 1, 4097)]
        [TestCase(4097, 1, 4098)]
        [TestCase(4098, 1, 4097)]
        [TestCase(4097, 0, 4097)]
        [TestCase(4096, 0, 4095)]
        public void SetData3ParameterExceptionTest(int arraySize, int startIndex, int elements)
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
                Texture2D t = Texture2D.FromStream(gd, reader.BaseStream);
                t.GetData(reference);
                Assert.Throws(Is.InstanceOf<Exception>(), () => t.SetData(data, startIndex, elements));
                t.GetData(written);
                for (int i = 0; i < written.Length; i++)
                {
                    Assert.AreEqual(reference[i].R, written[i].R, "Bad color written in position:{0};", i);
                    Assert.AreEqual(reference[i].G, written[i].G, "Bad color written in position:{0};", i);
                    Assert.AreEqual(reference[i].B, written[i].B, "Bad color written in position:{0};", i);
                    Assert.AreEqual(reference[i].A, written[i].A, "Bad color written in position:{0};", i);
                }

                t.Dispose();
            }
        }

        [TestCase(4096, 0, 4096, 0, 0, 64, 64)]
        [TestCase(4096, 0, 3969, 1, 1, 63, 63)]
        [TestCase(3969, 0, 3969, 1, 1, 63, 63)]
        [TestCase(4096, 1, 3969, 1, 1, 63, 63)]
        [TestCase(4097, 1, 3969, 1, 1, 63, 63)]
        [TestCase(3970, 1, 3969, 1, 1, 63, 63)]
        [TestCase(4097, 1, 4096, 0, 0, 64, 64)]
        public void SetData5ParameterGoodTest(int arraySize, int startIndex, int elements, int x, int y, int w, int h)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/LogoOnly_64px.png"))
            {
                Rectangle area = new Rectangle(x, y, w, h);
                int areaLength = w * h;
                Color[] data = new Color[arraySize];
                Color[] reference = new Color[4096];
                Color[] written = new Color[4096];
                for (int i = 0; i < arraySize; i++)
                {
                    data[i] = Color.White;
                }
                Texture2D t = Texture2D.FromStream(gd, reader.BaseStream);
                t.GetData(reference);
                t.SetData(0, area, data, startIndex, elements);
                t.GetData(written);
                for (int i = 0; i < written.Length; i++)
                {
                    int rx = i % 64, ry = i / 64;
                    if (area.Contains(rx, ry))
                    {
                        Assert.AreEqual(255, written[i].R, "Bad color written in position:{0};", i);
                        Assert.AreEqual(255, written[i].G, "Bad color written in position:{0};", i);
                        Assert.AreEqual(255, written[i].B, "Bad color written in position:{0};", i);
                        Assert.AreEqual(255, written[i].A, "Bad color written in position:{0};", i);
                    }
                    else
                    {
                        Assert.AreEqual(reference[i].R, written[i].R, "Color written in position:{0}; beyond array data", i);
                        Assert.AreEqual(reference[i].G, written[i].G, "Color written in position:{0}; beyond array data", i);
                        Assert.AreEqual(reference[i].B, written[i].B, "Color written in position:{0}; beyond array data", i);
                        Assert.AreEqual(reference[i].A, written[i].A, "Color written in position:{0}; beyond array data", i);
                    }
                }

                t.Dispose();
            }
        }
        [TestCase(3844, 0, 3844, 1, 1, 63, 63)]
        [TestCase(3845, 1, 3844, 1, 1, 63, 63)]
        [TestCase(3969, 0, 4096, 1, 1, 63, 63)]
        [TestCase(3970, 1, 4096, 1, 1, 63, 63)]
        [TestCase(4096, 0, 4096, -1, -1, 65, 65)]
        [TestCase(4096, 0, 4096, 1, 1, 63, 63)]
        [TestCase(4096, 0, 4095, 0, 0, 64, 64)]
        [TestCase(4096, 0, 3844, 1, 1, 63, 63)]
        [TestCase(4097, 1, 4096, 1, 1, 63, 63)]
        [TestCase(4097, 1, 4095, 0, 0, 64, 64)]
        [TestCase(4097, 1, 3844, 1, 1, 63, 63)]
        [TestCase(3970, 1, 4096, 1, 1, 63, 63)]
        public void SetData5ParameterExceptionTest(int arraySize, int startIndex, int elements, int x, int y, int w, int h)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader("Assets/Textures/LogoOnly_64px.png"))
            {
                Rectangle area = new Rectangle(x, y, w, h);
                int areaLength = w * h;
                Color[] data = new Color[arraySize];
                Color[] reference = new Color[4096];
                Color[] written = new Color[4096];
                for (int i = 0; i < arraySize; i++)
                {
                    data[i] = Color.White;
                }
                Texture2D t = Texture2D.FromStream(gd, reader.BaseStream);
                t.GetData(reference);
                Assert.Throws(Is.InstanceOf<Exception>(), () => t.SetData(0, area, data, startIndex, elements));
                t.GetData(written);
                for (int i = 0; i < written.Length; i++)
                {
                    Assert.AreEqual(reference[i].R, written[i].R, "Bad color written in position:{0};", i);
                    Assert.AreEqual(reference[i].G, written[i].G, "Bad color written in position:{0};", i);
                    Assert.AreEqual(reference[i].B, written[i].B, "Bad color written in position:{0};", i);
                    Assert.AreEqual(reference[i].A, written[i].A, "Bad color written in position:{0};", i);
                }

                t.Dispose();
            }
        }

        [Test]
        public void GetDataNegativeOrZeroRectWidthAndHeightThrows()
        {
            using (var t = new Texture2D(gd, 10, 10))
            {
                var data = new Color[4];
                var data2 = new Color[0];
                Assert.Throws<ArgumentException>(() => t.GetData(0, new Rectangle(5, 5,  2, -2), data, 0, 4));
                Assert.Throws<ArgumentException>(() => t.GetData(0, new Rectangle(5, 5, -2,  2), data, 0, 4));
                Assert.Throws<ArgumentException>(() => t.GetData(0, new Rectangle(5, 5, -2, -2), data, 0, 4));

                Assert.Throws<ArgumentException>(() => t.GetData(0, new Rectangle(5, 5, 0, 2), data2, 0, 4));
                Assert.Throws<ArgumentException>(() => t.GetData(0, new Rectangle(5, 5, 2, 0), data2, 0, 4));
                Assert.Throws<ArgumentException>(() => t.GetData(0, new Rectangle(5, 5, 0, 0), data2, 0, 4));
            }
        }

        [Test]
        public void GetAndSetDataDxtCompressed()
        {
            var t = content.Load<Texture2D>(Paths.Texture ("random_16px_dxt"));

            var b = new byte[t.Width*t.Height/2];
            var b2 = new byte[t.Width*t.Height/2];

            t.GetData(b);
            t.SetData(b);
            t.GetData(b2);

            Assert.AreEqual(b, b2);

            // MonoGame allows any kind of type that is not larger than one element while XNA only allows byte
#if !XNA
            var b3 = new short[t.Width*t.Height/4];
            t.GetData(b3);
            t.SetData(b3);

            t.GetData(b2);
            Assert.AreEqual(b, b2);

            var b4 = new int[t.Width*t.Height/8];
            t.GetData(b4);
            t.SetData(b4);

            t.GetData(b2);
            Assert.AreEqual(b, b2);

            var b5 = new long[t.Width*t.Height/16];
            t.GetData(b5);
            t.SetData(b5);

            t.GetData(b2);
            Assert.AreEqual(b, b2);

            // this is too large, DXT1 blocks are 64 bits while Vector4 is 128 bits
            var b6 = new Vector4[t.Width*t.Height/32];
            Assert.Throws<ArgumentException>(() => t.GetData(b6));
            Assert.Throws<ArgumentException>(() => t.SetData(b6));

            var b7 = new Vector3[t.Width*t.Height/24];
            Assert.Throws<ArgumentException>(() => t.GetData(b7));
            Assert.Throws<ArgumentException>(() => t.SetData(b7));
#endif

            t.Dispose();
        }

        // DXT1
        [TestCase(8, "random_16px_dxt", 0)]
        [TestCase(8, "random_16px_dxt", 1)]
        // DXT5
        [TestCase(16, "random_16px_dxt_alpha", 0)]
        [TestCase(16, "random_16px_dxt_alpha", 1)]
        public void GetAndSetDataDxtNotMultipleOf4Rounding(int bs, string texName, int mip)
        {
            var t = content.Load<Texture2D>(Paths.Texture (texName));

            var before = new byte[t.Width*t.Height*bs/16];
            t.GetData(before);

            var b1 = new byte[bs];
            var b2 = new byte[bs];

            t.GetData(mip, new Rectangle(0, 0, 4, 4), b1, 0, bs);

            t.GetData(mip, new Rectangle(0,0,1,1), b2, 0, bs);
            t.SetData(mip, new Rectangle(0,0,1,1), b2, 0, bs);
            Assert.AreEqual(b1, b2);

            t.GetData(mip, new Rectangle(0,0,1,3), b2, 0, bs);
            t.SetData(mip, new Rectangle(0,0,1,3), b2, 0, bs);
            Assert.AreEqual(b1, b2);

            t.GetData(mip, new Rectangle(0,0,4,3), b2, 0, bs);
            t.SetData(mip, new Rectangle(0,0,4,3), b2, 0, bs);
            Assert.AreEqual(b1, b2);

            t.GetData(mip, new Rectangle(0, 2, 4, 4), b2, 0, bs);
            t.SetData(mip, new Rectangle(0, 2, 4, 4), b2, 0, bs);
            Assert.AreEqual(b1, b2);

            t.GetData(mip, new Rectangle(2, 2, 4, 4), b2, 0, bs);
            t.SetData(mip, new Rectangle(2, 2, 4, 4), b2, 0, bs);
            Assert.AreEqual(b1, b2);

            t.GetData(mip, new Rectangle(3, 3, 4, 4), b2, 0, bs);
            t.SetData(mip, new Rectangle(3, 3, 4, 4), b2, 0, bs);
            Assert.AreEqual(b1, b2);

            t.GetData(mip, new Rectangle(4, 4, 4, 4), b2, 0, bs);
            t.SetData(mip, new Rectangle(4, 4, 4, 4), b2, 0, bs);
            Assert.AreNotEqual(b1, b2);

            var after = new byte[t.Width*t.Height*bs/16];
            t.GetData(after);

            Assert.AreEqual(before, after);

            t.Dispose();
        }

        [TestCase("random_16px_dxt", 8)]
        [TestCase("random_16px_dxt_alpha", 16)]
        public void GetAndSetDataDxtDontRoundWhenOutsideBounds(string texName, int bs)
        {
            var t = content.Load<Texture2D>(Paths.Texture(texName));

            var b = new byte[bs];

            // don't round if the unrounded rectangle would be outside the texture area
            Assert.Throws<ArgumentException>(() => t.GetData(0, new Rectangle(15, 15, 3, 3), b, 0, bs));
            // this does work
            t.GetData(0, new Rectangle(15, 15, 1, 1), b, 0, bs);

            t.Dispose();
        }

        [TestCase("random_16px_dxt", 8)]
        [TestCase("random_16px_dxt_alpha", 16)]
        public void GetAndSetDataDxtLowerMips(string texName, int bs)
        {
            var t = content.Load<Texture2D>(Paths.Texture(texName));

            var b = new byte[bs];
            var b2 = new byte[bs];

            t.GetData(0, new Rectangle(0,0,4,4), b, 0, bs);
            t.GetData(1, new Rectangle(0,0,4,4), b2, 0, bs);
            t.GetData(2, new Rectangle(0,0,4,4), b2, 0, bs);
            t.GetData(3, new Rectangle(0,0,2,2), b2, 0, bs);
            t.GetData(4, new Rectangle(0,0,1,1), b2, 0, bs);
            t.SetData(3, new Rectangle(0,0,2,2), b2, 0, bs);
            
            // would be rounded, but the rectangle is outside the texture area so it wil throw before rounding
            Assert.Throws<ArgumentException>(() => t.GetData(3, new Rectangle(1, 1, 2, 2), b, 0, bs));
            Assert.Throws<ArgumentException>(() => t.GetData(3, new Rectangle(0, 0, 3, 3), b, 0, bs));

            t.Dispose();
        }

        #region Get and SetData advanced

        [TestCase(1, 1)]
        [TestCase(8, 8)]
        [TestCase(31, 7)]
        public void ShouldSetAndGetDataForLevel(int width, int height)
        {
            var texture2D = new Texture2D(gd, width, height, true, SurfaceFormat.Color);

            for (int i = 0; i < texture2D.LevelCount; i++)
            {
                var levelSize = Math.Max(width >> i, 1) * Math.Max(height >> i, 1);

                var savedData = new Color[levelSize];
                for (var index = 0; index < levelSize; index++)
                    savedData[index] = new Color(index % 255, index % 255, index % 255);
                texture2D.SetData(i, null, savedData, 0, savedData.Length);

                var readData = new Color[levelSize];
                texture2D.GetData(i, null, readData, 0, savedData.Length);

                Assert.AreEqual(savedData, readData);
            }

            texture2D.Dispose();
        }

        [Test]
        public void ShouldGetDataFromRectangle()
        {
            const int dataSize = 128 * 128;
            var texture2D = new Texture2D(gd, 128, 128, false, SurfaceFormat.Color);
            var savedData = new Color[dataSize];
            for (var index = 0; index < dataSize; index++) savedData[index] = new Color(index % 255, index % 255, index % 255);
            texture2D.SetData(savedData);

            var readData = new Color[4];
            texture2D.GetData(0, new Rectangle(126, 126, 2, 2), readData, 0, 4);

            var expectedData = new[]
            {
                new Color(189, 189, 189),
                new Color(190, 190, 190),
                new Color(62, 62, 62),
                new Color(63, 63, 63)
            };
            Assert.AreEqual(expectedData, readData);

            texture2D.Dispose();
        }
	
#if !XNA
        [TestCase(1, 1)]
        [TestCase(8, 8)]
        [TestCase(31, 7)]
        public void ShouldSetAndGetDataForTextureArray(int width, int height)
        {
            const int arraySize = 4;
            var texture2D = new Texture2D(gd, width, height, true, SurfaceFormat.Color, arraySize);

            for (var i = 0; i < arraySize; i++)
                for (var j = 0; j < texture2D.LevelCount; j++)
                {
                    var levelSize = Math.Max(width >> j, 1) * Math.Max(height >> j, 1);

                    var savedData = new Color[levelSize];
                    for (var index = 0; index < levelSize; index++)
                        savedData[index] = new Color((index + i) % 255, (index + i) % 255, (index + i) % 255);
                    texture2D.SetData(j, i, null, savedData, 0, savedData.Length);

                    var readData = new Color[levelSize];
                    texture2D.GetData(j, i, null, readData, 0, readData.Length);

                    Assert.AreEqual(savedData, readData);
                }

            texture2D.Dispose();
        }
#endif

        #endregion
    }
}
