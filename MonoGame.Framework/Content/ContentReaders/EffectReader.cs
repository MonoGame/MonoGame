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
        private static EffectPool effectpool;

        public EffectReader()
        {
        }
		static string [] supportedExtensions = new string[]  {".fxg"};
		
		public static string Normalize(string FileName)
		{
			if (File.Exists(FileName))
				return FileName;
			
			// Check the file extension
			if (!string.IsNullOrEmpty(Path.GetExtension(FileName)))
			{
				return null;
			}
			
			// Concat the file name with valid extensions
			foreach (var item in supportedExtensions)
			{
				if (File.Exists(FileName+item))
				  return FileName+item;
			}
			
			return null;
		}
		
		private static string TryFindAnyCased(string search, string[] arr, params string[] extensions)
		{
			return arr.FirstOrDefault(s => extensions.Any(ext => s.ToLower() == (search.ToLower() + ext)));
		}
		
		private static bool Contains(string search, string[] arr)
		{
			return arr.Any(s => s == search);
		}

        protected internal override Effect Read(ContentReader input, Effect existingInstance)
        {
            int count = input.ReadInt32();
            
            Effect effect = new Effect(input.GraphicsDevice,input.ReadBytes(count));
            effect.Name = input.AssetName;
            
            return effect;
        }
    }
}