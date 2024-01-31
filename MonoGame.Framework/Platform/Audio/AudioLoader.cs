// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using MonoGame.OpenAL;

namespace Microsoft.Xna.Framework.Audio
{
    internal static class AudioLoader
    {
        internal const int FormatPcm = 1;
        internal const int FormatMsAdpcm = 2;
        internal const int FormatIeee = 3;
        internal const int FormatIma4 = 17;

        public static ALFormat GetSoundFormat(int format, int channels, int bits)
        {
            switch (format)
            {
                case FormatPcm:
                    // PCM
                    switch (channels)
                    {
                        case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                        case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                        default: throw new NotSupportedException("The specified channel count is not supported.");
                    }
                case FormatMsAdpcm:
                    // Microsoft ADPCM
                    switch (channels)
                    {
                        case 1: return ALFormat.MonoMSAdpcm;
                        case 2: return ALFormat.StereoMSAdpcm;
                        default: throw new NotSupportedException("The specified channel count is not supported.");
                    }
                case FormatIeee:
                    // IEEE Float
                    switch (channels)
                    {
                        case 1: return ALFormat.MonoFloat32;
                        case 2: return ALFormat.StereoFloat32;
                        default: throw new NotSupportedException("The specified channel count is not supported.");
                    }
                case FormatIma4:
                    // IMA4 ADPCM
                    switch (channels)
                    {
                        case 1: return ALFormat.MonoIma4;
                        case 2: return ALFormat.StereoIma4;
                        default: throw new NotSupportedException("The specified channel count is not supported.");
                    }
                default:
                    throw new NotSupportedException("The specified sound format (" + format.ToString() + ") is not supported.");
            }
        }

        // Converts block alignment in bytes to sample alignment, primarily for compressed formats
        // Calculation of sample alignment from http://kcat.strangesoft.net/openal-extensions/SOFT_block_alignment.txt
        public static int SampleAlignment(ALFormat format, int blockAlignment)
        {
            switch (format)
            {
                case ALFormat.MonoIma4:
                    return (blockAlignment - 4) / 4 * 8 + 1;
                case ALFormat.StereoIma4:
                    return (blockAlignment / 2 - 4) / 4 * 8 + 1;
                case ALFormat.MonoMSAdpcm:
                    return (blockAlignment - 7) * 2 + 2;
                case ALFormat.StereoMSAdpcm:
                    return (blockAlignment / 2 - 7) * 2 + 2;
            }
            return 0;
        }

        /// <summary>
        /// Load a WAV file from stream.
        /// </summary>
        /// <param name="stream">The stream positioned at the start of the WAV file.</param>
        /// <param name="format">Gets the OpenAL format enumeration value.</param>
        /// <param name="frequency">Gets the frequency or sample rate.</param>
        /// <param name="channels">Gets the number of channels.</param>
        /// <param name="blockAlignment">Gets the block alignment, important for compressed sounds.</param>
        /// <param name="bitsPerSample">Gets the number of bits per sample.</param>
        /// <param name="samplesPerBlock">Gets the number of samples per block.</param>
        /// <param name="sampleCount">Gets the total number of samples.</param>
        /// <returns>The byte buffer containing the waveform data or compressed blocks.</returns>
        public static byte[] Load(Stream stream, out ALFormat format, out int frequency, out int channels, out int blockAlignment, out int bitsPerSample, out int samplesPerBlock, out int sampleCount)
        {
            byte[] audioData = null;

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // for now we'll only support wave files
                audioData = LoadWave(reader, out format, out frequency, out channels, out blockAlignment, out bitsPerSample, out samplesPerBlock, out sampleCount);
            }

            return audioData;
        }

        private static byte[] LoadWave(BinaryReader reader, out ALFormat format, out int frequency, out int channels, out int blockAlignment, out int bitsPerSample, out int samplesPerBlock, out int sampleCount)
        {
            byte[] audioData = null;

            //header
            string signature = new string(reader.ReadChars(4));
            if (signature != "RIFF")
                throw new ArgumentException("Specified stream is not a wave file.");
            reader.ReadInt32(); // riff_chunk_size

            string wformat = new string(reader.ReadChars(4));
            if (wformat != "WAVE")
                throw new ArgumentException("Specified stream is not a wave file.");

            int audioFormat = 0;
            channels = 0;
            bitsPerSample = 0;
            format = ALFormat.Mono16;
            frequency = 0;
            blockAlignment = 0;
            samplesPerBlock = 0;
            sampleCount = 0;

            // WAVE header
            while (audioData == null)
            {
                string chunkType = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();
                switch (chunkType)
                {
                    case "fmt ":
                        {
                            audioFormat = reader.ReadInt16(); // 2
                            channels = reader.ReadInt16(); // 4
                            frequency = reader.ReadInt32();  // 8
                            int byteRate = reader.ReadInt32();    // 12
                            blockAlignment = (int)reader.ReadInt16();  // 14
                            bitsPerSample = reader.ReadInt16(); // 16

                            // Read extra data if present
                            if (chunkSize > 16)
                            {
                                int extraDataSize = reader.ReadInt16();
                                if (audioFormat == FormatIma4)
                                {
                                    samplesPerBlock = reader.ReadInt16();
                                    extraDataSize -= 2;
                                }
                                if (extraDataSize > 0)
                                {
                                    if (reader.BaseStream.CanSeek)
                                        reader.BaseStream.Seek(extraDataSize, SeekOrigin.Current);
                                    else
                                    {
                                        for (int i = 0; i < extraDataSize; ++i)
                                            reader.ReadByte();
                                    }
                                }
                            }
                        }
                        break;
                    case "fact":
                        if (audioFormat == FormatIma4)
                        {
                            sampleCount = reader.ReadInt32() * channels;
                            chunkSize -= 4;
                        }
                        // Skip any remaining chunk data
                        if (chunkSize > 0)
                        {
                            if (reader.BaseStream.CanSeek)
                                reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                            else
                            {
                                for (int i = 0; i < chunkSize; ++i)
                                    reader.ReadByte();
                            }
                        }
                        break;
                    case "data":
                        audioData = reader.ReadBytes(chunkSize);
                        break;
                    default:
                        // Skip this chunk
                        if (reader.BaseStream.CanSeek)
                            reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                        else
                        {
                            for (int i = 0; i < chunkSize; ++i)
                                reader.ReadByte();
                        }
                        break;
                }
            }

            // Calculate fields we didn't read from the file
            format = GetSoundFormat(audioFormat, channels, bitsPerSample);

            if (samplesPerBlock == 0)
            {
                samplesPerBlock = SampleAlignment(format, blockAlignment);
            }

            if (sampleCount == 0)
            {
                switch (audioFormat)
                {
                    case FormatIma4:
                    case FormatMsAdpcm:
                        sampleCount = ((audioData.Length / blockAlignment) * samplesPerBlock) + SampleAlignment(format, audioData.Length % blockAlignment);
                        break;
                    case FormatPcm:
                    case FormatIeee:
                        sampleCount = audioData.Length / ((channels * bitsPerSample) / 8);
                        break;
                    default:
                        throw new InvalidDataException("Unhandled WAV format " + format.ToString());
                }
            }

            return audioData;
        }

        // Convert buffer containing 24-bit signed PCM wav data to a 16-bit signed PCM buffer
        internal static unsafe byte[] Convert24To16(byte[] data, int offset, int count)
        {
            if ((offset + count > data.Length) || ((count % 3) != 0))
                throw new ArgumentException("Invalid 24-bit PCM data received");
            // Sample count includes both channels if stereo
            var sampleCount = count / 3;
            var outData = new byte[sampleCount * sizeof(short)];
            fixed (byte* src = &data[offset])
            {
                fixed (byte* dst = &outData[0])
                {
                    var srcIndex = 0;
                    var dstIndex = 0;
                    for (int i = 0; i < sampleCount; ++i)
                    {
                        // Drop the least significant byte from the 24-bit sample to get the 16-bit sample
                        dst[dstIndex] = src[srcIndex + 1];
                        dst[dstIndex + 1] = src[srcIndex + 2];
                        dstIndex += 2;
                        srcIndex += 3;
                    }
                }
            }
            return outData;
        }

        // Convert buffer containing IEEE 32-bit float wav data to a 16-bit signed PCM buffer
        internal static unsafe byte[] ConvertFloatTo16(byte[] data, int offset, int count)
        {
            if ((offset + count > data.Length) || ((count % 4) != 0))
                throw new ArgumentException("Invalid 32-bit float PCM data received");
            // Sample count includes both channels if stereo
            var sampleCount = count / 4;
            var outData = new byte[sampleCount * sizeof(short)];
            fixed (byte* src = &data[offset])
            {
                float* f = (float*)src;
                fixed (byte* dst = &outData[0])
                {
                    byte* d = dst;
                    for (int i = 0; i < sampleCount; ++i)
                    {
                        short s = (short)(*f * 32767.0f);
                        *d++ = (byte)(s & 0xff);
                        *d++ = (byte)(s >> 8);
                        ++f;
                    }
                }
            }
            return outData;
        }

        #region IMA4 decoding

        // Step table
        static int[] stepTable = new int[]
        {
            7, 8, 9, 10, 11, 12, 13, 14,
            16, 17, 19, 21, 23, 25, 28, 31,
            34, 37, 41, 45, 50, 55, 60, 66,
            73, 80, 88, 97, 107, 118, 130, 143,
            157, 173, 190, 209, 230, 253, 279, 307,
            337, 371, 408, 449, 494, 544, 598, 658,
            724, 796, 876, 963, 1060, 1166, 1282, 1411,
            1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024,
            3327, 3660, 4026, 4428, 4871, 5358, 5894, 6484,
            7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
            15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794,
            32767
        };

        // Step index tables
        static int[] indexTable = new int[]
        {
            // ADPCM data size is 4
            -1, -1, -1, -1, 2, 4, 6, 8,
            -1, -1, -1, -1, 2, 4, 6, 8
        };

        struct ImaState
        {
            public int predictor;
            public int stepIndex;
        }

        static int AdpcmImaWavExpandNibble(ref ImaState channel, int nibble)
        {
            int diff = stepTable[channel.stepIndex] >> 3;
            if ((nibble & 0x04) != 0)
                diff += stepTable[channel.stepIndex];
            if ((nibble & 0x02) != 0)
                diff += stepTable[channel.stepIndex] >> 1;
            if ((nibble & 0x01) != 0)
                diff += stepTable[channel.stepIndex] >> 2;
            if ((nibble & 0x08) != 0)
                channel.predictor -= diff;
            else
                channel.predictor += diff;

            if (channel.predictor < -32768)
                channel.predictor = -32768;
            else if (channel.predictor > 32767)
                channel.predictor = 32767;

            channel.stepIndex += indexTable[nibble];

            if (channel.stepIndex < 0)
                channel.stepIndex = 0;
            else if (channel.stepIndex > 88)
                channel.stepIndex = 88;

            return channel.predictor;
        }

        // Convert buffer containing IMA/ADPCM wav data to a 16-bit signed PCM buffer
        internal static byte[] ConvertIma4ToPcm(byte[] buffer, int offset, int count, int channels, int blockAlignment)
        {
            ImaState channel0 = new ImaState();
            ImaState channel1 = new ImaState();

            int sampleCountFullBlock = ((blockAlignment / channels) - 4) / 4 * 8 + 1;
            int sampleCountLastBlock = 0;
            if ((count % blockAlignment) > 0)
                sampleCountLastBlock = (((count % blockAlignment) / channels) - 4) / 4 * 8 + 1;
            int sampleCount = ((count / blockAlignment) * sampleCountFullBlock) + sampleCountLastBlock;
            var samples = new byte[sampleCount * sizeof(short) * channels];
            int sampleOffset = 0;

            while (count > 0)
            {
                int blockSize = blockAlignment;
                if (count < blockSize)
                    blockSize = count;
                count -= blockAlignment;

                channel0.predictor = buffer[offset++];
                channel0.predictor |= buffer[offset++] << 8;
                if ((channel0.predictor & 0x8000) != 0)
                    channel0.predictor -= 0x10000;
                channel0.stepIndex = buffer[offset++];
                if (channel0.stepIndex > 88)
                    channel0.stepIndex = 88;
                offset++;
                int index = sampleOffset * 2;
                samples[index] = (byte)channel0.predictor;
                samples[index + 1] = (byte)(channel0.predictor >> 8);
                ++sampleOffset;

                if (channels == 2)
                {
                    channel1.predictor = buffer[offset++];
                    channel1.predictor |= buffer[offset++] << 8;
                    if ((channel1.predictor & 0x8000) != 0)
                        channel1.predictor -= 0x10000;
                    channel1.stepIndex = buffer[offset++];
                    if (channel1.stepIndex > 88)
                        channel1.stepIndex = 88;
                    offset++;
                    index = sampleOffset * 2;
                    samples[index] = (byte)channel1.predictor;
                    samples[index + 1] = (byte)(channel1.predictor >> 8);
                    ++sampleOffset;
                }

                if (channels == 2)
                {
                    for (int nibbles = 2 * (blockSize - 8); nibbles > 0; nibbles -= 16)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            index = (sampleOffset + i * 4) * 2;
                            int sample = AdpcmImaWavExpandNibble(ref channel0, buffer[offset + i] & 0x0f);
                            samples[index] = (byte)sample;
                            samples[index + 1] = (byte)(sample >> 8);

                            index = (sampleOffset + i * 4 + 2) * 2;
                            sample = AdpcmImaWavExpandNibble(ref channel0, buffer[offset + i] >> 4);
                            samples[index] = (byte)sample;
                            samples[index + 1] = (byte)(sample >> 8);
                        }
                        offset += 4;

                        for (int i = 0; i < 4; i++)
                        {
                            index = (sampleOffset + i * 4 + 1) * 2;
                            int sample = AdpcmImaWavExpandNibble(ref channel1, buffer[offset + i] & 0x0f);
                            samples[index] = (byte)sample;
                            samples[index + 1] = (byte)(sample >> 8);

                            index = (sampleOffset + i * 4 + 3) * 2;
                            sample = AdpcmImaWavExpandNibble(ref channel1, buffer[offset + i] >> 4);
                            samples[index] = (byte)sample;
                            samples[index + 1] = (byte)(sample >> 8);
                        }
                        offset += 4;
                        sampleOffset += 16;
                    }
                }
                else
                {
                    for (int nibbles = 2 * (blockSize - 4); nibbles > 0; nibbles -= 2)
                    {
                        index = (sampleOffset * 2);
                        int b = buffer[offset];
                        int sample = AdpcmImaWavExpandNibble(ref channel0, b & 0x0f);
                        samples[index] = (byte)sample;
                        samples[index + 1] = (byte)(sample >> 8);
                        index += 2;
                        sample = AdpcmImaWavExpandNibble(ref channel0, b >> 4);
                        samples[index] = (byte)sample;
                        samples[index + 1] = (byte)(sample >> 8);

                        sampleOffset += 2;
                        ++offset;
                    }
                }
            }

            return samples;
        }

        #endregion

        #region MS-ADPCM decoding

        static int[] adaptationTable = new int[]
        {
            230, 230, 230, 230, 307, 409, 512, 614,
            768, 614, 512, 409, 307, 230, 230, 230
        };

        static int[] adaptationCoeff1 = new int[]
        {
            256, 512, 0, 192, 240, 460, 392
        };

        static int[] adaptationCoeff2 = new int[]
        {
            0, -256, 0, 64, 0, -208, -232
        };

        struct MsAdpcmState
        {
            public int delta;
            public int sample1;
            public int sample2;
            public int coeff1;
            public int coeff2;
        }

        static int AdpcmMsExpandNibble(ref MsAdpcmState channel, int nibble)
        {
            int nibbleSign = nibble - (((nibble & 0x08) != 0) ? 0x10 : 0);
            int predictor = ((channel.sample1 * channel.coeff1) + (channel.sample2 * channel.coeff2)) / 256 + (nibbleSign * channel.delta);

            if (predictor < -32768)
                predictor = -32768;
            else if (predictor > 32767)
                predictor = 32767;

            channel.sample2 = channel.sample1;
            channel.sample1 = predictor;

            channel.delta = (adaptationTable[nibble] * channel.delta) / 256;
            if (channel.delta < 16)
                channel.delta = 16;

            return predictor;
        }

        // Convert buffer containing MS-ADPCM wav data to a 16-bit signed PCM buffer
        internal static byte[] ConvertMsAdpcmToPcm(byte[] buffer, int offset, int count, int channels, int blockAlignment)
        {
            MsAdpcmState channel0 = new MsAdpcmState();
            MsAdpcmState channel1 = new MsAdpcmState();
            int blockPredictor;

            int sampleCountFullBlock = ((blockAlignment / channels) - 7) * 2 + 2;
            int sampleCountLastBlock = 0;
            if ((count % blockAlignment) > 0)
                sampleCountLastBlock = (((count % blockAlignment) / channels) - 7) * 2 + 2;
            int sampleCount = ((count / blockAlignment) * sampleCountFullBlock) + sampleCountLastBlock;
            var samples = new byte[sampleCount * sizeof(short) * channels];
            int sampleOffset = 0;

            bool stereo = channels == 2;

            while (count > 0)
            {
                int blockSize = blockAlignment;
                if (count < blockSize)
                    blockSize = count;
                count -= blockAlignment;

                int totalSamples = ((blockSize / channels) - 7) * 2 + 2;
                if (totalSamples < 2)
                    break;

                int offsetStart = offset;
                blockPredictor = buffer[offset];
                ++offset;
                if (blockPredictor > 6)
                    blockPredictor = 6;
                channel0.coeff1 = adaptationCoeff1[blockPredictor];
                channel0.coeff2 = adaptationCoeff2[blockPredictor];
                if (stereo)
                {
                    blockPredictor = buffer[offset];
                    ++offset;
                    if (blockPredictor > 6)
                        blockPredictor = 6;
                    channel1.coeff1 = adaptationCoeff1[blockPredictor];
                    channel1.coeff2 = adaptationCoeff2[blockPredictor];
                }

                channel0.delta = buffer[offset];
                channel0.delta |= buffer[offset + 1] << 8;
                if ((channel0.delta & 0x8000) != 0)
                    channel0.delta -= 0x10000;
                offset += 2;
                if (stereo)
                {
                    channel1.delta = buffer[offset];
                    channel1.delta |= buffer[offset + 1] << 8;
                    if ((channel1.delta & 0x8000) != 0)
                        channel1.delta -= 0x10000;
                    offset += 2;
                }

                channel0.sample1 = buffer[offset];
                channel0.sample1 |= buffer[offset + 1] << 8;
                if ((channel0.sample1 & 0x8000) != 0)
                    channel0.sample1 -= 0x10000;
                offset += 2;
                if (stereo)
                {
                    channel1.sample1 = buffer[offset];
                    channel1.sample1 |= buffer[offset + 1] << 8;
                    if ((channel1.sample1 & 0x8000) != 0)
                        channel1.sample1 -= 0x10000;
                    offset += 2;
                }

                channel0.sample2 = buffer[offset];
                channel0.sample2 |= buffer[offset + 1] << 8;
                if ((channel0.sample2 & 0x8000) != 0)
                    channel0.sample2 -= 0x10000;
                offset += 2;
                if (stereo)
                {
                    channel1.sample2 = buffer[offset];
                    channel1.sample2 |= buffer[offset + 1] << 8;
                    if ((channel1.sample2 & 0x8000) != 0)
                        channel1.sample2 -= 0x10000;
                    offset += 2;
                }

                if (stereo)
                {
                    samples[sampleOffset] = (byte)channel0.sample2;
                    samples[sampleOffset + 1] = (byte)(channel0.sample2 >> 8);
                    samples[sampleOffset + 2] = (byte)channel1.sample2;
                    samples[sampleOffset + 3] = (byte)(channel1.sample2 >> 8);
                    samples[sampleOffset + 4] = (byte)channel0.sample1;
                    samples[sampleOffset + 5] = (byte)(channel0.sample1 >> 8);
                    samples[sampleOffset + 6] = (byte)channel1.sample1;
                    samples[sampleOffset + 7] = (byte)(channel1.sample1 >> 8);
                    sampleOffset += 8;
                }
                else
                {
                    samples[sampleOffset] = (byte)channel0.sample2;
                    samples[sampleOffset + 1] = (byte)(channel0.sample2 >> 8);
                    samples[sampleOffset + 2] = (byte)channel0.sample1;
                    samples[sampleOffset + 3] = (byte)(channel0.sample1 >> 8);
                    sampleOffset += 4;
                }

                blockSize -= (offset - offsetStart);
                if (stereo)
                {
                    for (int i = 0; i < blockSize; ++i)
                    {
                        int nibbles = buffer[offset];

                        int sample = AdpcmMsExpandNibble(ref channel0, nibbles >> 4);
                        samples[sampleOffset] = (byte)sample;
                        samples[sampleOffset + 1] = (byte)(sample >> 8);

                        sample = AdpcmMsExpandNibble(ref channel1, nibbles & 0x0f);
                        samples[sampleOffset + 2] = (byte)sample;
                        samples[sampleOffset + 3] = (byte)(sample >> 8);

                        sampleOffset += 4;
                        ++offset;
                    }
                }
                else
                {
                    for (int i = 0; i < blockSize; ++i)
                    {
                        int nibbles = buffer[offset];

                        int sample = AdpcmMsExpandNibble(ref channel0, nibbles >> 4);
                        samples[sampleOffset] = (byte)sample;
                        samples[sampleOffset + 1] = (byte)(sample >> 8);

                        sample = AdpcmMsExpandNibble(ref channel0, nibbles & 0x0f);
                        samples[sampleOffset + 2] = (byte)sample;
                        samples[sampleOffset + 3] = (byte)(sample >> 8);

                        sampleOffset += 4;
                        ++offset;
                    }
                }
            }

            return samples;
        }

        #endregion
    }
}
