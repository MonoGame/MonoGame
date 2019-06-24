// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.OpenAL;

namespace Microsoft.Xna.Framework.Audio
{
	internal class OALSoundBuffer : IDisposable
	{
		int openALDataBuffer;
		ALFormat openALFormat;
		int dataSize;
        bool _isDisposed;

		public OALSoundBuffer()
		{
            AL.GenBuffers(1, out openALDataBuffer);
            ALHelper.CheckError("Failed to generate OpenAL data buffer.");
		}

        ~OALSoundBuffer()
        {
            Dispose(false);
        }

		public int OpenALDataBuffer
        {
			get
            {
				return openALDataBuffer;
			}
		}

		public double Duration
        {
			get;
			set;
		}

        public unsafe void BindDataBuffer(byte[] dataBuffer, ALFormat format, int size, int sampleRate, int sampleAlignment = 0, int loopStart = 0, int loopLength = 0)
        {
            if ((format == ALFormat.MonoMSAdpcm || format == ALFormat.StereoMSAdpcm) && !OpenALSoundController.Instance.SupportsAdpcm)
                throw new InvalidOperationException("MS-ADPCM is not supported by this OpenAL driver");
            if ((format == ALFormat.MonoIma4 || format == ALFormat.StereoIma4) && !OpenALSoundController.Instance.SupportsIma4)
                throw new InvalidOperationException("IMA/ADPCM is not supported by this OpenAL driver");

            openALFormat = format;
            dataSize = size;
            int unpackedSize = 0;

            if (sampleAlignment > 0)
            {
                AL.Bufferi(openALDataBuffer, ALBufferi.UnpackBlockAlignmentSoft, sampleAlignment);
                ALHelper.CheckError("Failed to fill buffer.");
            }

            bool formatLoopEnabled = true;

            /* Formats that OpenAL Soft currently doesn't support loop points for:
             * MonoIma4
             * StereoIma4
             * MonoMSAdpcm
             * StereoMSAdpcm
            */
            if (format == ALFormat.MonoIma4 || format == ALFormat.StereoIma4
                || format == ALFormat.MonoMSAdpcm || format == ALFormat.StereoMSAdpcm)
            {
                formatLoopEnabled = false;
            }

            if (OpenALSoundController.GetInstance.SupportsLoopPoints && formatLoopEnabled && loopStart >= 0 && loopLength > 0)
            {
                //Loop end is loopStart + loopLength
                int* loopData = stackalloc int[2];
                loopData[0] = loopStart;
                loopData[1] = loopStart + loopLength;

                AL.Bufferiv(openALDataBuffer, ALBufferi.LoopSoftPointsExt, loopData);
                ALHelper.CheckError("Failed to set loop points.");
            }

            AL.BufferData(openALDataBuffer, openALFormat, dataBuffer, size, sampleRate);
            ALHelper.CheckError("Failed to fill buffer.");

            int bits, channels;
            Duration = -1;
            AL.GetBuffer(openALDataBuffer, ALGetBufferi.Bits, out bits);
            ALHelper.CheckError("Failed to get buffer bits");
            AL.GetBuffer(openALDataBuffer, ALGetBufferi.Channels, out channels);
            ALHelper.CheckError("Failed to get buffer channels");
            AL.GetBuffer(openALDataBuffer, ALGetBufferi.Size, out unpackedSize);
            ALHelper.CheckError("Failed to get buffer size");
            Duration = (float)(unpackedSize / ((bits / 8) * channels)) / (float)sampleRate;
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
