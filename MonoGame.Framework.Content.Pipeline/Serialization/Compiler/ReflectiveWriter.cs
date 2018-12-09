// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    class ReflectiveWriter<T> : ContentTypeWriter
    {
        private PropertyInfo[] _properties;
        private FieldInfo[] _fields;

        private Type _baseType;

        private string _runtimeType;
        private ContentCompiler _compiler;
        private static HashSet<MemberInfo> _sharedResources = new HashSet<MemberInfo>();

        public ReflectiveWriter()
            : base(typeof(T))
        {
        }

        public override bool CanDeserializeIntoExistingObject
        {
            get { return TargetType.IsClass; }
        }

        protected override void Initialize(ContentCompiler compiler)
        {
            _compiler = compiler;
            var type = ReflectionHelpers.GetBaseType(TargetType);                
            if (type != null && type != typeof(object) && !TargetType.IsValueType)
                _baseType = type;

            var runtimeType = TargetType.GetCustomAttributes(typeof(ContentSerializerRuntimeTypeAttribute), false).FirstOrDefault() as ContentSerializerRuntimeTypeAttribute;
            if (runtimeType != null)
                _runtimeType = runtimeType.RuntimeType;

            var typeVersion = TargetType.GetCustomAttributes(typeof(ContentSerializerTypeVersionAttribute), false).FirstOrDefault() as ContentSerializerTypeVersionAttribute;
            if (typeVersion != null)
                _typeVersion = typeVersion.TypeVersion;

            _properties = TargetType.GetAllProperties().Where(IsValidProperty).ToArray();
            _fields = TargetType.GetAllFields().Where(IsValidField).ToArray();
        }

        /// <inheritdoc/>
        internal override void OnAddedToContentWriter(ContentWriter output)
        {
            base.OnAddedToContentWriter(output);

            foreach (var property in _properties)
                output.GetTypeWriter(property.PropertyType);

            foreach (var field in _fields)
                output.GetTypeWriter(field.FieldType);
        }

        private bool IsValidProperty(PropertyInfo property)
        {
            // Properties must have at least a getter.
            if (property.CanRead == false)
                return false;

            // Skip over indexer properties.
            if (property.Name == "Item" && property.GetIndexParameters().Length > 0)
                return false;

            // Are we explicitly asked to ignore this item?
            if (ReflectionHelpers.GetCustomAttribute<ContentSerializerIgnoreAttribute>(property) != null)
                return false;

            var contentSerializerAttribute = ReflectionHelpers.GetCustomAttribute<ContentSerializerAttribute>(property);
            if (contentSerializerAttribute == null)
            {
                // There is no ContentSerializerAttribute, so non-public
                // properties cannot be serialized.
                if (!ReflectionHelpers.PropertyIsPublic(property))
                    return false;

                // Check the type reader to see if it is safe to
                // deserialize into the existing type.
                if (!property.CanWrite)
                {
                    if (!_compiler.GetTypeWriter(property.PropertyType).CanDeserializeIntoExistingObject)
                        return false;
                }
            }
            else if (contentSerializerAttribute.SharedResource)
            {
                _sharedResources.Add(property);
            }

            return true;
        }

        private bool IsValidField(FieldInfo field)
        {
            // Are we explicitly asked to ignore this item?
            if (ReflectionHelpers.GetCustomAttribute<ContentSerializerIgnoreAttribute>(field) != null)
                return false;

            var contentSerializerAttribute = ReflectionHelpers.GetCustomAttribute<ContentSerializerAttribute>(field);
            if (contentSerializerAttribute == null)
            {
                // There is no ContentSerializerAttribute, so non-public
                // fields cannot be deserialized.
                if (!field.IsPublic)
                    return false;

                // evolutional: Added check to skip initialise only fields
                if (field.IsInitOnly)
                    return false;
            }
            else if (contentSerializerAttribute.SharedResource)
            {
                _sharedResources.Add(field);
            }

            return true;
        }

        private static void Write(object parent, ContentWriter output, MemberInfo member)
        {
            var property = member as PropertyInfo;
            var field = member as FieldInfo;
            Debug.Assert(field != null || property != null);

            Type elementType;
            object memberObject;

            if (property != null)
            {
                elementType = property.PropertyType;
                memberObject = property.GetValue(parent, null);
            }
            else
            {
                elementType = field.FieldType;
                memberObject = field.GetValue(parent);
            }

            if (_sharedResources.Contains(member))
                output.WriteSharedResource(memberObject);
            else
            {
                var writer = output.GetTypeWriter(elementType);
                if (writer == null || elementType == typeof(object) || elementType == typeof(Array))
                    output.WriteObject(memberObject);
                else
                    output.WriteObject(memberObject, writer);
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
            if (_baseType != null)
            {
                var baseTypeWriter = output.GetTypeWriter(_baseType);
                baseTypeWriter.Write(output, value);
            }

            foreach (var property in _properties)
                Write(value, output, property);

            foreach (var field in _fields)
                Write(value, output, field);
        }
    }
}
