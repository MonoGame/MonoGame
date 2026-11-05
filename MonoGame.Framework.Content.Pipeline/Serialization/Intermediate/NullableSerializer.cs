// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

[ContentTypeSerializer]
class NullableSerializer<T> : ContentTypeSerializer<T?> where T : struct
{
    private ContentTypeSerializer _serializer = null!;
    private ContentSerializerAttribute _format = null!;

    protected internal override void Initialize(IntermediateSerializer serializer)
    {
        _serializer = serializer.GetTypeSerializer(typeof(T));
        _format = new ContentSerializerAttribute
        {
            FlattenContent = true
        };
    }

    protected internal override T? Deserialize(IntermediateReader input, ContentSerializerAttribute format, T? existingInstance) => input.ReadRawObject<T>(_format, _serializer);

    protected internal override void Serialize(IntermediateWriter output, T? value, ContentSerializerAttribute format) => output.WriteRawObject(value, _format, _serializer);
}
