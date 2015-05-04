// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    class NonGenericIListSerializer : ContentTypeSerializer
    {
        public NonGenericIListSerializer(Type targetType) :
            base(targetType, targetType.Name)
        {
        }

        public override bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

        public override bool ObjectIsEmpty(object value)
        {
            return ((IList) value).Count == 0;
        }

        protected internal override object Deserialize(IntermediateReader input, ContentSerializerAttribute format, object existingInstance)
        {
            var result = (IList) (existingInstance ?? Activator.CreateInstance(TargetType));

            // Create the item serializer attribute.
            var itemFormat = new ContentSerializerAttribute();
            itemFormat.ElementName = format.CollectionItemName;

            // Read all the items.
            while (input.MoveToElement(itemFormat.ElementName))
            {
                var value = input.ReadObject<object>(itemFormat);
                result.Add(value);
            }

            return result;
        }

        protected internal override void Serialize(IntermediateWriter output, object value, ContentSerializerAttribute format)
        {
            // Create the item serializer attribute.
            var itemFormat = new ContentSerializerAttribute();
            itemFormat.ElementName = format.CollectionItemName;

            // Read all the items.
            foreach (var item in (IList) value)
                output.WriteObject(item, itemFormat);
        }
    }
}