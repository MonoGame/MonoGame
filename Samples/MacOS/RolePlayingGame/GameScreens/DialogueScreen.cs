#region File Description
//-----------------------------------------------------------------------------
// DialogueScreen.cs
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
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Display of conversation dialog between the player and the npc
    /// </summary>
    class DialogueScreen : GameScreen
    {
        #region Graphics Data


        private Texture2D backgroundTexture;
        private Vector2 backgroundPosition;
        private Texture2D fadeTexture;

        private Texture2D selectButtonTexture;
        private Vector2 selectPosition;
        private Vector2 selectButtonPosition;

        private Vector2 backPosition;
        private Texture2D backButtonTexture;
        private Vector2 backButtonPosition;

        private Texture2D scrollTexture;
        private Vector2 scrollPosition;

        private Texture2D lineTexture;
        private Vector2 topLinePosition;
        private Vector2 bottomLinePosition;

        private Vector2 titlePosition;
        private Vector2 dialogueStartPosition;


        #endregion


        #region Text Data


        /// <summary>
        /// The title text shown at the top of the screen.
        /// </summary>
        private string titleText;

        /// <summary>
        /// The title text shown at the top of the screen.
        /// </summary>
        public string TitleText
        {
            get { return titleText; }
            set { titleText = value; }
        }


        /// <summary>
        /// The dialogue shown in the main portion of this dialog.
        /// </summary>
        private string dialogueText;

        /// <summary>
        /// The dialogue shown in the main portion of this dialog, broken into lines.
        /// </summary>
        private List<string> dialogueList = new List<string>();

        /// <summary>
        /// The dialogue shown in the main portion of this dialog.
        /// </summary>
        public string DialogueText
        {
            get { return dialogueText; }
            set
            {
                // trim the new value
                string trimmedValue = value.Trim();
                // if it's a match for what we already have, then this is trivial
                if (dialogueText == trimmedValue)
                {
                    return;
                }
                // assign the new value
                dialogueText = trimmedValue;
                // break the text into lines
                if (String.IsNullOrEmpty(dialogueText))
                {
                    dialogueList.Clear();
                }
                else
                {
                    dialogueList = Fonts.BreakTextIntoList(dialogueText, 
                        Fonts.DescriptionFont, maxWidth);
                }
                // set which lines ar edrawn
                startIndex = 0;
                endIndex = drawMaxLines;
                if (endIndex > dialogueList.Count)
                {
                    dialogueStartPosition = new Vector2(271f,
                        375f - ((dialogueList.Count - startIndex) *
                            (Fonts.DescriptionFont.LineSpacing) / 2));
                    endIndex = dialogueList.Count;
                }
                else
                {
                    dialogueStartPosition = new Vector2(271f, 225f);
                }
            }
        }


        /// <summary>
        /// The text shown next to the A button, if any.
        /// </summary>
        private string selectText = "Continue";

        /// <summary>
        /// The text shown next to the A button, if any.
        /// </summary>
        public string SelectText
        {
            get { return selectText; }
            set 
            {
                if (selectText != value)
                {
                    selectText = value;
                    if (selectButtonTexture != null)
                    {
                        selectPosition.X = selectButtonPosition.X -
                            Fonts.ButtonNamesFont.MeasureString(selectText).X - 10f;
                        selectPosition.Y = selectButtonPosition.Y;
                    }
                }
            }
        }


        /// <summary>
        /// The text shown next to the B button, if any.
        /// </summary>
        private string backText = "Back";

        /// <summary>
        /// The text shown next to the B button, if any.
        /// </summary>
        public string BackText
        {
            get { return backText; }
            set { backText = value; }
        }


        /// <summary>
        /// Maximum width of each line in pixels
        /// </summary>
        private const int maxWidth = 705;


        /// <summary>
        /// Starting index of the list to be displayed
        /// </summary>
        private int startIndex = 0;


        /// <summary>
        /// Ending index of the list to be displayed
        /// </summary>
        private int endIndex = drawMaxLines;


        /// <summary>
        /// Maximum number of lines to draw in the screen
        /// </summary>
        private const int drawMaxLines = 13;


        #endregion


        #region Initialization


        /// <summary>
        /// Construct a new DialogueScreen object.
        /// </summary>
        /// <param name="mapEntry"></param>
        public DialogueScreen()
        {
            this.IsPopup = true;
        }


        /// <summary>
        /// Load the graphics content
        /// </summary>
        /// <param name="batch">SpriteBatch object</param>
        /// <param name="screenWidth">Width of the screen</param>
        /// <param name="screenHeight">Height of the screen</param>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            fadeTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");
            backgroundTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\PopupScreen");
            scrollTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\ScrollButtons");
            selectButtonTexture = content.Load<Texture2D>(@"Textures\Buttons\AButton");
            backButtonTexture = content.Load<Texture2D>(@"Textures\Buttons\BButton");
            lineTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\SeparationLine");

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            backgroundPosition.X = (viewport.Width - backgroundTexture.Width) / 2;
            backgroundPosition.Y = (viewport.Height - backgroundTexture.Height) / 2;


            selectButtonPosition.X = viewport.Width / 2 + 260;
            selectButtonPosition.Y = backgroundPosition.Y + 530f;
            selectPosition.X = selectButtonPosition.X -
                Fonts.ButtonNamesFont.MeasureString(selectText).X - 10f;
            selectPosition.Y = selectButtonPosition.Y;

            backPosition.X = viewport.Width / 2 - 250f;
            backPosition.Y = backgroundPosition.Y + 530f;
            backButtonPosition.X = backPosition.X - backButtonTexture.Width - 10;
            backButtonPosition.Y = backPosition.Y;

            scrollPosition = backgroundPosition + new Vector2(820f, 200f);

            topLinePosition.X = (viewport.Width - lineTexture.Width) / 2 - 30f;
            topLinePosition.Y = 200f;

            bottomLinePosition.X = topLinePosition.X;
            bottomLinePosition.Y = 550f;

            titlePosition.X = (viewport.Width -
                Fonts.HeaderFont.MeasureString(titleText).X) / 2;
            titlePosition.Y = backgroundPosition.Y + 70f;
        }


        #endregion


        #region Updating


        /// <summary>
        /// Handles user input to the dialog.
        /// </summary>
        public override void HandleInput()
        {
            // Press Select or Bback
            if (InputManager.IsActionTriggered(InputManager.Action.Ok) ||
                InputManager.IsActionTriggered(InputManager.Action.Back))
            {
                ExitScreen();
                return;
            }

            // Scroll up
            if (InputManager.IsActionTriggered(InputManager.Action.CursorUp))
            {
                if (startIndex > 0)
                {
                    startIndex--;
                    endIndex--;
                }
            }
            // Scroll down
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorDown))
            {
                if (startIndex < dialogueList.Count - drawMaxLines)
                {
                    endIndex++;
                    startIndex++;
                }
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        /// draws the dialog.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Vector2 textPosition = dialogueStartPosition;

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            // draw the fading screen
            spriteBatch.Draw(fadeTexture, new Rectangle(0, 0, 1280, 720), Color.White);

            // draw popup background
            spriteBatch.Draw(backgroundTexture, backgroundPosition, Color.White);

            // draw the top line
            spriteBatch.Draw(lineTexture, topLinePosition, Color.White);

            // draw the bottom line
            spriteBatch.Draw(lineTexture, bottomLinePosition, Color.White);

            // draw scrollbar
            spriteBatch.Draw(scrollTexture, scrollPosition, Color.White);

            // draw title
            spriteBatch.DrawString(Fonts.HeaderFont, titleText, titlePosition,
                Fonts.CountColor);

            // draw the dialogue
            for (int i = startIndex; i < endIndex; i++)
            {
                spriteBatch.DrawString(Fonts.DescriptionFont, dialogueList[i],
                    textPosition, Fonts.CountColor);
                textPosition.Y += Fonts.DescriptionFont.LineSpacing;
            }

            // draw the Back button and adjoining text
            if (!String.IsNullOrEmpty(backText))
            {
                spriteBatch.DrawString(Fonts.ButtonNamesFont, backText, backPosition,
                    Color.White);
                spriteBatch.Draw(backButtonTexture, backButtonPosition, Color.White);
            }

            // draw the Select button and adjoining text
            if (!String.IsNullOrEmpty(selectText))
            {
                selectPosition.X = selectButtonPosition.X -
                    Fonts.ButtonNamesFont.MeasureString(selectText).X - 10f;
                selectPosition.Y = selectButtonPosition.Y;
                spriteBatch.DrawString(Fonts.ButtonNamesFont, selectText, selectPosition,
                    Color.White);
                spriteBatch.Draw(selectButtonTexture, selectButtonPosition, Color.White);
            }

            spriteBatch.End();
        }


        #endregion
    }
}