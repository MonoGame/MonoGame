// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    public sealed class IntermediateReader
    {
        private readonly string _filePath;

        public XmlReader Xml { get; private set; }

        public IntermediateSerializer Serializer { get; private set; }

        internal IntermediateReader(IntermediateSerializer serializer, XmlReader xmlReader, string filePath)
        {
            Serializer = serializer;
            Xml = xmlReader;
            _filePath = filePath;
        }

        public bool MoveToElement(string elementName)
        {
            var nodeType = Xml.MoveToContent();
            return  nodeType == XmlNodeType.Element && 
                    Xml.Name == elementName;
        }
 
        public void ReadExternalReference<T>(ExternalReference<T> existingInstance)
        {
            throw new NotImplementedException();
        }
            
        public T ReadObject<T>(ContentSerializerAttribute format)
        {
            return ReadObject(format, Serializer.GetTypeSerializer(typeof(T)), default(T));
        }

        public T ReadObject<T>(ContentSerializerAttribute format, ContentTypeSerializer typeSerializer)
        {
            return ReadObject(format, typeSerializer, default(T));
        }

        public T ReadObject<T>(ContentSerializerAttribute format, ContentTypeSerializer typeSerializer, T existingInstance)
        {
            if (!MoveToElement(format.ElementName))
                throw new InvalidContentException(string.Format("Element `{0}` was not found in `{1}`.", format.ElementName, _filePath));

            // Is the object overloading the serialized type?
            if (Xml.MoveToAttribute("Type"))
            {
                var type = ReadTypeName();
                typeSerializer = Serializer.GetTypeSerializer(type);
                Xml.MoveToElement();
            }

            return ReadRawObject(format, typeSerializer, existingInstance);
        }

        public T ReadObject<T>(ContentSerializerAttribute format, T existingInstance)
        {
            return ReadObject(format, Serializer.GetTypeSerializer(typeof(T)), existingInstance);            
        }

        public T ReadRawObject<T>(ContentSerializerAttribute format)
        {
            return ReadRawObject(format, Serializer.GetTypeSerializer(typeof(T)), default(T));         
        }

        public T ReadRawObject<T>(ContentSerializerAttribute format, ContentTypeSerializer typeSerializer)
        {
            return ReadRawObject(format, typeSerializer, default(T));         
        }

        public T ReadRawObject<T>(ContentSerializerAttribute format, ContentTypeSerializer typeSerializer, T existingInstance)
        {
            if (format.FlattenContent)
            {
                Xml.MoveToContent();
                return (T)typeSerializer.Deserialize(this, format, existingInstance);
            }

            if (!MoveToElement(format.ElementName))
                throw new InvalidContentException(string.Format("Element `{0}` was not found in `{1}`.", format.ElementName, _filePath));

            Xml.ReadStartElement();
            var result = typeSerializer.Deserialize(this, format, existingInstance);
            Xml.ReadEndElement();
            return (T)result;
        }

        public T ReadRawObject<T>(ContentSerializerAttribute format, T existingInstance)
        {
            return ReadRawObject(format, Serializer.GetTypeSerializer(typeof(T)), existingInstance);           
        }

        public void ReadSharedResource<T>(ContentSerializerAttribute format, Action<T> fixup)
        {
            throw new NotImplementedException();            
        }

        /// <summary>
        /// Reads the next type in the 
        /// </summary>
        /// <returns></returns>
        public Type ReadTypeName()
        {
            var typeName = Xml.ReadContentAsString();
            return Serializer.FindType(typeName);
        }
    }
}