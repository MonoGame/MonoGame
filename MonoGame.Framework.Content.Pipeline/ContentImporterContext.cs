// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides properties that define logging behavior for the importer.
    /// </summary>
    public abstract class ContentImporterContext
    {
        /// <summary>
        /// The absolute path to the root of the build intermediate (object) directory.
        /// </summary>
        public abstract string IntermediateDirectory { get; }

        /// <summary>
        /// Gets the logger for an importer.
        /// </summary>
        public abstract ContentBuildLogger Logger { get; }

        /// <summary>
        /// The absolute path to the root of the build output (binaries) directory.
        /// </summary>
        public abstract string OutputDirectory { get; }

        /// <summary>
        /// Initializes a new instance of ContentImporterContext.
        /// </summary>
        public ContentImporterContext()
        {

        }

        /// <summary>
        /// Adds a dependency to the specified file. This causes a rebuild of the file, when modified, on subsequent incremental builds.
        /// </summary>
        /// <param name="filename">Name of an asset file.</param>
        public abstract void AddDependency(string filename);
    }
}
