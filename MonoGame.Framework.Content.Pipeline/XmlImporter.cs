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
     * Backlog of stuff to support:
     * - Dictionaries
     * - Overridden names via ContentSerializer.CollectionItemName
     * - Array asset types (<Asset Type="Class[]">)
     * - Parsing optimized array element types (integer array, vector array)
     * - Polymorphic array types
     * - FlattenContent for nested classes
     * - Shared Resources
     * - External References
     */

    /// <summary>
    /// Implements an importer for reading intermediate XML files. This is a wrapper around IntermediateSerializer.
    /// </summary>
    [ContentImporter(".xml", DisplayName = "Xml Importer - MonoGame", DefaultProcessor = "ModelProcessor")]
    public class XmlImporter : ContentImporter<object>
    {
        private static readonly char[] _elementSeparator = new[] { ' ' };

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
            var nsLookup = CreateNamespaceLookup(doc);
            var typeName = assetElement.Attribute("Type").Value;

            // Expand any namespaces in the asset type
            foreach (var pair in nsLookup)
                typeName = typeName.Replace(pair.Key, pair.Value);

            // Now find the type information itself
            var actualType =
                (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                 from type in assembly.GetTypes()
                 where type.FullName == typeName || type.Name == typeName
                 select type).FirstOrDefault();

            if (actualType == null)
                throw new InvalidContentException(string.Format("There was an error deserializing intermediate XML. Cannot find type \"{0}\"", typeName));

            var asset = CreateObject(actualType, assetElement);
            return asset;
        }

        /// <summary>
        /// Creates an object and populates its fields and properties from XML elements
        /// </summary>
        private static object CreateObject(Type type, XElement rootElement)
        {
            var result = Activator.CreateInstance(type);
            foreach (var element in rootElement.Elements())
                SetValue(type, result, element);

            return result;
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

        private static void SetValue(Type type, object asset, XElement element)
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

        private static void SetFieldValue(FieldInfo field, object asset, XElement element)
        {
            // Skip parse for null values.
            if (element.Attribute("Null") != null)
            {
                field.SetValue(asset, null);
                return;
            }

            var parseType = field.FieldType;
            object value = null;

            if (parseType.GetInterfaces().Contains(typeof(IList)))
            {
                value = ParseList(parseType, element);
            }
            else
            {
                value = ParseDispatcher(parseType, element.Value);
            }

            field.SetValue(asset, value);
        }

        private static void SetPropertyValue(PropertyInfo property, object asset, XElement element)
        {
            // Skip parse for null values.
            if (element.Attribute("Null") != null)
            {
                property.SetValue(asset, null, null);
                return;
            }

            var parseType = property.PropertyType;
            object value = null;

            if (parseType.GetInterfaces().Contains(typeof(IList)) && parseType.IsGenericType)
            {
                value = ParseList(parseType, element);
            }
            else
            {
                value = ParseDispatcher(parseType, element.Value);
            }
            property.SetValue(asset, value, null);
        }

        private static object ParseList(Type parseType, XElement element)
        {
            var list = Activator.CreateInstance(parseType) as IList;
            var innerType = parseType.GetGenericArguments()[0];
            var children = element.Elements();
            foreach (var child in children)
            {
                var innerValue = CreateObject(innerType, child);
                list.Add(innerValue);
            }

            return list;
        }

        private static object ParseDispatcher(Type parseType, string value)
        {
            if (parseType == typeof(bool))
                return XmlConvert.ToBoolean(value);

            if (parseType == typeof(char))
                return XmlConvert.ToChar(value);

            if (parseType == typeof(byte))
                return XmlConvert.ToByte(value);

            if (parseType == typeof(short))
                return XmlConvert.ToInt16(value);

            if (parseType == typeof(ushort))
                return XmlConvert.ToUInt16(value);

            if (parseType == typeof(int))
                return XmlConvert.ToInt32(value);

            if (parseType == typeof(uint))
                return XmlConvert.ToUInt32(value);

            if (parseType == typeof(float))
                return XmlConvert.ToSingle(value);

            if (parseType == typeof(double))
                return XmlConvert.ToDouble(value);

            if (parseType == typeof(string))
                return System.Net.WebUtility.HtmlDecode(value);

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

            if (parseType.IsEnum)
                return Enum.Parse(parseType, value);

            if (parseType == typeof(Color))
            {
                // Swizzle ARGB -> ABGR
                var argb = uint.Parse(value, NumberStyles.HexNumber);
                var abgr = ((argb & 0xFF00FF00) | ((argb & 0x00FF0000) >> 16) | ((argb & 0x000000FF) << 16));
                return new Color { PackedValue = abgr };
            }

            throw new NotImplementedException();
        }
    }
}
