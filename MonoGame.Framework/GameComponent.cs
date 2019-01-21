// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{   
    public class GameComponent : IGameComponent, IUpdateable, IComparable<GameComponent>, IDisposable
    {
        bool _enabled = true;
        int _updateOrder;

        public Game Game { get; private set; }

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

        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;

        public GameComponent(Game game)
        {
            this.Game = game;
        }

        ~GameComponent()
        {
            Dispose(false);
        }

        public virtual void Initialize() { }

        public virtual void Update(GameTime gameTime) { }

        protected virtual void OnUpdateOrderChanged(object sender, EventArgs args)
        {
            EventHelpers.Raise(sender, UpdateOrderChanged, args);
        }

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

        #region IComparable<GameComponent> Members
        // TODO: Should be removed, as it is not part of XNA 4.0
        public int CompareTo(GameComponent other)
        {
            return other.UpdateOrder - this.UpdateOrder;
        }

        #endregion
    }
}
