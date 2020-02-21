using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class PackedVectorTest
    {
        [Test]
        public void Alpha8()
        {
            // Test the limits.
            Assert.AreEqual(0x0, new Alpha8(0f).PackedValue);
            Assert.AreEqual(0xFF, new Alpha8(1f).PackedValue);

            // Test clamping.
            Assert.AreEqual(0x0, new Alpha8(-1234f).PackedValue);
            Assert.AreEqual(0xFF, new Alpha8(1234f).PackedValue);

            //Test ordering
            Assert.AreEqual(124, new Alpha8(124f / 0xff).PackedValue);
            Assert.AreEqual(26, new Alpha8(0.1f).PackedValue);

			var packed = new Alpha8(0.5f).PackedValue;
            var unpacked = new Alpha8() { PackedValue = packed }.ToAlpha();
			Assert.AreEqual(0.5f, unpacked, 0.01f);

        }

        [Test]
        public void Bgra5551()
        {
            // Test the limits.
            Assert.AreEqual(0x0, new Bgra5551(Vector4.Zero).PackedValue);
            Assert.AreEqual(0xFFFF, new Bgra5551(Vector4.One).PackedValue);

            // Test ToVector4
            Assert.AreEqual(Vector4.Zero, new Bgra5551(Vector4.Zero).ToVector4());
            Assert.AreEqual(Vector4.One, new Bgra5551(Vector4.One).ToVector4());

            // Test clamping.
            Assert.AreEqual(Vector4.Zero, new Bgra5551(Vector4.One * -1234.0f).ToVector4());
            Assert.AreEqual(Vector4.One, new Bgra5551(Vector4.One * 1234.0f).ToVector4());

            //Test Ordering
            float x = 0x1a;
            float y = 0x16;
            float z = 0xd;
            float w = 0x1;
            Assert.AreEqual(0xeacd, new Bgra5551(x / 0x1f, y / 0x1f, z / 0x1f, w).PackedValue);
            x = 0.1f;
            y = -0.3f;
            z = 0.5f;
            w = -0.7f;
            Assert.AreEqual(3088, new Bgra5551(x, y, z, w).PackedValue);

            var packed = new Bgra5551(x, y, z, w).PackedValue;
            var unpacked = new Bgra5551() { PackedValue = packed }.ToVector4();
            Assert.AreEqual(x, unpacked.X, 0.1f);
            Assert.AreEqual(0f, unpacked.Y);
            Assert.AreEqual(z, unpacked.Z, 0.1f);
            Assert.AreEqual(0f, unpacked.W);
        }

        [Test]
        public void Rg32()
        {
            // Test the limits.
            Assert.AreEqual(0x0, new Rg32(Vector2.Zero).PackedValue);
            Assert.AreEqual(0xFFFFFFFF, new Rg32(Vector2.One).PackedValue);

            // Test ToVector2
            Assert.AreEqual(Vector2.Zero, new Rg32(Vector2.Zero).ToVector2());
            Assert.AreEqual(Vector2.One, new Rg32(Vector2.One).ToVector2());

            // Test clamping.
            Assert.AreEqual(Vector2.Zero, new Rg32(Vector2.One * -1234.0f).ToVector2());
            Assert.AreEqual(Vector2.One, new Rg32(Vector2.One * 1234.0f).ToVector2());

            //Test Ordering
            float x = 0xb6dc;
            float y = 0xA59f;
            Assert.AreEqual(0xa59fb6dc, new Rg32(x / 0xffff, y / 0xffff).PackedValue);
            x = 0.1f;
            y = -0.3f;
            Assert.AreEqual(6554, new Rg32(x, y).PackedValue);

			var packed = new Rg32(x, y).PackedValue;
			var unpacked = new Rg32() { PackedValue = packed }.ToVector2();
			Assert.AreEqual(x, unpacked.X, 0.01f);
			Assert.AreEqual(0f, unpacked.Y);

        }

        [Test]
        public void Rgba1010102()
        {
            // Test the limits.
            Assert.AreEqual(0x0, new Rgba1010102(Vector4.Zero).PackedValue);
            Assert.AreEqual(0xFFFFFFFF, new Rgba1010102(Vector4.One).PackedValue);

            // Test ToVector4
            Assert.AreEqual(Vector4.Zero, new Rgba1010102(Vector4.Zero).ToVector4());
            Assert.AreEqual(Vector4.One, new Rgba1010102(Vector4.One).ToVector4());

            // Test clamping.
            Assert.AreEqual(Vector4.Zero, new Rgba1010102(Vector4.One * -1234.0f).ToVector4());
            Assert.AreEqual(Vector4.One, new Rgba1010102(Vector4.One * 1234.0f).ToVector4());

            //Test Ordering
            float x = 0x2db;
            float y = 0x36d;
            float z = 0x3b7;
            float w = 0x1;
            Assert.AreEqual(0x7B7DB6DB, new Rgba1010102(x / 0x3ff, y / 0x3ff, z / 0x3ff, w / 3).PackedValue);
            x = 0.1f;
            y = -0.3f;
            z = 0.5f;
            w = -0.7f;
            Assert.AreEqual(536871014, new Rgba1010102(x, y, z, w).PackedValue);

			var packed = new Rgba1010102(x, y, z, w).PackedValue;
			var unpacked = new Rgba1010102() { PackedValue = packed }.ToVector4();
			Assert.AreEqual(x, unpacked.X, 0.01f);
			Assert.AreEqual(0f, unpacked.Y, 0.01f);
			Assert.AreEqual(z, unpacked.Z, 0.01f);
			Assert.AreEqual(0f, unpacked.W, 0.01f);
        }

        [Test]
        public void Rgba64()
        {
            // Test the limits.
            Assert.AreEqual(0x0, new Rgba64(Vector4.Zero).PackedValue);
            Assert.AreEqual(0xFFFFFFFFFFFFFFFF, new Rgba64(Vector4.One).PackedValue);

            // Test ToVector4
            Assert.AreEqual(Vector4.Zero, new Rgba64(Vector4.Zero).ToVector4());
            Assert.AreEqual(Vector4.One, new Rgba64(Vector4.One).ToVector4());

            // Test clamping.
            Assert.AreEqual(Vector4.Zero, new Rgba64(Vector4.One * -1234.0f).ToVector4());
            Assert.AreEqual(Vector4.One, new Rgba64(Vector4.One * 1234.0f).ToVector4());

            //Test data ordering
            Assert.AreEqual(0xC7AD8F5C570A1EB8, new Rgba64(((float) 0x1EB8) / 0xffff, ((float) 0x570A) / 0xffff, ((float) 0x8F5C) / 0xffff, ((float) 0xC7AD) / 0xffff).PackedValue);
            Assert.AreEqual(0xC7AD8F5C570A1EB8, new Rgba64(0.12f, 0.34f, 0.56f, 0.78f).PackedValue);
            Assert.AreEqual(0x73334CCC2666147B, new Rgba64(0.08f, 0.15f, 0.30f, 0.45f).PackedValue);

			float x = 0.1f;
			float y = -0.3f;
			float z = 0.5f;
			float w = -0.7f;
			var packed = new Rgba64(x, y, z, w).PackedValue;
			var unpacked = new Rgba64() { PackedValue = packed }.ToVector4();
			Assert.AreEqual(x, unpacked.X, 0.01f);
			Assert.AreEqual(0f, unpacked.Y, 0.01f);
			Assert.AreEqual(z, unpacked.Z, 0.01f);
			Assert.AreEqual(0f, unpacked.W, 0.01f);
        }

        [Test]
        public void NormalizedByte4()
        {
            Assert.AreEqual(0x0, new NormalizedByte4(Vector4.Zero).PackedValue);
            Assert.AreEqual(0x7F7F7F7F, new NormalizedByte4(Vector4.One).PackedValue);
            Assert.AreEqual(0x81818181, new NormalizedByte4(-Vector4.One).PackedValue);

            Assert.AreEqual(Vector4.One, new NormalizedByte4(Vector4.One).ToVector4());
            Assert.AreEqual(Vector4.Zero, new NormalizedByte4(Vector4.Zero).ToVector4());
            Assert.AreEqual(-Vector4.One, new NormalizedByte4(-Vector4.One).ToVector4());
            Assert.AreEqual(Vector4.One, new NormalizedByte4(Vector4.One * 1234.0f).ToVector4());
            Assert.AreEqual(-Vector4.One, new NormalizedByte4(Vector4.One * -1234.0f).ToVector4());
            //Test Ordering
            float x = 0.1f;
            float y = -0.3f;
            float z = 0.5f;
            float w = -0.7f;
            Assert.AreEqual(0xA740DA0D, new NormalizedByte4(x, y, z, w).PackedValue);
            Assert.AreEqual(958796544, new NormalizedByte4(0.0008f, 0.15f, 0.30f, 0.45f).PackedValue);

			var packed = new NormalizedByte4(x, y, z, w).PackedValue;
			var unpacked = new NormalizedByte4() { PackedValue = packed }.ToVector4();
			Assert.AreEqual(x, unpacked.X, 0.01f);
			Assert.AreEqual(y, unpacked.Y, 0.01f);
			Assert.AreEqual(z, unpacked.Z, 0.01f);
			Assert.AreEqual(w, unpacked.W, 0.01f);

        }

        [Test]
        public void NormalizedByte2()
        {
            Assert.AreEqual(0x0, new NormalizedByte2(Vector2.Zero).PackedValue);
            Assert.AreEqual(0x7F7F, new NormalizedByte2(Vector2.One).PackedValue);
            Assert.AreEqual(0x8181, new NormalizedByte2(-Vector2.One).PackedValue);

            Assert.AreEqual(Vector2.One, new NormalizedByte2(Vector2.One).ToVector2());
            Assert.AreEqual(Vector2.Zero, new NormalizedByte2(Vector2.Zero).ToVector2());
            Assert.AreEqual(-Vector2.One, new NormalizedByte2(-Vector2.One).ToVector2());
            Assert.AreEqual(Vector2.One, new NormalizedByte2(Vector2.One * 1234.0f).ToVector2());
            Assert.AreEqual(-Vector2.One, new NormalizedByte2(Vector2.One * -1234.0f).ToVector2());

            Assert.AreEqual(new Vector4(1, 1, 0, 1), ((IPackedVector) new NormalizedByte2(Vector2.One)).ToVector4());
            Assert.AreEqual(new Vector4(0, 0, 0, 1), ((IPackedVector) new NormalizedByte2(Vector2.Zero)).ToVector4());

            //Test Ordering
            float x = 0.1f;
            float y = -0.3f;
            Assert.AreEqual(0xda0d, new NormalizedByte2(x, y).PackedValue);

			var packed = new NormalizedByte2(x, y).PackedValue;
			var unpacked = new NormalizedByte2() { PackedValue = packed }.ToVector2();
			Assert.AreEqual(x, unpacked.X, 0.01f);
			Assert.AreEqual(y, unpacked.Y, 0.01f);
        }

        [Test]
        public void NormalizedShort4()
        {
            Assert.AreEqual(0x0, new NormalizedShort4(Vector4.Zero).PackedValue);
            Assert.AreEqual(0x7FFF7FFF7FFF7FFF, new NormalizedShort4(Vector4.One).PackedValue);
            Assert.AreEqual(0x8001800180018001, new NormalizedShort4(-Vector4.One).PackedValue);

            Assert.AreEqual(Vector4.One, new NormalizedShort4(Vector4.One).ToVector4());
            Assert.AreEqual(Vector4.Zero, new NormalizedShort4(Vector4.Zero).ToVector4());
            Assert.AreEqual(-Vector4.One, new NormalizedShort4(-Vector4.One).ToVector4());
            Assert.AreEqual(Vector4.One, new NormalizedShort4(Vector4.One * 1234.0f).ToVector4());
            Assert.AreEqual(-Vector4.One, new NormalizedShort4(Vector4.One * -1234.0f).ToVector4());

            //Test Ordering
            float x = 0.1f;
            float y = -0.3f;
            float z = 0.5f;
            float w = -0.7f;

            Assert.AreEqual(0xa6674000d99a0ccd, new NormalizedShort4(x, y, z, w).PackedValue);
            Assert.AreEqual(4150390751449251866, new NormalizedShort4(0.0008f, 0.15f, 0.30f, 0.45f).PackedValue);

			var packed = new NormalizedShort4(x, y, z, w).PackedValue;
			var unpacked = new NormalizedShort4() { PackedValue = packed }.ToVector4();
			Assert.AreEqual(x, unpacked.X, 0.01f);
			Assert.AreEqual(y, unpacked.Y, 0.01f);
			Assert.AreEqual(z, unpacked.Z, 0.01f);
			Assert.AreEqual(w, unpacked.W, 0.01f);

        }

        [Test]
        public void NormalizedShort2()
        {
            Assert.AreEqual(0x0, new NormalizedShort2(Vector2.Zero).PackedValue);
            Assert.AreEqual(0x7FFF7FFF, new NormalizedShort2(Vector2.One).PackedValue);
            Assert.AreEqual(0x80018001, new NormalizedShort2(-Vector2.One).PackedValue);

            Assert.AreEqual(Vector2.One, new NormalizedShort2(Vector2.One).ToVector2());
            Assert.AreEqual(Vector2.Zero, new NormalizedShort2(Vector2.Zero).ToVector2());
            Assert.AreEqual(-Vector2.One, new NormalizedShort2(-Vector2.One).ToVector2());
            Assert.AreEqual(Vector2.One, new NormalizedShort2(Vector2.One * 1234.0f).ToVector2());
            Assert.AreEqual(-Vector2.One, new NormalizedShort2(Vector2.One * -1234.0f).ToVector2());

            Assert.AreEqual(new Vector4(1, 1, 0, 1), ((IPackedVector) new NormalizedShort2(Vector2.One)).ToVector4());
            Assert.AreEqual(new Vector4(0, 0, 0, 1), ((IPackedVector) new NormalizedShort2(Vector2.Zero)).ToVector4());

            //Test Ordering
            float x = 0.35f;
            float y = -0.2f;
            Assert.AreEqual(0xE6672CCC, new NormalizedShort2(x, y).PackedValue);
            x = 0.1f;
            y = -0.3f;
            Assert.AreEqual(3650751693, new NormalizedShort2(x, y).PackedValue);

			var packed = new NormalizedShort2(x, y).PackedValue;
			var unpacked = new NormalizedShort2() { PackedValue = packed }.ToVector2();
			Assert.AreEqual(x, unpacked.X, 0.01f);
			Assert.AreEqual(y, unpacked.Y, 0.01f);
        }

        [Test]
        public void Short2()
        {
            // Test the limits.
            Assert.AreEqual(0x0, new Short2(Vector2.Zero).PackedValue);
            Assert.AreEqual(0x7FFF7FFF, new Short2(Vector2.One * 0x7FFF).PackedValue);
            Assert.AreEqual(0x80008000, new Short2(Vector2.One * -0x8000).PackedValue);

            // Test ToVector2.
            Assert.AreEqual(Vector2.One * 0x7FFF, new Short2(Vector2.One * 0x7FFF).ToVector2());
            Assert.AreEqual(Vector2.Zero, new Short2(Vector2.Zero).ToVector2());
            Assert.AreEqual(Vector2.One * -0x8000, new Short2(Vector2.One * -0x8000).ToVector2());
            Assert.AreEqual(Vector2.UnitX * 0x7FFF, new Short2(Vector2.UnitX * 0x7FFF).ToVector2());
            Assert.AreEqual(Vector2.UnitY * 0x7FFF, new Short2(Vector2.UnitY * 0x7FFF).ToVector2());

            // Test clamping.
            Assert.AreEqual(Vector2.One * 0x7FFF, new Short2(Vector2.One * 1234567.0f).ToVector2());
            Assert.AreEqual(Vector2.One * -0x8000, new Short2(Vector2.One * -1234567.0f).ToVector2());

            // Test ToVector4.
            Assert.AreEqual(new Vector4(0x7FFF, 0x7FFF, 0, 1), ((IPackedVector) new Short2(Vector2.One * 0x7FFF)).ToVector4());
            Assert.AreEqual(new Vector4(0, 0, 0, 1), ((IPackedVector) new Short2(Vector2.Zero)).ToVector4());
            Assert.AreEqual(new Vector4(-0x8000, -0x8000, 0, 1), ((IPackedVector) new Short2(Vector2.One * -0x8000)).ToVector4());

            //Test ordering
            float x = 0x2db1;
            float y = 0x361d;
            Assert.AreEqual(0x361d2db1, new Short2(x, y).PackedValue);
            x = 127.5f;
            y = -5.3f;
            Assert.AreEqual(4294639744, new Short2(x, y).PackedValue);

			var packed = new Short2(x, y).PackedValue;
			var unpacked = new Short2() { PackedValue = packed }.ToVector2();
			Assert.AreEqual(128f, unpacked.X);
			Assert.AreEqual(-5f, unpacked.Y);

        }

        [Test]
        public void Short4()
        {
            // Test the limits.
            Assert.AreEqual(0x0, new Short4(Vector4.Zero).PackedValue);
            Assert.AreEqual(0x7FFF7FFF7FFF7FFF, new Short4(Vector4.One * 0x7FFF).PackedValue);
            Assert.AreEqual(0x8000800080008000, new Short4(Vector4.One * -0x8000).PackedValue);

            // Test ToVector4.
            Assert.AreEqual(Vector4.One * 0x7FFF, new Short4(Vector4.One * 0x7FFF).ToVector4());
            Assert.AreEqual(Vector4.Zero, new Short4(Vector4.Zero).ToVector4());
            Assert.AreEqual(Vector4.One * -0x8000, new Short4(Vector4.One * -0x8000).ToVector4());
            Assert.AreEqual(Vector4.UnitX * 0x7FFF, new Short4(Vector4.UnitX * 0x7FFF).ToVector4());
            Assert.AreEqual(Vector4.UnitY * 0x7FFF, new Short4(Vector4.UnitY * 0x7FFF).ToVector4());
            Assert.AreEqual(Vector4.UnitZ * 0x7FFF, new Short4(Vector4.UnitZ * 0x7FFF).ToVector4());
            Assert.AreEqual(Vector4.UnitW * 0x7FFF, new Short4(Vector4.UnitW * 0x7FFF).ToVector4());

            // Test clamping.
            Assert.AreEqual(Vector4.One * 0x7FFF, new Short4(Vector4.One * 1234567.0f).ToVector4());
            Assert.AreEqual(Vector4.One * -0x8000, new Short4(Vector4.One * -1234567.0f).ToVector4());

            //Test Ordering
            float x = 0x2d1b;
            float y = 0x316d;
            float z = 0x73b7;
            float w = 0x00c1;
            Assert.AreEqual(0x00c173b7316d2d1b, new Short4(x, y, z, w).PackedValue);
            x = 0.1f;
            y = -0.3f;
            z = 0.5f;
            w = -0.7f;
            Assert.AreEqual(18446462598732840960, new Short4(x, y, z, w).PackedValue);

			var packed = new Short4(x, y, z, w).PackedValue;
			var unpacked = new Short4() { PackedValue = packed }.ToVector4();
			Assert.AreEqual(0f, unpacked.X);
			Assert.AreEqual(0f, unpacked.Y);
			Assert.AreEqual(0f, unpacked.Z);
			Assert.AreEqual(-1f, unpacked.W);

        }

        [Test]
        public void Bgra4444()
        {
            // Test the limits.
            Assert.AreEqual(0x0, new Bgra4444(Vector4.Zero).PackedValue);
            Assert.AreEqual(0xFFFF, new Bgra4444(Vector4.One).PackedValue);

            // Test ToVector4.
            Assert.AreEqual(Vector4.One, new Bgra4444(Vector4.One).ToVector4());
            Assert.AreEqual(Vector4.Zero, new Bgra4444(Vector4.Zero).ToVector4());
            Assert.AreEqual(Vector4.UnitX, new Bgra4444(Vector4.UnitX).ToVector4());
            Assert.AreEqual(Vector4.UnitY, new Bgra4444(Vector4.UnitY).ToVector4());
            Assert.AreEqual(Vector4.UnitZ, new Bgra4444(Vector4.UnitZ).ToVector4());
            Assert.AreEqual(Vector4.UnitW, new Bgra4444(Vector4.UnitW).ToVector4());

            // Test clamping.
            Assert.AreEqual(Vector4.Zero, new Bgra4444(Vector4.One * -1234.0f).ToVector4());
            Assert.AreEqual(Vector4.One, new Bgra4444(Vector4.One * 1234.0f).ToVector4());

            // Make sure the swizzle is correct.
            Assert.AreEqual(0x0F00, new Bgra4444(Vector4.UnitX).PackedValue);
            Assert.AreEqual(0x00F0, new Bgra4444(Vector4.UnitY).PackedValue);
            Assert.AreEqual(0x000F, new Bgra4444(Vector4.UnitZ).PackedValue);
            Assert.AreEqual(0xF000, new Bgra4444(Vector4.UnitW).PackedValue);

            float x = 0.1f;
            float y = -0.3f;
            float z = 0.5f;
            float w = -0.7f;
            Assert.AreEqual(520, new Bgra4444(x, y, z, w).PackedValue);

			var packed = new Bgra4444(x, y, z, w).PackedValue;
			var unpacked = new Bgra4444() { PackedValue = packed }.ToVector4();
			Assert.AreEqual(0.13f, unpacked.X, 0.01f);
			Assert.AreEqual(0f, unpacked.Y);
			Assert.AreEqual(0.53f, unpacked.Z, 0.01f);
			Assert.AreEqual(0f, unpacked.W);
        }

        [Test]
        public void Bgr565()
        {
            // Test the limits.
            Assert.AreEqual(0x0, new Bgr565(Vector3.Zero).PackedValue);
            Assert.AreEqual(0xFFFF, new Bgr565(Vector3.One).PackedValue);

            // Test ToVector3.
            Assert.AreEqual(Vector3.One, new Bgr565(Vector3.One).ToVector3());
            Assert.AreEqual(Vector3.Zero, new Bgr565(Vector3.Zero).ToVector3());
            Assert.AreEqual(Vector3.UnitX, new Bgr565(Vector3.UnitX).ToVector3());
            Assert.AreEqual(Vector3.UnitY, new Bgr565(Vector3.UnitY).ToVector3());
            Assert.AreEqual(Vector3.UnitZ, new Bgr565(Vector3.UnitZ).ToVector3());

            // Test clamping.
            Assert.AreEqual(Vector3.Zero, new Bgr565(Vector3.One * -1234.0f).ToVector3());
            Assert.AreEqual(Vector3.One, new Bgr565(Vector3.One * 1234.0f).ToVector3());

            // Make sure the swizzle is correct.
            Assert.AreEqual(0xF800, new Bgr565(Vector3.UnitX).PackedValue);
            Assert.AreEqual(0x07E0, new Bgr565(Vector3.UnitY).PackedValue);
            Assert.AreEqual(0x001F, new Bgr565(Vector3.UnitZ).PackedValue);

            float x = 0.1f;
            float y = -0.3f;
            float z = 0.5f;
            Assert.AreEqual(6160, new Bgr565(x, y, z).PackedValue);

			var packed = new Bgr565(x, y, z).PackedValue;
			var unpacked = new Bgr565() { PackedValue = packed }.ToVector3();
			Assert.AreEqual(x, unpacked.X, 0.1f);
			Assert.AreEqual(0f, unpacked.Y);
			Assert.AreEqual(z, unpacked.Z, 0.1f);
        }

        [Test]
        public void Byte4()
        {
            // Test the limits.
            Assert.AreEqual(0x0, new Byte4(Vector4.Zero).PackedValue);
            Assert.AreEqual(0xFFFFFFFF, new Byte4(Vector4.One * 255).PackedValue);

            // Test ToVector4.
            Assert.AreEqual(Vector4.One * 255, new Byte4(Vector4.One * 255).ToVector4());
            Assert.AreEqual(Vector4.Zero, new Byte4(Vector4.Zero).ToVector4());
            Assert.AreEqual(Vector4.UnitX * 255, new Byte4(Vector4.UnitX * 255).ToVector4());
            Assert.AreEqual(Vector4.UnitY * 255, new Byte4(Vector4.UnitY * 255).ToVector4());
            Assert.AreEqual(Vector4.UnitZ * 255, new Byte4(Vector4.UnitZ * 255).ToVector4());
            Assert.AreEqual(Vector4.UnitW * 255, new Byte4(Vector4.UnitW * 255).ToVector4());

            // Test clamping.
            Assert.AreEqual(Vector4.Zero, new Byte4(Vector4.One * -1234.0f).ToVector4());
            Assert.AreEqual(Vector4.One * 255, new Byte4(Vector4.One * 1234.0f).ToVector4());

            //Test ordering
            float x = 0x2d;
            float y = 0x36;
            float z = 0x7b;
            float w = 0x1a;
            Assert.AreEqual(0x1a7b362d, new Byte4(x, y, z, w).PackedValue);

            x = 127.5f;
            y = -12.3f;
            z = 0.5f;
            w = -0.7f;
            Assert.AreEqual(128, new Byte4(x, y, z, w).PackedValue);

			var packed = new Byte4(x, y, z, w).PackedValue;
			var unpacked = new Byte4() { PackedValue = packed }.ToVector4();
			Assert.AreEqual(128f, unpacked.X);
			Assert.AreEqual(0f, unpacked.Y);
			Assert.AreEqual(0f, unpacked.Z);
			Assert.AreEqual(0f, unpacked.W);
        }

        [Test]
        public void HalfSingle()
        {
            //Test limits
            Assert.AreEqual(15360, new HalfSingle(1f).PackedValue);
            Assert.AreEqual(0, new HalfSingle(0f).PackedValue);
            Assert.AreEqual(48128, new HalfSingle(-1f).PackedValue);

            //Test values
            Assert.AreEqual(11878, new HalfSingle(0.1f).PackedValue);
            Assert.AreEqual(46285, new HalfSingle(-0.3f).PackedValue);

			var packed = new HalfSingle(0.5f).PackedValue;
            var unpacked = new HalfSingle() { PackedValue = packed }.ToSingle();
			Assert.AreEqual(0.5f, unpacked, 0.01f);
        }

        [Test]
        public void HalfVector2()
        {
            //Test PackedValue
            Assert.AreEqual(0u, new HalfVector2(Vector2.Zero).PackedValue);
            Assert.AreEqual(1006648320u, new HalfVector2(Vector2.One).PackedValue);
            Assert.AreEqual(3033345638u, new HalfVector2(0.1f, -0.3f).PackedValue);

            //Test ToVector2
            Assert.AreEqual(Vector2.Zero, new HalfVector2(Vector2.Zero).ToVector2());
            Assert.AreEqual(Vector2.One, new HalfVector2(Vector2.One).ToVector2());
            Assert.AreEqual(Vector2.UnitX, new HalfVector2(Vector2.UnitX).ToVector2());
            Assert.AreEqual(Vector2.UnitY, new HalfVector2(Vector2.UnitY).ToVector2());

			var x = 0.1f;
			var y = -0.3f;
			var packed = new HalfVector2(x, y).PackedValue;
			var unpacked = new HalfVector2() { PackedValue = packed }.ToVector2();
			Assert.AreEqual(x, unpacked.X, 0.01f);
			Assert.AreEqual(y, unpacked.Y, 0.01f);
        }

        [Test]
        public void HalfVector4()
        {
            //Test PackedValue
            Assert.AreEqual(0uL, new HalfVector4(Vector4.Zero).PackedValue);
            Assert.AreEqual(4323521613979991040uL, new HalfVector4(Vector4.One).PackedValue);
            Assert.AreEqual(13547034390470638592uL, new HalfVector4(-Vector4.One).PackedValue);
            Assert.AreEqual(15360uL, new HalfVector4(Vector4.UnitX).PackedValue);
            Assert.AreEqual(1006632960uL, new HalfVector4(Vector4.UnitY).PackedValue);
            Assert.AreEqual(65970697666560uL, new HalfVector4(Vector4.UnitZ).PackedValue);
            Assert.AreEqual(4323455642275676160uL, new HalfVector4(Vector4.UnitW).PackedValue);
            Assert.AreEqual(4035285078724390502uL, new HalfVector4(0.1f, 0.3f, 0.4f, 0.5f).PackedValue);

            //Test ToVector4
            Assert.AreEqual(Vector4.Zero, new HalfVector4(Vector4.Zero).ToVector4());
            Assert.AreEqual(Vector4.One, new HalfVector4(Vector4.One).ToVector4());
            Assert.AreEqual(-Vector4.One, new HalfVector4(-Vector4.One).ToVector4());
            Assert.AreEqual(Vector4.UnitX, new HalfVector4(Vector4.UnitX).ToVector4());
            Assert.AreEqual(Vector4.UnitY, new HalfVector4(Vector4.UnitY).ToVector4());
            Assert.AreEqual(Vector4.UnitZ, new HalfVector4(Vector4.UnitZ).ToVector4());
            Assert.AreEqual(Vector4.UnitW, new HalfVector4(Vector4.UnitW).ToVector4());

			var x = 0.1f;
			var y = -0.3f;
			var z = 0.5f;
			var w = -0.7f;
			var packed = new HalfVector4(x, y, z, w).PackedValue;
			var unpacked = new HalfVector4() { PackedValue = packed }.ToVector4();
			Assert.AreEqual(x, unpacked.X, 0.01f);
			Assert.AreEqual(y, unpacked.Y, 0.01f);
			Assert.AreEqual(z, unpacked.Z, 0.01f);
			Assert.AreEqual(w, unpacked.W, 0.01f);
        }
    }
}
