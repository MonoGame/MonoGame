#region File Description
//-----------------------------------------------------------------------------
// WaypointSample.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

#endregion

namespace Waypoint
{
    /// <summary>
    /// This sample shows how an AI can navigate from point to point using 
    /// several different behaviors
    /// </summary>
    public class WaypointSample : Game
    {
        #region Constants

        /// <summary>
        /// Screen width in pixels
        /// </summary>
        const int screenWidth = 320;
        /// <summary>
        /// Screen height in pixels
        /// </summary>
        const int screenHeight = 480;

        /// <summary>
        /// Cursor move speed in pixels per second
        /// </summary>
        const float cursorMoveSpeed = 250.0f;

        #endregion

        #region Fields

        // Graphics data
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        // Cursor data
        Texture2D cursorTexture;
        Vector2 cursorCenter;
        Vector2 cursorLocation;

        // HUD data
        SpriteFont hudFont;
        // Where the HUD draws on the screen
        Vector2 hudLocation;

        // Input data
        KeyboardState previousKeyboardState;
        GamePadState previousGamePadState;
        KeyboardState currentKeyboardState;
        GamePadState currentGamePadState;
		TouchCollection currentTouchCollection;

        // The waypoint-following tank
        Tank tank;

        #endregion

        #region Initialization

        /// <summary>
        /// Construct a WaypointSample object
        /// </summary>
        public WaypointSample()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;

            tank = new Tank(this);
            Components.Add(tank);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting
        /// to run. This is where it can query for any required services and load and
        /// non-graphic related content.  Calling base.Initialize will enumerate 
        /// through any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            // This places the HUD near the upper left corner of the screen
            hudLocation = new Vector2(
                (float)Math.Floor(screenWidth * .1f), 
                (float)Math.Floor(screenHeight * .1f));

            // places the cursor in the center of the screen
            cursorLocation = 
                new Vector2((float)screenWidth / 2, (float)screenHeight / 2);
            
            // places the tank halfway between the center of the screen and the
            // upper left corner
            tank.Reset(
                new Vector2((float)screenWidth / 4, (float)screenHeight / 4));

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            cursorTexture = Content.Load<Texture2D>("cursor");
            cursorCenter = 
                new Vector2(cursorTexture.Width / 2, cursorTexture.Height / 2);

            hudFont = Content.Load<SpriteFont>("HUDFont");
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
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            HandleInput(elapsedTime);
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);

            string HudString = "Behavior : " + tank.BehaviorType.ToString();

            spriteBatch.Begin();

            // Draw the cursor
            spriteBatch.Draw(cursorTexture, cursorLocation, null, Color.White, 0f,
                cursorCenter, 1f, SpriteEffects.None, 0f);

            // Draw the string for current behavior
            spriteBatch.DrawString(hudFont, HudString, hudLocation, Color.White);
            
            spriteBatch.End();
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Read keyboard and gamepad input
        /// </summary>
        private void HandleInput(float elapsedTime)
        {
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentKeyboardState = Keyboard.GetState();
			
			currentTouchCollection = TouchPanel.GetState();
			
			//bool touched = false;
			int touchCount = 0;
				
			// tap the screen to select				
			foreach (TouchLocation location in currentTouchCollection)
			{
			    switch (location.State)
			    {
			        case TouchLocationState.Pressed:	
						//touched = true;	
					    touchCount++;
					    cursorLocation = location.Position;
			            break;
			        case TouchLocationState.Moved:
			            break;
			        case TouchLocationState.Released:
			            break;
			    }	
			}

            // Allows the game to exit
            if (currentGamePadState.Buttons.Back == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            // Update the cursor location by listening for left thumbstick input on
            // the GamePad and direction key input on the Keyboard, making sure to
            // keep the cursor inside the screen boundary
            cursorLocation.X += 
                currentGamePadState.ThumbSticks.Left.X * cursorMoveSpeed * elapsedTime;
            cursorLocation.Y -= 
                currentGamePadState.ThumbSticks.Left.Y * cursorMoveSpeed * elapsedTime;

            if (currentKeyboardState.IsKeyDown(Keys.Up))
            {
                cursorLocation.Y -= elapsedTime * cursorMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Down))
            {
                cursorLocation.Y += elapsedTime * cursorMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Left))
            {
                cursorLocation.X -= elapsedTime * cursorMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right))
            {
                cursorLocation.X += elapsedTime * cursorMoveSpeed;
            }
            cursorLocation.X = MathHelper.Clamp(cursorLocation.X, 0f, screenWidth);
            cursorLocation.Y = MathHelper.Clamp(cursorLocation.Y, 0f, screenHeight);

            // Change the tank move behavior if the user pressed B on
            // the GamePad or on the Keyboard.
            if ((previousGamePadState.Buttons.B == ButtonState.Released &&
                currentGamePadState.Buttons.B == ButtonState.Pressed) ||
                (previousKeyboardState.IsKeyUp(Keys.B) &&
                currentKeyboardState.IsKeyDown(Keys.B)) || ( touchCount == 2 ))
            {
                tank.CycleBehaviorType();
            }

            // Add the cursor's location to the WaypointList if the user pressed A on
            // the GamePad or on the Keyboard.
            if ((previousGamePadState.Buttons.A == ButtonState.Released &&
                currentGamePadState.Buttons.A == ButtonState.Pressed) ||
                (previousKeyboardState.IsKeyUp(Keys.A) &&
                currentKeyboardState.IsKeyDown(Keys.A)) || ( touchCount == 1 ))
            {
                	tank.Waypoints.Enqueue(cursorLocation);
            }

            // Delete all the current waypoints and reset the tanks’ location if 
            // the user pressed X on the GamePad or on the Keyboard.
            if ((previousGamePadState.Buttons.X == ButtonState.Released &&
                currentGamePadState.Buttons.X == ButtonState.Pressed) ||
                (previousKeyboardState.IsKeyUp(Keys.X) &&
                currentKeyboardState.IsKeyDown(Keys.X)) || ( touchCount == 3 ))
            {
                tank.Reset(
                    new Vector2((float)screenWidth / 4, (float)screenHeight / 4));
            }
        }

        #endregion
    }
}
