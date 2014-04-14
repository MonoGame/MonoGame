#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NUnit.Framework;

using MonoGame.Tests.Components;

namespace MonoGame.Tests.Visual {
	[TestFixture]
	class SpriteFontTest : VisualTestFixtureBase {

		private SpriteBatch _spriteBatch;
		private SpriteFont _defaultFont;

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();

			Game.LoadContentWith += (sender, e) => {
				_spriteBatch = new SpriteBatch (Game.GraphicsDevice);
				_defaultFont = Game.Content.Load<SpriteFont> (Paths.Font ("Default"));
			};

			Game.UnloadContentWith += (sender, e) => {
				_spriteBatch.Dispose ();
				_spriteBatch = null;

				_defaultFont = null;
			};

			Game.PreDrawWith += (sender, e) => {
				Game.GraphicsDevice.Clear (Color.CornflowerBlue);
			};
		}

		[Test]
		public void Plain ()
		{
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				_spriteBatch.DrawString (
					_defaultFont, "plain old text", new Vector2 (50, 50), Color.Violet);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Rotated ()
		{
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				_spriteBatch.DrawString (
					_defaultFont, "rotated", new Vector2 (50, 50), Color.Orange,
					MathHelper.PiOver4, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Scaled ()
		{
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				_spriteBatch.DrawString (
					_defaultFont, "scaled", new Vector2 (50, 50), Color.Orange,
					0, Vector2.Zero, new Vector2(3.0f, 1.5f), SpriteEffects.None, 0.0f);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[TestCase(SpriteEffects.FlipHorizontally)]
		[TestCase(SpriteEffects.FlipVertically)]
		[TestCase(SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically)]
		public void Draw_with_SpriteEffects (SpriteEffects effects)
		{
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				_spriteBatch.DrawString (
					_defaultFont, effects.ToString(), new Vector2 (50, 50), Color.Orange,
					0, Vector2.Zero, Vector2.One, effects, 0.0f);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Origins_rotated ()
		{
			Game.DrawWith += (sender, e) => {
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

				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Origins_scaled ()
		{
			Game.DrawWith += (sender, e) => {
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

				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Hullabaloo ()
		{
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				_spriteBatch.DrawString (
					_defaultFont, "hullabaloo", new Vector2 (100, 150), Color.HotPink,
					MathHelper.ToRadians(15), new Vector2(20f, 50f), new Vector2(0.8f, 1.1f),
					SpriteEffects.FlipHorizontally, 0.0f);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Hullabaloo2 ()
		{
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				_spriteBatch.DrawString (
					_defaultFont, "hullabaloo2", new Vector2 (100, 150), Color.Yellow,
					MathHelper.ToRadians(130), new Vector2(40f, 60f), new Vector2(1.8f, 1.1f),
					SpriteEffects.FlipVertically, 0.0f);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Multiline ()
		{
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();

				var text =
@"A programming genius called Hugh
Said 'I really must see if it's true.'
So he wrote a routine
To ask 'What's it all mean?'
But the answer was still '42'.
                R Humphries, Sutton Coldfield";

				_spriteBatch.DrawString (
					_defaultFont, text, new Vector2 (100, 150), Color.Yellow,
					MathHelper.ToRadians (20), new Vector2 (40f, 60f), new Vector2 (0.9f, 0.9f),
					SpriteEffects.None, 0.0f);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Font_spacing_is_respected ()
		{
			// DataFont has a non-zero Spacing property.
			SpriteFont font = null;
			Game.LoadContentWith += (sender, e) => {
				font = Game.Content.Load<SpriteFont> (Paths.Font ("DataFont"));
			};

			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				_spriteBatch.DrawString (
					font, "Now is the time for all good DataFonts",
					new Vector2 (50, 50), Color.Violet);
				_spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}

		[Test]
		public void Throws_when_drawing_unavailable_characters ()
		{
			const string text = "The rain in España stays mainly in the plain - now in français";
			Game.DrawWith += (sender, e) => {
				_spriteBatch.Begin ();
				Assert.Throws<ArgumentException> (() =>
					_spriteBatch.DrawString (_defaultFont, text, Vector2.Zero, Color.Violet));
				_spriteBatch.End ();
			};

			Game.Run ();
		}
	}
}
