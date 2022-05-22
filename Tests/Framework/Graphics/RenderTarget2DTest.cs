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
        public void GetDataMSAA()
        {
            const int size = 100;
            const int size2 = size * size;
            var rt = new RenderTarget2D(gd, size, size, false, SurfaceFormat.Color, DepthFormat.None, 8, RenderTargetUsage.DiscardContents);
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
    }
}
