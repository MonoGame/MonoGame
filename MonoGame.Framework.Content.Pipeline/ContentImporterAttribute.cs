// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides properties that identify and provide metadata about the importer, such as supported file extensions and caching information.
    /// Importers are required to initialize this attribute.
    /// </summary>
    public class ContentImporterAttribute : Attribute
    {
        List<string> extensions = new List<string>();

        /// <summary>
        /// Gets and sets the caching of the content during importation.
        /// </summary>
        public bool CacheImportedData { get; set; }

        /// <summary>
        /// Gets or sets the name of the default processor for content read by this importer.
        /// </summary>
        public string DefaultProcessor { get; set; }

        /// <summary>
        /// Gets or sets the string representing the importer in a user interface. This name is not used by the content pipeline and should not be passed to the BuildAssets task (a custom MSBuild task used by XNA Game Studio). It is used for display purposes only.
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// Gets the supported file name extensions of the importer.
        /// </summary>
        public IEnumerable<string> FileExtensions { get { return extensions; } }

        /// <summary>
        /// Initializes a new instance of ContentImporterAttribute and sets the file name extension supported by the importer.
        /// </summary>
        /// <param name="fileExtension">The list of file name extensions supported by the importer. Prefix each extension with a '.'.</param>
        public ContentImporterAttribute(
            string fileExtension
            )
        {
            extensions.Add(fileExtension);
        }

        /// <summary>
        /// Initializes a new instance of ContentImporterAttribute and sets the file name extensions supported by the importer.
        /// </summary>
        /// <param name="fileExtensions">The list of file name extensions supported by the importer. Prefix each extension with a '.'.</param>
        public ContentImporterAttribute(
            params string[] fileExtensions
            )
        {
            extensions.AddRange(fileExtensions);
        }
    }
}
