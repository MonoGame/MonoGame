// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tests.ContentPipeline;
using NUnit.Framework;
#if DESKTOPGL
using MonoGame.OpenGL;
#endif

namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    internal class GraphicsDeviceTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void BlendFactor()
        {
            Assert.AreEqual(Color.White, gd.BlendFactor);

            var blend = new BlendState
            {
                BlendFactor = Color.Crimson
            };

            gd.BlendState = blend;
            Assert.AreEqual(Color.Crimson, gd.BlendFactor);

            gd.BlendFactor = Color.CornflowerBlue;
            Assert.AreEqual(Color.Crimson, gd.BlendState.BlendFactor);
            Assert.AreEqual(Color.CornflowerBlue, gd.BlendFactor);

            blend.Dispose();
        }

        [Test]
        public void CtorAdapterNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new GraphicsDevice(null, GraphicsProfile.Reach, new PresentationParameters()));
        }

        [Test]
        public void CtorPresentationParametersNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.Reach, null));
        }

        [Test]
        public void DisposedWhenDisposingInvoked()
        {
            var count = 0;

            gd.Disposing += (sender, args) =>
            {
                Assert.IsTrue(gd.IsDisposed);
                count++;
            };

            gd.Dispose();
            Assert.AreEqual(1, count);

            // Disposing should not be invoked more than once
            gd.Dispose();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ResetDoesNotTriggerDeviceLost()
        {
            // TODO figure out exactly when a device is lost
            var resetCount = 0;
            var devLostCount = 0;

            gd.DeviceReset += (sender, args) =>
            {
                resetCount++;
                Assert.AreEqual(0, devLostCount);
            };

            gd.DeviceLost += (sender, args) =>
            {
                devLostCount++;
                Assert.AreEqual(1, resetCount);
            };

            gd.Reset();

            Assert.AreEqual(1, resetCount);
            Assert.AreEqual(0, devLostCount);
        }

        [Test]
        public void ResetDoesNotClearState()
        {
            gd.RasterizerState = RasterizerState.CullNone;
            gd.BlendState = BlendState.NonPremultiplied;
            gd.DepthStencilState = DepthStencilState.None;
            gd.SamplerStates[0] = SamplerState.PointClamp;
            var vbb = new VertexBufferBinding(new VertexBuffer(gd, VertexPositionColor.VertexDeclaration, 5, BufferUsage.WriteOnly));
            gd.SetVertexBuffers(vbb);

            gd.Reset();

            Assert.AreEqual(RasterizerState.CullNone, gd.RasterizerState);
            Assert.AreEqual(BlendState.NonPremultiplied, gd.BlendState);
            Assert.AreEqual(DepthStencilState.None, gd.DepthStencilState);
            Assert.AreEqual(SamplerState.PointClamp, gd.SamplerStates[0]);
            // TODO GetVertexBuffers is not implemented in MG
#if XNA
            Assert.AreEqual(vbb, gd.GetVertexBuffers()[0]);
#endif
            vbb.VertexBuffer.Dispose();
        }

        [Test, Ignore("Make sure dynamic graphics resources are notified when graphics device is lost")]
        public void ContentLostResources()
        {
            // https://blogs.msdn.microsoft.com/shawnhar/2007/12/12/virtualizing-the-graphicsdevice-in-xna-game-studio-2-0/

            var rt = new RenderTarget2D(gdm.GraphicsDevice, 5, 5);
            var vb = new DynamicVertexBuffer(gd, VertexPositionColor.VertexDeclaration, 1, BufferUsage.None);
            var ib = new DynamicIndexBuffer(gd, IndexElementSize.SixteenBits, 1, BufferUsage.None);
            var rtc = new RenderTargetCube(gd, 1, false, SurfaceFormat.Color, DepthFormat.Depth16);

            gd.Reset();

            Assert.IsTrue(rt.IsContentLost);
            Assert.IsFalse(rt.IsDisposed);

            Assert.IsTrue(vb.IsContentLost);
            Assert.IsFalse(vb.IsDisposed);

            Assert.IsTrue(ib.IsContentLost);
            Assert.IsFalse(ib.IsDisposed);

            Assert.IsTrue(rtc.IsContentLost);
            Assert.IsFalse(rtc.IsDisposed);

            rt.Dispose();
            vb.Dispose();
            ib.Dispose();
            rtc.Dispose();
        }

        [Test]
#if DESKTOPGL
        [Ignore("Does not throw the exception. Needs Investigating")]
#endif
        public void ResetWindowHandleNullThrowsException()
        {
            Assert.Throws<ArgumentException>(() => gd.Reset(new PresentationParameters()));
        }

		[Test]
		public void Clear()
		{
			var colors = new Color [] {
				Color.Red,
				Color.Orange,
				Color.Yellow,
				Color.Green,
				Color.Blue,
				Color.Indigo,
				Color.Violet
			};

            PrepareFrameCapture(colors.Length);

		    foreach (var color in colors)
		    {
		        gd.Clear(color);
                SubmitFrame();
		    }

            CheckFrames();
		}

        [Test]
        public void DrawPrimitivesParameterValidation()
        {
            var vertexBuffer = new VertexBuffer(
                gd, VertexPositionColorTexture.VertexDeclaration,
                3, BufferUsage.None);

            // No vertex shader or pixel shader.
            Assert.Throws<InvalidOperationException>(() => gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 1));

            new BasicEffect(gd).CurrentTechnique.Passes[0].Apply();

            // No vertexBuffer.
            Assert.Throws<InvalidOperationException>(() => gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 1));

            gd.SetVertexBuffer(vertexBuffer);

            // Success - "normal" usage.
            Assert.DoesNotThrow(() => gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 1));

            // vertexStart too small / large.
            Assert.DoesNotThrow(() => gd.DrawPrimitives(PrimitiveType.TriangleList, -1, 1));
            Assert.DoesNotThrow(() => gd.DrawPrimitives(PrimitiveType.TriangleList, 3, 1));

            // primitiveCount too small / large.
            Assert.Throws<ArgumentOutOfRangeException>(() => gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 0));
            Assert.DoesNotThrow(() => gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 2));

            // vertexStart + primitiveCount too large.
            Assert.DoesNotThrow(() => gd.DrawPrimitives(PrimitiveType.TriangleList, 1, 1));

            vertexBuffer.Dispose();
        }

        [Test]
        public void DrawIndexedPrimitivesParameterValidation()
        {
            var vertexBuffer = new VertexBuffer(
                gd, VertexPositionColorTexture.VertexDeclaration,
                3, BufferUsage.None);
            var indexBuffer = new IndexBuffer(
                gd, IndexElementSize.SixteenBits, 
                3, BufferUsage.None);

            // No vertex shader or pixel shader.
            Assert.Throws<InvalidOperationException>(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1));

            new BasicEffect(gd).CurrentTechnique.Passes[0].Apply();

            // No vertexBuffer.
            Assert.Throws<InvalidOperationException>(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1));

            gd.SetVertexBuffer(vertexBuffer);

            // No indexBuffer.
            Assert.Throws<InvalidOperationException>(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1));

            gd.Indices = indexBuffer;

            // Success - "normal" usage.
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1));

            // XNA doesn't do upfront parameter validation on the Assert.DoesNotThrow tests,
            // but it *sometimes* fails later with an AccessViolationException, so we can't actually
            // run these tests as part of the XNA test suite.

            // baseVertex too small / large.
#if !XNA
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, -1, 0, 3, 0, 1));
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 3, 0, 3, 0, 1));
#endif

            // startIndex too small / large.
#if !XNA
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, -1, 1));
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 3, 1));
#endif

            // primitiveCount too small / large.
            Assert.Throws<ArgumentOutOfRangeException>(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 0));
#if !XNA
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 2));
#endif

            // startIndex + primitiveCount too large.
#if !XNA
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 1, 1));
#endif
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        }

        // This overload of DrawIndexedPrimitives is not supported on XNA.
#if !XNA
        [Test]
        public void DrawIndexedPrimitivesParameterValidation2()
        {
            var vertexBuffer = new VertexBuffer(
                gd, VertexPositionColorTexture.VertexDeclaration,
                3, BufferUsage.None);
            var indexBuffer = new IndexBuffer(
                gd, IndexElementSize.SixteenBits, 
                3, BufferUsage.None);

            // No vertex shader or pixel shader.
            Assert.Throws<InvalidOperationException>(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 1));

            var effect = new BasicEffect(gd);
            effect.CurrentTechnique.Passes[0].Apply();

            // No vertexBuffer.
            Assert.Throws<InvalidOperationException>(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 1));

            gd.SetVertexBuffer(vertexBuffer);

            // No indexBuffer.
            Assert.Throws<InvalidOperationException>(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 1));

            gd.Indices = indexBuffer;

            // Success - "normal" usage.
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 1));

            // baseVertex too small / large.
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, -1, 0, 1));
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 3, 0, 1));

            // startIndex too small / large.
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, -1, 1));
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 3, 1));

            // primitiveCount too small / large.
            Assert.Throws<ArgumentOutOfRangeException>(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 0));
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2));

            // startIndex + primitiveCount too large.
            Assert.DoesNotThrow(() => gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 1, 1));

            effect.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        }
#endif

#if XNA || DIRECTX
        [Test]
        public void DrawInstancedPrimitivesParameterValidation()
        {
            var vertexBuffer = new VertexBuffer(
                gd, VertexPositionColorTexture.VertexDeclaration,
                3, BufferUsage.None);

            VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
                new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
                new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
                new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
            );
            var instanceBuffer = new VertexBuffer(
                gd, instanceVertexDeclaration,
                10, BufferUsage.None);

            var indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, 3, BufferUsage.None);

            // No vertex shader or pixel shader.
            Assert.Throws<InvalidOperationException>(() => gd.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1, 10));

            var effect = AssetTestUtility.LoadEffect(content, "Instancing");
            effect.Techniques[0].Passes[0].Apply();

            // No vertexBuffers.
            Assert.Throws<InvalidOperationException>(() => gd.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1, 10));

            gd.SetVertexBuffers(
                new VertexBufferBinding(vertexBuffer, 0, 0),
                new VertexBufferBinding(instanceBuffer, 0, 1));

            // No indexBuffer.
            Assert.Throws<InvalidOperationException>(() => gd.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1, 10));

            gd.Indices = indexBuffer;

            // Success - "normal" usage.
            Assert.DoesNotThrow(() => gd.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1, 10));

            // primitiveCount too small / large.
            Assert.Throws<ArgumentOutOfRangeException>(() => gd.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 0, 10));

            effect.Dispose();
            vertexBuffer.Dispose();
            instanceBuffer.Dispose();
            indexBuffer.Dispose();
        }

        [Test]
        public void DrawInstancedPrimitivesVisualTest()
        {
            VertexBuffer vertexBuffer = null;
            IndexBuffer indexBuffer = null;
            VertexBuffer instanceVertexBuffer = null;
            Matrix[] worldTransforms = null;
            EffectPass pass = null;

            // Create vertex and index buffer for a quad.
            var vertices = new[]
            {
                new VertexPositionTexture(new Vector3(-1,  1, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3( 1,  1, 0), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3( 1, -1, 0), new Vector2(1, 1)),
            };
            vertexBuffer = new VertexBuffer(
                gd, VertexPositionTexture.VertexDeclaration,
                4, BufferUsage.None);
            vertexBuffer.SetData(vertices);

            var indices = new ushort[] { 0, 1, 2, 1, 3, 2 };
            indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, 6, BufferUsage.None);
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
            instanceVertexBuffer = new VertexBuffer(gd, instanceVertexDeclaration, worldTransforms.Length, BufferUsage.None);
            instanceVertexBuffer.SetData(worldTransforms);

            var view = Matrix.CreateLookAt(new Vector3(0, 0, 6), new Vector3(0, 0, 0), Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, gd.Viewport.AspectRatio, 0.1f, 100);

            var effect = AssetTestUtility.LoadEffect(content, "Instancing");
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            pass = effect.Techniques[0].Passes[0];

            PrepareFrameCapture();

            gd.Clear(Color.CornflowerBlue);

            gd.SetVertexBuffers(
                new VertexBufferBinding(vertexBuffer, 0, 0),
                new VertexBufferBinding(instanceVertexBuffer, 0, 1));

            gd.Indices = indexBuffer;

            pass.Apply();

            gd.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 6, 0, 2, worldTransforms.Length);

            // There is a minor difference in the rasterization between XNA and DirectX. 
            Similarity = 0.98f;

            CheckFrames();

            effect.Dispose();
            vertexBuffer.Dispose();
            instanceVertexBuffer.Dispose();
            indexBuffer.Dispose();
        }
#endif

        [Test]
        public void DrawUserPrimitivesParameterValidation()
        {
            var vertexDataNonEmpty = new[]
            {
                new VertexPositionColorTexture(Vector3.Zero, Color.White, Vector2.Zero),
                new VertexPositionColorTexture(Vector3.Zero, Color.White, Vector2.Zero),
                new VertexPositionColorTexture(Vector3.Zero, Color.White, Vector2.Zero)
            };
            var vertexDataEmpty = new VertexPositionColorTexture[0];

            // No vertex shader or pixel shader.
            Assert.Throws<InvalidOperationException>(() => gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 1));

            var effect = new BasicEffect(gd);
            effect.CurrentTechnique.Passes[0].Apply();

            // Success - "normal" usage.
            Assert.DoesNotThrow(() => gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 1));

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
            Assert.Throws<ArgumentNullException>(() => gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 1, null));

            effect.Dispose();
        }

        private void DoDrawUserPrimitivesAsserts(VertexPositionColorTexture[] vertexData, int vertexOffset, int primitiveCount, Action<TestDelegate> assertMethod)
        {
            assertMethod(() => gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertexData, vertexOffset, primitiveCount));
            assertMethod(() => gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertexData, vertexOffset, primitiveCount, VertexPositionColorTexture.VertexDeclaration));
        }

        [Test]
        public void DrawUserIndexedPrimitivesParameterValidation()
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
            Assert.Throws<InvalidOperationException>(() => gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 0, 1));

            var effect = new BasicEffect(gd);
            effect.CurrentTechnique.Passes[0].Apply();

            // Success - "normal" usage.
            Assert.DoesNotThrow(() => gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 0, 1));

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
            Assert.Throws<ArgumentNullException>(() => gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 0, 1, null));

            // Smaller vertex stride in VertexDeclaration than in actual vertices.

            // XNA is inconsistent; in DrawUserIndexedPrimitives, it allows vertexStride to be less than the actual size of the data,
            // but in VertexBuffer.SetData, XNA requires vertexStride to greater than or equal to the actual size of the data.
            // Since we use a DynamicVertexBuffer to implement DrawUserIndexedPrimitives, we use the same validation in both places.
            // The same applies to DrawUserPrimitives.
#if XNA
            Assert.DoesNotThrow(() => gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 0, 1, VertexPositionColor.VertexDeclaration));
            Assert.DoesNotThrow(() => gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty.Select(x => (int) x).ToArray(), 0, 1, VertexPositionColor.VertexDeclaration));
#else
            Assert.Throws<ArgumentOutOfRangeException>(() => gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty, 0, 1, VertexPositionColor.VertexDeclaration));
            Assert.Throws<ArgumentOutOfRangeException>(() => gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexDataNonEmpty, 0, 3, indexDataNonEmpty.Select(x => (int) x).ToArray(), 0, 1, VertexPositionColor.VertexDeclaration));
#endif
            effect.Dispose();
        }

        private void DoDrawUserIndexedPrimitivesAsserts(VertexPositionColorTexture[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, Action<TestDelegate> assertMethod)
        {
            assertMethod(() => gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount));
            assertMethod(() => gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, VertexPositionColorTexture.VertexDeclaration));

            var intIndexData = (indexData == null) ? null : indexData.Select(x => (int) x).ToArray();
            assertMethod(() => gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, vertexOffset, numVertices, intIndexData, indexOffset, primitiveCount));
            assertMethod(() => gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, vertexOffset, numVertices, intIndexData, indexOffset, primitiveCount, VertexPositionColorTexture.VertexDeclaration));
        }

        [Test]
#if DESKTOPGL
        [Ignore("Vertex Textures are not implemented for OpenGL")]
#endif
        public void VertexTexturesGetSet()
        {
            // TODO: The availability of vertex textures should depend on GraphicsProfile.

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
                var texture = new Texture2D(gd, 4, 4, false, format);

                if (supportedVertexTextureFormats.Contains(format))
                {
                    gd.VertexTextures[0] = texture;
                    var retrievedTexture = gd.VertexTextures[0];
                    Assert.That(retrievedTexture, Is.SameAs(texture));
                }
                else
                {
                    Assert.Throws<NotSupportedException>(() =>
                        gd.VertexTextures[0] = texture);
                }
            }
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
#if DESKTOPGL
        [Ignore("Vertex Textures are not implemented for OpenGL")]
#endif
        public void VertexTextureVisualTest()
        {
            // Implements an extremely simple terrain that reads from a heightmap in the vertex shader.
            PrepareFrameCapture();

            const int heightMapSize = 64;
            var heightMapTexture = new Texture2D(gd, heightMapSize, heightMapSize, false, SurfaceFormat.Single);
            var heightMapData = new float[heightMapSize * heightMapSize];
            for (var y = 0; y < heightMapSize; y++)
                for (var x = 0; x < heightMapSize; x++)
                    heightMapData[(y * heightMapSize) + x] = (float) Math.Sin(x / 2.0f) + (float) Math.Sin(y / 3.0f);
            heightMapTexture.SetData(heightMapData);

            var viewMatrix = Matrix.CreateLookAt(new Vector3(32, 10, 60), new Vector3(32, 0, 30), Vector3.Up);
            var projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                gd.Viewport.AspectRatio, 1.0f, 100.0f);

            var effect = AssetTestUtility.LoadEffect(content, "VertexTextureEffect");
            effect.Parameters["WorldViewProj"].SetValue(viewMatrix * projectionMatrix);
            effect.Parameters["HeightMapTexture"].SetValue(heightMapTexture);
            effect.Parameters["HeightMapSize"].SetValue((float) heightMapSize);

            effect.CurrentTechnique.Passes[0].Apply();

            const int numVertices = heightMapSize * heightMapSize;
            var vertexBuffer = new VertexBuffer(gd, typeof(VertexPosition2), numVertices, BufferUsage.WriteOnly);
            var vertices = new VertexPosition2[numVertices];
            for (var y = 0; y < heightMapSize; y++)
                for (var x = 0; x < heightMapSize; x++)
                    vertices[(y * heightMapSize) + x] = new VertexPosition2 { Position = new Vector2(x, y) };
            vertexBuffer.SetData(vertices);
            gd.SetVertexBuffer(vertexBuffer);

            const int numIndices = (heightMapSize - 1) * (heightMapSize - 1) * 2 * 3;
            var indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, numIndices, BufferUsage.WriteOnly);
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
            gd.Indices = indexBuffer;

            gd.RasterizerState = RasterizerState.CullNone;

            gd.Clear(Color.CornflowerBlue);

            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                0, 0, numVertices, 0, numIndices / 3);

            CheckFrames();

            effect.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        }

        [Test]
#if DESKTOPGL
        [Ignore("Vertex samplers are not implemented for OpenGL")]
#endif
        public void VertexSamplerStatesGetSet()
        {
            var samplerState = new SamplerState { Filter = TextureFilter.Point };
            gd.VertexSamplerStates[0] = samplerState;

            var retrievedSamplerState = gd.VertexSamplerStates[0];
            Assert.That(retrievedSamplerState, Is.SameAs(samplerState));

            samplerState.Dispose();
        }

        [Test]
        public void PresentInvalidOperationException()
        {
            // This should work else it means we had
            // some bad state to start this test!
            gd.Present();

            // You can't call present with a RT set.
            var rt = new RenderTarget2D(gd, 100, 100);
            gd.SetRenderTarget(rt);
            Assert.Throws<InvalidOperationException>(() => gd.Present());

            // Set the default RT and present works again.
            gd.SetRenderTarget(null);
            gd.Present();

            // Cleanup.
            rt.Dispose();
        }

#if DESKTOPGL
        [Test]
        public void DifferentVboGetsSet()
        {
            var vb1 = new VertexBuffer(gd, VertexPosition.VertexDeclaration, 6, BufferUsage.None);
            var vb2 = new VertexBuffer(gd, VertexPosition.VertexDeclaration, 8, BufferUsage.None);

            var se = new SpriteEffect(gd);
            se.CurrentTechnique.Passes[0].Apply();

            gd.SetVertexBuffer(vb1);
            gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

            int vbo;
            GL.GetInteger(GetPName.ArrayBufferBinding, out vbo);
            Assert.AreEqual(vb1.vbo, vbo);

            gd.SetVertexBuffer(vb2);
            gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

            GL.GetInteger(GetPName.ArrayBufferBinding, out vbo);
            Assert.AreEqual(vb2.vbo, vbo);

            se.Dispose();
            vb1.Dispose();
            vb2.Dispose();
        }
#endif

        private static Rectangle?[] BackBufferRects()
        {
            return new Rectangle?[]
            {
                null,
                new Rectangle(100, 100, 250, 250),
            };
        }

        [TestCaseSource("BackBufferRects")]
        public void GetBackBufferData(Rectangle? rectangle)
        {
            gd.Clear(Color.CornflowerBlue);

            Rectangle rect;
            if (rectangle == null)
                rect = gd.Viewport.Bounds;
            else
                rect = rectangle.Value;

            var tex = content.Load<Texture2D>(Paths.Texture("Surge"));
            var sb = new SpriteBatch(gd);

            sb.Begin();
            sb.Draw(tex, new Rectangle(150, 150, 300, 300), Color.White);
            sb.End();

            var buffer = new Color[rect.Width * rect.Height];
            gd.GetBackBufferData(rect, buffer, 0, rect.Width * rect.Height);

            var backbufferTex = new Texture2D(gd, rect.Width, rect.Height);
            backbufferTex.SetData(buffer);

            PrepareFrameCapture();
            gd.Clear(Color.Red);

            sb.Begin();
            sb.Draw(backbufferTex, rect, Color.White);
            sb.End();

            CheckFrames();
        }
    }
}
