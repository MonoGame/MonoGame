// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio
{
    internal static class SoundEffectInstancePool
    {
        private static readonly List<SoundEffectInstance> _playingInstances;
        private static readonly List<SoundEffectInstance> _pooledInstances;

        private static readonly object _locker;

        static SoundEffectInstancePool()
        {
            _locker = new object();

            // Reduce garbage generation by allocating enough capacity for
            // the maximum playing instances or at least some reasonable value.
            var maxInstances = SoundEffect.MAX_PLAYING_INSTANCES < 1024 ? SoundEffect.MAX_PLAYING_INSTANCES : 1024;
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
                lock(_locker)
                	return _playingInstances.Count < SoundEffect.MAX_PLAYING_INSTANCES;
            }
        }

        /// <summary>
        /// Add the specified instance to the pool if it is a pooled instance and removes it from the
        /// list of playing instances.
        /// </summary>
        /// <param name="inst">The SoundEffectInstance</param>
        internal static void Add(SoundEffectInstance inst)
        {
            lock (_locker) {

            if (inst._isPooled)
            {
                _pooledInstances.Add(inst);
                inst._effect = null;
            }

            _playingInstances.Remove(inst);

            } // lock(_locker)
        }

        /// <summary>
        /// Adds the SoundEffectInstance to the list of playing instances.
        /// </summary>
        /// <param name="inst">The SoundEffectInstance to add to the playing list.</param>
        internal static void Remove(SoundEffectInstance inst)
        {
            lock (_locker)
                _playingInstances.Add(inst);
        }

        /// <summary>
        /// Returns a pooled SoundEffectInstance if one is available, or allocates a new
        /// SoundEffectInstance if the pool is empty.
        /// </summary>
        /// <returns>The SoundEffectInstance.</returns>
        internal static SoundEffectInstance GetInstance(bool forXAct)
        {
            lock (_locker) {

            SoundEffectInstance inst = null;
            var count = _pooledInstances.Count;
            if (count > 0)
            {
                // Grab the item at the end of the list so the remove doesn't copy all
                // the list items down one slot.
                inst = _pooledInstances[count - 1];
                _pooledInstances.RemoveAt(count - 1);

                // Reset used instance to the "default" state.
                inst._isPooled = true;
                inst._isXAct = forXAct;
                inst.Volume = 1.0f;
                inst.Pan = 0.0f;
                inst.Pitch = 0.0f;
                inst.IsLooped = false;
                inst.PlatformSetReverbMix(0);
                inst.PlatformClearFilter();
            }
            else
            {
                inst = new SoundEffectInstance();
                inst._isPooled = true;
                inst._isXAct = forXAct;
            }

            return inst;

            } // lock (_locker)
        }

        /// <summary>
        /// Iterates the list of playing instances, returning them to the pool if they
        /// have stopped playing.
        /// </summary>
        internal static void Update()
        {
            lock (_locker) {

            SoundEffectInstance inst = null;

            // Cleanup instances which have finished playing.                    
            for (var x = 0; x < _playingInstances.Count;)
            {
                inst = _playingInstances[x];

                // Don't consume XACT instances... XACT will
                // clear this flag when it is done with the wave.
                if (inst._isXAct)
                {
                    x++;
                    continue;
                }

                if (inst.IsDisposed || inst.State == SoundState.Stopped || (inst._effect == null && !inst._isDynamic))
                {
#if OPENAL
                    if (!inst.IsDisposed)
                        inst.Stop(true); // force stopping it to free its AL source
#endif
                    Add(inst);
                    continue;
                }

                x++;
            }

            } // lock (_locker)
        }

        /// <summary>
        /// Iterates the list of playing instances, stop them and return them to the pool if they are instances of the given SoundEffect.
        /// </summary>
        /// <param name="effect">The SoundEffect</param>
        internal static void StopPooledInstances(SoundEffect effect)
        {
            lock (_locker) {

            SoundEffectInstance inst = null;

            for (var x = 0; x < _playingInstances.Count;)
            {
                inst = _playingInstances[x];
                if (inst._effect == effect)
                {
                    inst.Stop(true); // stop immediatly
                    Add(inst);
                    continue;
                }

                x++;
            }

            } // lock (_locker)
        }

        internal static void UpdateMasterVolume()
        {
            lock (_locker) {

            foreach (var inst in _playingInstances)
            {
                // XAct sounds are not controlled by the SoundEffect
                // master volume, so we can skip them completely.
                if (inst._isXAct)
                    continue;

                // Re-applying the volume to itself will update
                // the sound with the current master volume.
                inst.Volume = inst.Volume;
            }
        }

        } // lock (_locker)
    }
}
