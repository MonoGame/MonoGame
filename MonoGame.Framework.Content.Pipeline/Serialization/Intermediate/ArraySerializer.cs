// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

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

        public override bool ObjectIsEmpty(T[] value)
        {
            return value.Length == 0;
        }

        protected internal override void ScanChildren(IntermediateSerializer serializer, ChildCallback callback, T[] value)
        {
            _listSerializer.ScanChildren(serializer, callback, new List<T>(value));
        }

        protected internal override T[] Deserialize(IntermediateReader input, ContentSerializerAttribute format, T[] existingInstance)
        {
            if (existingInstance != null)
                throw new InvalidOperationException("You cannot deserialize an array into a getter-only property.");
            var result = _listSerializer.Deserialize(input, format, null);
            return result.ToArray();
        }

        protected internal override void Serialize(IntermediateWriter output, T[] value, ContentSerializerAttribute format)
        {
            _listSerializer.Serialize(output, new List<T>(value), format);
        }
    }
}