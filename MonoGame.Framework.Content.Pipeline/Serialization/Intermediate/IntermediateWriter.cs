// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    public sealed class IntermediateWriter
    {
        private string _filePath;

        internal IntermediateWriter(IntermediateSerializer serializer, XmlWriter xmlWriter, string filePath)
        {
            Serializer = serializer;
            Xml = xmlWriter;
            _filePath = filePath;
        }

        public XmlWriter Xml { get; private set; }

        public IntermediateSerializer Serializer { get; private set; }

        public void WriteExternalReference<T>(ExternalReference<T> value)
        {
            throw new NotImplementedException();
        }

        public void WriteObject<T>(T value, ContentSerializerAttribute format)
        {
            throw new NotImplementedException();
        }

        public void WriteObject<T>(T value, ContentSerializerAttribute format, ContentTypeSerializer typeSerializer)
        {
            throw new NotImplementedException();
        }

        public void WriteRawObject<T>(T value, ContentSerializerAttribute format)
        {
            throw new NotImplementedException();
        }

        public void WriteRawObject<T>(T value, ContentSerializerAttribute format, ContentTypeSerializer typeSerializer)
        {
            throw new NotImplementedException();
        }

        public void WriteSharedResource<T>(T value, ContentSerializerAttribute format)
        {
            throw new NotImplementedException();       
        }

        public void WriteTypeName(Type type)
        {
            throw new NotImplementedException();
        }
    }        
}