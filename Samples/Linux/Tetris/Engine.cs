using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#if !LINUX

using MonoMac.Foundation;

#endif

namespace Tetris
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Engine : Microsoft.Xna.Framework.Game
	{
		// Graphics
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D tetrisBackground, tetrisTextures;
		SpriteFont gameFont;
		readonly Rectangle[] blockRectangles = new Rectangle[7];

		// Game
		Board board;
		Score score;
		bool pause = false;

		// Input
		KeyboardState oldKeyboardState = Keyboard.GetState ();

		public Engine ()
			{
			graphics = new GraphicsDeviceManager (this);

#if !LINUX			
			Content.RootDirectory = Path.Combine (NSBundle.MainBundle.ResourcePath, "Content");
#else
			Content.RootDirectory = "Content";
#endif
			
			// Create sprite rectangles for each figure in texture file
			// O figure
			blockRectangles [0] = new Rectangle (312, 0, 24, 24);
			// I figure
			blockRectangles [1] = new Rectangle (0, 24, 24, 24);
			// J figure
			blockRectangles [2] = new Rectangle (120, 0, 24, 24);
			// L figure
			blockRectangles [3] = new Rectangle (216, 24, 24, 24);
			// S figure
			blockRectangles [4] = new Rectangle (48, 96, 24, 24);
			// Z figure
			blockRectangles [5] = new Rectangle (240, 72, 24, 24);
			// T figure
			blockRectangles [6] = new Rectangle (144, 96, 24, 24);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize ()
		{
			Window.Title = "MonoGame XNA Tetris 2D";

			graphics.PreferredBackBufferHeight = 600;
			graphics.PreferredBackBufferWidth = 800;

			this.TargetElapsedTime = TimeSpan.FromSeconds (1.0f / 10.0f);

			// Try to open file if it exists, otherwise create it
			using (FileStream fileStream = File.Open ("record.dat", FileMode.OpenOrCreate)) {
				fileStream.Close ();
			}

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

			// Add the SpriteBatch service
			Services.AddService (typeof(SpriteBatch), spriteBatch);

			//Load 2D textures
			tetrisBackground = Content.Load<Texture2D> ("background");
			tetrisTextures = Content.Load<Texture2D> ("tetris");

			// Load game font
			//gameFont = Content.Load<SpriteFont> ("font");
			gameFont = Content.Load<SpriteFont> ("Arial");

			// Create game field
			board = new Board (this, ref tetrisTextures, blockRectangles);
			board.Initialize ();
			Components.Add (board);

			// Save player's score and game level
			score = new Score (this, gameFont);
			score.Initialize ();
			Components.Add (score);

			// Load game record
			using (StreamReader streamReader = File.OpenText ("record.dat")) {
				string player = null;
				if ((player = streamReader.ReadLine ()) != null)
					score.RecordPlayer = player;
				int record = 0;
				if ((record = Convert.ToInt32 (streamReader.ReadLine ())) != 0)
					score.RecordScore = record;
			}
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime)
		{
			// Allows the game to exit
			KeyboardState keyboardState = Keyboard.GetState ();
			if (keyboardState.IsKeyDown (Keys.Escape))
				this.Exit ();

			// Check pause
			bool pauseKey = (oldKeyboardState.IsKeyDown (Keys.P) && (keyboardState.IsKeyUp (Keys.P)));

			oldKeyboardState = keyboardState;

			if (pauseKey)
				pause = !pause;

			if (!pause) {
				// Find dynamic figure position
				board.FindDynamicFigure ();

				// Increase player score
				int lines = board.DestroyLines ();
				if (lines > 0) {
					score.Value += (int)((5.0f / 2.0f) * lines * (lines + 3));
					board.Speed += 0.005f;
				}

				score.Level = (int)(10 * board.Speed);

				// Create new shape in game
				if (!board.CreateNewFigure ())
					GameOver ();
				else {
					// If left key is pressed
					if (keyboardState.IsKeyDown (Keys.Left))
						board.MoveFigureLeft ();
					// If right key is pressed
					if (keyboardState.IsKeyDown (Keys.Right))
						board.MoveFigureRight ();
					// If down key is pressed
					if (keyboardState.IsKeyDown (Keys.Down))
						board.MoveFigureDown ();

					// Rotate figure
					if (keyboardState.IsKeyDown (Keys.Up) || keyboardState.IsKeyDown (Keys.Space))
						board.RotateFigure ();

					// Moving figure
					if (board.Movement >= 1) {
						board.Movement = 0;
						board.MoveFigureDown ();
					} else
						board.Movement += board.Speed;
				}
			}

			base.Update (gameTime);
		}

		private void GameOver ()
		{
			if (score.Value > score.RecordScore) {
				score.RecordScore = score.Value;

				pause = true;

				Record record = new Record ();
				//record.ShowDialog ();

				score.RecordPlayer = record.Player;

				using (StreamWriter writer = File.CreateText ("record.dat")) {
					writer.WriteLine (score.RecordPlayer);
					writer.WriteLine (score.RecordScore);
				}

				pause = false;
			}
			board.Initialize ();
			score.Initialize ();
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			spriteBatch.Begin ();
			spriteBatch.Draw (tetrisBackground, Vector2.Zero, Color.White);

			base.Draw (gameTime);
			spriteBatch.End ();
		}
	}
}