﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    internal class GraphicsDeviceTest : VisualTestFixtureBase
    {
        [Test]
        public void DrawPrimitivesParameterValidation()
        {
            Game.DrawWith += (sender, e) =>
            {
                var vertexBuffer = new VertexBuffer(
                    Game.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration,
                    3, BufferUsage.None);

                // No vertex shader or pixel shader.
                Assert.Throws<InvalidOperationException>(() => Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1));

                new BasicEffect(Game.GraphicsDevice).CurrentTechnique.Passes[0].Apply();

                // No vertexBuffer.
                Assert.Throws<InvalidOperationException>(() => Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1));

                Game.GraphicsDevice.SetVertexBuffer(vertexBuffer);

                // Success - "normal" usage.
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1));

                // vertexStart too small / large.
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, -1, 1));
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 3, 1));

                // primitiveCount too small / large.
                Assert.Throws<ArgumentOutOfRangeException>(() => Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 0));
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2));

                // vertexStart + primitiveCount too large.
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 1, 1));
            };
            Game.Run();
        }

        [Test]
        public void DrawIndexedPrimitivesParameterValidation()
        {
            Game.DrawWith += (sender, e) =>
            {
                var vertexBuffer = new VertexBuffer(
                    Game.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration,
                    3, BufferUsage.None);
                var indexBuffer = new IndexBuffer(
                    Game.GraphicsDevice, IndexElementSize.SixteenBits, 
                    3, BufferUsage.None);

                // No vertex shader or pixel shader.
                Assert.Throws<InvalidOperationException>(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1));

                new BasicEffect(Game.GraphicsDevice).CurrentTechnique.Passes[0].Apply();

                // No vertexBuffer.
                Assert.Throws<InvalidOperationException>(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1));

                Game.GraphicsDevice.SetVertexBuffer(vertexBuffer);

                // No indexBuffer.
                Assert.Throws<InvalidOperationException>(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1));

                Game.GraphicsDevice.Indices = indexBuffer;

                // Success - "normal" usage.
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1));

                // XNA doesn't do upfront parameter validation on the Assert.DoesNotThrow tests,
                // but it *sometimes* fails later with an AccessViolationException, so we can't actually
                // run these tests as part of the XNA test suite.

                // baseVertex too small / large.
#if !XNA
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, -1, 0, 3, 0, 1));
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 3, 0, 3, 0, 1));
#endif

                // startIndex too small / large.
#if !XNA
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, -1, 1));
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 3, 1));
#endif

                // primitiveCount too small / large.
                Assert.Throws<ArgumentOutOfRangeException>(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 0));
#if !XNA
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 2));
#endif

                // startIndex + primitiveCount too large.
#if !XNA
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 1, 1));
#endif
            };
            Game.Run();
        }

        [Test]
        public void DrawUserPrimitivesParameterValidation()
        {
            Game.DrawWith += (sender, e) =>
            {
                var vertexDataNonEmpty = new[]
                {
                    new VertexPositionColorTexture(Vector3.Zero, Color.White, Vector2.Zero),
                    new VertexPositionColorTexture(Vector3.Zero, Color.White, Vector2.Zero),
                    new VertexPositionColorTexture(Vector3.Zero, Color.White, Vector2.Zero)
                };
                var vertexDataEmpty = new VertexPositionColorTexture[0];

                // No vertex shader or pixel shader.
                Assert.Throws<InvalidOperationException>(() => Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 1));

                new BasicEffect(Game.GraphicsDevice).CurrentTechnique.Passes[0].Apply();

                // Success - "normal" usage.
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 1));

                // Null vertexData.
                DoDrawUserPrimitivesAsserts(null, 0, 1, d => Assert.Throws<ArgumentNullException>(d));

                // Empty vertexData.
                DoDrawUserPrimitivesAsserts(vertexDataEmpty, 0, 1, d => Assert.Throws<ArgumentOutOfRangeException>(d));

                // vertexOffset too small / large.
                DoDrawUserPrimitivesAsserts(vertexDataNonEmpty, -1, 1, d => Assert.Throws<ArgumentOutOfRangeException>(d));
                DoDrawUserPrimitivesAsserts(vertexDataNonEmpty, 3, 1, d => Assert.Throws<ArgumentOutOfRangeException>(d));

                // primitiveCount too small / large.
                DoDrawUserPrimitivesAsserts(vertexDataNonEmpty, 0, 0, d => Assert.Throws<ArgumentOutOfRangeException>(d));
                DoDrawUserPrimitivesAsserts(vertexDataNonEmpty, 0, 2, d => Assert.Throws<ArgumentOutOfRangeException>(d));

                // vertexOffset + primitiveCount too large.
                DoDrawUserPrimitivesAsserts(vertexDataNonEmpty, 1, 1, d => Assert.Throws<ArgumentOutOfRangeException>(d));

                // Null vertexDeclaration.
                Assert.Throws<ArgumentNullException>(() => Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 1, null));
            };
            Game.Run();
        }

        private void DoDrawUserPrimitivesAsserts(VertexPositionColorTexture[] vertexData, int vertexOffset, int primitiveCount, Action<TestDelegate> assertMethod)
        {
            assertMethod(() => Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexData, vertexOffset, primitiveCount));
            assertMethod(() => Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexData, vertexOffset, primitiveCount, VertexPositionColorTexture.VertexDeclaration));
        }

        [Test]
        public void DrawUserIndexedPrimitivesParameterValidation()
        {
            Game.DrawWith += (sender, e) =>
            {
                var vertexDataNonEmpty = new[]
                {
                    new VertexPositionColorTexture(Vector3.Zero, Color.White, Vector2.Zero),
                    new VertexPositionColorTexture(Vector3.Zero, Color.White, Vector2.Zero),
                    new VertexPositionColorTexture(Vector3.Zero, Color.White, Vector2.Zero)
                };
                var vertexDataEmpty = new VertexPositionColorTexture[0];

                var indexDataNonEmpty = new short[] { 0, 1, 2 };
                var indexDataEmpty = new short[0];

                // No vertex shader or pixel shader.
                Assert.Throws<InvalidOperationException>(() => Game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 0, 1));

                new BasicEffect(Game.GraphicsDevice).CurrentTechnique.Passes[0].Apply();

                // Success - "normal" usage.
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 0, 1));

                // Failure cases.

                // Null vertexData.
                DoDrawUserIndexedPrimitivesAsserts(null, 0, 3, indexDataNonEmpty, 0, 1, d => Assert.Throws<ArgumentNullException>(d));

                // Empty vertexData.
                DoDrawUserIndexedPrimitivesAsserts(vertexDataEmpty, 0, 3, indexDataNonEmpty, 0, 1, d => Assert.Throws<ArgumentNullException>(d));

                // vertexOffset too small / large.
                DoDrawUserIndexedPrimitivesAsserts(vertexDataNonEmpty, -1, 3, indexDataNonEmpty, 0, 1, d => Assert.Throws<ArgumentOutOfRangeException>(d));
                DoDrawUserIndexedPrimitivesAsserts(vertexDataNonEmpty, 3, 3, indexDataNonEmpty, 0, 1, d => Assert.Throws<ArgumentOutOfRangeException>(d));

                // numVertices too small / large.
                DoDrawUserIndexedPrimitivesAsserts(vertexDataNonEmpty, 0, 0, indexDataNonEmpty, 0, 1, d => Assert.Throws<ArgumentOutOfRangeException>(d));
                DoDrawUserIndexedPrimitivesAsserts(vertexDataNonEmpty, 0, 4, indexDataNonEmpty, 0, 1, d => Assert.Throws<ArgumentOutOfRangeException>(d));

                // vertexOffset + numVertices too large.
                DoDrawUserIndexedPrimitivesAsserts(vertexDataNonEmpty, 1, 3, indexDataNonEmpty, 0, 1, d => Assert.Throws<ArgumentOutOfRangeException>(d));

                // Null indexData.
                DoDrawUserIndexedPrimitivesAsserts(vertexDataNonEmpty, 0, 3, null, 0, 1, d => Assert.Throws<ArgumentNullException>(d));

                // Empty indexData.
                DoDrawUserIndexedPrimitivesAsserts(vertexDataNonEmpty, 0, 3, indexDataEmpty, 0, 1, d => Assert.Throws<ArgumentNullException>(d));

                // indexOffset too small / large.
                DoDrawUserIndexedPrimitivesAsserts(vertexDataNonEmpty, 0, 3, indexDataNonEmpty, -1, 1, d => Assert.Throws<ArgumentOutOfRangeException>(d));
                DoDrawUserIndexedPrimitivesAsserts(vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 1, 1, d => Assert.Throws<ArgumentOutOfRangeException>(d));

                // primitiveCount too small / large.
                DoDrawUserIndexedPrimitivesAsserts(vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 0, -1, d => Assert.Throws<ArgumentOutOfRangeException>(d));
                DoDrawUserIndexedPrimitivesAsserts(vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 0, 0, d => Assert.Throws<ArgumentOutOfRangeException>(d));
                DoDrawUserIndexedPrimitivesAsserts(vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 0, 2, d => Assert.Throws<ArgumentOutOfRangeException>(d));

                // indexOffset + primitiveCount too large.
                DoDrawUserIndexedPrimitivesAsserts(vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 1, 1, d => Assert.Throws<ArgumentOutOfRangeException>(d));

                // Null vertexDeclaration.
                Assert.Throws<ArgumentNullException>(() => Game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 0, 1, null));

                // Smaller vertex stride in VertexDeclaration than in actual vertices.

                // XNA is inconsistent; in DrawUserIndexedPrimitives, it allows vertexStride to be less than the actual size of the data,
                // but in VertexBuffer.SetData, XNA requires vertexStride to greater than or equal to the actual size of the data.
                // Since we use a DynamicVertexBuffer to implement DrawUserIndexedPrimitives, we use the same validation in both places.
                // The same applies to DrawUserPrimitives.
#if XNA
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 0, 1, VertexPositionColor.VertexDeclaration));
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty.Select(x => (int) x).ToArray(), 0, 1, VertexPositionColor.VertexDeclaration));
#else
                Assert.Throws<ArgumentOutOfRangeException>(() => Game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 0, 1, VertexPositionColor.VertexDeclaration));
                Assert.Throws<ArgumentOutOfRangeException>(() => Game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty.Select(x => (int) x).ToArray(), 0, 1, VertexPositionColor.VertexDeclaration));
#endif
            };
            Game.Run();
        }

        private void DoDrawUserIndexedPrimitivesAsserts(VertexPositionColorTexture[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, Action<TestDelegate> assertMethod)
        {
            assertMethod(() => Game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount));
            assertMethod(() => Game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, VertexPositionColorTexture.VertexDeclaration));

            var intIndexData = (indexData == null) ? null : indexData.Select(x => (int) x).ToArray();
            assertMethod(() => Game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, vertexOffset, numVertices, intIndexData, indexOffset, primitiveCount));
            assertMethod(() => Game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, vertexOffset, numVertices, intIndexData, indexOffset, primitiveCount, VertexPositionColorTexture.VertexDeclaration));
        }
    }
}