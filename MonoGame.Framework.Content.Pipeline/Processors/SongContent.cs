// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Represents a processed Song object.
    /// </summary>
    public sealed class SongContent
    {
        internal string fileName;
        internal TimeSpan duration;

        /// <summary>
        /// Creates a new instance of the SongContent class
        /// </summary>
        /// <param name="fileName">Filename of the song</param>
        /// <param name="duration">Duration of the song</param>
        internal SongContent(string fileName, TimeSpan duration)
        {
            this.fileName = fileName;
            this.duration = duration;
        }
    }
}
