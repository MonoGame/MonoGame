// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics;

public class SpriteFontContent
{
    public string FontName = string.Empty;

    FontDescriptionStyle Style = FontDescriptionStyle.Regular;

    public float FontSize;

    public Texture2DContent Texture = new Texture2DContent();

    public List<Rectangle> Glyphs = new List<Rectangle>();

    public List<Rectangle> Cropping = new List<Rectangle>();

    public List<char> CharacterMap = new List<char>();

    public int VerticalLineSpacing;

    public float HorizontalSpacing;

    public List<Vector3> Kerning = new List<Vector3>();

    public Nullable<char> DefaultCharacter;

    // Distance field metadata (internal use). Defaults keep classic bitmap behavior.
    public byte DistanceFieldType = 0; // 0=None,1=SDF
    public float DistanceFieldSpread = 0f; // in texels
    public float DistanceFieldEmSize = 0f; // design size baseline

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