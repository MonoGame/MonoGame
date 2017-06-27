using System;
using System.IO;
#if GLES
using OpenTK.Audio.OpenAL;
#else
using OpenAL;
#endif

namespace Microsoft.Xna.Framework.Audio
{
    internal static class AudioLoader
    {
        const int FormatPcm = 1;
        const int FormatMsAdpcm = 2;
        const int FormatIeee = 3;
        const int FormatIma4 = 17;

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
#if GLES
                        case 1: return (ALFormat)0x1302;
                        case 2: return (ALFormat)0x1303;
#else
                        case 1: return ALFormat.MonoMicrosoftAdpcm;
                        case 2: return ALFormat.StereoMicrosoftAdpcm;
#endif
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
#if GLES
                        case 1: return (ALFormat)0x1300;
                        case 2: return (ALFormat)0x1301;
#else
                        case 1: return ALFormat.MonoIma4;
                        case 2: return ALFormat.StereoIma4;
#endif
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
                case ALFormat.MonoMicrosoftAdpcm:
                    return (blockAlignment - 7) * 2 + 2;
                case ALFormat.StereoMicrosoftAdpcm:
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
        /// <param name="blockAlignment">Gets the block alignment, important for compressed sounds.</param>
        /// <param name="bitsPerSample">Gets the number of bits per sample.</param>
        /// <param name="samplesPerBlock">Gets the number of samples per block.</param>
        /// <param name="sampleCount">Gets the total number of samples.</param>
        /// <returns>The byte buffer containing the waveform data or compressed blocks.</returns>
        public static byte[] Load(Stream stream, out ALFormat format, out int frequency, out int blockAlignment, out int bitsPerSample, out int samplesPerBlock, out int sampleCount)
        {
            byte[] audioData = null;

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // for now we'll only support wave files
                audioData = LoadWave(reader, out format, out frequency, out blockAlignment, out bitsPerSample, out samplesPerBlock, out sampleCount);
            }

            return audioData;
        }

        private static byte[] LoadWave(BinaryReader reader, out ALFormat format, out int frequency, out int blockAlignment, out int bitsPerSample, out int samplesPerBlock, out int sampleCount)
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
            int numChannels = 0;
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
                            numChannels = reader.ReadInt16(); // 4
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
                                    reader.BaseStream.Seek(extraDataSize, SeekOrigin.Current);
                            }
                        }
                        break;
                    case "fact":
                        {
                            if (audioFormat == FormatIma4)
                            {
                                sampleCount = reader.ReadInt32() * numChannels;
                                chunkSize -= 4;
                            }
                            // Skip any remaining chunk data
                            if (chunkSize > 0)
                                reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                        }
                        break;
                    case "data":
                        {
                            audioData = reader.ReadBytes(chunkSize);

                            if (audioFormat == 1 && bitsPerSample == 24)
                            {
                                // 24-bit PCM is reduced to 16-bit PCM
                                audioData = Convert24to16(audioData);
                                format = numChannels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
                                bitsPerSample = 16;
                                blockAlignment = 2 * numChannels;
                                samplesPerBlock = numChannels;
                            }
                        }
                        break;
                    default:
                        // Skip this chunk
                        reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                        break;
                }
            }

            // Calculate fields we didn't read from the file
            format = GetSoundFormat(audioFormat, numChannels, bitsPerSample);

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
                        sampleCount = ((int)audioData.Length / blockAlignment) * samplesPerBlock;
                        break;
                    case FormatPcm:
                    case FormatIeee:
                        sampleCount = (int)(audioData.Length / ((numChannels * bitsPerSample) / 8));
                        break;
                    default:
                        throw new InvalidDataException("Unhandled WAV format " + format.ToString());
                }
            }

            return audioData;
        }

        private static unsafe byte[] Convert24to16(byte[] data)
        {
            if (data.Length % 3 != 0)
                throw new ArgumentException("Invalid 24-bit PCM data received");
            var sampleCount = data.Length / 3;
            var outSize = sampleCount * 2;
            var outData = new byte[outSize];
            fixed (byte* src = &data[0])
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
    }
}
