// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// This type is not meant to be used directly by MonoGame users.
    /// Its purpose is to allow to work-around AOT issues when loading assets with the ContentManager fail due to the absence of runtime-reflection support in that context (i.e. missing types due to trimming and inability to statically discover them at compile-time).
    /// If ContentManager.Load() throws an NotSupportedExeception, the message should provide insights on how to fix it.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All)]
    public class ListReader<T> : ContentTypeReader<List<T>>
    {
        ContentTypeReader elementReader;

        /// <summary/>
        public ListReader()
        {
        }

        /// <summary/>
        protected internal override void Initialize(ContentTypeReaderManager manager)
        {
			Type readerType = typeof(T);
			elementReader = manager.GetTypeReader(readerType);
        }

        /// <summary/>
        public override bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

        /// <summary/>
        protected internal override List<T> Read(ContentReader input, List<T> existingInstance)
        {
            int count = input.ReadInt32();
            List<T> list = existingInstance;
            if (list == null) list = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                if (ReflectionHelpers.IsValueType(typeof(T)))
				{
                	list.Add(input.ReadObject<T>(elementReader));
				}
				else
				{
                    var readerType = input.Read7BitEncodedInt();
                	list.Add(readerType > 0 ? input.ReadObject<T>(input.TypeReaders[readerType - 1]) : default(T));
				}
            }
            return list;
        }
    }
}
