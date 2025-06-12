// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Tools.Pipeline
{
    public enum IncludeType
    {
        Skip,
        Copy,
        Link,
        Create
    }

    /// <summary>
    /// This class is used for IncludeAction file handling.
    /// </summary>
    public class IncludeItem
    {
        /// <summary>
        /// Gets or sets the source path.
        /// </summary>
        /// <value>The source path, should be an absolute path.</value>
        public string SourcePath { get; set; }

        public IncludeType IncludeType { get; set; }

        /// <summary>
        /// Gets or sets the relative destination path.
        /// </summary>
        /// <value>The relative destination path.</value>
        public string RelativeDestPath { get; set; }

        public bool IsDirectory { get; set; }

        /// <summary>
        /// Gets or sets the item template.
        /// 
        /// Only usefull if the action is create and the item is not a directory.
        /// </summary>
        /// <value>The item template.</value>
        public ContentItemTemplate ItemTemplate { get; set; }
    }
}
