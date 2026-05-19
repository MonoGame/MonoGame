// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Microphone state. 
    /// </summary>
    public enum MicrophoneState
    {
        /// <summary>
        /// The <see cref="Microphone"/> audio capture has started.
        /// </summary>
        Started,
        /// <summary>
        /// The <see cref="Microphone"/> audio capture has stopped.
        /// </summary>
        Stopped
    }

    /// <summary>
    /// Provides microphones capture features. 
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

        private TimeSpan _bufferDuration = TimeSpan.FromMilliseconds(1000.0);

        /// <summary>
        /// Gets or sets the capture buffer duration. This value must be greater than 100 milliseconds, lower than 1000 milliseconds, and must be 10 milliseconds aligned (BufferDuration % 10 == 10).
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
        /// Determines if the microphone is a wired headset.
        /// Note: XNA could know if a headset microphone was plugged in an Xbox 360 controller but MonoGame can't.
        /// Hence, this is always true on mobile platforms, and always false otherwise.
        /// </summary>
        public bool IsHeadset
        {
            get { return _isHeadset; }
        }

        private int _sampleRate = 44100; // XNA default is 44100, don't know if it supports any other rates

        /// <summary>
        /// Returns the sample rate of the captured audio.
        /// Note: default value is 44100hz
        /// </summary>
        public int SampleRate
        {
            get { return _sampleRate; }
        }

        private MicrophoneState _state = MicrophoneState.Stopped;

        /// <summary>
        /// Returns the state of the Microphone. 
        /// </summary>
        public MicrophoneState State
        {
            get { return _state; }
        }

        #endregion

        #region Static Members

        private static List<Microphone> _allMicrophones = null;

        /// <summary>
        /// Returns all compatible microphones.
        /// </summary>
        public static ReadOnlyCollection<Microphone> All
        {
            get
            {
                SoundEffect.Initialize();                
                if (_allMicrophones == null)
                    _allMicrophones = new List<Microphone>();
                return new ReadOnlyCollection<Microphone>(_allMicrophones);
            }
        }

        private static Microphone _default = null;

        /// <summary>
        /// Returns the default microphone.
        /// </summary>
        public static Microphone Default
        {
            get { SoundEffect.Initialize(); return _default; }
        }       

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the duration based on the size of the buffer (assuming 16-bit PCM data).
        /// </summary>
        /// <param name="sizeInBytes">Size, in bytes</param>
        /// <returns>TimeSpan of the duration.</returns>
        public TimeSpan GetSampleDuration(int sizeInBytes)
        {
            // this should be 10ms aligned
            // this assumes 16bit mono data
            return SoundEffect.GetSampleDuration(sizeInBytes, _sampleRate, AudioChannels.Mono);
        }

        /// <summary>
        /// Returns the size, in bytes, of the array required to hold the specified duration of 16-bit PCM data. 
        /// </summary>
        /// <param name="duration">TimeSpan of the duration of the sample.</param>
        /// <returns>Size, in bytes, of the buffer.</returns>
        public int GetSampleSizeInBytes(TimeSpan duration)
        {
            // this should be 10ms aligned
            // this assumes 16bit mono data
            return SoundEffect.GetSampleSizeInBytes(duration, _sampleRate, AudioChannels.Mono);
        }

        /// <summary>
        /// Starts microphone capture.
        /// </summary>
        public void Start()
        {
            PlatformStart();
        }

        /// <summary>
        /// Stops microphone capture.
        /// </summary>
        public void Stop()
        {
            PlatformStop();
        }

        /// <summary>
        /// Gets the latest available data from the microphone.
        /// </summary>
        /// <param name="buffer">Buffer, in bytes, of the captured data (16-bit PCM).</param>
        /// <returns>The buffer size, in bytes, of the captured data.</returns>
        public int GetData(byte[] buffer)
        {
            return GetData(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Gets the latest available data from the microphone.
        /// </summary>
        /// <param name="buffer">Buffer, in bytes, of the captured data (16-bit PCM).</param>
        /// <param name="offset">Byte offset.</param>
        /// <param name="count">Amount, in bytes.</param>
        /// <returns>The buffer size, in bytes, of the captured data.</returns>
        public int GetData(byte[] buffer, int offset, int count)
        {
            return PlatformGetData(buffer, offset, count);
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Event fired when the audio data are available.
        /// </summary>
        public event EventHandler<EventArgs> BufferReady;

        #endregion

        #region Static Methods

        internal static void UpdateMicrophones()
        {
            // querying all running microphones for new samples available
            if (_allMicrophones != null)
                for (int i = 0; i < _allMicrophones.Count; i++)
                    _allMicrophones[i].Update();
        }

        internal static void StopMicrophones()
        {
            // stopping all running microphones before shutting down audio devices
            if (_allMicrophones != null)
                for (int i = 0; i < _allMicrophones.Count; i++)
                    _allMicrophones[i].Stop();
        }

        #endregion
    }
}
