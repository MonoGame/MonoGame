// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

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
