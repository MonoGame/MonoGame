// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    class TextureCubeTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ZeroSizeShouldFailTest()
        {
            TextureCube texture;
            Assert.Throws<ArgumentOutOfRangeException>(() => texture = new TextureCube(gd, 0, false, SurfaceFormat.Color));
        }

        [TestCase(1)]
        [TestCase(8)]
        [TestCase(31)]
        public void ShouldSetAndGetData(int size)
        {
            var dataSize = size * size;
            var textureCube = new TextureCube(gd, size, false, SurfaceFormat.Color);

            for (var i = 0; i < 6; i++)
            {
                var savedData = new Color[dataSize];
                for (var index = 0; index < dataSize; index++)
                    savedData[index] = new Color(index + i, index + i, index + i);
                textureCube.SetData((CubeMapFace) i, savedData);

                var readData = new Color[dataSize];
                textureCube.GetData((CubeMapFace) i, readData);

                Assert.AreEqual(savedData, readData);
            }

            textureCube.Dispose();
        }

        [Test]
        public void GetAndSetDataDxtCompressed()
        {
            var t = content.Load<TextureCube>(Paths.Texture ("SampleCube64DXT1Mips"));

            for (var f = 0; f < 6; f++)
            {
                var face = (CubeMapFace) f;
                var b = new byte[t.Size*t.Size/2];
                var b2 = new byte[t.Size*t.Size/2];

                t.GetData(face, b);
                t.SetData(face, b);
                t.GetData(face, b2);

                Assert.AreEqual(b, b2);

                // MonoGame allows any kind of type that is not larger than one element while XNA only allows byte
#if !XNA
                var b3 = new short[t.Size*t.Size/4];
                t.GetData(face, b3);
                t.SetData(face, b3);

                t.GetData(face, b2);
                Assert.AreEqual(b, b2);

                var b4 = new int[t.Size*t.Size/8];
                t.GetData(face, b4);
                t.SetData(face, b4);

                t.GetData(face, b2);
                Assert.AreEqual(b, b2);

                var b5 = new long[t.Size*t.Size/16];
                t.GetData(face, b5);
                t.SetData(face, b5);

                t.GetData(face, b2);
                Assert.AreEqual(b, b2);

                // this is too large, DXT1 blocks are 64 bits while Vector4 is 128 bits
                var b6 = new Vector4[t.Size*t.Size/32];
                Assert.Throws<ArgumentException>(() => t.GetData(face, b6));
                Assert.Throws<ArgumentException>(() => t.SetData(face, b6));

                var b7 = new Vector3[t.Size*t.Size/24];
                Assert.Throws<ArgumentException>(() => t.GetData(face, b7));
                Assert.Throws<ArgumentException>(() => t.SetData(face, b7));
#endif
            }

            t.Dispose();
        }

        // DXT1
        [TestCase(8, "SampleCube64DXT1Mips", 0)]
        [TestCase(8, "SampleCube64DXT1Mips", 1)]
        // TODO DXT5
        //[TestCase(16, "SampleCube64DXT5Mips", 0)]
        //[TestCase(16, "SampleCube64DXT5Mips", 1)]
        public void GetAndSetDataDxtNotMultipleOf4Rounding(int bs, string texName, int mip)
        {
            var t = content.Load<TextureCube>(Paths.Texture (texName));

            for (var f = 0; f < 6; f++)
            {
                var face = (CubeMapFace) f;
                var before = new byte[t.Size*t.Size*bs/16];
                t.GetData(face, before);

                var b1 = new byte[bs];
                var b2 = new byte[bs];

                t.GetData(face, mip, new Rectangle(0, 0, 4, 4), b1, 0, bs);

                t.GetData(face, mip, new Rectangle(0, 0, 1, 1), b2, 0, bs);
                t.SetData(face, mip, new Rectangle(0, 0, 1, 1), b2, 0, bs);
                Assert.AreEqual(b1, b2);

                t.GetData(face, mip, new Rectangle(0, 0, 1, 3), b2, 0, bs);
                t.SetData(face, mip, new Rectangle(0, 0, 1, 3), b2, 0, bs);
                Assert.AreEqual(b1, b2);

                t.GetData(face, mip, new Rectangle(0, 0, 4, 3), b2, 0, bs);
                t.SetData(face, mip, new Rectangle(0, 0, 4, 3), b2, 0, bs);
                Assert.AreEqual(b1, b2);

                t.GetData(face, mip, new Rectangle(0, 2, 4, 4), b2, 0, bs);
                t.SetData(face, mip, new Rectangle(0, 2, 4, 4), b2, 0, bs);
                Assert.AreEqual(b1, b2);

                t.GetData(face, mip, new Rectangle(2, 2, 4, 4), b2, 0, bs);
                t.SetData(face, mip, new Rectangle(2, 2, 4, 4), b2, 0, bs);
                Assert.AreEqual(b1, b2);

                t.GetData(face, mip, new Rectangle(3, 3, 4, 4), b2, 0, bs);
                t.SetData(face, mip, new Rectangle(3, 3, 4, 4), b2, 0, bs);
                Assert.AreEqual(b1, b2);

                t.GetData(face, mip, new Rectangle(4, 4, 4, 4), b2, 0, bs);
                t.SetData(face, mip, new Rectangle(4, 4, 4, 4), b2, 0, bs);
                Assert.AreNotEqual(b1, b2);

                var after = new byte[t.Size*t.Size*bs/16];
                t.GetData(face, after);

                Assert.AreEqual(before, after);
            }

            t.Dispose();
        }

        [TestCase("SampleCube64DXT1Mips", 8)]
        //[TestCase("SampleCube64DXT5Mips", 16)]
        public void GetAndSetDataDxtDontRoundWhenOutsideBounds(string texName, int bs)
        {
            var t = content.Load<TextureCube>(Paths.Texture(texName));

            var b = new byte[bs];

            for (var f = 0; f < 6; f++)
            {
                var face = (CubeMapFace) f;
                // don't round if the unrounded rectangle would be outside the texture area
                Assert.Throws<ArgumentException>(() => t.GetData(face, 0, new Rectangle(63, 63, 3, 3), b, 0, bs));
                // this does work
                t.GetData(face, 0, new Rectangle(63, 63, 1, 1), b, 0, bs);
            }

            t.Dispose();
        }

        [TestCase("SampleCube64DXT1Mips", 8)]
        //[TestCase("SampleCube64DXT5Mips", 16)]
        public void GetAndSetDataDxtLowerMips(string texName, int bs)
        {
            var t = content.Load<TextureCube>(Paths.Texture(texName));

            for (var f = 0; f < 6; f++)
            {
                var face = (CubeMapFace) f;
                var b = new byte[bs];
                var b2 = new byte[bs];

                t.GetData(face, 0, new Rectangle(0, 0, 4, 4), b, 0, bs);
                t.GetData(face, 1, new Rectangle(0, 0, 4, 4), b2, 0, bs);
                t.GetData(face, 2, new Rectangle(0, 0, 4, 4), b2, 0, bs);
                t.GetData(face, 3, new Rectangle(0, 0, 2, 2), b2, 0, bs);
                t.GetData(face, 4, new Rectangle(0, 0, 1, 1), b2, 0, bs);
                t.SetData(face, 3, new Rectangle(0, 0, 2, 2), b2, 0, bs);

                // would be rounded, but the rectangle is outside the texture area so it wil throw before rounding
                Assert.Throws<ArgumentException>(() => t.GetData(face, 5, new Rectangle(1, 1, 2, 2), b, 0, bs));
                Assert.Throws<ArgumentException>(() => t.GetData(face, 5, new Rectangle(0, 0, 3, 3), b, 0, bs));
            }

            t.Dispose();
        }

        [Test]
        public void NullDeviceShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
            {                
                var texture = new TextureCube(null, 16, false, SurfaceFormat.Color);
                texture.Dispose();
            });
            GC.GetTotalMemory(true); // collect uninitialized Texture
        }
    }
}
