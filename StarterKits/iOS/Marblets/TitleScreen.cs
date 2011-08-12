#region File Description
//-----------------------------------------------------------------------------
// TitleScreen.cs
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
	/// Title screen is the initial screen for Marblets. Allows you to select 2d or 3d 
	/// and displays the high scores
	/// </summary>
	public class TitleScreen : Screen
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TitleScreen"/> class.
		/// </summary>
		/// <param name="game">The game object to use</param>
		/// <param name="backgroundImage">The background image to use when this is 
		/// visible</param>
		/// <param name="backgroundMusic">The background music to play when this is 
		/// visible</param>
		public TitleScreen(Game game, string backgroundImage,
						   SoundEntry backgroundMusic)
			: base(game, backgroundImage, backgroundMusic)
		{
		}

		/// <summary>
		/// Checks for game start button presses
		/// </summary>
		/// 
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			GameState returnValue = GameState.None;

			//Transition to next state if those properties are set
			if(InputHelper.GamePads[PlayerIndex.One].APressed || Clicked)
			{
				Sound.Play(SoundEntry.Menu2DStart);
				returnValue = GameState.Play2D;
			}

			MarbletsGame.NextGameState = returnValue;
		}

		/// <summary>
		/// Draws the high scores
		/// </summary>
		/// 
		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			//If the guide is visible - then there are no high scores yet and the array
			//will be null
			if(MarbletsGame.HighScores != null)
			{
				SpriteBatch.Begin();

				//Draw the high scores
				for(int i = 0; i < 5; i++)
				{
					if(MarbletsGame.HighScores[i] != 0)
					{
						Font.Draw(SpriteBatch, FontStyle.Small, 135, 340 + i * 22,
								  MarbletsGame.HighScores[i]);
					}
				}

				SpriteBatch.End();
			}
		}
	}
}
