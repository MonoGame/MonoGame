// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Defines how the <see cref="Game"/> should be run.
    /// </summary>
    public enum GameRunBehavior
    {
        /// <summary>
        /// The game loop will be run asynchronously.
        /// </summary>
        Asynchronous,
        /// <summary>
        /// The game loop will be run synchronously.
        /// </summary>
        Synchronous
    }
}
