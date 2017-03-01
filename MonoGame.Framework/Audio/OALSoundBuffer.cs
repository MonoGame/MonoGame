// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if MONOMAC && PLATFORM_MACOS_LEGACY
using MonoMac.OpenAL;
#endif
#if MONOMAC && !PLATFORM_MACOS_LEGACY
using OpenTK.Audio.OpenAL;
#endif

#if GLES 
using OpenTK.Audio.OpenAL;
#endif

#if DESKTOPGL
using OpenAL;
#endif


namespace Microsoft.Xna.Framework.Audio
{
	internal class OALSoundBuffer : IDisposable
	{
		int openALDataBuffer;
		ALFormat openALFormat;
		int dataSize;
		int sampleRate;
        bool _isDisposed;

		public OALSoundBuffer ()
		{
            try
            {
                AL.GenBuffers(1, out openALDataBuffer);
                ALHelper.CheckError("Failed to generate OpenAL data buffer.");
            }
            catch (DllNotFoundException e)
            {
                throw new NoAudioHardwareException("OpenAL drivers could not be found.", e);
            }
		}

        ~OALSoundBuffer()
        {
            Dispose(false);
        }

		public int OpenALDataBuffer {
			get {
				return openALDataBuffer;
			}
		}

		public double Duration {
			get;
			set;
		}

        public void BindDataBuffer(byte[] dataBuffer, ALFormat format, int size, int sampleRate, int alignment = 0)
        {
            openALFormat = format;
            dataSize = size;
            this.sampleRate = sampleRate;
            int unpackedSize = 0;
#if DESKTOPGL
            if (alignment > 0) {
                AL.Bufferi (openALDataBuffer, ALBufferi.UnpackBlockAlignmentSoft, alignment);
                ALHelper.CheckError ("Failed to fill buffer.");
            }
#endif

            AL.BufferData(openALDataBuffer, openALFormat, dataBuffer, size, this.sampleRate);
            ALHelper.CheckError("Failed to fill buffer.");

            int bits, channels;

            AL.GetBuffer(openALDataBuffer, ALGetBufferi.Bits, out bits);
            ALError alError = AL.GetError();
            if (alError != ALError.NoError)
            {
                Console.WriteLine("Failed to get buffer bits: {0}, format={1}, size={2}, sampleRate={3}", AL.GetErrorString(alError), format, size, sampleRate);
                Duration = -1;
            }
            else
            {
                AL.GetBuffer(openALDataBuffer, ALGetBufferi.Channels, out channels);

                alError = AL.GetError();
                if (alError != ALError.NoError)
                {
                    Console.WriteLine("Failed to get buffer channels: {0}, format={1}, size={2}, sampleRate={3}", AL.GetErrorString(alError), format, size, sampleRate);
                    Duration = -1;
                }
                else
                {
                    AL.GetBuffer (openALDataBuffer, ALGetBufferi.Size, out unpackedSize);
                    alError = AL.GetError ();
                    if (alError != ALError.NoError) {
                        Console.WriteLine ("Failed to get buffer size: {0}, format={1}, size={2}, sampleRate={3}", AL.GetErrorString (alError), format, size, sampleRate);
                        Duration = -1;
                    } else {
                        Duration = (float)(unpackedSize / ((bits / 8) * channels)) / (float)sampleRate;
                    }
                }
            }
            //Console.WriteLine("Duration: " + Duration + " / size: " + size + " bits: " + bits + " channels: " + channels + " rate: " + sampleRate);

        }

		public void Dispose()
		{
            Dispose(true);
            GC.SuppressFinalize(this);
		}

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Clean up managed objects
                }
                // Release unmanaged resources
                if (AL.IsBuffer(openALDataBuffer))
                {
                    ALHelper.CheckError("Failed to fetch buffer state.");
                    AL.DeleteBuffers(1, ref openALDataBuffer);
                    ALHelper.CheckError("Failed to delete buffer.");
                }

                _isDisposed = true;
            }
        }
	}
}

