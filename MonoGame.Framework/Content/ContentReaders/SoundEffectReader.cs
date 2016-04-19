// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Audio;


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
            
            var header = input.ReadBytes(input.ReadInt32());
            var data = input.ReadBytes(input.ReadInt32());
            var loopStart = input.ReadInt32();
            var loopLength = input.ReadInt32();
            var durationMs = input.ReadInt32();

            var format = (int)BitConverter.ToUInt16(header, 0);
            var channels = BitConverter.ToUInt16(header, 2);
            var sampleRate = (int)BitConverter.ToUInt16(header, 4);
            //var avgBPS = (int)BitConverter.ToUInt16(header, 8);
            var blockAlignment = (int)BitConverter.ToUInt16(header, 12);
            //var bps = (int)BitConverter.ToUInt16(header, 14);

            // Initialize the effect.
            var effect = new SoundEffect(data, format, sampleRate, channels, blockAlignment, durationMs, loopStart, loopLength);
            effect.Name = input.AssetName;

            return effect;
        }
	}
}
