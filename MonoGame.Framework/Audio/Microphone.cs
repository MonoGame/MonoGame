// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Enumeration that indicates whether the recording state of the Microphone has started or stopped. 
    /// </summary>
    public enum MicrophoneState
    {
        Started,
        Stopped
    }

    /// <summary>
    /// Provides properties, methods, and fields and events for capturing audio data with microphones. 
    /// </summary>
    public sealed partial class Microphone
    {
        #region Internal Constructors

        internal Microphone()
        {

        }

        internal Microphone(string name)
        {
            Name = name;
        }

        #endregion

        #region Public Fields

        /// <summary>
        /// Returns the friendly name of the microphone.
        /// </summary>
        public readonly string Name;

        #endregion

        #region Public Properties

        private TimeSpan _bufferDuration = TimeSpan.FromMilliseconds(500.0); // what's the XNA default? 

        /// <summary>
        /// Gets or sets audio capture buffer duration of the microphone.
        /// </summary>
        public TimeSpan BufferDuration
        {
            get { return _bufferDuration; }
            set
            {
                if (value.TotalMilliseconds < 100 || value.TotalMilliseconds > 1000)
                    throw new ArgumentOutOfRangeException("Buffer duration must be a value between 100 and 1000 milliseconds.");
                if (value.TotalMilliseconds % 10 != 0)
                    throw new ArgumentOutOfRangeException("Buffer duration must be 10ms aligned (BufferDuration % 10 == 0)");
                _bufferDuration = value;
            }
        }

        // always true on mobile, this can't be queried on any platform (it was most probably only set to true if the headset was plugged in an XInput controller)
#if IOS || ANDROID
        private const bool _isHeadset = true;
#else
        private const bool _isHeadset = false;
#endif
        /// <summary>
        /// Determines if the microphone is a wired headset or a Bluetooth device.
        /// Note: this is always true on mobile platforms, and always false otherwise
        /// </summary>
        public bool IsHeadset
        {
            get { return _isHeadset; }
        }

        private int _sampleRate = 44100; // XNA default is 44100, don't know if it supports any other rates

        /// <summary>
        /// Returns the sample rate at which the microphone is capturing audio data. 
        /// </summary>
        public int SampleRate
        {
            get { return _sampleRate; }
        }

        private MicrophoneState _state = MicrophoneState.Stopped;

        /// <summary>
        /// Returns the recording state of the Microphone object. 
        /// </summary>
        public MicrophoneState State
        {
            get { return _state; }
        }

        #endregion

        #region Static Members

        private static List<Microphone> _allMicrophones = null;

        /// <summary>
        /// Returns the collection of all currently-available microphones.
        /// </summary>
        public static ReadOnlyCollection<Microphone> All
        {
            get
            {
                if (_allMicrophones == null)
                    _allMicrophones = new List<Microphone>();
                return _allMicrophones.AsReadOnly();
            }
        }

        private static Microphone _default = null;

        /// <summary>
        /// Returns the default attached microphone.
        /// </summary>
        public static Microphone Default
        {
            get { return _default; }
        }       

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the duration of audio playback based on the size of the buffer.
        /// </summary>
        /// <param name="sizeInBytes">Size, in bytes, of the audio data.</param>
        /// <returns>TimeSpan object that represents the duration of the audio playback.</returns>
        public TimeSpan GetSampleDuration(int sizeInBytes)
        {
            // this should be 10ms aligned
            // this assumes 16bit mono data
            return SoundEffect.GetSampleDuration(sizeInBytes, _sampleRate, AudioChannels.Mono);
        }

        /// <summary>
        /// Returns the size of the byte array required to hold the specified duration of audio for this microphone object. 
        /// </summary>
        /// <param name="duration">TimeSpan object that contains the duration of the audio sample. </param>
        /// <returns>Size (10 ms block aligned), in bytes, of the audio buffer.</returns>
        public int GetSameSizeInBytes(TimeSpan duration)
        {
            // this should be 10ms aligned
            // this assumes 16bit mono data
            return SoundEffect.GetSampleSizeInBytes(duration, _sampleRate, AudioChannels.Mono);
        }

        /// <summary>
        /// Starts microphone audio capture.
        /// </summary>
        public void Start()
        {
            PlatformStart();
        }

        /// <summary>
        /// Stops microphone audio capture.
        /// </summary>
        public void Stop()
        {
            PlatformStop();
        }

        /// <summary>
        /// Gets the latest recorded data from the microphone based on the audio capture buffer.
        /// </summary>
        /// <param name="buffer">Buffer, in bytes, containing the captured audio data. The audio format must be PCM wave data.</param>
        /// <returns>The buffer size, in bytes, of the audio data.</returns>
        public int GetData(byte[] buffer)
        {
            return GetData(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Gets the latest captured audio data from the microphone based on the specified offset and byte count.
        /// </summary>
        /// <param name="buffer">Buffer, in bytes, containing the captured audio data. The audio format must be PCM wave data.</param>
        /// <param name="offset">Offset, in bytes, to the starting position of the data. </param>
        /// <param name="count">Amount, in bytes, of desired audio data. </param>
        /// <returns>The buffer size, in bytes, of the audio data.</returns>
        public int GetData(byte[] buffer, int offset, int count)
        {
            return PlatformGetData(buffer, offset, count);
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Event that occurs when the audio capture buffer is ready to processed.
        /// </summary>
        public event EventHandler<EventArgs> BufferReady;

        #endregion

        #region Static Methods

        internal static void UpdateMicrophones()
        {
            // querying all running microphones for new samples available
            for (int i = 0; i < _allMicrophones.Count; i++)
                _allMicrophones[i].Update();
        }

        internal static void StopMicrophones()
        {
            // stopping all running microphones before shutting down audio devices
            for (int i = 0; i < _allMicrophones.Count; i++)
                _allMicrophones[i].Stop();
        }

        #endregion
    }
}
