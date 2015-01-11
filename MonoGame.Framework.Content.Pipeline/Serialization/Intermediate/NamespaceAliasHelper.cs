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

            var childNamespaces = new List<string>();
            ContentTypeSerializer.ChildCallback onScanChild = (contentTypeSerializer, child) =>
            {
                if (child == null)
                    return;

                var childType = child.GetType();

                if (contentTypeSerializer.TargetType == childType)
                    return;

                if (_serializer.HasTypeAlias(childType))
                    return;

                var childNamespace = childType.Namespace;

                if (string.IsNullOrEmpty(childNamespace))
                    return;

                childNamespaces.Add(childNamespace);
            };

            // Force top-level object type to be included.
            onScanChild(_serializer.GetTypeSerializer(typeof(object)), value);

            var serializer = _serializer.GetTypeSerializer(typeof(T));
            serializer.ScanChildren(_serializer, onScanChild, value);

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

        public bool TryGetAliasedTypeName(Type type, out string typeName)
        {
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                AliasedNamespace namespaceAlias;
                if (_namespaceLookupReverse.TryGetValue(type.Namespace, out namespaceAlias))
                {
                    typeName = namespaceAlias.Alias + ":" + namespaceAlias.TypePrefix + type.Name;
                    return true;
                }
            }

            typeName = null;
            return false;
        }
    }
}