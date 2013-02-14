// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Implements a file format importer for use with game assets.
    /// Importers, either provided by the framework or written by a developer, must derive from ContentImporter, as well as being marked with a ContentImporterAttribute.
    /// An importer should produce results in the standard intermediate object model. If an asset has information not supported by the object model, the importer should output it as opaque data (key/value attributes attached to the relevant object). By following this procedure, a content pipeline can access specialized digital content creation (DCC) tool information, even when that information has not been fully standardized into the official object model.
    /// You can also design custom importers that accept and import types containing specific third-party extensions to the object model.
    /// </summary>
    public abstract class ContentImporter<T> : IContentImporter
    {
        /// <summary>
        /// Initializes a new instance of ContentImporter.
        /// </summary>
        protected ContentImporter()
        {

        }

        /// <summary>
        /// Called by the framework when importing a game asset. This is the method called by XNA when an asset is to be imported into an object that can be recognized by the Content Pipeline.
        /// </summary>
        /// <param name="filename">Name of a game asset file.</param>
        /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
        /// <returns>Resulting game asset.</returns>
        public abstract T Import(string filename, ContentImporterContext context);

        /// <summary>
        /// Called by the framework when importing a game asset. This is the method called by XNA when an asset is to be imported into an object that can be recognized by the Content Pipeline.
        /// </summary>
        /// <param name="filename">Name of a game asset file.</param>
        /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
        /// <returns>Resulting game asset.</returns>
        Object IContentImporter.Import(string filename, ContentImporterContext context)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");
            if (context == null)
                throw new ArgumentNullException("context");
            return Import(filename, context);
        }
    }
}
