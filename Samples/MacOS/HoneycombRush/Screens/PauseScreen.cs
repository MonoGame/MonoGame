#region File Description
//-----------------------------------------------------------------------------
// pauseBackground.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using Microsoft.Xna.Framework;


#endregion

namespace HoneycombRush
{
    class PauseScreen : MenuScreen
    {
        #region Initializations


        public PauseScreen()
            : base(string.Empty)
        {
            IsPopup = true;
        }

        /// <summary>
        /// Load screen resources
        /// </summary>
        public override void LoadContent()
        {
            AudioManager.PlaySound("menu");

            MenuEntry returnGameMenuEntry = new MenuEntry("Resume");
            returnGameMenuEntry.Scale = 0.7f;
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Calculate menu positions - we do this here since we want the screen
            // manager to be available
            int quarterViewportWidth = ScreenManager.GraphicsDevice.Viewport.Width / 4;
            int menuEntryHeight = SafeArea.Bottom - ScreenManager.ButtonBackground.Height * 2;            
            returnGameMenuEntry.Position = new Vector2(quarterViewportWidth -
                ScreenManager.ButtonBackground.Width / 2, menuEntryHeight);
            exitMenuEntry.Position = new Vector2(3 * quarterViewportWidth -
                ScreenManager.ButtonBackground.Width / 2, menuEntryHeight);

            // Hook up menu event handlers.
            returnGameMenuEntry.Selected += ReturnGameMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            //// Add entries to the menu.
            MenuEntries.Add(returnGameMenuEntry);
            MenuEntries.Add(exitMenuEntry);

            base.LoadContent();
        }
        

        #endregion

        #region Update


        /// <summary>
        /// Respond to "Return" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ReturnGameMenuEntrySelected(object sender, EventArgs e)
        {
            AudioManager.PauseResumeSounds(true);

            foreach (GameScreen screen in ScreenManager.GetScreens())
            {
                if (!(screen is GameplayScreen))
                {
                    screen.ExitScreen();
                }
            }
        }

        /// <summary>
        /// Respond to "Quit Game" Item Selection
        /// </summary>
        /// <param name="playerIndex"></param>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
            {
                screen.ExitScreen();
            }

            ScreenManager.AddScreen(new BackgroundScreen("titleScreen"), null);
            ScreenManager.AddScreen(new MainMenuScreen(), null);
        }


        #endregion
    }
}



