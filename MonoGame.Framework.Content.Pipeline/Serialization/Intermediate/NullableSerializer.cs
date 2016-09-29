using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class NullableSerializer<T> : ContentTypeSerializer<T?> where T : struct
    {
        private ContentTypeSerializer _serializer;
        private ContentSerializerAttribute _format;

        protected internal override void Initialize(IntermediateSerializer serializer)
        {
            _serializer = serializer.GetTypeSerializer(typeof(T));
            _format = new ContentSerializerAttribute
            {
                FlattenContent = true
            };
        }

        protected internal override T? Deserialize(IntermediateReader input, ContentSerializerAttribute format, T? existingInstance)
        {
            return input.ReadRawObject<T>(_format, _serializer);
        }

        protected internal override void Serialize(IntermediateWriter output, T? value, ContentSerializerAttribute format)
        {
            output.WriteRawObject<T>(value.Value, _format, _serializer);
        }
    }
}
