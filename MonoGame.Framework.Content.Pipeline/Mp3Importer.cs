// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reading MP3 audio files for use in the Content Pipeline.
    /// </summary>
    [ContentImporter(".mp3", DisplayName = "Mp3 Importer - MonoGame", DefaultProcessor = "SongProcessor")]
    public class Mp3Importer : ContentImporter<AudioContent>
    {
        /// <summary>
        /// Initializes a new instance of Mp3Importer.
        /// </summary>
        public Mp3Importer()
        {
        }

        /// <summary>
        /// Called by the XNA Framework when importing an MP3 audio file to be used as a game asset. This is the method called by the XNA Framework when an asset is to be imported into an object that can be recognized by the Content Pipeline.
        /// </summary>
        /// <param name="filename">Name of a game asset file.</param>
        /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
        /// <returns>Resulting game asset.</returns>
        public override AudioContent Import(string filename, ContentImporterContext context)
        {
            throw new NotImplementedException();
        }
    }
}
