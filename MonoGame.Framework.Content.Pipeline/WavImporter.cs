// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reading .wav audio files for use in the Content Pipeline.
    /// </summary>
    [ContentImporter(".wav", DisplayName = "Wav Importer - MonoGame", DefaultProcessor = "SoundEffectProcessor")]
    public class WavImporter : ContentImporter<AudioContent>
    {
        /// <summary>
        /// Called by the XNA Framework when importing a .wav audio file to be used as a game asset. This is the method called by the XNA Framework when an asset is to be imported into an object that can be recognized by the Content Pipeline.
        /// </summary>
        /// <param name="filename">Name of a game asset file.</param>
        /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
        /// <returns>Resulting game asset.</returns>
        /// <remarks>This importer only supports 8bit and 16bit depths with sample rates from 8KHz up to 48KHz.</remarks>
        public override AudioContent Import(string filename, ContentImporterContext context)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            if (context == null)
                throw new ArgumentNullException("context");

            if (!File.Exists(filename))
                throw new FileNotFoundException(string.Format("Could not locate audio file {0}.", Path.GetFileName(filename)));

            var content = new AudioContent(filename, AudioFileType.Wav);

            // Validate the format of the input.
            if (content.Format.SampleRate < 8000 || content.Format.SampleRate > 48000)
                throw new InvalidContentException(string.Format("Audio file {0} contains audio data with unsupported sample rate of {1}KHz. Supported sample rates are from 8KHz up to 48KHz.", Path.GetFileName(filename), content.Format.SampleRate));
            if (content.Format.BitsPerSample != 8 && content.Format.BitsPerSample != 16)
                throw new InvalidContentException(string.Format("Audio file {0} contains audio data with unsupported bit depth of {1}. Supported bit depths are from 8bit and 16bit.", Path.GetFileName(filename), content.Format.BitsPerSample));
            
            return content;
        }
    }
}
