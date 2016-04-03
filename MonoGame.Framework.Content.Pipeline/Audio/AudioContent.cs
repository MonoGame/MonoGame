// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Microsoft.Xna.Framework.Content.Pipeline.Audio
{
    /// <summary>
    /// Encapsulates and provides operations, such as format conversions, on the source audio. This type is produced by the audio importers and used by audio processors to produce compiled audio assets.
    /// </summary>
    public class AudioContent : ContentItem
    {
        internal List<byte> data;
        TimeSpan duration;
        string fileName;
        AudioFileType fileType;
        AudioFormat format;
        int loopLength;
        int loopStart;

        /// <summary>
        /// Gets the raw audio data.
        /// </summary>
        /// <value>If unprocessed, the source data; otherwise, the processed data.</value>
        public ReadOnlyCollection<byte> Data { get { return data.AsReadOnly(); } }

        /// <summary>
        /// Gets the duration (in milliseconds) of the audio data.
        /// </summary>
        /// <value>Duration of the audio data.</value>
        public TimeSpan Duration { get { return duration; } }

        /// <summary>
        /// Gets the file name containing the audio data.
        /// </summary>
        /// <value>The name of the file containing this data.</value>
        [ContentSerializerAttribute]
        public string FileName { get { return fileName; } }

        /// <summary>
        /// Gets the AudioFileType of this audio source.
        /// </summary>
        /// <value>The AudioFileType of this audio source.</value>
        public AudioFileType FileType { get { return fileType; } }

        /// <summary>
        /// Gets the AudioFormat of this audio source.
        /// </summary>
        /// <value>The AudioFormat of this audio source.</value>
        public AudioFormat Format { get { return format; } }

        /// <summary>
        /// Gets the loop length, in samples.
        /// </summary>
        /// <value>The number of samples in the loop.</value>
        public int LoopLength { get { return loopLength; } }

        /// <summary>
        /// Gets the loop start, in samples.
        /// </summary>
        /// <value>The number of samples to the start of the loop.</value>
        public int LoopStart { get { return loopStart; } }

        /// <summary>
        /// Initializes a new instance of AudioContent.
        /// </summary>
        /// <param name="audioFileName">Name of the audio source file to be processed.</param>
        /// <param name="audioFileType">Type of the processed audio: WAV, MP3 or WMA.</param>
        /// <remarks>Constructs the object from the specified source file, in the format specified.</remarks>
        public AudioContent(string audioFileName, AudioFileType audioFileType)
        {
            fileName = audioFileName;
            fileType = audioFileType;
            Read(audioFileName);
        }

        /// <summary>
        /// Returns the sample rate for the given quality setting.
        /// </summary>
        /// <param name="quality">The quality setting.</param>
        /// <returns>The sample rate for the quality.</returns>
        int QualityToSampleRate(ConversionQuality quality)
        {
            switch (quality)
            {
                case ConversionQuality.Low:
                    return Math.Max(8000, format.SampleRate / 2);
            }

            return Math.Max(8000, format.SampleRate);
        }

        /// <summary>
        /// Returns the bitrate for the given quality setting.
        /// </summary>
        /// <param name="quality">The quality setting.</param>
        /// <returns>The bitrate for the quality.</returns>
        int QualityToBitRate(ConversionQuality quality)
        {
            switch (quality)
            {
                case ConversionQuality.Low:
                    return 96000;
                case ConversionQuality.Medium:
                    return 128000;
            }

            return 192000;
        }

        /// <summary>
        /// Transcodes the source audio to the target format and quality.
        /// </summary>
        /// <param name="formatType">Format to convert this audio to.</param>
        /// <param name="quality">Quality of the processed output audio. For streaming formats, it can be one of the following: Low (96 kbps), Medium (128 kbps), Best (192 kbps).  For WAV formats, it can be one of the following: Low (11kHz ADPCM), Medium (22kHz ADPCM), Best (44kHz PCM)</param>
        /// <param name="saveToFile">
        /// The name of the file that the converted audio should be saved into.  This is used for SongContent, where
        /// the audio is stored external to the XNB file.  If this is null, then the converted audio is stored in
        /// the Data property.
        /// </param>
        public void ConvertFormat(ConversionFormat formatType, ConversionQuality quality, string saveToFile)
        {
            var temporarySource = Path.GetTempFileName();
            var temporaryOutput = Path.GetTempFileName();
            try
            {
                using (var fs = new FileStream(temporarySource, FileMode.Create, FileAccess.Write))
                {
                    var dataBytes = this.data.ToArray();
                    fs.Write(dataBytes, 0, dataBytes.Length);
                }

                string ffmpegCodecName, ffmpegMuxerName;
                int format;
                switch (formatType)
                {
                    case ConversionFormat.Adpcm:
                        // ADPCM Microsoft 
                        ffmpegCodecName = "adpcm_ms";
                        ffmpegMuxerName = "wav";
                        format = 0x0002; /* WAVE_FORMAT_ADPCM */
                        break;
                    case ConversionFormat.Pcm:
                        // PCM signed 16-bit little-endian
                        ffmpegCodecName = "pcm_s16le";
                        ffmpegMuxerName = "wav";
                        format = 0x0001; /* WAVE_FORMAT_PCM */
                        break;
                    case ConversionFormat.WindowsMedia:
                        // Windows Media Audio 2
                        ffmpegCodecName = "wmav2";
                        ffmpegMuxerName = "asf";
                        format = 0x0161; /* WAVE_FORMAT_WMAUDIO2 */
                        break;
                    case ConversionFormat.Xma:
                        throw new NotSupportedException(
                            "XMA is not a supported encoding format. It is specific to the Xbox 360.");
                    case ConversionFormat.ImaAdpcm:
                        // ADPCM IMA WAV
                        ffmpegCodecName = "adpcm_ima_wav";
                        ffmpegMuxerName = "wav";
                        format = 0x0011; /* WAVE_FORMAT_IMA_ADPCM */
                        break;
                    case ConversionFormat.Aac:
                        // AAC (Advanced Audio Coding)
                        // Requires -strict experimental
                        ffmpegCodecName = "aac";
                        ffmpegMuxerName = "ipod";
                        format = 0x0000; /* WAVE_FORMAT_UNKNOWN */
                        break;
                    case ConversionFormat.Vorbis:
                        // Vorbis
                        ffmpegCodecName = "libvorbis";
                        ffmpegMuxerName = "ogg";
                        format = 0x0000; /* WAVE_FORMAT_UNKNOWN */
                        break;
                    default:
                        // Unknown format
                        throw new NotSupportedException();
                }

                string ffmpegStdout, ffmpegStderr;
                var ffmpegExitCode = ExternalTool.Run(
                    "ffmpeg",
                    string.Format(
                        "-y -i \"{0}\" -vn -c:a {1} -b:a {2} -f:a {3} -strict experimental \"{4}\"",
                        temporarySource,
                        ffmpegCodecName,
                        QualityToBitRate(quality),
                        ffmpegMuxerName,
                        temporaryOutput),
                    out ffmpegStdout,
                    out ffmpegStderr);
                if (ffmpegExitCode != 0)
                {
                    throw new InvalidOperationException("ffmpeg exited with non-zero exit code: \n" + ffmpegStdout + "\n" + ffmpegStderr);
                }

                byte[] rawData;
                using (var fs = new FileStream(temporaryOutput, FileMode.Open, FileAccess.Read))
                {
                    rawData = new byte[fs.Length];
                    fs.Read(rawData, 0, rawData.Length);
                }

                if (saveToFile != null)
                {
                    using (var fs = new FileStream(saveToFile, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(rawData, 0, rawData.Length);
                    }

                    this.data = null;
                }
                else
                {
                    this.data = rawData.ToList();
                }

                // Get the audio metadata from the output file
                string ffprobeStdout, ffprobeStderr;
                var ffprobeExitCode = ExternalTool.Run(
                    "ffprobe",
                    string.Format("-i \"{0}\" -show_entries streams -v quiet -of flat", temporaryOutput),
                    out ffprobeStdout,
                    out ffprobeStderr);
                if (ffprobeExitCode != 0)
                {
                    throw new InvalidOperationException("ffprobe exited with non-zero exit code.");
                }

                // Set default values if information is not available.
                int averageBytesPerSecond = 0;
                int bitsPerSample = 0;
                int blockAlign = 0;
                int channelCount = 0;
                int sampleRate = 0;
                double durationInSeconds = 0;

                var numberFormat = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
                foreach (var line in ffprobeStdout.Split(new[] { '\r', '\n', '\0' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = line.Split(new[] { '=' }, 2);

                    switch (kv[0])
                    {
                        case "streams.stream.0.sample_rate":
                            sampleRate = int.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.bits_per_sample":
                            bitsPerSample = int.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.duration":
                            durationInSeconds = double.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.channels":
                            channelCount = int.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.bit_rate":
                            averageBytesPerSecond = (int.Parse(kv[1].Trim('"'), numberFormat) / 8);
                            break;
                    }
                }

                // Calculate blockAlign.
                switch (formatType)
                {
                    case ConversionFormat.Adpcm:
                    case ConversionFormat.ImaAdpcm:
                    case ConversionFormat.Pcm:
                        // Block alignment value is the number of bytes in an atomic unit (that is, a block) of audio for a particular format. For Pulse Code Modulation (PCM) formats, the formula for calculating block alignment is as follows: 
                        //  •   Block Alignment = Bytes per Sample x Number of Channels
                        // For example, the block alignment value for 16-bit PCM format mono audio is 2 (2 bytes per sample x 1 channel). For 16-bit PCM format stereo audio, the block alignment value is 4.
                        // https://msdn.microsoft.com/en-us/library/system.speech.audioformat.speechaudioformatinfo.blockalign(v=vs.110).aspx
                        // Get the raw PCM from the output WAV file
                        using (var reader = new BinaryReader(new MemoryStream(rawData)))
                        {
                            data = GetRawWavData(reader, ref blockAlign).ToList();
                        }
                        break;
                    default:
                        // blockAlign is not available from ffprobe (and may or may not
                        // be relevant for non-PCM formats anyway)
                        break;
                }

                this.duration = TimeSpan.FromSeconds(durationInSeconds);
                this.format = new AudioFormat(
                    averageBytesPerSecond,
                    bitsPerSample,
                    blockAlign,
                    channelCount,
                    format,
                    sampleRate);

                // Loop start and length in number of samples. Defaults to entire sound
                loopStart = 0;
                if (data != null && bitsPerSample > 0 && channelCount > 0)
                    loopLength = data.Count / ((bitsPerSample / 8) * channelCount);
                else
                    loopLength = 0;
            }
            finally
            {
                File.Delete(temporarySource);
                File.Delete(temporaryOutput);
            }
        }

        private void Read(string filename)
        {
            // Must be opened in read mode otherwise it fails to open read-only files (found in some source control systems)
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                this.data = data.ToList();
            }
        }

        // Borrowed from AudioLoader to get the raw data from the WAV
        private static byte[] GetRawWavData(BinaryReader reader, ref int blockAlign)
        {
            byte[] audioData;

            //header
            string signature = new string(reader.ReadChars(4));
            if (signature != "RIFF")
            {
                throw new NotSupportedException("Specified stream is not a wave file.");
            }

            reader.ReadInt32(); // riff_chunck_size

            string wformat = new string(reader.ReadChars(4));
            if (wformat != "WAVE")
            {
                throw new NotSupportedException("Specified stream is not a wave file.");
            }

            // WAVE header
            string format_signature = new string(reader.ReadChars(4));
            while (format_signature != "fmt ")
            {
                reader.ReadBytes(reader.ReadInt32());
                format_signature = new string(reader.ReadChars(4));
            }

            int format_chunk_size = reader.ReadInt32();

            // total bytes read: tbp
            int audio_format = reader.ReadInt16(); // 2
            int num_channels = reader.ReadInt16(); // 4
            int sample_rate = reader.ReadInt32();  // 8
            reader.ReadInt32();    // 12, byte_rate
            blockAlign = reader.ReadInt16();  // 14, block_align
            int bits_per_sample = reader.ReadInt16(); // 16

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
                throw new NotSupportedException("Specified wave file is not supported.");
            }

            int data_chunk_size = reader.ReadInt32();
            audioData = reader.ReadBytes(data_chunk_size);

            return audioData;
        }
	}
}
