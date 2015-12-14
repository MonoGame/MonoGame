// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tests.ContentPipeline;
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

        // This overload of DrawIndexedPrimitives is not supported on XNA.
#if !XNA
        [Test]
        public void DrawIndexedPrimitivesParameterValidation2()
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
                Assert.Throws<InvalidOperationException>(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 1));

                new BasicEffect(Game.GraphicsDevice).CurrentTechnique.Passes[0].Apply();

                // No vertexBuffer.
                Assert.Throws<InvalidOperationException>(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 1));

                Game.GraphicsDevice.SetVertexBuffer(vertexBuffer);

                // No indexBuffer.
                Assert.Throws<InvalidOperationException>(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 1));

                Game.GraphicsDevice.Indices = indexBuffer;

                // Success - "normal" usage.
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 1));

                // baseVertex too small / large.
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, -1, 0, 1));
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 3, 0, 1));

                // startIndex too small / large.
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, -1, 1));
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 3, 1));

                // primitiveCount too small / large.
                Assert.Throws<ArgumentOutOfRangeException>(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 0));
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2));

                // startIndex + primitiveCount too large.
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 1, 1));
            };
            Game.Run();
        }
#endif

#if XNA || DIRECTX
        [Test]
        public void DrawInstancedPrimitivesParameterValidation()
        {
            Game.DrawWith += (sender, e) =>
            {
                var vertexBuffer = new VertexBuffer(
                    Game.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration,
                    3, BufferUsage.None);

                VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
                (
                    new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
                    new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
                    new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
                    new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
                );
                var instanceBuffer = new VertexBuffer(
                    Game.GraphicsDevice, instanceVertexDeclaration,
                    10, BufferUsage.None);

                var indexBuffer = new IndexBuffer(Game.GraphicsDevice, IndexElementSize.SixteenBits, 3, BufferUsage.None);

                // No vertex shader or pixel shader.
                Assert.Throws<InvalidOperationException>(() => Game.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1, 10));

                var effect = AssetTestUtility.CompileEffect(Game.GraphicsDevice, "Instancing.fx");
                effect.Techniques[0].Passes[0].Apply();

                // No vertexBuffers.
                Assert.Throws<InvalidOperationException>(() => Game.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1, 10));

                Game.GraphicsDevice.SetVertexBuffers(
                    new VertexBufferBinding(vertexBuffer, 0, 0),
                    new VertexBufferBinding(instanceBuffer, 0, 1));

                // No indexBuffer.
                Assert.Throws<InvalidOperationException>(() => Game.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1, 10));

                Game.GraphicsDevice.Indices = indexBuffer;

                // Success - "normal" usage.
                Assert.DoesNotThrow(() => Game.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1, 10));

                // primitiveCount too small / large.
                Assert.Throws<ArgumentOutOfRangeException>(() => Game.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 0, 10));
            };
            Game.Run();
        }

        [Test]
        public void DrawInstancedPrimitivesVisualTest()
        {
            VertexBuffer vertexBuffer = null;
            IndexBuffer indexBuffer = null;
            VertexBuffer instanceVertexBuffer = null;
            Matrix[] worldTransforms = null;
            EffectPass pass = null;

            Game.LoadContentWith += (sender, e) =>
            {
                // Create vertex and index buffer for a quad.
                var vertices = new[]
                {
                    new VertexPositionTexture(new Vector3(-1,  1, 0), new Vector2(0, 0)),
                    new VertexPositionTexture(new Vector3( 1,  1, 0), new Vector2(1, 0)),
                    new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
                    new VertexPositionTexture(new Vector3( 1, -1, 0), new Vector2(1, 1)),
                };
                vertexBuffer = new VertexBuffer(
                    Game.GraphicsDevice, VertexPositionTexture.VertexDeclaration,
                    4, BufferUsage.None);
                vertexBuffer.SetData(vertices);

                var indices = new ushort[] { 0, 1, 2, 1, 3, 2 };
                indexBuffer = new IndexBuffer(Game.GraphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.None);
                indexBuffer.SetData(indices);

                // Create vertex buffer with instance data.
                worldTransforms = new Matrix[8 * 4];
                for (int i = 0; i < worldTransforms.Length; i++)
                {
                    worldTransforms[i] = Matrix.CreateScale(0.4f) *
                        Matrix.CreateRotationZ(0.05f * i) *
                        Matrix.CreateTranslation(-3.5f + (i % 8), -1.5f + (int)(i / 8), 0);
                }
                VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
                (
                    new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
                    new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
                    new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
                    new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
                );
                instanceVertexBuffer = new VertexBuffer(Game.GraphicsDevice, instanceVertexDeclaration, worldTransforms.Length, BufferUsage.None);
                instanceVertexBuffer.SetData(worldTransforms);

                var view = Matrix.CreateLookAt(new Vector3(0, 0, 6), new Vector3(0, 0, 0), Vector3.Up);
                var projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4, Game.GraphicsDevice.Viewport.AspectRatio, 0.1f, 100);

                var effect = AssetTestUtility.CompileEffect(Game.GraphicsDevice, "Instancing.fx");
                effect.Parameters["View"].SetValue(view);
                effect.Parameters["Projection"].SetValue(projection);
                pass = effect.Techniques[0].Passes[0];
            };

            Game.DrawWith += (sender, e) =>
            {
                Game.GraphicsDevice.Clear(Color.CornflowerBlue);

                Game.GraphicsDevice.SetVertexBuffers(
                    new VertexBufferBinding(vertexBuffer, 0, 0),
                    new VertexBufferBinding(instanceVertexBuffer, 0, 1));

                Game.GraphicsDevice.Indices = indexBuffer;

                pass.Apply();

                Game.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 6, 0, 2, worldTransforms.Length);
            };

            //Game.Run(until: frameInfo => false);

            // There is a minor difference in the rasterization between XNA and DirectX. 
            var similarity = 0.98f;
            RunSingleFrameTest(similarity);
        }
#endif

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

        [Test]
        public void VertexTexturesGetSet()
        {
            // TODO: The availability of vertex textures should depend on GraphicsProfile.

            Game.DrawWith += (sender, e) =>
            {
#if XNA
                var supportedVertexTextureFormats = new[]
                {
                    SurfaceFormat.Single, 
                    SurfaceFormat.Vector2, 
                    SurfaceFormat.Vector4,
                    SurfaceFormat.HalfSingle, 
                    SurfaceFormat.HalfVector2, 
                    SurfaceFormat.HalfVector4,
                    SurfaceFormat.HdrBlendable
                };
#else
                var supportedVertexTextureFormats = Enum.GetValues(typeof(SurfaceFormat)).Cast<SurfaceFormat>().ToArray();
#endif
                foreach (var format in Enum.GetValues(typeof(SurfaceFormat)).Cast<SurfaceFormat>())
                {
                    var texture = new Texture2D(Game.GraphicsDevice, 4, 4, false, format);

                    if (supportedVertexTextureFormats.Contains(format))
                    {
                        Game.GraphicsDevice.VertexTextures[0] = texture;
                        var retrievedTexture = Game.GraphicsDevice.VertexTextures[0];
                        Assert.That(retrievedTexture, Is.SameAs(texture));
                    }
                    else
                    {
                        Assert.Throws<NotSupportedException>(() =>
                            Game.GraphicsDevice.VertexTextures[0] = texture);
                    }
                }
            };
            Game.Run();
        }

        private struct VertexPosition2 : IVertexType
        {
            public Vector2 Position;

            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));

            VertexDeclaration IVertexType.VertexDeclaration
            {
                get { return VertexDeclaration; }
            }
        }

        [Test]
        public void VertexTextureVisualTest()
        {
            // Implements an extremely simple terrain that reads from a heightmap in the vertex shader.

            Game.DrawWith += (sender, e) =>
            {
                const int heightMapSize = 64;
                var heightMapTexture = new Texture2D(Game.GraphicsDevice, heightMapSize, heightMapSize, false, SurfaceFormat.Single);
                var heightMapData = new float[heightMapSize * heightMapSize];
                for (var y = 0; y < heightMapSize; y++)
                    for (var x = 0; x < heightMapSize; x++)
                        heightMapData[(y * heightMapSize) + x] = (float) Math.Sin(x / 2.0f) + (float) Math.Sin(y / 3.0f);
                heightMapTexture.SetData(heightMapData);

                var viewMatrix = Matrix.CreateLookAt(new Vector3(32, 10, 60), new Vector3(32, 0, 30), Vector3.Up);
                var projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                    Game.GraphicsDevice.Viewport.AspectRatio, 1.0f, 100.0f);

                var effect = AssetTestUtility.CompileEffect(Game.GraphicsDevice, "VertexTextureEffect.fx");
                effect.Parameters["WorldViewProj"].SetValue(viewMatrix * projectionMatrix);
                effect.Parameters["HeightMapTexture"].SetValue(heightMapTexture);
                effect.Parameters["HeightMapSize"].SetValue((float) heightMapSize);

                effect.CurrentTechnique.Passes[0].Apply();

                const int numVertices = heightMapSize * heightMapSize;
                var vertexBuffer = new VertexBuffer(Game.GraphicsDevice, typeof(VertexPosition2), numVertices, BufferUsage.WriteOnly);
                var vertices = new VertexPosition2[numVertices];
                for (var y = 0; y < heightMapSize; y++)
                    for (var x = 0; x < heightMapSize; x++)
                        vertices[(y * heightMapSize) + x] = new VertexPosition2 { Position = new Vector2(x, y) };
                vertexBuffer.SetData(vertices);
                Game.GraphicsDevice.SetVertexBuffer(vertexBuffer);

                const int numIndices = (heightMapSize - 1) * (heightMapSize - 1) * 2 * 3;
                var indexBuffer = new IndexBuffer(Game.GraphicsDevice, IndexElementSize.SixteenBits, numIndices, BufferUsage.WriteOnly);
                var indexData = new short[numIndices];
                var indexIndex = 0;
                for (short y = 0; y < heightMapSize - 1; y++)
                    for (short x = 0; x < heightMapSize - 1; x++)
                    {
                        var baseIndex = (short) ((y * heightMapSize) + x);

                        indexData[indexIndex++] = baseIndex;
                        indexData[indexIndex++] = (short) (baseIndex + heightMapSize);
                        indexData[indexIndex++] = (short) (baseIndex + 1);

                        indexData[indexIndex++] = (short) (baseIndex + 1);
                        indexData[indexIndex++] = (short) (baseIndex + heightMapSize);
                        indexData[indexIndex++] = (short) (baseIndex + heightMapSize + 1);
                    }
                indexBuffer.SetData(indexData);
                Game.GraphicsDevice.Indices = indexBuffer;

                Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

                Game.GraphicsDevice.Clear(Color.CornflowerBlue);

                Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    0, 0, numVertices, 0, numIndices / 3);
            };
            RunSingleFrameTest();
        }

        [Test]
        public void VertexSamplerStatesGetSet()
        {
            Game.DrawWith += (sender, e) =>
            {
                var samplerState = new SamplerState { Filter = TextureFilter.Point };
                Game.GraphicsDevice.VertexSamplerStates[0] = samplerState;

                var retrievedSamplerState = Game.GraphicsDevice.VertexSamplerStates[0];
                Assert.That(retrievedSamplerState, Is.SameAs(samplerState));
            };
            Game.Run();
        }
    }
}