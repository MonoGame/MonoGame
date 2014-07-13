// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.Xna.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    internal class ReflectiveSerializer : ContentTypeSerializer
    {
        private struct ElementInfo
        {
            public ContentSerializerAttribute Attribute;
            public ContentTypeSerializer Serializer;
            public Action<object, object> Setter;
            public Func<object, object> Getter;
        };

        private readonly Dictionary<string, ElementInfo> _elements = new Dictionary<string, ElementInfo>();

        private bool GetElementInfo(IntermediateSerializer serializer, MemberInfo member, out ElementInfo info)
        {
            info = new ElementInfo();

            // Are we ignoring this property?
            if (ReflectionHelpers.GetCustomAttribute(member, typeof(ContentSerializerIgnoreAttribute)) != null)
                return false;

            var prop = member as PropertyInfo;
            var field = member as FieldInfo;
            
            var attrib = ReflectionHelpers.GetCustomAttribute(member, typeof(ContentSerializerAttribute)) as ContentSerializerAttribute;
            if (attrib != null)
            {
                // Store the attribute for later use.
                info.Attribute = attrib.Clone();

                // Default the to member name as the element name.
                if (string.IsNullOrEmpty(attrib.ElementName))
                    info.Attribute.ElementName = member.Name;
            }
            else
            {
                // We don't have a serializer attribute, so we can
                // only access this member thru a public field/property.

                if (prop != null)
                {
                    // If we don't have at least a public getter then this 
                    // property can't be serialized or deserialized in any way.
                    if (prop.GetGetMethod() == null)
                        return false;

                    // If there is no public setter and the property is a system
                    // type then we have no way for it to be deserialized.
                    if (prop.GetSetMethod() == null &&
                        prop.PropertyType.Namespace == "System")
                        return false;
                }
                else if (field != null)
                {
                    if (!field.IsPublic)
                        return false;
                }

                info.Attribute = new ContentSerializerAttribute();
                info.Attribute.ElementName = member.Name;
            }

            if (prop != null)
            {
                info.Serializer = serializer.GetTypeSerializer(prop.PropertyType);
                if (prop.CanWrite)
                    info.Setter = (o, v) => prop.SetValue(o, v, null);
                info.Getter = (o) => prop.GetValue(o, null);
            }
            else if (field != null)
            {
                info.Serializer = serializer.GetTypeSerializer(field.FieldType);
                info.Setter = field.SetValue;
                info.Getter = field.GetValue;
            }

            return true;
        }

        public ReflectiveSerializer(Type targetType) :
            base(targetType, string.Empty)
        {
        }

        protected internal override void Initialize(IntermediateSerializer serializer)
        {
            var properties = TargetType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                ElementInfo info;
                if (GetElementInfo(serializer, prop, out info))
                    _elements.Add(info.Attribute.ElementName, info);
            }

            var fields = TargetType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                ElementInfo info;
                if (GetElementInfo(serializer, field, out info))
                    _elements.Add(info.Attribute.ElementName, info);                
            }
        }

        protected internal override object Deserialize(IntermediateReader input, ContentSerializerAttribute format, object existingInstance)
        {
            var result = existingInstance;
            if (result == null)
            {
                try
                {
                    result = Activator.CreateInstance(TargetType, true);
                }
                catch (MissingMethodException e)
                {
                    throw new Exception(string.Format("Couldn't create object of type {0}: {1}", TargetType.Name, e.Message), e);
                }                
            }

            var reader = input.Xml;
            var depth = reader.Depth;

            // Read the next node.
            while (reader.Read())
            {
                // Skip over any whitespace.
                if (reader.NodeType == XmlNodeType.Whitespace)
                    continue;

                // Did we reach the end of this object?
                if (reader.NodeType == XmlNodeType.EndElement)
                    break;

                Debug.Assert(reader.Depth == depth, "We are not at the right depth!");

                if (reader.NodeType == XmlNodeType.Element)
                {
                    var elementName = reader.Name;

                    ElementInfo info;
                    if (!_elements.TryGetValue(elementName, out info))
                        throw new InvalidContentException(string.Format("Element `{0}` was not found in type `{1}`.", elementName, TargetType));

                    if (info.Attribute.SharedResource)
                    {
                        Action<object> fixup = (o) => info.Setter(result, o);
                        input.ReadSharedResource(info.Attribute, fixup);
                    }
                    else if (info.Setter == null)
                    {
                        var value = info.Getter(result);
                        input.ReadObject(info.Attribute, info.Serializer, value);
                    }
                    else
                    {
                        var value = input.ReadObject<object>(info.Attribute, info.Serializer, null);
                        info.Setter(result, value);
                    }

                    continue;
                }

                // If we got here then we were not interested 
                // in this node... so skip its children.
                reader.Skip();
            }

            return result;
        }

        public override bool ObjectIsEmpty(object value)
        {
            throw new NotImplementedException(); 
        }

        protected internal override void ScanChildren(IntermediateSerializer serializer, ChildCallback callback, object value)
        {
            throw new NotImplementedException();
        }

        protected internal override void Serialize(IntermediateWriter output, object value, ContentSerializerAttribute format)
        {
            throw new NotImplementedException();
        }
    }
}