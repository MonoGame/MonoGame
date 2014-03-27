using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio
{
    internal static class SoundEffectInstancePool
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

        private static readonly List<SoundEffectInstance> _playingInstances = new List<SoundEffectInstance>();
        private static readonly List<SoundEffectInstance> _pooledInstances = new List<SoundEffectInstance>();

        internal static bool SoundsAvailable
        {
            get
            {
                return _playingInstances.Count < MAX_PLAYING_INSTANCES;
            }
        }

        internal static List<SoundEffectInstance> GetAllPlayingSounds()
        {
            return new List<SoundEffectInstance>(_playingInstances);
        }

        internal static void Add(SoundEffectInstance inst)
        {
            if (inst._IsPooled)
                _pooledInstances.Add(inst);

            _playingInstances.Remove(inst);
        }

        internal static void Remove(SoundEffectInstance inst)
        {
            _playingInstances.Add(inst);
        }

        internal static SoundEffectInstance GetInstance()
        {
            SoundEffectInstance inst = null;
            if (_pooledInstances.Count > 0)
            {
                inst = _pooledInstances[0];
                _pooledInstances.Remove(inst);

                // Reset used instance to the "default" state.
                inst.Volume = 1.0f;
                inst.Pan = 0.0f;
                inst.Pitch = 0.0f;
                inst.IsLooped = false;
            }
            else
                inst = new SoundEffectInstance();

            inst._IsPooled = true;

            return inst;
        }

        internal static void Update()
        {
            SoundEffectInstance inst = null;
            // Cleanup instances which have finished playing.                    
            for (var x = 0; x < _playingInstances.Count;)
            {
                inst = _playingInstances[x];

                if (inst.State == SoundState.Stopped)
                {
                    Add(inst);
                    continue;
                }

                x++;
            }
        }
    }
}

