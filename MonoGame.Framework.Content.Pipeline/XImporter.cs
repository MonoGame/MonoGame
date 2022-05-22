// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reading DirectX Object (.x) files for use in the Content Pipeline.
    /// </summary>
    [ContentImporter(".x", DisplayName = "X Importer - MonoGame", DefaultProcessor = "ModelProcessor")]
    public class XImporter : ContentImporter<NodeContent>
    {
        /// <summary>
        /// Initializes a new instance of XImporter.
        /// </summary>
        public XImporter()
        {
        }

        /// <summary>
        /// Called by the XNA Framework when importing a .x file to be used as a game asset. This is the method called by the XNA Framework when an asset is to be imported into an object that can be recognized by the Content Pipeline.
        /// </summary>
        /// <param name="filename">Name of a game asset file.</param>
        /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
        /// <returns>Resulting game asset.</returns>
        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            var importer = new OpenAssetImporter("XImporter", true);
            return importer.Import(filename, context);
        }
    }
}
