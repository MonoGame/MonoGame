// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using NAudio.Wave;

namespace Microsoft.Xna.Framework.Content.Pipeline.Audio
{
    /// <summary>
    /// Encapsulates and provides operations, such as format conversions, on the source audio. This type is produced by the audio importers and used by audio processors to produce compiled audio assets.
    /// </summary>
    public class AudioContent : ContentItem, IDisposable
    {
        internal List<byte> data;
        WaveStream reader;
        TimeSpan duration;
        string fileName;
        AudioFileType fileType;
        AudioFormat format;
        int loopLength;
        int loopStart;
        bool disposed;

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
            Read();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before garbage collection reclaims the object.
        /// </summary>
        ~AudioContent()
        {
            Dispose(false);
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
        /// Converts the audio using the specified wave format.
        /// </summary>
        /// <param name="waveFormat">The WaveFormat to use for the conversion.</param>
        void ConvertWav(WaveFormat waveFormat)
        {
            reader.Position = 0;
#if WINDOWS
            //var mediaTypes = MediaFoundationEncoder.GetOutputMediaTypes(NAudio.MediaFoundation.AudioSubtypes.MFAudioFormat_PCM);
            using (var resampler = new MediaFoundationResampler(reader, waveFormat))
            {
                using (var outStream = new MemoryStream())
                {
                    // Since we cannot determine ahead of time the number of bytes to be
                    // read, read four seconds worth at a time.
                    byte[] bytes = new byte[reader.WaveFormat.AverageBytesPerSecond * 4];
                    while (true)
                    {
                        int bytesRead = resampler.Read(bytes, 0, bytes.Length);
                        if (bytesRead == 0)
                            break;
                        outStream.Write(bytes, 0, bytesRead);
                    }
                    data = new List<byte>(outStream.ToArray());
                    format = new AudioFormat(waveFormat);
                }
            }
#else
            throw new NotImplementedException();
#endif
        }

        /// <summary>
        /// Transcodes the source audio to the target format and quality.
        /// </summary>
        /// <param name="formatType">Format to convert this audio to.</param>
        /// <param name="quality">Quality of the processed output audio. For streaming formats, it can be one of the following: Low (96 kbps), Medium (128 kbps), Best (192 kbps).  For WAV formats, it can be one of the following: Low (11kHz ADPCM), Medium (22kHz ADPCM), Best (44kHz PCM)</param>
        /// <param name="targetFileName">Name of the file containing the processed source audio. Must be null for Wav and Adpcm. Must not be null for streaming compressed formats.</param>
        public void ConvertFormat(ConversionFormat formatType, ConversionQuality quality, string targetFileName)
        {
            if (disposed)
                throw new ObjectDisposedException("AudioContent");

            switch (formatType)
            {
                case ConversionFormat.Adpcm:
                    ConvertWav(new AdpcmWaveFormat(QualityToSampleRate(quality), format.ChannelCount));
                    break;

                case ConversionFormat.Pcm:
                    ConvertWav(new WaveFormat(QualityToSampleRate(quality), format.ChannelCount));
                    break;

                case ConversionFormat.WindowsMedia:
#if WINDOWS
                    reader.Position = 0;
                    MediaFoundationEncoder.EncodeToWma(reader, targetFileName, QualityToBitRate(quality));
                    break;
#else
                    throw new NotSupportedException("WindowsMedia encoding supported on Windows only");
#endif

                case ConversionFormat.Xma:
                    throw new NotSupportedException("XMA is not a supported encoding format. It is specific to the Xbox 360.");

                case ConversionFormat.ImaAdpcm:
                    ConvertWav(new ImaAdpcmWaveFormat(QualityToSampleRate(quality), format.ChannelCount, 4));
                    break;

                case ConversionFormat.Aac:
#if WINDOWS
                    reader.Position = 0;
                    MediaFoundationEncoder.EncodeToAac(reader, targetFileName, QualityToBitRate(quality));
                    break;
#else
                    throw new NotImplementedException();
#endif

                case ConversionFormat.Vorbis:
                    throw new NotImplementedException("Vorbis is not yet implemented as an encoding format.");
            }
        }

        /// <summary>
        /// Immediately releases the managed and unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        /// <param name="disposing">True if disposing of the managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Release managed resources
                    if (reader != null)
                        reader.Dispose();
                    reader = null;
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Read an audio file.
        /// </summary>
        void Read()
        {
#if WINDOWS
            reader = new MediaFoundationReader(fileName);
            duration = reader.TotalTime;
            format = new AudioFormat(reader.WaveFormat);

            var bytes = new byte[reader.Length];
            var read = reader.Read(bytes, 0, bytes.Length);
            data = new List<byte>(bytes);
#else
            throw new NotImplementedException();
#endif
        }
    }
}
