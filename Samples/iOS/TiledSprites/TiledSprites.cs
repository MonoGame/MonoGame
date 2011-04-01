#region File Description
//-----------------------------------------------------------------------------
// TiledSprites.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

#endregion

namespace TiledSprites
{
	/// <summary>
	/// This sample showcases a variety of common sprite operations
	/// using SpriteBatch
	/// </summary>
	public class TiledSpritesSample : Game
	{
	#region Private Types
		/// <summary>
		/// Some tiles based on included media
		/// </summary>
		public enum TileName : int
		{
			Empty = 0,
			Base = 1,
			Detail1 = 2,
			Detail2 = 3,
			Detail3 = 4,
			Detail4 = 5,
			SoftDetail1 = 6,
			SoftDetail2 = 7,
			SoftDetail3 = 8,
			SoftDetail4 = 9,
			Rocks1 = 10,
			Rocks2 = 11,
			Rocks3 = 12,
			Rocks4 = 13,
			Clouds = 14
		}
	#endregion

	#region Constants
		private const float MovementRate = 500f;
		private const float ZoomRate = 0.5f;
		private const float RotationRate = 1.5f;
		private const int numTiles = 200;
		private const float animationTime = 0.1f;
		private static readonly Vector2 animatedSpriteScale = new Vector2 (.3f, .3f);
	#endregion

	#region Fields
		//utility types
		GraphicsDeviceManager graphics;

		//input state storage
		KeyboardState lastKeyboardState = new KeyboardState ();
#if IPHONE
		GamePadState lastGamePadState = new GamePadState ();
		GamePadState currentGamePadState = new GamePadState ();
#endif		
		KeyboardState currentKeyboardState = new KeyboardState ();
		private Random rand;

		//2D camera abstraction
		private Camera2D camera;
		private Vector2 screenCenter;

		//tile information
		private SpriteSheet groundSheet;
		private SpriteSheet cloudSheet;
		private SpriteBatch spriteBatch;
		private TileGrid rockLayer;
		private TileGrid groundLayer;
		private TileGrid cloudLayer;
		private TileGrid detailLayer;

		//animated sprite
		private SpriteSheet animatedSpriteSheet;
		private AnimatedSprite animatedSprite;
		private Vector2 animatedSpritePosition;
		private float accumulator;

	#endregion

	#region Initialization
		public TiledSpritesSample ()
			{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";
			
			graphics.PreferredBackBufferWidth = 320;
			graphics.PreferredBackBufferHeight = 480;

			rand = new Random ();
		}

		/// <summary>
		/// Load the graphics content.
		/// </summary>
		protected override void LoadContent ()
		{
			//When the backbuffer resolution changes, this part of the
			//LoadContent calback is used to reset the screen center
			screenCenter = new Vector2 (
				(float)graphics.GraphicsDevice.Viewport.Width / 2f,
				(float)graphics.GraphicsDevice.Viewport.Height / 2f);


		#region Set up Graphics resources
			Texture2D groundTexture = Content.Load<Texture2D> ("ground");
			Texture2D cloudTexture = Content.Load<Texture2D> ("clouds");
			spriteBatch = new SpriteBatch (graphics.GraphicsDevice);
		#endregion

		#region Set Up Tile Sources
			//set up the tile sheets with source rectangles
			//for each of the different sprites
			cloudSheet = new SpriteSheet (cloudTexture);
			cloudSheet.AddSourceSprite ((int)TileName.Clouds, 
		new Rectangle (0, 0, 1024, 1024));
			groundSheet = new SpriteSheet (groundTexture);
			groundSheet.AddSourceSprite ((int)TileName.Base, 
		new Rectangle (0, 0, 510, 510));
			groundSheet.AddSourceSprite ((int)TileName.Detail1, 
		new Rectangle (514, 0, 255, 255));
			groundSheet.AddSourceSprite ((int)TileName.Detail2, 
		new Rectangle (769, 0, 255, 255));
			groundSheet.AddSourceSprite ((int)TileName.Detail3, 
		new Rectangle (514, 256, 255, 255));
			groundSheet.AddSourceSprite ((int)TileName.Detail4, 
		new Rectangle (769, 256, 255, 255));
			groundSheet.AddSourceSprite ((int)TileName.SoftDetail1, 
		new Rectangle (514, 514, 255, 255));
			groundSheet.AddSourceSprite ((int)TileName.SoftDetail2, 
		new Rectangle (769, 514, 255, 255));
			groundSheet.AddSourceSprite ((int)TileName.SoftDetail3, 
		new Rectangle (514, 769, 255, 255));
			groundSheet.AddSourceSprite ((int)TileName.SoftDetail4, 
		new Rectangle (769, 769, 255, 255));
			groundSheet.AddSourceSprite ((int)TileName.Rocks1, 
		new Rectangle (0, 514, 255, 255));
			groundSheet.AddSourceSprite ((int)TileName.Rocks2, 
		new Rectangle (256, 514, 255, 255));
			groundSheet.AddSourceSprite ((int)TileName.Rocks3, 
		new Rectangle (0, 769, 255, 255));
			groundSheet.AddSourceSprite ((int)TileName.Rocks4, 
		new Rectangle (256, 769, 255, 255));
		#endregion

		#region Setup Tile Grids
			//Create the ground layer tile
			groundLayer = new TileGrid (510, 510, numTiles, numTiles,
		Vector2.Zero, groundSheet, graphics);

			//calculate the number of detial tiles, which are 
			//half the size of the base tiles, so there are
			//twice as many (minus one since they are being offset)
			int numDetailTiles = (numTiles * 2 - 1);

			//add an offset to break up the pattern
			detailLayer = new TileGrid (255, 255, numDetailTiles, numDetailTiles,
		new Vector2 (127, 127), groundSheet, graphics);

			rockLayer = new TileGrid (255, 255, numDetailTiles, numDetailTiles,
		new Vector2 (0, 0), groundSheet, graphics);

			int numCloudTiles = numTiles / 6 + 1;
			cloudLayer = new TileGrid (1024, 1024, numCloudTiles, numCloudTiles,
		Vector2.Zero, cloudSheet, graphics);

			//These loops fill the datas with some appropriate data.  
			//The clouds and ground clutter have been randomized.
			for (int i = 0; i < numTiles; i++) {
				for (int j = 0; j < numTiles; j++) {
					groundLayer.SetTile (i, j, 1);
				}
			}

			for (int i = 0; i < numDetailTiles; i++) {
				for (int j = 0; j < numDetailTiles; j++) {
					switch (rand.Next (20)) {
					case 0:
						detailLayer.SetTile (i, j, (int)TileName.Detail1);
						break;
					case 1:
						detailLayer.SetTile (i, j, (int)TileName.Detail2);
						break;
					case 2:
						detailLayer.SetTile (i, j, (int)TileName.Detail3);
						break;
					case 3:
						detailLayer.SetTile (i, j, (int)TileName.Detail4);
						break;
					case 4:
					case 5:
						detailLayer.SetTile (i, j, (int)TileName.SoftDetail1);
						break;
					case 6:
					case 7:
						detailLayer.SetTile (i, j, (int)TileName.SoftDetail2);
						break;
					case 8:
					case 9:
						detailLayer.SetTile (i, j, (int)TileName.SoftDetail3);
						break;
					case 10:
					case 11:
						detailLayer.SetTile (i, j, (int)TileName.SoftDetail4);
						break;
					}
				}
			}
			for (int i = 0; i < numDetailTiles; i++) {
				for (int j = 0; j < numDetailTiles; j++) {
					switch (rand.Next (25)) {
					case 0:
						rockLayer.SetTile (i, j, (int)TileName.Rocks1);
						break;
					case 1:
						rockLayer.SetTile (i, j, (int)TileName.Rocks2);
						break;
					case 2:
						rockLayer.SetTile (i, j, (int)TileName.Rocks3);
						break;
					case 3:
					case 4:
						rockLayer.SetTile (i, j, (int)TileName.Rocks4);
						break;
					}
				}
			}

			for (int i = 0; i < numCloudTiles; i++) {
				for (int j = 0; j < numCloudTiles; j++) {

					cloudLayer.SetTile (i, j, (int)TileName.Clouds);

				}
			}
		#endregion

			// Set up AnimatedSprite
			animatedSpriteSheet = new SpriteSheet (Content.Load<Texture2D> ("ball"));

			animatedSprite = new AnimatedSprite (animatedSpriteSheet, 254, 254, 1,
		4, 4, new Point (1, 0), 15);


			// Set Up a 2D Camera
			camera = new Camera2D ();

			ResetToInitialPositions ();

			//assuming a resolution change, need to update the sprite's "position"
			animatedSprite.Origin = camera.Position - animatedSpritePosition;
			animatedSprite.Position = screenCenter;

		}

		/// <summary>
		/// Reset the camera to the center of the tile grid
		/// and reset the position of the animted sprite
		/// </summary>
		private void ResetToInitialPositions ()
		{
			//set up the 2D camera
			//set the initial position to the center of the
			//tile field
			camera.Position = new Vector2 (numTiles * 255);
			camera.Rotation = 0f;
			camera.Zoom = 1f;
			camera.MoveUsingScreenAxis = true;

			//the animated sprite has no concept of a camera, so 
			//making the sprite camera relative is the job
			//of the game program
			animatedSpritePosition = camera.Position;
			animatedSprite.ScaleValue = animatedSpriteScale;
			animatedSprite.Position = screenCenter;


			CameraChanged ();
		}


	#endregion

	#region Update and Render
		/// <summary>
		/// Update the game world.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime)
		{
			HandleInput ();

			//Set the camera's state to Unchanged for this frame
			//this will save us from having to update visibility if the camera
			//does not move
			camera.ResetChanged ();

			//Call sample-specific input handling function
			HandleKeyboardInput ((float)gameTime.ElapsedGameTime.TotalSeconds);
#if IPHONE
			HandleGamePadInput ((float)gameTime.ElapsedGameTime.TotalSeconds);
#endif
			if (camera.IsChanged) {
				CameraChanged ();
			}


			//thottle the animation update speed to the frame animation
			//time
			accumulator += (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (accumulator > animationTime) {
				animatedSprite.IncrementAnimationFrame ();
				accumulator -= animationTime;
			}

			base.Update (gameTime);
		}

	#region Input Handling Functions
#if IPHONE
		/// <summary>
		/// Handle Game Pad input during Update
		/// </summary>
		public void HandleGamePadInput (float elapsed)
		{
			if (currentGamePadState.IsConnected) {
				//the left thumbstick moves the animated sprite around the world
				if ((Math.Abs (currentGamePadState.ThumbSticks.Left.X) > .1f) || 
			(Math.Abs (currentGamePadState.ThumbSticks.Left.Y) > .1f)) {
					//Sprite movement is being updated relative to the camera
					//rotation
					animatedSpritePosition.X += (float)Math.Cos (-camera.Rotation) * 
			currentGamePadState.ThumbSticks.Left.X * elapsed * MovementRate;
					animatedSpritePosition.Y += (float)Math.Sin (-camera.Rotation) * 
			currentGamePadState.ThumbSticks.Left.X * elapsed * MovementRate;
					animatedSpritePosition.Y -= (float)Math.Cos (camera.Rotation) * 
			currentGamePadState.ThumbSticks.Left.Y * elapsed * MovementRate;
					animatedSpritePosition.X -= (float)Math.Sin (camera.Rotation) * 
			currentGamePadState.ThumbSticks.Left.Y * elapsed * MovementRate;

					//since the sprite position has changed, the Origin must be updated
					//on the animated sprite object
					animatedSprite.Origin = (camera.Position - animatedSpritePosition)
 / animatedSpriteScale.X;
				}
				//right thumbstick controls the camera position
				if ((Math.Abs (currentGamePadState.ThumbSticks.Right.X) > .1f) || 
			(Math.Abs (currentGamePadState.ThumbSticks.Right.Y) > .1f)) {
					float dX = currentGamePadState.ThumbSticks.Right.X * elapsed * 
			MovementRate;
					float dY = currentGamePadState.ThumbSticks.Right.Y * elapsed * 
			MovementRate;
					camera.MoveRight (ref dX);
					camera.MoveUp (ref dY);
				}
				//the triggers control rotation
				if ((Math.Abs (currentGamePadState.Triggers.Left) > .1f) || 
			(Math.Abs (currentGamePadState.Triggers.Right) > .1f)) {
					float dX = currentGamePadState.Triggers.Left * 
			elapsed * RotationRate;
					dX += -currentGamePadState.Triggers.Right * 
			elapsed * RotationRate;
					camera.Rotation += dX;
				}
				//the A and B buttons control zoom
				if ((currentGamePadState.Buttons.A == ButtonState.Pressed) || 
			(currentGamePadState.Buttons.B == ButtonState.Pressed)) {
					float delta = elapsed * ZoomRate;
					if (currentGamePadState.Buttons.B == ButtonState.Pressed)
						delta = -delta;

					camera.Zoom += delta;
					if (camera.Zoom < .5f)
						camera.Zoom = .5f;
					if (camera.Zoom > 2f)
						camera.Zoom = 2f;
				}
				if ((currentGamePadState.Buttons.RightStick == ButtonState.Pressed) && 
			(lastGamePadState.Buttons.RightStick == ButtonState.Released)) {
					ResetToInitialPositions ();
				}
			}
		}
#endif
		/// <summary>
		/// Handle Keyboard input during Update
		/// </summary>
		public void HandleKeyboardInput (float elapsed)
		{
			//check for camera movement
			float dX = ReadKeyboardAxis (currentKeyboardState, Keys.Left, Keys.Right) * 
		elapsed * MovementRate;
			float dY = ReadKeyboardAxis (currentKeyboardState, Keys.Down, Keys.Up) * 
		elapsed * MovementRate;
			camera.MoveRight (ref dX);
			camera.MoveUp (ref dY);

			//check for animted sprite movement
			animatedSpritePosition.X += (float)Math.Cos (-camera.Rotation) * 
		ReadKeyboardAxis (currentKeyboardState, Keys.A, Keys.D) * 
		elapsed * MovementRate;
			animatedSpritePosition.Y += (float)Math.Sin (-camera.Rotation) * 
		ReadKeyboardAxis (currentKeyboardState, Keys.A, Keys.D) * 
		elapsed * MovementRate;

			animatedSpritePosition.X -= (float)Math.Sin (camera.Rotation) * 
		ReadKeyboardAxis (currentKeyboardState, Keys.S, Keys.W) * 
		elapsed * MovementRate;
			animatedSpritePosition.Y -= (float)Math.Cos (camera.Rotation) * 
		ReadKeyboardAxis (currentKeyboardState, Keys.S, Keys.W) * 
		elapsed * MovementRate;

			//since the sprite position has changed, the Origin must be updated
			//on the animated sprite object
			animatedSprite.Origin = (camera.Position - animatedSpritePosition)
 / animatedSpriteScale.X;

			//check for camera rotation
			dX = ReadKeyboardAxis (currentKeyboardState, Keys.E, Keys.Q) * 
		elapsed * RotationRate;
			camera.Rotation += dX;


			//check for camera zoom
			dX = ReadKeyboardAxis (currentKeyboardState, Keys.X, Keys.Z) * 
		elapsed * ZoomRate;

			//limit the zoom
			camera.Zoom += dX;
			if (camera.Zoom < .5f)
				camera.Zoom = .5f;
			if (camera.Zoom > 2f)
				camera.Zoom = 2f;

			//check for camera reset
			if (currentKeyboardState.IsKeyDown (Keys.R) && 
		lastKeyboardState.IsKeyDown (Keys.R)) {
				ResetToInitialPositions ();
			}
		}

		/// <summary>
		/// This function is called when the camera's values have changed
		/// and is used to update the properties of the tiles and animated sprite
		/// </summary>
		public void CameraChanged ()
		{
			//set rotation
			groundLayer.CameraRotation = detailLayer.CameraRotation = 
		cloudLayer.CameraRotation = rockLayer.CameraRotation = 
		animatedSprite.Rotation = camera.Rotation;

			//set zoom
			groundLayer.CameraZoom = detailLayer.CameraZoom = 
		rockLayer.CameraZoom = camera.Zoom;
			animatedSprite.ScaleValue = animatedSpriteScale * camera.Zoom;
			cloudLayer.CameraZoom = camera.Zoom + 1.0f;

			//For an extra special effect, the camera zoom is figured into the cloud
			//alpha. The clouds will appear to fade out as camera zooms in.
			cloudLayer.Color = new Color (new Vector4 (
			1.0f, 1.0f, 1.0f, 2 / (2f * camera.Zoom + 1.0f)));

			//set position
			groundLayer.CameraPosition = camera.Position;
			detailLayer.CameraPosition = camera.Position;
			rockLayer.CameraPosition = camera.Position;
			//to acheive a paralax effect, scale down cloud movement
			cloudLayer.CameraPosition = camera.Position / 3.0f;

			//The animcated sprite's origin is set so that rotation
			//will occur around the camera center (accounting for scale)
			animatedSprite.Origin = (camera.Position - animatedSpritePosition)
 / animatedSpriteScale.X;

			//changes have been accounted for, reset the changed value so that this
			//function is not called unnecessarily
			camera.ResetChanged ();
		}
	#endregion

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			//since we're drawing in order from back to front,
			//depth buffer is disabled
			// TODO graphics.GraphicsDevice.RenderState.DepthBufferEnable = false;
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);

			//draw the background layers
			groundLayer.Color = Color.LightGray;

			groundLayer.Draw (spriteBatch);
			detailLayer.Draw (spriteBatch);
			rockLayer.Draw (spriteBatch);

			animatedSprite.Draw (spriteBatch, Color.AntiqueWhite, 
		BlendState.AlphaBlend);

			//draw the clouds
			cloudLayer.Draw (spriteBatch);


			base.Draw (gameTime);
		}

	#endregion

	#region Handle Input

		/// <summary>
		/// Handles input for quitting the game.
		/// </summary>
		private void HandleInput ()
		{
			lastKeyboardState = currentKeyboardState;
#if IPHONE
			lastGamePadState = currentGamePadState;
#endif
			
			currentKeyboardState = Keyboard.GetState ();
#if IPHONE
			currentGamePadState = GamePad.GetState (PlayerIndex.One);
			// Check for exit.
			if (currentKeyboardState.IsKeyDown (Keys.Escape) || 
		currentGamePadState.Buttons.Back == ButtonState.Pressed) {
				Exit ();
			}
#endif			
			
		}

		/// <summary>
		/// Uses a pair of keys to simulate a positive or negative axis input.
		/// </summary>
		private static float ReadKeyboardAxis (KeyboardState keyState, Keys downKey, 
						Keys upKey)
		{
			float value = 0;

			if (keyState.IsKeyDown (downKey))
				value -= 1.0f;

			if (keyState.IsKeyDown (upKey))
				value += 1.0f;

			return value;
		}

	#endregion
	}
}