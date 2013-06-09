/*
	Sound.cs
	 
	Author:
	      Christian Beaumont chris@foundation42.org (http://www.foundation42.com)
	
	Copyright (c) 2009 Foundation42 LLC
	
	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
*/

using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
#if !PORTABLE
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;
#endif

namespace Microsoft.Xna.Framework.Audio
{    
    /// <summary>
    /// Note: This class is part of the OpenAL implementation of the XNA sound system
    ///       and is not included in WinRT projects.
    /// </summary>
    internal class Sound : IDisposable
    {
#if !PORTABLE

        private static AudioContext context = null;
#endif
        private int sourceID;
        private int bufferID = -1;

        private bool looping;
        // when looping, to stop we must first disable looping then stop, but we still want to user to "believe" it has looping activated
        private bool loopingCtrl;
        bool disposed;

        public double Duration
        {
            get
            {
                return Seconds;
            }
        }

        public double CurrentPosition
        {
            get
            {
                float seconds = 0;
#if !PORTABLE
                AL.GetSource(sourceID, ALSourcef.SecOffset, out seconds);
#endif
                return (double)seconds;
            }

            set
            {
#if !PORTABLE
                AL.Source(sourceID, ALSourcef.SecOffset, (float)value);
#endif
                }
        }

        public bool Looping
        {
            get
            {
                return looping;
            }
            set
            {
                // just do something if it's really needed
                if (looping != value)
                {
#if !PORTABLE
                    AL.Source(sourceID, ALSourceb.Looping, value);
#endif
                    loopingCtrl = looping = value;
                }
            }
        }

        public float Pan
        {
            get;
            set;
        }

        public bool Playing
        {
            get
            {
#if !PORTABLE
                return AL.GetSourceState(sourceID) == ALSourceState.Playing;
#else
                return false;
#endif
            }

            set
            {
                // just do something if it's really needed
                if (value != Playing)
                {
                    if (value)
                        Play();
                    else
                        Stop();
                }
            }
        }

        public float Volume
        {
            get
            {
                float volume = 0;
#if !PORTABLE
                AL.GetSource(sourceID, ALSourcef.Gain, out volume);
#endif
                return volume;
            }
            set
            {
                if (value < 0f || value > 1f)
                    throw new ArgumentException("Volume should be between 0 and 1.0");
#if !PORTABLE
                AL.Source(sourceID, ALSourcef.Gain, value);
#endif
            }
        }
		
		public float Rate 
		{ 
			get; 
			set; 
		}        

        public Sound(string filename, float volume, bool looping)
        {
#if !PORTABLE
            ALFormat format;
            int size;
            int freq;
            byte[] data;
            Stream s;

            try
            {
                s = File.OpenRead(filename);
            }
            catch (IOException e)
            {
                throw new Content.ContentLoadException("Could not load audio data", e);
            }

            data = AudioLoader.Load(s, out format, out size, out freq);

            s.Close();

            Initialize(data, format, size, freq, volume, looping);
#endif
        }

        public Sound(byte[] audiodata, float volume, bool looping)
        {
#if !PORTABLE
            ALFormat format;
            int size;
            int freq;
            byte[] data;
            Stream s;

            try
            {
                s = new MemoryStream(audiodata);
            }
            catch (IOException e)
            {
                throw new Content.ContentLoadException("Could not load audio data", e);
            }

            data = AudioLoader.Load(s, out format, out size, out freq);

            s.Close();

            Initialize(data, format, size, freq, volume, looping);
#endif
        }      
		
		~Sound()
		{
			Dispose(false);	
		}

        private static void InitilizeSoundServices()
        {
#if !PORTABLE
            if (context == null)
                context = new AudioContext();
#endif
        }

        internal static void DisposeSoundServices()
        {
#if !PORTABLE
            if (context != null)
            {
                context.Dispose();
                context = null;
            }
#endif
        }

#if !PORTABLE
        private void Initialize(byte[] data, ALFormat format, int size, int frequency,
                                float volume, bool looping)
        {
            InitilizeSoundServices();

            bufferID = AL.GenBuffer();
            sourceID = AL.GenSource();

            try
            {
                // loads sound into buffer
                AL.BufferData(bufferID, format, data, size, frequency);

                // binds buffer to source
                AL.Source(sourceID, ALSourcei.Buffer, bufferID);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Volume = volume;
            this.looping = looping;
        }
#endif
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                }
                Stop();
 #if !PORTABLE
                AL.DeleteSource(sourceID);
                AL.DeleteBuffer(bufferID);
#endif
                bufferID = -1;

                disposed = true;
            }
        }

        #region Buffer Info

        private static int GetSampleSize(int bits, int channels)
        {
            return (bits / 8) * channels;
        }

        /// <summary>
        /// Gets the number of channels in buffer
        /// > 1 is valid, but buffer won’t be positioned when played
        /// </summary>
        private int Channels
        {
            get
            {
                int rv = 0;
#if !PORTABLE
                AL.GetBuffer(bufferID, ALGetBufferi.Channels, out rv);
                return rv;
#else
                return 0;
#endif
                }
        }

        /// <summary>
        /// Gets the size of buffer in bytes
        /// </summary>
        private int Size
        {
            get
            {
                int rv = 0;
#if !PORTABLE
                AL.GetBuffer(bufferID, ALGetBufferi.Size, out rv);
#endif
                return rv;
            }
        }

        /// <summary>
        /// Gets the bit depth of buffer
        /// </summary>
        private int Bits
        {
            get
            {
                int rv = 0;
#if !PORTABLE
                AL.GetBuffer(bufferID, ALGetBufferi.Bits, out rv);
#endif
                return rv;
            }
        }

        /// <summary>
        /// Gets the frequency of buffer in Hz
        /// </summary>
        private int Frequency
        {
            get
            {
                int rv = 0;
#if !PORTABLE
                AL.GetBuffer(bufferID, ALGetBufferi.Frequency, out rv);
#endif
                return rv;
            }
        }

        /// <summary>
        /// Gets the number of Samples by Size / GetSampleSize(Bits, Channels)
        /// </summary>
        private int Samples
        {
            get
            {
                return Size / GetSampleSize(Bits, Channels);
            }
        }

        /// <summary>
        /// Gets the seconds of play time in the buffer based on Samples / Frequency
        /// </summary>
        private double Seconds
        {
            get
            {
                return (double)Samples / (double)Frequency;
            }
        }

        #endregion

        public void Pause()
        {
#if !PORTABLE
            AL.SourcePause(sourceID);
#endif
        }
		
		public void Resume()
        {
            Play();
        }

        public void Play()
        {
            // if we must loop but looping was disabled
            if (looping && !loopingCtrl)
            {
#if !PORTABLE
                // then enable looping
                AL.Source(sourceID, ALSourceb.Looping, true);
#endif
                loopingCtrl = true;
            }

            // and play
#if !PORTABLE
            AL.SourcePlay(sourceID);
#endif
        }

        public void Stop()
        {
            // if looping is enabled
            if (loopingCtrl)
            {
#if !PORTABLE
                // disable it
                AL.Source(sourceID, ALSourceb.Looping, false);
#endif
                loopingCtrl = false;
            }

#if !PORTABLE
            // and stop
            AL.SourceStop(sourceID);
#endif
        }

        public static Sound CreateAndPlay(string url, float volume, bool looping)
        {
            var sound = new Sound(url, volume, looping);
            sound.Play();
            return sound;
        }
    }
}

