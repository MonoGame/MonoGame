// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections;
using System.Reflection;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    internal class GenericCollectionHelper
    {
        public static bool IsGenericCollectionType(Type type, bool checkAncestors) => GetCollectionElementType(type, checkAncestors) != null;

        private static Type? GetCollectionElementType(Type type, bool checkAncestors)
        {
            if (!checkAncestors && type.BaseType != null && FindCollectionInterface(type.BaseType) != null)
                return null;

            return FindCollectionInterface(type)?.GetGenericArguments()[0];
        }

        private static Type? FindCollectionInterface(Type type)
        {
            var interfaces = type.FindInterfaces((t, _) =>
            {
                if (t.IsGenericType)
                    return t.GetGenericTypeDefinition() == typeof(ICollection<>);
                return false;
            }, null);

            return (interfaces.Length == 1) ? interfaces[0] : null;
        }

        private readonly ContentTypeSerializer _contentSerializer;
        private readonly PropertyInfo _countProperty;
        private readonly MethodInfo _addMethod;

        public GenericCollectionHelper(IntermediateSerializer serializer, Type type)
        {
            var collectionElementType = GetCollectionElementType(type, false) ??
                throw new ArgumentException($"Invalid type has been provided for GenericCollectionHelper: {type}", nameof(type));
            _contentSerializer = serializer.GetTypeSerializer(collectionElementType);

            var collectionType = typeof(ICollection<>).MakeGenericType(collectionElementType);
            _countProperty = collectionType.GetProperty("Count")!;
            _addMethod = collectionType.GetMethod("Add", [collectionElementType])!;
        }

        public bool ObjectIsEmpty(object? list)
        {
            if (list == null)
                return true;

            if (_countProperty.GetValue(list, null) is not int listCount)
                return true;

            return listCount == 0;
        }

        public void ScanChildren(ContentTypeSerializer.ChildCallback callback, object collection)
        {
            foreach (var item in (IEnumerable) collection)
                if (item != null)
                    callback(_contentSerializer, item);
        }

        public void Serialize(IntermediateWriter output, object? collection, ContentSerializerAttribute format)
        {
            if (collection == null)
            {
                return;
            }

            var itemFormat = new ContentSerializerAttribute
            {
                ElementName = format.CollectionItemName
            };
            foreach (var item in (IEnumerable) collection)
                output.WriteObject(item, itemFormat, _contentSerializer);
        }

        public void Deserialize(IntermediateReader input, object collection, ContentSerializerAttribute format)
        {
            var itemFormat = new ContentSerializerAttribute
            {
                ElementName = format.CollectionItemName
            };
            while (input.MoveToElement(format.CollectionItemName))
                _addMethod.Invoke(collection, [input.ReadObject<object>(itemFormat, _contentSerializer)]);
        }
    }
}
