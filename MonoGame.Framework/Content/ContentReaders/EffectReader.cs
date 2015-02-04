// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Linq;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    internal class EffectReader : ContentTypeReader<Effect>
    {
        public EffectReader()
        {
        }
		static string [] supportedExtensions = new string[]  {".fxg"};
		
		public static string Normalize(string FileName)
		{
            return ContentTypeReader.Normalize(FileName, supportedExtensions);
		}
		
		private static string TryFindAnyCased(string search, string[] arr, params string[] extensions)
		{
			return arr.FirstOrDefault(s => extensions.Any(ext => s.ToLowerInvariant() == (search.ToLowerInvariant() + ext)));
		}
		
		private static bool Contains(string search, string[] arr)
		{
			return arr.Any(s => s == search);
		}

        protected internal override Effect Read(ContentReader input, Effect existingInstance)
        {
            int dataSize = input.ReadInt32();
            byte[] data = MemoryPool.Current.GetPooledBuffer(dataSize);
            input.Read(data, 0, dataSize);
            var effect = new Effect(input.GraphicsDevice, data, 0, dataSize); 
            MemoryPool.Current.PoolBuffer(data);

            effect.Name = input.AssetName;
       
            return effect;
        }
    }
}