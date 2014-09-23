using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Linq;

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
            Effect effect = existingInstance;
            
            var dataCount = (int)input.ReadUInt32();
            var data = input.ReadBytes(dataCount);

            if (effect == null)
            {
                effect = new Effect(input.GraphicsDevice, data);
            }            
            
            effect.Name = input.AssetName;            
            return effect;
        }
    }
}