// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    /// <summary>
    /// Used to identify custom ContentTypeSerializer classes. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ContentTypeSerializerAttribute : Attribute
    {
        /// <summary>
        /// Initializes an instance of the ContentTypeSerializerAttribute.
        /// </summary>
        public ContentTypeSerializerAttribute()
        {
        }
    }
}