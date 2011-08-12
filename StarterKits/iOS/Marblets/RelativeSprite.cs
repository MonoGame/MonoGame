#region File Description
//-----------------------------------------------------------------------------
// RelativeSprite.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace Marblets
{
	/// <summary>
	/// This class provides helper functions for making sprites, which are pixel based
	/// get drawn in correct positions and sizes when a window is resized.
	/// For the purposes of this code 1280x720 HD resolution is considered the base 
	/// size. Everything is sized based on its relative value to that.
	/// </summary>
	public class RelativeSpriteBatch : SpriteBatch
	{
		private const int fullHeight = 480;
		private const int fullWidth = 320;
		private static float scale;
		private static Vector2 offset;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelativeSpriteBatch"/> class.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device where sprites will be 
		/// drawn.</param>
		public RelativeSpriteBatch(GraphicsDevice graphicsDevice)
			: base(graphicsDevice)
		{
			Resize();
		}

		/// <summary>
		/// Adjust the scale and location of the drawn graphics so that everything 
		/// fits on the screen and is centered.
		/// </summary>
		public void Resize()
		{
			// Scale is used to stretch or shrink the drawn images so that everything
			// is visible on screen.
			scale = 
                Math.Min((float)GraphicsDevice.Viewport.Height / (float)fullHeight,
				(float)GraphicsDevice.Viewport.Width / (float)fullWidth);
			// The offset used to center the drawn images on the screen
			offset = 
                new Vector2((GraphicsDevice.Viewport.Width - fullWidth * scale) / 2,
				(GraphicsDevice.Viewport.Height - fullHeight * scale) / 2);
		}

		/// <summary>
		/// Draws a sprite at a particular position
		/// </summary>
		/// <param name="texture">The texture to take the sprite from. For this overload
		/// it will draw the entire texture.</param>
		/// <param name="position">The position in 1280x720 screen space. It will 
		/// actually be drawn in the correct relative position and size</param>
		/// <param name="color">The color to tint the sprite</param>
		public new void Draw(Texture2D texture, Vector2 position, Color color)
		{
			this.Draw(texture, position, null, color);
		}

		/// <summary>
		/// Draws a sprite at a particular position
		/// </summary>
		/// <param name="texture">The texture to take the sprite from. For this overload
		/// it will draw the entire texture.</param>
		/// <param name="position">The position in 1280x720 screen space. It will 
		/// actually be drawn in the correct relative position and size</param>
		/// <param name="source">The part of the texture to draw</param>
		/// <param name="color">The color to tint the sprite</param>
		public new void Draw(Texture2D texture, Vector2 position, Rectangle? source,
							 Color color)
		{
			//Scale and move the sprite based on current screen size 
			base.Draw(texture, (position * scale) + offset, source, color, 0,
				Vector2.Zero, scale, SpriteEffects.None, 0);
		}
	}
}
