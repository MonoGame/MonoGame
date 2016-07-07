// MonoGame - Copyright (C) The MonoGame Team
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
                    platform == TargetPlatform.WindowsPhone ||
                    platform == TargetPlatform.WindowsPhone8 ||
                    platform == TargetPlatform.WindowsStoreApp ||
                    platform == TargetPlatform.iOS;
        }

        public override ConversionQuality ConvertAudio(TargetPlatform platform, ConversionQuality quality, AudioContent content)
        {
            // Default to PCM data.
            var targetFormat = ConversionFormat.Pcm;
            if (quality != ConversionQuality.Best)
            {
                if (platform == TargetPlatform.iOS || platform == TargetPlatform.MacOSX)
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

            if (    platform == TargetPlatform.Windows ||
                    platform == TargetPlatform.WindowsPhone8 ||
                    platform == TargetPlatform.WindowsStoreApp)
                targetFormat = ConversionFormat.WindowsMedia;

            else if (platform == TargetPlatform.DesktopGL)
                targetFormat = ConversionFormat.Vorbis;

            // Get the song output path with the target format extension.
            outputFileName = Path.ChangeExtension(outputFileName, AudioHelper.GetExtension(targetFormat));

            // Make sure the output folder for the file exists.
            Directory.CreateDirectory(Path.GetDirectoryName(outputFileName));

            return ConvertToFormat(content, targetFormat, quality, outputFileName);
        }

        public static void ProbeFormat(string sourceFile, out AudioFormat audioFormat, out TimeSpan duration, out int loopStart, out int loopLength)
        {
            string ffprobeStdout, ffprobeStderr;
            var ffprobeExitCode = ExternalTool.Run(
                "ffprobe",
                string.Format("-i \"{0}\" -show_entries streams -v quiet -of flat", sourceFile),
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
            double durationInSeconds = 0;

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
                        case "streams.stream.0.duration":
                            durationInSeconds = double.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.channels":
                            channelCount = int.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.bit_rate":
                            averageBytesPerSecond = (int.Parse(kv[1].Trim('"'), numberFormat)/8);
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

            // Calculate block alignment... it may only be valid if 
            // ffprob returns a bits per sample value.
            {
                // Block alignment value is the number of bytes in an atomic unit (that is, a block) of audio for a particular format. For Pulse Code Modulation (PCM) formats, the formula for calculating block alignment is as follows: 
                //  - Block Alignment = Bytes per Sample x Number of Channels
                // For example, the block alignment value for 16-bit PCM format mono audio is 2 (2 bytes per sample x 1 channel). For 16-bit PCM format stereo audio, the block alignment value is 4.
                // https://msdn.microsoft.com/en-us/library/system.speech.audioformat.speechaudioformatinfo.blockalign(v=vs.110).aspx
                blockAlign = (bitsPerSample / 8) * channelCount;
            }

            duration = TimeSpan.FromSeconds(durationInSeconds);
            audioFormat = new AudioFormat(
                averageBytesPerSecond,
                bitsPerSample,
                blockAlign,
                channelCount,
                format,
                sampleRate);

            // Loop start and length in number of samples. Defaults to entire sound
            // TODO: Extract loop info when it exists from ffprob.
            loopStart = 0;
            loopLength = (int)Math.Floor(sampleRate * durationInSeconds);
        }

        public static void WritePcmFile(AudioContent content, string saveToFile)
        {
            var temporarySource = Path.GetTempFileName();

            try
            {
                File.WriteAllBytes(temporarySource, content.Data.ToArray());

                string ffmpegStdout, ffmpegStderr;
                var ffmpegExitCode = ExternalTool.Run(
                    "ffmpeg",
                    string.Format(
                        "-y -i \"{0}\" -vn -c:a pcm_s16le -b:a 192000 -f:a wav -strict experimental \"{1}\"",
                        temporarySource,
                        saveToFile),
                    out ffmpegStdout,
                    out ffmpegStderr);
                if (ffmpegExitCode != 0)
                    throw new InvalidOperationException("ffmpeg exited with non-zero exit code: \n" + ffmpegStdout + "\n" + ffmpegStderr);
            }
            finally
            {
                ExternalTool.DeleteFile(temporarySource);
            }            
        }

        public static ConversionQuality ConvertToFormat(AudioContent content, ConversionFormat formatType, ConversionQuality quality, string saveToFile)
        {
            var temporarySource = Path.GetTempFileName();
            var temporaryOutput = Path.GetTempFileName();
            try
            {
                using (var fs = new FileStream(temporarySource, FileMode.Create, FileAccess.Write))
                {
                    var dataBytes = content.Data.ToArray();
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
                        ffmpegMuxerName = "s16le";
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
                int ffmpegExitCode;
                do
                {
                    ffmpegExitCode = ExternalTool.Run(
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
                AudioFormat audioFormat;
                TimeSpan duration;
                int loopStart, loopLength;
                ProbeFormat(temporaryOutput, out audioFormat, out duration, out loopStart, out loopLength);

                content.SetData(rawData, audioFormat, duration, loopStart, loopLength);
            }
            finally
            {
                ExternalTool.DeleteFile(temporarySource);
                ExternalTool.DeleteFile(temporaryOutput);
            }

            return quality;
        }
    }
}
