using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NUnit.Framework;

using MonoGame.Tests.Components;

namespace MonoGame.Tests.Visual {
	class ViewportTest : VisualTestFixtureBase {
		[Test]
		public void Affects_draw_origin ()
		{
			SpriteBatch spriteBatch = null;
			Texture2D swatch = null;

			Game.LoadContentWith += (sender, e) => {
				spriteBatch = new SpriteBatch (Game.GraphicsDevice);
				swatch = Game.Content.Load<Texture2D> (Paths.Texture ("white-64"));
			};

			Game.UnloadContentWith += (sender, e) => {
				spriteBatch.Dispose ();
			};

			Game.DrawWith += (sender, e) => {
				Game.GraphicsDevice.Clear (Color.CornflowerBlue);
				Game.GraphicsDevice.Viewport = new Viewport (20, 40, 100, 100);

				spriteBatch.Begin ();
				spriteBatch.Draw (swatch, new Vector2(10, 20), Color.GreenYellow);
				spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Does_not_clip_device_clear ()
		{
			Game.DrawWith += (sender, e) => {
				var presentParams = Game.GraphicsDevice.PresentationParameters;
				Game.GraphicsDevice.Viewport = new Viewport (
					0, 0,
					presentParams.BackBufferWidth,
					presentParams.BackBufferHeight);
				Game.GraphicsDevice.Clear (Color.CornflowerBlue);

				Game.GraphicsDevice.Viewport = new Viewport (30, 40, 100, 200);
				Game.GraphicsDevice.Clear (Color.Red);
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Clips_SpriteBatch_draws ()
		{
			SpriteBatch spriteBatch = null;
			Texture2D swatch = null;

			Game.LoadContentWith += (sender, e) => {
				spriteBatch = new SpriteBatch (Game.GraphicsDevice);
				swatch = Game.Content.Load<Texture2D> (Paths.Texture ("white-64"));
			};

			Game.UnloadContentWith += (sender, e) => {
				spriteBatch.Dispose ();
			};

			Game.DrawWith += (sender, e) => {
				Game.GraphicsDevice.Clear (Color.CornflowerBlue);
				Game.GraphicsDevice.Viewport = new Viewport (30, 40, 50, 60);

				spriteBatch.Begin ();
				spriteBatch.Draw (
					swatch, new Vector2 (20, -20), null, Color.Indigo,
					MathHelper.PiOver4, Vector2.Zero, Vector2.One,
					SpriteEffects.None, 0);
				spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}
	}
}
