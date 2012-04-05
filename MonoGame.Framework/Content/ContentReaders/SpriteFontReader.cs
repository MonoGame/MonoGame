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

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class SpriteFontReader : ContentTypeReader<SpriteFont>
    {
        internal SpriteFontReader()
        {
        }

        static string[] supportedExtensions = new string[] { ".spritefont" };

        internal static string Normalize(string fileName)
        {
            return Normalize(fileName, supportedExtensions);
        }
	
        protected internal override SpriteFont Read(ContentReader input, SpriteFont existingInstance)
        {
            Texture2D texture = input.ReadObject<Texture2D>();
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
