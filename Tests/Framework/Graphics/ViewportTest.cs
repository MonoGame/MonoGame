using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [NonParallelizable]
    [RunOnUiTestFixture]
    class ViewportTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void Affects_draw_origin()
        {
            PrepareFrameCapture();

            var spriteBatch = new SpriteBatch(gd);
            var swatch = content.Load<Texture2D>(Paths.Texture("white-64"));

            gd.Clear(Color.CornflowerBlue);
            gd.Viewport = new Viewport(20, 40, 100, 100);

            spriteBatch.Begin();
            spriteBatch.Draw(swatch, new Vector2(10, 20), Color.GreenYellow);
            spriteBatch.End();

            CheckFrames();

            spriteBatch.Dispose();
            swatch.Dispose();
        }

        [Test]
        public void Does_not_clip_device_clear()
        {
            PrepareFrameCapture();

            var presentParams = gd.PresentationParameters;
            gd.Viewport = new Viewport(
                0, 0,
                presentParams.BackBufferWidth,
                presentParams.BackBufferHeight);
            gd.Clear(Color.CornflowerBlue);

            gd.Viewport = new Viewport(30, 40, 100, 200);
            gd.Clear(Color.Red);

            CheckFrames();
        }

        // Ensure that when a device clear operation is performed, it clears the entire target
        // instead of just the viewport/scissor region
        // See: https://github.com/monogame/monogame/issues/9500
        [Test]
        public void Does_not_clip_device_clear_after_scissor_test_draw()
        {
            using SpriteBatch sb = new SpriteBatch(gd);
            using Texture2D swatch = content.Load<Texture2D>(Paths.Texture("white-64"));
            using RasterizerState scissorEnabled = new RasterizerState {ScissorTestEnable = true };

            // Clear to cornflower blue first to set precedent
            gd.Clear(Color.CornflowerBlue);

            // Set viewport to a smaller size to test if the clear is obeying hte viewport
            // which it should not!
            gd.Viewport = new Viewport(100, 100, 100, 100);
            gd.ScissorRectangle = new Rectangle(110, 110, 110, 110);

            // Draw random stuff to dirty the pre-clear state
            // Ensure scissor raster state is applied to test if it affects the clear operation
            sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, scissorEnabled);
            sb.Draw(swatch, Vector2.Zero, Color.White);
            sb.End();

            // Perform clear
            gd.Clear(ClearOptions.Target, Color.Red.ToVector4(), 0, 0);

            // Test single pixel in top-left corner, should be red from the clear
            var cornerPixel = new Color[1];
            gd.GetBackBufferData(new Rectangle(0, 0, 1, 1), cornerPixel, 0, 1);
            Assert.That(cornerPixel[0], Is.EqualTo(Color.Red));

            // Test pixel within the viewport scissor region, shoudl also be red from clear
            var viewportPixel = new Color[1];
            gd.GetBackBufferData(new Rectangle(120, 120, 1, 1), viewportPixel, 0, 1);
            Assert.That(viewportPixel[0], Is.EqualTo(Color.Red));
        }

        // Ensure that Clear preserves a pending scissor test disable.
        // See: https://github.com/MonoGame/MonoGame/issues/9586
        [Test]
        public void Does_not_restore_scissor_test_after_pending_disable_and_clear()
        {
            using SpriteBatch sb = new SpriteBatch(gd);
            using Texture2D swatch = content.Load<Texture2D>(Paths.Texture("white-64"));
            using RasterizerState scissorEnabled = new RasterizerState { ScissorTestEnable = true };
            using RasterizerState scissorDisabled = new RasterizerState { ScissorTestEnable = false };

            gd.Clear(Color.CornflowerBlue);

            // Use an empty scissor rectangle so a later incorrectly restored
            // scissor test prevents the final draw from writing any pixels.
            gd.ScissorRectangle = new Rectangle(0, 0, 0, 0);

            // Draw A applies the enabled scissor state.
            sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp,
                DepthStencilState.Default, scissorEnabled);
            sb.Draw(swatch, Vector2.Zero, Color.White);
            sb.End();

            // Queue a disabled scissor state without drawing. Clear must apply it
            // and leave the scissor test disabled after it completes.
            gd.RasterizerState = scissorDisabled;
            gd.Clear(Color.Red);

            // Draw B reuses the disabled state to verify that Clear leaves
            // the OpenGL scissor test disabled.
            sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp,
                DepthStencilState.Default, scissorDisabled);
            sb.Draw(swatch, Vector2.Zero, Color.Lime);
            sb.End();

            Color[] pixel = new Color[1];
            gd.GetBackBufferData(new Rectangle(0, 0, 1, 1), pixel, 0, 1);
            Assert.That(pixel[0], Is.EqualTo(Color.Lime));
        }

        [Test]
        public void Clips_SpriteBatch_draws()
        {
            PrepareFrameCapture();

            var spriteBatch = new SpriteBatch(gd);
            var swatch = content.Load<Texture2D>(Paths.Texture("white-64"));

            gd.Clear(Color.CornflowerBlue);
            gd.Viewport = new Viewport(30, 40, 50, 60);

            spriteBatch.Begin();
            spriteBatch.Draw(
                swatch, new Vector2(20, -20), null, Color.Indigo,
                MathHelper.PiOver4, Vector2.Zero, Vector2.One,
                SpriteEffects.None, 0);
            spriteBatch.End();

            CheckFrames();

            spriteBatch.Dispose();
            swatch.Dispose();
        }

        [Test]
        public void Custom_Viewport_is_Preserved_After_Render_Pass_Clear()
        {
            using var renderTarget = new RenderTarget2D(gd,
                200,
                200,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.PreserveContents);

            gd.SetRenderTarget(renderTarget);

            // Set custom letterboxed viewport before clearing the target.
            gd.Viewport = new Viewport(50, 50, 100, 100);
            gd.Clear(Color.Black);

            // Fill custom viewport with orange.
            using var spriteBatch = new SpriteBatch(gd);
            using var orangePixel = new Texture2D(gd, 1, 1);
            orangePixel.SetData(new[] { Color.MonoGameOrange });

            spriteBatch.Begin();
            spriteBatch.Draw(orangePixel, new Rectangle(0, 0, gd.Viewport.Width, gd.Viewport.Height), Color.White);
            spriteBatch.End();

            gd.SetRenderTarget(null);

            var pixels = new Color[200 * 200];
            renderTarget.GetData(pixels);

            // Outside the custom viewport must remain black.
            Assert.AreEqual(Color.Black, pixels[10 * 200 + 10], "Top left letterbox margin must remain black.");
            Assert.AreEqual(Color.Black, pixels[190 * 200 + 190], "Bottom right letterbox margin must remain black.");

            // Inside the custom viewport must be orange.
            Assert.AreEqual(Color.MonoGameOrange, pixels[100 * 200 + 100], "Custom viewport center must be orange.");
        }

        [Test]
        public void Custom_Viewport_And_Scissor_Preserved_After_Command_Buffer_Restart()
        {
            gd.Clear(Color.Black);

            // Configure custom viewport and custom scissor rectangle intersect.
            gd.Viewport = new Viewport(50, 50, 100, 100);
            gd.ScissorRectangle = new Rectangle(70, 0, 60, 200);

            using var rasterizerState = new RasterizerState
            {
                ScissorTestEnable = true,
            };
            gd.RasterizerState = rasterizerState;

            using var effect = new BasicEffect(gd)
            {
                VertexColorEnabled = true
            };
            effect.CurrentTechnique.Passes[0].Apply();

            var blackQuad = new[]
            {
                new VertexPositionColor(new Vector3(-1f, 1f, 0f), Color.Black),
                new VertexPositionColor(new Vector3(1f, 1f, 0f), Color.Black),
                new VertexPositionColor(new Vector3(-1f, -1f, 0f), Color.Black),
                new VertexPositionColor(new Vector3(1f, -1f, 0f), Color.Black),
            };

            // Initial draw flushes all dirty flags and binds the pipeline.
            gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, blackQuad, 0, 2);

            // Reading backbuffer data mid-frame forces backend to:
            // End current render pass,
            // submit and restart command buffer,
            // and only mark renderTargetDirty.
            var dummy = new Color[1];
            gd.GetBackBufferData(new Rectangle(0, 0, 1, 1), dummy, 0, 1);

            var orangeQuad = new[]
            {
                new VertexPositionColor(new Vector3(-1f, 1f, 0f), Color.MonoGameOrange),
                new VertexPositionColor(new Vector3(1f, 1f, 0f), Color.MonoGameOrange),
                new VertexPositionColor(new Vector3(-1f, -1f, 0f), Color.MonoGameOrange),
                new VertexPositionColor(new Vector3(1f, -1f, 0f), Color.MonoGameOrange),
            };

            // Draw again without changing viewport or scissor.
            gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, orangeQuad, 0, 2);

            var width = gd.PresentationParameters.BackBufferWidth;
            var height = gd.PresentationParameters.BackBufferHeight;
            
            var pixels = new Color[width * height];
            gd.GetBackBufferData(pixels);

            Assert.AreEqual(Color.Black,
                pixels[25 * width + 100],
                "Pixel outside custom viewport but inside scissor must remain black.");

            Assert.AreEqual(Color.Black,
                pixels[100 * width + 55],
                "Pixel inside custom viewport but outside custom scissor must remain black.");

            Assert.AreEqual(Color.MonoGameOrange,
                pixels[100 * width + 100],
                "Pixel inside both custom viewport and custom scissor must be orange.");
        }
    }
}
