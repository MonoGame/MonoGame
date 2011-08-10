#region File Description
//-----------------------------------------------------------------------------
// MenuEntry.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace GameStateManagement
{
    /// <summary>
    /// Helper class represents a single entry in a MenuScreen. By default this
    /// just draws the entry text string, but it can be customized to display menu
    /// entries in different ways. This also provides an event that will be raised
    /// when the menu entry is selected.
    /// </summary>
    class MenuEntry
    {
        #region Fields

        /// <summary>
        /// The text rendered for this entry.
        /// </summary>
        string text;

        /// <summary>
        /// Tracks a fading selection effect on the entry.
        /// </summary>
        /// <remarks>
        /// The entries transition out of the selection effect when they are deselected.
        /// </remarks>
        float selectionFade;

        Rectangle destination;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the text of this menu entry.
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        /// <summary>
        /// Gets or sets the position at which to draw this menu entry.
        /// </summary>
        public Rectangle Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        public float Scale { get; set; }

        public float Rotation { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Event raised when the menu entry is selected.
        /// </summary>
        public event EventHandler<PlayerIndexEventArgs> Selected;


        /// <summary>
        /// Method for raising the Selected event.
        /// </summary>
        protected internal virtual void OnSelectEntry(PlayerIndex playerIndex)
        {
            if (Selected != null)
                Selected(this, new PlayerIndexEventArgs(playerIndex));
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new menu entry with the specified text.
        /// </summary>
        public MenuEntry(string text)
        {
            this.text = text;

            Scale = 1f;
            Rotation = 0f;
        }
        #endregion

        #region Update and Draw
        /// <summary>
        /// Updates the menu entry.
        /// </summary>
        public virtual void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
        {
            // there is no such thing as a selected item on Windows Phone, so we always
            // force isSelected to be false
#if WINDOWS_PHONE || IOS || ANDROID
            isSelected = false;
#endif

            // When the menu selection changes, entries gradually fade between
            // their selected and deselected appearance, rather than instantly
            // popping to the new state.
            float fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4;

            if (isSelected)
                selectionFade = Math.Min(selectionFade + fadeSpeed, 1);
            else
                selectionFade = Math.Max(selectionFade - fadeSpeed, 0);
        }


        /// <summary>
        /// Draws the menu entry. This can be overridden to customize the appearance.
        /// </summary>
        public virtual void Draw(MenuScreen screen, bool isSelected, GameTime gameTime)
        {
            Color textColor = isSelected ? Color.White : Color.Black;
            Color tintColor = isSelected ? Color.White : Color.Gray;

#if WINDOWS_PHONE || IOS || ANDROID
            // there is no such thing as a selected item on Windows Phone, so we always
            // force isSelected to be false

            isSelected = false;
            tintColor = Color.White;
            textColor = Color.Black;
#endif

            // Draw text, centered on the middle of each line.
            ScreenManager screenManager = screen.ScreenManager;
            SpriteBatch spriteBatch = screenManager.SpriteBatch;
            SpriteFont font = screenManager.Font;

            spriteBatch.Draw(screenManager.ButtonBackground, destination, tintColor);

            spriteBatch.DrawString(screenManager.Font, text, getTextPosition(screen),
                textColor, Rotation, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }


        /// <summary>
        /// Queries how much space this menu entry requires.
        /// </summary>
        public virtual int GetHeight(MenuScreen screen)
        {
            return screen.ScreenManager.Font.LineSpacing;
        }

        /// <summary>
        /// Queries how wide the entry is, used for centering on the screen.
        /// </summary>
        public virtual int GetWidth(MenuScreen screen)
        {
            return (int)screen.ScreenManager.Font.MeasureString(Text).X;
        }

        private Vector2 getTextPosition(MenuScreen screen)
        {
            Vector2 textPosition = Vector2.Zero;
            if (Scale == 1f)
            {
                textPosition = new Vector2((int)destination.X + destination.Width / 2 - GetWidth(screen) / 2,
                                   (int)destination.Y);
            }
            else
            {
                textPosition = new Vector2(
                    (int)destination.X + (destination.Width / 2 - ((GetWidth(screen) / 2) * Scale)),
                                 (int)destination.Y + (GetHeight(screen) - GetHeight(screen) * Scale) / 2);
            }

            return textPosition;
        }
        #endregion
    }
}
