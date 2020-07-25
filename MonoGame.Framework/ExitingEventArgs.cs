using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Event arguments for <see cref="Game.Exiting"/>.
    /// </summary>
    public class ExitingEventArgs : EventArgs
    {
        /// <summary>
        /// Set to <c>true</c> to cancel closing the game.
        /// </summary>
        public bool Cancel { get; set; }
    }
}
