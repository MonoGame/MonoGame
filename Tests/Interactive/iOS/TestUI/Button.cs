// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System.Drawing;

namespace MonoGame.InteractiveTests.TestUI {
	class Button : View {
		public Button ()
		{
			Padding = new PaddingF (10);
		}

		private View _content;
		public View Content {
			get { return _content; }
			set {
				if (_content != value) {
					if (_content != null)
						_content.RemoveFromSuperview ();
					_content = value;
					if (_content != null)
						Add (_content);
				}
			}
		}

		private PaddingF _padding;
		public PaddingF Padding {
			get { return _padding; }
			set {
				if (_padding != value) {
					_padding = value;
					SetNeedsLayout ();
				}
			}
		}

		public override void LayoutSubviews()
		{
			foreach (var view in Subviews) {
				view.LayoutIfNeeded ();

				var frame = view.Frame;
				frame.X = Math.Max (frame.X, Padding.Left);
				frame.Y = Math.Max (frame.Y, Padding.Top);
				view.Frame = frame;
			}
		}

		public override System.Drawing.SizeF SizeThatFits(SizeF size)
		{
			var maxContentSize = size;
			if (size != Size.Empty) {
				size.Width -= Padding.Horizontal;
				size.Height -= Padding.Vertical;
			}

			var fitSize = Content.SizeThatFits (maxContentSize);
			fitSize.Width += Padding.Horizontal;
			fitSize.Height += Padding.Vertical;
			return fitSize;
		}

		public event EventHandler<EventArgs> Tapped;
		protected virtual void OnTapped(EventArgs e)
		{
			var handler = Tapped;
			if (handler != null)
				handler (this, e);
		}

		public override bool HandleGestureSample(GestureSample gestureSample, GameTime gameTime)
		{
			if (gestureSample.GestureType == GestureType.Tap) {
				OnTapped (EventArgs.Empty);
				return true;
			}
			return base.HandleGestureSample(gestureSample, gameTime);
		}
	}
}
