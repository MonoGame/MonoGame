// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// This is used to specify the version when deserializing this object at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class ContentSerializerTypeVersionAttribute : Attribute
    {
        /// <summary>
        /// Creates an instance of the attribute.
        /// </summary>
        /// <param name="typeVersion">The version passed to the type at runtime.</param>
        public ContentSerializerTypeVersionAttribute(int typeVersion)
        {
            TypeVersion = typeVersion;
        }

        /// <summary>
        /// The version passed to the type at runtime.
        /// </summary>
        public int TypeVersion { get; private set; }
    }
}

