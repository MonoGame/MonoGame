﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tests.ContentPipeline;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics {
	[TestFixture]
	class SpriteBatchTest : GraphicsDeviceTestFixtureBase {
		private SpriteBatch _spriteBatch;
		private Texture2D _texture;
        private Texture2D _texture2;
        private Texture2D _texture3;
        private BasicEffect _effect;
        private Effect _effect2;

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();

            _spriteBatch = new SpriteBatch (gd);
            _texture = content.Load<Texture2D> (Paths.Texture ("MonoGameIcon"));
            _texture2 = content.Load<Texture2D>(Paths.Texture("Surge"));
            _texture3 = content.Load<Texture2D> (Paths.Texture ("Lines-64"));
            _effect = new BasicEffect(gd)
            {
                VertexColorEnabled = true,
                TextureEnabled = true,
                View = Matrix.Identity,
                World = Matrix.Identity
            };
            var effect2Name = "Grayscale";
#if XNA
            effect2Name = System.IO.Path.Combine("XNA", effect2Name);
#elif WINDOWS
            effect2Name = System.IO.Path.Combine("DirectX", effect2Name);
#endif
            _effect2 = content.Load<Effect>(Paths.Effect(effect2Name));
            
            // use a pretty small rectangle by default for all tests that only draw 1 or 2 sprites
            CaptureRegion = new Rectangle(0, 0, 120, 120);
		}

	    [TearDown]
	    public override void TearDown()
	    {
	        _spriteBatch.Dispose();
            _texture.Dispose();
            _texture2.Dispose();
            _texture3.Dispose();
            _effect.Dispose();
            _effect2.Dispose();
	    }

		[Test]
		public void Draw_without_blend ()
		{
            PrepareFrameCapture();

            _spriteBatch.Begin (SpriteSortMode.Deferred, BlendState.Opaque);
            _spriteBatch.Draw (_texture, new Vector2 (20, 20), Color.White);
            _spriteBatch.End ();

            CheckFrames();
		}

		[Test]
		public void Draw_with_additive_blend ()
		{
            PrepareFrameCapture();

            _spriteBatch.Begin (SpriteSortMode.Deferred, BlendState.Additive);
            _spriteBatch.Draw (_texture, new Vector2 (20, 20), Color.White);
            _spriteBatch.Draw (_texture, new Vector2 (30, 30), Color.White);
            _spriteBatch.End ();

            CheckFrames();
		}

		[Test]
		public void Draw_normal ()
		{
            PrepareFrameCapture();

            _spriteBatch.Begin ();
            _spriteBatch.Draw (_texture, new Vector2 (20, 20), Color.White);
            _spriteBatch.End ();

            CheckFrames();
		}

		[TestCase(0.5f, 0.5f)]
		[TestCase(1.5f, 1.5f)]
		[TestCase(0.75f, 2.0f)]
		[TestCase(1.25f, 0.8f)]
		public void Draw_stretched (float xScale, float yScale)
		{
            CaptureRegion = new Rectangle(0, 0, 200, 200);
            PrepareFrameCapture();

            var rect = new Rectangle (
                30, 50, (int) (_texture.Width * xScale), (int) (_texture.Height * yScale));

            _spriteBatch.Begin ();
            _spriteBatch.Draw (_texture, rect, Color.White);
            _spriteBatch.End ();

            CheckFrames();
		}

		[TestCase("Red")]
		[TestCase("GreenYellow")]
		[TestCase("Teal")]
		public void Draw_with_filter_color (string colorName)
		{
			var color = colorName.ToColor ();
            PrepareFrameCapture();

            _spriteBatch.Begin ();
            _spriteBatch.Draw (_texture, new Vector2 (20, 20), color);
            _spriteBatch.End ();

            CheckFrames();
		}

		[TestCase (0.38f)]
		[TestCase (1.41f)]
		[TestCase (2.17f)]
		[TestCase (2.81f)]
		public void Draw_rotated (float rotation)
		{
            PrepareFrameCapture();

            var position = new Vector2 (50, 50);
            var origin = new Vector2 (_texture.Width / 2, _texture.Height / 2);

            _spriteBatch.Begin ();
            _spriteBatch.Draw (
                _texture, position, null, Color.White,
                rotation, origin, 1, SpriteEffects.None, 0);
            _spriteBatch.End ();

            CheckFrames();
		}

		[Test]
		public void Draw_with_source_rect ()
		{
            PrepareFrameCapture();

            _spriteBatch.Begin ();
            _spriteBatch.Draw (
                _texture, new Vector2 (20, 20),
                new Rectangle (20, 20, 40, 40), Color.White);
            _spriteBatch.End ();

            CheckFrames();
		}

		[TestCase(10, 10, 40, 40)]
		[TestCase(30, 30, 30, 50)]
		[TestCase(20, 30, 80, 60)]
		public void Draw_with_source_and_dest_rect (int x, int y, int width, int height)
		{
            CaptureRegion = new Rectangle(0, 0, 120, 120);
            PrepareFrameCapture();

			var destRect = new Rectangle(x, y, width, height);
			var sourceRect = new Rectangle(20, 20, 40, 40);
            _spriteBatch.Begin ();
            _spriteBatch.Draw (_texture, destRect, sourceRect, Color.White);
            _spriteBatch.End ();

            CheckFrames();
		}

		[TestCase("Red", 120)]
		[TestCase("White", 80)]
		[TestCase("GreenYellow", 200)]
		public void Draw_with_alpha_blending (string colorName, byte alpha)
		{
            PrepareFrameCapture();

			var color = colorName.ToColor();
            color = Color.FromNonPremultiplied(color.R, color.G, color.B, alpha);

            _spriteBatch.Begin ();
            _spriteBatch.Draw (_texture, new Vector2 (20, 20), Color.White);
            _spriteBatch.Draw (_texture, new Vector2 (40, 40), color);
            _spriteBatch.End ();

            CheckFrames();
		}

		[TestCase (SpriteEffects.FlipHorizontally)]
		[TestCase (SpriteEffects.FlipVertically)]
		[TestCase (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically)]
		public void Draw_with_SpriteEffects (SpriteEffects effects)
		{
            PrepareFrameCapture();

            _spriteBatch.Begin ();
            _spriteBatch.Draw (
                _texture, new Vector2(30, 30), null, Color.White,
                0.0f, Vector2.Zero, 1.0f, effects, 0);
            _spriteBatch.End ();

            CheckFrames();
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
            CaptureRegion = new Rectangle(0, 0, 250, 250);
            PrepareFrameCapture();

			var matrix = Matrices [matrixIndex];

            _spriteBatch.Begin (
                SpriteSortMode.Immediate, BlendState.AlphaBlend,
                null, null, null, null, matrix);
            _spriteBatch.Draw (_texture, new Vector2(10, 10), Color.White);
            _spriteBatch.End ();

            CheckFrames();
		}

        [TestCase(SpriteSortMode.BackToFront)]
        [TestCase(SpriteSortMode.Deferred)]
        [TestCase(SpriteSortMode.FrontToBack)]
        [TestCase(SpriteSortMode.Immediate)]
#if !XNA
        // Disabled on XNA because the sorting algorithm is probably different
        [TestCase(SpriteSortMode.Texture)]
#endif
        public void Draw_with_SpriteSortMode(SpriteSortMode sortMode)
        {
            Similarity = 0.995f;
            CaptureRegion = new Rectangle(0, 0, 180, 180);
            PrepareFrameCapture();

            _spriteBatch.Begin(sortMode, BlendState.AlphaBlend);

            _spriteBatch.Draw(_texture, new Vector2(10), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            _spriteBatch.Draw(_texture2, new Vector2(30), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
            _spriteBatch.Draw(_texture3, new Vector2(45), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.3f);
            _spriteBatch.Draw(_texture, new Vector2(60), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
            _spriteBatch.Draw(_texture3, new Vector2(105), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_texture2, new Vector2(90, 90), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.1f);
            _spriteBatch.End();

            CheckFrames();
        }

        [Test]
        public void DrawRequiresTexture()
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            Assert.Throws<ArgumentNullException>(() => _spriteBatch.Draw(null, new Vector2(20, 20), Color.White));
            _spriteBatch.End();
        }

        [Test]
        public void DrawWithTexture()
        {
            Assert.That(gd.Textures[0], Is.Null);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            _spriteBatch.Draw(_texture, new Vector2(20, 20), Color.White);
            _spriteBatch.End();

            Assert.That(gd.Textures[0], Is.SameAs(_texture));
        }

        [Test]
        public void DrawWithCustomEffectAndTwoTextures()
        {
            var customSpriteEffect = AssetTestUtility.CompileEffect(gd, "CustomSpriteBatchEffect.fx");
            var texture2 = new Texture2D(gd, 1, 1, false, SurfaceFormat.Color);

            customSpriteEffect.Parameters["SourceTexture"].SetValue(texture2);
            customSpriteEffect.Parameters["OtherTexture"].SetValue(texture2);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, customSpriteEffect);
            _spriteBatch.Draw(_texture, new Vector2(20, 20), Color.White);
            _spriteBatch.End();

            Assert.That(gd.Textures[0], Is.SameAs(_texture));
            Assert.That(gd.Textures[1], Is.SameAs(texture2));

            customSpriteEffect.Dispose();
            texture2.Dispose();
        }

        [Test]
        public void DrawWithLayerDepth()
        {
            CaptureRegion = new Rectangle(0, 0, 250, 250);
            PrepareFrameCapture();

             // Row 0, column 0: Deferred, no depth test.
            _spriteBatch.Begin();
            _spriteBatch.Draw(
                _texture, new Vector2(30, 30), null, Color.Red,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, -1.0f);
            _spriteBatch.Draw(
                _texture, new Vector2(40, 40), null, Color.Green,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.Draw(
                _texture, new Vector2(50, 50), null, Color.Blue,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            _spriteBatch.Draw(
                _texture, new Vector2(60, 60), null, Color.White,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 2.0f);
            _spriteBatch.End();

            // Row 0, column 1: Deferred, with depth test.
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null);
            _spriteBatch.Draw(
                _texture, new Vector2(130, 30), null, Color.Red,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, -1.0f);
            _spriteBatch.Draw(
                _texture, new Vector2(140, 40), null, Color.Green,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.Draw(
                _texture, new Vector2(150, 50), null, Color.Blue,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            _spriteBatch.Draw(
                _texture, new Vector2(160, 60), null, Color.White,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 2.0f);
            _spriteBatch.End();

            // Row 1, column 0: BackToFront, no depth test.
            _spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, DepthStencilState.None, null);
            _spriteBatch.Draw(
                _texture, new Vector2(30, 130), null, Color.Red,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, -1.0f);
            _spriteBatch.Draw(
                _texture, new Vector2(40, 140), null, Color.Green,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.Draw(
                _texture, new Vector2(50, 150), null, Color.Blue,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            _spriteBatch.Draw(
                _texture, new Vector2(60, 160), null, Color.White,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 2.0f);
            _spriteBatch.End();

            // Row 1, column 1: BackToFront, with depth test.
            _spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, DepthStencilState.Default, null);
            _spriteBatch.Draw(
                _texture, new Vector2(130, 130), null, Color.Red,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, -1.0f);
            _spriteBatch.Draw(
                _texture, new Vector2(140, 140), null, Color.Green,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.Draw(
                _texture, new Vector2(150, 150), null, Color.Blue,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            _spriteBatch.Draw(
                _texture, new Vector2(160, 160), null, Color.White,
                0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 2.0f);
            _spriteBatch.End();

            CheckFrames();
        }

        [Test]
        public void Draw_many()
        {
            CaptureRegion = null;
            PrepareFrameCapture();

            _spriteBatch.Begin();
            for (int x = 0; x < 99; x++)
                for (int y = 0; y < 59; y++)
                    _spriteBatch.Draw(_texture, new Rectangle(4+x*8, 4+y*8, 4, 4), Color.White);
            _spriteBatch.End();

            CheckFrames();
        }
        
        [TestCase(SpriteSortMode.Deferred)]
        [TestCase(SpriteSortMode.Immediate)]
        public void Draw_with_viewport_changing(SpriteSortMode sortMode)
        {
            Similarity = 0.975f;
            CaptureRegion = null;
            PrepareFrameCapture();

            // Test SpriteEffect
            var vp = gd.Viewport; 
            var lvp = new Viewport (vp.X, vp.Y, vp.Width/3, vp.Height); //Left
            var mvp = new Viewport(vp.X + vp.Width / 3, vp.Y, vp.Width / 3, vp.Height); //middle
            var rvp = new Viewport(vp.X+(vp.Width /3)*2, vp.Y, vp.Width / 3, vp.Height); //Right

            // Test viewport change
            gd.Viewport = rvp;
            _spriteBatch.Begin(sortMode, BlendState.AlphaBlend);
            _spriteBatch.Draw(_texture, new Vector2(10, 10), null, Color.White);
            gd.Viewport = mvp;
            _spriteBatch.Draw(_texture2, new Vector2(70, 10), null, Color.White);
            gd.Viewport = lvp;
            _spriteBatch.Draw(_texture3, new Vector2(130, 10), null, Color.White);
            gd.Viewport = vp;
            _spriteBatch.Draw(_texture2, new Vector2(190, 10), null, Color.White);
            _spriteBatch.End();

            // Test viewport/effect BasicEffect (Vertex & Pixel shader)
#if DIRECTX
            Matrix halfPixelOffset = Matrix.Identity;
#else            
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
#endif

            _effect.Projection = halfPixelOffset * Matrix.CreateOrthographicOffCenter(0, gd.Viewport.Width, gd.Viewport.Height, 0, 0, 1);
            
            gd.Viewport = rvp;
            _spriteBatch.Begin(sortMode, BlendState.AlphaBlend, null, null, null, _effect);
            _spriteBatch.Draw(_texture, new Vector2(10, 110), null, Color.White);
            gd.Viewport = mvp;
            _spriteBatch.Draw(_texture2, new Vector2(70, 110), null, Color.White);
            gd.Viewport = lvp;
            _spriteBatch.Draw(_texture3, new Vector2(130, 110), null, Color.White);
            gd.Viewport = vp;
            _spriteBatch.Draw(_texture2, new Vector2(190, 110), null, Color.White);
            _spriteBatch.End();

            // Test BasicEffect (Vertex & Pixel shader)
            // re-apply projection when viewport dimensions change
            gd.Viewport = rvp;
            _effect.Projection = halfPixelOffset * Matrix.CreateOrthographicOffCenter(0, gd.Viewport.Width, gd.Viewport.Height, 0, 0, 1);
            _spriteBatch.Begin(sortMode, BlendState.AlphaBlend, null, null, null, _effect);
            _spriteBatch.Draw(_texture, new Vector2(10, 210), null, Color.White);
            gd.Viewport = mvp;
            _spriteBatch.Draw(_texture2, new Vector2(70, 210), null, Color.White);
            gd.Viewport = lvp;
            _spriteBatch.Draw(_texture3, new Vector2(130, 210), null, Color.White);
            gd.Viewport = vp;
            _effect.Projection = halfPixelOffset * Matrix.CreateOrthographicOffCenter(0, gd.Viewport.Width, gd.Viewport.Height, 0, 0, 1);
            _spriteBatch.Draw(_texture2, new Vector2(190, 210), null, Color.White);
            _spriteBatch.End();
            
            // TODO: test custom Effect with no Vertex shader
            gd.Viewport = rvp;
            _spriteBatch.Begin(sortMode, BlendState.AlphaBlend, null, null, null, _effect2);
            _spriteBatch.Draw(_texture, new Vector2(10, 310), null, Color.White);
            gd.Viewport = mvp;
            _spriteBatch.Draw(_texture2, new Vector2(70, 310), null, Color.White);
            gd.Viewport = lvp;
            _spriteBatch.Draw(_texture3, new Vector2(130, 310), null, Color.White);
            gd.Viewport = vp;
            _spriteBatch.Draw(_texture2, new Vector2(190, 310), null, Color.White);
            _spriteBatch.End();

            CheckFrames();
        }
    }
}
