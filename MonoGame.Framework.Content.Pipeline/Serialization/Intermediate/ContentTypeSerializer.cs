// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    public abstract class ContentTypeSerializer
    {
        protected ContentTypeSerializer(Type targetType, string xmlTypeName)
        {
            TargetType = targetType;
            XmlTypeName = xmlTypeName;
        }

        public virtual bool CanDeserializeIntoExistingObject
        {
            get { return false; }
        }

        public Type TargetType { get; private set; }

        public string XmlTypeName { get; private set; }

        protected internal abstract object Deserialize(IntermediateReader input, ContentSerializerAttribute format, object existingInstance);

        protected internal virtual void Initialize(IntermediateSerializer serializer)
        {     
        }

        public virtual bool ObjectIsEmpty(object value)
        {
            return false;
        }

        protected internal virtual void ScanChildren(IntermediateSerializer serializer, ChildCallback callback, object value)
        {
        }

        protected internal abstract void Serialize(IntermediateWriter output, object value, ContentSerializerAttribute format);

        internal protected delegate void ChildCallback(ContentTypeSerializer typeSerializer, object value);
    }
}