#region License
/*
 Microsoft Public License (Ms-PL)
 MonoGame - Copyright © 2012 The MonoGame Team
 
 All rights reserved.
 
 This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
 accept the license, do not use the software.
 
 1. Definitions
 The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
 U.S. copyright law.
 
 A "contribution" is the original software, or any additions or changes to the software.
 A "contributor" is any person that distributes its contribution under this license.
 "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 
 2. Grant of Rights
 (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 
 3. Conditions and Limitations
 (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
 your patent license from such contributor to the software ends automatically.
 (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
 notices that are present in the software.
 (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
 a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
 code form, you may only do so under a license that complies with this license.
 (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
 or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
 permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
 purpose and non-infringement.
 */
#endregion License

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
