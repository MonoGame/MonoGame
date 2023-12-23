#nullable enable

using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
///     Helper class for drawing text strings and sprites in one or more optimized batches.
/// </summary>
public interface ISpriteBatch : IDisposable
{
    /// <summary>
    ///     Begins a new sprite and text batch with the specified render state.
    /// </summary>
    /// <param name="sortMode">The drawing order for sprite and text drawing.Deferred by default.</param>
    /// <param name="blendState">State of the blending. Uses AlphaBlend if null.</param>
    /// <param name="samplerState">State of the sampler. Uses LinearClamp if null.</param>
    /// <param name="depthStencilState">State of the depth-stencil buffer. Uses None if null.</param>
    /// <param name="rasterizerState">State of the rasterization. Uses CullCounterClockwise if null.</param>
    /// <param name="effect">A custom Effect to override the default sprite effect. Uses default sprite effect if null.</param>
    /// <param name="transformMatrix">An optional matrix used to transform the sprite geometry. Uses Identity if null.</param>
    void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Effect? effect = null, Matrix? transformMatrix = null);

    /// <summary>
    ///     Flushes the sprite batch and restores the device state to how it was before <see cref="Begin" /> was called.
    /// </summary>
    /// <remarks>Call End after all calls to Draw are complete.</remarks>
    void End();

    /// <summary>
    ///     Adds a sprite to the batch of sprites to be rendered, specifying the texture, destination rectangle, and color
    ///     tint.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    /// <param name="destinationRectangle">A rectangle specifying, in screen coordinates, where the sprite will be drawn.</param>
    /// <param name="color">The color channel modulation to use. Use <see cref="Color.White" /> for full color with no tinting.</param>
    void Draw(Texture2D texture, Rectangle destinationRectangle, Color color);

    /// <summary>
    ///     Adds a sprite to the batch of sprites to be rendered, specifying the texture, destination rectangle, and color
    ///     tint.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    /// <param name="destinationRectangle">A rectangle specifying, in screen coordinates, where the sprite will be drawn.</param>
    /// <param name="color">The color channel modulation to use. Use <see cref="Color.White" /> for full color with no tinting.</param>
    void Draw(Texture2D texture, RectangleF destinationRectangle, Color color);

    /// <summary>
    ///     Adds a sprite to the batch of sprites to be rendered, specifying the texture, destination and source rectangles,
    ///     and color tint.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    /// <param name="destinationRectangle">
    ///     A rectangle specifying, in screen coordinates, where the sprite will be drawn. If
    ///     this rectangle is not the same size as <paramref name="sourceRectangle" /> the sprite will be scaled to fit.
    /// </param>
    /// <param name="sourceRectangle">
    ///     A rectangle specifying, in texels, which section of the rectangle to draw. Use null to
    ///     draw the entire texture.
    /// </param>
    /// <param name="color">The color channel modulation to use. Use Color.White for full color with no tinting.</param>
    void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color);

    /// <summary>
    ///     Adds a sprite to the batch of sprites to be rendered, specifying the texture, destination and source rectangles,
    ///     and color tint.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    /// <param name="destinationRectangle">
    ///     A rectangle specifying, in screen coordinates, where the sprite will be drawn. If
    ///     this rectangle is not the same size as <paramref name="sourceRectangle" /> the sprite will be scaled to fit.
    /// </param>
    /// <param name="sourceRectangle">
    ///     A rectangle specifying, in texels, which section of the rectangle to draw. Use null to
    ///     draw the entire texture.
    /// </param>
    /// <param name="color">The color channel modulation to use. Use Color.White for full color with no tinting.</param>
    void Draw(Texture2D texture, RectangleF destinationRectangle, Rectangle? sourceRectangle, Color color);

    /// <summary>
    ///     Adds a sprite to the batch of sprites to be rendered, specifying the texture, destination and source rectangles,
    ///     color tint, rotation, origin, effects, and sort depth.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    /// <param name="destinationRectangle">
    ///     A rectangle specifying, in screen coordinates, where the sprite will be drawn. If
    ///     this rectangle is not the same size as <paramref name="sourceRectangle"/>, the sprite is scaled to fit.
    /// </param>
    /// <param name="sourceRectangle">
    ///     A rectangle specifying, in texels, which section of the rectangle to draw. Use null to
    ///     draw the entire texture.
    /// </param>
    /// <param name="color">The color channel modulation to use. Use Color.White for full color with no tinting.</param>
    /// <param name="rotation">The angle, in radians, to rotate the sprite around the origin.</param>
    /// <param name="origin">The origin of the sprite. Specify (0,0) for the upper-left corner.</param>
    /// <param name="effects">Rotations to apply prior to rendering.</param>
    /// <param name="layerDepth">The sorting depth of the sprite, between 0 (front) and 1 (back).</param>
    void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth);

    /// <summary>
    ///     Adds a sprite to the batch of sprites to be rendered, specifying the texture, destination and source rectangles,
    ///     color tint, rotation, origin, effects, and sort depth.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    /// <param name="destinationRectangle">
    ///     A rectangle specifying, in screen coordinates, where the sprite will be drawn. If
    ///     this rectangle is not the same size as <paramref name="sourceRectangle"/>, the sprite is scaled to fit.
    /// </param>
    /// <param name="sourceRectangle">
    ///     A rectangle specifying, in texels, which section of the rectangle to draw. Use null to
    ///     draw the entire texture.
    /// </param>
    /// <param name="color">The color channel modulation to use. Use Color.White for full color with no tinting.</param>
    /// <param name="rotation">The angle, in radians, to rotate the sprite around the origin.</param>
    /// <param name="origin">The origin of the sprite. Specify (0,0) for the upper-left corner.</param>
    /// <param name="effects">Rotations to apply prior to rendering.</param>
    /// <param name="layerDepth">The sorting depth of the sprite, between 0 (front) and 1 (back).</param>
    void Draw(Texture2D texture, RectangleF destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth);

    /// <summary>
    ///     Adds a sprite to the batch of sprites to be rendered, specifying the texture, screen position, and color tint.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    /// <param name="position">The location, in screen coordinates, where the sprite will be drawn.</param>
    /// <param name="color">The color channel modulation to use. Use <see cref="Color.White" /> for full color with no tinting.</param>
    void Draw(Texture2D texture, Vector2 position, Color color);

    /// <summary>
    ///     Adds a sprite to the batch of sprites to be rendered, specifying the texture, destination and source rectangles,
    ///     and color tint.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    /// <param name="position">
    ///     A rectangle specifying, in screen coordinates, where the sprite will be drawn. If this rectangle
    ///     is not the same size as <paramref name="sourceRectangle" /> the sprite will be scaled to fit.
    /// </param>
    /// <param name="sourceRectangle">
    ///     A rectangle specifying, in texels, which section of the rectangle to draw. Use null to
    ///     draw the entire texture.
    /// </param>
    /// <param name="color">The color channel modulation to use. Use <see cref="Color.White" /> for full color with no tinting.</param>
    void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color);

    /// <summary>
    ///     Adds a sprite to the batch of sprites to be rendered, specifying the texture, screen position, optional source
    ///     rectangle, color tint, rotation, origin, scale, effects, and sort depth.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    /// <param name="position">The location, in screen coordinates, where the sprite will be drawn.</param>
    /// <param name="sourceRectangle">
    ///     A rectangle specifying, in texels, which section of the rectangle to draw. Use null to
    ///     draw the entire texture.
    /// </param>
    /// <param name="color">The color channel modulation to use. Use Color.White for full color with no tinting.</param>
    /// <param name="rotation">The angle, in radians, to rotate the sprite around the origin.</param>
    /// <param name="origin">The origin of the sprite. Specify (0,0) for the upper-left corner.</param>
    /// <param name="scale">Uniform multiply by which to scale the sprite width and height.</param>
    /// <param name="effects">Rotations to apply prior to rendering.</param>
    /// <param name="layerDepth">The sorting depth of the sprite, between 0 (front) and 1 (back).</param>
    void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);

    /// <summary>
    ///     Adds a sprite to the batch of sprites to be rendered, specifying the texture, screen position, source rectangle,
    ///     color tint, rotation, origin, scale, effects, and sort depth.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    /// <param name="position">The location, in screen coordinates, where the sprite will be drawn.</param>
    /// <param name="sourceRectangle">
    ///     A rectangle specifying, in texels, which section of the rectangle to draw. Use null to
    ///     draw the entire texture.
    /// </param>
    /// <param name="color">The color channel modulation to use. Use Color.White for full color with no tinting.</param>
    /// <param name="rotation">The angle, in radians, to rotate the sprite around the origin.</param>
    /// <param name="origin">The origin of the sprite. Specify (0,0) for the upper-left corner.</param>
    /// <param name="scale">Vector containing separate scalar multiples for the x- and y-axes of the sprite.</param>
    /// <param name="effects">Rotations to apply before rendering.</param>
    /// <param name="layerDepth">The sorting depth of the sprite, between 0 (front) and 1 (back).</param>
    void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);

    /// <summary>
    ///     Adds a sprite string to the batch of sprites to be rendered, specifying the font, output text, screen position, and
    ///     color tint.
    /// </summary>
    /// <param name="spriteFont">The sprite font.</param>
    /// <param name="text">The string to draw.</param>
    /// <param name="position">The location, in screen coordinates, where the text will be drawn.</param>
    /// <param name="color">The desired color of the text.</param>
    void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color);

    /// <summary>
    ///     Adds a sprite string to the batch of sprites to be rendered, specifying the font, output text, screen position,
    ///     color tint, rotation, origin, scale, and effects.
    /// </summary>
    /// <param name="spriteFont">The sprite font.</param>
    /// <param name="text">The string to draw.</param>
    /// <param name="position">The location, in screen coordinates, where the text will be drawn.</param>
    /// <param name="color">The desired color of the text.</param>
    /// <param name="rotation">The angle, in radians, to rotate the text around the origin.</param>
    /// <param name="origin">The origin of the string. Specify (0,0) for the upper-left corner.</param>
    /// <param name="scale">Uniform multiply by which to scale the sprite width and height.</param>
    /// <param name="effects">Rotations to apply prior to rendering.</param>
    /// <param name="layerDepth">The sorting depth of the sprite, between 0 (front) and 1 (back).</param>
    void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);

    /// <summary>
    ///     Adds a sprite string to the batch of sprites to be rendered, specifying the font, output text, screen position,
    ///     color tint, rotation, origin, scale, effects, and depth.
    /// </summary>
    /// <param name="spriteFont">The sprite font.</param>
    /// <param name="text">The string to draw.</param>
    /// <param name="position">The location, in screen coordinates, where the text will be drawn.</param>
    /// <param name="color">The desired color of the text.</param>
    /// <param name="rotation">The angle, in radians, to rotate the text around the origin.</param>
    /// <param name="origin">The origin of the string. Specify (0,0) for the upper-left corner.</param>
    /// <param name="scale">Vector containing separate scalar multiples for the x- and y-axes of the sprite.</param>
    /// <param name="effects">Rotations to apply prior to rendering.</param>
    /// <param name="layerDepth">The sorting depth of the sprite, between 0 (front) and 1 (back).</param>
    void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);

    /// <summary>
    ///     Adds a sprite string to the batch of sprites to be rendered, specifying the font, output text, screen position, and
    ///     color tint.
    /// </summary>
    /// <param name="spriteFont">The sprite font.</param>
    /// <param name="text">The string to draw.</param>
    /// <param name="position">The location, in screen coordinates, where the text will be drawn.</param>
    /// <param name="color">The desired color of the text.</param>
    void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color);

    /// <summary>
    ///     Adds a sprite string to the batch of sprites to be rendered, specifying the font, output text, screen position,
    ///     color tint, rotation, origin, scale, and effects.
    /// </summary>
    /// <param name="spriteFont">The sprite font.</param>
    /// <param name="text">The string to draw.</param>
    /// <param name="position">The location, in screen coordinates, where the text will be drawn.</param>
    /// <param name="color">The desired color of the text.</param>
    /// <param name="rotation">The angle, in radians, to rotate the text around the origin.</param>
    /// <param name="origin">The origin of the string. Specify (0,0) for the upper-left corner.</param>
    /// <param name="scale">Uniform multiply by which to scale the sprite width and height.</param>
    /// <param name="effects">Rotations to apply prior to rendering.</param>
    /// <param name="layerDepth">The sorting depth of the sprite, between 0 (front) and 1 (back).</param>
    void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);

    /// <summary>
    ///     Adds a sprite string to the batch of sprites to be rendered, specifying the font, output text, screen position,
    ///     color tint, rotation, origin, scale, effects, and depth.
    /// </summary>
    /// <param name="spriteFont">The sprite font.</param>
    /// <param name="text">The string to draw.</param>
    /// <param name="position">The location, in screen coordinates, where the text will be drawn.</param>
    /// <param name="color">The desired color of the text.</param>
    /// <param name="rotation">The angle, in radians, to rotate the text around the origin.</param>
    /// <param name="origin">The origin of the string. Specify (0,0) for the upper-left corner.</param>
    /// <param name="scale">Vector containing separate scalar multiples for the x- and y-axes of the sprite.</param>
    /// <param name="effects">Rotations to apply prior to rendering.</param>
    /// <param name="layerDepth">The sorting depth of the sprite, between 0 (front) and 1 (back).</param>
    void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);
}
