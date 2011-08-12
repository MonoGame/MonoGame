#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System.Threading;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;


#endregion

namespace HoneycombRush
{
    class LevelOverScreen : GameScreen
    {
        #region Fields


        SpriteFont font36px;
        SpriteFont font16px;

        Rectangle safeArea;

        string text;
        bool isLoading;
        Vector2 textSize;

        DifficultyMode? difficultyMode;

        Thread thread;
        GameplayScreen gameplayScreen;
		
	bool assetsLoaded = false;	

        #endregion

        #region Initialization


        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="difficultyMode">The next level</param>
        public LevelOverScreen(string text, DifficultyMode? difficultyMode)
        {
            this.text = text;
            EnabledGestures = GestureType.Tap;
            this.difficultyMode = difficultyMode;
        }

        /// <summary>
        /// Load screen resources
        /// </summary>
        public override void LoadContent()
        {
            if (difficultyMode.HasValue)
            {
                gameplayScreen = new GameplayScreen(difficultyMode.Value);
                gameplayScreen.ScreenManager = ScreenManager;
            }
            font36px = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameScreenFont36px");
            font16px = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameScreenFont16px");
            textSize = font36px.MeasureString(text);
            safeArea = SafeArea;

            base.LoadContent();
        }


        #endregion

        #region Update


        /// <summary>
        /// Update the screen
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="otherScreenHasFocus">Whether another screen has the focus.</param>
        /// <param name="coveredByOtherScreen">Whether this screen is covered by another.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // If null don't do anything
            if (null != thread)
            {
                // If we finishedloading the assets, add the game play screen
                if (thread.ThreadState == ThreadState.Stopped)
                {
                    // Exit all the screen
                    foreach (GameScreen screen in ScreenManager.GetScreens())
                    {
                        screen.ExitScreen();
                    }

                    // Add the gameplay screen
                    if (difficultyMode.HasValue)
                    {
                        ScreenManager.AddScreen(gameplayScreen, null);
                    }
                }
            }
            else if (assetsLoaded)
            {
                // Screen is not exiting
                if ( !IsExiting)
                {
                    // Move on to the game play screen once highscore data is loaded                    
                    foreach (GameScreen screen in ScreenManager.GetScreens())
                    {
                        screen.ExitScreen();
                    }

                    // Add the gameplay screen
                    if (difficultyMode.HasValue)
                    {
                        ScreenManager.AddScreen(gameplayScreen, null);
                    }
                }
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Handle any input from the user
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="input"></param>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            PlayerIndex player;

            // Return to the main menu when a tap gesture is recognized
            if (input.Gestures.Count > 0)
            {
                GestureSample sample = input.Gestures[0];
                if (sample.GestureType == GestureType.Tap)
                {
                    StartNewLevelOrExit(input);
                    input.Gestures.Clear();
                }
            }
            // Handle keyboard
            else if (input.IsNewKeyPress(Keys.Enter, ControllingPlayer, out player) ||
                input.IsNewKeyPress(Keys.Space, ControllingPlayer, out player))
            {
                StartNewLevelOrExit(input);
            }

            base.HandleInput(gameTime, input);
        }


        #endregion

        #region Render


        /// <summary>
        /// Renders the screen
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw the footer text

            if (difficultyMode.HasValue)
            {
#if WINDOWS_PHONE
                string actionText = "Touch to start next level";
#else
                string actionText = "Press space to start next level";
#endif

                spriteBatch.DrawString(font16px, actionText,
                    new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 -
                        font16px.MeasureString(actionText).X / 2,
                        safeArea.Bottom - font16px.MeasureString(actionText).Y - 4),
                    Color.Black);
            }
            else
            {
#if WINDOWS_PHONE
                string actionText = "Touch to end game";
#else
                string actionText = "Press space to end game";
#endif
                spriteBatch.DrawString(font16px, actionText,
                    new Vector2(safeArea.Left + safeArea.Width / 2 - font16px.MeasureString(actionText).X / 2,
                        safeArea.Top + safeArea.Height - font16px.MeasureString(actionText).Y - 4),
                    Color.Black);
            }

            spriteBatch.End();
        }


        #endregion

        #region Private Functions


        /// <summary>
        /// Starts new level or exit to High Score
        /// </summary>
        /// <param name="input"></param>
        private void StartNewLevelOrExit(InputState input)
        {
            // If there is no next level - go to high score screen
            if (!difficultyMode.HasValue)
            {
                // If is in high score, gets is name
                if (GameplayScreen.FinalScore != 0 && HighScoreScreen.IsInHighscores(GameplayScreen.FinalScore))
                {
                    Guide.BeginShowKeyboardInput(PlayerIndex.One,
                        "Player Name", "What is your name (max 15 characters)?", "Player",
                        AfterPlayerEnterName, null);
                }
                else
                {
                    foreach (GameScreen screen in ScreenManager.GetScreens())
                    {
                        screen.ExitScreen();
                    }

                    ScreenManager.AddScreen(new BackgroundScreen("highScoreScreen"), null);
                    ScreenManager.AddScreen(new HighScoreScreen(), null);
                }
            }
            // If not already loading
            else if (!isLoading)
            {
#if MAC			
		// Start loading the resources on main thread
		// If not then all sorts of errors happen for 
		// AutoReleasPools and OpenGL does not handle 
		// multiple thread to well when using Thread
		MonoMac.AppKit.NSApplication.SharedApplication.BeginInvokeOnMainThread(delegate {  
			gameplayScreen.LoadAssets ();
			isLoading = false;
			assetsLoaded = true;
		});
#else				
                // Start loading the resources in an additional thread
                thread = new Thread(new ThreadStart(gameplayScreen.LoadAssets));

                isLoading = true;
                thread.Start();
#endif				
            }
        }

        /// <summary>
        /// A handler invoked after the user has enter his name.
        /// </summary>
        /// <param name="result"></param>
        private void AfterPlayerEnterName(IAsyncResult result)
        {
            // Gets the name entered
            string playerName = Guide.EndShowKeyboardInput(result);
            if (!string.IsNullOrEmpty(playerName))
            {
                // Ensure that it is valid
                if (playerName != null && playerName.Length > 15)
                    playerName = playerName.Substring(0, 15);

                // Puts it in high score
                HighScoreScreen.PutHighScore(playerName, GameplayScreen.FinalScore);
                HighScoreScreen.HighScoreChanged();
            }

            // Moves to the next screen
            foreach (GameScreen screen in ScreenManager.GetScreens())
            {
                screen.ExitScreen();
            }

            ScreenManager.AddScreen(new BackgroundScreen("highScoreScreen"), null);
            ScreenManager.AddScreen(new HighScoreScreen(), null);
        }


        #endregion
    }
}
