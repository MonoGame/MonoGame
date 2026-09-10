// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [NonParallelizable]
    [RunOnUiTestFixture]
    class RenderTarget2DTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ZeroSizeShouldFailTest()
        {
            RenderTarget2D renderTarget;
            Assert.Throws<ArgumentOutOfRangeException>(() => renderTarget = new RenderTarget2D(gd, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => renderTarget = new RenderTarget2D(gd, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => renderTarget = new RenderTarget2D(gd, 0, 0));
        }

        [Test]
        public void NullDeviceShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
            {
                var renderTarget = new RenderTarget2D(null, 16, 16);
                renderTarget.Dispose();
            });
            GC.GetTotalMemory(true); // collect uninitialized renderTarget
        }

        [Test]
#if XNA
        [Ignore("XNA mipmaps fail our pixel comparison tests")]
#endif
        public void GenerateMips()
        {
#if VULKAN
            if (OperatingSystem.IsMacOS())
            {
                Assert.Ignore("TODO: Fix on macOS");
                return;
            }
#endif

            // Please note:
            // The reference image was created with the MonoGame/Windows test.
            // Mipmaps created by XNA and MonoGame are different.
            // Mipmaps created by DirectX 11 and OpenGL can also be different - at least for 
            // NPOT textures.

            PrepareFrameCapture();

            var texture = content.Load<Texture2D>(Paths.Texture("MonoGameIcon"));
            var spriteBatch = new SpriteBatch(gd);

            // Remember original (frame capture) render target.
            var renderTargets = gd.GetRenderTargets();

            var viewport = gd.Viewport;
            var renderTarget = new RenderTarget2D(
                gd,
                128,
                128,
                true,     // Enable mipmaps.
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.DiscardContents);

            // Render sprites with random positions into the offscreen render target.
            gd.SetRenderTarget(renderTarget);
            gd.Clear(Color.Gray);
            spriteBatch.Begin();
            for (int i = 0; i < 5; i++)
            {
                spriteBatch.Draw(
                    texture,
                    new Vector2(
                        (i * 1664525 + 1013904223) % (renderTarget.Width - texture.Width),
                        (i * 22695477 + 7777) % (renderTarget.Height - texture.Height)),
                    Color.White);
            }
            spriteBatch.End();

            gd.SetRenderTargets(renderTargets);

            // Display all mip levels.
            gd.Clear(Color.CornflowerBlue);
            int x = 0;
            int y = 0;
            for (int i = 0; i < renderTarget.LevelCount; i++)
            {
                var samplerState = new SamplerState
                {
                    Filter = TextureFilter.Point,
                    MipMapLevelOfDetailBias = i,
                    MaxMipLevel = i,
                };
                
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, samplerState, null, null);
                spriteBatch.Draw(renderTarget, new Vector2(x, y), Color.White);
                spriteBatch.End();

                x += renderTarget.Width + 1;
                if (x + renderTarget.Width > viewport.Width)
                {
                    x = 0;
                    y += renderTarget.Height + 1;
                }
                samplerState.Dispose();
            }

            CheckFrames();

            texture.Dispose();
            spriteBatch.Dispose();
            renderTarget.Dispose();
        }
        
        [Test]
        [TestCase(SurfaceFormat.Color, SurfaceFormat.Color)]
        // unsupported renderTarget formats
        [TestCase(SurfaceFormat.Alpha8, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Dxt1, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Dxt3, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Dxt5, SurfaceFormat.Color)]
#if !XNA        
        [TestCase(SurfaceFormat.Dxt1a, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Dxt1SRgb, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Dxt3SRgb, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Dxt5SRgb, SurfaceFormat.Color)]
#endif
        [TestCase(SurfaceFormat.NormalizedByte2, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.NormalizedByte4, SurfaceFormat.Color)]
        public void PreferredSurfaceFormatTest(SurfaceFormat preferredSurfaceFormat, SurfaceFormat expectedSurfaceFormat)
        {                    
            var renderTarget = new RenderTarget2D(gd, 16, 16, false, preferredSurfaceFormat, DepthFormat.None);
                    
            Assert.AreEqual(renderTarget.Format, expectedSurfaceFormat);
        }

        [Test]
#if DESKTOPGL
        [Ignore ("Causes GL.GetError() returned 1282. Need to fix.")]
#endif
        public void GetDataMSAA()
        {
            const int size = 100;
            const int size2 = size * size;
            var rt = new RenderTarget2D(gd, size, size, false, SurfaceFormat.Color, DepthFormat.None, 4, RenderTargetUsage.DiscardContents);
            var data = new Color[size2];
            // create some arbitrary data here
            for (var i = 0; i < size2; i++)
                data[i] = new Color(new Vector3(1f / (i + 1)));

            rt.SetData(data);

            var returnedData = new Color[size2];
            rt.GetData(returnedData);
            // verify that the gotten data is the same as the data we attempt to set
            for (var i = 0; i < size2; i++)
                Assert.AreEqual(data[i], returnedData[i]);

            rt.Dispose();
        }

#if DIRECTX
        [Test]
        [TestCase(1)]
        [TestCase(2)]
        public void GetSharedHandle(int preferredMultiSampleCount)
        {
            var rt = new RenderTarget2D(gd, 16, 16, false, SurfaceFormat.Color, DepthFormat.None, preferredMultiSampleCount, RenderTargetUsage.PlatformContents, true);            
            var sharedHandle = rt.GetSharedHandle();
            Assert.AreNotEqual(sharedHandle, IntPtr.Zero);

            var resource = SharpDX.CppObject.FromPointer<SharpDX.DXGI.Resource>(sharedHandle);

            rt.Dispose();
        }
#endif

        [Test]
        [TestCase(DepthFormat.None, 0)]
        [TestCase(DepthFormat.None, 1)]
        [TestCase(DepthFormat.None, 4)]
        [TestCase(DepthFormat.Depth16, 0)]
        [TestCase(DepthFormat.Depth16, 1)]
        [TestCase(DepthFormat.Depth16, 4)]
        [TestCase(DepthFormat.Depth24, 0)]
        [TestCase(DepthFormat.Depth24, 1)]
        [TestCase(DepthFormat.Depth24, 4)]
        [TestCase(DepthFormat.Depth24Stencil8, 0)]
        [TestCase(DepthFormat.Depth24Stencil8, 1)]
        [TestCase(DepthFormat.Depth24Stencil8, 4)]
        public void ClearAndGetDataWithMultiSample(DepthFormat depthFormat, int multiSampleCount)
        {
            const int size = 16;
            var rt = new RenderTarget2D(gd, size, size, mipMap: false, SurfaceFormat.Color, depthFormat, multiSampleCount, RenderTargetUsage.DiscardContents);
            try
            {
                var previousTargets = gd.GetRenderTargets();
                gd.SetRenderTarget(rt);
                gd.Clear(Color.MonoGameOrange);
                gd.SetRenderTargets(previousTargets);

                var pixels = new Color[size * size];
                rt.GetData(pixels);

                for (int i=0; i < pixels.Length; i++)
                {
                    Assert.AreEqual(Color.MonoGameOrange, pixels[i], $"Pixel {i} should be {Color.MonoGameOrange} but was {pixels[i]}");
                }
            }
            finally
            {
               rt.Dispose(); 
            }
        }

        
        // Disposed render targets should not stay referenced by the GraphicsDevice.
        // See issue: https://github.com/MonoGame/MonoGame/issues/9485
        [Test]
        [TestCase(DepthFormat.None, 0)]
        [TestCase(DepthFormat.None, 4)]
        [TestCase(DepthFormat.Depth16, 0)]
        [TestCase(DepthFormat.Depth16, 4)]
        [TestCase(DepthFormat.Depth24, 0)]
        [TestCase(DepthFormat.Depth24, 4)]
        [TestCase(DepthFormat.Depth24Stencil8, 0)]
        [TestCase(DepthFormat.Depth24Stencil8, 4)]
        public void DisposeAfterUse_NonMsaaRenderTarget_DoesNotRemainReferencedByGraphicsDevice(DepthFormat depthFormat, int preferredMultiSampleCount)
        {
            WeakReference weakRef = CreateAndDisposeRenderTarget(depthFormat, preferredMultiSampleCount);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.False(
                weakRef.IsAlive,
                "Disposed RenderTarget2D was still strongly referenced by the GraphicsDevice.");
        }

        // Keep creation and disposal out of the test method so the JIT does not extend the local lifetime.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private WeakReference CreateAndDisposeRenderTarget(DepthFormat depthFormat, int preferredMultiSampleCount)
        {
            RenderTarget2D renderTarget = new RenderTarget2D(
                gd,
                16,
                16,
                false,
                SurfaceFormat.Color,
                depthFormat,
                preferredMultiSampleCount,
                RenderTargetUsage.DiscardContents);

            gd.SetRenderTarget(renderTarget);
            gd.Clear(Color.CornflowerBlue);
            gd.SetRenderTarget(null);

            renderTarget.Dispose();

            return new WeakReference(renderTarget);
        }

        [Test]
        public void TestRenderTargetSync()
        {
            // This test is based on issue:
            // https://github.com/MonoGame/MonoGame/issues/9425

            var accumulationRenderTarget = new RenderTarget2D(
                gd,
                400,
                200,
                false, //Also fixed if mipMap is set to true
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.PreserveContents);

            var auxRenderTarget = new RenderTarget2D(gd, 600, 600);

            var spriteBatch = new SpriteBatch(gd);

            var whiteSquareTexture = new Texture2D(gd, 1, 1);
            whiteSquareTexture.SetData(new Color[] { Color.White });

            var maxBlendState = new BlendState
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Max,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
                AlphaBlendFunction = BlendFunction.Max,
            };

            var _rectangles = new List<Rectangle>
            {
                new(0, 0, 200, 200),
                new(200, 0, 200, 200)
            };


            // Do this a few times as we could get lucky
            // and not have a GPU artifact on one test.

            for (int t = 0; t < 6; t++)
            {
                gd.SetRenderTarget(accumulationRenderTarget);
                gd.Clear(Color.Red);

                for (int i = 0; i < _rectangles.Count; i++)
                {
                    gd.SetRenderTarget(auxRenderTarget);
                    gd.Clear(Color.Black);

                    spriteBatch.Begin();
                    spriteBatch.Draw(whiteSquareTexture, _rectangles[i], Color.White * 0.75f);
                    spriteBatch.End();

                    gd.SetRenderTarget(accumulationRenderTarget);

                    spriteBatch.Begin(blendState: maxBlendState);
                    spriteBatch.Draw(auxRenderTarget, Vector2.Zero, Color.White * 0.75f);
                    spriteBatch.End();
                }

                var data = accumulationRenderTarget.GetColorData();

                var good = new Color(255, 143, 143, 255);
                foreach (var color in data)
                {
                    // Some graphics drivers can be off in color because of
                    // subtle blend math optimizations... so use a tolerance.
                    Assert.True(good.AreEqual(color, 2), $"Color mismatch! {color} should be {good}");
                }
            }
        }
    }
}
