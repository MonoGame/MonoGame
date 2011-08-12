#region File Description
//-----------------------------------------------------------------------------
// TutorialGameBoard.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace Marblets
{
	/// <summary>
	/// An override on the gameboard class that allows us to add different modes to the 
	/// game play This is a very simple example for a tutorial. It just adds new marbles
	/// every few seconds.
	/// </summary>
	class TutorialGameBoard : GameBoard
	{
		private double nextDropTime;
		private const double dropInterval = 5.0;

		/// <summary>
		/// Create a new gameboard
		/// </summary>
		/// <param name="game"></param>
		public TutorialGameBoard(Game game)
			: base(game)
		{
		}

		/// <summary>
		/// Updates the game for the tutorial. Calls the normal gameboard update and 
		/// then checks the clock to see if its time to add some missing marbles
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update(GameTime gameTime)
		{
			if(gameTime == null)
			{
				throw new ArgumentNullException("gameTime");
			}


			//Ensure base class is updated
			base.Update(gameTime);

			//If its time to add marbles then add a row
			if(gameTime.TotalGameTime.TotalSeconds > nextDropTime)
			{
				AttemptDrop();
				nextDropTime = gameTime.TotalGameTime.TotalSeconds + dropInterval;
			}
		}

		private void AttemptDrop()
		{
			//Drop a marble into every column that isn't full
			bool createdMarble = false;

			for(int x = 0; x < Width; x++)
			{
				for(int y = Height-1; y >=0; y--)
				{
					if(Marbles[x, y] == null)
					{
						Marbles[x, y] = new Marble();
						Marbles[x, y].Position = BoardToScreen(x, y);
						createdMarble = true;
						break;
					}
				}
			}

			if(createdMarble)
			{
				Sound.Play(SoundEntry.LandMarbles);
			}
		}
	}
}
