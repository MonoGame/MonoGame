#region File Description
//-----------------------------------------------------------------------------
// ${Namespace}Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

#endregion

namespace ${Namespace}
{
	/// <summary>
	/// Default Project Template
	/// </summary>
	public class ${Namespace}Game : Game
	{

	#region Fields
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D logoTexture;
	#endregion

	#region Initialization

        public ${Namespace}Game()  
        {

			graphics = new GraphicsDeviceManager(this);
			
			Content.RootDirectory = "Content";

			graphics.IsFullScreen = false;
		}

		/// <summary>
		/// Overridden from the base Game.Initialize. Once the GraphicsDevice is setup,
		/// we'll use the viewport to initialize some values.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();
		}


		/// <summary>
		/// Load your graphics content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be use to draw textures.
			spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
			
			// TODO: use this.Content to load your game content here eg.
			logoTexture = Content.Load<Texture2D>("logo");
		}

	#endregion

	#region Update and Draw

		/// <summary>
        	/// Allows the game to run logic such as updating the world,
        	/// checking for collisions, gathering input, and playing audio.
        	/// </summary>
        	/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// TODO: Add your update logic here			
            		
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself. 
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			// Clear the backbuffer
			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

			spriteBatch.Begin();

			// draw the logo
			spriteBatch.Draw(logoTexture, new Vector2 (130, 200), Color.White);

			spriteBatch.End();

			//TODO: Add your drawing code here
			base.Draw(gameTime);
		}

	#endregion
	}
}