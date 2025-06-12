// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Microsoft.Xna.Framework.Content.Pipeline.Audio
{
    /// <summary>
    /// Encapsulates the native audio format (WAVEFORMATEX) information of the audio content.
    /// </summary>
    public sealed class AudioFormat
    {
        int averageBytesPerSecond;
        int bitsPerSample;
        int blockAlign;
        int channelCount;
        int format;
        List<byte> nativeWaveFormat;
        int sampleRate;

        /// <summary>
        /// Gets the average bytes processed per second.
        /// </summary>
        /// <value>Average bytes processed per second.</value>
        public int AverageBytesPerSecond { get { return averageBytesPerSecond; } }

        /// <summary>
        /// Gets the bit depth of the audio content.
        /// </summary>
        /// <value>If the audio has not been processed, the source bit depth; otherwise, the bit depth of the new format.</value>
        public int BitsPerSample { get { return bitsPerSample; } }

        /// <summary>
        /// Gets the number of bytes per sample block, taking channels into consideration. For example, for 16-bit stereo audio (PCM format), the size of each sample block is 4 bytes.
        /// </summary>
        /// <value>Number of bytes, per sample block.</value>
        public int BlockAlign { get { return blockAlign; } }

        /// <summary>
        /// Gets the number of channels.
        /// </summary>
        /// <value>If the audio has not been processed, the source channel count; otherwise, the new channel count.</value>
        public int ChannelCount { get { return channelCount; } }

        /// <summary>
        /// Gets the format of the audio content.
        /// </summary>
        /// <value>If the audio has not been processed, the format tag of the source content; otherwise, the new format tag.</value>
        public int Format { get { return format; } }

        /// <summary>
        /// Gets the raw byte buffer for the format. For non-PCM formats, this buffer contains important format-specific information beyond the basic format information exposed in other properties of the AudioFormat type.
        /// </summary>
        /// <value>The raw byte buffer represented in a collection.</value>
        public ReadOnlyCollection<byte> NativeWaveFormat { get { return nativeWaveFormat.AsReadOnly(); } }

        /// <summary>
        /// Gets the sample rate of the audio content.
        /// </summary>
        /// <value>If the audio has not been processed, the source sample rate; otherwise, the new sample rate.</value>
        public int SampleRate { get { return sampleRate; } }

        internal AudioFormat(
            int averageBytesPerSecond,
            int bitsPerSample,
            int blockAlign,
            int channelCount,
            int format,
            int sampleRate)
        {
            this.averageBytesPerSecond = averageBytesPerSecond;
            this.bitsPerSample = bitsPerSample;
            this.blockAlign = blockAlign;
            this.channelCount = channelCount;
            this.format = format;
            this.sampleRate = sampleRate;

            this.nativeWaveFormat = this.ConstructNativeWaveFormat();
        }

        private List<byte> ConstructNativeWaveFormat()
        {
            using (var memory = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memory))
                {
                    writer.Write((short)this.format);
                    writer.Write((short)this.channelCount);
                    writer.Write((int)this.sampleRate);
                    writer.Write((int)this.averageBytesPerSecond);
                    writer.Write((short)this.blockAlign);
                    writer.Write((short)this.bitsPerSample);
                    writer.Write((short)0);

                    var bytes = new byte[memory.Position];
                    memory.Seek(0, SeekOrigin.Begin);
                    memory.Read(bytes, 0, bytes.Length);
                    return bytes.ToList();
                }
            }
        }
    }
}
