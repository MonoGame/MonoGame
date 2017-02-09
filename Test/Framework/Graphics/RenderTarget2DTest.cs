﻿// MonoGame - Copyright (C) The MonoGame Team
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
#if XNA
        [Ignore]
#endif
        public void GenerateMips()
        {
            // Please note:
            // The reference image was created with the MonoGame/Windows test.
            // Mipmaps created by XNA and MonoGame are different.
            // Mipmaps created by DirectX 11 and OpenGL can also be different - at least for 
            // NPOT textures.
            var texture = content.Load<Texture2D>(Paths.Texture("MonoGameIcon"));

            CaptureRegion = new Rectangle(0, 0, 128 * 4 + 3, 128 * 2 + 1);
            PrepareFrameCapture();

            var spriteBatch = new SpriteBatch(gd);

            // Remember original (frame capture) render target.
            var renderTargets = gd.GetRenderTargets();
            RenderTarget2D originalRenderTarget = null;
            if (renderTargets != null && renderTargets.Length > 0)
                originalRenderTarget = renderTargets[0].RenderTarget as RenderTarget2D;

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

            gd.SetRenderTarget(originalRenderTarget);

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
                if (i == 3)
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
    }
}
