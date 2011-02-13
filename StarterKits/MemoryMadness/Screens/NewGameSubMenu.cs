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
using GameStateManagement;

#endregion

namespace MemoryMadness
{
    class NewGameSubMenuScreen : MenuScreen
    {
        #region Initializations

        public NewGameSubMenuScreen()
            : base("")
        {
            // Create our menu entries.
            MenuEntry newGameMenuEntry = new MenuEntry("New Game");
            MenuEntry loadGameMenuEntry = new MenuEntry("Load");

            // Hook up menu event handlers.
            newGameMenuEntry.Selected += NewGameMenuEntrySelected;
            loadGameMenuEntry.Selected += LoadGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(newGameMenuEntry);
            MenuEntries.Add(loadGameMenuEntry);
        }

        #endregion

        #region Update

        /// <summary>
        /// Respond to "Load Game" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LoadGameMenuEntrySelected(object sender, EventArgs e)
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
                screen.ExitScreen();

            ScreenManager.AddScreen(new LoadingAndInstructionsScreen(false), null);
        }

        /// <summary>
        /// Respond to "New Game" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NewGameMenuEntrySelected(object sender, EventArgs e)
        {
            if (PhoneApplicationService.Current.State.ContainsKey("CurrentLevel"))
            {                
                PhoneApplicationService.Current.State.Remove("CurrentLevel");
            }

            LoadGameMenuEntrySelected(sender, e);
        }

        /// <summary>
        /// Handle the back button and return to the main menu.
        /// </summary>
        /// <param name="playerIndex"></param>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
                screen.ExitScreen();

            ScreenManager.AddScreen(new BackgroundScreen(false), null);
            ScreenManager.AddScreen(new MainMenuScreen(), null);            
        }

        #endregion
    }
}
