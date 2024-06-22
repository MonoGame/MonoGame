// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Class to provide methods and properties for processing sprite fonts.
    /// </summary>
	public class SpriteFontContent
    {
        /// <summary>
        /// Creates a new instance of the SpriteFontContent class.
        /// </summary>
        public SpriteFontContent() { }

        /// <summary>
        /// Creates a new instance of the SpriteFontContent class.
        /// </summary>
        /// <param name="desc">Font description.</param>
        public SpriteFontContent(FontDescription desc)
        {
            FontName = desc.FontName;
            Style = desc.Style;
            FontSize = desc.Size;
            CharacterMap = new List<char>(desc.Characters.Count);
            VerticalLineSpacing = (int)desc.Spacing; // Will be replaced in the pipeline.
            HorizontalSpacing = desc.Spacing;

            DefaultCharacter = desc.DefaultCharacter;
        }

        /// <summary>
        /// Get or set the name of the font.
        /// </summary>
        public string FontName = string.Empty;

        FontDescriptionStyle Style = FontDescriptionStyle.Regular;

        /// <summary>
        /// Get or set the font size.
        /// </summary>
        public float FontSize;

        /// <summary>
        /// Get or set the fonts texture.
        /// </summary>
        public Texture2DContent Texture = new Texture2DContent();

        /// <summary>
        /// Gets or sets the list of glyphs.
        /// </summary>
        public List<Rectangle> Glyphs = new List<Rectangle>();

        /// <summary>
        /// Gets or sets the list of cropping rectangles.
        /// </summary>
        public List<Rectangle> Cropping = new List<Rectangle>();

        /// <summary>
        /// Gets or sets the character map list.
        /// </summary>
        public List<Char> CharacterMap = new List<Char>();

        /// <summary>
        /// Get or set the vertical line spacing.
        /// </summary>
        public int VerticalLineSpacing;

        /// <summary>
        /// Get or set the horizontal line spacing.
        /// </summary>
        public float HorizontalSpacing;

        /// <summary>
        /// Get or set the kerning list.
        /// </summary>
        public List<Vector3> Kerning = new List<Vector3>();

        /// <summary>
        /// Get or set the default character.
        /// </summary>
        public Nullable<Char> DefaultCharacter;

    }
}
