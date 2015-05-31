// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reading .ogg audio files for use in the Content Pipeline.
    /// </summary>
    [ContentImporter(".ogg", DisplayName = "Ogg Importer - MonoGame", DefaultProcessor = "SongProcessor")]
    public class OggImporter : ContentImporter<AudioContent>
    {
        /// <summary>
        /// Called by the XNA Framework when importing an ogg audio file to be used as a game asset. This is the method called by the XNA Framework when an asset is to be imported into an object that can be recognized by the Content Pipeline.
        /// </summary>
        /// <param name="filename">Name of a game asset file.</param>
        /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
        /// <returns>Resulting game asset.</returns>
        public override AudioContent Import(string filename, ContentImporterContext context)
        {
            var content = new AudioContent(filename, AudioFileType.Wav);
            return content;
        }
    }
}
