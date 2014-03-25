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

        public bool IsLooped
        { 
            get { return PlatformGetIsLooped(); }
            set { PlatformSetIsLooped(value); }
        }

        public float Pan
        {
            get { return PlatformGetPan(); } 
            set { PlatformSetPan(value); }
        }

        public float Pitch
        {
            get { return PlatformGetPitch(); }
            set { PlatformSetPitch(value); }
        }

        public SoundState State { get { return PlatformGetState(); } }

        public bool IsDisposed { get { return isDisposed; } }

        public float Volume
        {
            get { return PlatformGetVolume(); } 
            set { PlatformSetVolume(value); }
        }

        internal SoundEffectInstance(){}
        
        /// <summary>
        /// Creates a standalone SoundEffectInstance from given wavedata.
        /// </summary>
        internal SoundEffectInstance(byte[] buffer, int sampleRate, int channels)
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
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            PlatformDispose();

            isDisposed = true;
        }
    }
}
