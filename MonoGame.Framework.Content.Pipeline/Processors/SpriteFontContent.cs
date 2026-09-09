// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics;

public class SpriteFontContent
{
    public SpriteFontContent(FontDescription? desc = null)
    {
        if (desc != null)
        {
            FontName = desc.FontName;
            Style = desc.Style;
            FontSize = desc.Size;
            CharacterMap = new List<char>(desc.Characters.Count);
            VerticalLineSpacing = (int)desc.Spacing; // Will be replaced in the pipeline.
            HorizontalSpacing = desc.Spacing;
            DefaultCharacter = desc.DefaultCharacter;
        }
    }

    public string FontName { get; init; } = string.Empty;

    public FontDescriptionStyle Style { get; init; } = FontDescriptionStyle.Regular;

    public float FontSize { get; init; }

    public Texture2DContent Texture { get; init; } = new();

    public List<Rectangle> Glyphs { get; init; } = [];

    public List<Rectangle> Cropping { get; init; } = [];

    public List<char> CharacterMap { get; init; } = [];

    public int VerticalLineSpacing { get; set; }

    public float HorizontalSpacing { get; init; }

    public List<Vector3> Kerning { get; init; } = [];

    public char? DefaultCharacter { get; init; }
}
