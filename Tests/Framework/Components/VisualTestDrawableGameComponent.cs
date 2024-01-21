// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MonoGame.Tests.Components {
	class VisualTestDrawableGameComponent : DrawableGameComponent {
		public VisualTestDrawableGameComponent (Game game)
			: base (game)
		{
		}

		private UpdateGuard _updateGuard = new UpdateGuard ();
		public override void Update (GameTime gameTime)
		{
			base.Update (gameTime);

			if (gameTime.ElapsedGameTime == TimeSpan.Zero)
				return;
			if (_updateGuard.ShouldUpdate (Game.Services.RequireService<IFrameInfoSource> ().FrameInfo))
				UpdateOncePerDraw (gameTime);
		}

		protected virtual void UpdateOncePerDraw (GameTime gameTime)
		{
		}
	}
}
