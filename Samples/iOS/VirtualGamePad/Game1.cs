using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;


namespace Microsoft.Xna.Samples.VirtualGamePad
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;		
		Texture2D texture, caracter;
		Vector2 position = new Vector2();
		Color caracterColor = Color.White;
		SpriteFont font;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
			
			graphics.IsFullScreen = true;					
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

			// Set the initial mouse position
			Mouse.SetPosition(160,240);
			
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

            // TODO: use this.Content to load your game content here
            texture = Content.Load<Texture2D>("gamepad.png");
			caracter = Content.Load<Texture2D>("monogameicon");
			font = Content.Load<SpriteFont>("font");
			
			// Set the virtual GamePad
			ButtonDefinition BButton = new ButtonDefinition();
			BButton.Texture = texture;
			BButton.Position = new Vector2(200,150);
			BButton.Type = Buttons.B;
			BButton.TextureRect = new Rectangle(72,77,36,36);
			
			ButtonDefinition AButton = new ButtonDefinition();
			AButton.Texture = texture;
			AButton.Position = new Vector2(150,150);
			AButton.Type = Buttons.A;
			AButton.TextureRect = new Rectangle(73,114,36,36);
			
			GamePad.ButtonsDefinitions.Add(BButton);
			GamePad.ButtonsDefinitions.Add(AButton);
			
			ThumbStickDefinition thumbStick = new ThumbStickDefinition();
			thumbStick.Position = new Vector2(200,200);
			thumbStick.Texture = texture;
			thumbStick.TextureRect = new Rectangle(2,2,68,68);
			
			GamePad.LeftThumbStickDefinition = thumbStick;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {			
			 // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
			
			 // TODO: Add your update logic here
			
			if (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed)
				caracterColor = Color.Green;
						
			if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed)
				caracterColor = Color.Red;
						
			//  change the caracter position and center in touch
            //MouseState mouse = Mouse.GetState();
            //position.X = mouse.X - (caracter.Width/2);
            //position.Y = mouse.Y - (caracter.Height/2);
			
			// change the caracter position using thumbstick
			GamePadState gamepastatus = GamePad.GetState(PlayerIndex.One);
			position.Y += (int) (gamepastatus.ThumbSticks.Left.Y * -4);
			position.X += (int) (gamepastatus.ThumbSticks.Left.X * 4);
			
			// change the caracter position using accelerometer
			//position.Y += (int)(Accelerometer.GetState().Acceleration.Y * -4);
            //position.X += (int)(Accelerometer.GetState().Acceleration.X * 4);
						
			//  Keep inside the screen
            //  right
            if(position.X + caracter.Width >  Window.ClientBounds.Width)
			{
               position.X = Window.ClientBounds.Width - caracter.Width;
			}
            //  bottom
            if (position.Y + caracter.Height > Window.ClientBounds.Height)
			{
                position.Y = Window.ClientBounds.Height - caracter.Height;
			}
            //  left
            if (position.X < 0)
			{
                position.X = 0;
			}
            //  top
            if (position.Y < 0)
			{
                position.Y = 0;
			}
			
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
           	graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
			
			spriteBatch.Begin();
			spriteBatch.Draw(caracter,position,caracterColor);
			spriteBatch.DrawString(font,GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.ToString(),Vector2.One,Color.Black);
			spriteBatch.DrawString(font,Accelerometer.GetState().Acceleration.ToString(),new Vector2(1,40),Color.Black);
			
			// Draw the virtual GamePad
			GamePad.Draw(gameTime,spriteBatch);
			
			spriteBatch.End();
			
            base.Draw(gameTime);
        }
    }
}
