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

        private string _name = string.Empty;
        
        private bool _isDisposed = false;
        private TimeSpan _duration = TimeSpan.Zero;

        #endregion

        #region Internal Constructors

        internal SoundEffect() {}

        internal SoundEffect(byte[] buffer, int format, int sampleRate, int channels, int blockAlignment, int durationMs, int loopStart, int loopLength)
        {
            _duration = TimeSpan.FromMilliseconds(durationMs);

            // This is regular PCM data.
            if (format == 1)
            {
                PlatformInitializePCM(buffer, 0, buffer.Length, sampleRate, (AudioChannels) channels, loopStart, loopLength);
                return;
            }

            // Everything else is platform specific.
            PlatformInitializeFormat(buffer, format, sampleRate, channels, blockAlignment, loopStart, loopLength);
        }

        #endregion

        #region Public Constructors

        /// <param name="buffer">Buffer containing 16bit PCM wave data.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz)</param>
        /// <param name="channels">Number of channels (mono or stereo).</param>
        public SoundEffect(byte[] buffer, int sampleRate, AudioChannels channels)
        {
            if (sampleRate < 8000 || sampleRate > 48000)
                throw new ArgumentOutOfRangeException("sampleRate");
            if ((int)channels != 1 && (int)channels != 2)
                throw new ArgumentOutOfRangeException("channels");

            if (buffer == null || buffer.Length == 0)
                throw new ArgumentException("Ensure that the buffer length is non-zero.", "buffer");

            // Make sure the buffer length matches the block alignment.
            var blockAlign = (int)channels * 2;
            if ((buffer.Length % blockAlign) != 0)
                throw new ArgumentException("Ensure that the buffer meets the block alignment requirements for the number of channels.", "buffer");

            _duration = GetSampleDuration(buffer.Length, sampleRate, channels);

            // Use the total sample count for the loop length.
            var loopLength = buffer.Length / blockAlign;

            PlatformInitializePCM(buffer, 0, buffer.Length, sampleRate, channels, 0, loopLength);
        }

        /// <param name="buffer">Buffer containing 16bit PCM wave data.</param>
        /// <param name="offset">Offset, in bytes, to the starting position of the audio data.</param>
        /// <param name="count">Amount, in bytes, of audio data.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz)</param>
        /// <param name="channels">Number of channels (mono or stereo).</param>
        /// <param name="loopStart">The position, in samples, where the audio should begin looping.</param>
        /// <param name="loopLength">The duration, in samples, that audio should loop over.</param>
        /// <remarks>Use SoundEffect.GetSampleDuration() to convert time to samples.</remarks>
        public SoundEffect(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            if (sampleRate < 8000 || sampleRate > 48000)
                throw new ArgumentOutOfRangeException("sampleRate");
            if ((int)channels != 1 && (int)channels != 2)
                throw new ArgumentOutOfRangeException("channels");

            if (buffer == null || buffer.Length == 0)
                throw new ArgumentException("Ensure that the buffer length is non-zero.", "buffer");
            var blockAlign = (int)channels * 2;
            if ((buffer.Length % blockAlign) != 0)
                throw new ArgumentException("Ensure that the buffer meets the block alignment requirements for the number of channels.", "buffer");

            if (count <= 0)
                throw new ArgumentException("Ensure that the count is greater than zero.", "count");
            if ((count % blockAlign) != 0)
                throw new ArgumentException("Ensure that the count meets the block alignment requirements for the number of channels.", "count");

            if (offset < 0)
                throw new ArgumentException("The offset cannot be negative.", "offset");
            if (((ulong)count + (ulong)offset) > (ulong)buffer.Length)
                throw new ArgumentException("Ensure that the offset+count region lines within the buffer.", "offset");

            var totalSamples = buffer.Length / blockAlign;

            if (loopStart < 0)
                throw new ArgumentException("The loopStart cannot be negative.", "loopStart");
            if (loopStart > totalSamples)
                throw new ArgumentException("The loopStart cannot be greater than the total number of samples.", "loopStart");

            if (loopLength == 0)
                loopLength = totalSamples - loopStart;

            if (loopLength < 0)
                throw new ArgumentException("The loopLength cannot be negative.", "loopLength");
            if (((ulong)loopStart + (ulong)loopLength) > (ulong)totalSamples)
                throw new ArgumentException("Ensure that the loopStart+loopLength region lies within the sample range.", "loopLength");

            _duration = GetSampleDuration(count, sampleRate, channels);

            PlatformInitializePCM(buffer, offset, count, sampleRate, channels, loopStart, loopLength);
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
        /// Returns the duration for 16bit PCM audio.
        /// </summary>
        /// <param name="sizeInBytes">The length of the audio data in bytes.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz). Must be between 8000 Hz and 48000 Hz</param>
        /// <param name="channels">Number of channels in the audio data.</param>
        /// <returns>The duration of the audio data.</returns>
        public static TimeSpan GetSampleDuration(int sizeInBytes, int sampleRate, AudioChannels channels)
        {
            if (sizeInBytes < 0)
                throw new ArgumentException("Buffer size cannot be negative.", "sizeInBytes");
            if (sampleRate < 8000 || sampleRate > 48000)
                throw new ArgumentOutOfRangeException("sampleRate");

            var numChannels = (int)channels;
            if (numChannels != 1 && numChannels != 2)
                throw new ArgumentOutOfRangeException("channels");

            if (sizeInBytes == 0)
                return TimeSpan.Zero;

            // Reference
            // http://tinyurl.com/hq9slfy

            var dur = sizeInBytes / (sampleRate * numChannels * 16f / 8f);

            var duration = TimeSpan.FromSeconds(dur);

            return duration;
        }

        /// <summary>
        /// Returns the data size in bytes for 16bit PCM audio.
        /// </summary>
        /// <param name="duration">The total duration of the audio data.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz), of audio data. Must be between 8,000 and 48,000 Hz.</param>
        /// <param name="channels">Number of channels in the audio data.</param>
        /// <returns>The size in bytes of a single sample of audio data.</returns>
        public static int GetSampleSizeInBytes(TimeSpan duration, int sampleRate, AudioChannels channels)
        {
            if (duration < TimeSpan.Zero || duration > TimeSpan.FromMilliseconds(0x7FFFFFF))
                throw new ArgumentOutOfRangeException("duration");
            if (sampleRate < 8000 || sampleRate > 48000)
                throw new ArgumentOutOfRangeException("sampleRate");

            var numChannels = (int)channels;
            if (numChannels != 1 && numChannels != 2)
                throw new ArgumentOutOfRangeException("channels");

            // Reference
            // http://tinyurl.com/hq9slfy

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
