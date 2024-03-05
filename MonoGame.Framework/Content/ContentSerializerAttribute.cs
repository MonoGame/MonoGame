// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// Defines a custom <see cref="Attribute"/> that marks a field or property to control how it is serialized or to
    /// indicate that protected or private data should be included in serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ContentSerializerAttribute : Attribute
    {
        private string _collectionItemName;

        /// <summary>
        /// Initializes a new instance of the content serializer attribute.
        /// </summary>
        public ContentSerializerAttribute()
        {
            AllowNull = true;
        }

        /// <summary>
        /// Gets or Sets a value that indicates whether this member can have a null value (default is true).
        /// </summary>
        public bool AllowNull { get; set; }

        /// <summary>
        /// Gets or Sets the XML element name for each item in a collection (default is "Item"). 
        /// </summary>
        public string CollectionItemName
        {
            get
            {
                // Return the default if unset.
                if (string.IsNullOrEmpty(_collectionItemName))
                    return "Item";

                return _collectionItemName;
            }
            set
            {
                _collectionItemName = value;
            }
        }

        /// <summary>
        /// Gets or Sets the XML element name (default is the name of the managed type member).
        /// </summary>
        public string ElementName { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether to write member contents directly into the current XML context
        /// rather than wrapping the member in a new XML element (default is false).
        /// </summary>
        public bool FlattenContent { get; set; }

        /// <summary>
        /// Gets a value that indicates whether an explicit <see cref="CollectionItemName"/> is being used or the
        /// default value.
        /// </summary>
        public bool HasCollectionItemName
        {
            get
            {
                return !string.IsNullOrEmpty(_collectionItemName);
            }
        }

        /// <summary>
        /// Gets or Sets a value indicating whether to write this element if the member is null and skip past it if not
        /// found when deserializing XML (default is false).
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether this member is reference from multiple parents and should be
        /// serialized as a unique ID reference (default is false).
        /// </summary>
        public bool SharedResource { get; set; }

        /// <summary>
        /// Creates a copy of this content serializer attribute.
        /// </summary>
        /// <returns>The copy of this content serializer attribute.</returns>
        public ContentSerializerAttribute Clone()
        {
            var clone = new ContentSerializerAttribute ();
            clone.AllowNull = AllowNull;
            clone._collectionItemName = _collectionItemName;
            clone.ElementName = ElementName;
            clone.FlattenContent = FlattenContent;
            clone.Optional = Optional;
            clone.SharedResource = SharedResource;
            return clone;
        }
    }
} 
