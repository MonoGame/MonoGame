// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    internal class ReflectiveSerializer : ContentTypeSerializer
    {
        const BindingFlags _bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        private struct ElementInfo
        {
            public ContentSerializerAttribute Attribute;
            public ContentTypeSerializer Serializer;
            public Action<object, object> Setter;
            public Func<object, object> Getter;
        };

        private readonly List<ElementInfo> _elements = new List<ElementInfo>();

        private ContentTypeSerializer _baseSerializer;
        private GenericCollectionHelper _collectionHelper;

        private bool GetElementInfo(IntermediateSerializer serializer, MemberInfo member, out ElementInfo info)
        {
            info = new ElementInfo();

            // Are we ignoring this property?
            if (ReflectionHelpers.GetCustomAttribute<ContentSerializerIgnoreAttribute>(member) != null)
                return false;

            var prop = member as PropertyInfo;
            var field = member as FieldInfo;

            var attrib = ReflectionHelpers.GetCustomAttribute<ContentSerializerAttribute>(member);
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

                    // If there is a setter, but it's private, then don't include this element
                    // (although technically we could, as long as we have a serializer with
                    // CanDeserializeIntoExistingObject=true for this property type)
                    var setter = prop.GetSetMethod(true);
                    if (setter != null && !setter.IsPublic)
                        return false;

                    // If there is no setter, and we don't have a type serializer 
                    // that can deserialize into an existing object, then we have no way 
                    // for it to be deserialized.
                    if (setter == null && !serializer.GetTypeSerializer(prop.PropertyType).CanDeserializeIntoExistingObject)
                        return false;

                    // Don't serialize or deserialize indexers.
                    if (prop.GetIndexParameters().Any())
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
                info.Getter = o => prop.GetValue(o, null);
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
            // If we have a base type then we need to deserialize it first.
            if (TargetType.BaseType != null)
                _baseSerializer = serializer.GetTypeSerializer(TargetType.BaseType);

            // Cache all our serializable properties.
            var properties = TargetType.GetProperties(_bindingFlags);
            foreach (var prop in properties)
            {
                ElementInfo info;
                if (GetElementInfo(serializer, prop, out info))
                    _elements.Add(info);
            }

            // Cache all our serializable fields.
            var fields = TargetType.GetFields(_bindingFlags);
            foreach (var field in fields)
            {
                ElementInfo info;
                if (GetElementInfo(serializer, field, out info))
                    _elements.Add(info);                
            }

            if (GenericCollectionHelper.IsGenericCollectionType(TargetType, false))
                _collectionHelper = serializer.GetCollectionHelper(TargetType);
        }

        public override bool CanDeserializeIntoExistingObject
        {
            get { return TargetType.IsClass && TargetType.BaseType != null; }
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

            // First deserialize the base type.
            if (_baseSerializer != null)
                _baseSerializer.Deserialize(input, format, result);

            // Now deserialize our own elements.
            foreach (var info in _elements)
            {
                if (!info.Attribute.FlattenContent)
                {
                    if (!input.MoveToElement(info.Attribute.ElementName))
                    {
                        // If the the element was optional then we can
                        // safely skip it and continue.
                        if (info.Attribute.Optional)
                            continue;
                        
                        // We failed to find a required element.
                        throw new InvalidContentException(string.Format("The Xml element `{0}` is required, but element `{1}` was found at line {2}:{3}. Try changing the element order or adding missing elements.", info.Attribute.ElementName, input.Xml.Name, ((IXmlLineInfo)input.Xml).LineNumber, ((IXmlLineInfo)input.Xml).LinePosition));
                    }
                }

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
                    var value = input.ReadObject<object>(info.Attribute, info.Serializer);
                    info.Setter(result, value);
                }
            }

            if (_collectionHelper != null)
                _collectionHelper.Deserialize(input, result, format);

            return result;
        }

        public override bool ObjectIsEmpty(object value)
        {
            if (_baseSerializer != null)
                return _baseSerializer.ObjectIsEmpty(value);
            if (_collectionHelper != null)
                return _collectionHelper.ObjectIsEmpty(value);
            return false;
        }

        protected internal override void ScanChildren(IntermediateSerializer serializer, ChildCallback callback, object value)
        {
            if (serializer.AlreadyScanned(value))
                return;

            // First scan the base type.
            if (_baseSerializer != null)
                _baseSerializer.ScanChildren(serializer, callback, value);

            // Now scan our own elements.
            foreach (var info in _elements)
            {
                var elementValue = info.Getter(value);

                callback(info.Serializer, elementValue);

                var elementSerializer = info.Serializer;
                if (elementValue != null)
                    elementSerializer = serializer.GetTypeSerializer(elementValue.GetType());

                elementSerializer.ScanChildren(serializer, callback, elementValue);
            }

            if (_collectionHelper != null)
                _collectionHelper.ScanChildren(callback, value);
        }

        protected internal override void Serialize(IntermediateWriter output, object value, ContentSerializerAttribute format)
        {
            // First serialize the base type.
            if (_baseSerializer != null)
                _baseSerializer.Serialize(output, value, format);

            // Now serialize our own elements.
            foreach (var info in _elements)
            {
                var elementValue = info.Getter(value);

                if (info.Attribute.SharedResource)
                    output.WriteSharedResource(elementValue, info.Attribute);
                else
                    output.WriteObjectInternal(elementValue, info.Attribute, info.Serializer, info.Serializer.TargetType);
            }

            if (_collectionHelper != null)
                _collectionHelper.Serialize(output, value, format);
        }
    }
}
