#region File Description

//-----------------------------------------------------------------------------
// PauseScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework;
using GameStateManagement;
using Microsoft.Xna.Framework.GamerServices;

#endregion

namespace MemoryMadness
{
    class PauseScreen : MenuScreen
    {
        #region Fields

        bool isResuming;
        bool checkHighscore = false;
        bool moveToHighScore = false;
        bool moveToMainMenu = false;

        #endregion

        #region Initializations

        /// <summary>
        /// Creates a new instance of the pause screen.
        /// </summary>
        /// <param name="isResuming">Whether or not the screen is displayed as a
        /// response to resuming the game (returning to it after the win key has
        /// been pressed, for example).</param>
        public PauseScreen(bool isResuming)
            : base("Pause")
        {
            // Create our menu entries
            MenuEntry returnGameMenuEntry = new MenuEntry("Return");
            MenuEntry exitMenuEntry = new MenuEntry("Quit");

            // Hook up menu event handlers
            returnGameMenuEntry.Selected += ReturnGameMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu
            MenuEntries.Add(returnGameMenuEntry);
            MenuEntries.Add(exitMenuEntry);

            this.isResuming = isResuming;

            if (!isResuming)
                AudioManager.PauseResumeSounds(false);
        }

        #endregion

        #region Loading

        /// <summary>
        /// Load screen resources
        /// </summary>
        public override void LoadContent()
        {
            if (isResuming && !AudioManager.IsInitialized)
                AudioManager.LoadSounds();

            AudioManager.PlaySound("menu");
            base.LoadContent();
        }

        #endregion

        #region Update

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (checkHighscore && (!Guide.IsVisible)) 
            {
                checkHighscore = false;

                var gameplayScreen = GetGameplayScreen();

                var levelNumber = gameplayScreen.currentLevel.levelNumber;

                if (HighScoreScreen.IsInHighscores(levelNumber))
                {
                    // Show the device's keyboard to record a high score
                    Guide.BeginShowKeyboardInput(PlayerIndex.One,
                        Constants.HighscorePopupTitle, Constants.HighscorePopupText,
                        Constants.HighscorePopupDefault, ShowHighscorePromptEnded,
                        levelNumber);                        
                }
                else
                {
                    moveToMainMenu = true;
                }
            }
            else if (moveToHighScore)
            {
                foreach (GameScreen screen in ScreenManager.GetScreens())
                    screen.ExitScreen();

                ScreenManager.AddScreen(new BackgroundScreen(true), null);
                ScreenManager.AddScreen(new HighScoreScreen(), null);
            }
            else if (moveToMainMenu)
            {
                foreach (GameScreen screen in ScreenManager.GetScreens())
                    screen.ExitScreen();

                ScreenManager.AddScreen(new BackgroundScreen(false), null);
                ScreenManager.AddScreen(new MainMenuScreen(), null);
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Asynchronous handler for the highscore player name popup messagebox.
        /// </summary>
        /// <param name="result">The popup messagebox result. The result's
        /// AsyncState should contain the level number which acts as the
        /// highscore.</param>
        private void ShowHighscorePromptEnded(IAsyncResult result)
        {
            string playerName = Guide.EndShowKeyboardInput(result);            

            int levelNumber = (int)result.AsyncState;

            if (playerName != null)
            {
                if (playerName.Length > 15)
                    playerName = playerName.Substring(0, 15);

                HighScoreScreen.PutHighScore(playerName, levelNumber);
            }

            moveToHighScore = true;            
        }

        /// <summary>
        /// Respond to "Return" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ReturnGameMenuEntrySelected(object sender, EventArgs e)
        {
            if (!isResuming)
            {
                // Resume sounds and activate the gameplay screen
                AudioManager.PauseResumeSounds(true);

                var screens = ScreenManager.GetScreens();

                foreach (GameScreen screen in screens)
                {
                    if (!(screen is GameplayScreen))
                    {
                        screen.ExitScreen();
                    }
                }

                (ScreenManager.GetScreens()[0] as GameplayScreen).IsActive = true;
            }
            else
            {
                // Since we are resuming the game, go to the loading screen which will
                // in turn initialize the gameplay screen
                foreach (GameScreen screen in ScreenManager.GetScreens())
                    screen.ExitScreen();

                ScreenManager.AddScreen(new LoadingAndInstructionsScreen(true), null);
            }
        }

        /// <summary>
        /// Respond to "Quit Game" Item Selection
        /// </summary>
        /// <param name="playerIndex"></param>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            AudioManager.StopSounds();

            // Give the user a chance to save his current progress
            Guide.BeginShowMessageBox("Save Game", "Do you want to save your progress?",
                new String[] { "Yes", "No" }, 0, MessageBoxIcon.Warning,
                ShowSaveDialogEnded, null);
        }

        /// <summary>
        /// Asynchronous handler for the game save popup messagebox.
        /// </summary>
        /// <param name="result">The popup messagebox result.</param>
        private void ShowSaveDialogEnded(IAsyncResult result)
        {
            int? res = Guide.EndShowMessageBox(result);

            if (res.HasValue)
            {
                // Store the user's progress
                if (res.Value == 0)
                {
                    if (!PhoneApplicationService.Current.State.ContainsKey(
                        "CurrentLevel"))
                    {
                        var gameplayScreen = GetGameplayScreen();

                        PhoneApplicationService.Current.State["CurrentLevel"]
                            = gameplayScreen.currentLevel.levelNumber;
                    }

                    foreach (GameScreen screen in ScreenManager.GetScreens())
                        screen.ExitScreen();

                    ScreenManager.AddScreen(new BackgroundScreen(false),
                        null);
                    ScreenManager.AddScreen(new MainMenuScreen(), null);
                }
                // The user really quit the game, see if he has a high score
                else
                {
                    checkHighscore = true;
                }
            }                        
        }

        #endregion

        /// <summary>
        /// Finds a gameplay screen objects among all screens and returns it.
        /// </summary>
        /// <returns>A gameplay screen instance, or null if none 
        /// are available.</returns>
        private GameplayScreen GetGameplayScreen()
        {
            var screens = ScreenManager.GetScreens();

            foreach (var screen in screens)
            {
                if (screen is GameplayScreen)
                {
                    return screen as GameplayScreen;
                }
            }

            return null;
        }
    }
}
