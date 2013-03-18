using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MonoGame.Tests.Components {
	class FlexibleGameComponent : VisualTestDrawableGameComponent {

		public FlexibleGameComponent (Game game)
			: base (game)
		{
		}

		public Action<FrameInfo> InitializeAction { get; set; }
		public Action<FrameInfo> LoadContentAction { get; set; }
		public Action<FrameInfo> UnloadContentAction { get; set; }
		public Action<FrameInfo> UpdateAction { get; set; }
		public Action<FrameInfo> UpdateOncePerDrawAction { get; set; }
		public Action<FrameInfo> DrawAction { get; set; }

		private IFrameInfoSource _frameInfoSource;
		public override void Initialize ()
		{
			_frameInfoSource = Game.Services.RequireService<IFrameInfoSource> ();

			if (InitializeAction != null)
				InitializeAction (_frameInfoSource.FrameInfo);

			base.Initialize ();
		}

		protected override void LoadContent ()
		{
			base.LoadContent ();

			if (LoadContentAction != null)
				LoadContentAction (_frameInfoSource.FrameInfo);
		}

		protected override void UnloadContent ()
		{
			base.UnloadContent ();

			if (UnloadContentAction != null)
				UnloadContentAction (_frameInfoSource.FrameInfo);
		}

		public override void Update (GameTime gameTime)
		{
			base.Update (gameTime);

			if (UpdateAction != null)
				UpdateAction (_frameInfoSource.FrameInfo);
		}

		protected override void UpdateOncePerDraw (GameTime gameTime)
		{
			base.UpdateOncePerDraw (gameTime);

			if (UpdateOncePerDrawAction != null)
				UpdateOncePerDrawAction (_frameInfoSource.FrameInfo);
		}

		public override void Draw (GameTime gameTime)
		{
			base.Draw (gameTime);

			if (DrawAction != null)
				DrawAction (_frameInfoSource.FrameInfo);
		}
	}
}
