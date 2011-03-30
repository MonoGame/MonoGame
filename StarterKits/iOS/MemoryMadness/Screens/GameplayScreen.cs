#region File Description

//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using Microsoft.Xna.Framework.GamerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Input.Touch;

#endregion

namespace MemoryMadness
{
    class GameplayScreen : GameScreen
    {
        #region Fields

        private bool isActive;

        private bool isLevelChange;

        /// <summary>
        /// Sets/gets whether or not the game is active. Set operations propogate
        /// to the current level.
        /// </summary>
        public new bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;

                if (null != currentLevel)
                    currentLevel.IsActive = value;
            }
        }

        bool moveToHighScore = false;

        // Gameplay variables        
        public Level currentLevel;
        int currentLevelNumber;
        int movesPerformed = 0;

        int maxLevelNumber;

        // Rendering variables
        SpriteFont levelNumberFont;
        SpriteFont textFont;
        Texture2D background;
        Texture2D buttonsTexture;

        // Input related variables
        TimeSpan inputTimeMeasure;
        TimeSpan inputGracePeriod = TimeSpan.FromMilliseconds(150);
        TouchInputState inputState = TouchInputState.Idle;
        List<TouchLocation> lastPressInput;

        #endregion

        #region Initializations

        public GameplayScreen(int levelNumber, int movesPerformed)
            : this(levelNumber)
        {
            this.movesPerformed = movesPerformed;
        }
        public GameplayScreen(int levelNumber)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.0);

            currentLevelNumber = levelNumber;
        }

        #endregion

        #region Loading

        /// <summary>
        /// Load the game content
        /// </summary>
        public override void LoadContent()
        {
            XDocument doc = XDocument.Load(@"Content\Gameplay\LevelDefinitions.xml");
            var levels = doc.Document.Descendants(XName.Get("Level"));
            foreach (var level in levels)
            {
                maxLevelNumber++;
            }

            // Resolution for a possible situation which can occur while debugging the 
            // game. The game may remember it is on a level which is higher than the
            // highest available level, following a change to the definition file.
            if (currentLevelNumber > maxLevelNumber)
                currentLevelNumber = 1;

            InitializeLevel();

            base.LoadContent();
        }

        /// <summary>
        /// Initialize the level portrayed by the gameplay screen.
        /// </summary>
        private void InitializeLevel()
        {
            currentLevel = new Level(ScreenManager.Game,
                         ScreenManager.SpriteBatch,
                         currentLevelNumber, movesPerformed, buttonsTexture);
            currentLevel.IsActive = true;

            ScreenManager.Game.Components.Add(currentLevel);
        }

        /// <summary>
        /// Load assets used by the gameplay screen.
        /// </summary>
        public void LoadAssets()
        {
            levelNumberFont =
                ScreenManager.Game.Content.Load<SpriteFont>(@"Fonts\GameplayLargeFont");
            textFont =
                ScreenManager.Game.Content.Load<SpriteFont>(@"Fonts\GameplaySmallFont");
            background =
                ScreenManager.Game.Content.Load<Texture2D>(
                    @"Textures\Backgrounds\gameplayBG");
            buttonsTexture =
                ScreenManager.Game.Content.Load<Texture2D>(@"Textures\ButtonStates");
        }

        #endregion

        #region Update
        /// <summary>
        /// Handle user input.
        /// </summary>
        /// <param name="input">The input to handle.</param>
        public override void HandleInput(InputState input)
        {
            if (IsActive)
            {
                if (input == null)
                    throw new ArgumentNullException("input");

                if (input.IsPauseGame(null))
                {
                    PauseCurrentGame();
                }

                if (input.TouchState.Count > 0)
                {
                    // We are about to handle touch input
                    switch (inputState)
                    {
                        case TouchInputState.Idle:
                            // We have yet to receive input, start grace period
                            inputTimeMeasure = TimeSpan.Zero;
                            inputState = TouchInputState.GracePeriod;
                            lastPressInput = new List<TouchLocation>();
                            foreach (var touch in input.TouchState)
                            {
                                if (touch.State == TouchLocationState.Pressed)
                                {
                                    lastPressInput.Add(touch);
                                }
                            }
                            break;
                        case TouchInputState.GracePeriod:
                            // Do nothing during the grace period other than remembering 
                            // additional presses
                            foreach (var touch in input.TouchState)
                            {
                                if (touch.State == TouchLocationState.Pressed)
                                {
                                    lastPressInput.Add(touch);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Update all the game component
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // Do not advance to the highscore screen if sounds are playing
            if (moveToHighScore && !AudioManager.AreSoundsPlaying())
            {
                ScreenManager.Game.Components.Remove(currentLevel);

                foreach (GameScreen screen in ScreenManager.GetScreens())
                    screen.ExitScreen();

                ScreenManager.AddScreen(new BackgroundScreen(true), null);
                ScreenManager.AddScreen(new HighScoreScreen(), null);
            }

            // Do not perform advance update logic if the game is inactive or we are
            // moving to the highscore screen
            if (!IsActive || moveToHighScore)
            {
                base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
                return;
            }

            if ((inputState == TouchInputState.GracePeriod) && (isActive))
            {
                inputTimeMeasure += gameTime.ElapsedGameTime;

                // if the input grace period is over, handle the touch input
                if (inputTimeMeasure >= inputGracePeriod)
                {
                    currentLevel.RegisterTouch(lastPressInput);
                    inputState = TouchInputState.Idle;
                }
            }

            // If the user passed the level, advance to the next or finish the game if
            // the current level was last
            if (currentLevel.CurrentState == LevelState.FinishedOk && isActive)
            {
                AudioManager.PlaySound("success");

                if (currentLevelNumber < maxLevelNumber)
                {
                    currentLevelNumber++;
                    isLevelChange = true;
                }
                else
                {
                    FinishCurrentGame();
                }
            }
            // If the user failed to pass the level, revert to level one, allowing the
            // user to register a highscore if he reached a high enough level
            else if (currentLevel.CurrentState == LevelState.FinishedFail)
            {               
                isActive = false;

                if (HighScoreScreen.IsInHighscores(currentLevelNumber))
                {
                    // The player has a highscore - show the device's keyboard
                    Guide.BeginShowKeyboardInput(PlayerIndex.One,
                        Constants.HighscorePopupTitle, Constants.HighscorePopupText,
                        Constants.HighscorePopupDefault, ShowHighscorePromptEnded,
                        false);
                }
                else
                {
                    AudioManager.PlaySound("fail");
                    isActive = true;
                    currentLevelNumber = 1;
                    isLevelChange = true;
                }
            }

            if (isLevelChange)
            {
                ScreenManager.Game.Components.Remove(currentLevel);

                currentLevel = new Level(ScreenManager.Game,
                                            ScreenManager.SpriteBatch,
                                            currentLevelNumber, buttonsTexture);
                currentLevel.IsActive = true;

                ScreenManager.Game.Components.Add(currentLevel);

                isLevelChange = false;
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        #endregion

        #region Render

        /// <summary>
        /// Draw The gameplay screen
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.CornflowerBlue);
            ScreenManager.SpriteBatch.Begin();

            ScreenManager.SpriteBatch.Draw(background, Vector2.Zero, Color.White);

            if (IsActive)
            {
                string text;
                Vector2 size;
                Vector2 position;

                if (currentLevel.CurrentState == LevelState.NotReady)
                {
                    text = "Preparing...";
                    size = textFont.MeasureString(text);
                    position = new Vector2((ScreenManager.GraphicsDevice.Viewport.Width - size.X) / 2,
                        (ScreenManager.GraphicsDevice.Viewport.Height - size.Y) / 2);
                    position.X += 20f;
                    ScreenManager.SpriteBatch.DrawString(textFont, text,
                        position, Color.White, 0f, Vector2.Zero, 0.9f, SpriteEffects.None, 0f);
                }
                else
                {
                    // Draw the current level text, with the text color representing the
                    // game's current state

                    Color levelColor = Color.White;

                    switch (currentLevel.CurrentState)
                    {
                        case LevelState.NotReady:
                        case LevelState.Ready:
                            break;
                        case LevelState.Flashing:
                            levelColor = Color.Yellow;
                            break;
                        case LevelState.Started:
                        case LevelState.Success:
                        case LevelState.InProcess:
                        case LevelState.FinishedOk:
                            levelColor = Color.LimeGreen;
                            break;
                        case LevelState.Fault:
                        case LevelState.FinishedFail:
                            levelColor = Color.Red;
                            break;
                        default:
                            break;
                    }

                    // Draw "Level" text
                    text = "Level";
                    size = textFont.MeasureString(text);
                    position = new Vector2(70, (
                        ScreenManager.GraphicsDevice.Viewport.Height - size.Y) / 2);

                    ScreenManager.SpriteBatch.DrawString(
                        textFont, text, position, levelColor);

                    // Draw level number
                    text = currentLevelNumber.ToString("D2");
                    size = levelNumberFont.MeasureString(text);
                    position = new Vector2(290, (
                        ScreenManager.GraphicsDevice.Viewport.Height - size.Y) / 2);

                    ScreenManager.SpriteBatch.DrawString(
                        levelNumberFont, text, position, levelColor);
                }
            }

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Finish the current game
        /// </summary>
        private void FinishCurrentGame()
        {
            isActive = false;

            if (HighScoreScreen.IsInHighscores(currentLevelNumber))
            {
                // Show the device's keyboard to enter a name for the highscore
                Guide.BeginShowKeyboardInput(PlayerIndex.One,
                    Constants.HighscorePopupTitle, Constants.HighscorePopupText,
                    Constants.HighscorePopupDefault, ShowHighscorePromptEnded, true);
            }
            else
            {
                moveToHighScore = true;
            }
        }

        /// <summary>
        /// Asynchronous handler for the highscore player name popup messagebox.
        /// </summary>
        /// <param name="result">The popup messagebox result. The result's
        /// AsyncState property should be true if the user successfully finished
        /// the game, or false otherwise.</param>
        private void ShowHighscorePromptEnded(IAsyncResult result)
        {
            string playerName = Guide.EndShowKeyboardInput(result);

            bool finishedGame = (bool)result.AsyncState;

            if (playerName != null)
            {
                if (playerName.Length > 15)
                    playerName = playerName.Substring(0, 15);

                HighScoreScreen.PutHighScore(playerName, currentLevelNumber);
            }

            if (finishedGame)
            {
                moveToHighScore = true;
            }
            else
            {
                AudioManager.PlaySound("fail");
                isActive = true;
                currentLevelNumber = 1;
                isLevelChange = true;
            }
        }

        /// <summary>
        /// Pause the game.
        /// </summary>
        private void PauseCurrentGame()
        {
            IsActive = false;

            // Pause sounds
            AudioManager.PauseResumeSounds(false);

            ScreenManager.AddScreen(new BackgroundScreen(false), null);
            ScreenManager.AddScreen(new PauseScreen(false), null);
        }

        #endregion
    }
}
