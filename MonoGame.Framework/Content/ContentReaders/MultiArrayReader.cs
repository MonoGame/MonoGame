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
    public class MultiArrayReader<T> : ContentTypeReader<Array>
    {
        ContentTypeReader elementReader;

        /// <summary/>
        public MultiArrayReader() { }

        /// <summary/>
        protected internal override void Initialize(ContentTypeReaderManager manager)
        {
            Type readerType = typeof(T);
            elementReader = manager.GetTypeReader(readerType);
        }

        /// <summary/>
        protected internal override Array Read(ContentReader input, Array existingInstance)
        {
            var rank = input.ReadInt32();
            if (rank < 1)
                throw new RankException();

            var dimensions = new int[rank];
            var count = 1;
            for (int d = 0; d < dimensions.Length; d++)
                count *= dimensions[d] = input.ReadInt32();


            var array = existingInstance;
            if (array == null)
                array = Array.CreateInstance(typeof(T), dimensions);//new T[count];
            else if (dimensions.Length != array.Rank)
                throw new RankException("existingInstance");

            var indices = new int[rank];

            for (int i = 0; i < count; i++)
            {
                T value;
                if (ReflectionHelpers.IsValueType(typeof(T)))
                    value = input.ReadObject<T>(elementReader);
                else
                {
                    var readerType = input.Read7BitEncodedInt();
                    if (readerType > 0)
                        value = input.ReadObject<T>(input.TypeReaders[readerType - 1]);
                    else
                        value = default(T);
                }

                CalcIndices(array, i, indices);
                array.SetValue(value, indices);
            }

            return array;
        }

        static void CalcIndices(Array array, int index, int[] indices)
        {
            if (array.Rank != indices.Length)
                throw new Exception("indices");

            for (int d = 0; d < indices.Length; d++)
            {
                if (index == 0)
                    indices[d] = 0;
                else
                {
                    indices[d] = index % array.GetLength(d);
                    index /= array.GetLength(d);
                }
            }

            if (index != 0)
                throw new ArgumentOutOfRangeException("index");
        }
    }
}
