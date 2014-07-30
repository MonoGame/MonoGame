﻿using System;
using System.Reflection;
using System.Linq;

#if WINRT
using System.Reflection.Emit;
#endif

namespace Microsoft.Xna.Framework.Content
{
    internal static class ContentExtensions
    {
        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
#if WINRT
            var typeInfo = type.GetTypeInfo();
            var ctor = typeInfo.DeclaredConstructors.FirstOrDefault(c => !c.IsStatic && c.GetParameters().Length == 0);
            return ctor;
#else
            var attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            return type.GetConstructor(attrs, null, new Type[0], null);
#endif
        }

        public static PropertyInfo[] GetAllProperties(this Type type)
        {

            // Sometimes, overridden properties of abstract classes can show up even with 
            // BindingFlags.DeclaredOnly is passed to GetProperties. Make sure that
            // all properties in this list are defined in this class by comparing
            // its get method with that of it's base class. If they're the same
            // Then it's an overridden property.
#if WINRT
            PropertyInfo[] infos= type.GetTypeInfo().DeclaredProperties.ToArray();
            var nonStaticPropertyInfos = from p in infos
                                         where (p.GetMethod != null) && (!p.GetMethod.IsStatic) &&
                                         (p.GetMethod == p.GetMethod.GetRuntimeBaseDefinition())
                                         select p;
            return nonStaticPropertyInfos.ToArray();
#else
            var attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var allProps = type.GetProperties(attrs).ToList();

            var props = allProps.FindAll(p => p.GetGetMethod() != null && p.GetGetMethod() == p.GetGetMethod().GetBaseDefinition()).ToArray();
            return props;
#endif
        }


        public static FieldInfo[] GetAllFields(this Type type)
        {
#if WINRT
            FieldInfo[] fields= type.GetTypeInfo().DeclaredFields.ToArray();
            var nonStaticFields = from field in fields
                    where !field.IsStatic
                    select field;
            return nonStaticFields.ToArray();
#else
            var attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            return type.GetFields(attrs);
#endif
        }
    }
}
