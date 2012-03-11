using System;
using System.Reflection;

#if WINRT
using System.Reflection.Emit;
using System.Linq;
#endif

namespace Microsoft.Xna.Framework.Content
{
    public static class ContentExtensions
    {
        public static bool GetIsConcreteClass(this Type type)
        {
#if WINRT
            var info = type.GetTypeInfo();
            return info.IsClass && !info.IsAbstract;
#else
            return type.IsClass && !type.IsAbstract;
#endif
        }

        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
#if WINRT
            return TypeBuilder.GetConstructor(type, null);
#else
            var attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            return type.GetConstructor(attrs, null, new Type[0], null);
#endif
        }

        public static PropertyInfo[] GetAllProperties(this Type type)
        {
#if WINRT
            return type.GetTypeInfo().DeclaredProperties.ToArray();
#else
            var attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            return type.GetProperties(attrs);
#endif
        }


        public static FieldInfo[] GetAllFields(this Type type)
        {
#if WINRT
            return type.GetTypeInfo().DeclaredFields.ToArray();
#else
            var attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            return type.GetFields(attrs);
#endif
        }
    }
}
