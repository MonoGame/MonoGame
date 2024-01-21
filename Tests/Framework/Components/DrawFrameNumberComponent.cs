// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests.Components {
	class DrawFrameNumberComponent : DrawableGameComponent {
		private SpriteBatch _batch;
		private SpriteFont _font;

		public DrawFrameNumberComponent (Game game)
			: base (game)
		{
		}

		protected override void LoadContent ()
		{
			_batch = new SpriteBatch (Game.GraphicsDevice);
			_font = Game.Content.Load<SpriteFont> (Paths.Font ("Default"));
		}

		protected override void UnloadContent ()
		{
			_batch.Dispose ();
			_batch = null;

			_font = null;
		}

		public override void Draw (GameTime gameTime)
		{
			var frameInfoSource = Game.Services.RequireService<IFrameInfoSource> ();
			var frameInfo = frameInfoSource.FrameInfo;

			// TODO: Add support for different placements and colors.
			_batch.Begin ();
			_batch.DrawString (_font, frameInfo.DrawNumber.ToString(), Vector2.Zero, Color.White);
			_batch.End ();
		}
	}
}
