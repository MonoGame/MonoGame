using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    internal class GenericCollectionHelper
    {
        public static bool IsGenericCollectionType(Type type, bool checkAncestors)
        {
            return GetCollectionElementType(type, checkAncestors) != null;
        }

        private static Type GetCollectionElementType(Type type, bool checkAncestors)
        {
            if (!checkAncestors && type.BaseType != null && FindCollectionInterface(type.BaseType) != null)
                return null;

            var collectionInterface = FindCollectionInterface(type);
            if (collectionInterface == null)
                return null;

            return collectionInterface.GetGenericArguments()[0];
        }

        private static Type FindCollectionInterface(Type type)
        {
            var interfaces = type.FindInterfaces((t, o) =>
            {
                if (t.IsGenericType)
                    return t.GetGenericTypeDefinition() == typeof(ICollection<>);
                return false;
            }, null);

            return (interfaces.Length == 1)
                ? interfaces[0]
                : null;
        }

        private readonly ContentTypeSerializer _contentSerializer;
        private readonly PropertyInfo _countProperty;
        private readonly MethodInfo _addMethod;

        public GenericCollectionHelper(IntermediateSerializer serializer, Type type)
        {
            var collectionElementType = GetCollectionElementType(type, false);
            _contentSerializer = serializer.GetTypeSerializer(collectionElementType);

            var collectionType = typeof(ICollection<>).MakeGenericType(collectionElementType);
            _countProperty = collectionType.GetProperty("Count");
            _addMethod = collectionType.GetMethod("Add", new[] { collectionElementType });
        }

        public bool ObjectIsEmpty(object list)
        {
            return (int) _countProperty.GetValue(list, null) == 0;
        }

        public void ScanChildren(ContentTypeSerializer.ChildCallback callback, object collection)
        {
            foreach (var item in (IEnumerable) collection)
                if (item != null)
                    callback(_contentSerializer, item);
        }

        public void Serialize(IntermediateWriter output, object collection, ContentSerializerAttribute format)
        {
            var itemFormat = new ContentSerializerAttribute();
            itemFormat.ElementName = format.CollectionItemName;
            foreach (var item in (IEnumerable) collection)
                output.WriteObject(item, itemFormat, _contentSerializer);
        }

        public void Deserialize(IntermediateReader input, object collection, ContentSerializerAttribute format)
        {
            var itemFormat = new ContentSerializerAttribute();
            itemFormat.ElementName = format.CollectionItemName;
            while (input.MoveToElement(format.CollectionItemName))
                _addMethod.Invoke(collection, new[] { input.ReadObject<object>(itemFormat, _contentSerializer) });
        }
    }
}