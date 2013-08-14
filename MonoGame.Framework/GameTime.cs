#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Snapshot of the game timing state expressed in values that can be used by variable-step (real time) or fixed-step (game time) games.
    /// </summary>
    public class GameTime
    {
        /// <summary>
        /// The amount of game time since the start of the game.
        /// </summary>
        public TimeSpan TotalGameTime { get; set; }

        /// <summary>
        /// The amount of elapsed game time since the last update.
        /// </summary>
        public TimeSpan ElapsedGameTime { get; set; }

        /// <summary>
        /// Gets a value indicating that the game loop is taking longer than its TargetElapsedTime. In this case, the game loop can be considered to be running too slowly and should do something to "catch up."
        /// </summary>
        public bool IsRunningSlowly { get; set; }

        /// <summary>
        /// Creates a new instance of GameTime.
        /// </summary>
        public GameTime()
        {
        }

        /// <summary>
        /// Creates a new instance of GameTime.
        /// </summary>
        /// <param name="totalGameTime">The amount of game time since the start of the game.</param>
        /// <param name="elapsedGameTime">The amount of elapsed game time since the last update.</param>
        public GameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime)
            : this(totalGameTime, elapsedGameTime, false)
        {
        }

        /// <summary>
        /// Creates a new instance of GameTime.
        /// </summary>
        /// <param name="totalRealTime">The amount of game time since the start of the game.</param>
        /// <param name="elapsedRealTime">The amount of elapsed game time since the last update.</param>
        /// <param name="isRunningSlowly">Whether the game is running multiple updates this frame.</param>
        public GameTime(TimeSpan totalRealTime, TimeSpan elapsedRealTime, bool isRunningSlowly)
        {
            TotalGameTime = totalRealTime;
            ElapsedGameTime = elapsedRealTime;
            IsRunningSlowly = isRunningSlowly;
        }
    }
}
