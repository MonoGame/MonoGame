// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reading Windows Media Audio (.wma) files for use in the Content Pipeline.
    /// </summary>
    [ContentImporter(".wma", DisplayName = "Wma Importer - MonoGame", DefaultProcessor = "SongProcessor")]
    public class WmaImporter : ContentImporter<AudioContent>
    {
        /// <summary>
        /// Initializes a new instance of WmaImporter.
        /// </summary>
        public WmaImporter()
        {
        }

        /// <summary>
        /// Called by the XNA Framework when importing a .wma file to be used as a game asset. This is the method called by the XNA Framework when an asset is to be imported into an object that can be recognized by the Content Pipeline.
        /// </summary>
        /// <param name="filename">Name of a game asset file.</param>
        /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
        /// <returns>Resulting game asset.</returns>
        public override AudioContent Import(string filename, ContentImporterContext context)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            if (context == null)
                throw new ArgumentNullException("context");

            if (!File.Exists(filename))
                throw new FileNotFoundException(string.Format("Could not locate audio file {0}.", Path.GetFileName(filename)));

            var content = new AudioContent(filename, AudioFileType.Wma);
            return content;
        }
    }
}
