// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

namespace MonoGame.InteractiveTests.TestUI
{
    /// <summary>
    /// Maintains a flat list of views to help draw, hit-test and manage lifecycle of <see cref="View"/>.
    /// </summary>
    class ViewCollection : Collection<View>
    {
        private readonly View _superview;

        public ViewCollection(View superview)
        {
            _superview = superview;
        }

        private Universe _universe;

        public Universe Universe
        {
            get { return _universe; }
            set
            {
                _universe = value;
                foreach (var view in Items)
                    view.Universe = value;
            }
        }

        public IEnumerable<View> HitTest(Point p)
        {
            foreach (var view in Items)
            {
                var local = p;
                local.X -= view.Frame.X;
                local.Y -= view.Frame.Y;

                foreach (var subview in view.HitTest(local))
                    yield return subview;
            }
        }

        public IEnumerable<View> HitTest(Rectangle rect)
        {
            throw new NotImplementedException();
        }

        public void Draw(DrawContext context, GameTime gameTime)
        {
            foreach (var view in Items)
            {
                if (!view.IsVisible)
                    continue;

                var matrix = context.Matrix * Matrix.CreateTranslation(view.Frame.X, view.Frame.Y, 0);
                context.Begin(matrix);
                try { view.Draw(context, gameTime); }
                finally { context.End(); }
            }
        }

        protected override void InsertItem(int index, View item)
        {
            base.InsertItem(index, item);

            if (item.Superview != null)
                item.RemoveFromSuperview();
            item.Superview = _superview;
            item.Universe = _universe;
        }

        protected override void SetItem(int index, View item)
        {
            throw new InvalidOperationException(
                "ViewCollection does not support setting items by index.");
        }

        protected override void ClearItems()
        {
            foreach (var view in Items)
            {
                view.Superview = null;
                view.Universe = null;
            }

            base.ClearItems();
        }
    }
}
