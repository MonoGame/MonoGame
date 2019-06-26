// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Allows getting keystrokes from keyboard.
    /// </summary>
	public static partial class Keyboard
	{
        /// <summary>
        /// Returns the current keyboard state.
        /// </summary>
        /// <returns>Current keyboard state.</returns>
		public static KeyboardState GetState()
		{
            return PlatformGetState();
		}
		
        /// <summary>
        /// Returns the current keyboard state for a given player.
        /// </summary>
        /// <param name="playerIndex">Player index of the keyboard.</param>
        /// <returns>Current keyboard state.</returns>
        [Obsolete("Use GetState() instead. In future versions this method can be removed.")]
        public static KeyboardState GetState(PlayerIndex playerIndex)
		{
            return PlatformGetState();
		}
	}
}
