// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Accesses a statically typed ContentImporter instance from generic code using dynamic typing.
    /// </summary>
    public interface IContentImporter
    {
        /// <summary>
        /// Imports an asset from the specified file.
        /// </summary>
        /// <param name="filename">Name of the game asset file.</param>
        /// <param name="context">A ContentImporterContext class containing information for importing a game asset, such as a logger interface.</param>
        /// <returns>Resulting game asset.</returns>
        Object Import(string filename, ContentImporterContext context);
    }
}
