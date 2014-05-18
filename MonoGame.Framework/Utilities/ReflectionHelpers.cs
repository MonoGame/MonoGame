
using System;
using System.IO;
using System.Reflection;

namespace Microsoft.Xna.Framework.Utilities
{
	internal static class ReflectionHelpers
	{
		public static bool IsValueType(Type targetType)
		{
			if (targetType == null)
			{
				throw new NullReferenceException("Must supply the targetType parameter");
			}
#if WINRT
			return targetType.GetTypeInfo().IsValueType;
#else
			return targetType.IsValueType;
#endif
		}

		public static Type GetBaseType(Type targetType)
		{
			if (targetType == null)
			{
				throw new NullReferenceException("Must supply the targetType parameter");
			}
#if WINRT
			var type = targetType.GetTypeInfo().BaseType;
#else
			var type = targetType.BaseType;
#endif
			return type;
		}

        /// <summary>
        /// Returns true if the given type represents a class that is not abstract
        /// </summary>
		public static bool IsConcreteClass(Type t)
		{
			if (t == null)
			{
				throw new NullReferenceException("Must supply the t (type) parameter");
			}
#if WINRT
			var ti = t.GetTypeInfo();
			if (ti.IsClass && !ti.IsAbstract)
				return true;
#else
			if (t.IsClass && !t.IsAbstract)
				return true;
#endif
			return false;
		}

        public static MethodInfo GetPropertyGetMethod(PropertyInfo property)
        {
            if (property == null)
            {
                throw new NullReferenceException("Must supply the property parameter");
            }

#if WINRT
            return property.GetMethod;
#else
            return property.GetGetMethod();
#endif
        }

        public static MethodInfo GetPropertySetMethod(PropertyInfo property)
        {
            if (property == null)
            {
                throw new NullReferenceException("Must supply the property parameter");
            }

#if WINRT
            return property.SetMethod;
#else
            return property.GetSetMethod();
#endif
        }

		public static Attribute GetCustomAttribute(MemberInfo member, Type memberType)
		{
			if (member == null)
			{
				throw new NullReferenceException("Must supply the member parameter");
			}
			if (memberType == null)
			{
				throw new NullReferenceException("Must supply the memberType parameter");
			}
#if WINRT
			Attribute attr = member.GetCustomAttribute(memberType);
#else
			Attribute attr = Attribute.GetCustomAttribute(member, memberType);
#endif
			return attr;
		}

        /// <summary>
        /// Returns true if the get and set methods of the given property exist and are public
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

            var setMethod = GetPropertySetMethod(property);
            if (setMethod == null || !setMethod.IsPublic)
                return false;

            return true;
        }

		public static bool IsAssignableFrom(Type type, object provider)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			if (provider == null)
				throw new ArgumentNullException("provider");
#if WINRT
			if (type.GetTypeInfo().IsAssignableFrom(provider.GetType().GetTypeInfo()))
				return true;
#else
			if (type.IsAssignableFrom(provider.GetType()))
				return true;
#endif
			return false;
		}

	}
}
