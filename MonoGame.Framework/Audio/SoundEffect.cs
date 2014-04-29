// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
﻿
using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Provides a loaded sound resource. You can play multiple instances of the SoundEffect by calling Play.</summary>
    /// <remarks>
    /// <para>A SoundEffect contains the audio data and metadata (such as wave data and loop information) loaded from a sound file. You can create multiple SoundEffectInstance objects, and play them from a single SoundEffect. These objects share the resources of that SoundEffect.</para>
    /// <para>You can create a SoundEffect by calling ContentManager.Load. When you make that call, use the type SoundEffect and the asset name of an audio file. The audio file must be part of the Content project. Be sure to use the SoundEffect - XNA Framework content processor.</para>
    /// <para>The only limit to the number of loaded SoundEffect objects is memory. A loaded SoundEffect will continue to hold its memory resources throughout its lifetime. All SoundEffectInstance objects created from a SoundEffect share memory resources. When a SoundEffect object is destroyed, all SoundEffectInstance objects previously created by that SoundEffect will stop playing and become invalid.</para>
    /// </remarks>
    public sealed partial class SoundEffect : IDisposable
    {
        #region Internal Audio Data

        private string _name;
        
        private bool isDisposed = false;
        private TimeSpan _duration = TimeSpan.Zero;

        #endregion

        #region Internal Constructors

        internal SoundEffect() { }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of SoundEffect based on an audio buffer, sample rate, and number of audio channels.
        /// </summary>
        /// <param name="buffer">Buffer that contains the audio data. The audio format must be PCM wave data.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz), of audio data.</param>
        /// <param name="channels">Number of channels (mono or stereo) of audio data.</param>
        public SoundEffect(byte[] buffer, int sampleRate, AudioChannels channels)
        {
            _duration = GetSampleDuration(buffer.Length, sampleRate, channels);

            PlatformInitialize(buffer, sampleRate, channels);
        }

        /// <summary>
        /// Initializes a new instance of SoundEffect with specified parameters such as audio sample rate, channels, looping criteria, and a buffer to hold the audio.
        /// </summary>
        /// <param name="buffer">Buffer that contains the audio data. The audio format must be PCM wave data.</param>
        /// <param name="offset">Offset, in bytes, to the starting position of the audio data.</param>
        /// <param name="count">Amount, in bytes, of audio data.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz), of audio data.</param>
        /// <param name="channels">Number of channels (mono or stereo) of audio data.</param>
        /// <param name="loopStart">Loop start in samples.</param>
        /// <param name="loopLength">Loop length in samples.</param>
        public SoundEffect(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            _duration = GetSampleDuration(count, sampleRate, channels);

            PlatformInitialize(buffer, offset, count, sampleRate, channels, loopStart, loopLength);
        }

        #endregion

        #region Additional SoundEffect/SoundEffectInstance Creation Methods

        /// <summary>
        /// Creates a new SoundEffectInstance for this SoundEffect.
        /// </summary>
        /// <returns>A new SoundEffectInstance for this SoundEffect.</returns>
        /// <remarks>Creating a SoundEffectInstance before calling Play allows you to access advanced playback features, such as 3D positioning.</remarks>
        public SoundEffectInstance CreateInstance()
        {
            var inst = new SoundEffectInstance();
            PlatformSetupInstance(inst);

            inst._IsPooled = false;

            return inst;
        }

        /// <summary>
        /// Creates a SoundEffect object based on the specified data stream.
        /// </summary>
        /// <param name="s">Stream object that contains the data for this SoundEffect object.</param>
        /// <returns>SoundEffect object that this method creates.</returns>
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
        /// Returns the sample duration based on the specified sample size and sample rate.
        /// </summary>
        /// <param name="sizeInBytes">Size, in bytes, of audio data.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz), of audio content. The sampleRate must be between 8000 Hz and 48000 Hz</param>
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
        /// Returns the size of the audio sample based on duration, sample rate, and audio channels.
        /// </summary>
        /// <param name="duration">TimeSpan object that contains the sample duration.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz), of audio content. The sampleRate parameter must be between 8,000 Hz and 48,000 Hz.</param>
        /// <param name="channels">Number of channels in the audio data.</param>
        /// <returns></returns>
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

        /// <summary>Plays a sound</summary>
        /// <returns>true if the sound is playing back successfully; otherwise, false.</returns>
        /// <remarks>
        /// <para>Play returns false if too many sounds already are playing.</para>
        /// <para>To loop a sound or apply 3D effects, call CreateInstance instead of Play, and SoundEffectInstance.Play.</para>
        /// <para>Sounds play in a "fire and forget" fashion with Play; therefore, the lifetime of these sounds is managed by the framework. These sounds will play once, and then stop. They cannot be looped or 3D positioned. To loop a sound or apply 3D effects, call CreateInstance instead of Play, or SoundEffectInstance.Play.</para>
        /// </remarks>
        public bool Play()
        {
            return Play(1.0f, 0.0f, 0.0f);
        }

        /// <summary>Plays a sound based on specified volume, pitch, and panning.</summary>
        /// <returns>true if the sound is playing back successfully; otherwise, false.</returns>
        /// <param name="volume">Volume, ranging from 0.0f (silence) to 1.0f (full volume). 1.0f is full volume relative to SoundEffect.MasterVolume.</param>
        /// <param name="pitch">Pitch adjustment, ranging from -1.0f (down one octave) to 1.0f (up one octave). 0.0f is unity (normal) pitch.</param>
        /// <param name="pan">Panning, ranging from -1.0f (full left) to 1.0f (full right). 0.0f is centered.</param>
        /// <remarks>
        /// <para>Play returns false if too many sounds already are playing.</para>
        /// <para>To loop a sound or apply 3D effects, call CreateInstance instead of Play, and SoundEffectInstance.Play.</para>
        /// <para>Sounds play in a "fire and forget" fashion with Play; therefore, the lifetime of these sounds is managed by the framework. These sounds will play once, and then stop. They cannot be looped or 3D positioned. To loop a sound or apply 3D effects, call CreateInstance instead of Play, or SoundEffectInstance.Play.</para>
        /// </remarks>
        public bool Play(float volume, float pitch, float pan)
        {
            if (!SoundEffectInstancePool.SoundsAvailable)
                return false;
           
            var inst = SoundEffectInstancePool.GetInstance();

            PlatformSetupInstance(inst);

            inst.Volume = volume;
            inst.Pitch = pitch;
            inst.Pan = pan;
            inst.Play();

            return true;
        }

        #endregion

        #region Public Properties

        /// <summary>Gets the duration of the SoundEffect.</summary>
        /// <remarks>This is a read-only property. The sound duration is initialized from the sound file and cannot be changed.</remarks>
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
        /// Gets or sets the master volume that affects all SoundEffectInstance sounds.
        /// </summary>
        /// <remarks>
        /// <para>The sound effect master volume affects all sound effect instances, including currently playing instances and newly created instances. Each SoundEffectInstance also has its own volume (SoundEffectInstance.Volume) that is relative to the master volume.</para>
        /// <para>You can adjust the volume of all sounds effects by changing MasterVolume. You can adjust the volume of an individual SoundEffectInstance by changing SoundEffectInstance.Volume, or by specifying an instance volume when you call Play to create the instance.</para>
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

                PlatformSetMasterVolume();
            }
        }

        static float _distanceScale = 1.0f;
        /// <summary>
        /// Gets or sets a value that adjusts the effect of distance calculations on the sound (emitter).
        /// </summary>
        /// <remarks>If sounds are attenuating too fast, which means that the sounds get quiet too quickly as they move away from the listener, you need to increase the DistanceScale. If sounds are not attenuating fast enough, decrease the DistanceScale.</remarks>
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
        /// Gets or sets a value that adjusts the effect of doppler calculations on the sound (emitter).
        /// </summary>
        /// <remarks>
        /// <para>DopplerScale changes the relative velocities of emitters and listeners.</para>
        /// <para>If sounds are shifting (pitch) too much for the given relative velocity of the emitter and listener, decrease the DopplerScale. If sounds are not shifting enough for the given relative velocity of the emitter and listener, increase the DopplerScale.</para>
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
        /// <summary>Returns the speed of sound: 343.5 meters per second.</summary>
        /// <remarks>Use this value to simulate different environments. A smaller speed of sound will exaggerate the doppler effect. A higher speed of sound will reduce the doppler effect. Speed of sound has no impact on distance attenuation.</remarks>
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

        /// <summary>Gets a value that indicates whether the object is disposed.</summary>
        public bool IsDisposed { get { return isDisposed; } }

        public void Dispose()
        {
            PlatformDispose();
        }

        #endregion

    }
}
