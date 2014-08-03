// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    // The intermediate serializer implementation is based on testing XNA behavior and the following sources:
    //
    // http://msdn.microsoft.com/en-us/library/Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate.aspx
    // http://blogs.msdn.com/b/shawnhar/archive/2008/08/12/everything-you-ever-wanted-to-know-about-intermediateserializer.aspx
    // http://blogs.msdn.com/b/shawnhar/archive/2008/08/26/customizing-intermediateserializer-part-1.aspx
    // http://blogs.msdn.com/b/shawnhar/archive/2008/08/26/customizing-intermediateserializer-part-2.aspx
    // http://blogs.msdn.com/b/shawnhar/archive/2008/08/27/why-intermediateserializer-control-attributes-are-not-part-of-the-content-pipeline.aspx
    //

    
    public class IntermediateSerializer
    {
        /// <summary>
        /// According to the examples on Sean Hargreaves' blog, explicit types
        /// can also specify the type aliases from C#. This maps those names
        /// to the actual .NET framework types for parsing.
        /// </summary>
        private static readonly Dictionary<string, Type> _typeAliases = new Dictionary<string, Type>
        {
            { "bool",   typeof(bool) },
            { "byte",   typeof(byte) },
            { "sbyte",  typeof(sbyte) },
            { "char",   typeof(char) },
            { "decimal",typeof(decimal) },
            { "double", typeof(double) },
            { "float",  typeof(float) },
            { "int",    typeof(int) },
            { "uint",   typeof(uint) },
            { "long",   typeof(long) },
            { "ulong",  typeof(ulong) },
            { "object", typeof(object) },
            { "short",  typeof(short) },
            { "ushort", typeof(ushort) },
            { "string", typeof(string) }
        };

        /// <summary>
        /// Maps "ShortName:" -> "My.Namespace.LongName." for type lookups.
        /// </summary>
        private Dictionary<string, string> _namespaceLookup;

        private Dictionary<Type, ContentTypeSerializer> _serializers;

        private Dictionary<Type, Type> _genericSerializerTypes;


        public static T Deserialize<T>(XmlReader input, string referenceRelocationPath)
        {
            var serializer = new IntermediateSerializer();
            var reader = new IntermediateReader(serializer, input, referenceRelocationPath);
            var asset = default(T);

            try
            {
                if (!reader.MoveToElement("XnaContent"))
                    throw new InvalidContentException(string.Format("Could not find XnaContent element in '{0}'.",
                                                                    referenceRelocationPath));

                // Initialize the namespace lookups from
                // the attributes on the XnaContent element.
                serializer.CreateNamespaceLookup(input);

                // Move past the XnaContent.
                input.ReadStartElement();

                // Read the asset.
                var format = new ContentSerializerAttribute {ElementName = "Asset"};
                asset = reader.ReadObject<T>(format);

                // Process the shared resources and external references.
                reader.ReadSharedResources();
                reader.ReadExternalReferences();

                // Move past the closing XnaContent element.
                input.ReadEndElement();
            }
            catch (XmlException xmlException)
            {
                throw reader.NewInvalidContentException(xmlException, "An error occured parsing.");
            }

            return asset;
        }

        public ContentTypeSerializer GetTypeSerializer(Type type)
        {
            // Create the known serializers if we haven't already.
            if (_serializers == null)
            {
                _serializers = new Dictionary<Type, ContentTypeSerializer>();
                _genericSerializerTypes = new Dictionary<Type, Type>();

                var types = ContentTypeSerializerAttribute.GetTypes();
                foreach (var t in types)
                {
                    if (t.IsGenericType)
                    {
                        var genericType = t.BaseType.GetGenericArguments()[0];
                        _genericSerializerTypes.Add(genericType.GetGenericTypeDefinition(), t);
                    }
                    else
                    {
                        var cts = Activator.CreateInstance(t) as ContentTypeSerializer;
                        cts.Initialize(this);
                        _serializers.Add(cts.TargetType, cts);
                    }
                }
            }

            // Look it up.
            ContentTypeSerializer serializer;
            if (_serializers.TryGetValue(type, out serializer))
                return serializer;

            Type serializerType;

            if (type.IsArray)
            {
                if (type.GetArrayRank() != 1)
                    throw new RankException("We only support single dimension arrays.");

                var arrayType = typeof(ArraySerializer<>).MakeGenericType(new[] { type.GetElementType() });
                serializer = (ContentTypeSerializer)Activator.CreateInstance(arrayType);
            }
            else if (type.IsGenericType && _genericSerializerTypes.TryGetValue(type.GetGenericTypeDefinition(), out serializerType))
            {
                serializerType = serializerType.MakeGenericType(type.GetGenericArguments());
                serializer = (ContentTypeSerializer)Activator.CreateInstance(serializerType);
            }
            else if (type.IsEnum)
            {
                serializer = new EnumSerializer(type);
            }
            else
            {
                // The reflective serializer is not for primitive types!
                if (type.IsPrimitive)
                    throw new NotImplementedException(string.Format("Unhandled primitive type `{0}`!", type.FullName));

                // We still don't have a serializer then we 
                // fallback to the reflection based serializer.
                serializer = new ReflectiveSerializer(type);
            }

            Debug.Assert(serializer.TargetType == type, "Target type mismatch!");

            // We cache the serializer before we initialize it to 
            // avoid a stack overflow on recursive types.
            _serializers.Add(type, serializer);
            serializer.Initialize(this);

            return serializer;
        }

        public static void Serialize<T>(XmlWriter output, T value, string referenceRelocationPath)
        {
            var serializer = new IntermediateSerializer();
            var writer = new IntermediateWriter(serializer, output, referenceRelocationPath);
            output.WriteStartElement("XnaContent");
            
            // TODO: Write namespaces?

            // Write the asset.
            var format = new ContentSerializerAttribute { ElementName = "Asset" };
            writer.WriteObject(value, format);

            // TODO: Write the shared resources and external 
            // references here!

            // Close the XnaContent element.
            output.WriteEndElement();
        }

        /// <summary>
        /// Builds a lookup table from a short name to the full namespace.
        /// </summary>
        private void CreateNamespaceLookup(XmlReader reader)
        {
            _namespaceLookup = new Dictionary<string, string>();

            for (var i=0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                if (reader.Prefix != "xmlns")
                    continue;

                _namespaceLookup.Add(reader.LocalName + ":", reader.Value + ".");
            }
        }

        /// <summary>
        /// Finds the type in any assembly loaded into the AppDomain.
        /// </summary>
        internal Type FindType(string typeName)
        {
            Type foundType;

            // Shortcut for friendly C# names
            if (_typeAliases.TryGetValue(typeName, out foundType))
                return foundType;

            // If this is an array then handle it separately.
            if (typeName.EndsWith("[]"))
            {
                var arrayType = typeName.Substring(0, typeName.Length - 2);
                foundType = FindType(arrayType);
                return foundType == null ? null : foundType.MakeArrayType();
            }

            // Expand any namespaces in the asset type
            foreach (var pair in _namespaceLookup)
                typeName = typeName.Replace(pair.Key, pair.Value);
            var expandedName = typeName;

            foundType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                         from type in assembly.GetTypes()
                         where type.FullName == typeName || type.Name == typeName
                         select type).FirstOrDefault();

            if (foundType == null)
                foundType = Type.GetType(expandedName, false, true);

            return foundType;
        }
    }        
}