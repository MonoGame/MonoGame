#region File Description
//-----------------------------------------------------------------------------
// InputHelper.cs
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
	/// Provides a wrapper around the gamepads to allow single button presses to be 
	/// detected also provides keyboard aliasing to allow keyboards to be used to play
	/// </summary>
	public class InputHelper : GameComponent
	{
		/// <summary>
		/// Current pressed state of the gamepads
		/// </summary>
		static public GamePads GamePads = new GamePads();

		/// <summary>
		/// Initializes a new instance of the <see cref="InputHelper"/> class.
		/// </summary>
		/// <param name="game">Game the game component should be attached to.</param>
		public InputHelper(Game game)
			: base(game)
		{
		}

		/// <summary>
		/// Called when the GameComponent needs to be updated. This polls all the game 
		/// pads and updates the state
		/// </summary>
		/// <param name="gameTime">Current game time</param>
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			GamePads.Update(this.Game);
		}
	}
}
