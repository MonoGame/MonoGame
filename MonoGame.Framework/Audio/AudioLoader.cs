using System;
using System.IO;
#if GLES
using OpenTK.Audio.OpenAL;
#else
using OpenAL;
#endif

namespace Microsoft.Xna.Framework.Audio
{
    internal class AudioLoader
    {
        private AudioLoader()
        {
        }

        public static ALFormat GetSoundFormat(int format, int channels, int bits)
        {
            switch (format)
            {
                case 1:
                    // PCM
                    switch (channels)
                    {
                        case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                        case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                        default: throw new NotSupportedException("The specified channel count is not supported.");
                    }
                case 2:
                    // Microsoft ADPCM
                    switch (channels)
                    {
#if GLES
                        case 1: return (ALFormat)0x1302;
                        case 2: return (ALFormat)0x1303;
#else
                        case 1: return ALFormat.MonoMSAdpcm;
                        case 2: return ALFormat.StereoMSAdpcm;
#endif
                        default: throw new NotSupportedException("The specified channel count is not supported.");
                    }
                case 3:
                    // IEEE Float
                    switch (channels)
                    {
                        case 1: return ALFormat.MonoFloat32;
                        case 2: return ALFormat.StereoFloat32;
                        default: throw new NotSupportedException("The specified channel count is not supported.");
                    }
                case 17:
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

        /// <summary>
        /// Load a WAV file from stream.
        /// </summary>
        /// <param name="stream">The stream positioned at the start of the WAV file.</param>
        /// <param name="format">Gets the OpenAL format enumeration value.</param>
        /// <param name="frequency">Gets the frequency or sample rate.</param>
        /// <param name="blockAlignment">Gets the block alignment, important for compressed sounds.</param>
        /// <param name="byteRate">Gets the number of bytes per second when playing the sound at normal speed.</param>
        /// <returns>The byte buffer containing the waveform data or compressed blocks.</returns>
        public static byte[] Load(Stream stream, out ALFormat format, out int frequency, out int blockAlignment, out int byteRate)
        {
            byte[] audioData = null;

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // for now we'll only support wave files
                audioData = LoadWave(reader, out format, out frequency, out blockAlignment, out byteRate);
            }

            return audioData;
        }

        private static byte[] LoadWave(BinaryReader reader, out ALFormat format,out int frequency, out int blockAlignment, out int byteRate)
        {
            // code based on opentk example

            byte[] audioData;

            //header
            string signature = new string(reader.ReadChars(4));
            if (signature != "RIFF")
            {
                throw new ArgumentException("Specified stream is not a wave file.");
            }

			reader.ReadInt32(); // riff_chunk_size

            string wformat = new string(reader.ReadChars(4));
            if (wformat != "WAVE")
            {
                throw new ArgumentException("Specified stream is not a wave file.");
            }

            // WAVE header
            string format_signature = new string(reader.ReadChars(4));
            while (format_signature != "fmt ") {
                reader.ReadBytes(reader.ReadInt32());
                format_signature = new string(reader.ReadChars(4));
            }

            int format_chunk_size = reader.ReadInt32();

            // total bytes read: tbp
            int audio_format = reader.ReadInt16(); // 2
            int num_channels = reader.ReadInt16(); // 4
            frequency = reader.ReadInt32();  // 8
			byteRate = reader.ReadInt32();    // 12, byte_rate
			blockAlignment = (int)reader.ReadInt16();  // 14, block_align
            int bits_per_sample = reader.ReadInt16(); // 16
            if (byteRate == 0)
                byteRate = (frequency * bits_per_sample / 8) * num_channels;

            // reads residual bytes
            if (format_chunk_size > 16)
                reader.ReadBytes(format_chunk_size - 16);
            
            string data_signature = new string(reader.ReadChars(4));

            while (data_signature.ToLowerInvariant() != "data")
            {
                reader.ReadBytes(reader.ReadInt32());
                data_signature = new string(reader.ReadChars(4));
            }

            if (data_signature != "data")
            {
                throw new ArgumentException("Specified wave file is not supported.");
            }

            int data_chunk_size = reader.ReadInt32();

            format = GetSoundFormat(audio_format, num_channels, bits_per_sample);
            audioData = reader.ReadBytes(data_chunk_size);

            if (audio_format == 1 && bits_per_sample == 24)
            {
                // 24-bit PCM is reduced to 16-bit PCM
                audioData = Convert24to16(audioData);
                format = num_channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
                byteRate = frequency * 2 * num_channels;
                blockAlignment = 2 * num_channels;
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
