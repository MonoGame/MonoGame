using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class SpriteFontContent
    {
        public string FontName = string.Empty;

        public string Style = string.Empty;

        public float FontSize;

        public Texture2DContent Texture = new Texture2DContent();

        public List<Rectangle> Glyphs = new List<Rectangle>();

        public List<Rectangle> Cropping = new List<Rectangle>();

        public List<Char> CharacterMap = new List<Char>();

        public int VerticalLineSpacing;

        public float HorizontalSpacing;

        public List<Vector3> Kerning = new List<Vector3>();

        public Nullable<Char> DefaultCharacter;	 

    }
}
