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

namespace MonoGame.Tests.Components {
	class ImplicitDrawOrderComponent : VisualTestDrawableGameComponent {
		private const int NumberOfBatches = 3;
		private const int ItemsPerBatch = 3;
		private int _batchNumber = 0;

		private List<TestDrawable> _drawables = new List<TestDrawable> ();
		private List<TestDrawable> _drawablesInDrawOrder = new List<TestDrawable> ();
		private bool _drawablesOrderedCorrectly;

		private SpriteBatch _spriteBatch;
		private SpriteFont _font;

		public ImplicitDrawOrderComponent (Game game)
			: base (game)
		{
			UpdateOrder = 100;
			DrawOrder = 100;
		}

		protected override void LoadContent ()
		{
			base.LoadContent ();
			_spriteBatch = new SpriteBatch (GraphicsDevice);
			_font = Game.Content.Load<SpriteFont> (Paths.Font("Default"));
		}

		protected override void UnloadContent ()
		{
			if (_spriteBatch != null) {
				_spriteBatch.Dispose ();
				_spriteBatch = null;
			}
			_font = null;
			base.UnloadContent ();
		}

		protected override void UpdateOncePerDraw (GameTime gameTime)
		{
			base.UpdateOncePerDraw (gameTime);

			if (_batchNumber < NumberOfBatches) {
				for (int i = 0; i < ItemsPerBatch; ++i) {
					var drawable = new TestDrawable (Game, this, _drawables.Count + 1);
					_drawables.Add (drawable);
					Game.Components.Add (drawable);
				}
				_batchNumber++;
			}
		}

		public override void Draw (GameTime gameTime)
		{
			base.Draw (gameTime);
			_drawablesOrderedCorrectly = ListsEqual (_drawables, _drawablesInDrawOrder);
			_drawablesInDrawOrder.Clear ();

			_spriteBatch.Begin ();
			DrawStatusString (
			    string.Format ("{0} drawables", _drawables.Count),
			    0, _drawablesOrderedCorrectly);
			_spriteBatch.End ();
		}

		private void DrawStatusString (string item, int linesFromBottom, bool isCorrect)
		{
			var position = new Vector2 (
			    10, GraphicsDevice.Viewport.Height - ((1 + linesFromBottom) * _font.LineSpacing));
			if (isCorrect)
				_spriteBatch.DrawString (_font, item + " correctly ordered!", position, Color.Lime);
			else
				_spriteBatch.DrawString (_font, item + " incorrectly ordered.", position, Color.Red);
		}

		private static bool ListsEqual<T> (IList<T> a, IList<T> b)
		{
			if (a.Count != b.Count)
				return false;

			var equalityComparer = EqualityComparer<T>.Default;
			for (int i = 0; i < a.Count; ++i)
				if (!equalityComparer.Equals (a [i], b [i]))
					return false;
			return true;
		}

		private class TestDrawable : DrawableGameComponent {
			private static readonly Color [] Colors = new Color [] {
				Color.White, Color.Red, Color.Orange, Color.Yellow, Color.Green,
				Color.Blue, Color.Indigo, Color.Violet, Color.Black
			};

			ImplicitDrawOrderComponent _owner;
			private int _number;
			private Color _color;
			public TestDrawable (Game game, ImplicitDrawOrderComponent owner, int number)
				: base (game)
			{
				_owner = owner;
				_number = number;
				_color = Colors [_number % Colors.Length];
			}

			private SpriteBatch _spriteBatch;
			protected override void LoadContent ()
			{
				base.LoadContent ();
				_spriteBatch = new SpriteBatch (Game.GraphicsDevice);
			}

			protected override void UnloadContent ()
			{
				base.UnloadContent ();

				_spriteBatch.Dispose ();
				_spriteBatch = null;
			}

			public override void Draw (GameTime gameTime)
			{
				_owner._drawablesInDrawOrder.Add (this);

				float halfEx = _owner._font.MeasureString ("x").X / 2;
				var position = new Vector2 (_number * halfEx, 0);

				_spriteBatch.Begin (SpriteSortMode.Immediate, BlendState.AlphaBlend);
				_spriteBatch.DrawString (_owner._font, _number.ToString (), position, _color);
				_spriteBatch.End ();
			}
		}
	}
}
