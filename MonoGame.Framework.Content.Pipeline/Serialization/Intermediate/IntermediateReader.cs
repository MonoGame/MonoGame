// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    public sealed class IntermediateReader
    {
        private readonly string _filePath;

        private readonly Dictionary<string, Action<object>> _resourceFixups;

        private readonly Dictionary<string, Action<Type, string>> _externalReferences;

        public XmlReader Xml { get; private set; }

        public IntermediateSerializer Serializer { get; private set; }

        internal IntermediateReader(IntermediateSerializer serializer, XmlReader xmlReader, string filePath)
        {
            Serializer = serializer;
            Xml = xmlReader;
            _filePath = filePath;
            _resourceFixups = new Dictionary<string, Action<object>>();
            _externalReferences = new Dictionary<string, Action<Type, string>>();
        }

        public bool MoveToElement(string elementName)
        {
            var nodeType = Xml.MoveToContent();
            return  nodeType == XmlNodeType.Element && 
                    Xml.Name == elementName;
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
            if (!format.FlattenContent)
            {
                if (!MoveToElement(format.ElementName))
                    throw NewInvalidContentException(null, "Element '{0}' was not found.", format.ElementName);

                // Is the object null?
                var isNull = Xml.GetAttribute("Null");
                if (isNull != null && XmlConvert.ToBoolean(isNull))
                {
                    if (!format.AllowNull)
                        throw NewInvalidContentException(null, "Element '{0}' cannot be null.", format.ElementName);

                    Xml.Skip();
                    return default(T);
                }

                // Is the object overloading the serialized type?
                if (Xml.MoveToAttribute("Type"))
                {
                    var type = ReadTypeName();
                    if (type == null)
                        throw NewInvalidContentException(null, "Could not resolve type '{0}'.", Xml.ReadContentAsString());
                    if (!typeSerializer.TargetType.IsAssignableFrom(type))
                        throw NewInvalidContentException(null, "Type '{0}' is not assignable to '{1}'.", type.FullName, typeSerializer.TargetType.FullName);

                    typeSerializer = Serializer.GetTypeSerializer(type);
                    Xml.MoveToElement();
                }
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
                throw NewInvalidContentException(null, "Element '{0}' was not found.", format.ElementName);

            var isEmpty = Xml.IsEmptyElement;
            if (!isEmpty)
                Xml.ReadStartElement();

            var result = typeSerializer.Deserialize(this, format, existingInstance);

            if (!isEmpty)
                Xml.ReadEndElement();

            return (T)result;
        }

        public T ReadRawObject<T>(ContentSerializerAttribute format, T existingInstance)
        {
            return ReadRawObject(format, Serializer.GetTypeSerializer(typeof(T)), existingInstance);           
        }

        public void ReadSharedResource<T>(ContentSerializerAttribute format, Action<T> fixup)
        {
            string str;

            if (format.FlattenContent)
                str = Xml.ReadContentAsString();
            else
            {
                if (!MoveToElement(format.ElementName))
                    throw NewInvalidContentException(null, "Element '{0}' was not found.", format.ElementName);

                str = Xml.ReadElementContentAsString();
            }
            
            // Do we already have one for this?
            Action<object> prevFixup;
            if (!_resourceFixups.TryGetValue(str, out prevFixup))
                _resourceFixups.Add(str, (o) => fixup((T)o));
            else
            {
                _resourceFixups[str] = (o) =>
                {
                    prevFixup(o);
                    fixup((T)o);
                };
            }
        }

        internal void ReadSharedResources()
        {
            if (!MoveToElement("Resources"))
                return;

            var resources = new Dictionary<string, object>();
            var resourceFormat = new ContentSerializerAttribute { ElementName = "Resource" };

            // Read all the resources.
            Xml.ReadStartElement();
            while (MoveToElement("Resource"))
            {
                var id = Xml.GetAttribute("ID");
                var resource = ReadObject<object>(resourceFormat);
                resources.Add(id, resource);
            }
            Xml.ReadEndElement();

            // Execute the fixups.
            foreach (var fixup in _resourceFixups)
            {
                var resouce = resources[fixup.Key];
                fixup.Value(resouce);
            }
        }

        public void ReadExternalReference<T>(ExternalReference<T> existingInstance)
        {
            if (!MoveToElement("Reference"))
                return;

            var str = Xml.ReadElementContentAsString();

            Action<Type, string> fixup = (type, filename) =>
            {
                if (type != typeof(T))
                    throw NewInvalidContentException(null, "Invalid external reference type");

                existingInstance.Filename = filename;
            };
            _externalReferences.Add(str, fixup);
        }

        internal void ReadExternalReferences()
        {
            if (!MoveToElement("ExternalReferences"))
                return;

            var currentDir = Path.GetDirectoryName(_filePath);

            // Read all the external references.
            Xml.ReadStartElement();
            while (MoveToElement("ExternalReference"))
            {
                Action<Type, string> fixup;
                var id = Xml.GetAttribute("ID");
                if (!_externalReferences.TryGetValue(id, out fixup))
                    throw NewInvalidContentException(null, "Unknown external reference id '{0}'!", id);

                Xml.MoveToAttribute("TargetType");
                var targetType = ReadTypeName();
                if (targetType == null)
                    throw NewInvalidContentException(null, "Could not resolve type '{0}'.", Xml.ReadContentAsString());

                Xml.MoveToElement();
                var filename = Xml.ReadElementString();
                filename = Path.Combine(currentDir, filename);

                // Apply the fixups.
                fixup(targetType, filename);
            }
            Xml.ReadEndElement();
        }

        internal InvalidContentException NewInvalidContentException(Exception innerException, string message, params object[] args)
        {
            var xmlInfo = (IXmlLineInfo)Xml;
            var lineAndColumn = string.Format("{0},{1}", xmlInfo.LineNumber, xmlInfo.LinePosition);
            var identity = new ContentIdentity(_filePath, string.Empty, lineAndColumn);
            return new InvalidContentException(string.Format(message, args), identity, innerException);
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