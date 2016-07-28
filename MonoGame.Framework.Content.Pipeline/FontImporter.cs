// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reading .ttf and .otf files for use in the Content Pipeline.
    /// </summary>
    [ContentImporter(
        ".ttf",
        ".otf",
        ".otc",
        DisplayName = "Font Importer - MonoGame", DefaultProcessor = "FontProcessor")]
    public class FontImporter : ContentImporter<FontDescription>
    {
        /// <summary>
        /// Called by the MonoGame Framework when importing a .ttf / .otf file to be used as a game asset. This is the method called by the MonoGame Framework when an asset is to be imported into an object that can be recognized by the Content Pipeline.
        /// </summary>
        /// <param name="filename">Name of a game asset file.</param>
        /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
        /// <returns>Resulting game asset.</returns>
        public override FontDescription Import(string filename, ContentImporterContext context)
        {
            return new FontDescription { FontName = filename };
        }
    }
}
