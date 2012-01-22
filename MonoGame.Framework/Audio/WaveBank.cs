#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.IO;
namespace Microsoft.Xna.Framework.Audio
{
    public class WaveBank : IDisposable
    {
        internal SoundEffectInstance[] sounds;
        internal string BankName;

        struct Segment
        {
            public int Offset;
            public int Length;
        }

        struct WaveBankEntry
        {
            public int Format;
            public Segment PlayRegion;
            public Segment LoopRegion;
            public int FlagsAndDuration;
        }

        struct WaveBankHeader
        {
            public int Version;
            public Segment[] Segments;
        }

        struct WaveBankData
        {
            public int    Flags;                                // Bank flags
            public int    EntryCount;                           // Number of entries in the bank
            public string BankName;                             // Bank friendly name
            public int    EntryMetaDataElementSize;             // Size of each entry meta-data element, in bytes
            public int    EntryNameElementSize;                 // Size of each entry name element, in bytes
            public int    Alignment;                            // Entry alignment, in bytes
            public int    CompactFormat;                        // Format data for compact bank
            public int    BuildTime;                            // Build timestamp
        }
        
        private const int Flag_EntryNames = 0x00010000; // Bank includes entry names
        private const int Flag_Compact = 0x00020000; // Bank uses compact format
        private const int Flag_SyncDisabled = 0x00040000; // Bank is disabled for audition sync
        private const int Flag_SeekTables = 0x00080000; // Bank includes seek tables.
        private const int Flag_Mask = 0x000F0000;
        
        private const int MiniFormatTag_PCM = 0x0;
        private const int MiniFormatTag_XMA = 0x1;
        private const int MiniFormatTag_ADPCM = 0x2;
        private const int MiniForamtTag_WMA = 0x3;
        
        public WaveBank(AudioEngine audioEngine, string nonStreamingWaveBankFilename)
        {
            //XWB PARSING
            //Adapted from MonoXNA
            //Originally adaped from Luigi Auriemma's unxwb
			
            WaveBankHeader wavebankheader;
            WaveBankData wavebankdata;
            WaveBankEntry wavebankentry;

            wavebankdata.EntryNameElementSize = 0;
            wavebankdata.CompactFormat = 0;
            wavebankdata.Alignment = 0;

            wavebankentry.Format = 0;
            wavebankentry.PlayRegion.Length = 0;
            wavebankentry.PlayRegion.Offset = 0;

            int wavebank_offset = 0;

            // Check for windows-style directory separator character
            nonStreamingWaveBankFilename = nonStreamingWaveBankFilename.Replace('\\',Path.DirectorySeparatorChar);

            BinaryReader reader = new BinaryReader(new FileStream(nonStreamingWaveBankFilename, FileMode.Open));
            reader.ReadBytes(4);

            wavebankheader.Version = reader.ReadInt32();

            int last_segment = 4;
            //if (wavebankheader.Version == 1) goto WAVEBANKDATA;
            if (wavebankheader.Version <= 3) last_segment = 3;
            if (wavebankheader.Version >= 42) reader.ReadInt32();    // skip HeaderVersion

            wavebankheader.Segments = new Segment[5];

            for (int i = 0; i <= last_segment; i++)
            {
                wavebankheader.Segments[i].Offset = reader.ReadInt32();
                wavebankheader.Segments[i].Length = reader.ReadInt32();
            }

            reader.BaseStream.Seek(wavebankheader.Segments[0].Offset, SeekOrigin.Begin);

            //WAVEBANKDATA:

            wavebankdata.Flags = reader.ReadInt32();
            wavebankdata.EntryCount = reader.ReadInt32();

            if ((wavebankheader.Version == 2) || (wavebankheader.Version == 3))
            {
                wavebankdata.BankName = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(16)).Replace("\0", "");
            }
            else
            {
                wavebankdata.BankName = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(64)).Replace("\0", "");
            }

            BankName = wavebankdata.BankName;

            if (wavebankheader.Version == 1)
            {
                //wavebank_offset = (int)ftell(fd) - file_offset;
                wavebankdata.EntryMetaDataElementSize = 20;
            }
            else
            {
                wavebankdata.EntryMetaDataElementSize = reader.ReadInt32();
                wavebankdata.EntryNameElementSize = reader.ReadInt32();
                wavebankdata.Alignment = reader.ReadInt32();
                wavebank_offset = wavebankheader.Segments[1].Offset; //METADATASEGMENT
            }

            int compact_format;

            if ((wavebankdata.Flags & Flag_Compact) != 0)
            {
                compact_format = reader.ReadInt32();
            }

            int playregion_offset = wavebankheader.Segments[last_segment].Offset;
            if (playregion_offset == 0)
            {
                playregion_offset =
                    wavebank_offset +
                    (wavebankdata.EntryCount * wavebankdata.EntryMetaDataElementSize);
            }
            
            int segidx_entry_name = 2;
            if (wavebankheader.Version >= 42) segidx_entry_name = 3;
            
            int waveentry_offset = wavebankheader.Segments[segidx_entry_name].Offset;
            if ((wavebankheader.Segments[segidx_entry_name].Offset != 0) &&
                (wavebankheader.Segments[segidx_entry_name].Length != 0))
            {
                if (wavebankdata.EntryNameElementSize == -1) wavebankdata.EntryNameElementSize = 0;
                byte[] entry_name = new byte[wavebankdata.EntryNameElementSize + 1];
                entry_name[wavebankdata.EntryNameElementSize] = 0;
            }

            sounds = new SoundEffectInstance[wavebankdata.EntryCount];

            for (int current_entry = 0; current_entry < wavebankdata.EntryCount; current_entry++)
            {
                reader.BaseStream.Seek(wavebank_offset, SeekOrigin.Begin);
                //SHOWFILEOFF;

                //memset(&wavebankentry, 0, sizeof(wavebankentry));


                if ((wavebankdata.Flags & Flag_Compact) != 0)
                {
                    int len = reader.ReadInt32();
                    wavebankentry.Format = wavebankdata.CompactFormat;
                    wavebankentry.PlayRegion.Offset = (len & ((1 << 21) - 1)) * wavebankdata.Alignment;
                    wavebankentry.PlayRegion.Length = (len >> 21) & ((1 << 11) - 1);

                    // workaround because I don't know how to handke the deviation length

                    reader.BaseStream.Seek(wavebank_offset + wavebankdata.EntryMetaDataElementSize, SeekOrigin.Begin);
                    //MYFSEEK(wavebank_offset + wavebankdata.dwEntryMetaDataElementSize); // seek to the next
                    if (current_entry == (wavebankdata.EntryCount - 1))
                    {              // the last track
                        len = wavebankheader.Segments[last_segment].Length;
                    }
                    else
                    {
                        len = ((reader.ReadInt32() & ((1 << 21) - 1)) * wavebankdata.Alignment);
                    }
                    wavebankentry.PlayRegion.Length =
                        len -                               // next offset
                        wavebankentry.PlayRegion.Offset;  // current offset
                    goto wavebank_handle;
                }

                if (wavebankheader.Version == 1)
                {
                    wavebankentry.Format = reader.ReadInt32();
                    wavebankentry.PlayRegion.Offset = reader.ReadInt32();
                    wavebankentry.PlayRegion.Length = reader.ReadInt32();
                    wavebankentry.LoopRegion.Offset = reader.ReadInt32();
                    wavebankentry.LoopRegion.Length = reader.ReadInt32();
                }
                else
                {
                    if (wavebankdata.EntryMetaDataElementSize >= 4) wavebankentry.FlagsAndDuration = reader.ReadInt32();
                    if (wavebankdata.EntryMetaDataElementSize >= 8) wavebankentry.Format = reader.ReadInt32();
                    if (wavebankdata.EntryMetaDataElementSize >= 12) wavebankentry.PlayRegion.Offset = reader.ReadInt32();
                    if (wavebankdata.EntryMetaDataElementSize >= 16) wavebankentry.PlayRegion.Length = reader.ReadInt32();
                    if (wavebankdata.EntryMetaDataElementSize >= 20) wavebankentry.LoopRegion.Offset = reader.ReadInt32();
                    if (wavebankdata.EntryMetaDataElementSize >= 24) wavebankentry.LoopRegion.Length = reader.ReadInt32();
                }

                if (wavebankdata.EntryMetaDataElementSize < 24)
                {                              // work-around
                    if (wavebankentry.PlayRegion.Length != 0)
                    {
                        wavebankentry.PlayRegion.Length = wavebankheader.Segments[last_segment].Length;
                    }

                }// else if(wavebankdata.EntryMetaDataElementSize > sizeof(WaveBankEntry)) {    // skip unused fields
            //   MYFSEEK(wavebank_offset + wavebankdata.EntryMetaDataElementSize);
            //}

                wavebank_handle:
                wavebank_offset += wavebankdata.EntryMetaDataElementSize;
                wavebankentry.PlayRegion.Offset += playregion_offset;
                
                // Parse WAVEBANKMINIWAVEFORMAT
                
                int codec;
                int chans;
                int rate;
                int align;
                int bits;

                if (wavebankheader.Version == 1)
                {         // I'm not 100% sure if the following is correct
                    // version 1:
                    // 1 00000000 000101011000100010 0 001 0
                    // | |         |                 | |   |
                    // | |         |                 | |   wFormatTag
                    // | |         |                 | nChannels
                    // | |         |                 ???
                    // | |         nSamplesPerSec
                    // | wBlockAlign
                    // wBitsPerSample

                    codec = (wavebankentry.Format) & ((1 << 1) - 1);
                    chans = (wavebankentry.Format >> (1)) & ((1 << 3) - 1);
                    rate = (wavebankentry.Format >> (1 + 3 + 1)) & ((1 << 18) - 1);
                    align = (wavebankentry.Format >> (1 + 3 + 1 + 18)) & ((1 << 8) - 1);
                    bits = (wavebankentry.Format >> (1 + 3 + 1 + 18 + 8)) & ((1 << 1) - 1);

                    /*} else if(wavebankheader.dwVersion == 23) { // I'm not 100% sure if the following is correct
                        // version 23:
                        // 1000000000 001011101110000000 001 1
                        // | |        |                  |   |
                        // | |        |                  |   ???
                        // | |        |                  nChannels?
                        // | |        nSamplesPerSec
                        // | ???
                        // !!!UNKNOWN FORMAT!!!

                        //codec = -1;
                        //chans = (wavebankentry.Format >>  1) & ((1 <<  3) - 1);
                        //rate  = (wavebankentry.Format >>  4) & ((1 << 18) - 1);
                        //bits  = (wavebankentry.Format >> 31) & ((1 <<  1) - 1);
                        codec = (wavebankentry.Format                    ) & ((1 <<  1) - 1);
                        chans = (wavebankentry.Format >> (1)             ) & ((1 <<  3) - 1);
                        rate  = (wavebankentry.Format >> (1 + 3)         ) & ((1 << 18) - 1);
                        align = (wavebankentry.Format >> (1 + 3 + 18)    ) & ((1 <<  9) - 1);
                        bits  = (wavebankentry.Format >> (1 + 3 + 18 + 9)) & ((1 <<  1) - 1); */

                }
                else
                {
                    // 0 00000000 000111110100000000 010 01
                    // | |        |                  |   |
                    // | |        |                  |   wFormatTag
                    // | |        |                  nChannels
                    // | |        nSamplesPerSec
                    // | wBlockAlign
                    // wBitsPerSample

                    codec = (wavebankentry.Format) & ((1 << 2) - 1);
                    chans = (wavebankentry.Format >> (2)) & ((1 << 3) - 1);
                    rate = (wavebankentry.Format >> (2 + 3)) & ((1 << 18) - 1);
                    align = (wavebankentry.Format >> (2 + 3 + 18)) & ((1 << 8) - 1);
                    bits = (wavebankentry.Format >> (2 + 3 + 18 + 8)) & ((1 << 1) - 1);
                }
                
                reader.BaseStream.Seek(wavebankentry.PlayRegion.Offset, SeekOrigin.Begin);
                byte[] audiodata = reader.ReadBytes(wavebankentry.PlayRegion.Length);
                
                if (codec == MiniFormatTag_PCM) {
                    
                    //write PCM data into a wav
                    
                    MemoryStream mStream = new MemoryStream(44+audiodata.Length);
                    BinaryWriter writer = new BinaryWriter(mStream);

                    writer.Write("RIFF".ToCharArray());
                    writer.Write((int)(36+audiodata.Length));
                    writer.Write("WAVE".ToCharArray());
                    
                    writer.Write("fmt ".ToCharArray());
                    writer.Write((int)16); //header size
                    writer.Write((short)1); //format (PCM)
                    writer.Write((short)chans);
                    writer.Write((int)rate); //sample rate
                    writer.Write((int)rate*align); //byte rate
                    writer.Write((short)align);

					if (bits == 1) 
					{
                        writer.Write((short)16);
                    } 
					else 
					{
                        writer.Write((short)8); //not sure if this is right
                    } 
                    
                    writer.Write("data".ToCharArray());
                    writer.Write((int)audiodata.Length);
                    writer.Write(audiodata);
                                        
                    writer.Close();
                    mStream.Close();
					
                    sounds[current_entry] = new SoundEffect(mStream.ToArray(), rate, AudioChannels.Mono).CreateInstance();
					
                } else if (codec == MiniForamtTag_WMA) { //WMA or xWMA (or XMA2)
                    byte[] wmaSig = {0x30, 0x26, 0xb2, 0x75, 0x8e, 0x66, 0xcf, 0x11, 0xa6, 0xd9, 0x0, 0xaa, 0x0, 0x62, 0xce, 0x6c};
                    
                    bool isWma = true;
                    for (int i=0; i<wmaSig.Length; i++) {
                        if (wmaSig[i] != audiodata[i]) {
                            isWma = false;
                            break;
                        }
                    }
                    
                    //Let's support m4a data as well for convenience
                    byte[][] m4aSigs = new byte[][] {
                        new byte[] {0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x4D, 0x34, 0x41, 0x20, 0x00, 0x00, 0x02, 0x00},
                        new byte[] {0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x4D, 0x34, 0x41, 0x20, 0x00, 0x00, 0x00, 0x00}
                    };
                    
                    bool isM4a = false;
                    for (int i=0; i<m4aSigs.Length; i++) {    
                        byte[] sig = m4aSigs[i];
                        bool matches = true;
                        for (int j=0; j<sig.Length; j++) {
                            if (sig[j] != audiodata[j]) {
                                matches = false;
                                break;
                            }
                        }
                        if (matches) {
                            isM4a = true;
                            break;
                        }
                    }
                    
                    if (isWma || isM4a) {
                        //WMA data can sometimes be played directly
                        
                        //hack - NSSound can't play non-wav from data, we have to give a filename
                        string filename = Path.GetTempFileName();
                        if (isWma) {
                            filename = filename.Replace(".tmp", ".wma");
                        } else if (isM4a) {
                            filename = filename.Replace(".tmp", ".m4a");
                        }
                        FileStream audioFile = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
                        audioFile.Write(audiodata, 0, audiodata.Length);
                        audioFile.Close();
                        
                        sounds[current_entry] = new SoundEffect(filename).CreateInstance();
                    } else {
                        //An xWMA or XMA2 file. Can't be played atm :(
                        throw new NotImplementedException();
                    }
                } else {
                    throw new NotImplementedException();
                }
                
            }
			
			audioEngine.Wavebanks[BankName] = this;
        }
		
		public WaveBank(AudioEngine audioEngine, string streamingWaveBankFilename, int offset, short packetsize)
			: this(audioEngine, streamingWaveBankFilename)
		{
			if (offset != 0) {
				throw new NotImplementedException();
			}
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
		#endregion
    }
}

