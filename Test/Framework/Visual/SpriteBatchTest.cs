using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NUnit.Framework;

namespace MonoGame.Tests.Visual {
	[TestFixture]
	class SpriteBatchTest : VisualTestFixtureBase {
		private SpriteBatch _spriteBatch;
		private Texture2D _texture;

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();

			Game.LoadContentWith += (sender, e) => {
				_spriteBatch = new SpriteBatch (Game.GraphicsDevice);
				_texture = Game.Content.Load<Texture2D> (Paths.Texture ("MonoGameIcon"));
			};

			Game.UnloadContentWith += (sender, e) => {
				_spriteBatch.Dispose ();
				_spriteBatch = null;
				_texture = null;
			};

			Game.PreDrawWith += (sender, e) => {
				Game.GraphicsDevice.Clear (Color.CornflowerBlue);
			};
		}

		[Test]
		public void Draw_without_blend ()
		{
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin (SpriteSortMode.Deferred, BlendState.Opaque);
				_spriteBatch.Draw (_texture, new Vector2 (20, 20), Color.White);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Draw_with_additive_blend ()
		{
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin (SpriteSortMode.Deferred, BlendState.Additive);
				_spriteBatch.Draw (_texture, new Vector2 (20, 20), Color.White);
				_spriteBatch.Draw (_texture, new Vector2 (30, 30), Color.White);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Draw_normal ()
		{
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				_spriteBatch.Draw (_texture, new Vector2 (20, 20), Color.White);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[TestCase(0.5f, 0.5f)]
		[TestCase(1.5f, 1.5f)]
		[TestCase(0.75f, 2.0f)]
		[TestCase(1.25f, 0.8f)]
		public void Draw_stretched (float xScale, float yScale)
		{
			Game.DrawWith += (sender, e) => {
				var rect = new Rectangle (
					30, 50, (int) (_texture.Width * xScale), (int) (_texture.Height * yScale));

				_spriteBatch.Begin ();
				_spriteBatch.Draw (_texture, rect, Color.White);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[TestCase("Red")]
		[TestCase("GreenYellow")]
		[TestCase("Teal")]
		public void Draw_with_filter_color (string colorName)
		{
			var color = colorName.ToColor ();
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				_spriteBatch.Draw (_texture, new Vector2 (20, 20), color);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[TestCase (0.38f)]
		[TestCase (1.41f)]
		[TestCase (2.17f)]
		[TestCase (2.81f)]
		public void Draw_rotated (float rotation)
		{
			Game.DrawWith += (sender, e) => {
				var position = new Vector2 (50, 50);
				var origin = new Vector2 (_texture.Width / 2, _texture.Height / 2);

				_spriteBatch.Begin ();
				_spriteBatch.Draw (
					_texture, position, null, Color.White,
					rotation, origin, 1, SpriteEffects.None, 0);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Draw_with_source_rect ()
		{
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				_spriteBatch.Draw (
					_texture, new Vector2 (20, 20),
					new Rectangle (20, 20, 40, 40), Color.White);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[TestCase(10, 10, 40, 40)]
		[TestCase(30, 30, 30, 50)]
		[TestCase(20, 30, 80, 60)]
		public void Draw_with_source_and_dest_rect (int x, int y, int width, int height)
		{
			var destRect = new Rectangle(x, y, width, height);
			var sourceRect = new Rectangle(20, 20, 40, 40);
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				_spriteBatch.Draw (_texture, destRect, sourceRect, Color.White);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[TestCase("Red", 120)]
		[TestCase("White", 80)]
		[TestCase("GreenYellow", 200)]
		public void Draw_with_alpha_blending (string colorName, byte alpha)
		{
			var color = colorName.ToColor();
			color.A = alpha;

			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				_spriteBatch.Draw (_texture, new Vector2 (20, 20), Color.White);
				_spriteBatch.Draw (_texture, new Vector2 (40, 40), color);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[TestCase (SpriteEffects.FlipHorizontally)]
		[TestCase (SpriteEffects.FlipVertically)]
		[TestCase (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically)]
		public void Draw_with_SpriteEffects (SpriteEffects effects)
		{
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				_spriteBatch.Draw (
					_texture, new Vector2(30, 30), null, Color.White,
					0.0f, Vector2.Zero, 1.0f, effects, 0);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		private static readonly Matrix [] Matrices = new Matrix [] {
			Matrix.Identity,
			Matrix.CreateRotationZ(0.25f),
			Matrix.CreateScale(2),
			Matrix.CreateTranslation(30, 40, 0),
			Matrix.CreateRotationZ(0.9f) * Matrix.CreateScale(2) * Matrix.CreateTranslation(128, 32, 0)
		};

		// Note that [Range(0, Matrices.Length -1)] is in use here,
		// rather then [TestCaseSource("Matrices")].  Passing the matrix
		// in directly results in an enormous test name (and captured
		// image filename).
		[Test]
		public void Draw_with_matrix ([Range(0, 4)]int matrixIndex)
		{
			var matrix = Matrices [matrixIndex];

			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin (
					SpriteSortMode.Immediate, BlendState.AlphaBlend,
					null, null, null, null, matrix);
				_spriteBatch.Draw (_texture, new Vector2(10, 10), Color.White);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		// FIXME: This scissoring code is not valid in XNA. It
		//        complains about RasterizerState being
		//        immutable after it's bound to a
		//        GraphicsDevice.  MonoGame probably should to,
		//        rather than allowing mutation.

		// Now let's try some scissoring
		//_spriteBatch.Begin ();
		//_spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle (50, 40, (int) _clippingSize, (int) _clippingSize);
		//_spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
		//_spriteBatch.Draw (_texture, new Rectangle (50, 40, 320, 40), Color.White);
		//_spriteBatch.DrawString (_font, "Scissor Clipping Test", new Vector2 (50, 40), Color.Red);
		//_spriteBatch.End ();

		//_spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = false;
	}
}
