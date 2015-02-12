using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class EnumConformingTest
    {
        [Test]
        void PresentIntervalEnum()
        {
            Assert.AreEqual(0, (int)(PresentInterval.Default));
            Assert.AreEqual(1, (int)(PresentInterval.One));
            Assert.AreEqual(2, (int)(PresentInterval.Two));
            Assert.AreEqual(3, (int)(PresentInterval.Immediate));
        }

        [Test]
        public void PrimitiveTypeEnum()
        {
            Assert.AreEqual(0, (int)(PrimitiveType.TriangleList));
            Assert.AreEqual(1, (int)(PrimitiveType.TriangleStrip));
            Assert.AreEqual(2, (int)(PrimitiveType.LineList));
            Assert.AreEqual(3, (int)(PrimitiveType.LineStrip));
        }

        [Test]
        public void RenderTargetUsageEnum()
        {
            Assert.AreEqual(0, (int)(RenderTargetUsage.DiscardContents));
            Assert.AreEqual(1, (int)(RenderTargetUsage.PreserveContents));
            Assert.AreEqual(2, (int)(RenderTargetUsage.PlatformContents));
        }

        [Test]
        public void SetDataOptionsEnum()
        {
            Assert.AreEqual(0, (int)(SetDataOptions.None));
            Assert.AreEqual(1, (int)(SetDataOptions.Discard));
            Assert.AreEqual(2, (int)(SetDataOptions.NoOverwrite));
        }

        [Test]
        public void SpriteSortModeEnum()
        {
            Assert.AreEqual(0, (int)(SpriteSortMode.Deferred));
            Assert.AreEqual(1, (int)(SpriteSortMode.Immediate));
            Assert.AreEqual(2, (int)(SpriteSortMode.Texture));
            Assert.AreEqual(3, (int)(SpriteSortMode.BackToFront));
            Assert.AreEqual(4, (int)(SpriteSortMode.FrontToBack));
        }

        [Test]
        public void StencilOperationEnum()
        {
            Assert.AreEqual(0, (int)(StencilOperation.Keep));
            Assert.AreEqual(1, (int)(StencilOperation.Zero));
            Assert.AreEqual(2, (int)(StencilOperation.Replace));
            Assert.AreEqual(3, (int)(StencilOperation.Increment));
            Assert.AreEqual(4, (int)(StencilOperation.Decrement));
            Assert.AreEqual(5, (int)(StencilOperation.IncrementSaturation));
            Assert.AreEqual(6, (int)(StencilOperation.DecrementSaturation));
            Assert.AreEqual(7, (int)(StencilOperation.Invert));
        }

        [Test]
        public void VertexElementUsageEnum()
        {
                Assert.AreEqual(0, (int)(VertexElementUsage.Position));
                Assert.AreEqual(1, (int)(VertexElementUsage.Color));
                Assert.AreEqual(2, (int)(VertexElementUsage.TextureCoordinate));
                Assert.AreEqual(3, (int)(VertexElementUsage.Normal));
                Assert.AreEqual(4, (int)(VertexElementUsage.Binormal));
                Assert.AreEqual(5, (int)(VertexElementUsage.Tangent));
                Assert.AreEqual(6, (int)(VertexElementUsage.BlendIndices));
                Assert.AreEqual(7, (int)(VertexElementUsage.BlendWeight));
                Assert.AreEqual(8, (int)(VertexElementUsage.Depth));
                Assert.AreEqual(9, (int)(VertexElementUsage.Fog));
                Assert.AreEqual(10, (int)(VertexElementUsage.PointSize));
                Assert.AreEqual(11, (int)(VertexElementUsage.Sample));
                Assert.AreEqual(12, (int)(VertexElementUsage.TessellateFactor));
        }
    }
}
