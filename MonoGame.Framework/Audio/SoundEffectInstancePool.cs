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

        static SoundEffectInstancePool()
        {
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
        internal static SoundEffectInstance GetInstance(bool forXAct)
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
                inst._isPooled = true;
                inst._isXAct = forXAct;
                inst.Volume = 1.0f;
                inst.Pan = 0.0f;
                inst.Pitch = 0.0f;
                inst.IsLooped = false;
            }
            else
            {
                inst = new SoundEffectInstance();
                inst._isPooled = true;
                inst._isXAct = forXAct;
            }

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

                x++;
            }
        }

        /// <summary>
        /// Iterates the list of playing instances, stop them and return them to the pool if they are instances of the given SoundEffect.
        /// </summary>
        /// <param name="effect">The SoundEffect</param>
        internal static void StopPooledInstances(SoundEffect effect)
        {
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
        }

        internal static void UpdateMasterVolume()
        {
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

        internal static void Shutdown()
        {
            // We need to dispose all SoundEffectInstances before shutdown,
            // so as to destroy all SourceVoice instances,
            // before we can destroy our XAudio MasterVoice instance.
            // Otherwise XAudio shutdown fails, causing intermittent crashes.
            foreach (var inst in _playingInstances)
            {
                inst.Dispose();
            }
            _playingInstances.Clear();

            foreach (var inst in _pooledInstances)
            {
                inst.Dispose();
            }
            _pooledInstances.Clear();
        }
    }
}
