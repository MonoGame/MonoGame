#region File Description
//-----------------------------------------------------------------------------
// Font.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#define MOUSE  //uncomment to enable mouse - see mouse tutorial

#region Using Statements
using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Collections.Generic;
#endregion

namespace Marblets
{
	/// <summary>
	/// The current animation state of the board and of individual marbles
	/// </summary>
	public enum Animation
	{
		/// <summary>
		/// No animations happening
		/// </summary>
		None,
		/// <summary>
		/// Some marbles are currently breaking
		/// </summary>
		Breaking,
		/// <summary>
		/// Breaking marbles are all gone
		/// </summary>
		Gone,
	}

	/// <summary>
	/// The game board holds the pieces, the cursor and handles most of the game logic
	/// </summary>
	public class GameBoard : DrawableGameComponent
	{
		/// <summary>
		/// Number if marbles in the x direction
		/// </summary>
		public const int Width = 7;
		/// <summary>
		/// Number of marbles in the y direction
		/// </summary>
		public const int Height = 9;

		private const int LeftEdge = 2;
		private const int TopEdge = 45;

		/// <summary>
		/// Marbles stores the set of marbles used on the game board
		/// </summary>
		internal Marble[,] Marbles = new Marble[Width, Height];

		/// <summary>
		/// Animation state stores what the game board is currently doing at any moment 
		/// in time
		/// </summary>
		internal Animation AnimationState = Animation.None;
		private bool gameOver;

		/// <summary>
		/// Cursor position and texture
		/// </summary>
		private int cursorX;
		private int cursorY;
		private Texture2D marbleCursorTexture;

		/// <summary>
		/// Score information
		/// </summary>
		private Vector2 scoreOffset = new Vector2(0, -Marble.Height * 1.5f);
		private GameTime scoreFadeStart;
		private float scoreFadeFactor;
		private const float scoreFadeDistance = Marble.Height * 2;
		private int totalSelected;

#if MOUSE
        private int lastMouseX;
        private int lastMouseY;
        private bool buttonDown;
#endif

		/// <summary>
		/// Create a new game board
		/// </summary>
		/// <param name="game"></param>
		public GameBoard(Game game)
			: base(game)
		{
			Marble.Initialize();
#if MOUSE
            MouseState mouse = Mouse.GetState();
            lastMouseX = mouse.X;
            lastMouseY = mouse.Y;
#endif

		}



		/// <summary>
		/// Load graphics content.
		/// </summary>
		protected override void LoadContent()
		{
			marbleCursorTexture = 
                MarbletsGame.Content.Load<Texture2D>("Textures/marble_cursor");
			Marble.LoadContent();

			base.LoadContent();
		}

		/// <summary>
		/// Checks to see if the game is over yet
		/// </summary>
		public bool GameOver
		{
			get
			{
				if(!gameOver && IsGameOver())
				{
					gameOver = true;
				}

				return gameOver;
			}
		}


		/// <summary>
		/// Called when the GameComponent needs to be updated.  
		/// Game Board update performs animation on the marbles, checks for cursor 
		/// movement. Most of the game logic is here or called from here
		/// </summary>
		/// <param name="gameTime">Current game time</param>
		public override void Update(GameTime gameTime)
		{
			if(gameTime == null)
				return;

			base.Update(gameTime);

			if(!GameOver)
			{
				Marble.UpdateStatic(gameTime);

				foreach(Marble marble in Marbles)
				{
					if(marble != null)
					{
						marble.Update(gameTime);
					}
				}

				switch(AnimationState)
				{
					case Animation.Breaking:
						{
							//Fade the score
							scoreFadeFactor = (float)((gameTime.TotalGameTime - 
                                scoreFadeStart.TotalGameTime).TotalSeconds / 
                                Marble.BreakTime);

							//Wait until all marbles are broken.
							bool stillBreaking = false;
							foreach(Marble marble in Marbles)
							{
								if(marble != null)
								{
									if(marble.Animation == Animation.Breaking)
									{
										stillBreaking = true;
										break;
									}
								}
							}

							//Done breaking - do the 'fall' and 'slide'
							if(!stillBreaking)
							{
								FallAndSlide();
								totalSelected = 0;
								FindCursorPosition();
								FindSelectedMarbles();
								Sound.Play(SoundEntry.LandMarbles);
								AnimationState = Animation.None;
							}
							break;
						}

					case Animation.None:
						{
							moveCursor();
							BreakMarbles(gameTime);
							break;
						}
				}
			}
		}

		private void FallAndSlide()
		{
			//Fall
			for(int x = 0; x < Width; x++)
			{
				int moveDistance = 0;
				for(int y = Height - 1; y >= 0; y--)
				{
					//Remember the current marble
					Marble currentMarble = Marbles[x, y];

					if(moveDistance > 0)
					{
						Marbles[x, y + moveDistance] = Marbles[x, y];
						Marbles[x, y] = null;
						if(Marbles[x, y + moveDistance] != null)
						{
							Marbles[x, y + moveDistance].Position = 
                                BoardToScreen(x, y + moveDistance);
						}
					}

					if(currentMarble != null && currentMarble.Selected)
					{
						moveDistance++;
					}
				}

				//Tidy up any marbles that didn't have anything above them
				for(int y = 0; y < moveDistance; y++)
				{
					Marbles[x, y] = null;
				}
			}

			//Slide
			for(int y = 0; y < Height; y++)
			{
				int moveDistance = 0;
				for(int x = Width - 1; x >= 0; x--)
				{
					//Remember the current marble
					Marble currentMarble = Marbles[x, y];

					if(moveDistance > 0)
					{
						Marbles[x + moveDistance, y] = Marbles[x, y];
						Marbles[x, y] = null;
						if(Marbles[x + moveDistance, y] != null)
						{
							Marbles[x + moveDistance, y].Position = 
                                BoardToScreen(x + moveDistance, y);
						}
					}

					if(currentMarble == null)
					{
						moveDistance++;
					}
				}

				//Tidy up any marbles that didn't have anything to their left 
				for(int x = 0; x < moveDistance; x++)
				{
					Marbles[x, y] = null;
				}
			}
		}

		/// <summary>
		/// Converts a board grid position into a pixel screen position 
		/// in 1280x720 space
		/// </summary>
		/// <param name="x">0-number of marbles wide</param>
		/// <param name="y">0-number of marbles high</param>
		/// <returns></returns>
		protected static Vector2 BoardToScreen(int x, int y)
		{
			return new Vector2(LeftEdge + (float)x * Marble.Width,
							   TopEdge + (float)y * Marble.Height);
		}


		private void FindCursorPosition()
		{
			//If the cursor is on a marble then leave it there
			if(Marbles[cursorX, cursorY] != null) return;

			//We search the squares in this order....
			//   X  3  8  15
			//   1  2  7  14
			//   4  5  6  13
			//   9  10 11 12

			//Otherwise move to the closes marble down and to the right.
			for(int distance = 1;
				 distance < Math.Max(Width - cursorX, Height - cursorY) - 1;
				 distance++)
			{
				//Search below and across
				if(cursorY + distance < Height)
				{
					for(int x = cursorX;
						 x < Math.Min(cursorX + distance+1, Width); x++)
					{
						if(Marbles[x, cursorY + distance] != null)
						{
							cursorX = x;
							cursorY = cursorY + distance;
							return;
						}
					}
				}

				//Search to the right
				if(cursorX + distance < Width)
				{
					for(int y = Math.Min(cursorY + distance, Height) -1; y >= cursorY;
						 y--)
					{
						if(Marbles[cursorX + distance, y] != null)
						{
							cursorX = cursorX + distance;
							cursorY = y;
							return;
						}
					}
				}
			}

			//If we exit the loop then there is no place to go so they must have cleared
			//the board in which case its game over anyway
			return;
		}

		private void BreakMarbles(GameTime time)
		{

#if MOUSE
            bool buttonClicked=false;
            MouseState mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                buttonDown = true;
            }


            if (buttonDown && mouse.LeftButton == ButtonState.Released)
            {
                buttonDown = false;
                buttonClicked = true;
            }

#endif
			//break if possible
			bool burstPressed = InputHelper.GamePads[PlayerIndex.One].APressed
#if MOUSE
                || (Game.IsMouseVisible && buttonClicked)
#endif
;


			if(totalSelected > 0 && burstPressed)
			{
				if(totalSelected > 10)
				{
					Sound.Play(SoundEntry.ClearBonus);
				}
				else
				{
					Sound.Play(SoundEntry.ClearMarbles);
				}

				MarbletsGame.Score += Score(totalSelected);

				AnimationState = Animation.Breaking;
				foreach(Marble marble in Marbles)
				{
					if(marble != null && marble.Selected)
					{
						marble.Break(time);
					}
				}

				//Setup the score fading
				scoreFadeStart = time;
			}
			else if(burstPressed)
			{
				//not enough marbles
				Sound.Play(SoundEntry.ClearIllegal);
			}

		}

		private void moveCursor()
		{
			//Move the cursor
			bool cursorMovedWithMouse = false;

#if MOUSE
            //If user moves the mouse then show it
            MouseState mouse = Mouse.GetState();
            if (mouse.X != lastMouseX && mouse.Y != lastMouseY)
            {
                //TODO: Doesn't seem to actually show the mouse until you move outside 
                //the window and back in.
                Game.IsMouseVisible = true;
            }

            //if the mouse is visible then track the cursor with the mouse
            if (Game.IsMouseVisible)
            {
                //Find which marble is under the cursor
                cursorMovedWithMouse = moveCursorWithMouse(mouse.X, mouse.Y);
                lastMouseX = mouse.X;
                lastMouseY = mouse.Y;
            }
#endif

			//Move the cursor with keyboard/gamepad
			bool cursorMovedWithGamePad = false;

			if(InputHelper.GamePads[PlayerIndex.One].LeftPressed)
			{
				cursorMovedWithGamePad = FindNextMarbleX(-1);
			}
			if(InputHelper.GamePads[PlayerIndex.One].RightPressed)
			{
				cursorMovedWithGamePad = FindNextMarbleX(1);
			}
			if(InputHelper.GamePads[PlayerIndex.One].UpPressed)
			{
				cursorMovedWithGamePad = FindNextMarbleY(-1);
			}
			if(InputHelper.GamePads[PlayerIndex.One].DownPressed)
			{
				cursorMovedWithGamePad = FindNextMarbleY(1);
			}

			//if anything moved we play the move sound
			if(cursorMovedWithGamePad || cursorMovedWithMouse)
			{
				Sound.Play(SoundEntry.Navigate);
				FindSelectedMarbles();
			}

#if MOUSE
            //If the user goes back to moving with the keyboard/gamepad 
            //then hide the mouse
            if (cursorMovedWithGamePad)
            {
                Game.IsMouseVisible = false;
            }
#endif

		}

#if MOUSE
        private bool moveCursorWithMouse(int x, int y)
        {
            bool clickedOnMarble = false;

            double scale = (float)GraphicsDevice.Viewport.Height / 480.0;
            double offset = (int)((GraphicsDevice.Viewport.Width - 320 * scale) / 2);

            int boardX = (int)((((x-offset)/scale) - LeftEdge) / Marble.Width);
            int boardY = (int)(((y/scale) - TopEdge ) / Marble.Height);
            if (boardX >=0 && boardX < Width && boardY>=0 && boardY < Height && 
                Marbles[boardX, boardY] != null)
            {
                //Don't actually move and play the sound if the mouse is moving within 
                //the marble
                if (cursorX != boardX || cursorY != boardY)
                {
                    cursorX = boardX;
                    cursorY = boardY;
                    clickedOnMarble = true;
                }
            }

            return clickedOnMarble;

        }
#endif

		private bool FindNextMarbleX(int direction)
		{
			bool cursorMoved = false;

			for(int move = 0; move < GameBoard.Width; move++)
			{
				//Wrap at the edge
				if(cursorX == 0 && direction == -1)
				{
					cursorX = GameBoard.Width;
				}
				else if(cursorX == (GameBoard.Width - 1) && direction == 1)
				{
					cursorX = -1;
				}

				//Move
				cursorX += direction;

				//Stop when we find a marble
				if(Marbles[cursorX, cursorY] != null)
				{
					cursorMoved = true;
					break;
				}
			}
			return cursorMoved;
		}

		private bool FindNextMarbleY(int direction)
		{
			bool cursorMoved = false;

			for(int move = 0; move < GameBoard.Height; move++)
			{
				//Wrap at the edge
				if(cursorY == 0 && direction == -1)
				{
					cursorY = GameBoard.Height;
				}
				else if(cursorY == (GameBoard.Height -1) && direction == 1)
				{
					cursorY = -1;
				}

				//Move
				cursorY += direction;

				//Stop when we find a marble
				if(Marbles[cursorX, cursorY] != null)
				{
					cursorMoved = true;
					break;
				}
			}
			return cursorMoved;
		}



		private void FindSelectedMarbles()
		{
			//Update the selected portions of the board
			foreach(Marble marble in Marbles)
			{
				if(marble != null)
				{
					marble.Selected = false;
				}
			}

			totalSelected = 0;
			SelectMarble(cursorX, cursorY);
		}

		private static int Score(int totalSelected)
		{
			return totalSelected * (totalSelected - 1);
		}

		//Select all marbles with the same color as this one
		private void SelectMarble(int x, int y)
		{
			if(Marbles[x, y] != null)
			{
				Marbles[x, y].Selected = true;
				totalSelected++;

				SelectSurrounding(Marbles[x, y].Color, x, y);

				//If this is the only one then don't select it
				if(totalSelected == 1)
				{
					Marbles[x, y].Selected = false;
					totalSelected--;
				}
			}
		}

		private void SelectSurrounding(Color color, int x, int y)
		{
			if(x < Width - 1) MatchColor(color, x + 1, y);
			if(x > 0) MatchColor(color, x - 1, y);
			if(y < Height - 1) MatchColor(color, x, y + 1);
			if(y > 0) MatchColor(color, x, y - 1);
		}

		private void MatchColor(Color color, int x, int y)
		{
			//If we are already selected then early out - we've been here
			if(Marbles[x, y] != null && !Marbles[x, y].Selected)
			{
				//If the color matches
				if(Marbles[x, y].Color == color)
				{
					totalSelected++;
					Marbles[x, y].Selected = true;
					SelectSurrounding(color, x, y);
				}
			}
		}


		/// <summary>
		/// Draws the game board and all child objects
		/// </summary>
		/// <param name="spriteBatch">a sprite batch to use to draw any sprites</param>
		public void Draw(RelativeSpriteBatch spriteBatch)
		{
			if(spriteBatch == null)
				return;

			foreach(Marble marble in Marbles)
			{
				if(marble != null)
				{
					marble.Draw(spriteBatch);
				}
			}

			if(!GameOver)
			{
				//Draw the cursor
				spriteBatch.Draw(marbleCursorTexture, BoardToScreen(cursorX, cursorY),
								 Color.White);
			}

			if(!GameOver)
			{
				if(totalSelected > 0)
				{
					Vector2 position = BoardToScreen(cursorX, cursorY) + scoreOffset;
					Color color = Color.White;

					if(AnimationState == Animation.Breaking)
					{
						//Slide the score up and fade it
						position -= new Vector2(0, scoreFadeFactor * scoreFadeDistance);
						color = Color.White * (1f - scoreFadeFactor);
					}

					Font.Draw(spriteBatch, FontStyle.Large, position,
							  Score(totalSelected), color);
				}
			}
		}


		/// <summary>
		/// Returns the state of the board as a string - useful for debugging
		/// </summary>
		/// <returns>String representation of the board</returns>
		public override string ToString()
		{
			StringBuilder board = new StringBuilder();
			for(int y = 0; y < Height; y++)
			{
				for(int x = 0; x < Width; x++)
				{
					if(Marbles[x, y] == null)
						board.Append("--null-- ");
					else
						board.Append(Marbles[x, y].Color.ToString() + " ");
				}
				board.AppendLine();
			}
			board.AppendLine();
			return board.ToString();
		}

		/// <summary>
		/// Check to see if the game is over or not. This is a function rather than a 
		/// property to indicate that it does significant
		/// work rather than being a cheap property get
		/// </summary>
		/// <returns>true is the game is over</returns>
		private bool IsGameOver()
		{
			//Check all squares on board for pairs either to the right or below. 
			//This will catch any possibilities
			for(int y = 0; y < Height; y++)
			{
				for(int x = 0; x < Width; x++)
				{
					if(Marbles[x, y] != null)
					{
						if((x < Width-1 && Marbles[x + 1, y] != null && 
                             Marbles[x, y].Color == Marbles[x + 1, y].Color) ||
                            (y < Height-1 && Marbles[x, y + 1] != null && 
                             Marbles[x, y].Color == Marbles[x, y + 1].Color))
						{
							return false;
						}
					}
				}
			}

			//Game is over.
			SaveHighScores();

			return true;
		}

		/// <summary>
		/// Save the high scores to the drive.
		/// </summary>
		private static void SaveHighScores()
		{
			//Insert the score into the high score table
			for(int i = 0; i < 5; i++)
			{
				if(MarbletsGame.Score > MarbletsGame.HighScores[i])
				{
					//Insert new score
					MarbletsGame.HighScores.Insert(i, MarbletsGame.Score);
					//And remove the lowest from the bottom
					MarbletsGame.HighScores.RemoveAt(5);
					break;
				}
			}
		}

		/// <summary>
		/// Initializes the board
		/// </summary>
		public void NewGame()
		{
			gameOver = false;

			//Fill board
			for(int x = 0; x < Width; x++)
			{
				for(int y = 0; y < Height; y++)
				{
					Marbles[x, y] = new Marble();
					Marbles[x, y].Position = BoardToScreen(x, y);
				}
			}

			cursorX = 0;
			cursorY = 0;

			//The cursor might be on a matching color set
			FindSelectedMarbles();
		}
	}
}
