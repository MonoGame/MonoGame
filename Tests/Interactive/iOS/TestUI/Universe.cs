// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using System.Drawing;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace MonoGame.InteractiveTests.TestUI {
	class Universe {

		private readonly ViewCollection _views;
		public Universe (ContentManager content)
		{
			if (content == null)
				throw new ArgumentNullException ("content");
			_content = content;
			_views = new ViewCollection (null);
			_views.Universe = this;
		}

		private readonly ContentManager _content;
		public ContentManager Content {
			get { return _content; }
		}

		public bool AutoHandleInput { get; set; }

		public void Add (View view)
		{
			_views.Add (view);
		}

		public bool HandleGestureSample (GestureSample gestureSample, GameTime gameTime)
		{
			bool handled = false;
			var position = new PointF (gestureSample.Position.X, gestureSample.Position.Y);
			foreach (var view in _views.HitTest (position)) {
				if (view.HandleGestureSample (gestureSample, gameTime)) {
					handled = true;
					break;
				}
			}

			return handled;
		}

		public void Update(GameTime gameTime)
		{
			if (AutoHandleInput) {
				while (TouchPanel.IsGestureAvailable) {
					var gestureSample = TouchPanel.ReadGesture ();
					HandleGestureSample (gestureSample, gameTime);
				}
			}
		}

		public void Draw(DrawContext context, GameTime gameTime)
		{
			_views.Draw (context, gameTime);
		}
	}
}

