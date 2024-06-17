// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
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

