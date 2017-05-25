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
#if NET4
            return targetType.IsValueType;    
#else
            return targetType.GetTypeInfo().IsValueType;
#endif
        }

        public static Type GetBaseType(Type targetType)
        {
            if (targetType == null)
            {
                throw new NullReferenceException("Must supply the targetType parameter");
            }
#if NET4
            return targetType.BaseType;
#else
            return targetType.GetTypeInfo().BaseType;
#endif
        }

        /// <summary>
        /// Returns the Assembly of a Type
        /// </summary>
        public static Assembly GetAssembly(Type targetType)
        {
            if (targetType == null)
            {
                throw new NullReferenceException("Must supply the targetType parameter");
            }
#if NET4
            return targetType.Assembly;
#else
            return targetType.GetTypeInfo().Assembly;
#endif
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
#if NET4
            if (t.IsClass && !t.IsAbstract)
                return true;
#else            
            var ti = t.GetTypeInfo();
            if (ti.IsClass && !ti.IsAbstract)
                return true;
#endif
            return false;
        }

        public static MethodInfo GetMethodInfo(Type type, string methodName)
        {
#if NET4
            return type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
#else
            return type.GetTypeInfo().GetDeclaredMethod(methodName);
#endif
        }

        public static MethodInfo GetPropertyGetMethod(PropertyInfo property)
        {
            if (property == null)
            {
                throw new NullReferenceException("Must supply the property parameter");
            }

#if NET4
            return property.GetGetMethod();
#else
            return property.GetMethod;
#endif
        }

        public static MethodInfo GetPropertySetMethod(PropertyInfo property)
        {
            if (property == null)
            {
                throw new NullReferenceException("Must supply the property parameter");
            }

#if NET4
            return property.GetSetMethod();
#else
            return property.SetMethod;
#endif
        }

        public static T GetCustomAttribute<T>(MemberInfo member) where T : Attribute
        {
            if (member == null)
                throw new NullReferenceException("Must supply the member parameter");

#if NET4
            return Attribute.GetCustomAttribute(member, typeof(T)) as T;
#else
            return member.GetCustomAttribute(typeof(T)) as T;
#endif
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
#if NET4
            if (type.IsAssignableFrom(objectType))
                return true;
#else
            if (type.GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo()))
                return true;            
#endif
            return false;
        }
    }
}