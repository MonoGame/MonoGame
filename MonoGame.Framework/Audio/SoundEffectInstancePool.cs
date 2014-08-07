// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio
{
    internal static class SoundEffectInstancePool
    {

#if WINDOWS || (WINRT && !WINDOWS_PHONE) || LINUX || WEB || ANGLE

        // These platforms are only limited by memory.
        private const int MAX_PLAYING_INSTANCES = int.MaxValue;

#elif MONOMAC

        // Reference: http://stackoverflow.com/questions/3894044/maximum-number-of-openal-sound-buffers-on-iphone
        private const int MAX_PLAYING_INSTANCES = 256;

#elif PSM

        // Reference: http://community.eu.playstation.com/t5/Audio/Multiple-sound-effects/m-p/16681132/highlight/true#M49
        private const int MAX_PLAYING_INSTANCES = 128;

#elif WINDOWS_PHONE

        // Reference: http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.instanceplaylimitexception.aspx
        private const int MAX_PLAYING_INSTANCES = 64;

#elif IOS

        // Reference: http://stackoverflow.com/questions/3894044/maximum-number-of-openal-sound-buffers-on-iphone
        private const int MAX_PLAYING_INSTANCES = 32;

#elif ANDROID

        // Set to the same as OpenAL on iOS
        internal const int MAX_PLAYING_INSTANCES = 32;

#endif

        private static readonly List<SoundEffectInstance> _playingInstances;
        private static readonly List<SoundEffectInstance> _pooledInstances;

        static SoundEffectInstancePool()
        {
            // Reduce garbage generation by allocating enough capacity for
            // the maximum playing instances or at least some reasonable value.
            var maxInstances = MAX_PLAYING_INSTANCES < 1024 ? MAX_PLAYING_INSTANCES : 1024;
            _playingInstances = new List<SoundEffectInstance>(maxInstances);
            _pooledInstances = new List<SoundEffectInstance>(maxInstances);
        }

        /// <summary>
        /// Gets a value indicating whether the platform has capacity for more sounds to be played at this time.
        /// </summary>
        /// <value><c>true</c> if more sounds can be played; otherwise, <c>false</c>.</value>
        internal static bool SoundsAvailable
        {
            get
            {
                return _playingInstances.Count < MAX_PLAYING_INSTANCES;
            }
        }

        /// <summary>
        /// Add the specified instance to the pool if it is a pooled instance and removes it from the
        /// list of playing instances.
        /// </summary>
        /// <param name="inst">The SoundEffectInstance</param>
        internal static void Add(SoundEffectInstance inst)
        {
            if (inst._isPooled)
            {
                _pooledInstances.Add(inst);
                inst._effect = null;
            }

            _playingInstances.Remove(inst);
        }

        /// <summary>
        /// Adds the SoundEffectInstance to the list of playing instances.
        /// </summary>
        /// <param name="inst">The SoundEffectInstance to add to the playing list.</param>
        internal static void Remove(SoundEffectInstance inst)
        {
            _playingInstances.Add(inst);
        }

        /// <summary>
        /// Returns a pooled SoundEffectInstance if one is available, or allocates a new
        /// SoundEffectInstance if the pool is empty.
        /// </summary>
        /// <returns>The SoundEffectInstance.</returns>
        internal static SoundEffectInstance GetInstance()
        {
            SoundEffectInstance inst = null;
            var count = _pooledInstances.Count;
            if (count > 0)
            {
                // Grab the item at the end of the list so the remove doesn't copy all
                // the list items down one slot.
                inst = _pooledInstances[count - 1];
                _pooledInstances.RemoveAt(count - 1);

                // Reset used instance to the "default" state.
                inst.Volume = 1.0f;
                inst.Pan = 0.0f;
                inst.Pitch = 0.0f;
                inst.IsLooped = false;
            }
            else
                inst = new SoundEffectInstance();

            inst._isPooled = true;

            return inst;
        }

        /// <summary>
        /// Iterates the list of playing instances, returning them to the pool if they
        /// have stopped playing.
        /// </summary>
        internal static void Update()
        {
#if OPENAL
            OpenALSoundController.GetInstance.Update();
#endif

            SoundEffectInstance inst = null;
            // Cleanup instances which have finished playing.                    
            for (var x = 0; x < _playingInstances.Count;)
            {
                inst = _playingInstances[x];

                if (inst.State == SoundState.Stopped || inst.IsDisposed || inst._effect == null)
                {
                    Add(inst);
                    continue;
                }
                else if (inst._effect.IsDisposed)
                {
                    Add(inst);
                    // Instances created through SoundEffect.CreateInstance need to be disposed when
                    // their owner SoundEffect is disposed.
                    if (!inst._isPooled)
                        inst.Dispose();
                    continue;
                }

                x++;
            }
        }

        /// <summary>
        /// Updates the volumes of all currently playing instances. Used when SoundEffect.MasterVolume is changed.
        /// </summary>
        internal static void UpdateVolumes()
        {
            foreach (var inst in _playingInstances)
                inst.Volume = inst.Volume;
        }
    }
}
