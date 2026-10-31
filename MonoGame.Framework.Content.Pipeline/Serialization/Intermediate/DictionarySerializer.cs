// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

[ContentTypeSerializer]
class DictionarySerializer<TKey, TValue>() : ContentTypeSerializer<Dictionary<TKey, TValue>>("dictionary") where TKey : notnull
{
    private ContentTypeSerializer _keySerializer = null!;
    private ContentTypeSerializer _valueSerializer = null!;
    private ContentSerializerAttribute _keyFormat = null!;
    private ContentSerializerAttribute _valueFormat = null!;

    public override bool CanDeserializeIntoExistingObject => true;

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

    public override bool ObjectIsEmpty(Dictionary<TKey, TValue>? value) => value == null || value.Count == 0;

    protected internal override void ScanChildren(IntermediateSerializer serializer, ChildCallback callback, Dictionary<TKey, TValue> value)
    {
        foreach (var kvp in value)
        {
            callback(_keySerializer, kvp.Key);
            callback(_valueSerializer, kvp.Value);
        }
    }

    protected internal override Dictionary<TKey, TValue> Deserialize(IntermediateReader input, ContentSerializerAttribute format, Dictionary<TKey, TValue>? existingInstance)
    {
        var result = existingInstance ?? [];

        while (input.MoveToElement(format.CollectionItemName))
        {
            input.Xml.ReadStartElement();

            var key = input.ReadObject<TKey>(_keyFormat, _keySerializer);
            var value = input.ReadObject<TValue>(_valueFormat, _valueSerializer);
            result.Add(key, value);

            input.Xml.ReadEndElement();
        }

        return result;
    }

    protected internal override void Serialize(IntermediateWriter output, Dictionary<TKey, TValue>? value, ContentSerializerAttribute format)
    {
        foreach (var kvp in value ?? [])
        {
            output.Xml.WriteStartElement(format.CollectionItemName);

            output.WriteObject(kvp.Key, _keyFormat, _keySerializer);
            output.WriteObject(kvp.Value, _valueFormat, _valueSerializer);

            output.Xml.WriteEndElement();
        }
    }
}
