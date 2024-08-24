// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// This class is used in the <see cref="GameWindow.FileDrop"/> event as <see cref="EventArgs"/>.
    /// </summary>
    public struct FileDropEventArgs
    {
        /// <summary>
        /// Creates an instance of <see cref="FileDropEventArgs"/>.
        /// </summary>
        /// <param name="files">Array of paths to dropped files.</param>
        public FileDropEventArgs(string[] files)
        {
            Files = files;
        }

        /// <summary>
        /// The paths of dropped files
        /// </summary>
        public string[] Files { get; private set; }
    }
}
