#region File Description
//-----------------------------------------------------------------------------
// Font.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Marblets
{
	/// <summary>
	/// Different fonts for use in Marblets
	/// </summary>
	public enum FontStyle
	{

		/// <summary>
		/// Small font
		/// </summary>
		Small,

		/// <summary>
		/// Large font
		/// </summary>
		Large,
	}

	/// <summary>
	/// Utility wrapper for pulling digits from a small font sheet
	/// </summary>
	public static class Font
	{
		private struct FontInfo
		{
			public string Filename;
			public string Characters;
			public int StartOffset;
			public int CharacterSpacing;
			public int CharacterWidth;
			public int CharacterHeight;

			public FontInfo(string fileName, string characters, int startOffset,
							int characterSpacing, int characterWidth,
							int characterHeight)
			{
				Filename = fileName;
				Characters = characters;
				StartOffset = startOffset;
				CharacterSpacing = characterSpacing;
				CharacterWidth = characterWidth;
				CharacterHeight = characterHeight;
			}
		}

		private static FontInfo[] fontInfo = new FontInfo[] 
        {
            new FontInfo("Textures/numbers_small", "1234567890,", 10, 32, 18, 32),
            new FontInfo("Textures/numbers_large", "1234567890,", 20, 64, 30, 64),
        };

		private static Texture2D[] fontTextures = new Texture2D[fontInfo.Length];


		/// <summary>
		/// Load graphics content.
		/// </summary>
		public static void LoadContent()
		{
			//Load all the font textures
			int fontCount = 0;
			foreach(FontInfo font in fontInfo)
			{
				fontTextures[fontCount++] =
                    MarbletsGame.Content.Load<Texture2D>(font.Filename);
			}
		}


		/// <summary>
		/// Draws some text from the given font
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to use</param>
		/// <param name="fontStyle">Which font to use</param>
		/// <param name="x">X position in screen pixel space</param>
		/// <param name="y">Y position in screen pixel space</param>
		/// <param name="number">The number to draw</param>
		public static void Draw(RelativeSpriteBatch spriteBatch, FontStyle fontStyle,
								int x, int y, int number)
		{
			//No color - use 'white' i.e. use whatever is in the file
			Draw(spriteBatch, fontStyle, x, y, number.ToString(), Color.White);
		}


		/// <summary>
		/// Draws some text from the given font
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to use</param>
		/// <param name="fontStyle">Which font to use</param>
		/// <param name="x">X position in screen pixel space</param>
		/// <param name="y">Y position in screen pixel space</param>
		/// <param name="digits">The characters to draw</param>
		public static void Draw(RelativeSpriteBatch spriteBatch, FontStyle fontStyle,
								int x, int y, string digits)
		{
			//No color - use 'white' i.e. use whatever is in the file
			Draw(spriteBatch, fontStyle, x, y, digits, Color.White);
		}



		/// <summary>
		/// Draws some text from the given font
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to use</param>
		/// <param name="fontStyle">Which font to use</param>
		/// <param name="position">A vector x,y position</param>
		/// <param name="number">A number to draw</param>
		/// <param name="color">The color of the text</param>
		public static void Draw(RelativeSpriteBatch spriteBatch, FontStyle fontStyle,
								Vector2 position, int number, Color color)
		{
			Draw(spriteBatch, fontStyle, (int)position.X, (int)position.Y,
				 number.ToString(), color);
		}


		/// <summary>
		/// Draws some text from the given font
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to use</param>
		/// <param name="fontStyle">Which font to use</param>
		/// <param name="x">X position in screen pixel space</param>
		/// <param name="y">Y position in screen pixel space</param>
		/// <param name="digits">The characters to draw</param>
		/// <param name="color">The color to draw it in</param>
		public static void Draw(RelativeSpriteBatch spriteBatch, FontStyle fontStyle,
								int x, int y, string digits, Color color)
		{
			float xPosition = x;
			FontInfo thisFont = fontInfo[(int)fontStyle];

			for(int i = 0; i < digits.Length; i++)
			{
				//Don't draw anything if its a space character
				if(digits[i] != ' ')
				{
					//Look up the character position
					int character = thisFont.Characters.IndexOf(digits[i]);

					//Draw the correct character at the correct position
					spriteBatch.Draw(fontTextures[(int)fontStyle],
									 new Vector2(xPosition, (float)y),
						new Rectangle(character * thisFont.CharacterSpacing +
                                      thisFont.StartOffset, 0, thisFont.CharacterWidth,
									  thisFont.CharacterHeight), color);
				}

				//Move the position of the next character.
				//If the character is a comma or colon then use a 'fudge factor' to make 
				//the font look a little proportional
				xPosition += ((digits[i] == ',' || digits[i] == ':') ?
                              thisFont.CharacterWidth / 2 : thisFont.CharacterWidth);
			}
		}
	}
}
