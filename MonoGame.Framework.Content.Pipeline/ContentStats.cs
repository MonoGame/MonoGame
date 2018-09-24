// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Content building statistics for a single source content file.
    /// </summary>
    public struct ContentStats
    {
        /// <summary>
        /// The absolute path to the source content file.
        /// </summary>
        public string SourceFile;

        /// <summary>
        /// The absolute path to the destination content file.
        /// </summary>
        public string DestFile;

        /// <summary>
        /// The content processor type name.
        /// </summary>
        public string ProcessorType;

        /// <summary>
        /// The content type name.
        /// </summary>
        public string ContentType;

        /// <summary>
        /// The source file size in bytes.
        /// </summary>
        public long SourceFileSize;

        /// <summary>
        /// The destination file size in bytes.
        /// </summary>
        public long DestFileSize;

        /// <summary>
        /// The content build time in seconds.
        /// </summary>
        public float BuildSeconds;
    }
}
