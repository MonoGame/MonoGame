// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework
{   
    /// <summary>
    /// Defines the index of player for various MonoGame components.
    /// </summary>
    /// <remarks>
    /// Use <see cref="GamePad.MaximumGamePadCount" /> to determine the number of supported gamepads on the current 
    /// platform to ensure that a valid <see cref="PlayerIndex" /> is used when accessing gamepad input.
    /// Not all platforms support all eight player indices.
    /// </remarks>
    public enum PlayerIndex
    {
        /// <summary>
        /// The first player index.
        /// </summary>
        One = 0,
        /// <summary>
        /// The second player index.
        /// </summary>
        Two = 1,
        /// <summary>
        /// The third player index.
        /// </summary>
        Three = 2,
        /// <summary>
        /// The fourth player index.
        /// </summary>
        Four = 3,
        /// <summary>
        /// The fifth player index.
        /// </summary>
        Five = 4,
        /// <summary>
        /// The sixth player index.
        /// </summary>
        Six = 5,
        /// <summary>
        /// The seventh player index.
        /// </summary>
        Seven = 6,
        /// <summary>
        /// The eighth player index.
        /// </summary>
        Eight = 7
    }
}
