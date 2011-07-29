using System;
using System.Collections.Generic;

#if ANDROID
using Android.App;
#endif

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Samples.Draw2D
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;		
		Texture2D texture;
		SpriteFont font;
		Vector2 position;
		FPSCounterComponent fps;
		int wheel;
		KeyboardState oldKeyboardState;
		
		
#if ANDROID 
		public Game1 (Activity activity) : base (activity)
#else 
        public Game1 ()  
#endif
		{
			graphics = new GraphicsDeviceManager (this);
			
			Content.RootDirectory = "Content";
			
			graphics.PreferMultiSampling = true;
			graphics.IsFullScreen = false;	

			Window.AllowUserResizing = false;

			TargetElapsedTime = TimeSpan.FromSeconds(1f/30);
			
			graphics.SupportedOrientations = DisplayOrientation.Portrait | DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight | DisplayOrientation.PortraitUpsideDown;
		}
		
		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize ()
		{
			// TODO: Add your initialization logic here

			position = new Vector2 (250,20);
			wheel = 0;
			
			base.Initialize ();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);

			// TODO: use this.Content to load your game content here
			texture = Content.Load<Texture2D> ("monogameicon");
			// TODO ball = Content.Load<Texture2D> ("purpleBall.xnb");
			font = Content.Load<SpriteFont> ("spriteFont1");

			fps = new FPSCounterComponent (this,spriteBatch,font);
			Components.Add(fps);
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime)
		{
			KeyboardState ks = Keyboard.GetState();
			MouseState mouse = Mouse.GetState();
			
			if (ks[Keys.Escape] == KeyState.Down)
				base.Exit();
			
			if (ks[Keys.Left] == KeyState.Down || mouse.LeftButton == ButtonState.Pressed)
				position.X -= 1f;
			else if (ks[Keys.Right] == KeyState.Down || mouse.RightButton == ButtonState.Pressed)
				position.X += 1f;
			else if (mouse.MiddleButton == ButtonState.Pressed)
				position = new Vector2(mouse.X, mouse.Y);
			
			if (wheel - mouse.ScrollWheelValue > 0)
				position.Y -= 1f;
			else if (wheel - mouse.ScrollWheelValue < 0)
				position.Y += 1f;
			
			if (ks.IsKeyDown(Keys.Enter) && oldKeyboardState.IsKeyUp(Keys.Enter))
				graphics.ToggleFullScreen();
			
			wheel = mouse.ScrollWheelValue;
			    
			oldKeyboardState = ks;
			
			base.Update (gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);

			// Draw without blend
			spriteBatch.Begin (SpriteSortMode.Deferred, BlendState.AlphaBlend);			
			spriteBatch.Draw (texture, position, Color.White);	
			
			spriteBatch.DrawString(font, "Viewport: " + GraphicsDevice.Viewport.X + "," + GraphicsDevice.Viewport.Y + "," + GraphicsDevice.Viewport.Width + "," + GraphicsDevice.Viewport.Height, new Vector2(0, 30), Color.White);
			spriteBatch.DrawString(font, "ClientBounds: " + Window.ClientBounds, new Vector2(0, 50), Color.White);
			spriteBatch.DrawString(font, "BackBuffer: " + graphics.PreferredBackBufferWidth + ", " + graphics.PreferredBackBufferHeight, new Vector2(0, 70), Color.White);
			
			
			base.Draw(gameTime);
			
			spriteBatch.End ();
		}
	}
}
