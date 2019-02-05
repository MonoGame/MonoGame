// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    public class DrawableGameComponent : GameComponent, IDrawable
    {
        private bool _initialized;
        private int _drawOrder;
        private bool _visible = true;

        public Graphics.GraphicsDevice GraphicsDevice
        {
            get { return this.Game.GraphicsDevice; } 
        }

        public int DrawOrder
        {
            get { return _drawOrder; }
            set
            {
                if (_drawOrder != value)
                {
                    _drawOrder = value;
                    OnDrawOrderChanged(this, EventArgs.Empty);
                }
            }
        }

        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnVisibleChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        public DrawableGameComponent(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            if (!_initialized)
            {
                _initialized = true;
                LoadContent();
            }
        }

        protected virtual void LoadContent() { }

        protected virtual void UnloadContent () { }

        public virtual void Draw(GameTime gameTime) { }

        protected virtual void OnVisibleChanged(object sender, EventArgs args)
        {
            EventHelpers.Raise(sender, VisibleChanged, args);
        }

        protected virtual void OnDrawOrderChanged(object sender, EventArgs args)
        {
            EventHelpers.Raise(sender, DrawOrderChanged, args);
        }
    }
}
