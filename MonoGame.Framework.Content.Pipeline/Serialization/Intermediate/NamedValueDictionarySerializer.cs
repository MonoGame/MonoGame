// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class NamedValueDictionarySerializer<T> : ContentTypeSerializer<NamedValueDictionary<T>>
    {
        private ContentTypeSerializer _keySerializer;

        private ContentSerializerAttribute _keyFormat;
        private ContentSerializerAttribute _valueFormat;

        public NamedValueDictionarySerializer() :
            base("namedValueDictionary")
        {
        }

        public override bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

        protected internal override void Initialize(IntermediateSerializer serializer)
        {
            _keySerializer = serializer.GetTypeSerializer(typeof(string));

            _keyFormat = new ContentSerializerAttribute
            {
                ElementName = "Key",
                AllowNull = false
            };

            _valueFormat = new ContentSerializerAttribute
            {
                ElementName = "Value",
                AllowNull = typeof(T).IsValueType
            };
        }

        public override bool ObjectIsEmpty(NamedValueDictionary<T> value)
        {
            return value.Count == 0;
        }

        protected internal override void ScanChildren(IntermediateSerializer serializer, ChildCallback callback, NamedValueDictionary<T> value)
        {
            foreach (var kvp in value)
                callback(serializer.GetTypeSerializer(typeof(T)), kvp.Value);
        }

        protected internal override NamedValueDictionary<T> Deserialize(IntermediateReader input, ContentSerializerAttribute format, NamedValueDictionary<T> existingInstance)
        {
            var result = existingInstance ?? new NamedValueDictionary<T>();

            var valueSerializer = input.Serializer.GetTypeSerializer(result.DefaultSerializerType);

            while (input.MoveToElement(format.CollectionItemName))
            {
                input.Xml.ReadStartElement();

                var key = input.ReadObject<string>(_keyFormat, _keySerializer);
                var value = input.ReadObject<T>(_valueFormat, valueSerializer);
                result.Add(key,value);

                input.Xml.ReadEndElement();
            }

            return result;
        }

        protected internal override void Serialize(IntermediateWriter output, NamedValueDictionary<T> value, ContentSerializerAttribute format)
        {
            var valueSerializer = output.Serializer.GetTypeSerializer(value.DefaultSerializerType);

            foreach (var kvp in value)
            {
                output.Xml.WriteStartElement(format.CollectionItemName);

                output.WriteObject(kvp.Key, _keyFormat, _keySerializer);
                output.WriteObject(kvp.Value, _valueFormat, valueSerializer);

                output.Xml.WriteEndElement();
            }
        }
    }
}