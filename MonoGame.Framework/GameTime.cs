// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Holds the time state of a <see cref="Game"/>.
    /// </summary>
    public class GameTime
    {
        /// <summary>
        /// Time since the start of the <see cref="Game"/>.
        /// </summary>
        public TimeSpan TotalGameTime { get; set; }

        /// <summary>
        /// Time since the last call to <see cref="Game.Update"/>.
        /// </summary>
        public TimeSpan ElapsedGameTime { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="Game"/> is running slowly.
        ///
        /// This flag is set to <c>true</c> when <see cref="Game.IsFixedTimeStep"/> is set to <c>true</c>
        /// and a tick of the game loop takes longer than <see cref="Game.TargetElapsedTime"/> for
        /// a few frames in a row.
        /// </summary>
        public bool IsRunningSlowly { get; set; }

        /// <summary>
        /// Create a <see cref="GameTime"/> instance with a <see cref="TotalGameTime"/> and
        /// <see cref="ElapsedGameTime"/> of <code>0</code>.
        /// </summary>
        public GameTime()
        {
            TotalGameTime = TimeSpan.Zero;
            ElapsedGameTime = TimeSpan.Zero;
            IsRunningSlowly = false;
        }

        /// <summary>
        /// Create a <see cref="GameTime"/> with the specified <see cref="TotalGameTime"/>
        /// and <see cref="ElapsedGameTime"/>.
        /// </summary>
        /// <param name="totalGameTime">The total game time elapsed since the start of the <see cref="Game"/>.</param>
        /// <param name="elapsedGameTime">The time elapsed since the last call to <see cref="Game.Update"/>.</param>
        public GameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime)
        {
            TotalGameTime = totalGameTime;
            ElapsedGameTime = elapsedGameTime;
            IsRunningSlowly = false;
        }

        /// <summary>
        /// Create a <see cref="GameTime"/> with the specified <see cref="TotalGameTime"/>
        /// and <see cref="ElapsedGameTime"/>.
        /// </summary>
        /// <param name="totalRealTime">The total game time elapsed since the start of the <see cref="Game"/>.</param>
        /// <param name="elapsedRealTime">The time elapsed since the last call to <see cref="Game.Update"/>.</param>
        /// <param name="isRunningSlowly">A value indicating if the <see cref="Game"/> is running slowly.</param>
		public GameTime (TimeSpan totalRealTime, TimeSpan elapsedRealTime, bool isRunningSlowly)
		{
            TotalGameTime = totalRealTime;
            ElapsedGameTime = elapsedRealTime;
		    IsRunningSlowly = isRunningSlowly;
		}
    }
}

