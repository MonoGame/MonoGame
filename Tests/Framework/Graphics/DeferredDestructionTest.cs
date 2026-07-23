// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if VULKAN || DIRECTX12

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [Category("MemoryLifeCycle")]
    [NonParallelizable]
    [RunOnUiTestFixture]
    internal class DeferredDestructionTest
    {
        [Test]
        public void WaitForGpuToIdleBeforeDisposingBuffer()
        {
            var testGame = new TestGameBase()
            {
                IsFixedTimeStep = false,
            };
            new GraphicsDeviceManager(testGame)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
            };
            var graphicsDeviceManager = testGame.Services.GetService<IGraphicsDeviceManager>();
            graphicsDeviceManager.CreateDevice();
            var graphicsDevice = testGame.GraphicsDevice;

            // Repeat many times to catch the racing condition.
            for (var i = 0; i < 256; i++)
            {
                // Create a vertex buffer.
                var vertexBuffer = new VertexBuffer(
                    graphicsDevice,
                    VertexPositionColor.VertexDeclaration,
                    3,
                    BufferUsage.None);

                // Issue GPU commands referencing the buffer.
                vertexBuffer.SetData(
                [
                    new VertexPositionColor(new Vector3(1, 0, 0), Color.Red),
                    new VertexPositionColor(new Vector3(0, 1, 0), Color.Green),
                    new VertexPositionColor(new Vector3(0, 0, 1), Color.Blue),
                ]);

                // Create index buffer.
                var indexBuffer = new IndexBuffer(
                    graphicsDevice,
                    IndexElementSize.SixteenBits,
                    3,
                    BufferUsage.None);

                // Issue GPU commands referencing the buffer.
                indexBuffer.SetData(new short[] { 0, 1, 2 });

                graphicsDevice.Clear(Color.Black);
                graphicsDevice.Present();

                // Dispose while GPU *might* still be referencing the buffer memory.
                vertexBuffer.Dispose();
                indexBuffer.Dispose();
            }

            //graphicsDevice.Dispose();
            testGame.Dispose();

            Assert.Pass();
        }

        [Test]
        public void WaitForGpuToIdleBeforeDisposingRenderTarget()
        {
            var testGame = new TestGameBase()
            {
                IsFixedTimeStep = false,
            };
            new GraphicsDeviceManager(testGame)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
            };
            var graphicsDeviceManager = testGame.Services.GetService<IGraphicsDeviceManager>();
            graphicsDeviceManager.CreateDevice();
            var graphicsDevice = testGame.GraphicsDevice;

            for (var i = 0; i < 100; i++)
            {
                var rt = new RenderTarget2D(graphicsDevice, 64, 64);

                graphicsDevice.SetRenderTarget(rt);
                graphicsDevice.Clear(new Color(i * 5, 0, 0));
                graphicsDevice.SetRenderTarget(null);
                graphicsDevice.Present();

                rt.Dispose();
            }

            //graphicsDevice.Dispose();
            testGame.Dispose();

            // We should only get here if we correctly wait for the GPU on each disposal.
            Assert.Pass();
        }

        [Test]
        public void RenderTargetTextureSurvivesDeferredDestruction()
        {
            var testGame = new TestGameBase()
            {
                IsFixedTimeStep = false,
            };
            new GraphicsDeviceManager(testGame)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
            };
            var graphicsDeviceManager = testGame.Services.GetService<IGraphicsDeviceManager>();
            graphicsDeviceManager.CreateDevice();
            var graphicsDevice = testGame.GraphicsDevice;

            var renderTarget = new RenderTarget2D(graphicsDevice, 128, 128);

            // Advance frames past threshold to make texture appear stale.
            for (var i = 0; i < 10; i++)
            {
                graphicsDevice.Clear(Color.Black);
                graphicsDevice.Present();
            }

            // Bind RenderTarget and present.
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.MonoGameOrange);
            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Present();

            // Read RenderTarget texture to verify the texture frame was updated.
            var data = new Color[128 * 128];
            renderTarget.GetData(data);

            Assert.AreEqual(
                Color.MonoGameOrange,
                data[0],
                "Render target texture was destroyed prematurely.");

            renderTarget.Dispose();
            //graphicsDevice.Dispose();
            testGame.Dispose();
        }

        [Test]
        public void RenderTargetTextureWithDepthSurvivesDeferredDestruction()
        {
            var testGame = new TestGameBase()
            {
                IsFixedTimeStep = false,
            };
            new GraphicsDeviceManager(testGame)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
            };
            var graphicsDeviceManager = testGame.Services.GetService<IGraphicsDeviceManager>();
            graphicsDeviceManager.CreateDevice();
            var graphicsDevice = testGame.GraphicsDevice;

            var renderTarget = new RenderTarget2D(
                graphicsDevice,
                128,
                128,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24);

            // Advance frames past threshold to make texture appear stale.
            for (var i = 0; i < 10; i++)
            {
                graphicsDevice.Clear(Color.Black);
                graphicsDevice.Present();
            }

            // Bind RenderTarget and present.
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.MonoGameOrange);
            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Present();

            // Read RenderTarget texture to verify the texture frame was updated.
            var data = new Color[128 * 128];
            renderTarget.GetData(data);

            Assert.AreEqual(
                Color.MonoGameOrange,
                data[0],
                "Render target texture with depth was destroyed prematurely.");

            renderTarget.Dispose();
            //graphicsDevice.Dispose();
            testGame.Dispose();
        }

        [Test]
        public void MultipleRenderTargetsDisposeOrder()
        {
            var testGame = new TestGameBase()
            {
                IsFixedTimeStep = false,
            };
            new GraphicsDeviceManager(testGame)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
            };
            var graphicsDeviceManager = testGame.Services.GetService<IGraphicsDeviceManager>();
            graphicsDeviceManager.CreateDevice();
            var graphicsDevice = testGame.GraphicsDevice;

            var renderTarget1 = new RenderTarget2D(graphicsDevice, 128, 128);
            var renderTarget2 = new RenderTarget2D(graphicsDevice, 128, 128);
            var renderTarget3 = new RenderTarget2D(graphicsDevice, 128, 128);
            var renderTarget4 = new RenderTarget2D(graphicsDevice, 128, 128);

            // Bind and use each.
            graphicsDevice.SetRenderTarget(renderTarget1);
            graphicsDevice.Clear(Color.Red);

            graphicsDevice.SetRenderTarget(renderTarget2);
            graphicsDevice.Clear(Color.Green);

            graphicsDevice.SetRenderTarget(renderTarget3);
            graphicsDevice.Clear(Color.Blue);

            graphicsDevice.SetRenderTarget(renderTarget4);
            graphicsDevice.Clear(Color.MonoGameOrange);

            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Present();

            renderTarget4.Dispose();
            renderTarget3.Dispose();
            renderTarget2.Dispose();
            renderTarget1.Dispose();
            //graphicsDevice.Dispose();
            testGame.Dispose();

            Assert.Pass();
        }

        [Test]
        public void DisposeAfterRenderTargetPresented()
        {
            var testGame = new TestGameBase()
            {
                IsFixedTimeStep = false,
            };
            new GraphicsDeviceManager(testGame)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
            };
            var graphicsDeviceManager = testGame.Services.GetService<IGraphicsDeviceManager>();
            graphicsDeviceManager.CreateDevice();
            var graphicsDevice = testGame.GraphicsDevice;

            // Create RenderTargets to be put on deferred destruction queues.
            var renderTarget1 = new RenderTarget2D(graphicsDevice, 4096, 4096);
            var renderTarget2 = new RenderTarget2D(
                graphicsDevice,
                4096,
                4096,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24);

            // Issue GPU commands on renderTarget1.
            graphicsDevice.SetRenderTarget(renderTarget1);
            graphicsDevice.Clear(Color.Red);
            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Present();

            // Issue GPU commands on renderTarget2.
            graphicsDevice.SetRenderTarget(renderTarget2);
            graphicsDevice.Clear(Color.Blue);
            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Present();

            // Dispose resources and the game immediately.
            renderTarget1.Dispose();
            renderTarget2.Dispose();
            //graphicsDevice.Dispose();
            testGame.Dispose();

            // We should only reach here if there were no out-of-order disposals of resources.
            Assert.Pass();
        }        
    }
}

#endif
