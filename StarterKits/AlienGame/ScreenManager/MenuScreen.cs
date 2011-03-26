//-----------------------------------------------------------------------------
// MenuScreen.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AlienGameSample
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    abstract class MenuScreen : GameScreen
    {
        List<MenuEntry> menuEntries = new List<MenuEntry>();
        int selectedEntry;
        string menuTitle;

        /// <summary>
        /// Gets the list of menu entries, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected IList<MenuEntry> MenuEntries
        {
            get { return menuEntries; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuScreen(string menuTitle)
        {
            this.menuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(1.0);
            TransitionOffTime = TimeSpan.FromSeconds(1.0);
        }

        public override void LoadContent()
        {
            base.LoadContent();
        }

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            // Accept or cancel the menu?
            if (input.MenuSelect)
            {
                OnSelectEntry(selectedEntry);
            }
            else if (input.MenuCancel)
            {
                OnCancel();
            }
        }

        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryIndex)
        {
            menuEntries[selectedEntry].OnSelectEntry();
        }

        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel()
        {
            ExitScreen();
        }

        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, EventArgs e)
        {
            OnCancel();
        }

        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                bool isSelected = IsActive && (i == selectedEntry);

                menuEntries[i].Update(this, isSelected, gameTime);
            }
        }

        /// <summary>
        /// Draws the menu.  Tweaked a bit from the sample so that it draws menus on the bottom left corner and transitions
        /// on and off from the bottom.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            if (menuEntries.Count > 0)
            {
                SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
                SpriteFont font = ScreenManager.Font;

                Vector2 position = new Vector2(40, 420 - menuEntries[0].GetHeight(this));

                // Make the menu slide into place during transitions, using a
                // power curve to make things look more interesting (this makes
                // the movement slow down as it nears the end).
                float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

                position.Y += transitionOffset * 512;

                spriteBatch.Begin();

                // Draw each menu entry in turn.
                for (int i = menuEntries.Count - 1; i >= 0; --i)
                {
                    MenuEntry menuEntry = menuEntries[i];

                    position.X = 160 - font.MeasureString(menuEntry.Text).X / 2;
					
					bool isSelected = IsActive && (i == selectedEntry);
					
                    menuEntry.Draw(this, position, isSelected, gameTime);

                    position.Y -= menuEntry.GetHeight(this);
                }

                spriteBatch.End();

            }
        }
    }
}
