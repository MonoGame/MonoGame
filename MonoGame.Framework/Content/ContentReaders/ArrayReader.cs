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

using System;
#if WINRT
using System.Reflection;
#endif

namespace Microsoft.Xna.Framework.Content
{
    internal class ArrayReader<T> : ContentTypeReader<T[]>
    {
        ContentTypeReader elementReader;

        internal ArrayReader()
        {
        }

        protected internal override void Initialize(ContentTypeReaderManager manager)
		{
			Type readerType = typeof(T);
			elementReader = manager.GetTypeReader(readerType);
        }

        protected internal override T[] Read(ContentReader input, T[] existingInstance)
        {
            uint count = input.ReadUInt32();
            T[] array = existingInstance;
            if (array == null)
                array = new T[count];

#if WINRT
            if (typeof(T).GetTypeInfo().IsValueType)
#else
            if (typeof(T).IsValueType)
#endif
			{
                for (uint i = 0; i < count; i++)
                {
                	array[i] = input.ReadObject<T>(elementReader);
                }
			}
			else
			{
                for (uint i = 0; i < count; i++)
                {
                    int readerType = input.Read7BitEncodedInt();
                	array[i] = readerType > 0 ? input.ReadObject<T>(input.TypeReaders[readerType - 1]) : default(T);
                }
			}
            return array;
        }
    }
}
