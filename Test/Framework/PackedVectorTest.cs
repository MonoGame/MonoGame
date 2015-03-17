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

            Assert.AreEqual(new Vector4(1,1,0,1), ((IPackedVector)new NormalizedByte2(Vector2.One)).ToVector4());
            Assert.AreEqual(new Vector4(0,0,0,1), ((IPackedVector)new NormalizedByte2(Vector2.Zero)).ToVector4());
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

            Assert.AreEqual(new Vector4(1, 1, 0, 1), ((IPackedVector)new NormalizedShort2(Vector2.One)).ToVector4());
            Assert.AreEqual(new Vector4(0, 0, 0, 1), ((IPackedVector)new NormalizedShort2(Vector2.Zero)).ToVector4());
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
            Assert.AreEqual(new Vector4(0x7FFF, 0x7FFF, 0, 1), ((IPackedVector)new Short2(Vector2.One * 0x7FFF)).ToVector4());
            Assert.AreEqual(new Vector4(0, 0, 0, 1), ((IPackedVector)new Short2(Vector2.Zero)).ToVector4());
            Assert.AreEqual(new Vector4(-0x8000, -0x8000, 0, 1), ((IPackedVector)new Short2(Vector2.One * -0x8000)).ToVector4());
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
        }
    }
}
