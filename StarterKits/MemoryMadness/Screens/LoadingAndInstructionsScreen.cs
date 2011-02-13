#region File Description

//-----------------------------------------------------------------------------
// LoadingAndInstructionScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GameStateManagement;
using System.Threading;
using Microsoft.Xna.Framework.Input.Touch;

#endregion

namespace MemoryMadness
{
    class LoadingAndInstructionsScreen : GameScreen
    {
        #region Fields

        Texture2D background;
        SpriteFont font;
        bool isLoading;
        GameplayScreen gameplayScreen;
        Thread thread;

        int levelNumber;
        int movesPerformed;

        bool isResuming;

        #endregion

        #region Initialization

        public LoadingAndInstructionsScreen(bool isResuming)
        {
            levelNumber = 1;
            movesPerformed = 0;

            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            EnabledGestures = GestureType.Tap;

            // Initialize the current level and number of moves already performed
            // according to the game state information
            if (PhoneApplicationService.Current.State.ContainsKey("CurrentLevel"))
            {
                this.levelNumber = 
                    (int)PhoneApplicationService.Current.State["CurrentLevel"];
                PhoneApplicationService.Current.State.Remove("CurrentLevel");
            }

            if (PhoneApplicationService.Current.State.ContainsKey("MovesPerformed"))
            {
                this.movesPerformed = 
                    (int)PhoneApplicationService.Current.State["MovesPerformed"];
                PhoneApplicationService.Current.State.Remove("MovesPerformed");
            }

            this.isResuming = isResuming;
        }

        #endregion

        #region Loading

        /// <summary>
        /// Load the screen resources
        /// </summary>
        public override void LoadContent()
        {
            if (!isResuming)
            {
                background = Load<Texture2D>(@"Textures\Backgrounds\Instructions");
            }
            else
            {
                background = Load<Texture2D>(@"Textures\Backgrounds\Resuming");
            }
            font = Load<SpriteFont>(@"Fonts\MenuFont");

            // Create a new instance of the gameplay screen
            gameplayScreen = new GameplayScreen(levelNumber, movesPerformed);
            gameplayScreen.ScreenManager = ScreenManager;
        }

        #endregion

        #region Update

        /// <summary>
        /// Exit the screen after a tap gesture.
        /// </summary>
        /// <param name="input"></param>
        public override void HandleInput(InputState input)
        {
            if (!isLoading)
            {
                if (input.Gestures.Count > 0)
                {
                    if (input.Gestures[0].GestureType == GestureType.Tap)
                    {
                        // Start loading the gameplay resources in an additional thread
                        thread = new Thread(
                            new ThreadStart(gameplayScreen.LoadAssets));

                        isLoading = true;
                        thread.Start();
                    }
                }
            }
            base.HandleInput(input);
        }

        /// <summary>
        /// Screen update logic.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // If the additional thread is running, wait for it to finish
            if (null != thread)
            {
                // If the additional thread finished loading and the screen is not 
                // exiting, exit it
                if (thread.ThreadState == ThreadState.Stopped && !IsExiting)
                {
                    // Move on to the gameplay screen
                    foreach (GameScreen screen in ScreenManager.GetScreens())
                        screen.ExitScreen();

                    gameplayScreen.IsActive = true;
                    ScreenManager.AddScreen(gameplayScreen, null);
                }
            }
            // if resuming, don't wait for the user to launch the loading thread
            else if (isResuming)
            {
                thread = new Thread(
                            new ThreadStart(gameplayScreen.LoadAssets));

                isLoading = true;
                thread.Start();
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        #endregion

        #region Render

        /// <summary>
        /// Render screen 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw Background
            spriteBatch.Draw(background, new Vector2(0, 0),
                 Color.White * TransitionAlpha);

            // If loading gameplay screen resource in the 
            // background show "Loading..." text
            if (isLoading && !isResuming)
            {
                string text = "Loading...";
                Vector2 size = font.MeasureString(text);
                Vector2 position = new Vector2(
                    (ScreenManager.GraphicsDevice.Viewport.Width - size.X) / 2,
                    (ScreenManager.GraphicsDevice.Viewport.Height - size.Y) / 2);
                spriteBatch.DrawString(font, text, position, Color.White);
            }

            spriteBatch.End();
        }

        #endregion
    }
}
