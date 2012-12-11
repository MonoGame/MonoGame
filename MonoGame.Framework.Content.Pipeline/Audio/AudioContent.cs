// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Content.Pipeline.Audio
{
    /// <summary>
    /// Encapsulates and provides operations, such as format conversions, on the source audio. This type is produced by the audio importers and used by audio processors to produce compiled audio assets.
    /// </summary>
    public class AudioContent : ContentItem, IDisposable
    {
        List<byte> data;
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before garbage collection reclaims the object.
        /// </summary>
        ~AudioContent()
        {
            Dispose(false);
        }

        /// <summary>
        /// Transcodes the source audio to the target format and quality.
        /// </summary>
        /// <param name="formatType">Format of the processed source audio: WAV, MP3 or WMA.</param>
        /// <param name="quality">Quality of the processed source audio. It can be one of the following: Low (96 kbps), Medium (128 kbps), Best (192 kbps)</param>
        /// <param name="targetFileName">Name of the file containing the processed source audio.</param>
        public void ConvertFormat(ConversionFormat formatType, ConversionQuality quality, string targetFileName)
        {
            if (disposed)
                throw new ObjectDisposedException("AudioContent");

            throw new NotImplementedException();
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        /// <param name="disposing">True if disposing of the unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Release unmanaged resources
                    // ...
                }
                disposed = true;
            }
        }
    }
}
