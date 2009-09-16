//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;

namespace AlienGameSample
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
            : base("Main")
        {
            // Create our menu entries.
            MenuEntry startGameMenuEntry = new MenuEntry("START GAME");
            MenuEntry exitMenuEntry = new MenuEntry("QUIT");

            // Hook up menu event handlers.
            startGameMenuEntry.Selected += StartGameMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(startGameMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void StartGameMenuEntrySelected(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new GameplayScreen());
        }

        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel()
        {
            ScreenManager.Game.Exit();
        }
    }
}
