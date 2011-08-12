#region File Description
//-----------------------------------------------------------------------------
// ScaledSpriteBatch.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


#endregion

namespace HoneycombRush
{
    /// <summary>
    /// Represents a spritebatch where all drawing operations are scaled by a specific non-uniform factor. If a draw 
    /// operation specifies a specific scale factor, it will be multiplied by the default one.
    /// </summary>
    public class ScaledSpriteBatch : SpriteBatch
    {
        #region Properties


        /// <summary>
        /// Determines the scale factor for all drawing operations
        /// </summary>
        public Vector2 ScaleVector { get; set; }


        #endregion

        #region Initialization


        public ScaledSpriteBatch(GraphicsDevice graphicsDevice, Vector2 initialScale) : base(graphicsDevice)
        {
            ScaleVector = initialScale;
        }


        #endregion

        #region Draw overrides


        /// <summary>
        /// Adds a sprite to a batch of sprites for rendering using the specified texture,
        /// position and color. Reference page contains links to related code samples.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The location (in screen coordinates) to draw the sprite.</param>
        /// <param name="color">The color to tint a sprite. Use Color.White for full color with no tinting.</param>
        /// <remarks>The drawing operation will be scaled according to the <see cref="ScaleVector"/> 
        /// property.</remarks>
        public new void Draw(Texture2D texture, Vector2 position, Color color)
        {
            base.Draw(texture, position, null, color, 0, Vector2.Zero, ScaleVector, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Adds a sprite to a batch of sprites for rendering using the specified texture,
        /// position, source rectangle, and color.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The location (in screen coordinates) to draw the sprite.</param>
        /// <param name="sourceRectangle">A rectangle that specifies (in texels) the source texels from a texture.
        /// Use null to draw the entire texture.</param>
        /// <param name="color">The color to tint a sprite. Use Color.White for full color with no tinting.</param>
        /// <remarks>The drawing operation will be scaled according to the <see cref="ScaleVector"/> 
        /// property.</remarks>
        public new void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
        {
            base.Draw(texture, position, sourceRectangle, color, 0, Vector2.Zero, ScaleVector, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Adds a sprite to a batch of sprites for rendering using the specified texture,
        /// position, source rectangle, color, rotation, origin, scale, effects, and
        /// layer. Reference page contains links to related code samples.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The location (in screen coordinates) to draw the sprite.</param>
        /// <param name="sourceRectangle">A rectangle that specifies (in texels) the source texels from a texture.
        /// Use null to draw the entire texture.</param>
        /// <param name="color">The color to tint a sprite. Use Color.White for full color with no tinting.</param>
        /// <param name="rotation">Specifies the angle (in radians) to rotate the sprite about its center.</param>
        /// <param name="origin">The sprite origin; the default is (0,0) which represents the 
        /// upper-left corner.</param>
        /// <param name="scale">Scale factor.</param>
        /// <param name="effects">Effects to apply.</param>
        /// <param name="layerDepth">The depth of a layer. By default, 0 represents the front layer and 1 represents
        /// a back layer. Use SpriteSortMode if you want sprites to be sorted during drawing.</param>
        /// <remarks>The drawing operation will be scaled according to the <see cref="ScaleVector"/> 
        /// property.</remarks>
        public new void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, 
            float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            base.Draw(texture, position, sourceRectangle, color, rotation, origin, scale * ScaleVector, effects, 
                layerDepth);
        }

        /// <summary>
        /// Adds a sprite to a batch of sprites for rendering using the specified texture,
        /// position, source rectangle, color, rotation, origin, scale, effects and layer.
        /// Reference page contains links to related code samples.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The location (in screen coordinates) to draw the sprite.</param>
        /// <param name="sourceRectangle">A rectangle that specifies (in texels) the source texels from a texture.
        /// Use null to draw the entire texture.</param>
        /// <param name="color">The color to tint a sprite. Use Color.White for full color with no tinting.</param>
        /// <param name="rotation">Specifies the angle (in radians) to rotate the sprite about its center.</param>
        /// <param name="origin">The sprite origin; the default is (0,0) which represents the 
        /// upper-left corner.</param>
        /// <param name="scale">Scale factor.</param>
        /// <param name="effects">Effects to apply.</param>
        /// <param name="layerDepth">The depth of a layer. By default, 0 represents the front layer and 1 represents
        /// a back layer. Use SpriteSortMode if you want sprites to be sorted during drawing.</param>
        /// <remarks>The drawing operation will be scaled according to the <see cref="ScaleVector"/> 
        /// property.</remarks>
        public new void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            base.Draw(texture, position, sourceRectangle, color, rotation, origin, scale * ScaleVector, effects, 
                layerDepth);
        }

        /// <summary>
        /// Adds a string to a batch of sprites for rendering using the specified font,
        /// text, position, and color.
        /// </summary>
        /// <param name="spriteFont">A font for diplaying text</param>
        /// <param name="text">A text string.</param>
        /// <param name="position">The location (in screen coordinates) to draw the sprite.</param>
        /// <param name="color">The color to tint a sprite. Use Color.White for full color with no tinting.</param>
        /// <remarks>The drawing operation will be scaled according to the <see cref="ScaleVector"/> 
        /// property.</remarks>
        public new void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
        {
            base.DrawString(spriteFont, text, position, color, 0, Vector2.Zero, ScaleVector, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Adds a string to a batch of sprites for rendering using the specified font,
        /// text, position, and color.
        /// </summary>
        /// <param name="spriteFont">A font for diplaying text.</param>
        /// <param name="text">A text string.</param>
        /// <param name="position">The location (in screen coordinates) to draw the sprite.</param>
        /// <param name="color">The color to tint a sprite. Use Color.White for full color with no tinting.</param>
        /// <remarks>The drawing operation will be scaled according to the <see cref="ScaleVector"/> 
        /// property.</remarks>
        public new void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
        {
            base.DrawString(spriteFont, text, position, color, 0, Vector2.Zero, ScaleVector, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Adds a string to a batch of sprites for rendering using the specified font,
        /// text, position, color, rotation, origin, scale, effects and layer.
        /// </summary>
        /// <param name="spriteFont">A font for diplaying text.</param>
        /// <param name="text">A text string.</param>
        /// <param name="position">The location (in screen coordinates) to draw the sprite.</param>
        /// <param name="color">The color to tint a sprite. Use Color.White for full color with no tinting.</param>
        /// <param name="rotation">Specifies the angle (in radians) to rotate the sprite about its center.</param>
        /// <param name="origin">The sprite origin; the default is (0,0) which represents the 
        /// upper-left corner.</param>
        /// <param name="scale">Scale factor.</param>
        /// <param name="effects">Effects to apply.</param>
        /// <param name="layerDepth">The depth of a layer. By default, 0 represents the front layer and 1 represents
        /// a back layer. Use SpriteSortMode if you want sprites to be sorted during drawing.</param>
        /// <remarks>The drawing operation will be scaled according to the <see cref="ScaleVector"/> 
        /// property.</remarks>
        public new void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation,
            Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            base.DrawString(spriteFont, text, position, color, rotation, origin, scale * ScaleVector, effects,
                layerDepth);
        }

        /// <summary>
        /// Adds a string to a batch of sprites for rendering using the specified font,
        /// text, position, color, rotation, origin, scale, effects and layer.
        /// </summary>
        /// <param name="spriteFont">A font for diplaying text.</param>
        /// <param name="text">A text string.</param>
        /// <param name="position">The location (in screen coordinates) to draw the sprite.</param>
        /// <param name="color">The color to tint a sprite. Use Color.White for full color with no tinting.</param>
        /// <param name="rotation">Specifies the angle (in radians) to rotate the sprite about its center.</param>
        /// <param name="origin">The sprite origin; the default is (0,0) which represents the upper-left 
        /// corner.</param>
        /// <param name="scale">Scale factor.</param>
        /// <param name="effects">Effects to apply.</param>
        /// <param name="layerDepth">The depth of a layer. By default, 0 represents the front layer and 1 represents
        /// a back layer. Use SpriteSortMode if you want sprites to be sorted during drawing.</param>
        /// <remarks>The drawing operation will be scaled according to the <see cref="ScaleVector"/> 
        /// property.</remarks>
        public new void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation,
            Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            DrawString(spriteFont, text, position, color, rotation, origin, scale * ScaleVector, effects, layerDepth);
        }

        /// <summary>
        /// Adds a string to a batch of sprites for rendering using the specified font,
        /// text, position, color, rotation, origin, scale, effects and layer.
        /// </summary>
        /// <param name="spriteFont">A font for diplaying text.</param>
        /// <param name="text">Text string.</param>
        /// <param name="position">The location (in screen coordinates) to draw the sprite.</param>
        /// <param name="color">The color to tint a sprite. Use Color.White for full color with no tinting.</param>
        /// <param name="rotation">Specifies the angle (in radians) to rotate the sprite about its center.</param>
        /// <param name="origin">The sprite origin; the default is (0,0) which represents the upper-left 
        /// corner.</param>
        /// <param name="scale">Scale factor.</param>
        /// <param name="effects">Effects to apply.</param>
        /// <param name="layerDepth">The depth of a layer. By default, 0 represents the front layer and 1 represents
        /// a back layer. Use SpriteSortMode if you want sprites to be sorted during drawing.</param>
        /// <remarks>The drawing operation will be scaled according to the <see cref="ScaleVector"/> 
        /// property.</remarks>
        public new void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            base.DrawString(spriteFont, text, position, color, rotation, origin, scale * ScaleVector, effects, 
                layerDepth);
        }

        /// <summary>
        /// Adds a string to a batch of sprites for rendering using the specified font,
        /// text, position, color, rotation, origin, scale, effects and layer.
        /// </summary>
        /// <param name="spriteFont">A font for diplaying text.</param>
        /// <param name="text">Text string.</param>
        /// <param name="position">The location (in screen coordinates) to draw the sprite.</param>
        /// <param name="color">The color to tint a sprite. Use Color.White for full color with no tinting.</param>
        /// <param name="rotation">Specifies the angle (in radians) to rotate the sprite about its center.</param>
        /// <param name="origin">The sprite origin; the default is (0,0) which represents the upper-left 
        /// corner.</param>
        /// <param name="scale">Scale factor.</param>
        /// <param name="effects">Effects to apply.</param>
        /// <param name="layerDepth">The depth of a layer. By default, 0 represents the front layer and 1 represents
        /// a back layer. Use SpriteSortMode if you want sprites to be sorted during drawing.</param>
        /// <remarks>The drawing operation will be scaled according to the <see cref="ScaleVector"/> 
        /// property.</remarks>
        public new void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            base.DrawString(spriteFont, text, position, color, rotation, origin, scale * ScaleVector, 
                effects, layerDepth);
        }


        #endregion
    }
}
