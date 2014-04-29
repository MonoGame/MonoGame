// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Provides a single playing, paused, or stopped instance of a SoundEffect sound.</summary>
    /// <remarks>
    /// <para>You can create a SoundEffectInstance by calling SoundEffect.CreateInstance. Initially, the SoundEffectInstance is created as stopped, but you can play it by calling Play.</para>
    /// <para>You can modify the volume, panning, and pitch of the SoundEffectInstance by setting the Volume, Pitch, and Pan properties.</para>
    /// </remarks>
    public sealed partial class SoundEffectInstance : IDisposable
    {
        private bool isDisposed = false;

        internal bool _IsPooled = true;

        private float _pan;
        private float _volume;
        private float _pitch;

        /// <summary>Gets a value that indicates whether looping is enabled for the SoundEffectInstance.</summary>
        /// <remarks>If you want to make a sound play continuously until stopped, be sure to set IsLooped to true before you call SoundEffect.Play.</remarks>
        public bool IsLooped
        { 
            get { return PlatformGetIsLooped(); }
            set { PlatformSetIsLooped(value); }
        }

        /// <summary>Gets or sets the panning for the SoundEffectInstance.</summary>
        /// <value>Panning, ranging from -1.0f (full left) to 1.0f (full right). 0.0f is centered.</value>
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

        /// <summary>Gets or sets the pitch adjustment for the SoundEffectInstance.</summary>
        /// <value>Pitch adjustment, ranging from -1.0f (down one octave) to 1.0f (up one octave). 0.0f is unity (normal) pitch.</value>
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

        /// <summary>Gets or sets the volume of the SoundEffectInstance.</summary>
        /// <value>Volume, ranging from 0.0f (silence) to 1.0f (full volume). 1.0f is full volume relative to SoundEffect.MasterVolume.</value>
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

        /// <summary>Gets the current state (playing, paused, or stopped) of the SoundEffectInstance.</summary>
        public SoundState State { get { return PlatformGetState(); } }

        /// <summary>Gets a value that indicates whether the object is disposed.</summary>
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

        /// <summary>Applies 3D positioning to the sound using a single listener.</summary>
        /// <param name="listener">Position of the listener.</param>
        /// <param name="emitter">Position of the emitter.</param>
        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            PlatformApply3D(listener, emitter);
        }

        /// <summary>Applies 3D position to the sound using multiple listeners.</summary>
        /// <param name="listeners">Positions of each listener.</param>
        /// <param name="emitter">Position of the emitter.</param>
        public void Apply3D(AudioListener[] listeners, AudioEmitter emitter)
        {
            foreach (var l in listeners)
				PlatformApply3D(l, emitter);
        }

        /// <summary>Pauses a SoundEffectInstance.</summary>
        /// <remarks>To resume a paused SoundEffectInstance, call Play.</remarks>
        public void Pause()
        {
            PlatformPause();
        }

        /// <summary>Plays or resumes a SoundEffectInstance.</summary>
        /// <remarks>If the SoundEffectInstance is paused, Play resumes playing it at the last played position. If the SoundEffectInstance is stopped, Play begins to play it.</remarks>
        /// 
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

        /// <summary>Resumes playback for a SoundEffectInstance.</summary>
        public void Resume()
        {
            PlatformResume();
        }

        /// <summary>Immediately stops playing a SoundEffectInstance.</summary>
        public void Stop()
        {
            PlatformStop(true);
        }

        /// <summary>Stops playing a SoundEffectInstance, either immediately or as authored.</summary>
        /// <param name="immediate">Whether to stop playing immediately, or to break out of the loop region and play the release. Specify true to stop playing immediately, or false to break out of the loop region and play the release phase (the remainder of the sound).</param>
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

        /// <summary>Releases unmanaged resources held by this SoundEffectInstance.</summary>
        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            PlatformDispose();
        }
    }
}
