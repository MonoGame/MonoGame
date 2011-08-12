#region File Description
//-----------------------------------------------------------------------------
// SaveLoadScreen.cs
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
using Microsoft.Xna.Framework.Content;
using RolePlayingGameData;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Displays a list of existing save games, 
    /// allowing the user to save, load, or delete.
    /// </summary>
    class SaveLoadScreen : GameScreen
    {
        public enum SaveLoadScreenMode
        {
            Save,
            Load,
        };

        /// <summary>
        /// The mode of this screen.
        /// </summary>
        private SaveLoadScreenMode mode;


        /// <summary>
        /// The current selected slot.
        /// </summary>
        private int currentSlot;


        #region Graphics Data


        private Texture2D backgroundTexture;
        private Vector2 backgroundPosition;

        private Texture2D plankTexture;
        private Vector2 plankPosition;

        private Texture2D backTexture;
        private Vector2 backPosition;

        private Texture2D deleteTexture;
        private Vector2 deletePosition = new Vector2(400f, 610f);
        private Vector2 deleteTextPosition = new Vector2(410f, 615f);

        private Texture2D selectTexture;
        private Vector2 selectPosition;

        private Texture2D lineBorderTexture;
        private Vector2 lineBorderPosition;

        private Texture2D highlightTexture;
        private Texture2D arrowTexture;

        private Vector2 titleTextPosition;
        private Vector2 backTextPosition;
        private Vector2 selectTextPosition;


        #endregion


        #region Initialization


        /// <summary>
        /// Create a new SaveLoadScreen object.
        /// </summary>
        public SaveLoadScreen(SaveLoadScreenMode mode) : base()
        {
            this.mode = mode;

            // refresh the save game descriptions
            Session.RefreshSaveGameDescriptions();
        }


        /// <summary>
        /// Loads the graphics content for this screen.
        /// </summary>
        public override void LoadContent()
        {
            // load the textures
            ContentManager content = ScreenManager.Game.Content;
            backgroundTexture = 
                content.Load<Texture2D>(@"Textures\MainMenu\MainMenu");
            plankTexture = 
                content.Load<Texture2D>(@"Textures\MainMenu\MainMenuPlank03");
            backTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\BButton");
            selectTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\AButton");
            deleteTexture =
                content.Load<Texture2D>(@"Textures\Buttons\XButton");
            lineBorderTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\LineBorder");
            highlightTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\HighlightLarge");
            arrowTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\SelectionArrow");

            // calculate the image positions
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            backgroundPosition = new Vector2(
                (viewport.Width - backgroundTexture.Width) / 2,
                (viewport.Height - backgroundTexture.Height) / 2); 
            plankPosition = backgroundPosition + new Vector2(
                backgroundTexture.Width / 2 - plankTexture.Width / 2, 
                60f);
            backPosition = backgroundPosition + new Vector2(225, 610);
            selectPosition = backgroundPosition + new Vector2(1120, 610);
            lineBorderPosition = backgroundPosition + new Vector2(200, 570);

            // calculate the text positions
            titleTextPosition = backgroundPosition + new Vector2(
                plankPosition.X + (plankTexture.Width - 
                    Fonts.HeaderFont.MeasureString("Load").X) / 2,
                plankPosition.Y + (plankTexture.Height - 
                    Fonts.HeaderFont.MeasureString("Load").Y) / 2);
            backTextPosition = new Vector2(backPosition.X + 55, backPosition.Y + 5);
            deleteTextPosition.X += deleteTexture.Width;
            selectTextPosition = new Vector2(
                selectPosition.X - Fonts.ButtonNamesFont.MeasureString("Select").X - 5,
                selectPosition.Y + 5);

            base.LoadContent();
        }


        #endregion


        #region Handle Input


        /// <summary>
        /// Respond to user input.
        /// </summary>
        public override void HandleInput()
        {
            // handle exiting the screen
            if (InputManager.IsActionTriggered(InputManager.Action.Back))
            {
                ExitScreen();
                return;
            }
            
            // handle selecting a save game
            if (InputManager.IsActionTriggered(InputManager.Action.Ok) &&
                (Session.SaveGameDescriptions != null))
            {
                switch (mode)
                {
                    case SaveLoadScreenMode.Load:
                        if ((currentSlot >= 0) &&
                            (currentSlot < Session.SaveGameDescriptions.Count) &&
                            (Session.SaveGameDescriptions[currentSlot] != null))
                        {
                            if (Session.IsActive)
                            {
                                MessageBoxScreen messageBoxScreen = new MessageBoxScreen(
                                    "Are you sure you want to load this game?");
                                messageBoxScreen.Accepted += 
                                    ConfirmLoadMessageBoxAccepted;
                                ScreenManager.AddScreen(messageBoxScreen);
                            }
                            else
                            {
                                ConfirmLoadMessageBoxAccepted(null, EventArgs.Empty);
                            }
                        }
                        break;

                    case SaveLoadScreenMode.Save:
                        if ((currentSlot >= 0) && 
                            (currentSlot <= Session.SaveGameDescriptions.Count))
                        {
                            if (currentSlot == Session.SaveGameDescriptions.Count)
                            {
                                ConfirmSaveMessageBoxAccepted(null, EventArgs.Empty);
                            }
                            else
                            {
                                MessageBoxScreen messageBoxScreen = new MessageBoxScreen(
                                   "Are you sure you want to overwrite this save game?");
                                messageBoxScreen.Accepted += 
                                    ConfirmSaveMessageBoxAccepted;
                                ScreenManager.AddScreen(messageBoxScreen);
                            }
                        }
                        break;
                }

            }
            // handle deletion
            else if (InputManager.IsActionTriggered(InputManager.Action.DropUnEquip) &&
                (Session.SaveGameDescriptions != null))
            {
                if ((currentSlot >= 0) &&
                    (currentSlot < Session.SaveGameDescriptions.Count) &&
                    (Session.SaveGameDescriptions[currentSlot] != null))
                {
                    MessageBoxScreen messageBoxScreen = new MessageBoxScreen(
                        "Are you sure you want to delete this save game?");
                    messageBoxScreen.Accepted += ConfirmDeleteMessageBoxAccepted;
                    ScreenManager.AddScreen(messageBoxScreen);
                }
            }
            // handle cursor-down
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorDown) &&
                (Session.SaveGameDescriptions != null))
            {
                int maximumSlot = Session.SaveGameDescriptions.Count;
                if (mode == SaveLoadScreenMode.Save)
                {
                    maximumSlot = Math.Min(maximumSlot + 1, 
                        Session.MaximumSaveGameDescriptions);
                }
                if (currentSlot < maximumSlot - 1)
                {
                    currentSlot++;
                }
            }
            // handle cursor-up
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorUp) &&
                (Session.SaveGameDescriptions != null))
            {
                if (currentSlot >= 1)
                {
                    currentSlot--;
                }
            }
        }


        /// <summary>
        /// Callback for the Save Game confirmation message box.
        /// </summary>
        void ConfirmSaveMessageBoxAccepted(object sender, EventArgs e)
        {
            if ((currentSlot >= 0) &&
                (currentSlot <= Session.SaveGameDescriptions.Count))
            {
                if (currentSlot == Session.SaveGameDescriptions.Count)
                {
                    Session.SaveSession(null);
                }
                else
                {
                    Session.SaveSession(Session.SaveGameDescriptions[currentSlot]);
                }
                ExitScreen();
            }
        }


        /// <summary>
        /// Delegate type for the save-game-selected-to-load event.
        /// </summary>
        /// <param name="saveGameDescription">
        /// The description of the file to load.
        /// </param>
        public delegate void LoadingSaveGameHandler(
            SaveGameDescription saveGameDescription);

        /// <summary>
        /// Fired when a save game is selected to load.
        /// </summary>
        /// <remarks>
        /// Loading save games exits multiple screens, 
        /// so we use events to move backwards.
        /// </remarks>
        public event LoadingSaveGameHandler LoadingSaveGame;


        /// <summary>
        /// Callback for the Load Game confirmation message box.
        /// </summary>
        void ConfirmLoadMessageBoxAccepted(object sender, EventArgs e)
        {
            if ((Session.SaveGameDescriptions != null) && (currentSlot >= 0) &&
                (currentSlot < Session.SaveGameDescriptions.Count) &&
                (Session.SaveGameDescriptions[currentSlot] != null))
            {
                ExitScreen();
                if (LoadingSaveGame != null)
                {
                    LoadingSaveGame(Session.SaveGameDescriptions[currentSlot]);
                }
            }
        }


        /// <summary>
        /// Callback for the Delete Game confirmation message box.
        /// </summary>
        void ConfirmDeleteMessageBoxAccepted(object sender, EventArgs e)
        {
            if ((Session.SaveGameDescriptions != null) && (currentSlot >= 0) &&
                (currentSlot < Session.SaveGameDescriptions.Count) &&
                (Session.SaveGameDescriptions[currentSlot] != null))
            {
                Session.DeleteSaveGame(Session.SaveGameDescriptions[currentSlot]);
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draws the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            spriteBatch.Draw(backgroundTexture, backgroundPosition, Color.White);
            spriteBatch.Draw(plankTexture, plankPosition, Color.White);
            spriteBatch.Draw(lineBorderTexture, lineBorderPosition, Color.White);

            spriteBatch.Draw(backTexture, backPosition, Color.White);
            spriteBatch.DrawString(Fonts.ButtonNamesFont, "Back", 
                backTextPosition, Color.White);

            spriteBatch.DrawString(Fonts.HeaderFont, 
                (mode == SaveLoadScreenMode.Load ? "Load" : "Save"), 
                titleTextPosition, Fonts.TitleColor);

            if ((Session.SaveGameDescriptions != null))
            {
                for (int i = 0; i < Session.SaveGameDescriptions.Count; i++)
                {
                    Vector2 descriptionTextPosition = new Vector2(295f,
                        200f + i * (Fonts.GearInfoFont.LineSpacing + 40f));
                    Color descriptionTextColor = Color.Black;

                    // if the save game is selected, draw the highlight color
                    if (i == currentSlot)
                    {
                        descriptionTextColor = Fonts.HighlightColor;
                        spriteBatch.Draw(highlightTexture,
                            descriptionTextPosition + new Vector2(-100, -23),
                            Color.White);
                        spriteBatch.Draw(arrowTexture,
                            descriptionTextPosition + new Vector2(-75, -15), 
                            Color.White);

                        spriteBatch.Draw(deleteTexture, deletePosition,
                            Color.White);
                        spriteBatch.DrawString(Fonts.ButtonNamesFont, "Delete",
                            deleteTextPosition, Color.White);

                        spriteBatch.Draw(selectTexture, selectPosition, Color.White);
                        spriteBatch.DrawString(Fonts.ButtonNamesFont, "Select",
                            selectTextPosition, Color.White);
                    }

                    spriteBatch.DrawString(Fonts.GearInfoFont,
                        Session.SaveGameDescriptions[i].ChapterName,
                        descriptionTextPosition, descriptionTextColor);
                    descriptionTextPosition.X = 650;
                    spriteBatch.DrawString(Fonts.GearInfoFont,
                        Session.SaveGameDescriptions[i].Description,
                        descriptionTextPosition, descriptionTextColor);
                }

                // if there is space for one, add an empty entry
                if ((mode == SaveLoadScreenMode.Save) &&
                    (Session.SaveGameDescriptions.Count <
                        Session.MaximumSaveGameDescriptions))
                {
                    int i = Session.SaveGameDescriptions.Count;
                    Vector2 descriptionTextPosition = new Vector2(
                        295f,
                        200f + i * (Fonts.GearInfoFont.LineSpacing + 40f));
                    Color descriptionTextColor = Color.Black;

                    // if the save game is selected, draw the highlight color
                    if (i == currentSlot)
                    {
                        descriptionTextColor = Fonts.HighlightColor;
                        spriteBatch.Draw(highlightTexture,
                            descriptionTextPosition + new Vector2(-100, -23),
                            Color.White);
                        spriteBatch.Draw(arrowTexture,
                            descriptionTextPosition + new Vector2(-75, -15),
                            Color.White);
                        spriteBatch.Draw(selectTexture, selectPosition, Color.White);
                        spriteBatch.DrawString(Fonts.ButtonNamesFont, "Select",
                            selectTextPosition, Color.White);
                    }

                    spriteBatch.DrawString(Fonts.GearInfoFont, "-------empty------",
                        descriptionTextPosition, descriptionTextColor);
                    descriptionTextPosition.X = 650;
                    spriteBatch.DrawString(Fonts.GearInfoFont, "-----",
                        descriptionTextPosition, descriptionTextColor);
                }
            }

            // if there are no slots to load, report that
            if (Session.SaveGameDescriptions == null)
            {
                spriteBatch.DrawString(Fonts.GearInfoFont, 
                    "No Storage Device Available",
                    new Vector2(295f, 200f), Color.Black);
            }
            else if ((mode == SaveLoadScreenMode.Load) &&
                (Session.SaveGameDescriptions.Count <= 0))
            {
                spriteBatch.DrawString(Fonts.GearInfoFont, 
                    "No Save Games Available",
                    new Vector2(295f, 200f), Color.Black);
            }

            spriteBatch.End();
        }

        #endregion
    }
}