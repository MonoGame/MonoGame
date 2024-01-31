// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Handles the buffer events of all DynamicSoundEffectInstance instances.
    /// </summary>
    internal static class DynamicSoundEffectInstanceManager
    {
        private static readonly List<WeakReference> _playingInstances;

        static DynamicSoundEffectInstanceManager()
        {
            _playingInstances = new List<WeakReference>();
        }

        public static void AddInstance(DynamicSoundEffectInstance instance)
        {
            var weakRef = new WeakReference(instance);
            _playingInstances.Add(weakRef);
        }

        public static void RemoveInstance(DynamicSoundEffectInstance instance)
        {
            for (int i = _playingInstances.Count - 1; i >= 0; i--)
            {
                if (_playingInstances[i].Target == instance)
                {
                    _playingInstances.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Updates buffer queues of the currently playing instances.
        /// </summary>
        /// <remarks>
        /// XNA posts <see cref="DynamicSoundEffectInstance.BufferNeeded"/> events always on the main thread.
        /// </remarks>
        public static void UpdatePlayingInstances()
        {
            for (int i = _playingInstances.Count - 1; i >= 0; i--)
            {
                var target = _playingInstances[i].Target as DynamicSoundEffectInstance;
                if (target != null)
                {
                    if (!target.IsDisposed)
                        target.UpdateQueue();
                }
                else
                {
                    // The instance has been disposed.
                    _playingInstances.RemoveAt(i);
                }
            }
        }
    }
}
