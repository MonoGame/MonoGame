// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides properties that identify and provide metadata about the importer, such as supported file extensions and caching information.
    /// Importers are required to initialize this attribute.
    /// </summary>
    /// <param name="fileExtensions">The list of file name extensions supported by the importer. Prefix each extension with a '.'.</param>
    [AttributeUsage(AttributeTargets.Class)]
    public class ContentImporterAttribute(params string[] fileExtensions) : Attribute
    {
        /// <summary>
        /// Gets and sets the caching of the content during importation.
        /// </summary>
        public bool CacheImportedData { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the default processor for content read by this importer.
        /// </summary>
        public string DefaultProcessor { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the string representing the importer in a user interface. This name is not used by the content pipeline and should not be passed to the BuildAssets task (a custom MSBuild task used by XNA Game Studio). It is used for display purposes only.
        /// </summary>
        public virtual string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the supported file name extensions of the importer.
        /// </summary>
        public IEnumerable<string> FileExtensions { get; } = fileExtensions;
    }
}
