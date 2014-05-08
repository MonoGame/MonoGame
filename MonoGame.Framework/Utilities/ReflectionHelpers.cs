
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
			return !targetType.GetTypeInfo().IsValueType;
#else
			return !targetType.IsValueType;
#endif
		   
		}

		public static Type GetBaseTpye(Type targetType)
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

		public static bool IsAbstractClass(Type t)
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

		public static MethodInfo GetPropertyMethod(PropertyInfo property, string method)
		{
			if (property == null)
			{
				throw new NullReferenceException("Must supply the property parameter");
			}

			MethodInfo methodInfo;
#if WINRT
			if(method == "get")
				methodInfo = property.GetMethod;
			else
				methodInfo = property.SetMethod;
#else
			if(method == "get")
                methodInfo = property.GetGetMethod();
			else
                methodInfo = property.GetSetMethod();
#endif
			return methodInfo;

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

		public static bool HasPublicProperties(PropertyInfo property)
		{
			if (property == null)
			{
				throw new NullReferenceException("Must supply the property parameter");
			}
#if WINRT
			if ( property.GetMethod != null && !property.GetMethod.IsPublic )
				return true;
			if ( property.SetMethod != null && !property.SetMethod.IsPublic )
				return true;
#else
			foreach (MethodInfo info in property.GetAccessors(true))
			{
				if (info.IsPublic == false)
					return true;
			}
#endif
			return false;
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
