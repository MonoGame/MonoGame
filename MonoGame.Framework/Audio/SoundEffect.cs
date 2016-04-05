// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
﻿
using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Represents a loaded sound resource.</summary>
    /// <remarks>
    /// <para>A SoundEffect represents the buffer used to hold audio data and metadata. SoundEffectInstances are used to play from SoundEffects. Multiple SoundEffectinstance objects can be created and played from the same SoundEffect object.</para>
    /// <para>The only limit on the number of loaded SoundEffects is restricted by available memory. When a SoundEffect is disposed, all SoundEffectInstances created from it will become invalid.</para>
    /// <para>SoundEffect.Play() can be used for 'fire and forget' sounds. If advanced playback controls like volume or pitch is required, use SoundEffect.CreateInstance().</para>
    /// </remarks>
    public sealed partial class SoundEffect : IDisposable
    {
        #region Internal Audio Data

        private string _name;
        
        private bool _isDisposed = false;
        private TimeSpan _duration = TimeSpan.Zero;

        #endregion

        #region Internal Constructors

        internal SoundEffect() { }

        #endregion

        #region Public Constructors

        /// <param name="buffer">Buffer containing PCM wave data.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz)</param>
        /// <param name="channels">Number of channels (mono or stereo).</param>
        public SoundEffect(byte[] buffer, int sampleRate, AudioChannels channels)
        {
            _duration = GetSampleDuration(buffer.Length, sampleRate, channels);

            PlatformInitialize(buffer, sampleRate, channels);
        }

        /// <param name="buffer">Buffer containing PCM wave data.</param>
        /// <param name="offset">Offset, in bytes, to the starting position of the audio data.</param>
        /// <param name="count">Amount, in bytes, of audio data.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz)</param>
        /// <param name="channels">Number of channels (mono or stereo).</param>
        /// <param name="loopStart">The position, in samples, where the audio should begin looping.</param>
        /// <param name="loopLength">The duration, in samples, that audio should loop over.</param>
        /// <remarks>Use SoundEffect.GetSampleDuration() to convert time to samples.</remarks>
        public SoundEffect(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            _duration = GetSampleDuration(count, sampleRate, channels);

            PlatformInitialize(buffer, offset, count, sampleRate, channels, loopStart, loopLength);
        }

        #endregion

        #region Finalizer

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Microsoft.Xna.Framework.Audio.SoundEffect"/> is reclaimed by garbage collection.
        /// </summary>
        ~SoundEffect()
        {
            Dispose(false);
        }

        #endregion

        #region Additional SoundEffect/SoundEffectInstance Creation Methods

        /// <summary>
        /// Creates a new SoundEffectInstance for this SoundEffect.
        /// </summary>
        /// <returns>A new SoundEffectInstance for this SoundEffect.</returns>
        /// <remarks>Creating a SoundEffectInstance before calling SoundEffectInstance.Play() allows you to access advanced playback features, such as volume, pitch, and 3D positioning.</remarks>
        public SoundEffectInstance CreateInstance()
        {
            var inst = new SoundEffectInstance();
            PlatformSetupInstance(inst);

            inst._isPooled = false;
            inst._effect = this;

            return inst;
        }

        /// <summary>
        /// Creates a SoundEffect object based on the specified data stream.
        /// </summary>
        /// <param name="s">Stream object containing PCM wave data.</param>
        /// <returns>A new SoundEffect object.</returns>
        /// <remarks>The Stream object must point to the head of a valid PCM wave file. Also, this wave file must be in the RIFF bitstream format.</remarks>
        public static SoundEffect FromStream(Stream s)
        {
            if (s == null)
                throw new ArgumentNullException();

            // Notes from the docs:

            /*The Stream object must point to the head of a valid PCM wave file. Also, this wave file must be in the RIFF bitstream format.
              The audio format has the following restrictions:
              Must be a PCM wave file
              Can only be mono or stereo
              Must be 8 or 16 bit
              Sample rate must be between 8,000 Hz and 48,000 Hz*/

            var sfx = new SoundEffect();

            sfx.PlatformLoadAudioStream(s);

            return sfx;
        }

        /// <summary>
        /// Gets the TimeSpan representation of a single sample.
        /// </summary>
        /// <param name="sizeInBytes">Size, in bytes, of audio data.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz). Must be between 8000 Hz and 48000 Hz</param>
        /// <param name="channels">Number of channels in the audio data.</param>
        /// <returns>TimeSpan object that represents the calculated sample duration.</returns>
        public static TimeSpan GetSampleDuration(int sizeInBytes, int sampleRate, AudioChannels channels)
        {
            if (sampleRate < 8000 || sampleRate > 48000)
                throw new ArgumentOutOfRangeException();

            // Reference: http://social.msdn.microsoft.com/Forums/windows/en-US/5a92be69-3b4e-4d92-b1d2-141ef0a50c91/how-to-calculate-duration-of-wave-file-from-its-size?forum=winforms
            var numChannels = (int)channels;

            var dur = sizeInBytes / (sampleRate * numChannels * 16f / 8f);

            var duration = TimeSpan.FromSeconds(dur);

            return duration;
        }

        /// <summary>
        /// Gets the size of a sample from a TimeSpan.
        /// </summary>
        /// <param name="duration">TimeSpan object that contains the sample duration.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz), of audio data. Must be between 8,000 and 48,000 Hz.</param>
        /// <param name="channels">Number of channels in the audio data.</param>
        /// <returns>Size of a single sample of audio data.</returns>
        public static int GetSampleSizeInBytes(TimeSpan duration, int sampleRate, AudioChannels channels)
        {
            if (sampleRate < 8000 || sampleRate > 48000)
                throw new ArgumentOutOfRangeException();

            // Reference: http://social.msdn.microsoft.com/Forums/windows/en-US/5a92be69-3b4e-4d92-b1d2-141ef0a50c91/how-to-calculate-duration-of-wave-file-from-its-size?forum=winforms

            var numChannels = (int)channels;

            var sizeInBytes = duration.TotalSeconds * (sampleRate * numChannels * 16f / 8f);

            return (int)sizeInBytes;
        }

        #endregion

        #region Play

        /// <summary>Gets an internal SoundEffectInstance and plays it.</summary>
        /// <returns>True if a SoundEffectInstance was successfully played, false if not.</returns>
        /// <remarks>
        /// <para>Play returns false if more SoundEffectInstances are currently playing then the platform allows.</para>
        /// <para>To loop a sound or apply 3D effects, call SoundEffect.CreateInstance() and SoundEffectInstance.Play() instead.</para>
        /// <para>SoundEffectInstances used by SoundEffect.Play() are pooled internally.</para>
        /// </remarks>
        public bool Play()
        {
            var inst = GetPooledInstance(false);
            if (inst == null)
                return false;

            inst.Play();

            return true;
        }

        /// <summary>Gets an internal SoundEffectInstance and plays it with the specified volume, pitch, and panning.</summary>
        /// <returns>True if a SoundEffectInstance was successfully created and played, false if not.</returns>
        /// <param name="volume">Volume, ranging from 0.0 (silence) to 1.0 (full volume). Volume during playback is scaled by SoundEffect.MasterVolume.</param>
        /// <param name="pitch">Pitch adjustment, ranging from -1.0 (down an octave) to 0.0 (no change) to 1.0 (up an octave).</param>
        /// <param name="pan">Panning, ranging from -1.0 (left speaker) to 0.0 (centered), 1.0 (right speaker).</param>
        /// <remarks>
        /// <para>Play returns false if more SoundEffectInstances are currently playing then the platform allows.</para>
        /// <para>To apply looping or simulate 3D audio, call SoundEffect.CreateInstance() and SoundEffectInstance.Play() instead.</para>
        /// <para>SoundEffectInstances used by SoundEffect.Play() are pooled internally.</para>
        /// </remarks>
        public bool Play(float volume, float pitch, float pan)
        {
            var inst = GetPooledInstance(false);
            if (inst == null)
                return false;

            inst.Volume = volume;
            inst.Pitch = pitch;
            inst.Pan = pan;

            inst.Play();

            return true;
        }

        /// <summary>
        /// Returns a sound effect instance from the pool or null if none are available.
        /// </summary>
        internal SoundEffectInstance GetPooledInstance(bool forXAct)
        {
            if (!SoundEffectInstancePool.SoundsAvailable)
                return null;

            var inst = SoundEffectInstancePool.GetInstance(forXAct);
            inst._effect = this;
            PlatformSetupInstance(inst);

            return inst;
        }

        #endregion

        #region Public Properties

        /// <summary>Gets the duration of the SoundEffect.</summary>
        public TimeSpan Duration { get { return _duration; } }

        /// <summary>Gets or sets the asset name of the SoundEffect.</summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        #endregion

        #region Static Members

        static float _masterVolume = 1.0f;
        /// <summary>
        /// Gets or sets the master volume scale applied to all SoundEffectInstances.
        /// </summary>
        /// <remarks>
        /// <para>Each SoundEffectInstance has its own Volume property that is independent to SoundEffect.MasterVolume. During playback SoundEffectInstance.Volume is multiplied by SoundEffect.MasterVolume.</para>
        /// <para>This property is used to adjust the volume on all current and newly created SoundEffectInstances. The volume of an individual SoundEffectInstance can be adjusted on its own.</para>
        /// </remarks>
        public static float MasterVolume 
        { 
            get { return _masterVolume; }
            set
            {
                if (value < 0.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException();

                if (_masterVolume == value)
                    return;
                
                _masterVolume = value;
                SoundEffectInstancePool.UpdateMasterVolume();
            }
        }

        static float _distanceScale = 1.0f;
        /// <summary>
        /// Gets or sets the scale of distance calculations.
        /// </summary>
        /// <remarks> 
        /// <para>DistanceScale defaults to 1.0 and must be greater than 0.0.</para>
        /// <para>Higher values reduce the rate of falloff between the sound and listener.</para>
        /// </remarks>
        public static float DistanceScale
        {
            get { return _distanceScale; }
            set
            {
                if (value <= 0f)
                    throw new ArgumentOutOfRangeException ("value of DistanceScale");

                _distanceScale = value;
            }
        }

        static float _dopplerScale = 1f;
        /// <summary>
        /// Gets or sets the scale of Doppler calculations applied to sounds.
        /// </summary>
        /// <remarks>
        /// <para>DopplerScale defaults to 1.0 and must be greater or equal to 0.0</para>
        /// <para>Affects the relative velocity of emitters and listeners.</para>
        /// <para>Higher values more dramatically shift the pitch for the given relative velocity of the emitter and listener.</para>
        /// </remarks>
        public static float DopplerScale
        {
            get { return _dopplerScale; }
            set
            {
                // As per documenation it does not look like the value can be less than 0
                //   although the documentation does not say it throws an error we will anyway
                //   just so it is like the DistanceScale
                if (value < 0.0f)
                    throw new ArgumentOutOfRangeException ("value of DopplerScale");

                _dopplerScale = value;
            }
        }

        static float speedOfSound = 343.5f;
        /// <summary>Returns the speed of sound used when calculating the Doppler effect..</summary>
        /// <remarks>
        /// <para>Defaults to 343.5. Value is measured in meters per second.</para>
        /// <para>Has no effect on distance attenuation.</para>
        /// </remarks>
        public static float SpeedOfSound
        {
            get { return speedOfSound; }
            set
            {
                if (value <= 0.0f)
                    throw new ArgumentOutOfRangeException();

                speedOfSound = value;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>Indicates whether the object is disposed.</summary>
        public bool IsDisposed { get { return _isDisposed; } }

        /// <summary>Releases the resources held by this <see cref="Microsoft.Xna.Framework.Audio.SoundEffect"/>.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the resources held by this <see cref="Microsoft.Xna.Framework.Audio.SoundEffect"/>.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c>, Dispose was called explicitly.</param>
        /// <remarks>If the disposing parameter is true, the Dispose method was called explicitly. This
        /// means that managed objects referenced by this instance should be disposed or released as
        /// required.  If the disposing parameter is false, Dispose was called by the finalizer and
        /// no managed objects should be touched because we do not know if they are still valid or
        /// not at that time.  Unmanaged resources should always be released.</remarks>
        void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                SoundEffectInstancePool.StopPooledInstances(this);
                PlatformDispose(disposing);
                _isDisposed = true;
            }
        }

        #endregion

    }
}
