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
using System.Collections.Generic;
using System.Text;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Blackjack
{
    class MainMenuScreen : MenuScreen
    {
        public static string Theme = "Red";

        #region Initializations

        /// <summary>
        /// Initializes a new instance of the screen.
        /// </summary>
        public MainMenuScreen()
            : base("")
        {
        }
        #endregion

        public override void LoadContent()
        {
            // Create our menu entries.
            MenuEntry startGameMenuEntry = new MenuEntry("Play");
            MenuEntry themeGameMenuEntry = new MenuEntry("Theme");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            startGameMenuEntry.Selected += StartGameMenuEntrySelected;
            themeGameMenuEntry.Selected += ThemeGameMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(startGameMenuEntry);
            MenuEntries.Add(themeGameMenuEntry);
            MenuEntries.Add(exitMenuEntry);

            base.LoadContent();
        }

        #region Update
        /// <summary>
        /// Respond to "Play" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StartGameMenuEntrySelected(object sender, EventArgs e)
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
                screen.ExitScreen();

            ScreenManager.AddScreen(new GameplayScreen(Theme), null);
        }

        /// <summary>
        /// Respond to "Theme" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ThemeGameMenuEntrySelected(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenu(), null);
        }

        /// <summary>
        /// Respond to "Exit" Item Selection
        /// </summary>
        /// <param name="playerIndex"></param>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.Game.Exit();
        }
        #endregion
    }
}
