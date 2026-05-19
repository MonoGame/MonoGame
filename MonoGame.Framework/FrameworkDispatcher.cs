// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Helper class for processing internal framework events.
    /// </summary>
    /// <remarks>
    /// If you use <see cref="Game"/> class, <see cref="Update()"/> is called automatically.
    /// Otherwise you must call it as part of your game loop.
    /// </remarks>
    public static class FrameworkDispatcher
    {
        private static bool _initialized = false;

        /// <summary>
        /// Processes internal framework updates and raises events.
        /// If you don't use the <see cref="Game"/> class you should call this from your update loop.
        /// This method is not thread safe.
        /// </summary>
        public static void Update()
        {
            if (!_initialized)
                Initialize();

            DoUpdate();
        }

        private static void DoUpdate()
        {
            DynamicSoundEffectInstanceManager.UpdatePlayingInstances();
            SoundEffectInstancePool.Update();
            Microphone.UpdateMicrophones();
        }

        private static void Initialize()
        {
            _initialized = true;
        }
    }
}

