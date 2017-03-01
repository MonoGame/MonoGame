// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Represents a collection of wave files.</summary>
    public class WaveBank : IDisposable
    {
        private readonly SoundEffect[] _sounds;
        private string _bankName;

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
        
        /// <summary>
        /// </summary>
        public bool IsInUse { get; private set; }

        /// <summary>
        /// </summary>
        public bool IsPrepared { get; private set; }

        /// <param name="audioEngine">Instance of the AudioEngine to associate this wave bank with.</param>
        /// <param name="nonStreamingWaveBankFilename">Path to the .xwb file to load.</param>
        /// <remarks>This constructor immediately loads all wave data into memory at once.</remarks>
        public WaveBank(AudioEngine audioEngine, string nonStreamingWaveBankFilename)
        {
            if (audioEngine == null)
                throw new ArgumentNullException("audioEngine");
            if (string.IsNullOrEmpty(nonStreamingWaveBankFilename))
                throw new ArgumentNullException("nonStreamingWaveBankFilename");

            //XWB PARSING
            //Adapted from MonoXNA
            //Originally adaped from Luigi Auriemma's unxwb
            
            WaveBankHeader wavebankheader;
            WaveBankData wavebankdata;
            WaveBankEntry wavebankentry;

            wavebankdata.EntryNameElementSize = 0;
            wavebankdata.CompactFormat = 0;
            wavebankdata.Alignment = 0;
            wavebankdata.BuildTime = 0;

            wavebankentry.Format = 0;
            wavebankentry.PlayRegion.Length = 0;
            wavebankentry.PlayRegion.Offset = 0;

            int wavebank_offset = 0;

            BinaryReader reader = new BinaryReader(AudioEngine.OpenStream(nonStreamingWaveBankFilename));

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
                wavebankdata.BankName = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(16),0,16).Replace("\0", "");
            }
            else
            {
                wavebankdata.BankName = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(64),0,64).Replace("\0", "");
            }

            _bankName = wavebankdata.BankName;

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

            if ((wavebankdata.Flags & Flag_Compact) != 0)
            {
                reader.ReadInt32(); // compact_format
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
            
            if ((wavebankheader.Segments[segidx_entry_name].Offset != 0) &&
                (wavebankheader.Segments[segidx_entry_name].Length != 0))
            {
                if (wavebankdata.EntryNameElementSize == -1) wavebankdata.EntryNameElementSize = 0;
                byte[] entry_name = new byte[wavebankdata.EntryNameElementSize + 1];
                entry_name[wavebankdata.EntryNameElementSize] = 0;
            }

            _sounds = new SoundEffect[wavebankdata.EntryCount];

            for (int current_entry = 0; current_entry < wavebankdata.EntryCount; current_entry++)
            {
                reader.BaseStream.Seek(wavebank_offset, SeekOrigin.Begin);
                //SHOWFILEOFF;

                //memset(&wavebankentry, 0, sizeof(wavebankentry));
                wavebankentry.LoopRegion.Length = 0;
                wavebankentry.LoopRegion.Offset = 0;

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

                MiniFormatTag codec;
                int chans;
                int rate;
                int align;
                //int bits;

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

                    codec = (MiniFormatTag)((wavebankentry.Format) & ((1 << 1) - 1));
                    chans = (wavebankentry.Format >> (1)) & ((1 << 3) - 1);
                    rate = (wavebankentry.Format >> (1 + 3 + 1)) & ((1 << 18) - 1);
                    align = (wavebankentry.Format >> (1 + 3 + 1 + 18)) & ((1 << 8) - 1);
                    //bits = (wavebankentry.Format >> (1 + 3 + 1 + 18 + 8)) & ((1 << 1) - 1);

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

                    codec = (MiniFormatTag)((wavebankentry.Format) & ((1 << 2) - 1));
                    chans = (wavebankentry.Format >> (2)) & ((1 << 3) - 1);
                    rate = (wavebankentry.Format >> (2 + 3)) & ((1 << 18) - 1);
                    align = (wavebankentry.Format >> (2 + 3 + 18)) & ((1 << 8) - 1);
                    //bits = (wavebankentry.Format >> (2 + 3 + 18 + 8)) & ((1 << 1) - 1);
                }
                
                reader.BaseStream.Seek(wavebankentry.PlayRegion.Offset, SeekOrigin.Begin);
                byte[] audiodata = reader.ReadBytes(wavebankentry.PlayRegion.Length);

                // Call the special constuctor on SoundEffect to sort it out.
                _sounds[current_entry] = new SoundEffect(codec, audiodata, chans, rate, align, wavebankentry.LoopRegion.Offset, wavebankentry.LoopRegion.Length);                
            }
            
            audioEngine.Wavebanks[_bankName] = this;

            IsPrepared = true;
        }
        
        /// <param name="audioEngine">Instance of the AudioEngine to associate this wave bank with.</param>
        /// <param name="streamingWaveBankFilename">Path to the .xwb to stream from.</param>
        /// <param name="offset">DVD sector-aligned offset within the wave bank data file.</param>
        /// <param name="packetsize">Stream packet size, in sectors, to use for each stream. The minimum value is 2.</param>
        /// <remarks>
        /// <para>This constructor streams wave data as needed.</para>
        /// <para>Note that packetsize is in sectors, which is 2048 bytes.</para>
        /// <para>AudioEngine.Update() must be called at least once before using data from a streaming wave bank.</para>
        /// </remarks>
        public WaveBank(AudioEngine audioEngine, string streamingWaveBankFilename, int offset, short packetsize)
            : this(audioEngine, streamingWaveBankFilename)
        {
            if (offset != 0)
                throw new NotImplementedException();
        }

        internal SoundEffect GetSoundEffect(int trackIndex)
        {
            return _sounds[trackIndex];
        }

        /// <summary>
        /// This event is triggered when the WaveBank is disposed.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        /// <summary>
        /// Is true if the WaveBank has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Disposes the WaveBank.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WaveBank()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            if (disposing)
            {
                foreach (var s in _sounds)
                    s.Dispose();

                IsPrepared = false;
                IsInUse = false;

                if (Disposing != null)
                    Disposing(this, EventArgs.Empty);
            }
        }
    }
}

