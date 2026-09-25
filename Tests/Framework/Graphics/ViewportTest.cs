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
    }
}
