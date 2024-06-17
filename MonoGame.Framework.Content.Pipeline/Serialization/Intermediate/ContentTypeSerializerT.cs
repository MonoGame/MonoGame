// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    public abstract class ContentTypeSerializer<T> : ContentTypeSerializer
    {
        protected ContentTypeSerializer() :
            this(string.Empty)
        {
        }

        protected ContentTypeSerializer(string xmlTypeName) :
            base(typeof(T), xmlTypeName)
        {
        }

        protected internal abstract T Deserialize(IntermediateReader input, ContentSerializerAttribute format, T existingInstance);

        protected internal override object Deserialize(IntermediateReader input, ContentSerializerAttribute format, object existingInstance)
        {
            var cast = existingInstance == null ? default(T) : (T)existingInstance;
            return Deserialize(input, format, cast);
        }

        public virtual bool ObjectIsEmpty(T value)
        {
            return base.ObjectIsEmpty(value);
        }
        
        public override bool ObjectIsEmpty(object value)
        {
            var cast = value == null ? default(T) : (T)value;
            return ObjectIsEmpty(cast);
        }

        protected internal virtual void ScanChildren(IntermediateSerializer serializer, ChildCallback callback, T value)
        {
        }

        protected internal override void ScanChildren(IntermediateSerializer serializer, ChildCallback callback, object value)
        {
            if (value == null)
                return;
            ScanChildren(serializer, callback, (T)value);
        }

        protected internal abstract void Serialize(IntermediateWriter output, T value, ContentSerializerAttribute format);

        protected internal override void Serialize(IntermediateWriter output, object value, ContentSerializerAttribute format)
        {
            var cast = value == null ? default(T) : (T)value;
            Serialize(output, cast, format);
        }
    }
}