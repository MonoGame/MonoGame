// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// A drawable object that, when added to the <see cref="Game.Components">Game.Components</see> collection of a
    /// <see cref="Game"/> instance, will have it's <see cref="Draw(GameTime)">Draw(GameTime)</see> method called when
    /// <see cref="Game.Draw(GameTime)">Game.Draw(GameTime)</see> is called.
    /// </summary>
    /// <remarks>
    /// This inherits from <see cref="GameComponent"/> so it's <see cref="GameComponent.Update(GameTime)">Update(GameTime)</see>
    /// method will also be called when <see cref="Game.Update(GameTime)">Game.Update(GameTime)</see> is called.
    /// </remarks>
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

        /// <summary>
        /// Gets the order in which this component should be drawn, relative to other components that are in the
        /// same <see cref="GameComponentCollection"/>.
        /// </summary>
        /// <remarks>
        /// This value can be any integer.  Components in the <see cref="GameComponentCollection"/> are drawn in
        /// ascending order based on their <b>DrawOrder</b>.
        /// </remarks>
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

        /// <summary>
        /// Gets or Sets a value that indicates whether the <see cref="Draw(GameTime)"/> method of this component
        /// should be called.
        /// </summary>
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

        /// <inheritdoc />
        public override void Initialize()
        {
            if (!_initialized)
            {
                _initialized = true;
                LoadContent();
            }
        }

        /// <inheritdoc />
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
