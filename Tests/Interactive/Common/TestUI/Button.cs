// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;

namespace MonoGame.InteractiveTests.TestUI
{
    /// <summary>Shows a button with some text on it that can be tapped/touched.</summary>
    class Button : View
    {
        public Button()
        {
            Padding = new PaddingF(10);
        }

        private View _content;

        public View Content
        {
            get { return _content; }
            set
            {
                if (_content != value)
                {
                    if (_content != null)
                        _content.RemoveFromSuperview();
                    _content = value;
                    if (_content != null)
                        Add(_content);
                }
            }
        }

        private PaddingF _padding;

        public PaddingF Padding
        {
            get { return _padding; }
            set
            {
                if (_padding != value)
                {
                    _padding = value;
                    SetNeedsLayout();
                }
            }
        }

        public override void LayoutSubviews()
        {
            foreach (var view in Subviews)
            {
                view.LayoutIfNeeded();

                var frame = view.Frame;
                frame.X = Math.Max(frame.X, (int)Padding.Left);
                frame.Y = Math.Max(frame.Y, (int)Padding.Top);
                view.Frame = frame;
            }
        }

        public override Point SizeThatFits(Point size)
        {
            var maxContentSize = size;
            if (size != Point.Zero)
            {
                size.X -= (int)Padding.Horizontal;
                size.Y -= (int)Padding.Vertical;
            }

            var fitSize = Content.SizeThatFits(maxContentSize);
            fitSize.X += (int)Padding.Horizontal;
            fitSize.Y += (int)Padding.Vertical;
            return fitSize;
        }

        public event EventHandler<EventArgs> Tapped;

        protected virtual void OnTapped(EventArgs e)
        {
            var handler = Tapped;
            if (handler != null)
                handler(this, e);
        }

        public override bool HandleGestureSample(Point point, GameTime gameTime)
        {
            OnTapped(EventArgs.Empty);
            return true;
        }
    }
}
