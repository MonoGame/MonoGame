using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Linq;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// Methods to retrieve information about types, such as constructors, properties, and fields.
    /// </summary>
    internal static class ContentExtensions
    {
        /// <summary>
        /// Retrieves all non-static constructors belonging to <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type to retrieve the constructors from.</param>
        /// <returns>List of found non-static constructors</returns>
        public static ConstructorInfo GetDefaultConstructor(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
            this Type type
            )
        {
#if NET45
            var typeInfo = type.GetTypeInfo();
            var ctor = typeInfo.DeclaredConstructors.FirstOrDefault(c => !c.IsStatic && c.GetParameters().Length == 0);
            return ctor;
#else
            var attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            return type.GetConstructor(attrs, null, new Type[0], null);
#endif
        }

        /// <summary>
        /// Retrieves all non-static properties belonging to <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type to retrieve the properties from.</param>
        /// <returns>List of found non-static properties</returns>
        public static PropertyInfo[] GetAllProperties(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
            this Type type)
        {

            // Sometimes, overridden properties of abstract classes can show up even with 
            // BindingFlags.DeclaredOnly is passed to GetProperties. Make sure that
            // all properties in this list are defined in this class by comparing
            // its get method with that of it's base class. If they're the same
            // Then it's an overridden property.
#if NET45
            PropertyInfo[] infos= type.GetTypeInfo().DeclaredProperties.ToArray();
            var nonStaticPropertyInfos = from p in infos
                                         where (p.GetMethod != null) && (!p.GetMethod.IsStatic) &&
                                         (p.GetMethod == p.GetMethod.GetRuntimeBaseDefinition())
                                         select p;
            return nonStaticPropertyInfos.ToArray();
#else
            const BindingFlags attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var allProps = type.GetProperties(attrs).ToList();
            var props = allProps.FindAll(p => p.GetGetMethod(true) != null && p.GetGetMethod(true) == p.GetGetMethod(true).GetBaseDefinition()).ToArray();
            return props;
#endif
        }

        /// <summary>
        /// Retrieves all non-static fields belonging to <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type to retrieve the fields from.</param>
        /// <returns>List of found non-static fields</returns>
        public static FieldInfo[] GetAllFields(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)]
            this Type type
            )
        {
#if NET45
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

        /// <summary>
        /// Whether <paramref name="type"/> is a <see langword="class" />.
        /// </summary>
        /// <param name="type">Type to determine.</param>
        /// <returns>Whether <paramref name="type"/> is a <see langword="class" />.</returns>
        public static bool IsClass(this Type type)
        {
#if NET45
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }
    }
}
