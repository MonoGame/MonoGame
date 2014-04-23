// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#region Using Statements
using System;
#if !DIRECTX
using System.IO;
#endif
#endregion Statements

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffectInstance : IDisposable
    {
        private bool isDisposed = false;

        internal bool _IsPooled = true;

        private float _pan;
        private float _volume;
        private float _pitch;

        public bool IsLooped
        { 
            get { return PlatformGetIsLooped(); }
            set { PlatformSetIsLooped(value); }
        }

        public float Pan
        {
            get { return _pan; } 
            set
            {
                if (value < -1.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException();

                PlatformSetPan(value);
                _pan = value;
            }
        }

        public float Pitch
        {
            get { return _pitch; }
            set
            {
                if (value < -1.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException();

                PlatformSetPitch(value);
                _pitch = value;
            }
        }

        public float Volume
        {
            get { return _volume; }
            set
            {
                if (value < 0.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException();

                PlatformSetVolume(value);
                _volume = value;
            }
        }

        public SoundState State { get { return PlatformGetState(); } }

        public bool IsDisposed { get { return isDisposed; } }

        internal SoundEffectInstance()
        {
            _pan = 0.0f;
            _volume = 1.0f;
            _pitch = 0.0f;            
        }
        
        /// <summary>
        /// Creates a standalone SoundEffectInstance from given wavedata.
        /// </summary>
        internal SoundEffectInstance(byte[] buffer, int sampleRate, int channels)
            : base()
        {
            PlatformInitialize(buffer, sampleRate, channels);
        }


        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            PlatformApply3D(listener, emitter);
        }

        public void Apply3D(AudioListener[] listeners, AudioEmitter emitter)
        {
            foreach (var l in listeners)
				PlatformApply3D(l, emitter);
        }

        public void Pause()
        {
            PlatformPause();
        }

        public void Play()
        {
            if (State == SoundState.Playing)
                return;

            // We don't need to check if we're at the instance play limit
            // if we're resuming from a paused state.
            if (State != SoundState.Paused)
            {
                SoundEffectInstancePool.Remove(this);

                if (!SoundEffectInstancePool.SoundsAvailable)
                    throw new InstancePlayLimitException();
            }

            PlatformPlay();
        }

        public void Resume()
        {
            PlatformResume();
        }

        public void Stop()
        {
            PlatformStop(true);
        }

        public void Stop(bool immediate)
        {
            
            PlatformStop(immediate);

            // instances typically call Stop
            // as they dispose. Prevent this
            // from being added to the SFXInstancePool
            if (isDisposed)
                return;

            // Return this SFXInstance back
            // to the pool to be used later.
            SoundEffectInstancePool.Add(this);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            PlatformDispose();
        }
    }
}
