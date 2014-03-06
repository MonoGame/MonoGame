using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio
{
    internal static class SFXInstancePool
    {

#if WINDOWS || (WINRT && !WINDOWS_PHONE) || LINUX || ANDROID

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
#endif

        private static List<SoundEffectInstance> _playingInstances = new List<SoundEffectInstance>();
        private static List<SoundEffectInstance> _availableInstances = new List<SoundEffectInstance>();
        private static List<WeakReference> _callersInstances = new List<WeakReference>();

        private static bool _playCountDirty = true;
        private static int _playingInstCount;

        internal static bool SoundsAvailable
        {
            get
            {
                if (!_playCountDirty)
                    return _playingInstCount < MAX_PLAYING_INSTANCES;

                _playCountDirty = false;

                _playingInstCount = 0;

                _playingInstCount += _playingInstances.Count(s => s.State != SoundState.Stopped);
                _playingInstCount += _callersInstances.Count(SFXInstanceIsPlaying);

                return _playingInstCount < MAX_PLAYING_INSTANCES;
            }
        }

        private static bool SFXInstanceIsPlaying(WeakReference instanceRef)
        {
            if (!instanceRef.IsAlive)
                return false;

            var data = instanceRef.Target as SoundEffectInstance;

            if (data == null || data.State == SoundState.Stopped)
                return false;

            return true;
        }

        internal static void Add(SoundEffectInstance inst)
        {
            if (inst._isInternal)
                return;

            _playCountDirty = true;

            System.Diagnostics.Debug.Assert(!_availableInstances.Contains(inst));

            _availableInstances.Add(inst);
            _playingInstances.Remove(inst);
        }

        internal static void Remove(SoundEffectInstance inst)
        {
            if (inst._isInternal)
                return;

            _playCountDirty = true;

            System.Diagnostics.Debug.Assert(_availableInstances.Contains(inst));

            _availableInstances.Remove(inst);
            _playingInstances.Add(inst);
        }

        internal static SoundEffectInstance GetInstance(bool releaseToCaller)
        {
            _playCountDirty = true;

            SoundEffectInstance inst = null;
            if (_availableInstances.Count > 0)
            {
                inst = _availableInstances[0];
                _availableInstances.Remove(inst);
            }
            else
                inst = new SoundEffectInstance();

            // If we're going to release this instance
            // to the caller, keep a WeakReference of it. This lets it be garbage
            // collected, but still allows us to keep
            // track of it to maintain a count of sounds playing
            // in the system.
            if (releaseToCaller)
                _callersInstances.Add(new WeakReference(inst));

            inst._isInternal = !releaseToCaller;

            return inst;
        }

        internal static void Update()
        {
            _playCountDirty = false;
            _playingInstCount = 0;

            SoundEffectInstance inst = null;
            // Cleanup instances which have finished playing.                    
            for (var x = 0; x < _playingInstances.Count; )
            {
                inst = _playingInstances[x];

                if (inst.State == SoundState.Stopped)
                {
                    _playingInstances.Remove(inst);
                    _availableInstances.Add(inst);

                    continue;
                }

                _playingInstCount++;

                x++;
            }

            // Loop through SoundEffectInstances that have been
            // released to the caller and check their state
            // Stop tracking them once they've been GCed.
            for (var x = 0; x < _callersInstances.Count; )
            {
                var weakRef = _callersInstances[x];

                if (weakRef.IsAlive)
                {
                    var data = weakRef.Target as SoundEffectInstance;

                    if (data == null)
                    {
                        _callersInstances.Remove(weakRef);
                        continue;
                    }

                    if (data.State != SoundState.Stopped)
                        _playingInstCount++;
                }
                else
                {
                    _callersInstances.Remove(weakRef);
                    continue;
                }

                x++;
            }
        }
    }
}

