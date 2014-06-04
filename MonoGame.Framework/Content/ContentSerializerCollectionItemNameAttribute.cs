// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// This is used to specify the XML element name to use for each item in a collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ContentSerializerCollectionItemNameAttribute : Attribute
    {
        /// <summary>
        /// Creates an instance of the attribute.
        /// </summary>
        /// <param name="collectionItemName">The XML element name to use for each item in the collection.</param>
        public ContentSerializerCollectionItemNameAttribute(string collectionItemName)
        {
            CollectionItemName = collectionItemName;
        }

        /// <summary>
        /// The XML element name to use for each item in the collection.
        /// </summary>
        public string CollectionItemName { get; private set;}
    }
}

