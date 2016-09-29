// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Implements an importer for reading intermediate XML files. This is a wrapper around IntermediateSerializer.
    /// </summary>
    [ContentImporter(".xml", DisplayName = "Xml Importer - MonoGame", DefaultProcessor = "PassThroughProcessor")]
    public class XmlImporter : ContentImporter<object>
    {
        /// <summary>
        /// Called by the XNA Framework when importing an intermediate file to be used as a game 
        /// asset. This is the method called by the XNA Framework when an asset is to be imported 
        /// into an object that can be recognized by the Content Pipeline.
        /// </summary>
        /// <param name="filename">Name of a game asset file.</param>
        /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
        /// <returns>The imported game asset.</returns>
        public override object Import(string filename, ContentImporterContext context)
        {
            using (var reader = XmlReader.Create(filename))
                return IntermediateSerializer.Deserialize<object>(reader, filename);
        }
    }
}
