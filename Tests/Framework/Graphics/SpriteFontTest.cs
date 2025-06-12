// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NUnit.Framework;

namespace MonoGame.Tests.Graphics {
	[TestFixture]
    [NonParallelizable]
	class SpriteFontTest : GraphicsDeviceTestFixtureBase {

		private SpriteBatch _spriteBatch;
		private SpriteFont _defaultFont;

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();

            _spriteBatch = new SpriteBatch (gd);
            _defaultFont = content.Load<SpriteFont> (Paths.Font ("Default"));
		}

	    [TearDown]
	    public override void TearDown()
	    {
            _spriteBatch.Dispose();
	        _spriteBatch = null;

	        base.TearDown();
	    }

        [Test]
        [TestCase("Default", "The quick brown fox jumps over the lazy dog. 1234567890", 605, 21)]
        [TestCase("Default", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 275, 59)]
        [TestCase("Default", "The quick brown fox jumps over the lazy dog.\r1234567890", 594, 21)]
        [TestCase("DataFont", "The quick brown fox jumps over the lazy dog. 1234567890", 417, 19)]
        [TestCase("DataFont", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 196, 53)]
        [TestCase("JingJing", "The quick brown fox jumps over the lazy dog. 1234567890", 918, 45)]
        [TestCase("JingJing", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 435, 135)]
        [TestCase("JingJing", "%", 21, 45)] // LSB=2, W=17, RSB=2
        [TestCase("JingJing", "*", 10, 45)] // LSB=0, W=10, RSB=-1
        [TestCase("Lindsey", "The quick brown fox jumps over the lazy dog. 1234567890", 1031, 49)]
        [TestCase("Lindsey", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 454, 139)]
        [TestCase("Lindsey", "B", 25, 49)] // LSB=-3, W=24, RSB=1
        [TestCase("Motorwerk", "The quick brown fox jumps over the lazy dog. 1234567890", 932, 44)]
        [TestCase("Motorwerk", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 441, 124)]
        [TestCase("Motorwerk", " ", 12, 44)] // LSB=0, W=1, RSB=11
        [TestCase("Motorwerk", "(", 18, 44)] // LSB=3, W=15, RSB=-6
        [TestCase("Motorwerk", ")", 14, 44)] // LSB=-1, W=14, RSB=-1
        [TestCase("Motorwerk", "_", 15, 44)] // LSB=-2, W=15, RSB=0
        [TestCase("QuartzMS", "The quick brown fox jumps over the lazy dog. 1234567890", 947, 39)]
        [TestCase("QuartzMS", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 440, 111)]
        [TestCase("QuartzMS", "#", 20, 39)] // LSB=0, W=20, RSB=0
        [TestCase("SegoeKeycaps", "The quick brown fox jumps over the lazy dog. 1234567890", 988, 20)]
        [TestCase("SegoeKeycaps", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 448, 58)]
        [TestCase("SegoeKeycaps", "!", 16, 20)] // LSB=1, W=15, RSB=0
        [RunOnUI]
        public void MeasureString_returns_correct_values(string fontName, string text, float width, float height)
        {
            var font = game.Content.Load<SpriteFont>(Paths.Font(fontName));
            var actualSize = font.MeasureString(text);
            var expectedSize = new Vector2(width, height);
            Assert.That(actualSize, Is.EqualTo(expectedSize).Using(Vector2Comparer.Epsilon));
        }

		[Test]
        [RunOnUI]
		public void Plain ()
		{
            PrepareFrameCapture();

            _spriteBatch.Begin ();
            // Row 0: String test.
            _spriteBatch.DrawString (
                _defaultFont, "plain old text", new Vector2 (50, 50), Color.Violet);
            // Row 1: StringBuilder test.
            _spriteBatch.DrawString (
                _defaultFont, new StringBuilder("plain old text"), new Vector2 (50, 290), Color.Violet);
            _spriteBatch.End ();

            CheckFrames();
		}

		[Test]
        [RunOnUI]
		public void Rotated ()
		{
            PrepareFrameCapture();

            _spriteBatch.Begin ();
            // Row 0: String test.
            _spriteBatch.DrawString (
                _defaultFont, "rotated", new Vector2 (50, 50), Color.Orange,
                MathHelper.PiOver4, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);            
            _spriteBatch.DrawString (
                _defaultFont, "rotated", new Vector2 (450, 50), Color.Orange,
                MathHelper.PiOver4, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
            // Row 1: StringBuilder test.
            _spriteBatch.DrawString (
                _defaultFont, new StringBuilder("rotated"), new Vector2 (50, 290), Color.Orange,
                MathHelper.PiOver4, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.DrawString (
                _defaultFont, new StringBuilder("rotated"), new Vector2 (450, 290), Color.Orange,
                MathHelper.PiOver4, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
            _spriteBatch.End ();

            CheckFrames();
		}

		[Test]
        [RunOnUI]
		public void Scaled ()
		{
            PrepareFrameCapture();

            _spriteBatch.Begin ();
            // Row 0: String test.
            _spriteBatch.DrawString (
                _defaultFont, "scaled", new Vector2 (50, 50), Color.Orange,
                0, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.DrawString (
                _defaultFont, "scaled", new Vector2 (450, 50), Color.Orange,
                0, Vector2.Zero, new Vector2(3.0f, 1.5f), SpriteEffects.None, 0.0f);
            // Row 1: StringBuilder test.
            _spriteBatch.DrawString (
                _defaultFont, new StringBuilder("scaled"), new Vector2 (50, 290), Color.Orange,
                0, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.DrawString (
                _defaultFont, new StringBuilder("scaled"), new Vector2 (450, 290), Color.Orange,
                0, Vector2.Zero, new Vector2(3.0f, 1.5f), SpriteEffects.None, 0.0f);
            _spriteBatch.End ();

            CheckFrames();
		}

        [Test]
		[TestCase(SpriteEffects.FlipHorizontally)]
		[TestCase(SpriteEffects.FlipVertically)]
		[TestCase(SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically)]
        [RunOnUI]
		public void Draw_with_SpriteEffects (SpriteEffects effects)
		{
            PrepareFrameCapture();

            _spriteBatch.Begin ();
            // Row 0: String test.
            _spriteBatch.DrawString (
                _defaultFont, effects.ToString(), new Vector2 (50, 50), Color.Orange,
                0, Vector2.Zero, 1f, effects, 0.0f);            
            _spriteBatch.DrawString (
                _defaultFont, effects.ToString(), new Vector2 (450, 50), Color.Orange,
                0, Vector2.Zero, Vector2.One, effects, 0.0f);
            // Row 1: StringBuilder test.
            _spriteBatch.DrawString (
                _defaultFont, new StringBuilder(effects.ToString()), new Vector2 (50, 290), Color.Orange,
                0, Vector2.Zero, 1f, effects, 0.0f);            
            _spriteBatch.DrawString (
                _defaultFont, new StringBuilder(effects.ToString()), new Vector2 (450, 290), Color.Orange,
                0, Vector2.Zero, Vector2.One, effects, 0.0f);
            _spriteBatch.End ();

            CheckFrames();
		}

		[Test]
        [RunOnUI]
		public void Origins_rotated ()
		{
            PrepareFrameCapture();

            _spriteBatch.Begin ();

            var position = new Vector2 (100, 100);
            var text = "origin";

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.Orange, MathHelper.PiOver4,
                new Vector2(0f, 0f), 1.0f, SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.Blue, MathHelper.PiOver4,
                new Vector2(40f, 0f), 1.0f, SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.HotPink, MathHelper.PiOver4,
                new Vector2(0f, 40f), 1.0f, SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.Violet, MathHelper.PiOver4,
                new Vector2(40f, 40f), 1.0f, SpriteEffects.None, 0.0f);

            position = new Vector2 (500, 100);

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.Orange, MathHelper.PiOver4,
                new Vector2(0f, 0f), new Vector2(1.0f, 1.0f), SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.Blue, MathHelper.PiOver4,
                new Vector2(40f, 0f), new Vector2(1.0f, 1.0f), SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.HotPink, MathHelper.PiOver4,
                new Vector2(0f, 40f), new Vector2(1.0f, 1.0f), SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.Violet, MathHelper.PiOver4,
                new Vector2(40f, 40f), new Vector2(1.0f, 1.0f), SpriteEffects.None, 0.0f);

            _spriteBatch.End ();

            CheckFrames();
		}

		[Test]
        [RunOnUI]
		public void Origins_scaled ()
		{
            PrepareFrameCapture();

            _spriteBatch.Begin ();

            var position = new Vector2 (100, 100);
            var text = "origin";

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.Orange, 0,
                new Vector2(0f, 0f), 0.5f, SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.Blue, 0,
                new Vector2(40f, 0f), 2.0f, SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.HotPink, 0,
                new Vector2(0f, 40f), 0.75f, SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.Violet, 0,
                new Vector2(40f, 40f), 1.0f, SpriteEffects.None, 0.0f);
            
            position = new Vector2 (500, 100);
            
            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.Orange, 0,
                new Vector2(0f, 0f), new Vector2(2.0f, 0.5f), SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.Blue, 0,
                new Vector2(40f, 0f), new Vector2(2.0f, 2.0f), SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.HotPink, 0,
                new Vector2(0f, 40f), new Vector2(2.0f, 0.75f), SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString (
                _defaultFont, text, position, Color.Violet, 0,
                new Vector2(40f, 40f), new Vector2(2.0f, 1.0f), SpriteEffects.None, 0.0f);

            _spriteBatch.End ();

            CheckFrames();
		}
        
		[Test]
        [RunOnUI]
		public void Draw_with_LayerDepth()
		{
            PrepareFrameCapture();

            var text = "depth";

            // Row 0, column 0: Deferred, no depth test.
            _spriteBatch.Begin();
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(30, 30), Color.Red,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.8f);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(40, 40), Color.Green,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(40, 40), Color.Blue,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.3f);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(60, 60), Color.White,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 1.0f);
            _spriteBatch.End();

            // Row 0, column 1: Deferred, with depth test.
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(330, 30), Color.Red,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.8f);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(340, 40), Color.Green,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(350, 50), Color.Blue,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.3f);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(360, 60), Color.White,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 1.0f);
            _spriteBatch.End();

            // Row 1, column 0: BackToFront, no depth test.
            _spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, DepthStencilState.None, null);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(30, 130), Color.Red,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.8f);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(40, 140), Color.Green,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(50, 150), Color.Blue,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.3f);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(60, 160), Color.White,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 1.0f);
            _spriteBatch.End();

            // Row 1, column 1: BackToFront, with depth test.
            _spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, DepthStencilState.Default, null);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(330, 130), Color.Red,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.8f);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(340, 140), Color.Green,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(350, 150), Color.Blue,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.3f);
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2(360, 160), Color.White,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 1.0f);
            _spriteBatch.End();
                        
            var sb = new StringBuilder(text);
            // Row 2, column 0: Deferred, no depth test, StringBuilder.
            _spriteBatch.Begin();
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(30, 270), Color.Red,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.8f);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(40, 280), Color.Green,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(40, 280), Color.Blue,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.3f);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(60, 300), Color.White,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 1.0f);
            _spriteBatch.End();

            // Row 2, column 1: Deferred, with depth test, StringBuilder.
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(330, 270), Color.Red,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.8f);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(340, 280), Color.Green,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(350, 280), Color.Blue,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.3f);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(360, 300), Color.White,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 1.0f);
            _spriteBatch.End();

            // Row 3, column 0: BackToFront, no depth test, StringBuilder.
            _spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, DepthStencilState.None, null);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(30, 370), Color.Red,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.8f);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(40, 380), Color.Green,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(50, 380), Color.Blue,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.3f);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(60, 400), Color.White,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 1.0f);
            _spriteBatch.End();

            // Row 3, column 1: BackToFront, with depth test, StringBuilder.
            _spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, DepthStencilState.Default, null);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(330, 370), Color.Red,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.8f);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(340, 380), Color.Green,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.0f);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(350, 380), Color.Blue,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0.3f);
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2(360, 400), Color.White,
                0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 1.0f);
            _spriteBatch.End();

            CheckFrames();
		}
        
		[Test]
        [RunOnUI]
		public void Hullabaloo ()
		{
            PrepareFrameCapture();

            _spriteBatch.Begin ();
            _spriteBatch.DrawString (
                _defaultFont, "hullabaloo", new Vector2 (100, 150), Color.HotPink,
                MathHelper.ToRadians(15), new Vector2(20f, 50f), new Vector2(0.8f, 1.1f),
                SpriteEffects.FlipHorizontally, 0.0f);
            _spriteBatch.End ();

            CheckFrames();
		}

		[Test]
        [RunOnUI]
		public void Hullabaloo2 ()
		{
            PrepareFrameCapture();

            _spriteBatch.Begin ();
            _spriteBatch.DrawString (
                _defaultFont, "hullabaloo2", new Vector2 (100, 150), Color.Yellow,
                MathHelper.ToRadians(130), new Vector2(40f, 60f), new Vector2(1.8f, 1.1f),
                SpriteEffects.FlipVertically, 0.0f);
            _spriteBatch.End ();

            CheckFrames();
		}

		[Test]
        [TestCase("The quick brown fox jumps over the lazy dog. 1234567890", TestName = "Multiline_noNewline")]
        [TestCase("The quick brown fox jumps\nover the lazy dog.\n1234567890", TestName = "Multiline_Newline")]
        [TestCase("The quick brown fox jumps over the lazy dog.\r1234567890", TestName = "Multiline_CarriageReturn")]
        [TestCase(@"A programming genius called Hugh
Said 'I really must see if it's true.'
So he wrote a routine
To ask 'What's it all mean?'
But the answer was still '42'.
                R Humphries, Sutton Coldfield", TestName = "Multiline_verbatimString")]
        [RunOnUI]
		public void Multiline (string text)
		{
            PrepareFrameCapture();

            _spriteBatch.Begin ();
            
            // Row 0: String test.
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2 (10, 10), Color.LightGreen);
                        
            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2 (100, 150), Color.Yellow,
                MathHelper.ToRadians (20), new Vector2 (40f, 60f), 0.9f,
                SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString(
                _defaultFont, text, new Vector2 (500, 150), Color.Blue,
                MathHelper.ToRadians (20), new Vector2 (40f, 60f), new Vector2 (0.4f, 0.8f),
                SpriteEffects.None, 0.0f);

            // Row 1: StringBuilder test.
            var sb = new StringBuilder(text);                         
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2 (10, 250), Color.LightGreen);
                        
            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2 (100, 390), Color.Yellow,
                MathHelper.ToRadians (20), new Vector2 (40f, 60f), 0.9f,
                SpriteEffects.None, 0.0f);

            _spriteBatch.DrawString(
                _defaultFont, sb, new Vector2 (500, 390), Color.Blue,
                MathHelper.ToRadians (20), new Vector2 (40f, 60f), new Vector2 (0.4f, 0.8f),
                SpriteEffects.None, 0.0f);

            _spriteBatch.End ();

            Similarity = 0.985f;
            CheckFrames();
		}

		[Test]
        [RunOnUI]
		public void Font_spacing_is_respected ()
		{
            PrepareFrameCapture();
			// DataFont has a non-zero Spacing property.
			var font = content.Load<SpriteFont> (Paths.Font ("DataFont"));
            var text = "Now is the time for all good DataFonts";

            _spriteBatch.Begin ();
            // Row 0: String test.
            _spriteBatch.DrawString (font, text, new Vector2 (50, 50), Color.Violet);
            _spriteBatch.DrawString (font, text, new Vector2 (50, 70), Color.Violet, 
                1f, Vector2.Zero, 1f, SpriteEffects.None, 0);
            _spriteBatch.DrawString (font, text, new Vector2 (50, 90), Color.Violet,
                1f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            // Row 1: StringBuilder test.            
            var sb = new StringBuilder(text);
            _spriteBatch.DrawString (font, sb, new Vector2 (50, 290), Color.Violet);
            _spriteBatch.DrawString (font, sb, new Vector2 (50, 310), Color.Violet, 
                1f, Vector2.Zero, 1f, SpriteEffects.None, 0);            
            _spriteBatch.DrawString (font, sb, new Vector2 (50, 330), Color.Violet,
                1f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            _spriteBatch.End ();

            CheckFrames();
		}

        [Test]
        [TestCase("The rain in España stays mainly in the plain - now in français")]
        [TestCase("\x1f")]
        [TestCase("\x7f")]
        [RunOnUI]
        public void Throws_when_drawing_unavailable_characters(string text)
		{
            _spriteBatch.Begin ();
            Assert.Throws<ArgumentException> (() =>
                _spriteBatch.DrawString (_defaultFont, text, Vector2.Zero, Color.Violet));
            _spriteBatch.End ();
		}

        [Test]
        [TestCase('ñ')]
        [TestCase((char)127)]
        [TestCase((char)31)]
        [RunOnUI]
        public void Throws_when_setting_unavailable_DefaultCharacter(char character)
		{
            Assert.Throws<ArgumentException> (() =>
                _defaultFont.DefaultCharacter = character);
		}

        [Test]
        [TestCase((char)32)]
        [TestCase((char)63)]
        [TestCase((char)126)]
        [RunOnUI]
        public void Does_not_throw_when_setting_available_DefaultCharacter(char character)
        {
            Assert.DoesNotThrow(() => _defaultFont.DefaultCharacter = character);
        }
	}
}
