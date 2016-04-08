// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

using Microsoft.Xna.Framework.Audio;

#if WINRT
using SharpDX.XAudio2;
#endif

namespace Microsoft.Xna.Framework.Content
{
	internal class SoundEffectReader : ContentTypeReader<SoundEffect>
	{
		protected internal override SoundEffect Read(ContentReader input, SoundEffect existingInstance)
		{         
            // XNB format for SoundEffect...
            //            
            // Byte [format size]	Format	WAVEFORMATEX structure
            // UInt32	Data size	
            // Byte [data size]	Data	Audio waveform data
            // Int32	Loop start	In bytes (start must be format block aligned)
            // Int32	Loop length	In bytes (length must be format block aligned)
            // Int32	Duration	In milliseconds

            // WAVEFORMATEX structure...
            //
            //typedef struct {
            //  WORD  wFormatTag;       // byte[0]  +2
            //  WORD  nChannels;        // byte[2]  +2
            //  DWORD nSamplesPerSec;   // byte[4]  +4
            //  DWORD nAvgBytesPerSec;  // byte[8]  +4
            //  WORD  nBlockAlign;      // byte[12] +2
            //  WORD  wBitsPerSample;   // byte[14] +2
            //  WORD  cbSize;           // byte[16] +2
            //} WAVEFORMATEX;
            
			byte[] header = input.ReadBytes(input.ReadInt32());
			byte[] data = input.ReadBytes(input.ReadInt32());
			int loopStart = input.ReadInt32();
			int loopLength = input.ReadInt32();
			input.ReadInt32();

#if DIRECTX            
            var count = data.Length;
            var format = (int)BitConverter.ToUInt16(header, 0);
            var sampleRate = (int)BitConverter.ToUInt16(header, 4);
            var channels = BitConverter.ToUInt16(header, 2);
            //var avgBPS = (int)BitConverter.ToUInt16(header, 8);
            var blockAlignment = (int)BitConverter.ToUInt16(header, 12);
            //var bps = (int)BitConverter.ToUInt16(header, 14);

            SharpDX.Multimedia.WaveFormat waveFormat;
            if (format == 1)
                waveFormat = new SharpDX.Multimedia.WaveFormat(sampleRate, channels);
            else if (format == 2)
                waveFormat = new SharpDX.Multimedia.WaveFormatAdpcm(sampleRate, channels, blockAlignment);
            else
                throw new NotSupportedException("Unsupported wave format!");

            return new SoundEffect(data, 0, count, sampleRate, (AudioChannels)channels, loopStart, loopLength)
            {
                _format = waveFormat,
                Name = input.AssetName,
            };
#else
            if(loopStart == loopLength) 
            {
                // do nothing. just killing the warning for non-DirectX path 
            }
            if (header[0] == 2 && header[1] == 0)
            {
                // We've found MSADPCM data! Let's decode it here.
                using (MemoryStream origDataStream = new MemoryStream(data))
                {
                    using (BinaryReader reader = new BinaryReader(origDataStream))
                    {
                        byte[] newData = MSADPCMToPCM.MSADPCM_TO_PCM(
                            reader,
                            header[2],
                            (short) ((header[12] / header[2]) - 22)
                        );
                        data = newData;
                    }
                }
                
                // This is PCM data now!
                header[0] = 1;
            }
            
            int sampleRate = (
                (header[4]) +
                (header[5] << 8) +
                (header[6] << 16) +
                (header[7] << 24)
            );

            var channels = (header[2] == 2) ? AudioChannels.Stereo : AudioChannels.Mono;
            return new SoundEffect(data, sampleRate, channels)
                {
                    Name = input.AssetName
                };
#endif
		}
	}
}
