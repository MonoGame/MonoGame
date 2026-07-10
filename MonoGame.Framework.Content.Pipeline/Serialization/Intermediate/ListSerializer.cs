// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class ListSerializer<T> : ContentTypeSerializer<List<T>>
    {
        private ContentTypeSerializer _itemSerializer;

        public ListSerializer() :
            base("list")
        {
        }

        public override bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

        protected internal override void Initialize(IntermediateSerializer serializer)
        {
            _itemSerializer = serializer.GetTypeSerializer(typeof(T));
        }

        public override bool ObjectIsEmpty(List<T> value)
        {
            return value.Count == 0;
        }

        protected internal override void ScanChildren(IntermediateSerializer serializer, ChildCallback callback, List<T> value)
        {
            foreach (var item in value)
                callback(_itemSerializer, item);
        }

        protected internal override List<T> Deserialize(IntermediateReader input, ContentSerializerAttribute format, List<T> existingInstance)
        {
            var result = existingInstance ?? new List<T>();

            var elementSerializer = _itemSerializer as ElementSerializer<T>;
            if (elementSerializer != null)
                elementSerializer.Deserialize(input, result);
            else
            {
                // Create the item serializer attribute.
                var itemFormat = new ContentSerializerAttribute();
                itemFormat.ElementName = format.CollectionItemName;

                // Read all the items.
                while (input.MoveToElement(itemFormat.ElementName))
                {
                    var value = input.ReadObject<T>(itemFormat, _itemSerializer);
                    result.Add(value);
                }
            }

            return result;
        }

        protected internal override void Serialize(IntermediateWriter output, List<T> value, ContentSerializerAttribute format)
        {
            var elementSerializer = _itemSerializer as ElementSerializer<T>;
            if (elementSerializer != null)
                elementSerializer.Serialize(output, value);
            else
            {
                // Create the item serializer attribute.
                var itemFormat = new ContentSerializerAttribute();
                itemFormat.ElementName = format.CollectionItemName;

                // Read all the items.
                foreach (var item in value)
                    output.WriteObject(item, itemFormat, _itemSerializer);
            }
        }
    }
}