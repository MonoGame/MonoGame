// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    class ArraySerializer<T> : ContentTypeSerializer<T[]>
    {
        private readonly ListSerializer<T> _listSerializer;

        public ArraySerializer() :
            base("array")
        {
            _listSerializer = new ListSerializer<T>();
        }

        protected internal override void Initialize(IntermediateSerializer serializer)
        {
            _listSerializer.Initialize(serializer);
        }

        protected internal override T[] Deserialize(IntermediateReader input, ContentSerializerAttribute format, T[] existingInstance)
        {
            var result = _listSerializer.Deserialize(input, format, null);
            return result.ToArray();
        }

        protected internal override void Serialize(IntermediateWriter output, T[] value, ContentSerializerAttribute format)
        {
            throw new NotImplementedException();
        }
    }
}