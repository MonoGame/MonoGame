// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// This type is not meant to be used directly by MonoGame users.
    /// Its purpose is to allow to work-around AOT issues when loading assets with the ContentManager fail due to the absence of runtime-reflection support in that context (i.e. missing types due to trimming and inability to statically discover them at compile-time).
    /// If ContentManager.Load() throws an NotSupportedExeception, the message should provide insights on how to fix it.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All)]
    public class ArrayReader<T> : ContentTypeReader<T[]>
    {
        ContentTypeReader elementReader;

        /// <summary/>
        public ArrayReader()
        {
        }

        /// <summary/>
        protected internal override void Initialize(ContentTypeReaderManager manager)
		{
			Type readerType = typeof(T);
			elementReader = manager.GetTypeReader(readerType);
        }

        /// <summary/>
        protected internal override T[] Read(ContentReader input, T[] existingInstance)
        {
            uint count = input.ReadUInt32();
            T[] array = existingInstance;
            if (array == null)
                array = new T[count];

            if (ReflectionHelpers.IsValueType(typeof(T)))
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
                    var readerType = input.Read7BitEncodedInt();
                	array[i] = readerType > 0 ? input.ReadObject<T>(input.TypeReaders[readerType - 1]) : default(T);
                }
			}
            return array;
        }
    }
}
