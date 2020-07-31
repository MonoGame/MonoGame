// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Arguments for the <see cref="GameComponentCollection.ComponentAdded"/> and
    /// <see cref="GameComponentCollection.ComponentRemoved"/> events.
    /// </summary>
    public class GameComponentCollectionEventArgs : EventArgs
    {
        private IGameComponent _gameComponent;

        /// <summary>
        /// Create a <see cref="GameComponentCollectionEventArgs"/> instance.
        /// </summary>
        /// <param name="gameComponent">The <see cref="IGameComponent"/> that the event notifies about.</param>
        public GameComponentCollectionEventArgs(IGameComponent gameComponent)
        {
            _gameComponent = gameComponent;
        }

        /// <summary>
        /// The <see cref="IGameComponent"/> that the event notifies about.
        /// </summary>
        public IGameComponent GameComponent
        {
            get
            {
                return _gameComponent;
            }
        }
    }
}

