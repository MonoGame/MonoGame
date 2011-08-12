#region File Description
//-----------------------------------------------------------------------------
// OptionsMenu.cs
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
using CardsFramework;
#endregion

namespace Blackjack
{
    class OptionsMenu : MenuScreen
    {
        Dictionary<string, Texture2D> themes = new Dictionary<string, Texture2D>();
        AnimatedGameComponent card;
        Texture2D background;
        Rectangle safeArea;


        #region Initializations

        /// <summary>
        /// Initializes a new instance of the screen.
        /// </summary>
        public OptionsMenu()
            : base("")
        {

        }
        #endregion

        /// <summary>
        /// Loads content required by the screen, and initializes the displayed menu.
        /// </summary>
        public override void LoadContent()
        {
            safeArea = ScreenManager.SafeArea;
            // Create our menu entries.
            MenuEntry themeGameMenuEntry = new MenuEntry("Deck");
            MenuEntry returnMenuEntry = new MenuEntry("Return");

            // Hook up menu event handlers.
            themeGameMenuEntry.Selected += ThemeGameMenuEntrySelected;
            returnMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(themeGameMenuEntry);
            MenuEntries.Add(returnMenuEntry);

            themes.Add("Red", ScreenManager.Game.Content.Load<Texture2D>(
                @"Images\Cards\CardBack_Red"));
            themes.Add("Blue", ScreenManager.Game.Content.Load<Texture2D>(
                @"Images\Cards\CardBack_Blue"));
            background = ScreenManager.Game.Content.Load<Texture2D>(
                @"Images\UI\table");

            card = new AnimatedGameComponent(ScreenManager.Game,
                themes[MainMenuScreen.Theme])
            {
                CurrentPosition = new Vector2(safeArea.Center.X, safeArea.Center.Y - 50)
            };

            ScreenManager.Game.Components.Add(card);

            base.LoadContent();
        }

        #region Update and Render
        /// <summary>
        /// Respond to "Theme" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ThemeGameMenuEntrySelected(object sender, EventArgs e)
        {
            if (MainMenuScreen.Theme == "Red")
            {
                MainMenuScreen.Theme = "Blue";
            }
            else
            {
                MainMenuScreen.Theme = "Red";
            }
            card.CurrentFrame = themes[MainMenuScreen.Theme];
        }

        /// <summary>
        /// Respond to "Return" Item Selection
        /// </summary>
        /// <param name="playerIndex"></param>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.Game.Components.Remove(card);
            ExitScreen();
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin();

            // Draw the card back
            ScreenManager.SpriteBatch.Draw(background, ScreenManager.GraphicsDevice.Viewport.Bounds,
                Color.White * TransitionAlpha);

            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }
        #endregion
    }
}
