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

using OpenTK.Audio.OpenAL;
using OpenTK.Audio;

namespace Microsoft.Xna.Framework.Audio
{    
    /// <summary>
    /// Note: This class is part of the OpenAL implementation of the XNA sound system
    ///       and is not included in WinRT projects.
    /// </summary>
    internal class Sound : IDisposable
    {
        private static AudioContext context = null;
        private int sourceID;
        private int bufferID = -1;

        private bool looping;
        // when looping, to stop we must first disable looping then stop, but we still want to user to "believe" it has looping activated
        private bool loopingCtrl;

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
                float seconds;
                AL.GetSource(sourceID, ALSourcef.SecOffset, out seconds);
                return (double)seconds;
            }

            set
            {
                AL.Source(sourceID, ALSourcef.SecOffset, (float)value);
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
                    AL.Source(sourceID, ALSourceb.Looping, value);
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
                return AL.GetSourceState(sourceID) == ALSourceState.Playing;
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
                float volume;
                AL.GetSource(sourceID, ALSourcef.Gain, out volume);
                return volume;
            }
            set
            {
                if (value < 0f || value > 1f)
                    throw new ArgumentException("Volume should be between 0 and 1.0");

                AL.Source(sourceID, ALSourcef.Gain, value);
            }
        }
		
		public float Rate 
		{ 
			get; 
			set; 
		}        

        public Sound(string filename, float volume, bool looping)
        {
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
        }

        public Sound(byte[] audiodata, float volume, bool looping)
        {
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
        }      
		
		~Sound()
		{
			Dispose();	
		}

        private static void InitilizeSoundServices()
        {
            if (context == null)
                context = new AudioContext();
        }

        internal static void DisposeSoundServices()
        {
            if (context != null)
            {
                context.Dispose();
                context = null;
            }
        }

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

        public void Dispose()
        {
            if (bufferID == -1)
                return;

            Stop();


            AL.DeleteSource(sourceID);
            AL.DeleteBuffer(bufferID);
            bufferID = -1;
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
                int rv;
                AL.GetBuffer(bufferID, ALGetBufferi.Channels, out rv);
                return rv;
            }
        }

        /// <summary>
        /// Gets the size of buffer in bytes
        /// </summary>
        private int Size
        {
            get
            {
                int rv;
                AL.GetBuffer(bufferID, ALGetBufferi.Size, out rv);
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
                int rv;
                AL.GetBuffer(bufferID, ALGetBufferi.Bits, out rv);
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
                int rv;
                AL.GetBuffer(bufferID, ALGetBufferi.Frequency, out rv);
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
            AL.SourcePause(sourceID);
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
                // then enable looping
                AL.Source(sourceID, ALSourceb.Looping, true);
                loopingCtrl = true;
            }

            // and play
            AL.SourcePlay(sourceID);
        }

        public void Stop()
        {
            // if looping is enabled
            if (loopingCtrl)
            {
                // disable it
                AL.Source(sourceID, ALSourceb.Looping, false);
                loopingCtrl = false;
            }

            // and stop
            AL.SourceStop(sourceID);
        }

        public static Sound CreateAndPlay(string url, float volume, bool looping)
        {
            var sound = new Sound(url, volume, looping);
            sound.Play();
            return sound;
        }
    }
}

