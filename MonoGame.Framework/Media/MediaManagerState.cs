// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using SharpDX.MediaFoundation;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// This class provides a way for the MediaManager to be initialised exactly once, 
    /// regardless of how many different places need it, and which is called first.
    /// </summary>
    internal sealed class MediaManagerState
    {
        private static bool started;

        /// <summary>
        /// Ensures that the MediaManager has been initialised. Must be called from UI thread.
        /// </summary>
        public static void CheckStartup()
        {
            if(!started)
            {
                started = true;
                MediaManager.Startup(true);
            }
        }

        /// <summary>
        /// Ensures that the MediaManager has been shutdown. Must be called from UI thread.
        /// </summary>
        public static void CheckShutdown()
        {
            if(started)
            {
                started = false;
                MediaManager.Shutdown();
            }
        }
    }
}
