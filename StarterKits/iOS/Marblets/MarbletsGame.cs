#region File Description
//-----------------------------------------------------------------------------
// MarbletsGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Marblets
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	partial class MarbletsGame : Game
	{
		/// <summary>
		/// A cache of content used by the game
		/// </summary>
		public new static ContentManager Content;

		/// <summary>
		/// The game settings from settings.xml
		/// </summary>
		public static Settings Settings = new Settings();

		/// <summary>
		/// The current game state
		/// </summary>
		public static GameState GameState = GameState.Started;

		/// <summary>
		/// The new game state
		/// </summary>
		public static GameState NextGameState = GameState.None;

		public static int Score; //= 0;

		public static List<int> HighScores;

		private GraphicsDeviceManager graphics;
		private GameScreen mainGame;
		private TitleScreen splashScreen;
		private InputHelper inputHelper;
		private KeyboardState keyState;


		public MarbletsGame()
		{
			//Create the content pipeline manager.
			base.Content.RootDirectory = "Content";
			MarbletsGame.Content = base.Content;

			//Set up the device to be HD res. The RelativeSpriteBatch will handle 
			//resizing for us
			graphics = new GraphicsDeviceManager(this);

			// If the window size changes, then our  drawable area changes and the 
			// game graphics might not fit.  
			// Hook into DeviceReset event so we can resize the graphics.
			graphics.DeviceReset += new EventHandler<EventArgs>(OnGraphicsComponentDeviceReset);

			graphics.PreferredBackBufferWidth = 320;
			graphics.PreferredBackBufferHeight = 480;

			Window.AllowUserResizing = true;

			mainGame = 
                new GameScreen(this, "Textures/play_frame", SoundEntry.MusicGame);
			mainGame.Enabled = false;
			mainGame.Visible = false;
			this.Components.Add(mainGame);

			splashScreen = 
                new TitleScreen(this, "Textures/title_frame", SoundEntry.MusicTitle);
			splashScreen.Enabled = true;
			splashScreen.Visible = true;
			this.Components.Add(splashScreen);

			inputHelper = new InputHelper(this);
			inputHelper.UpdateOrder = int.MinValue;
			this.Components.Add(inputHelper);
		}


		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to
		/// run. This is where it can query for any required services and load any 
		/// non-graphic related content.  Calling base.Initialize will enumerate through
		/// any components and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			//This will call initialize on all the game components
			base.Initialize();

			// create initial high scores
			HighScores = new List<int>();
			for(int i = 0; i < 5; i++)
			{
				HighScores.Add(500 - i * 100);
			}
		}


		/// <summary>
		/// Load your graphics content.
		/// </summary>
		protected override void LoadContent()
		{
			Sound.LoadContent(Content);

			IGraphicsDeviceService graphicsService = 
                (IGraphicsDeviceService)Services.GetService(
				typeof(IGraphicsDeviceService));

			//Ask static helper objects to reload too
			Font.LoadContent();
		}


		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			//Handle FullScreen
			keyState = Keyboard.GetState();

			// Allows the default game to exit on Xbox 360 and Windows
			if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                AltComboPressed(keyState, Keys.F4) ||
                keyState.IsKeyDown(Keys.Escape))
			{
				this.Exit();
			}

			if(InputHelper.GamePads[PlayerIndex.One].StartPressed)
			{
				NextGameState = GameState.Started;
			}

			if(NextGameState != GameState.None)
			{
				ChangeGameState();
			}
		}


		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.Black);
			//Nothing to draw the components will handle it
			base.Draw(gameTime);
		}


		private void ChangeGameState()
		{
			//Marblets only has 3 game states - the splash screen and the game screen
			//in 2d and 3d Since they are both game components of type screen just 
			//making the right one visible will cause the correct background to be shown
			//and the right music to be played
			if((GameState == GameState.Started) && (NextGameState == GameState.Play2D))
			{
				Score = 0;
				splashScreen.Enabled = false;
				splashScreen.Visible = false;
				mainGame.Enabled = true;
				mainGame.Visible = true;
				//Start a new game - reset score and board etc
				mainGame.NewGame();
				GameState = NextGameState;
			}
			else if(NextGameState == GameState.Started)
			{
				splashScreen.Enabled = true;
				splashScreen.Visible = true;
				mainGame.Enabled = false;
				mainGame.Visible = false;

				GameState = NextGameState;

			}
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			splashScreen.Shutdown();
			mainGame.Shutdown();

			base.OnExiting(sender, args);
		}

		/// <summary>
		/// Checks whether an alt+key combo is pressed.
		/// </summary>
		private static bool AltComboPressed(KeyboardState state, Keys key)
		{
			return state.IsKeyDown(key) &&
                   (state.IsKeyDown(Keys.LeftAlt) ||
                    state.IsKeyDown(Keys.RightAlt));
		}

		/// <summary>
		/// Resize the game graphics when the window size changes
		/// </summary>
		void OnGraphicsComponentDeviceReset(object sender, EventArgs e)
		{
			mainGame.SpriteBatch.Resize();
		}
	}

	/// <summary>
	/// This enum is for the state transitions.
	/// </summary>
	public enum GameState
	{
		/// <summary>
		/// Default value - means no state is set
		/// </summary>
		None,

		/// <summary>
		/// Nothing visible, game has just been run and nothing is initialized
		/// </summary>
		Started,

		/// <summary>
		/// Logo Screen is being displayed
		/// </summary>
		LogoSplash,

		/// <summary>
		/// Currently playing the 2d version
		/// </summary>
		Play2D,

	}

}