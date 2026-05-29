// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics;

public partial class SpriteBatch : GraphicsResource
{
    private void CheckValid(DynamicSpriteFont dynamicSpriteFont, string text)
    {
        if (dynamicSpriteFont == null)
        {
            throw new ArgumentNullException(nameof(dynamicSpriteFont), $"{nameof(dynamicSpriteFont)} must not be null.");
        }

        if (text == null)
        {
            throw new ArgumentNullException(nameof(text), $"{nameof(text)} must not be null.");
        }

        if (!_beginCalled)
        {
            throw new InvalidOperationException("DrawString was called, But Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
        }

        dynamicSpriteFont.EnsureGlyphs(text);
    }

    private void CheckValid(DynamicSpriteFont dynamicSpriteFont, StringBuilder text)
    {
        if (dynamicSpriteFont == null)
        {
            throw new ArgumentNullException(nameof(dynamicSpriteFont), $"{nameof(dynamicSpriteFont)} must not be null.");
        }

        if (text == null)
        {
            throw new ArgumentNullException(nameof(text), $"{nameof(text)} must not be null.");
        }

        if (!_beginCalled)
        {
            throw new InvalidOperationException("DrawString was called, But Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
        }

        dynamicSpriteFont.EnsureGlyphs(text);
    }

    private unsafe void DrawString(DynamicSpriteFont dynamicSpriteFont, ref DynamicSpriteFont.DynamicSpriteFontCharacterSource text, Vector2 position, Color color)
    {
        DynamicSpriteFont.DynamicSpriteFontSizeData sizeData = dynamicSpriteFont.GetCurrentSizeData();
        float sortKey = (_sortMode == SpriteSortMode.Texture) ? sizeData.Texture.SortingKey : 0;

        Vector2 offset = Vector2.Zero;
        bool firstGlyphOfLine = true;

        fixed (DynamicSpriteFont.DynamicSpriteFontGlyph* pGlyphs = sizeData.Glyphs)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\r')
                {
                    continue;
                }

                if (c == '\n')
                {
                    offset.X = 0;
                    offset.Y += sizeData.LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }

                int currentGlyphIndex = sizeData.GetGlyphIndexOrDefault(c);
                DynamicSpriteFont.DynamicSpriteFontGlyph* pCurrentGlyph = pGlyphs + currentGlyphIndex;

                if (firstGlyphOfLine)
                {
                    offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                }
                else
                {
                    offset.X += sizeData.Spacing + pCurrentGlyph->LeftSideBearing;
                }

                Vector2 p = offset;
                p.X += pCurrentGlyph->Cropping.X;
                p.Y += pCurrentGlyph->Cropping.Y;
                p += position;

                SpriteBatchItem item = _batcher.CreateBatchItem();
                item.Texture = sizeData.Texture;
                item.SortKey = sortKey;

                _texCoordTL.X = pCurrentGlyph->BoundsInTexture.X * sizeData.Texture.TexelWidth;
                _texCoordTL.Y = pCurrentGlyph->BoundsInTexture.Y * sizeData.Texture.TexelHeight;
                _texCoordBR.X = (pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * sizeData.Texture.TexelWidth;
                _texCoordBR.Y = (pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * sizeData.Texture.TexelHeight;

                item.Set(p.X,
                         p.Y,
                         pCurrentGlyph->BoundsInTexture.Width,
                         pCurrentGlyph->BoundsInTexture.Height,
                         color,
                         _texCoordTL,
                         _texCoordBR,
                         0);

                offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
            }
        }

        FlushIfNeeded();
    }

    unsafe void DrawString(DynamicSpriteFont dynamicSpriteFont,
                           ref DynamicSpriteFont.DynamicSpriteFontCharacterSource text,
                           Vector2 position,
                           Color color,
                           float rotation,
                           Vector2 origin,
                           Vector2 scale,
                           SpriteEffects effects,
                           float layerDepth,
                           bool rtl)
    {
        DynamicSpriteFont.DynamicSpriteFontSizeData sizeData = dynamicSpriteFont.GetCurrentSizeData();
        float sortKey = 0;
        switch (_sortMode)
        {
            case SpriteSortMode.Texture:
                sortKey = sizeData.Texture.SortingKey;
                break;
            case SpriteSortMode.FrontToBack:
                sortKey = layerDepth;
                break;
            case SpriteSortMode.BackToFront:
                sortKey = -layerDepth;
                break;
        }

        Vector2 flipAdjustment = Vector2.Zero;
        bool flippedVert = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
        bool flippedHorz = ((effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally) ^ rtl;

        if (flippedVert || flippedHorz || rtl)
        {
            Vector2 size = sizeData.MeasureString(ref text);

            if (flippedHorz ^ rtl)
            {
                origin.X *= -1;
                flipAdjustment.X = -size.X;
            }

            if (flippedVert)
            {
                origin.Y *= -1;
                flipAdjustment.Y = sizeData.LineSpacing - size.Y;
            }
        }

        Matrix transformation = Matrix.Identity;
        float cos = 0;
        float sin = 0;
        if (rotation == 0)
        {
            transformation.M11 = (flippedHorz ? -scale.X : scale.X);
            transformation.M22 = (flippedVert ? -scale.Y : scale.Y);
            transformation.M41 = ((flipAdjustment.X - origin.X) * transformation.M11) + position.X;
            transformation.M42 = ((flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y;
        }
        else
        {
            cos = MathF.Cos(rotation);
            sin = MathF.Sin(rotation);
            transformation.M11 = (flippedHorz ? -scale.X : scale.X) * cos;
            transformation.M12 = (flippedHorz ? -scale.X : scale.X) * sin;
            transformation.M21 = (flippedVert ? -scale.Y : scale.Y) * (-sin);
            transformation.M22 = (flippedVert ? -scale.Y : scale.Y) * cos;
            transformation.M41 = (((flipAdjustment.X - origin.X) * transformation.M11) + (flipAdjustment.Y - origin.Y) * transformation.M21) + position.X;
            transformation.M42 = (((flipAdjustment.X - origin.X) * transformation.M12) + (flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y;
        }

        Vector2 offset = Vector2.Zero;
        bool firstGlyphOfLine = true;

        fixed (DynamicSpriteFont.DynamicSpriteFontGlyph* pGlyphs = sizeData.Glyphs)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    offset.X = 0;
                    offset.Y += sizeData.LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }

                int currentGlyphIndex = sizeData.GetGlyphIndexOrDefault(c);
                DynamicSpriteFont.DynamicSpriteFontGlyph* pCurrentGlyph = pGlyphs + currentGlyphIndex;

                if (firstGlyphOfLine)
                {
                    offset.X = Math.Max(rtl ? pCurrentGlyph->RightSideBearing : pCurrentGlyph->LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                }
                else
                {
                    offset.X += sizeData.Spacing + (rtl ? pCurrentGlyph->RightSideBearing : pCurrentGlyph->LeftSideBearing);
                }

                Vector2 p = offset;

                if (flippedHorz)
                    p.X += pCurrentGlyph->BoundsInTexture.Width;
                p.X += pCurrentGlyph->Cropping.X;

                if (flippedVert)
                    p.Y += pCurrentGlyph->BoundsInTexture.Height - sizeData.LineSpacing;
                p.Y += pCurrentGlyph->Cropping.Y;

                Vector2.Transform(ref p, ref transformation, out p);

                SpriteBatchItem item = _batcher.CreateBatchItem();
                item.Texture = sizeData.Texture;
                item.SortKey = sortKey;

                _texCoordTL.X = pCurrentGlyph->BoundsInTexture.X * sizeData.Texture.TexelWidth;
                _texCoordTL.Y = pCurrentGlyph->BoundsInTexture.Y * sizeData.Texture.TexelHeight;
                _texCoordBR.X = (pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * sizeData.Texture.TexelWidth;
                _texCoordBR.Y = (pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * sizeData.Texture.TexelHeight;

                if ((effects & SpriteEffects.FlipVertically) != 0)
                {
                    float temp = _texCoordBR.Y;
                    _texCoordBR.Y = _texCoordTL.Y;
                    _texCoordTL.Y = temp;
                }

                if ((effects & SpriteEffects.FlipHorizontally) != 0)
                {
                    float temp = _texCoordBR.X;
                    _texCoordBR.X = _texCoordTL.X;
                    _texCoordTL.X = temp;
                }

                if (rotation == 0f)
                {
                    item.Set(p.X,
                             p.Y,
                             pCurrentGlyph->BoundsInTexture.Width * scale.X,
                             pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                             color,
                             _texCoordTL,
                             _texCoordBR,
                             layerDepth);
                }
                else
                {
                    item.Set(p.X,
                             p.Y,
                             0,
                             0,
                             pCurrentGlyph->BoundsInTexture.Width * scale.X,
                             pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                             sin,
                             cos,
                             color,
                             _texCoordTL,
                             _texCoordBR,
                             layerDepth);
                }

                offset.X += pCurrentGlyph->Width + (rtl ? pCurrentGlyph->LeftSideBearing : pCurrentGlyph->RightSideBearing);
            }
        }

        FlushIfNeeded();
    }

    /// <summary>
    /// Submits a text string of sprites for drawing with the current batch.
    /// </summary>
    /// <param name="dynamicSpriteFont">A runtime loaded font face.</param>
    /// <param name="text">The text which will be drawn.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    public void DrawString(DynamicSpriteFont dynamicSpriteFont, string text, Vector2 position, Color color)
    {
        CheckValid(dynamicSpriteFont, text);
        DynamicSpriteFont.DynamicSpriteFontCharacterSource source = new DynamicSpriteFont.DynamicSpriteFontCharacterSource(text);
        DrawString(dynamicSpriteFont, ref source, position, color);
    }

    /// <summary>
    /// Submit a text string of sprites for drawing in the current batch.
    /// </summary>
    /// <param name="dynamicSpriteFont">A runtime-loaded font face.</param>
    /// <param name="text">The text which will be drawn.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this string.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this string.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this string.</param>        
    public void DrawString(DynamicSpriteFont dynamicSpriteFont,
                           string text,
                           Vector2 position,
                           Color color,
                           float rotation,
                           Vector2 origin,
                           float scale,
                           SpriteEffects effects,
                           float layerDepth)
    {
        CheckValid(dynamicSpriteFont, text);
        DynamicSpriteFont.DynamicSpriteFontCharacterSource source = new DynamicSpriteFont.DynamicSpriteFontCharacterSource(text);
        DrawString(dynamicSpriteFont, ref source, position, color, rotation, origin, new Vector2(scale, scale), effects, layerDepth, false);
    }

    /// <summary>
    /// Submit a text string of sprites for drawing in the current batch.
    /// </summary>
    /// <param name="dynamicSpriteFont">A runtime-loaded font face.</param>
    /// <param name="text">The text which will be drawn.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this string.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this string.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this string.</param>        
    public void DrawString(DynamicSpriteFont dynamicSpriteFont,
                           string text,
                           Vector2 position,
                           Color color,
                           float rotation,
                           Vector2 origin,
                           Vector2 scale,
                           SpriteEffects effects,
                           float layerDepth)
    {
        CheckValid(dynamicSpriteFont, text);
        DynamicSpriteFont.DynamicSpriteFontCharacterSource source = new DynamicSpriteFont.DynamicSpriteFontCharacterSource(text);
        DrawString(dynamicSpriteFont, ref source, position, color, rotation, origin, scale, effects, layerDepth, false);
    }

    /// <summary>
    /// Submit a text string of sprites for drawing in the current batch.
    /// </summary>
    /// <param name="dynamicSpriteFont">A runtime-loaded font face.</param>
    /// <param name="text">The text which will be drawn.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this string.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this string.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this string.</param>
    /// <param name="rtl">Text is Right to Left.</param>
    public void DrawString(DynamicSpriteFont dynamicSpriteFont,
                           string text,
                           Vector2 position,
                           Color color,
                           float rotation,
                           Vector2 origin,
                           Vector2 scale,
                           SpriteEffects effects,
                           float layerDepth,
                           bool rtl)
    {
        CheckValid(dynamicSpriteFont, text);
        DynamicSpriteFont.DynamicSpriteFontCharacterSource source = new DynamicSpriteFont.DynamicSpriteFontCharacterSource(text);
        DrawString(dynamicSpriteFont, ref source, position, color, rotation, origin, scale, effects, layerDepth, rtl);
    }

    /// <summary>
    /// Submit a text string of sprites for drawing in the current batch.
    /// </summary>
    /// <param name="dynamicSpriteFont">A runtime-loaded font face.</param>
    /// <param name="text">The text which will be drawn.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    public void DrawString(DynamicSpriteFont dynamicSpriteFont, StringBuilder text, Vector2 position, Color color)
    {
        CheckValid(dynamicSpriteFont, text);
        DynamicSpriteFont.DynamicSpriteFontCharacterSource source = new DynamicSpriteFont.DynamicSpriteFontCharacterSource(text);
        DrawString(dynamicSpriteFont, ref source, position, color);
    }

    /// <summary>
    /// Submit a text string of sprites for drawing in the current batch.
    /// </summary>
    /// <param name="dynamicSpriteFont">A runtime-loaded font face.</param>
    /// <param name="text">The text which will be drawn.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this string.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this string.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this string.</param>
    public void DrawString(DynamicSpriteFont dynamicSpriteFont,
                           StringBuilder text,
                           Vector2 position,
                           Color color,
                           float rotation,
                           Vector2 origin,
                           float scale,
                           SpriteEffects effects,
                           float layerDepth)
    {
        CheckValid(dynamicSpriteFont, text);
        DynamicSpriteFont.DynamicSpriteFontCharacterSource source = new DynamicSpriteFont.DynamicSpriteFontCharacterSource(text);
        DrawString(dynamicSpriteFont, ref source, position, color, rotation, origin, new Vector2(scale, scale), effects, layerDepth, false);
    }

    /// <summary>
    /// Submit a text string of sprites for drawing in the current batch.
    /// </summary>
    /// <param name="dynamicSpriteFont">A runtime-loaded font face.</param>
    /// <param name="text">The text which will be drawn.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this string.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this string.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this string.</param>
    public void DrawString(DynamicSpriteFont dynamicSpriteFont,
                           StringBuilder text,
                           Vector2 position,
                           Color color,
                           float rotation,
                           Vector2 origin,
                           Vector2 scale,
                           SpriteEffects effects,
                           float layerDepth)
    {
        CheckValid(dynamicSpriteFont, text);
        DynamicSpriteFont.DynamicSpriteFontCharacterSource source = new DynamicSpriteFont.DynamicSpriteFontCharacterSource(text);
        DrawString(dynamicSpriteFont, ref source, position, color, rotation, origin, scale, effects, layerDepth, false);
    }

    /// <summary>
    /// Submit a text string of sprites for drawing in the current batch.
    /// </summary>
    /// <param name="dynamicSpriteFont">A runtime-loaded font face.</param>
    /// <param name="text">The text which will be drawn.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this string.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this string.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this string.</param>
    /// <param name="rtl">Text is Right to Left.</param>
    public void DrawString(DynamicSpriteFont dynamicSpriteFont,
                           StringBuilder text,
                           Vector2 position,
                           Color color,
                           float rotation,
                           Vector2 origin,
                           Vector2 scale,
                           SpriteEffects effects,
                           float layerDepth,
                           bool rtl)
    {
        CheckValid(dynamicSpriteFont, text);
        DynamicSpriteFont.DynamicSpriteFontCharacterSource source = new DynamicSpriteFont.DynamicSpriteFontCharacterSource(text);
        DrawString(dynamicSpriteFont, ref source, position, color, rotation, origin, scale, effects, layerDepth, rtl);
    }
}