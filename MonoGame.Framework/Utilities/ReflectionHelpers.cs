using System;
using System.Reflection;

namespace Microsoft.Xna.Framework.Utilities
{
    internal static partial class ReflectionHelpers
    {
        public static bool IsValueType(Type targetType)
        {
            if (targetType == null)
            {
                throw new NullReferenceException("Must supply the targetType parameter");
            }

            return targetType.GetTypeInfo().IsValueType;
        }

        public static Type GetBaseType(Type targetType)
        {
            if (targetType == null)
            {
                throw new NullReferenceException("Must supply the targetType parameter");
            }

            return targetType.GetTypeInfo().BaseType;
        }

        /// <summary>
        /// Returns true if the given type represents a non-object type that is not abstract.
        /// </summary>
        public static bool IsConcreteClass(Type t)
        {
            if (t == null)
            {
                throw new NullReferenceException("Must supply the t (type) parameter");
            }

            if (t == typeof(object))
                return false;
            
            var ti = t.GetTypeInfo();
            if (ti.IsClass && !ti.IsAbstract)
                return true;

            return false;
        }

        public static MethodInfo GetPropertyGetMethod(PropertyInfo property)
        {
            if (property == null)
            {
                throw new NullReferenceException("Must supply the property parameter");
            }

            return property.GetMethod;
        }

        public static MethodInfo GetPropertySetMethod(PropertyInfo property)
        {
            if (property == null)
            {
                throw new NullReferenceException("Must supply the property parameter");
            }

            return property.SetMethod;
        }

        public static T GetCustomAttribute<T>(MemberInfo member) where T : Attribute
        {
            if (member == null)
                throw new NullReferenceException("Must supply the member parameter");

            return member.GetCustomAttribute(typeof(T)) as T;
        }

        /// <summary>
        /// Returns true if the get method of the given property exist and are public.
        /// Note that we allow a getter-only property to be serialized (and deserialized),
        /// *if* CanDeserializeIntoExistingObject is true for the property type.
        /// </summary>
        public static bool PropertyIsPublic(PropertyInfo property)
        {
            if (property == null)
            {
                throw new NullReferenceException("Must supply the property parameter");
            }

            var getMethod = GetPropertyGetMethod(property);

            if (getMethod == null || !getMethod.IsPublic)
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if the given type can be assigned the given value
        /// </summary>
        public static bool IsAssignableFrom(Type type, object value)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (value == null)
                throw new ArgumentNullException("value");

            return IsAssignableFromType(type, value.GetType());
        }

        /// <summary>
        /// Returns true if the given type can be assigned a value with the given object type
        /// </summary>
        public static bool IsAssignableFromType(Type type, Type objectType)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (objectType == null)
                throw new ArgumentNullException("objectType");
            
            if (type.GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo()))
                return true;

            return false;
        }
    }
}