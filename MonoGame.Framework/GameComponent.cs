// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{   
    /// <summary>
    /// An object that can be attached to a <see cref="Microsoft.Xna.Framework.Game"/> and have its <see cref="Update"/>
    /// method called when <see cref="Microsoft.Xna.Framework.Game.Update"/> is called.
    /// </summary>
    public class GameComponent : IGameComponent, IUpdateable, IDisposable
    {
        bool _enabled = true;
        int _updateOrder;

        /// <summary>
        /// The <see cref="Game"/> that owns this <see cref="GameComponent"/>.
        /// </summary>
        public Game Game { get; private set; }

        /// <summary>
        /// Indicates whether <see cref="Update(GameTime)">GameComponent.Update(GameTime)</see> should be
        /// called when <see cref="Game.Update(GameTime)">Game.Update(GameTime)</see> is called.
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnEnabledChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Indicates the order in which the <see cref="GameComponent"/>
        /// should be updated relative to other <see cref="GameComponent"/> instances.
        /// Lower values are updated first.
        /// </summary>
        public int UpdateOrder
        {
            get { return _updateOrder; }
            set
            {
                if (_updateOrder != value)
                {
                    _updateOrder = value;
                    OnUpdateOrderChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler<EventArgs> EnabledChanged;

        /// <inheritdoc />
        public event EventHandler<EventArgs> UpdateOrderChanged;

        /// <summary>
        /// Create a <see cref="GameComponent"/>.
        /// </summary>
        /// <param name="game">The game that this component will belong to.</param>
        public GameComponent(Game game)
        {
            this.Game = game;
        }

        /// <summary/>
        ~GameComponent()
        {
            Dispose(false);
        }

        /// <summary>
        /// Called when the <see cref="GameComponent"/> needs to be initialized.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Update the component.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> of the <see cref="Game"/>.</param>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Called when <see cref="UpdateOrder"/> changed. Raises the <see cref="UpdateOrderChanged"/> event.
        /// </summary>
        /// <param name="sender">This <see cref="GameComponent"/>.</param>
        /// <param name="args">Arguments to the <see cref="UpdateOrderChanged"/> event.</param>
        protected virtual void OnUpdateOrderChanged(object sender, EventArgs args)
        {
            EventHelpers.Raise(sender, UpdateOrderChanged, args);
        }

        /// <summary>
        /// Called when <see cref="Enabled"/> changed. Raises the <see cref="EnabledChanged"/> event.
        /// </summary>
        /// <param name="sender">This <see cref="GameComponent"/>.</param>
        /// <param name="args">Arguments to the <see cref="EnabledChanged"/> event.</param>
        protected virtual void OnEnabledChanged(object sender, EventArgs args)
        {
            EventHelpers.Raise(sender, EnabledChanged, args);
        }

        /// <summary>
        /// Shuts down the component.
        /// </summary>
        protected virtual void Dispose(bool disposing) { }
        
        /// <summary>
        /// Shuts down the component.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
