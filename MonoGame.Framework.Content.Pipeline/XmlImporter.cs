// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /*
     * Yet to be implemented:
     * - Custom element names
     * - FlattenContent for nested classes
     * - Shared Resources
     * - External References
     * - Errors for items that aren't allowed to be null
     */

    /// <summary>
    /// Implements an importer for reading intermediate XML files. This is a wrapper around IntermediateSerializer.
    /// </summary>
    [ContentImporter(".xml", DisplayName = "Xml Importer - MonoGame", DefaultProcessor = "PassThroughProcessor")]
    public class XmlImporter : ContentImporter<object>
    {
        private static readonly char[] _elementSeparator = new[] { ' ' };

        /// <summary>
        /// Arrays of value types can be stored in a single XML element separated by
        /// whitespace. This table describes how many items of each type to yield from
        /// those lists.
        /// </summary>
        private static readonly Dictionary<Type, int> _elementCount = new Dictionary<Type, int>
        {
            { typeof(Vector2),      2 },
            { typeof(Vector3),      3 },
            { typeof(Vector4),      4 },
            { typeof(Quaternion),   4 },
            { typeof(Rectangle),    4 },
            { typeof(Color),        4 },
        };

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

        /// <summary>
        /// Initializes a new instance of XmlImporter.
        /// </summary>
        public XmlImporter()
        {
        }

        /// <summary>
        /// Called by the XNA Framework when importing an intermediate file to be used as a game asset. This is the method called by the XNA Framework when an asset is to be imported into an object that can be recognized by the Content Pipeline.
        /// </summary>
        /// <param name="filename">Name of a game asset file.</param>
        /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
        /// <returns>Resulting game asset.</returns>
        public override object Import(string filename, ContentImporterContext context)
        {
            var doc = XDocument.Load(filename, LoadOptions.PreserveWhitespace);
            if (doc.Root == null || doc.Root.Name != "XnaContent")
                throw new InvalidContentException(string.Format("'{0}' does not appear to be a valid XML asset.", filename));

            var assetElement = doc.Root.Element("Asset");
            if (assetElement == null)
                throw new InvalidContentException(string.Format("'{0}' is missing its Asset element.", filename));

            // Some XML assets have namespaces to reduce the verbosity of the file.
            // We create a map of "ShortName:" -> "My.Namespace.LongName" and replace
            // the substring as we go.
            _namespaceLookup = CreateNamespaceLookup(doc);

            var asset = ParseDispatcher(typeof(object), assetElement);
            return asset;
        }

        /// <summary>
        /// Finds the type in any assembly loaded into the AppDomain.
        /// </summary>
        private bool FindType(string typeName, out string expandedName, out Type foundType)
        {
            bool isArray = false;            
            if (typeName.EndsWith("[]"))
            {
                isArray = true;
                typeName = typeName.Replace("[]", "");
            }

            // TODO:
            // Deal with List types...

            // Shortcut for friendly C# names
            if (_typeAliases.TryGetValue(typeName, out foundType))
            {
                expandedName = foundType.FullName;
                return true;
            }

            // Expand any namespaces in the asset type
            foreach (var pair in _namespaceLookup)
                typeName = typeName.Replace(pair.Key, pair.Value);
            expandedName = typeName;

            foundType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                         from type in assembly.GetTypes()
                         where type.FullName == typeName || type.Name == typeName
                         select type).FirstOrDefault();

            if (foundType == null)
                foundType = Type.GetType(expandedName, false, true);

            if (foundType != null && isArray)
            {
                foundType = foundType.MakeArrayType();                
            }

            return foundType != null;
        }

        /// <summary>
        /// Builds a lookup table from a short name to the full namespace.
        /// </summary>
        private static Dictionary<string, string> CreateNamespaceLookup(XDocument doc)
        {
            var nsLookup = new Dictionary<string, string>();
            foreach (var attrib in doc.Root.Attributes())
            {
                if (!attrib.IsNamespaceDeclaration)
                    continue;

                nsLookup.Add(attrib.Name.LocalName + ":", attrib.Value + ".");
            }

            return nsLookup;
        }

        private void SetValue(Type type, object asset, XElement element)
        {
            var memberName = element.Name.LocalName;
            var member = type.GetMember(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault();
            if (member == null)
                throw new InvalidContentException(string.Format("Unable to find member '{0}' on type '{1}'", memberName, type.Name));

            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    SetFieldValue(member as FieldInfo, asset, element);
                    break;

                case MemberTypes.Property:
                    SetPropertyValue(member as PropertyInfo, asset, element);
                    break;

                default:
                    throw new NotImplementedException("Member type not yet implemented!");
            }
        }

        private void SetFieldValue(FieldInfo field, object asset, XElement element)
        {
            // Skip parse for null values.
            if (element.Attribute("Null") != null)
            {
                field.SetValue(asset, null);
                return;
            }

            var parseType = field.FieldType;
            var value = ParseDispatcher(parseType, element);
            field.SetValue(asset, value);
        }

        private void SetPropertyValue(PropertyInfo property, object asset, XElement element)
        {
            // Skip parse for null values.
            if (element.Attribute("Null") != null)
            {
                property.SetValue(asset, null, null);
                return;
            }

            var parseType = property.PropertyType;
            var value = ParseDispatcher(parseType, element);
            property.SetValue(asset, value, null);
        }

        private object ParseDispatcher(Type parseType, XElement element)
        {
            // Type override
            var explicitType = element.Attribute("Type");
            if (explicitType != null)
            {
                string fullTypeName;
                Type replacementType;
                if (!FindType(explicitType.Value, out fullTypeName, out replacementType))
                    throw new InvalidContentException(string.Format("There was an error deserializing intermediate XML. Cannot find type \"{0}\"", fullTypeName));

                parseType = replacementType;
            }

            if (parseType.IsValueType)
            {
                return ParseValueType(parseType, element.Value);
            }
            else
            {
                if (parseType.IsArray)
                {
                    return ParseArray(parseType, element);
                }
                else if (parseType.GetInterfaces().Contains(typeof(IList)))
                {
                    return ParseList(parseType, element);
                }
                else if (parseType.GetInterfaces().Contains(typeof(IDictionary)))
                {
                    return ParseDictionary(parseType, element);
                }
                else if (parseType == typeof(string))
                {
                    return System.Net.WebUtility.HtmlDecode(element.Value);
                }
                else
                {
                    return ParseObject(parseType, element);
                }
            }

            throw new NotImplementedException();
        }

        private object ParseObject(Type type, XElement rootElement)
        {
            object result = null;
            try
            {
                result = Activator.CreateInstance(type, true);
            }
            catch (MissingMethodException e)
            {
                throw new Exception(string.Format("Couldn't create object of type {0}: {1}", type.Name, e.Message), e);
            }

            foreach (var element in rootElement.Elements())
                SetValue(type, result, element);

            return result;
        }

        private object ParseList(Type parseType, XElement element)
        {
            if (!parseType.IsGenericType)
                throw new NotImplementedException("Non-generic lists are not supported.");

            var list = Activator.CreateInstance(parseType) as IList;
            var innerType = parseType.GetGenericArguments()[0];
            foreach (var child in ParseCollection(innerType, element))
                list.Add(child);

            return list;
        }

        private object ParseDictionary(Type parseType, XElement element)
        {
            if (!parseType.IsGenericType)
                throw new NotImplementedException("Non-generic dictionaries are not supported.");

            var dict = Activator.CreateInstance(parseType) as IDictionary;
            var keyType = parseType.GetGenericArguments()[0];
            var valueType = parseType.GetGenericArguments()[1];
            foreach (var item in element.Elements())
            {
                var key = ParseDispatcher(keyType, item.Element("Key"));
                var value = ParseDispatcher(valueType, item.Element("Value"));
                dict.Add(key, value);
            }

            return dict;
        }

        private object ParseArray(Type parseType, XElement element)
        {
            var innerType = parseType.GetElementType();
            var values = innerType.IsValueType
                ? ParseValueTypeArray(innerType, element).ToArray()
                : ParseCollection(innerType, element).ToArray();

            var array = Array.CreateInstance(innerType, values.Length);
            Array.Copy(values, array, values.Length);

            return array;
        }

        private static object ParseValueType(Type parseType, string value)
        {
            if (parseType == typeof(bool))
                return XmlConvert.ToBoolean(value);

            if (parseType == typeof(byte))
                return XmlConvert.ToByte(value);

            if (parseType == typeof(sbyte))
                return XmlConvert.ToSByte(value);

            if (parseType == typeof(char))
                return XmlConvert.ToChar(value);

            if (parseType == typeof(decimal))
                return XmlConvert.ToDecimal(value);

            if (parseType == typeof(double))
                return XmlConvert.ToDouble(value);

            if (parseType == typeof(float))
                return XmlConvert.ToSingle(value);

            if (parseType == typeof(int))
                return XmlConvert.ToInt32(value);

            if (parseType == typeof(uint))
                return XmlConvert.ToUInt32(value);

            if (parseType == typeof(long))
                return XmlConvert.ToInt64(value);

            if (parseType == typeof(ulong))
                return XmlConvert.ToUInt64(value);

            if (parseType == typeof(short))
                return XmlConvert.ToInt16(value);

            if (parseType == typeof(ushort))
                return XmlConvert.ToUInt16(value);

            if (parseType == typeof(Guid))
                return XmlConvert.ToGuid(value);

            if (parseType == typeof(TimeSpan))
                return XmlConvert.ToTimeSpan(value);

            if (parseType == typeof(Vector2))
            {
                var values = value.Split(_elementSeparator, 2);
                return new Vector2(XmlConvert.ToSingle(values[0]), XmlConvert.ToSingle(values[1]));
            }

            if (parseType == typeof(Vector3))
            {
                var values = value.Split(_elementSeparator, 3);
                return new Vector3(XmlConvert.ToSingle(values[0]), XmlConvert.ToSingle(values[1]), XmlConvert.ToSingle(values[2]));
            }

            if (parseType == typeof(Vector4))
            {
                var values = value.Split(_elementSeparator, 4);
                return new Vector4(XmlConvert.ToSingle(values[0]), XmlConvert.ToSingle(values[1]), XmlConvert.ToSingle(values[2]), XmlConvert.ToSingle(values[3]));
            }

            if (parseType == typeof(Quaternion))
            {
                var values = value.Split(_elementSeparator, 4);
                return new Quaternion(XmlConvert.ToSingle(values[0]), XmlConvert.ToSingle(values[1]), XmlConvert.ToSingle(values[2]), XmlConvert.ToSingle(values[3]));
            }

            if (parseType == typeof(Rectangle))
            {
                var values = value.Split(_elementSeparator, 4);
                return new Rectangle(XmlConvert.ToInt32(values[0]), XmlConvert.ToInt32(values[1]), XmlConvert.ToInt32(values[2]), XmlConvert.ToInt32(values[3]));
            }

            if (parseType.IsEnum)
                return Enum.Parse(parseType, value);

            if (parseType == typeof(Color))
            {
                // Swizzle ARGB -> ABGR
                var argb = uint.Parse(value, NumberStyles.HexNumber);

                uint abgr = 0;
                abgr |= (argb & 0xFF000000);
                abgr |= (argb & 0x00FF0000) >> 16;
                abgr |= (argb & 0x0000FF00);
                abgr |= (argb & 0x000000FF) << 16;
                
                return new Color { PackedValue = abgr };                
            }

            throw new NotImplementedException();
        }

        private static IEnumerable<object> ParseValueTypeArray(Type parseType, XElement element)
        {
            if (!_elementCount.ContainsKey(parseType))
                throw new NotImplementedException(string.Format("Value type arrays are not implemented for type '{0}'", parseType.Name));

            var values = element.Value.Split(_elementSeparator);

            // See if we need to read multiples.
            int elementCount;
            if (!_elementCount.TryGetValue(parseType, out elementCount))
                elementCount = 1;

            var offset = 0;
            while (offset < values.Length)
            {
                // This can probably be reworked to avoid lots of allocations.
                var substring = string.Join(" ", values, offset, elementCount);
                offset += elementCount;
                yield return ParseValueType(parseType, substring);
            }
        }

        private IEnumerable<object> ParseCollection(Type parseType, XElement element)
        {
            var children = element.Elements();
            foreach (var child in children)
                yield return ParseDispatcher(parseType, child);
        }
    }
}
