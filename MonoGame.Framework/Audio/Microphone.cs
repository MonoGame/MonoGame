// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Represents a physical microphone
    /// </summary>
    public sealed class Microphone
    {
        // The fixed sample rate used by the microphone class in Hz
        private const float SAMPLE_RATE = 44100f;

        // Calculate 10ms alignment value for sample rate (i.e. how many bytes of data is required every 10ms at the sample rate speed)
        private int alignOn = Convert.ToInt32(SAMPLE_RATE * 2f / 100f);

        private TimeSpan _bufferDuration;
        private static ReadOnlyCollection<Microphone> _all;
        private Microsoft.Xna.Framework.Audio.MicrophoneState _microphoneState = Microsoft.Xna.Framework.Audio.MicrophoneState.Stopped;
        private AudioCapture capture;
        private Thread pollBufferThread;

        #region Public Fields
        public string Name { get; private set; }
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the collection of all currently-available microphones
        /// </summary>
        /// <value>The collection of microphones</value>
        public static ReadOnlyCollection<Microphone> All
        { 
            get
            {
                if (_all == null)
                {
                    List<Microphone> _allList = new List<Microphone>();

                    // Fill collection with data found using OpenTK.Audio.AudioCapture.AvailableDevices
                    foreach (string deviceName in OpenTK.Audio.AudioCapture.AvailableDevices)
                    {
                        Microphone newMic = new Microphone();
                        newMic.Name = deviceName;
                        _allList.Add(newMic);
                    }
                    _all = new ReadOnlyCollection<Microphone>(_allList);
                }
                return(_all);
            }
        }

        /// <summary>
        /// Gets or sets audio capture buffer duration of the microphone
        /// </summary>
        /// <value>The duration of the audio capture buffer</value>
        public TimeSpan BufferDuration
        { 
            get
            {
                return(_bufferDuration);
            }
            set
            {
                double check = value.TotalMilliseconds;
                if (check < 100f || check > 1000f || check % 10f != 0f)
                    throw new ArgumentOutOfRangeException();

                // TODO: Once FrameworkDispatcher stub is complete we must ensure its Update 
                // method has been called at least once, or we should throw InvalidOperationException

                _bufferDuration = value;
            }
        }

        /// <summary>
        /// Returns the default attached microphone
        /// </summary>
        /// <value>The default microphone; returns null if there are no microphones attached</value>
        public static Microphone Default
        { 
            get
            {
                if (All.Count == 0)
                    return (null);

                string defaultDevice = OpenTK.Audio.AudioCapture.DefaultDevice;
                foreach (Microphone mic in All)
                {
                    if (mic.Name == defaultDevice)
                        return(mic);
                }
                return(null);
            }
        }

        /// <summary>
        /// Determines if the microphone is a wired headset or a Bluetooth device
        /// </summary>
        /// <value>If the microphone is a headset, the value is <c>true</c>; otherwise it is <c>false</c></value>
        public bool IsHeadset
        { 
            get
            {
                // TODO: Find work around as openal does not provide this information
                return(false);
            }
        }

        /// <summary>
        /// Returns the sample rate at which the microphone is capturing audio data
        /// </summary>
        /// <value>Sample rate of audio data being captured, in Hertz (Hz)</value>
        public int SampleRate
        { 
            get
            {
                return (Convert.ToInt32(SAMPLE_RATE));
            }
        }

        /// <summary>
        /// Returns the recording state of the Microphone object
        /// </summary>
        /// <value>Enumeration that indicates whether the recording state of the Microphone has started or stopped</value>
        public MicrophoneState State
        { 
            get
            {
                return(_microphoneState);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the latest recorded data from the microphone based on the audio capture buffer
        /// </summary>
        /// <returns>The buffer size, in bytes, of the audio data</returns>
        /// <param name="buffer">Buffer, in bytes, containing the captured audio data. The audio format must be PCM wave data</param>
        public int GetData(
            byte[] buffer
            )
        {
            // TODO: For Silverlight applications, once FrameworkDispatcher stub is complete we must ensure its Update 
            // method has been called at least once, or we should throw InvalidOperationException

            // Throw an ArgumentException if:
            // buffer is null, has zero length, or does not satisfy alignment requirements (sample alignment, not 10ms alignment)
            if (buffer == null || buffer.Length == 0 || buffer.Length % 2 != 0)
                throw new ArgumentOutOfRangeException();

            // Setup pinned buffer handle
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr bufferPtr = handle.AddrOfPinnedObject();

            // Read samples into buffer via pinned buffer handle pointer
            int availableSamples = capture.AvailableSamples;
            capture.ReadSamples(bufferPtr, availableSamples);

            // Tidy up
            handle.Free();

            // Return the data length
            return(availableSamples * 2);
        }

        /// <summary>
        /// Gets the latest captured audio data from the microphone based on the specified offset and byte count
        /// </summary>
        /// <returns>The buffer size, in bytes, of the audio data</returns>
        /// <param name="buffer">Buffer, in bytes, containing the captured audio data. The audio format must be PCM wave data</param>
        /// <param name="offset">Offset, in bytes, to the starting position of the data</param>
        /// <param name="count">Amount, in bytes, of desired audio data</param>
        public int GetData(
            byte[] buffer,
            int offset,
            int count
            )
        {
            // An ArgumentException should be thrown when the following arguments are invalid:
            // 1. buffer is null, has zero length, or does not satisfy alignment requirements  (sample alignment, not 10ms alignment)
            // 2. offset is less than zero, is greater than or equal to the size of the buffer, or does not satisfy alignment requirements  (sample alignment, not 10ms alignment)
            // 3. The sum of count and offset is greater than the size of the buffer, count is less than or equal to zero, 
            //    or does not satisfy alignment requirements  (sample alignment, not 10ms alignment)
            if (buffer == null || buffer.Length == 0 || buffer.Length % 2 != 0)
                throw new ArgumentOutOfRangeException();
            if (offset < 0 || offset > buffer.Length || offset % 2 != 0)
                throw new ArgumentOutOfRangeException();
            if (count <= 0 || offset + count > buffer.Length || count % 2 != 0)
                throw new ArgumentOutOfRangeException();

            // Create intermediate buffer
            // Note that the buffer is twice the size it needs to be to cater for any new samples that may
            // come in before GetData is called.
            byte[] data = new byte[capture.AvailableSamples * 4];

            // Copy specified chunk of intermediate buffer to passed buffer, and ensure we do not go out of bounds
            int dataSize = GetData(data);
            int realCount = count > dataSize ? dataSize : count;
            Array.Copy(data, offset, buffer, 0, realCount - 1);

            // Return the data size
            return(realCount);
        }

        /// <summary>
        /// Returns the duration of audio playback based on the size of the buffer
        /// </summary>
        /// <returns>TimeSpan object that represents the duration of the audio playback.</returns>
        /// <param name="sizeInBytes">Size, in bytes, of the audio data</param>
        public TimeSpan GetSampleDuration(
            int sizeInBytes
            )
        {
            return(TimeSpan.FromMilliseconds((float)sizeInBytes / 2f / SAMPLE_RATE / 1000f));
        }

        /// <summary>
        /// Returns the size of the byte array required to hold the specified duration of audio for this microphone object
        /// </summary>
        /// <returns>Size (10 ms block aligned), in bytes, of the audio buffer</returns>
        /// <param name="duration">TimeSpan object that contains the duration of the audio sample</param>
        public int GetSampleSizeInBytes(
            TimeSpan duration
            )
        {
            // Calculate actual size required for duration
            int size = Convert.ToInt32((duration.TotalMilliseconds * SAMPLE_RATE / 1000f)) * 2;           

            // If needed, add extra bytes to size to bring it up to 10ms alignment boundary
            int spill = size % alignOn;
            if(spill != 0) size += alignOn - spill;

            return(size);
        }

        /// <summary>
        /// Starts microphone audio capture
        /// </summary>
        public void Start()
        {
            // TODO: For Silverlight applications, once FrameworkDispatcher stub is complete we must ensure its Update 
            // method has been called at least once, or we should throw InvalidOperationException

            // Calc internal ring buffer size (in samples not bytes!)
            int bufferSize = Convert.ToInt32(_bufferDuration.TotalMilliseconds * SAMPLE_RATE / 1000f);

            // It seems that XNA always uses single chanel 16-bit integer PCM, so we will do the same
            capture = new AudioCapture(Name, Convert.ToInt32(SAMPLE_RATE), ALFormat.Mono16, bufferSize);
            capture.Start();
            _microphoneState = Microsoft.Xna.Framework.Audio.MicrophoneState.Started;

            // Start new thread that monitors capture.AvailableSamples > 0 and raises OnBufferReady event       
            pollBufferThread = new Thread(new ThreadStart(PollBuffer));
            pollBufferThread.Start();
        }

        /// <summary>
        /// Stops microphone audio capture
        /// </summary>
        public void Stop()
        {
            capture.Stop();
            _microphoneState = Microsoft.Xna.Framework.Audio.MicrophoneState.Stopped;
        }

        private void PollBuffer()
        {
            while (_microphoneState == MicrophoneState.Started)
            {
                if (capture.AvailableSamples > 0)
                    OnBufferReady(EventArgs.Empty);

                // 100ms is the minimum latency for this event under XNA
                Thread.Sleep(100);
            }
        }
        #endregion

        #region Public Events
        /// <summary>
        /// Event that occurs when the audio capture buffer is ready to be processed
        /// </summary>
        public event EventHandler<EventArgs> BufferReady;
        #endregion

        #region Private event raising
        private void OnBufferReady(EventArgs e)
        {
            if (BufferReady != null)
            {
                BufferReady(this, e);
            }
        }
        #endregion
    }
}

