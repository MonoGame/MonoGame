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
using System.Collections.ObjectModel;
using System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace MonoGame.InteractiveTests.TestUI {
	class View {
		private readonly ViewCollection _subviews;
		private readonly ReadOnlyCollection<View> _readOnlySubviews;
		private bool _needsLayout;
		public View ()
		{
			_subviews = new ViewCollection (this);
			_readOnlySubviews = new ReadOnlyCollection<View> (_subviews);

			BackgroundColor = Color.Transparent;
			IsVisible = true;
		}

		public Color BackgroundColor { get; set; }

		private RectangleF _frame;
		public RectangleF Frame {
			get { return _frame; }
			set {
				if (_frame != value) {
					_frame = SetFrameCore (value);
				}
			}
		}

		public bool IsVisible { get; set; }

		public PointF Location {
			get { return _frame.Location; }
			set {
				var newFrame = Frame;
				newFrame.Location = value;
				Frame = newFrame;
			}
		}

		public ReadOnlyCollection<View> Subviews {
			get { return _readOnlySubviews; }
		}

		public View Superview { get; set; }

		private Universe _universe;
		public Universe Universe {
			get { return _universe; }
			set {
				if (_universe != value) {
					_universe = value;
					_subviews.Universe = value;
				}
			}
		}

		public void Add (View subview)
		{
			_subviews.Add (subview);
			SetNeedsLayout ();
		}

		public IEnumerable<View> HitTest (PointF p)
		{
			foreach (var view in _subviews.HitTest (p))
				yield return view;

			if (HitTestSelf (p))
				yield return this;
		}

		public bool HitTestSelf (PointF p) {
			var rect = Frame;
			rect.Location = PointF.Empty;

			return rect.Contains (p);
		}

		public void LayoutIfNeeded ()
		{
			if (_needsLayout) {
				LayoutSubviews ();
				_needsLayout = false;
			}
		}

		public virtual void LayoutSubviews ()
		{
		}

		protected virtual RectangleF SetFrameCore(RectangleF frame)
		{
			return frame;
		}

		public virtual bool HandleGestureSample(GestureSample gestureSample, GameTime gameTime)
		{
			return false;
		}

		public void RemoveFromSuperview ()
		{
			if (Superview == null)
				throw new InvalidOperationException ("The View does not have a Superview");


			Superview._subviews.Remove (this);
			Superview.SetNeedsLayout ();
		}

		public void SetNeedsLayout ()
		{
			_needsLayout = true;
		}

		public virtual SizeF SizeThatFits (SizeF size)
		{
			return Frame.Size;
		}

		public void SizeToFit ()
		{
			// TODO: Distribute the new size according to this
			//       View's Origin (which doesn't exist yet).

			if (Superview == null)
				Frame = new RectangleF (Frame.Location, SizeThatFits (SizeF.Empty));
			else
				// TODO: Calculate the available size based on
				//       this View's location and Origin and
				//       the Superview Frame.
				Frame = new RectangleF (Frame.Location, SizeThatFits (Superview.Frame.Size));
		}

		public virtual void Update (GameTime gameTime)
		{
			LayoutIfNeeded ();
		}

		public virtual void Draw (DrawContext context, GameTime gameTime)
		{
			LayoutIfNeeded ();

			DrawBackground (context, gameTime);
			DrawForeground (context, gameTime);
			_subviews.Draw (context, gameTime);
			DrawAboveSubviews (context, gameTime);
		}

		protected virtual void DrawBackground (DrawContext context, GameTime gameTime)
		{
			if (BackgroundColor.A > 0) {
				var swatch = Universe.Content.Load<Texture2D>(@"Textures\white-1");
				context.SpriteBatch.Draw (
					swatch, Vector2.Zero, null, BackgroundColor, 0, Vector2.Zero,
					new Vector2 (Frame.Width, Frame.Height), SpriteEffects.None, 0);
			}
		}

		protected virtual void DrawForeground (DrawContext context, GameTime gameTime)
		{
		}

		protected virtual void DrawAboveSubviews (DrawContext context, GameTime gameTime)
		{
		}
	}
}

