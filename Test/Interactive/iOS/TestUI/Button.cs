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
