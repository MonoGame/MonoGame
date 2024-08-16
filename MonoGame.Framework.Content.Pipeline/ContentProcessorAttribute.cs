// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Gets any existing content processor components.
    /// </summary>
    public class ContentProcessorAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the string representing the processor in a user interface. This name is not used by the content pipeline and should not be passed to the BuildAssets task (a custom MSBuild task used by XNA Game Studio). It is used for display purposes only.
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// Initializes an instance of ContentProcessorAttribute.
        /// </summary>
        public ContentProcessorAttribute()
        {
        }
    }
}
