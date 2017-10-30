using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    internal class NamespaceAliasHelper
    {
        private readonly IntermediateSerializer _serializer;

        /// <summary>
        /// Maps "My.Namespace.LongName" -> "ShortName" for type lookups.
        /// </summary>
        private Dictionary<string, AliasedNamespace> _namespaceLookupReverse;

        private class AliasedNamespace
        {
            public string Alias;
            public string TypePrefix;
        }

        public NamespaceAliasHelper(IntermediateSerializer serializer)
        {
            _serializer = serializer;
        }

        public void WriteNamespaces<T>(XmlWriter writer, T value)
        {
            // Maps "My.Namespace.LongName" -> "ShortName" for type lookups.
            _namespaceLookupReverse = new Dictionary<string, AliasedNamespace>();

            // Get all namespaces of types used by "value" or its children.
            var childNamespaces = GetAllUsedNamespaces(value).Distinct().ToList();

            // Do first pass to determine what our aliases are. We do this on a sorted
            // list of namespaces so that more-nested namespaces will be processed last,
            // by which time we will have already created the aliases for parent namespaces.
            var sortedChildNamespaces = new List<string>(childNamespaces);
            sortedChildNamespaces.Sort();
            var tempAliases = new Dictionary<string, AliasedNamespace>();
            foreach (var childNamespace in sortedChildNamespaces)
            {
                var alias = FindAlias(tempAliases, childNamespace);
                if (alias != null)
                    tempAliases.Add(childNamespace, alias);
            }

            // Do second pass on the namespaces as they were originally ordered, to match XNA.
            foreach (var childNamespace in childNamespaces)
            {
                AliasedNamespace alias;
                if (tempAliases.TryGetValue(childNamespace, out alias))
                    _namespaceLookupReverse.Add(childNamespace, alias);
            }

            foreach (var kvp in _namespaceLookupReverse)
            {
                if (!string.IsNullOrEmpty(kvp.Value.TypePrefix))
                    continue;
                writer.WriteAttributeString("xmlns", kvp.Value.Alias, null, kvp.Key);
            }
        }

        private IEnumerable<string> GetAllUsedNamespaces<T>(T value)
        {
            var result = new List<string>();
            ContentTypeSerializer.ChildCallback onScanChild = (contentTypeSerializer, child) =>
            {
                if (child == null)
                    return;

                var childType = child.GetType();

                if (contentTypeSerializer.TargetType == childType)
                    return;

                if (contentTypeSerializer.TargetType.IsGenericType 
                    && contentTypeSerializer.TargetType.GetGenericTypeDefinition() == typeof(Nullable<>) 
                    && contentTypeSerializer.TargetType.GetGenericArguments()[0] == childType)
                    return;

                if (_serializer.HasTypeAlias(childType))
                    return;

                var childNamespace = childType.Namespace;

                if (string.IsNullOrEmpty(childNamespace))
                    return;

                result.Add(childNamespace);
            };

            // Force top-level object type to be included.
            onScanChild(_serializer.GetTypeSerializer(typeof(object)), value);

            // Scan child objects.
            var serializer = _serializer.GetTypeSerializer(typeof(T));
            serializer.ScanChildren(_serializer, onScanChild, value);

            return result;
        }

        private static AliasedNamespace FindAlias(Dictionary<string, AliasedNamespace> aliases, string childNamespace)
        {
            if (string.IsNullOrEmpty(childNamespace))
                return null;

            // If there isn't yet an alias for the last part of the namespace, use that.
            var alias = childNamespace.Substring(childNamespace.LastIndexOf('.') + 1);
            if (aliases.All(x => x.Value.Alias != alias))
                return new AliasedNamespace
                {
                    Alias = alias,
                    TypePrefix = string.Empty
                };

            // Otherwise, find the longest parent namespace, and use that, with a TypePrefix to make
            // this namespace relative to that one.
            if (aliases.Any(x => childNamespace.StartsWith(x.Key)))
            {
                string longestParentNamespace = string.Empty;
                foreach (var kvp in aliases.Where(x => string.IsNullOrEmpty(x.Value.TypePrefix)))
                {
                    if (childNamespace.StartsWith(kvp.Key) && kvp.Key.Length > longestParentNamespace.Length)
                        longestParentNamespace = kvp.Key;
                }
                return new AliasedNamespace
                {
                    Alias = aliases[longestParentNamespace].Alias,
                    TypePrefix = GetRelativeNamespace(longestParentNamespace, childNamespace) + "."
                };
            }

            return null;
        }

        /// <summary>
        /// Returns just the portion <paramref name="namespace"/> relative to <paramref name="namespaceParent"/>.
        /// For example, given namespaceParent=Foo.Bar and @namespace=Foo.Bar.Baz, will return Baz.
        /// </summary>
        private static string GetRelativeNamespace(string namespaceParent, string @namespace)
        {
            if (@namespace.StartsWith(namespaceParent))
                return @namespace.Substring(namespaceParent.Length + 1);
            throw new InvalidOperationException();
        }

        public bool TryGetAliasedTypeName(Type type, out string typeName)
        {
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                AliasedNamespace namespaceAlias;
                if (_namespaceLookupReverse.TryGetValue(type.Namespace, out namespaceAlias))
                {
                    typeName = namespaceAlias.Alias + ":" + namespaceAlias.TypePrefix + _serializer.GetTypeName(type);
                    return true;
                }
            }

            typeName = null;
            return false;
        }
    }
}