// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using Microsoft.Xna.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    internal class ReflectiveSerializer : ContentTypeSerializer
    {
        private struct ElementInfo
        {
            public string Name;
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
            
            // If we can write or read from it we can skip it.
            if (prop != null && (!prop.CanWrite || !prop.CanRead))
                return false;

            // Default the to member name as the element name.
            info.Name = member.Name;

            var attrib = ReflectionHelpers.GetCustomAttribute(member, typeof(ContentSerializerAttribute)) as ContentSerializerAttribute;
            if (attrib != null)
            {
                if (!string.IsNullOrEmpty(attrib.ElementName))
                    info.Name = attrib.ElementName;
            }
            else if (prop != null)
            {
                if (!ReflectionHelpers.PropertyIsPublic(prop))
                    return false;
            }
            else if (field != null)
            {
                if (!field.IsPublic)
                    return false;
            }

            if (prop != null)
            {
                info.Serializer = serializer.GetTypeSerializer(prop.PropertyType);
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
            var properties = TargetType.GetAllProperties();
            foreach (var prop in properties)
            {
                ElementInfo info;
                if (GetElementInfo(serializer, prop, out info))
                    _elements.Add(info.Name, info);
            }

            var fields = TargetType.GetAllFields();
            foreach (var field in fields)
            {
                ElementInfo info;
                if (GetElementInfo(serializer, field, out info))
                    _elements.Add(info.Name, info);                
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
                // Did we reach the end of this object?
                if (reader.NodeType == XmlNodeType.EndElement)
                    break;

                Debug.Assert(reader.Depth == depth, "We are not at the right depth!");

                if (reader.NodeType == XmlNodeType.Element)
                {
                    var elementName = reader.Name;
                    reader.ReadStartElement();

                    ElementInfo info;
                    if (!_elements.TryGetValue(elementName, out info))
                        throw new InvalidContentException(string.Format("Element `{0}` was not found in type `{1}`.", elementName, TargetType));
                    var value = info.Serializer.Deserialize(input, format, null);
                    info.Setter(result, value);
                    reader.ReadEndElement();
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