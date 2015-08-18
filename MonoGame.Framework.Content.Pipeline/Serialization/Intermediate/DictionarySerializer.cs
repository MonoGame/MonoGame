// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class DictionarySerializer<TKey,TValue> : ContentTypeSerializer<Dictionary<TKey,TValue>>
    {
        private ContentTypeSerializer _keySerializer;
        private ContentTypeSerializer _valueSerializer;

        private ContentSerializerAttribute _keyFormat;
        private ContentSerializerAttribute _valueFormat;

        public DictionarySerializer() :
            base("dictionary")
        {
        }

        public override bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

        protected internal override void Initialize(IntermediateSerializer serializer)
        {
            _keySerializer = serializer.GetTypeSerializer(typeof(TKey));
            _valueSerializer = serializer.GetTypeSerializer(typeof(TValue));

            _keyFormat = new ContentSerializerAttribute
            {
                ElementName = "Key",
                AllowNull = false
            };

            _valueFormat = new ContentSerializerAttribute()
            {
                ElementName = "Value",
                AllowNull = typeof(TValue).IsValueType
            };
        }

        public override bool ObjectIsEmpty(Dictionary<TKey, TValue> value)
        {
            return value.Count == 0;
        }

        protected internal override void ScanChildren(IntermediateSerializer serializer, ChildCallback callback, Dictionary<TKey, TValue> value)
        {
            foreach (var kvp in value)
            {
                callback(_keySerializer, kvp.Key);
                callback(_valueSerializer, kvp.Value);
            }
        }

        protected internal override Dictionary<TKey, TValue> Deserialize(IntermediateReader input, ContentSerializerAttribute format, Dictionary<TKey, TValue> existingInstance)
        {
            var result = existingInstance ?? new Dictionary<TKey, TValue>();

            while (input.MoveToElement(format.CollectionItemName))
            {
                input.Xml.ReadStartElement();

                var key = input.ReadObject<TKey>(_keyFormat, _keySerializer);
                var value = input.ReadObject<TValue>(_valueFormat, _valueSerializer);
                result.Add(key,value);

                input.Xml.ReadEndElement();
            }

            return result;
        }

        protected internal override void Serialize(IntermediateWriter output, Dictionary<TKey, TValue> value, ContentSerializerAttribute format)
        {
            foreach (var kvp in value)
            {
                output.Xml.WriteStartElement(format.CollectionItemName);

                output.WriteObject(kvp.Key, _keyFormat, _keySerializer);
                output.WriteObject(kvp.Value, _valueFormat, _valueSerializer);

                output.Xml.WriteEndElement();
            }
        }
    }
}