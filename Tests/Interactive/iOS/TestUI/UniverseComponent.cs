// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;

using Microsoft.Xna.Framework;

namespace MonoGame.InteractiveTests.TestUI {
	class UniverseComponent : DrawableGameComponent {
		public UniverseComponent (Game game, Universe universe = null)
			: base (game)
		{
			_universe = universe ?? new Universe (game.Content);
		}

		private readonly Universe _universe;
		public Universe Universe {
			get { return _universe; }
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			_universe.Update (gameTime);
		}

		private DrawContext _drawContext;
		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			if (_drawContext == null || _drawContext.GraphicsDevice != GraphicsDevice)
				_drawContext = new DrawContext (GraphicsDevice, Matrix.Identity);

			_drawContext.Begin (Matrix.Identity);
			try {
				_universe.Draw (_drawContext, gameTime);
			} finally {
				_drawContext.End ();
			}
		}
	}
}
