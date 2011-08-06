#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
#if !WINDOWS_PHONE
using Microsoft.Xna.Framework.Storage;
#endif


#endregion

namespace HoneycombRush
{
    class MainMenuScreen : MenuScreen
    {
        #region Fields


        bool isExiting = false;


        #endregion
        
        #region Initializations


        public MainMenuScreen()
            : base("")
        {
        }

        public override void LoadContent()
        {
            // Create our menu entries.
            MenuEntry startGameMenuEntry = new MenuEntry("Start");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Calculate menu positions - we do this here since we want the screen
            // manager to be available
            int quarterViewportWidth = ScreenManager.GraphicsDevice.Viewport.Width / 4;
            int menuEntryHeight = SafeArea.Bottom - ScreenManager.ButtonBackground.Height * 2;
            startGameMenuEntry.Position = new Vector2(quarterViewportWidth -
                ScreenManager.ButtonBackground.Width / 2, menuEntryHeight);
            exitMenuEntry.Position = new Vector2(3 * quarterViewportWidth -
                ScreenManager.ButtonBackground.Width / 2, menuEntryHeight);

            // Hook up menu event handlers.
            startGameMenuEntry.Selected += StartGameMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(startGameMenuEntry);
            MenuEntries.Add(exitMenuEntry);            

            AudioManager.LoadSounds();
            AudioManager.LoadMusic();

            AudioManager.PlayMusic("MenuMusic_Loop");

            base.LoadContent();
        }


        #endregion

        #region Update


        /// <summary>
        /// Performs necessary update logic.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="otherScreenHasFocus">Whether another screen has the focus.</param>
        /// <param name="coveredByOtherScreen">Whether this screen is covered by another.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (isExiting)
            {
                if (!HighScoreScreen.HighscoreSaved)
                {
                    HighScoreScreen.SaveHighscore();
                }
                else
                {
                    isExiting = false;
                    ScreenManager.Game.Exit();
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Handles user input.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="input">Input information.</param>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (isExiting)
            {
                return;
            }

            base.HandleInput(gameTime, input);
        }


        #endregion

        #region Menu handlers


        /// <summary>
        /// Respond to "Play" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StartGameMenuEntrySelected(object sender, EventArgs e)
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
            {
                screen.ExitScreen();
            }

#if WINDOWS_PHONE
            ScreenManager.AddScreen(new BackgroundScreen("Instructions"), null);
#elif XBOX
            ScreenManager.AddScreen(new BackgroundScreen("InstructionsXbox"), null);
#else
            ScreenManager.AddScreen(new BackgroundScreen("InstructionsPC"), null);
#endif

            ScreenManager.AddScreen(new LoadingAndInstructionScreen(), null);
            
            AudioManager.StopSound("MenuMusic_Loop");
        }

        /// <summary>
        /// Respond to "Exit" Item Selection
        /// </summary>
        /// <param name="playerIndex"></param>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            isExiting = true;            

            AudioManager.StopSound("MenuMusic_Loop");
        }


        #endregion
    }
}
