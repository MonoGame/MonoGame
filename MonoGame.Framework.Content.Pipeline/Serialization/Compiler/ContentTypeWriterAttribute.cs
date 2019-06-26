// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Identifies the components of a type writer. Custom content writers must apply this attribute to their class as well as extend the ContentTypeWriter class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ContentTypeWriterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the ContentTypeWriterAttribute class.
        /// </summary>
        public ContentTypeWriterAttribute()
        {
        }
    }
}
