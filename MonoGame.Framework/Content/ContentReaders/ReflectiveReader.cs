// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    internal class ReflectiveReader<T> : ContentTypeReader
    {
        delegate void ReadElement(ContentReader input, object parent);

        private List<ReadElement> _readers;

        private ConstructorInfo _constructor;

        private ContentTypeReader _baseTypeReader;


        internal ReflectiveReader() 
            : base(typeof(T))
        {
        }

        protected internal override void Initialize(ContentTypeReaderManager manager)
        {
            base.Initialize(manager);

            var baseType = ReflectionHelpers.GetBaseType(TargetType);
            if (baseType != null && baseType != typeof(object))
				_baseTypeReader = manager.GetTypeReader(baseType);

            _constructor = TargetType.GetDefaultConstructor();

            var properties = TargetType.GetAllProperties();
            var fields = TargetType.GetAllFields();
            _readers = new List<ReadElement>(fields.Length + properties.Length);

            // Gather the properties.
            foreach (var property in properties)
            {
                var read = GetElementReader(manager, property);
                if (read != null)
                    _readers.Add(read);
            }
            
            // Gather the fields.
            foreach (var field in fields)
            {
                var read = GetElementReader(manager, field);
                if (read != null)
                    _readers.Add(read);
            }
        }

        private static ReadElement GetElementReader(ContentTypeReaderManager manager, MemberInfo member)
        {
            var property = member as PropertyInfo;
            var field = member as FieldInfo;

            // properties must have public get and set
            if (property != null && (property.CanWrite == false || property.CanRead == false))
                return null;

            if (property != null && property.Name == "Item")
            {
                var getMethod = ReflectionHelpers.GetPropertyGetMethod(property);
                var setMethod = ReflectionHelpers.GetPropertySetMethod(property);

                if ((getMethod != null && getMethod.GetParameters().Length > 0) ||
                    (setMethod != null && setMethod.GetParameters().Length > 0))
                {
                    /*
                     * This is presumably a property like this[indexer] and this
                     * should not get involved in the object deserialization
                     * */
                    return null;
                }
            }

            var attr = ReflectionHelpers.GetCustomAttribute(member, typeof (ContentSerializerIgnoreAttribute));
            if (attr != null)
                return null;

            var contentSerializerAttribute =
                ReflectionHelpers.GetCustomAttribute(member, typeof (ContentSerializerAttribute)) as
                ContentSerializerAttribute;

            var isSharedResource = false;
            if (contentSerializerAttribute != null)
                isSharedResource = contentSerializerAttribute.SharedResource;
            else
            {
                if (property != null)
                {
                    if (!ReflectionHelpers.PropertyIsPublic(property))
                        return null;
                }
                else
                {
                    if (!field.IsPublic)
                        return null;

                    // evolutional: Added check to skip initialise only fields
                    if (field.IsInitOnly)
                        return null;

                    // Private fields can be serialized if they have ContentSerializerAttribute added to them
                    if (field.IsPrivate && contentSerializerAttribute == null)
                        return null;
                }
            }

            Action<object, object> setter;
            ContentTypeReader reader;
            Type elementType;
            if (property != null)
            {
                elementType = property.PropertyType;
                reader = manager.GetTypeReader(property.PropertyType);
                setter = (o, v) => property.SetValue(o, v, null);
            }
            else
            {
                elementType = field.FieldType;
                reader = manager.GetTypeReader(field.FieldType);
                setter = field.SetValue;
            }

            if (isSharedResource)
            {
                return (input, parent) =>
                {
                    Action<object> action = value => setter(parent, value);
                    input.ReadSharedResource(action);
                };
            }

            Func<object> construct = () => null;
            if (ReflectionHelpers.IsConcreteClass(elementType))
            {
                var constructor = elementType.GetDefaultConstructor();
                if (constructor != null)
                    construct = () => constructor.Invoke(null);
            }

            // Reading elements serialized as "object".
            if (reader == null && elementType == typeof(object))
            {
                return (input, parent) =>
                {
                    var obj2 = input.ReadObject<object>();
                    setter(parent, obj2);
                };
            }

            // evolutional: Fix. We can get here and still be NULL, exit gracefully
            if (reader == null)
                return null;

            return (input, parent) =>
            {
                var existing = construct();
                var obj2 = input.ReadObject(reader, existing);
                setter(parent, obj2);
            };
        }
      
        protected internal override object Read(ContentReader input, object existingInstance)
        {
            T obj;
            if (existingInstance != null)
                obj = (T)existingInstance;
            else
                obj = (_constructor == null ? (T)Activator.CreateInstance(typeof(T)) : (T)_constructor.Invoke(null));
		
			if(_baseTypeReader != null)
				_baseTypeReader.Read(input, obj);

            // Box the type.
            var boxed = (object)obj;

            foreach (var reader in _readers)
                reader(input, boxed);

            // Unbox it... required for value types.
            obj = (T)boxed;

            return obj;
        }
    }
}
