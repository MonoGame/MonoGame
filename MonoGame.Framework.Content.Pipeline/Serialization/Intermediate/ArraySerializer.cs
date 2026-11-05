// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

class ArraySerializer<T>() : ContentTypeSerializer<T[]>("array")
{
    private readonly ListSerializer<T> _listSerializer = new();

    protected internal override void Initialize(IntermediateSerializer serializer) => _listSerializer.Initialize(serializer);

    public override bool ObjectIsEmpty(T[]? value) => value == null || value.Length == 0;

    protected internal override void ScanChildren(IntermediateSerializer serializer, ChildCallback callback, T[] value)
        => _listSerializer.ScanChildren(serializer, callback, [.. value]);

    protected internal override T[] Deserialize(IntermediateReader input, ContentSerializerAttribute format, T[]? existingInstance)
    {
        if (existingInstance != null)
            throw new InvalidOperationException("You cannot deserialize an array into a getter-only property.");
        return [.. _listSerializer.Deserialize(input, format, null)];
    }

    protected internal override void Serialize(IntermediateWriter output, T[] value, ContentSerializerAttribute format)
        => _listSerializer.Serialize(output, [.. value], format);
}
