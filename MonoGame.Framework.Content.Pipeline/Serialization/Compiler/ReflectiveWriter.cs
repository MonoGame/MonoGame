// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    class ReflectiveWriter<T> : ContentTypeWriter
    {
        private PropertyInfo[] _properties;
        private FieldInfo[] _fields;

        private Type _baseType;
        private ContentTypeWriter _baseTypeWriter;

        private string _runtimeType;

        
        public ReflectiveWriter()
            : base(typeof(T))
        {
        }

        protected override void Initialize(ContentCompiler compiler)
        {
            var type = ReflectionHelpers.GetBaseType(TargetType);
            if (type != null && type != typeof(object))
            {
                _baseType = type;
                _baseTypeWriter = compiler.GetTypeWriter(_baseType);
            }

            var runtimeType = TargetType.GetCustomAttributes(typeof(ContentSerializerRuntimeTypeAttribute), false).FirstOrDefault() as ContentSerializerRuntimeTypeAttribute;
            if (runtimeType != null)
                _runtimeType = runtimeType.RuntimeType;

            var typeVersion = TargetType.GetCustomAttributes(typeof(ContentSerializerTypeVersionAttribute), false).FirstOrDefault() as ContentSerializerTypeVersionAttribute;
            if (typeVersion != null)
                _typeVersion = typeVersion.TypeVersion;
            
            _properties = TargetType.GetAllProperties();
            _fields = TargetType.GetAllFields();
        }

        private static void Write(object parent, ContentWriter output, MemberInfo member)
        {
            var property = member as PropertyInfo;
            var field = member as FieldInfo;
            Debug.Assert(field != null || property != null);

            // Properties must have public get and set
            if (property != null && (property.CanWrite == false || property.CanRead == false))
                return;

            if (property != null && property.Name == "Item")
            {
                var getMethod = ReflectionHelpers.GetPropertyGetMethod(property);
                var setMethod = ReflectionHelpers.GetPropertySetMethod(property);

                if ((getMethod != null && getMethod.GetParameters().Length > 0) ||
                    (setMethod != null && setMethod.GetParameters().Length > 0))
                {
                    // This is presumably a property like this[indexer] and this
                    // should not get involved in the object deserialization.
                    return;
                }
            }

            var attr = ReflectionHelpers.GetCustomAttribute(member, typeof(ContentSerializerIgnoreAttribute));
            if (attr != null) 
                return;

            var contentSerializerAttribute = ReflectionHelpers.GetCustomAttribute(member, typeof(ContentSerializerAttribute)) as ContentSerializerAttribute;

            bool isSharedResource = false;
            if (contentSerializerAttribute != null)
            {
                isSharedResource = contentSerializerAttribute.SharedResource;
            }
            else
            {
                if (property != null)
                {
                    if (!ReflectionHelpers.PropertyIsPublic(property))
                        return;
                }
                else
                {
                    if (!field.IsPublic)
                        return;

                    // evolutional: Added check to skip initialise only fields
                    if (field.IsInitOnly)
                        return;
                }
            }

            ContentTypeWriter writer;
            Type elementType;
            object memberObject;

            if (property != null)
            {
                elementType = property.PropertyType;
                writer = output.GetTypeWriter(elementType);
                memberObject = property.GetValue(parent, null);
            }
            else
            {
                elementType = field.FieldType;
                writer = output.GetTypeWriter(elementType);
                memberObject = field.GetValue(parent);
            }

            if (!isSharedResource)
            {
                if (writer == null && elementType == typeof(object))
                {
                    // Write elements serialized as "object".
                    output.WriteObject(memberObject);
                }
                else
                {
                    // We can get here and still be NULL, exit gracefully.
                    if (writer == null)
                        return;

                    output.WriteObject(memberObject, writer);
                }
            }
            else
            {
                output.WriteSharedResource(memberObject);
            }
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            if (string.IsNullOrEmpty(_runtimeType))
                return base.GetRuntimeType(targetPlatform);

            return _runtimeType;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Microsoft.Xna.Framework.Content.ReflectiveReader`1[[" + 
                        GetRuntimeType(targetPlatform) 
                    + "]]";
        }

        protected internal override void Write(ContentWriter output, object value)
        {
            if(_baseTypeWriter != null)
                _baseTypeWriter.Write(output, value);

            foreach (var property in _properties)
                Write(value, output, property);

            foreach (var field in _fields)
                Write(value, output, field);
        }
    }
}
