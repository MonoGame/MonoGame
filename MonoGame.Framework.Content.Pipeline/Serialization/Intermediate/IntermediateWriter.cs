// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    public sealed class IntermediateWriter
    {
        private readonly Stack<object> _currentObjectStack;
        private readonly Dictionary<object, string> _sharedResources;
        private readonly Dictionary<object, ExternalReference> _externalReferences;
        private readonly string _filePath;

        internal IntermediateWriter(IntermediateSerializer serializer, XmlWriter xmlWriter, string filePath)
        {
            Serializer = serializer;
            Xml = xmlWriter;
            _filePath = filePath;

            _currentObjectStack = new Stack<object>();
            _sharedResources = new Dictionary<object, string>();
            _externalReferences = new Dictionary<object, ExternalReference>();
        }

        public XmlWriter Xml { get; private set; }

        public IntermediateSerializer Serializer { get; private set; }

        public void WriteExternalReference<T>(ExternalReference<T> value)
        {
            ExternalReference externalReference;
            if (!_externalReferences.TryGetValue(value, out externalReference))
                _externalReferences.Add(value, externalReference = new ExternalReference
                {
                    ID = "#External" + (_externalReferences.Count + 1),
                    TargetType = typeof(T).FullName,
                    FileName = MakeRelativePath(value.Filename)
                });

            Xml.WriteElementString("Reference", externalReference.ID);
        }

        private string MakeRelativePath(string path)
        {
            var fullReferencePath = Path.GetFullPath(Path.GetDirectoryName(_filePath)) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(path);
            return new Uri(fullReferencePath).MakeRelativeUri(new Uri(fullPath)).ToString();
        }

        private class ExternalReference
        {
            public string ID;
            public string TargetType;
            public string FileName;
        }

        public void WriteObject<T>(T value, ContentSerializerAttribute format)
        {
            WriteObject(value, format, Serializer.GetTypeSerializer(typeof(T)));
        }

        public void WriteObject<T>(T value, ContentSerializerAttribute format, ContentTypeSerializer typeSerializer)
        {
            WriteObjectInternal(value, format, typeSerializer, typeof(T));
        }

        internal void WriteObjectInternal(object value, ContentSerializerAttribute format, ContentTypeSerializer typeSerializer, Type declaredType)
        {
            if (format.Optional && (value == null || typeSerializer.ObjectIsEmpty(value)))
                return;

            var isReferenceObject = false;
            if (value != null && !typeSerializer.TargetType.IsValueType)
            {
                if (_currentObjectStack.Contains(value))
                    throw new InvalidOperationException("Cyclic reference found during serialization. You may be missing a [ContentSerializer(SharedResource=true)] attribute.");
                _currentObjectStack.Push(value);
                isReferenceObject = true;
            }

            if (!format.FlattenContent)
            {
                Xml.WriteStartElement(format.ElementName);

                if (value == null)
                {
                    if (!format.AllowNull)
                        throw new InvalidOperationException(string.Format("Element {0} cannot be null.", format.ElementName));

                    Xml.WriteAttributeString("Null", "true");
                }
                else if (value.GetType() != typeSerializer.TargetType && !IsNullableType(declaredType))
                {
                    Xml.WriteStartAttribute("Type");
                    WriteTypeName(value.GetType());
                    Xml.WriteEndAttribute();

                    typeSerializer = Serializer.GetTypeSerializer(value.GetType());
                }
            }

            if (value != null && !typeSerializer.ObjectIsEmpty(value))
                typeSerializer.Serialize(this, value, format);

            if (!format.FlattenContent)
                Xml.WriteEndElement();

            if (isReferenceObject)
                _currentObjectStack.Pop();
        }

        private static bool IsNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public void WriteRawObject<T>(T value, ContentSerializerAttribute format)
        {
            WriteRawObject(value, format, Serializer.GetTypeSerializer(typeof(T)));
        }

        public void WriteRawObject<T>(T value, ContentSerializerAttribute format, ContentTypeSerializer typeSerializer)
        {
            if (!format.FlattenContent)
                Xml.WriteStartElement(format.ElementName);

            typeSerializer.Serialize(this, value, format);

            if (!format.FlattenContent)
                Xml.WriteEndElement();
        }

        public void WriteSharedResource<T>(T value, ContentSerializerAttribute format)
        {
            var sharedResourceID = GetSharedResourceID(value);

            if (format.FlattenContent)
                Xml.WriteValue(sharedResourceID);
            else
                Xml.WriteElementString(format.ElementName, sharedResourceID);
        }

        private string GetSharedResourceID(object value)
        {
            if (value == null)
                return null;

            string id;
            if (!_sharedResources.TryGetValue(value, out id))
                _sharedResources.Add(value, id = "#Resource" + (_sharedResources.Count + 1));
            return id;
        }

        internal void WriteSharedResources()
        {
            if (!_sharedResources.Any())
                return;

            Xml.WriteStartElement("Resources");

            // Loop like this because we might create more shared resources while we're serializing.
            var writtenSharedResources = new List<string>();
            while (_sharedResources.Any(x => !writtenSharedResources.Contains(x.Value)))
            {
                var sharedResource = _sharedResources.First(x => !writtenSharedResources.Contains(x.Value));
                writtenSharedResources.Add(sharedResource.Value);

                WriteSharedResource(sharedResource.Value, sharedResource.Key);
            }

            Xml.WriteEndElement();
        }

        private void WriteSharedResource(string id, object sharedResource)
        {
            Xml.WriteStartElement("Resource");

            Xml.WriteAttributeString("ID", id);

            Xml.WriteStartAttribute("Type");
            WriteTypeName(sharedResource.GetType());
            Xml.WriteEndAttribute();

            Serializer.GetTypeSerializer(sharedResource.GetType()).Serialize(this, sharedResource, new ContentSerializerAttribute());

            Xml.WriteEndElement();
        }

        internal void WriteExternalReferences()
        {
            if (!_externalReferences.Any())
                return;

            Xml.WriteStartElement("ExternalReferences");

            foreach (var externalReference in _externalReferences.Values)
            {
                Xml.WriteStartElement("ExternalReference");
                
                Xml.WriteAttributeString("ID", externalReference.ID);
                Xml.WriteAttributeString("TargetType", externalReference.TargetType);

                Xml.WriteValue(externalReference.FileName);

                Xml.WriteEndElement();
            }

            Xml.WriteEndElement();
        }

        public void WriteTypeName(Type type)
        {
            Xml.WriteString(Serializer.GetFullTypeName(type));
        }
    }        
}