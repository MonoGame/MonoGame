// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Globalization;
using System.IO;
using System.Linq;


namespace Microsoft.Xna.Framework.Content.Pipeline.Audio
{
    internal class DefaultAudioProfile : AudioProfile
    {
        public override bool Supports(TargetPlatform platform)
        {
            return  platform == TargetPlatform.Android ||
                    platform == TargetPlatform.DesktopGL ||
                    platform == TargetPlatform.MacOSX ||
                    platform == TargetPlatform.NativeClient ||
                    platform == TargetPlatform.RaspberryPi ||
                    platform == TargetPlatform.Windows ||
                    platform == TargetPlatform.iOS ||
                    platform == TargetPlatform.Web;
        }

        public override ConversionQuality ConvertAudio(TargetPlatform platform, ConversionQuality quality, AudioContent content)
        {
            // Default to PCM data, or ADPCM if the source is ADPCM.
            var targetFormat = ConversionFormat.Pcm;
            if (quality != ConversionQuality.Best || content.Format.Format == 2 || content.Format.Format == 17)
            {
                if (platform == TargetPlatform.iOS || platform == TargetPlatform.MacOSX || platform == TargetPlatform.DesktopGL)
                    targetFormat = ConversionFormat.ImaAdpcm;
                else
                    targetFormat = ConversionFormat.Adpcm;
            }

            return ConvertToFormat(content, targetFormat, quality, null);
        }

        public override ConversionQuality ConvertStreamingAudio(TargetPlatform platform, ConversionQuality quality, AudioContent content, ref string outputFileName)
        {
            // Most platforms will use AAC ("mp4") by default
            var targetFormat = ConversionFormat.Aac;

            if ( platform == TargetPlatform.Windows )
                targetFormat = ConversionFormat.WindowsMedia;

            else if (platform == TargetPlatform.DesktopGL)
                targetFormat = ConversionFormat.Vorbis;
            else if (platform == TargetPlatform.Web)
                targetFormat = ConversionFormat.Mp3;

            // Get the song output path with the target format extension.
            outputFileName = Path.ChangeExtension(outputFileName, AudioHelper.GetExtension(targetFormat));

            // Make sure the output folder for the file exists.
            Directory.CreateDirectory(Path.GetDirectoryName(outputFileName));

            return ConvertToFormat(content, targetFormat, quality, outputFileName);
        }

        public static void ProbeFormat(string sourceFile, out AudioFileType audioFileType, out AudioFormat audioFormat, out TimeSpan duration, out int loopStart, out int loopLength)
        {
            string ffprobeStdout, ffprobeStderr;
            var ffprobeExitCode = ExternalTool.Run(
                "ffprobe",
                string.Format("-i \"{0}\" -show_format -show_entries streams -v quiet -of flat", sourceFile),
                out ffprobeStdout,
                out ffprobeStderr);
            if (ffprobeExitCode != 0)
                throw new InvalidOperationException("ffprobe exited with non-zero exit code.");

            // Set default values if information is not available.
            int averageBytesPerSecond = 0;
            int bitsPerSample = 0;
            int blockAlign = 0;
            int channelCount = 0;
            int sampleRate = 0;
            int format = 0;
            string sampleFormat = null;
            double durationInSeconds = 0;
            var formatName = string.Empty;

            try
            {
                var numberFormat = CultureInfo.InvariantCulture.NumberFormat;
                foreach (var line in ffprobeStdout.Split(new[] {'\r', '\n', '\0'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = line.Split(new[] {'='}, 2);

                    switch (kv[0])
                    {
                        case "streams.stream.0.sample_rate":
                            sampleRate = int.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.bits_per_sample":
                            bitsPerSample = int.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.start_time":
                        {
                            double seconds;
                            if (double.TryParse(kv[1].Trim('"'), NumberStyles.Any, numberFormat, out seconds))
                                durationInSeconds += seconds;
                            break;
                        }
                        case "streams.stream.0.duration":
                            durationInSeconds += double.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.channels":
                            channelCount = int.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.sample_fmt":
                            sampleFormat = kv[1].Trim('"').ToLowerInvariant();
                            break;
                        case "streams.stream.0.bit_rate":
                            averageBytesPerSecond = (int.Parse(kv[1].Trim('"'), numberFormat)/8);
                            break;
                        case "format.format_name":
                            formatName = kv[1].Trim('"').ToLowerInvariant();
                            break;
                        case "streams.stream.0.codec_tag":
                        {
                            var hex = kv[1].Substring(3, kv[1].Length - 4);
                            format = int.Parse(hex, NumberStyles.HexNumber);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse ffprobe output.", ex);
            }

            // XNA seems to use the sample format for the bits per sample
            // in the case of non-PCM formats like MP3 and WMA.
            if (bitsPerSample == 0 && sampleFormat != null)
            {
                switch (sampleFormat)
                {
                    case "u8":
                    case "u8p":
                        bitsPerSample = 8;
                        break;
                    case "s16":
                    case "s16p":
                        bitsPerSample = 16;
                        break;
                    case "s32":
                    case "s32p":
                    case "flt":
                    case "fltp":
                        bitsPerSample = 32;
                        break;
                    case "dbl":
                    case "dblp":
                        bitsPerSample = 64;
                        break;
                }
            }

            // Figure out the file type.
            var durationMs = (int)Math.Floor(durationInSeconds * 1000.0);
            if (formatName == "wav")
            {
                audioFileType = AudioFileType.Wav;
            }
            else if (formatName == "mp3")
            {
                audioFileType = AudioFileType.Mp3;
                format = 1;
                durationMs = (int)Math.Ceiling(durationInSeconds * 1000.0);
                bitsPerSample = Math.Min(bitsPerSample, 16);
            }
            else if (formatName == "wma" || formatName == "asf")
            {
                audioFileType = AudioFileType.Wma;
                format = 1;
                durationMs = (int)Math.Ceiling(durationInSeconds * 1000.0);
                bitsPerSample = Math.Min(bitsPerSample, 16);
            }
            else if (formatName == "ogg")
            {
                audioFileType = AudioFileType.Ogg;
                format = 1;
                durationMs = (int)Math.Ceiling(durationInSeconds * 1000.0);
                bitsPerSample = Math.Min(bitsPerSample, 16);
            }
            else
                audioFileType = (AudioFileType) (-1);

            // XNA seems to calculate the block alignment directly from 
            // the bits per sample and channel count regardless of the 
            // format of the audio data.
            // ffprobe doesn't report blockAlign for ADPCM and we cannot calculate it like this
            if (bitsPerSample > 0 && (format != 2 && format != 17))
                blockAlign = (bitsPerSample * channelCount) / 8;

            // XNA seems to only be accurate to the millisecond.
            duration = TimeSpan.FromMilliseconds(durationMs);

            // Looks like XNA calculates the average bps from
            // the sample rate and block alignment.
            if (blockAlign > 0)
                averageBytesPerSecond = sampleRate * blockAlign;

            audioFormat = new AudioFormat(
                averageBytesPerSecond,
                bitsPerSample,
                blockAlign,
                channelCount,
                format,
                sampleRate);

            // Loop start and length in number of samples.  For some
            // reason XNA doesn't report loop length for non-WAV sources.
            loopStart = 0;
            if (audioFileType != AudioFileType.Wav)
                loopLength = 0;
            else
                loopLength = (int)Math.Floor(sampleRate * durationInSeconds);
        }

        internal static byte[] StripRiffWaveHeader(byte[] data, out AudioFormat audioFormat)
        {
            audioFormat = null;

            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    return data;

                reader.ReadInt32(); // riff_chunck_size

                var wformat = new string(reader.ReadChars(4));
                if (wformat != "WAVE")
                    return data;

                // Look for the data chunk.
                while (true)
                {
                    var chunkSignature = new string(reader.ReadChars(4));
                    if (chunkSignature.ToLowerInvariant() == "data")
                        break;
                    if (chunkSignature.ToLowerInvariant() == "fmt ")
                    {
                        int fmtLength = reader.ReadInt32();
                        short formatTag = reader.ReadInt16();
                        short channels = reader.ReadInt16();
                        int sampleRate = reader.ReadInt32();
                        int avgBytesPerSec = reader.ReadInt32();
                        short blockAlign = reader.ReadInt16();
                        short bitsPerSample = reader.ReadInt16();
                        audioFormat = new AudioFormat(avgBytesPerSec, bitsPerSample, blockAlign, channels, formatTag, sampleRate);

                        fmtLength -= 2 + 2 + 4 + 4 + 2 + 2;
                        if (fmtLength < 0)
                            throw new InvalidOperationException("riff wave header has unexpected format");
                        reader.BaseStream.Seek(fmtLength, SeekOrigin.Current);
                    }
                    else
                    {
                        reader.BaseStream.Seek(reader.ReadInt32(), SeekOrigin.Current);
                    }
                }

                var dataSize = reader.ReadInt32();
                data = reader.ReadBytes(dataSize);
            }

            return data;
        }

        public static void WritePcmFile(AudioContent content, string saveToFile, int bitRate = 192000, int? sampeRate = null)
        {
            string ffmpegStdout, ffmpegStderr;
            var ffmpegExitCode = ExternalTool.Run(
                "ffmpeg",
                string.Format(
                    "-y -i \"{0}\" -vn -c:a pcm_s16le -b:a {2} {3} -f:a wav -strict experimental \"{1}\"",
                    content.FileName,
                    saveToFile,
                    bitRate,
                    sampeRate != null ? "-ar " + sampeRate.Value : ""
                    ),
                out ffmpegStdout,
                out ffmpegStderr);
            if (ffmpegExitCode != 0)
                throw new InvalidOperationException("ffmpeg exited with non-zero exit code: \n" + ffmpegStdout + "\n" + ffmpegStderr);          
        }

        public static ConversionQuality ConvertToFormat(AudioContent content, ConversionFormat formatType, ConversionQuality quality, string saveToFile)
        {
            var temporaryOutput = Path.GetTempFileName();
            try
            {
                string ffmpegCodecName, ffmpegMuxerName;
                //int format;
                switch (formatType)
                {
                    case ConversionFormat.Adpcm:
                        // ADPCM Microsoft 
                        ffmpegCodecName = "adpcm_ms";
                        ffmpegMuxerName = "wav";
                        //format = 0x0002; /* WAVE_FORMAT_ADPCM */
                        break;
                    case ConversionFormat.Pcm:
                        // XNA seems to preserve the bit size of the input
                        // format when converting to PCM.
                        if (content.Format.BitsPerSample == 8)
                            ffmpegCodecName = "pcm_u8";
                        else if (content.Format.BitsPerSample == 32 && content.Format.Format == 3)
                            ffmpegCodecName = "pcm_f32le";
                        else
                            ffmpegCodecName = "pcm_s16le";
                        ffmpegMuxerName = "wav";
                        //format = 0x0001; /* WAVE_FORMAT_PCM */
                        break;
                    case ConversionFormat.WindowsMedia:
                        // Windows Media Audio 2
                        ffmpegCodecName = "wmav2";
                        ffmpegMuxerName = "asf";
                        //format = 0x0161; /* WAVE_FORMAT_WMAUDIO2 */
                        break;
                    case ConversionFormat.Xma:
                        throw new NotSupportedException(
                            "XMA is not a supported encoding format. It is specific to the Xbox 360.");
                    case ConversionFormat.ImaAdpcm:
                        // ADPCM IMA WAV
                        ffmpegCodecName = "adpcm_ima_wav";
                        ffmpegMuxerName = "wav";
                        //format = 0x0011; /* WAVE_FORMAT_IMA_ADPCM */
                        break;
                    case ConversionFormat.Aac:
                        // AAC (Advanced Audio Coding)
                        // Requires -strict experimental
                        ffmpegCodecName = "aac";
                        ffmpegMuxerName = "ipod";
                        //format = 0x0000; /* WAVE_FORMAT_UNKNOWN */
                        break;
                    case ConversionFormat.Vorbis:
                        // Vorbis
                        ffmpegCodecName = "libvorbis";
                        ffmpegMuxerName = "ogg";
                        //format = 0x0000; /* WAVE_FORMAT_UNKNOWN */
                        break;
                    case ConversionFormat.Mp3:
                        // Vorbis
                        ffmpegCodecName = "libmp3lame";
                        ffmpegMuxerName = "mp3";
                        //format = 0x0000; /* WAVE_FORMAT_UNKNOWN */
                        break;
                    default:
                        // Unknown format
                        throw new NotSupportedException();
                }

                string ffmpegStdout, ffmpegStderr;
                int ffmpegExitCode;
                do
                {
                    ffmpegExitCode = ExternalTool.Run(
                        "ffmpeg",
                        string.Format(
                            "-y -i \"{0}\" -vn -c:a {1} -b:a {2} -ar {3} -f:a {4} -strict experimental \"{5}\"",
                            content.FileName,
                            ffmpegCodecName,
                            QualityToBitRate(quality),
                            QualityToSampleRate(quality, content.Format.SampleRate),
                            ffmpegMuxerName,
                            temporaryOutput),
                        out ffmpegStdout,
                        out ffmpegStderr);
                    if (ffmpegExitCode != 0)
                        quality--;
                } while (quality >= 0 && ffmpegExitCode != 0);

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
                        fs.Write(rawData, 0, rawData.Length);
                }

                // Use probe to get the final format and information on the converted file.
                AudioFileType audioFileType;
                AudioFormat audioFormat;
                TimeSpan duration;
                int loopStart, loopLength;
                ProbeFormat(temporaryOutput, out audioFileType, out audioFormat, out duration, out loopStart, out loopLength);

                AudioFormat riffAudioFormat;
                byte[] data = StripRiffWaveHeader(rawData, out riffAudioFormat);

                // deal with adpcm
                if (audioFormat.Format == 2 || audioFormat.Format == 17)
                {
                    // riff contains correct blockAlign
                    audioFormat = riffAudioFormat;

                    // fix loopLength -> has to be multiple of sample per block
                    // see https://msdn.microsoft.com/de-de/library/windows/desktop/ee415711(v=vs.85).aspx
                    int samplesPerBlock = SampleAlignment(audioFormat);
                    loopLength = (int)(audioFormat.SampleRate * duration.TotalSeconds);
                    int remainder = loopLength % samplesPerBlock;
                    loopLength += samplesPerBlock - remainder;
                }

                content.SetData(data, audioFormat, duration, loopStart, loopLength);
            }
            finally
            {
                ExternalTool.DeleteFile(temporaryOutput);
            }

            return quality;
        }

        // Converts block alignment in bytes to sample alignment, primarily for compressed formats
        // Calculation of sample alignment from http://kcat.strangesoft.net/openal-extensions/SOFT_block_alignment.txt
        static int SampleAlignment(AudioFormat format)
        {
            switch (format.Format)
            {
                case 2:     // MS-ADPCM
                    return (format.BlockAlign / format.ChannelCount - 7) * 2 + 2;
                case 17:    // IMA/ADPCM
                    return (format.BlockAlign / format.ChannelCount - 4) / 4 * 8 + 1;
            }
            return 0;
        }
    }
}
