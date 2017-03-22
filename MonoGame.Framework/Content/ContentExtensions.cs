using System;
using System.Reflection;
using System.Linq;

namespace Microsoft.Xna.Framework.Content
{
    internal static class ContentExtensions
    {
        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            var ctor = typeInfo.DeclaredConstructors.FirstOrDefault(c => !c.IsStatic && c.GetParameters().Length == 0);
            return ctor;
        }

        public static PropertyInfo[] GetAllProperties(this Type type)
        {

            // Sometimes, overridden properties of abstract classes can show up even with 
            // BindingFlags.DeclaredOnly is passed to GetProperties. Make sure that
            // all properties in this list are defined in this class by comparing
            // its get method with that of it's base class. If they're the same
            // Then it's an overridden property.
            PropertyInfo[] infos= type.GetTypeInfo().DeclaredProperties.ToArray();
            var nonStaticPropertyInfos = from p in infos
                                         where (p.GetMethod != null) && (!p.GetMethod.IsStatic) &&
                                         (p.GetMethod == p.GetMethod.GetRuntimeBaseDefinition())
                                         select p;
            return nonStaticPropertyInfos.ToArray();
        }


        public static FieldInfo[] GetAllFields(this Type type)
        {
            FieldInfo[] fields= type.GetTypeInfo().DeclaredFields.ToArray();
            var nonStaticFields = from field in fields
                    where !field.IsStatic
                    select field;
            return nonStaticFields.ToArray();
        }

        public static bool IsClass(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }
    }
}
