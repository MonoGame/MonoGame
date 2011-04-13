#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

// Original source from SilverSprite project at http://silversprite.codeplex.com

using System;
using System.Reflection;

using Microsoft.Xna.Framework.Content;

namespace Microsoft.Xna.Framework.Content
{
    internal class ReflectiveReader<T> : ContentTypeReader
    {
        ConstructorInfo constructor;
        PropertyInfo[] properties;
        FieldInfo[] fields;
        ContentTypeReaderManager manager;

        internal ReflectiveReader() : base(typeof(T))
        {
        }

        protected internal override void Initialize(ContentTypeReaderManager manager)
        {
            base.Initialize(manager);
            this.manager = manager;
            BindingFlags attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            constructor = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
            properties = typeof(T).GetProperties(attrs);
            fields = typeof(T).GetFields(attrs);
        }

        object CreateChildObject(PropertyInfo property, FieldInfo field)
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
            if (t.IsClass)
            {
                ConstructorInfo constructor = t.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
                if (constructor != null)
                {
                    obj = constructor.Invoke(null);                
                }
            }
            return obj;
        }

        private void Read(object parent, ContentReader input, MemberInfo member)
        {
            PropertyInfo property = member as PropertyInfo;
            FieldInfo field = member as FieldInfo;
            if (property != null && property.CanWrite == false) return;
            Attribute attr = Attribute.GetCustomAttribute(member, typeof(ContentSerializerIgnoreAttribute));
            if (attr != null) return;
            Attribute attr2 = Attribute.GetCustomAttribute(member, typeof(ContentSerializerAttribute));
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
                    foreach (MethodInfo info in property.GetAccessors(true))
                    {
                        if (info.IsPublic == false) return;
                    }
                }
                else
                {
                    if (!field.IsPublic) return;
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
                object obj2 = null;
                obj2 = input.ReadObject<object>(reader, existingChildObject);
                if (property != null)
                {
                    property.SetValue(parent, obj2, null);
                }
                else
                {
                    if (field.IsPrivate == false) field.SetValue(parent, obj2);
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
                input.ReadSharedResource<object>(action);
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
                obj = (T)constructor.Invoke(null);
            }
            foreach (PropertyInfo property in properties)
            {
                Read(obj, input, property);
            }
            foreach (FieldInfo field in fields)
            {
                Read(obj, input, field);
            }
            return obj;
        }
    }
}
