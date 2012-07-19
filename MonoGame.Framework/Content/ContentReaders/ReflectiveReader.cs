#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

// Original source from SilverSprite project at http://silversprite.codeplex.com

using System;
using System.Reflection;

namespace Microsoft.Xna.Framework.Content
{
    internal class ReflectiveReader<T> : ContentTypeReader
    {
        ConstructorInfo constructor;
        PropertyInfo[] properties;
        FieldInfo[] fields;
        ContentTypeReaderManager manager;
		
		Type targetType;
		Type baseType;
		ContentTypeReader baseTypeReader;

        internal ReflectiveReader() : base(typeof(T))
        {
			targetType = typeof(T);
        }

        protected internal override void Initialize(ContentTypeReaderManager manager)
        {
            base.Initialize(manager);
            this.manager = manager;

#if WINRT
            var type = targetType.GetTypeInfo().BaseType;
#else
            var type = targetType.BaseType;
#endif
            if (type != null && type != typeof(object))
			{
				baseType = type;
				baseTypeReader = manager.GetTypeReader(baseType);
			}
			
            constructor = targetType.GetDefaultConstructor();
            properties = targetType.GetAllProperties();
            fields = targetType.GetAllFields();
        }

        static object CreateChildObject(PropertyInfo property, FieldInfo field)
        {
            object obj = null;
            Type t;
            if (property != null)
            {
                t = property.PropertyType;
            }
            else
            {
                t = field.FieldType;
            }

#if WINRT
            var ti = t.GetTypeInfo();
            if (ti.IsClass && !ti.IsAbstract)
#else
            if (t.IsClass && !t.IsAbstract)
#endif
            {
                var constructor = t.GetDefaultConstructor();
                if (constructor != null)
                {
                    obj = constructor.Invoke(null);                
                }
            }
            return obj;
        }

        private void Read( object parent, ContentReader input, MemberInfo member)
        {
            PropertyInfo property = member as PropertyInfo;
            FieldInfo field = member as FieldInfo;
            // properties must have public get and set
            if (property != null && (property.CanWrite == false || property.CanRead == false))
                return;
#if WINRT
            Attribute attr = member.GetCustomAttribute(typeof(ContentSerializerIgnoreAttribute));
#else
            Attribute attr = Attribute.GetCustomAttribute(member, typeof(ContentSerializerIgnoreAttribute));
#endif
            if (attr != null) 
                return;
#if WINRT
            Attribute attr2 = member.GetCustomAttribute(typeof(ContentSerializerAttribute));
#else
            Attribute attr2 = Attribute.GetCustomAttribute(member, typeof(ContentSerializerAttribute));
#endif
            bool isSharedResource = false;
            if (attr2 != null)
            {
                var cs = attr2 as ContentSerializerAttribute;
                isSharedResource = cs.SharedResource;
            }
            else
            {
                if (property != null)
                {
#if WINRT
                    if ( property.GetMethod != null && !property.GetMethod.IsPublic )
                        return;
                    if ( property.SetMethod != null && !property.SetMethod.IsPublic )
                        return;
#else
                    foreach (MethodInfo info in property.GetAccessors(true))
                    {
                        if (info.IsPublic == false)
                            return;
                    }
#endif
                }
                else
                {
                    if (!field.IsPublic)
                        return;
                }
            }
            ContentTypeReader reader = null;
            if (property != null)
            {
                reader = manager.GetTypeReader(property.PropertyType);
            }
            else
            {
                reader = manager.GetTypeReader(field.FieldType);
            }
            if (!isSharedResource)
            {
                object existingChildObject = CreateChildObject(property, field);
                object obj2;
				
                obj2 = input.ReadObject(reader, existingChildObject);
				
                if (property != null)
                {
                    property.SetValue(parent, obj2, null);
                }
                else
                {
                    // Private fields can be serialized if they have ContentSerializerAttribute added to them
                    if (field.IsPrivate == false || attr2 != null)
                        field.SetValue(parent, obj2);
                }
            }
            else
            {
                Action<object> action = delegate(object value)
                {
                    if (property != null)
                    {
                        property.SetValue(parent, value, null);
                    }
                    else
                    {
                        field.SetValue(parent, value);
                    }
                };
                input.ReadSharedResource(action);
            }
        }
        
        protected internal override object Read(ContentReader input, object existingInstance)
        {
            T obj;
            if (existingInstance != null)
            {
                obj = (T)existingInstance;
            }
            else
            {
                obj = (constructor == null ? (T)Activator.CreateInstance(typeof(T), false) : (T)constructor.Invoke(null));
            }
			
			if(baseTypeReader != null)
				baseTypeReader.Read(input, obj);

            // Box the type.
            var boxed = (object)obj;

            foreach (var property in properties)
                Read(boxed, input, property);

            foreach (var field in fields)
                Read(boxed, input, field);

            // Unbox it... required for value types.
            obj = (T)boxed;

            return obj;
        }
    }
}
