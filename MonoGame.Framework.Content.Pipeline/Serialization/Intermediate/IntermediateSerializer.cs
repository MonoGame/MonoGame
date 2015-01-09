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

        private static readonly Dictionary<Type, string> _typeAliasesReverse;

        static IntermediateSerializer()
        {
            _typeAliasesReverse = _typeAliases.ToDictionary(x => x.Value, x => x.Key);
        }

        private IntermediateSerializer()
        {
            _scannedObjects = new List<object>();
        }

        /// <summary>
        /// Maps "ShortName:" -> "My.Namespace.LongName." for type lookups.
        /// </summary>
        private Dictionary<string, string> _namespaceLookup;

        /// <summary>
        /// Maps "My.Namespace.LongName" -> "ShortName" for type lookups.
        /// </summary>
        private Dictionary<string, AliasedNamespace> _namespaceLookupReverse;

        private class AliasedNamespace
        {
            public string Alias;
            public string TypePrefix;
        }

        private Dictionary<Type, ContentTypeSerializer> _serializers;

        private Dictionary<Type, Type> _genericSerializerTypes;

        private readonly List<object> _scannedObjects;

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

            serializer.WriteNamespaces(output, value);

            // Write the asset.
            var format = new ContentSerializerAttribute { ElementName = "Asset" };
            writer.WriteObjectInternal(value, format, serializer.GetTypeSerializer(typeof(T)), typeof(object));

            // Process the shared resources and external references.
            writer.WriteSharedResources();
            writer.WriteExternalReferences();

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

        private void WriteNamespaces<T>(XmlWriter writer, T value)
        {
            // Maps "My.Namespace.LongName" -> "ShortName" for type lookups.
            _namespaceLookupReverse = new Dictionary<string, AliasedNamespace>();

            var childNamespaces = new List<string>();
            ContentTypeSerializer.ChildCallback onScanChild = (contentTypeSerializer, child) =>
            {
                if (child == null)
                    return;

                var childType = child.GetType();

                if (contentTypeSerializer.TargetType == childType)
                    return;

                if (_typeAliasesReverse.ContainsKey(childType))
                    return;

                var childNamespace = childType.Namespace;

                if (string.IsNullOrEmpty(childNamespace))
                    return;

                childNamespaces.Add(childNamespace);
            };

            // Force top-level object type to be included.
            onScanChild(GetTypeSerializer(typeof(object)), value);

            var serializer = GetTypeSerializer(typeof(T));
            serializer.ScanChildren(this, onScanChild, value);

            childNamespaces = childNamespaces.Distinct().ToList();

            // Do first pass to determinate what our aliases are.
            var sortedChildNamespaces = new List<string>(childNamespaces);
            sortedChildNamespaces.Sort();
            var tempAliases = new Dictionary<string, string>();
            foreach (var childNamespace in sortedChildNamespaces)
            {
                var alias = FindAlias(tempAliases, childNamespace);
                if (alias != null)
                    tempAliases.Add(childNamespace, alias);
            }

            // Do second pass to calculate the TypePrefix for each alias.
            foreach (var childNamespace in childNamespaces)
            {
                var alias = FindAvailableAlias(tempAliases, childNamespace, childNamespace);
                if (alias != null)
                    _namespaceLookupReverse.Add(childNamespace, alias);
            }

            var writtenAliases = new List<string>();
            foreach (var kvp in _namespaceLookupReverse)
            {
                if (!string.IsNullOrEmpty(kvp.Value.TypePrefix))
                    continue;
                if (!writtenAliases.Contains(kvp.Value.Alias))
                    writer.WriteAttributeString("xmlns", kvp.Value.Alias, null, kvp.Key);
                writtenAliases.Add(kvp.Value.Alias);
            }
        }

        private string FindAlias(Dictionary<string, string> aliases, string childNamespace)
        {
            if (string.IsNullOrEmpty(childNamespace))
                return null;

            var alias = childNamespace.Substring(childNamespace.LastIndexOf('.') + 1);
            if (aliases.All(x => x.Value != alias))
                return alias;

            // Find the longest parent namespace.
            if (aliases.Any(x => childNamespace.StartsWith(x.Key)))
            {
                string longestParentNamespace = string.Empty;
                foreach (var kvp in aliases)
                {
                    if (childNamespace.StartsWith(kvp.Key) && kvp.Key.Length > longestParentNamespace.Length)
                        longestParentNamespace = kvp.Key;
                }
                return aliases[longestParentNamespace];
            }

            return null;
        }

        private AliasedNamespace FindAvailableAlias(Dictionary<string, string> tempAliases, string childNamespace, string originalNamespace)
        {
            string alias;
            if (tempAliases.TryGetValue(childNamespace, out alias))
            {
                if (alias == childNamespace.Substring(childNamespace.LastIndexOf('.') + 1))
                    return new AliasedNamespace
                    {
                        Alias = alias,
                        TypePrefix = string.Empty
                    };

                var namespaceParent = tempAliases
                    .Where(x => x.Value == alias)
                    .Select(x => x.Key)
                    .OrderBy(x => x.Length)
                    .First();
                return new AliasedNamespace
                {
                    Alias = alias,
                    TypePrefix = GetRelativeNamespace(namespaceParent, childNamespace) + "."
                };
            }
            return null;
        }

        /// <summary>
        /// Returns just the portion <paramref name="@namespace"/> relative to <paramref name="namespaceParent"/>.
        /// For example, given namespaceParent=Foo.Bar and @namespace=Foo.Bar.Baz, will return Baz.
        /// </summary>
        private static string GetRelativeNamespace(string namespaceParent, string @namespace)
        {
            if (@namespace.StartsWith(namespaceParent))
                return @namespace.Substring(namespaceParent.Length + 1);
            throw new InvalidOperationException();
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

        /// <summary>
        /// Gets the (potentially) aliased name for any type.
        /// </summary>
        internal string GetTypeName(Type type)
        {
            string typeName;

            // Shortcut for friendly C# names
            if (_typeAliasesReverse.TryGetValue(type, out typeName))
                return typeName;

            // Look for aliased namespace.
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                AliasedNamespace namespaceAlias;
                if (_namespaceLookupReverse.TryGetValue(type.Namespace, out namespaceAlias))
                    return namespaceAlias.Alias + ":" + namespaceAlias.TypePrefix + type.Name;
            }

            return type.FullName;
        }

        internal bool AlreadyScanned(object value)
        {
            if (_scannedObjects.Contains(value))
                return true;
            _scannedObjects.Add(value);
            return false;
        }
    }
}