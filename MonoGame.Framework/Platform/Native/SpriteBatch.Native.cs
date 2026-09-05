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
            throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
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
            throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
        }

        dynamicSpriteFont.EnsureGlyphs(text);
    }

    private void DrawString(DynamicSpriteFont dynamicSpriteFont, ref FontCharacterSource text, Vector2 position, Color color)
    {
        PreparedTextFont preparedTextFont = dynamicSpriteFont.GetCurrentPreparedTextFont();
        DrawPreparedText(preparedTextFont, ref text, position, color);
    }

    private void DrawString(DynamicSpriteFont dynamicSpriteFont,
                           ref FontCharacterSource text,
                           Vector2 position,
                           Color color,
                           float rotation,
                           Vector2 origin,
                           Vector2 scale,
                           SpriteEffects effects,
                           float layerDepth,
                           bool rtl)
    {
        PreparedTextFont preparedTextFont = dynamicSpriteFont.GetCurrentPreparedTextFont();
        DrawPreparedText(preparedTextFont, ref text, position, color, rotation, origin, scale, effects, layerDepth, rtl);
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
        FontCharacterSource source = new FontCharacterSource(text);
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
        FontCharacterSource source = new FontCharacterSource(text);
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
        FontCharacterSource source = new FontCharacterSource(text);
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
        FontCharacterSource source = new FontCharacterSource(text);
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
        FontCharacterSource source = new FontCharacterSource(text);
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
        FontCharacterSource source = new FontCharacterSource(text);
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
        FontCharacterSource source = new FontCharacterSource(text);
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
        FontCharacterSource source = new FontCharacterSource(text);
        DrawString(dynamicSpriteFont, ref source, position, color, rotation, origin, scale, effects, layerDepth, rtl);
    }
}
