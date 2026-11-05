// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    /// <summary>
    /// IntermediateReader is used to read content from the intermediate format.
    /// </summary>
    public sealed class IntermediateReader
    {
        private readonly string _filePath;

        private readonly Dictionary<string, Action<object?>> _resourceFixups;

        private readonly Dictionary<string, List<Action<Type, string>>> _externalReferences;

        /// <summary>
        /// Gets the instances XML reader.
        /// </summary>
        public XmlReader Xml { get; private set; }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        public IntermediateSerializer Serializer { get; private set; }

        internal IntermediateReader(IntermediateSerializer serializer, XmlReader xmlReader, string filePath)
        {
            Serializer = serializer;
            Xml = xmlReader;
            _filePath = filePath;
            _resourceFixups = [];
            _externalReferences = [];
        }

        /// <summary>
        /// Moves the XML reader to the specified element.
        /// </summary>
        /// <param name="elementName">The name of the element to move to.</param>
        /// <returns><c>true</c> if the element is found, <c>false</c> otherwise.</returns>
        public bool MoveToElement(string elementName) => Xml.MoveToContent() == XmlNodeType.Element && Xml.Name == elementName;

        /// <summary>
        /// Reads an object from the XML.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="format">The format attribute to use.</param>
        /// <returns>The deserialized object of type T.</returns>
        public T ReadObject<T>(ContentSerializerAttribute format) => ReadObject(format, Serializer.GetTypeSerializer(typeof(T)), default(T));

        /// <summary>
        /// Reads an object from the XML.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="format">The format attribute to use.</param>
        /// <param name="typeSerializer">The type serializer to use.</param>
        /// <returns>The deserialized object of type T.</returns>
        public T ReadObject<T>(ContentSerializerAttribute format, ContentTypeSerializer typeSerializer) => ReadObject(format, typeSerializer, default(T));

        /// <summary>
        /// Reads an object from the XML.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="format">The format attribute of the object.</param>
        /// <param name="typeSerializer">The type serializer for the object.</param>
        /// <param name="existingInstance">An existing instance of the object.</param>
        /// <returns>The deserialized object of type T.</returns>
        /// <exception cref="InvalidContentException">
        /// Thrown when the element can not be found, is null or cannot be assigned.
        /// </exception>
        public T ReadObject<T>(ContentSerializerAttribute format, ContentTypeSerializer typeSerializer, T? existingInstance)
        {
            if (!format.FlattenContent)
            {
                if (!MoveToElement(format.ElementName))
                    throw NewInvalidContentException(null, $"Element '{format.ElementName}' was not found.");

                // Is the object null?
                var isNull = Xml.GetAttribute("Null");
                if (isNull != null && XmlConvert.ToBoolean(isNull))
                {
                    if (!format.AllowNull)
                        throw NewInvalidContentException(null, $"Element '{format.ElementName}' cannot be null.");

                    Xml.Skip();
                    return default!;
                }

                // Is the object overloading the serialized type?
                if (Xml.MoveToAttribute("Type"))
                {
                    var type = ReadTypeName() ?? throw NewInvalidContentException(null, $"Could not resolve type '{Xml.ReadContentAsString()}'.");
                    if (!typeSerializer.TargetType.IsAssignableFrom(type))
                        throw NewInvalidContentException(null, $"Type '{type.FullName}' is not assignable to '{typeSerializer.TargetType.FullName}'.");

                    typeSerializer = Serializer.GetTypeSerializer(type);
                    Xml.MoveToElement();
                }
            }

            return ReadRawObject(format, typeSerializer, existingInstance);
        }


        /// <summary>
        /// Reads an object from the XML.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="format">The format attribute of the object.</param>
        /// <param name="existingInstance">An existing instance of the object.</param>
        /// <returns>The deserialized object of type T.</returns>
        public T ReadObject<T>(ContentSerializerAttribute format, T existingInstance) => ReadObject(format, Serializer.GetTypeSerializer(typeof(T)), existingInstance);

        /// <summary>
        /// Reads a raw object from the XML using the specified format and type serializer.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="format">The format attribute of the object.</param>
        /// <returns>The deserialized object of type T.</returns>
        /// <exception cref="InvalidContentException">
        /// Thrown when the element can not be found or is null.
        /// </exception>
        public T ReadRawObject<T>(ContentSerializerAttribute format) => ReadRawObject(format, Serializer.GetTypeSerializer(typeof(T)), default(T));

        /// <summary>
        /// Reads an object from the XML.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="format">The format attribute of the object.</param>
        /// <param name="typeSerializer">The type serializer for the object.</param>
        /// <returns>The deserialized object of type T.</returns>
        public T ReadRawObject<T>(ContentSerializerAttribute format, ContentTypeSerializer typeSerializer) => ReadRawObject(format, typeSerializer, default(T));

        /// <summary>
        /// Reads a raw object from the XML using the specified format and type serializer.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="format">The format attribute of the object.</param>
        /// <param name="typeSerializer">The type serializer for the object.</param>
        /// <param name="existingInstance">An existing instance of the object.</param>
        /// <returns>The deserialized object of type T.</returns>
        /// <exception cref="InvalidContentException">
        /// Thrown when the element can not be found or is null.
        /// </exception>
        public T ReadRawObject<T>(ContentSerializerAttribute format, ContentTypeSerializer typeSerializer, T? existingInstance)
        {
            if (format.FlattenContent)
            {
                Xml.MoveToContent();
                return (T)typeSerializer.Deserialize(this, format, existingInstance)!;
            }

            if (!MoveToElement(format.ElementName))
                throw NewInvalidContentException(null, $"Element '{format.ElementName}' was not found.");

            var isEmpty = Xml.IsEmptyElement;
            if (!isEmpty)
                Xml.ReadStartElement();

            var result = typeSerializer.Deserialize(this, format, existingInstance);

            if (isEmpty)
                Xml.Skip();

            if (!isEmpty)
                Xml.ReadEndElement();

            return (T)result!;
        }

        /// <summary>
        /// Reads a raw object from the XML using the specified format and type serializer.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="format">The format attribute of the object.</param>
        /// <param name="existingInstance">An existing instance of the object.</param>
        /// <returns>The deserialized object of type T.</returns>
        /// <exception cref="InvalidContentException">
        /// Thrown when the element can not be found or is null.
        /// </exception>
        public T ReadRawObject<T>(ContentSerializerAttribute format, T existingInstance)
            => ReadRawObject(format, Serializer.GetTypeSerializer(typeof(T)), existingInstance);

        /// <summary>
        /// Reads a shared resource from the XML using the specified format and fixup action.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <param name="format">The format attribute of the resource.</param>
        /// <param name="fixup">The fixup action to apply to the resource.</param>
        /// <exception cref="InvalidContentException">
        /// Thrown if the element specified by the format attribute is not found.
        /// </exception>
        public void ReadSharedResource<T>(ContentSerializerAttribute format, Action<T?> fixup)
        {
            string str;

            if (format.FlattenContent)
                str = Xml.ReadContentAsString();
            else
            {
                if (!MoveToElement(format.ElementName))
                    throw NewInvalidContentException(null, $"Element '{format.ElementName}' was not found.");

                str = Xml.ReadElementContentAsString();
            }

            if (string.IsNullOrEmpty(str))
                return;

            // Do we already have one for this?
            if (!_resourceFixups.TryGetValue(str, out var prevFixup))
                _resourceFixups.Add(str, (o) => fixup((T?)o));
            else
            {
                _resourceFixups[str] = (o) =>
                {
                    prevFixup(o);
                    fixup((T?)o);
                };
            }
        }

        internal void ReadSharedResources()
        {
            if (!MoveToElement("Resources"))
                return;

            var resources = new Dictionary<string, object?>();
            var resourceFormat = new ContentSerializerAttribute { ElementName = "Resource" };

            // Read all the resources.
            Xml.ReadStartElement();
            while (MoveToElement("Resource"))
            {
                var id = Xml.GetAttribute("ID") ?? "";
                var resource = ReadObject<object>(resourceFormat);
                resources.Add(id, resource);
            }
            Xml.ReadEndElement();

            // Execute the fixups.
            foreach (var fixup in _resourceFixups)
            {
                if (!resources.TryGetValue(fixup.Key, out var resource))
                    throw new InvalidContentException("Missing shared resource \"" + fixup.Key + "\".");
                fixup.Value(resource);
            }
        }

        /// <summary>
        /// Reads an external reference of a given type.
        /// </summary>
        /// <param name="existingInstance">The existing instance of the ExternalReference.</param>
        /// <typeparam name="T">The type of the external reference.</typeparam>
        /// <exception cref="InvalidContentException">
        /// Thrown if the external reference type is invalid.
        /// </exception>
        public void ReadExternalReference<T>(ExternalReference<T> existingInstance)
        {
            if (!MoveToElement("Reference"))
                return;

            var str = Xml.ReadElementContentAsString();

            if (!_externalReferences.TryGetValue(str, out var fixups))
                _externalReferences.Add(str, fixups = []);
            fixups.Add(Fixup);
            return;

            void Fixup(Type type, string filename)
            {
                if (type != typeof(T))
                    throw NewInvalidContentException(null, "Invalid external reference type");

                existingInstance.Filename = filename;
            }
        }

        internal void ReadExternalReferences()
        {
            if (!MoveToElement("ExternalReferences"))
                return;

            var currentDir = Path.GetDirectoryName(_filePath) ?? "";

            // Read all the external references.
            Xml.ReadStartElement();
            while (MoveToElement("ExternalReference"))
            {
                var id = Xml.GetAttribute("ID") ?? "";
                if (!_externalReferences.TryGetValue(id, out var fixups))
                    throw NewInvalidContentException(null, $"Unknown external reference id '{id}'!");

                Xml.MoveToAttribute("TargetType");
                var targetType = ReadTypeName() ?? throw NewInvalidContentException(null, $"Could not resolve type '{Xml.ReadContentAsString()}'.");
                Xml.MoveToElement();
                var filename = Xml.ReadElementString();
                filename = Path.Combine(currentDir, filename);

                // Apply the fixups.
                foreach (var fixup in fixups)
                    fixup(targetType, filename);
            }
            Xml.ReadEndElement();
        }

        internal InvalidContentException NewInvalidContentException(Exception? innerException, string message)
        {
            var xmlInfo = (IXmlLineInfo)Xml;
            var lineAndColumn = $"{xmlInfo.LineNumber},{xmlInfo.LinePosition}";
            var identity = new ContentIdentity(_filePath, string.Empty, lineAndColumn);
            return new InvalidContentException(message, identity, innerException);
        }

        /// <summary>
        /// Reads the next type in the
        /// </summary>
        /// <returns></returns>
        public Type ReadTypeName() => Serializer.FindType(Xml.ReadContentAsString());
    }
}
