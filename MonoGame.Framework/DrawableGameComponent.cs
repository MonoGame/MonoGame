// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// A <see cref="GameComponent"/> that is drawn when its <see cref="Game"/> is drawn.
    /// </summary>
    public class DrawableGameComponent : GameComponent, IDrawable
    {
        private bool _initialized;
        private bool _disposed;
        private int _drawOrder;
        private bool _visible = true;

        /// <summary>
        /// Get the <see cref="GraphicsDevice"/> that this <see cref="DrawableGameComponent"/> uses for drawing.
        /// </summary>
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

        /// <inheritdoc />
        public event EventHandler<EventArgs> DrawOrderChanged;

        /// <inheritdoc />
        public event EventHandler<EventArgs> VisibleChanged;

        /// <summary>
        /// Create a <see cref="DrawableGameComponent"/>.
        /// </summary>
        /// <param name="game">The game that this component will belong to.</param>
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

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                UnloadContent();
            }
        }

        /// <summary>
        /// Load graphical resources needed by this component.
        /// </summary>
        protected virtual void LoadContent() { }

        /// <summary>
        /// Unload graphical resources needed by this component.
        /// </summary>
        protected virtual void UnloadContent () { }

        /// <summary>
        /// Draw this component.
        /// </summary>
        /// <param name="gameTime">The time elapsed since the last call to <see cref="Draw"/>.</param>
        public virtual void Draw(GameTime gameTime) { }

        /// <summary>
        /// Called when <see cref="Visible"/> changed.
        /// </summary>
        /// <param name="sender">This <see cref="DrawableGameComponent"/>.</param>
        /// <param name="args">Arguments to the <see cref="VisibleChanged"/> event.</param>
        protected virtual void OnVisibleChanged(object sender, EventArgs args)
        {
            EventHelpers.Raise(sender, VisibleChanged, args);
        }

        /// <summary>
        /// Called when <see cref="DrawOrder"/> changed.
        /// </summary>
        /// <param name="sender">This <see cref="DrawableGameComponent"/>.</param>
        /// <param name="args">Arguments to the <see cref="DrawOrderChanged"/> event.</param>
        protected virtual void OnDrawOrderChanged(object sender, EventArgs args)
        {
            EventHelpers.Raise(sender, DrawOrderChanged, args);
        }
    }
}
