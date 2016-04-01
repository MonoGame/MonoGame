// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Represents a single instance of a playing, paused, or stopped sound.</summary>
    /// <remarks>
    /// <para>SoundEffectInstances are created through SoundEffect.CreateInstance() and used internally by SoundEffect.Play()</para>
    /// </remarks>
    public partial class SoundEffectInstance : IDisposable
    {
        private bool _isDisposed = false;
        internal bool _isPooled = true;
        internal bool _isXAct;
        internal SoundEffect _effect;
        private float _pan;
        private float _volume;
        private float _pitch;

        /// <summary>Enables or Disables whether the SoundEffectInstance should repeat after playback.</summary>
        /// <remarks>This value has no effect on an already playing sound.</remarks>
        public bool IsLooped
        { 
            get { return PlatformGetIsLooped(); }
            set { PlatformSetIsLooped(value); }
        }

        /// <summary>Gets or sets the pan, or speaker balance..</summary>
        /// <value>Pan value ranging from -1.0 (left speaker) to 0.0 (centered), 1.0 (right speaker). Values outside of this range will throw an exception.</value>
        public float Pan
        {
            get { return _pan; } 
            set
            {
                if (value < -1.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException();

                _pan = value;
                PlatformSetPan(value);
            }
        }

        /// <summary>Gets or sets the pitch adjustment.</summary>
        /// <value>Pitch adjustment, ranging from -1.0 (down an octave) to 0.0 (no change) to 1.0 (up an octave). Values outside of this range will throw an Exception.</value>
        public float Pitch
        {
            get { return _pitch; }
            set
            {
                if (value < -1.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException();

                _pitch = value;
                PlatformSetPitch(value);
            }
        }

        /// <summary>Gets or sets the volume of the SoundEffectInstance.</summary>
        /// <value>Volume, ranging from 0.0 (silence) to 1.0 (full volume). Volume during playback is scaled by SoundEffect.MasterVolume.</value>
        /// <remarks>
        /// This is the volume relative to SoundEffect.MasterVolume. Before playback, this Volume property is multiplied by SoundEffect.MasterVolume when determining the final mix volume.
        /// </remarks>
        public float Volume
        {
            get { return _volume; }
            set
            {
                // XAct sound effects don't have volume limits.
                if (!_isXAct && (value < 0.0f || value > 1.0f))
                    throw new ArgumentOutOfRangeException();

                _volume = value;

                // XAct sound effects are not tied to the SoundEffect master volume.
                if (_isXAct)
                    PlatformSetVolume(value);
                else
                    PlatformSetVolume(value * SoundEffect.MasterVolume);
            }
        }

        /// <summary>Gets the SoundEffectInstance's current playback state.</summary>
        public SoundState State { get { return PlatformGetState(); } }

        /// <summary>Indicates whether the object is disposed.</summary>
        public bool IsDisposed { get { return _isDisposed; } }

        internal SoundEffectInstance()
        {
            _pan = 0.0f;
            _volume = 1.0f;
            _pitch = 0.0f;            
        }

        internal SoundEffectInstance(byte[] buffer, int sampleRate, int channels)
            : this()
        {
            PlatformInitialize(buffer, sampleRate, channels);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Microsoft.Xna.Framework.Audio.SoundEffectInstance"/> is reclaimed by garbage collection.
        /// </summary>
        ~SoundEffectInstance()
        {
            Dispose(false);
        }

        /// <summary>Applies 3D positioning to the SoundEffectInstance using a single listener.</summary>
        /// <param name="listener">Data about the listener.</param>
        /// <param name="emitter">Data about the source of emission.</param>
        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            PlatformApply3D(listener, emitter);
        }

        /// <summary>Applies 3D positioning to the SoundEffectInstance using multiple listeners.</summary>
        /// <param name="listeners">Data about each listener.</param>
        /// <param name="emitter">Data about the source of emission.</param>
        public void Apply3D(AudioListener[] listeners, AudioEmitter emitter)
        {
            foreach (var l in listeners)
				PlatformApply3D(l, emitter);
        }

        /// <summary>Pauses playback of a SoundEffectInstance.</summary>
        /// <remarks>Paused instances can be resumed with SoundEffectInstance.Play() or SoundEffectInstance.Resume().</remarks>
        public void Pause()
        {
            PlatformPause();
        }

        /// <summary>Plays or resumes a SoundEffectInstance.</summary>
        /// <remarks>Throws an exception if more sounds are playing than the platform allows.</remarks>
        public void Play()
        {
            if (State == SoundState.Playing)
                return;

            // We don't need to check if we're at the instance play limit
            // if we're resuming from a paused state.
            if (State != SoundState.Paused)
            {
                if (!SoundEffectInstancePool.SoundsAvailable)
                    throw new InstancePlayLimitException();

                SoundEffectInstancePool.Remove(this);
            }
            
            // For non-XAct sounds we need to be sure the latest
            // master volume level is applied before playback.
            if (!_isXAct)
                PlatformSetVolume(_volume * SoundEffect.MasterVolume);

            PlatformPlay();
        }

        /// <summary>Resumes playback for a SoundEffectInstance.</summary>
        /// <remarks>Only has effect on a SoundEffectInstance in a paused state.</remarks>
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
        /// <param name="immediate">Determined whether the sound stops immediately, or after playing its release phase and/or transitions.</param>
        /// <remarks>Stopping a sound with the immediate argument set to false will allow it to play any release phases, such as fade, before coming to a stop.</remarks>
        public void Stop(bool immediate)
        {
            PlatformStop(immediate);
        }

        /// <summary>Releases the resources held by this <see cref="Microsoft.Xna.Framework.Audio.SoundEffectInstance"/>.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the resources held by this <see cref="Microsoft.Xna.Framework.Audio.SoundEffectInstance"/>.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c>, Dispose was called explicitly.</param>
        /// <remarks>If the disposing parameter is true, the Dispose method was called explicitly. This
        /// means that managed objects referenced by this instance should be disposed or released as
        /// required.  If the disposing parameter is false, Dispose was called by the finalizer and
        /// no managed objects should be touched because we do not know if they are still valid or
        /// not at that time.  Unmanaged resources should always be released.</remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                PlatformDispose(disposing);
                _isDisposed = true;
            }
        }
    }
}
