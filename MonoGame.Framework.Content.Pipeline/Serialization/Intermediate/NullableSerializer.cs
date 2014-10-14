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
        private ElementSerializer<T> _serializer;

        protected internal override void Initialize(IntermediateSerializer serializer)
        {
            _serializer = (ElementSerializer<T>)serializer.GetTypeSerializer(typeof(T));
        }

        protected internal override T? Deserialize(IntermediateReader input, ContentSerializerAttribute format, T? existingInstance)
        {
            var list = new List<T>();
            _serializer.Deserialize(input, list);
            return list.First();            
        }

        protected internal override void Serialize(IntermediateWriter output, T? value, ContentSerializerAttribute format)
        {
            throw new NotImplementedException();
        }
    }
}
