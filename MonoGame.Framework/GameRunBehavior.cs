// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Defines how <see cref="Game"/> should be runned.
    /// </summary>
    public enum GameRunBehavior
    {
        /// <summary>
        /// The game loop will be runned asynchronous.
        /// </summary>
        Asynchronous,
        /// <summary>
        /// The game loop will be runned synchronous.
        /// </summary>
        Synchronous
    }
}