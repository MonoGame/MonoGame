using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NUnit.Framework;

namespace MonoGame.Tests.Graphics {
	class ViewportTest : GraphicsDeviceTestFixtureBase {
		[Test]
		public void Affects_draw_origin ()
		{
            PrepareFrameCapture();

            var spriteBatch = new SpriteBatch (gd);
            var swatch = content.Load<Texture2D> (Paths.Texture ("white-64"));

            gd.Clear (Color.CornflowerBlue);
            gd.Viewport = new Viewport (20, 40, 100, 100);

            spriteBatch.Begin ();
            spriteBatch.Draw (swatch, new Vector2(10, 20), Color.GreenYellow);
            spriteBatch.End ();

            CheckFrames();

            spriteBatch.Dispose();
            swatch.Dispose();
		}

		[Test]
		public void Does_not_clip_device_clear ()
		{
            PrepareFrameCapture();

            var presentParams = gd.PresentationParameters;
            gd.Viewport = new Viewport (
                0, 0,
                presentParams.BackBufferWidth,
                presentParams.BackBufferHeight);
            gd.Clear (Color.CornflowerBlue);

            gd.Viewport = new Viewport (30, 40, 100, 200);
            gd.Clear (Color.Red);

            CheckFrames();
		}

		[Test]
		public void Clips_SpriteBatch_draws ()
		{
            PrepareFrameCapture();

            var spriteBatch = new SpriteBatch (gd);
            var swatch = content.Load<Texture2D> (Paths.Texture ("white-64"));

            gd.Clear (Color.CornflowerBlue);
            gd.Viewport = new Viewport (30, 40, 50, 60);

            spriteBatch.Begin ();
            spriteBatch.Draw (
                swatch, new Vector2 (20, -20), null, Color.Indigo,
                MathHelper.PiOver4, Vector2.Zero, Vector2.One,
                SpriteEffects.None, 0);
            spriteBatch.End ();

            CheckFrames();

            spriteBatch.Dispose();
            swatch.Dispose();
		}
	}
}
