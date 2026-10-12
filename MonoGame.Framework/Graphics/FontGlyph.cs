// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Returns the texture bounds and spacing data for a single rendered glyph.
/// </summary>
internal struct FontGlyph
{
    /// <summary>
    /// Returns an empty glyph.
    /// </summary>
    public static readonly FontGlyph Empty = new FontGlyph();

    /// <summary>
    /// The rectangle in the font texture that contains this glyph.
    /// </summary>
    public Rectangle BoundsInTexture;

    /// <summary>
    /// The character associated with this glyph.
    /// </summary>
    public char Character;

    /// <summary>
    /// The rasterized font sized used to bake this glyph, in pixels.
    /// </summary>
    public int Size;

    /// <summary>
    /// The atlas page that owns this glyph.
    /// </summary>
    public int PageIndex;

    /// <summary>
    /// The cropping rectangle applied when positioning this glyph for drawing.
    /// </summary>
    public Rectangle Cropping;

    /// <summary>
    /// The amount of space between the left side of the glyph layout box and its first pixel.
    /// </summary>
    public float LeftSideBearing;

    /// <summary>
    /// The amount of space between the right side of the glyph layout box and its last pixel.
    /// </summary>
    public float RightSideBearing;

    /// <summary>
    /// The width of the glyph image before side bearings are applied.
    /// </summary>
    public float Width;

    /// <summary>
    /// The total horizontal advance represented by the left bearing, width, and right bearing.
    /// </summary>
    public float WidthIncludingBearings;
}
