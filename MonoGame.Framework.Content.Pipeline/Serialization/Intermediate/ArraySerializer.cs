// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    class ArraySerializer<T> : ContentTypeSerializer<T[]>
    {
        private ContentTypeSerializer _itemSerializer;

        public ArraySerializer() :
            base("array")
        {
        }

        protected internal override void Initialize(IntermediateSerializer serializer)
        {
            _itemSerializer = serializer.GetTypeSerializer(TargetType.GetElementType());
        }

        protected internal override T[] Deserialize(IntermediateReader input, ContentSerializerAttribute format, T[] existingInstance)
        {
            var result = new List<T>();

            // Create the item serializer attribute.
            var itemFormat = new ContentSerializerAttribute();
            itemFormat.ElementName = format.CollectionItemName;

            // Read all the items.
            while (input.MoveToElement(itemFormat.ElementName))
            {
                var value = input.ReadObject<T>(itemFormat, _itemSerializer);
                result.Add(value);
            }

            return result.ToArray();
        }

        protected internal override void Serialize(IntermediateWriter output, T[] value, ContentSerializerAttribute format)
        {
            throw new NotImplementedException();
        }
    }
}