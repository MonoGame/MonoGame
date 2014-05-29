// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// This is used to specify the type to use when deserializing this object at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class ContentSerializerRuntimeTypeAttribute : Attribute
    {
        /// <summary>
        /// Creates an instance of the attribute.
        /// </summary>
        /// <param name="runtimeType">The name of the type to use at runtime.</param>
        public ContentSerializerRuntimeTypeAttribute(string runtimeType)
        {
            RuntimeType = runtimeType;
        }

        /// <summary>
        /// The name of the type to use at runtime.
        /// </summary>
        public string RuntimeType { get; private set;}
    }
}

