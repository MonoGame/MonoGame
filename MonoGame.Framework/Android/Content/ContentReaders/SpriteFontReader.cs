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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Util;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Microsoft.Xna.Framework.Content
{
    internal class SpriteFontReader : ContentTypeReader<SpriteFont>
    {
        internal SpriteFontReader()
        {
        }

		public static string Normalize(string FileName)
		{
            int index = FileName.LastIndexOf(Path.DirectorySeparatorChar);
            string path = string.Empty;
            string file = FileName;
            if (index >= 0)
            {
                file = FileName.Substring(index + 1, FileName.Length - index - 1);
                path = FileName.Substring(0, index);
            }
            string[] files = Game.Activity.Assets.List(path);

            if (Contains(file, files))
                return FileName;
			
			// Check the file extension
			if (!string.IsNullOrEmpty(Path.GetExtension(FileName)))
			{
				return null;
			}
		    
            return Path.Combine(path, TryFindAnyCased(file, files, ".spritefont", ".xnb"));
		}

        private static string TryFindAnyCased(string search, string[] arr, params string[] extensions)
        {
            return arr.FirstOrDefault(s => extensions.Any(ext => s.ToLower() == (search.ToLower() + ext)));
        }

        private static bool Contains(string search, string[] arr)
        {
            return arr.Any(s => s == search);
        }

        protected internal override SpriteFont Read(ContentReader input, SpriteFont existingInstance)
        {
            Texture2D texture = input.ReadObject<Texture2D>();
			texture.IsSpriteFontTexture = true;
            List<Rectangle> glyphs = input.ReadObject<List<Rectangle>>();
            List<Rectangle> cropping = input.ReadObject<List<Rectangle>>();
            List<char> charMap = input.ReadObject<List<char>>();
            int lineSpacing = input.ReadInt32();
            float spacing = input.ReadSingle();
            List<Vector3> kerning = input.ReadObject<List<Vector3>>();
            char? defaultCharacter = null;
            if (input.ReadBoolean())
            {
                defaultCharacter = new char?(input.ReadChar());
            }
            return new SpriteFont(texture, glyphs, cropping, charMap, lineSpacing, spacing, kerning, defaultCharacter);
        }
    }
}
