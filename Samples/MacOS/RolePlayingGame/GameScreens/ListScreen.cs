#region File Description
//-----------------------------------------------------------------------------
// ListScreen.cs
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
using System.Collections.ObjectModel;
#endregion

namespace RolePlaying
{
    abstract class ListScreen<T> : GameScreen
    {
        #region Graphics Data

        protected readonly Vector2 iconOffset = new Vector2(0f, 0f);
        protected readonly Vector2 descriptionTextPosition = new Vector2(200, 550);
        private readonly Vector2 listPositionTopPosition = new Vector2(1160, 354);
        private readonly Vector2 listPositionBottomPosition = new Vector2(1160, 384);

        private Texture2D backgroundTexture;
        private readonly Rectangle backgroundDestination =
            new Rectangle(0, 0, 1280, 720);
        private Texture2D fadeTexture;

        private Texture2D listTexture;
        private readonly Vector2 listTexturePosition = new Vector2(187f, 180f);
        protected readonly Vector2 listEntryStartPosition = new Vector2(200f, 202f);
        protected const int listLineSpacing = 76;

        private Texture2D plankTexture;
        private Vector2 plankTexturePosition;
        protected string titleText = String.Empty;

        protected Texture2D goldTexture;
        private readonly Vector2 goldTexturePosition = new Vector2(490f, 640f);
        protected string goldText = String.Empty;
        private readonly Vector2 goldTextPosition = new Vector2(565f, 648f);

        private Texture2D highlightTexture;
        private readonly Vector2 highlightStartPosition = new Vector2(170f, 237f);
        private Texture2D selectionArrowTexture;
        private readonly Vector2 selectionArrowPosition = new Vector2(135f, 245f);

        private Texture2D leftTriggerTexture;
        private readonly Vector2 leftTriggerTexturePosition = new Vector2(340f, 50f);
        protected string leftTriggerText = String.Empty;

        private Texture2D rightTriggerTexture;
        private readonly Vector2 rightTriggerTexturePosition = new Vector2(900f, 50f);
        protected string rightTriggerText = String.Empty;

        private Texture2D leftQuantityArrowTexture;
        private Texture2D rightQuantityArrowTexture;

        private Texture2D backButtonTexture;
        private readonly Vector2 backButtonTexturePosition = new Vector2(80f, 640f);
        protected string backButtonText = String.Empty;
        private Vector2 backButtonTextPosition = new Vector2(90f, 645f); // + tex width

        private Texture2D selectButtonTexture;
        private readonly Vector2 selectButtonTexturePosition = new Vector2(1150f, 640f);
        protected string selectButtonText = String.Empty;

        private Texture2D xButtonTexture;
        private readonly Vector2 xButtonTexturePosition = new Vector2(240f, 640f);
        protected string xButtonText = String.Empty;
        private Vector2 xButtonTextPosition = new Vector2(250f, 645f); // + tex width

        private Texture2D yButtonTexture;
        private readonly Vector2 yButtonTexturePosition = new Vector2(890f, 640f);
        protected string yButtonText = String.Empty;


        #endregion


        #region Data Access


        /// <summary>
        /// Get the list that this screen displays.
        /// </summary>
        /// <returns></returns>
        public abstract ReadOnlyCollection<T> GetDataList();


        #endregion


        #region List Navigation


        /// <summary>
        /// The index of the selected entry.
        /// </summary>
        private int selectedIndex = 0;

        /// <summary>
        /// The index of the selected entry.
        /// </summary>
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    EnsureVisible(selectedIndex);
                }
            }
        }


        /// <summary>
        /// Ensure that the given index is visible on the screen.
        /// </summary>
        public void EnsureVisible(int index)
        {
            if (index < startIndex)
            {
                // if it's above the current selection, set the first entry
                startIndex = index;
            }
            if (selectedIndex > (endIndex - 1))
            {
                startIndex += selectedIndex - (endIndex - 1);
            }
            // otherwise, it should be in the current selection already
            // -- note that the start and end indices are checked in Draw.
        }




        /// <summary>
        /// Move the current selection up one entry.
        /// </summary>
        protected virtual void MoveCursorUp()
        {
            if (SelectedIndex > 0)
            {
                SelectedIndex--;
            }
        }


        /// <summary>
        /// Move the current selection down one entry.
        /// </summary>
        protected virtual void MoveCursorDown()
        {
            SelectedIndex++;   // safety-checked in Draw()
        }


        /// <summary>
        /// Decrease the selected quantity by one.
        /// </summary>
        protected virtual void MoveCursorLeft() { }


        /// <summary>
        /// Increase the selected quantity by one.
        /// </summary>
        protected virtual void MoveCursorRight() { }
        
        
        /// <summary>
        /// The first index displayed on the screen from the list.
        /// </summary>
        private int startIndex = 0;

        /// <summary>
        /// The first index displayed on the screen from the list.
        /// </summary>
        public int StartIndex
        {
            get { return startIndex; }
            set { startIndex = value; } // safety-checked in Draw
        }


        /// <summary>
        /// The last index displayed on the screen from the list.
        /// </summary>
        private int endIndex = 0;

        /// <summary>
        /// The last index displayed on the screen from the list.
        /// </summary>
        public int EndIndex
        {
            get { return endIndex; }
            set { endIndex = value; }   // safety-checked in Draw
        }


        /// <summary>
        /// The maximum number of list entries that the screen can show at once.
        /// </summary>
        public const int MaximumListEntries = 4;


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new ListScreen object.
        /// </summary>
        public ListScreen()
            : base() 
        {
            this.IsPopup = true;
        }
 

        /// <summary>
        /// Load the graphics content from the content manager.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            // load the background textures
            fadeTexture = content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");
            backgroundTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\GameScreenBkgd");
            listTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\InfoDisplay");
            plankTexture =
                content.Load<Texture2D>(@"Textures\MainMenu\MainMenuPlank03");
            goldTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\GoldIcon");

            // load the foreground textures
            highlightTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\HighlightLarge");
            selectionArrowTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\SelectionArrow");

            // load the trigger images
            leftTriggerTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\LeftTriggerButton");
            rightTriggerTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\RightTriggerButton");
            leftQuantityArrowTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\QuantityArrowLeft");
            rightQuantityArrowTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\QuantityArrowRight");
            backButtonTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\BButton");
            selectButtonTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\AButton");
            xButtonTexture =
                content.Load<Texture2D>(@"Textures\Buttons\XButton");
            yButtonTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\YButton");

            // calculate the centered positions
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            plankTexturePosition = new Vector2(
                viewport.X + (viewport.Width - plankTexture.Width) / 2f, 67f);

            // adjust positions for texture sizes
            if (backButtonTexture != null)
            {
                backButtonTextPosition.X += backButtonTexture.Width;
            }
            if (xButtonTexture != null)
            {
                xButtonTextPosition.X += xButtonTexture.Width;
            }
            
            base.LoadContent();
        }


        #endregion


        #region Input Handling


        /// <summary>
        /// Handle user input.
        /// </summary>
        public override void HandleInput()
        {
            if (InputManager.IsActionTriggered(InputManager.Action.PageLeft))
            {
                PageScreenLeft();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.PageRight))
            {
                PageScreenRight();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorUp))
            {
                MoveCursorUp();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorDown))
            {
                MoveCursorDown();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.IncreaseAmount))
            {
                MoveCursorRight();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.DecreaseAmount))
            {
                MoveCursorLeft();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.Back))
            {
                BackTriggered();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.Ok))
            {
                ReadOnlyCollection<T> dataList = GetDataList();
                if ((selectedIndex >= 0) && (selectedIndex < dataList.Count))
                {
                    SelectTriggered(dataList[selectedIndex]);
                }
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.DropUnEquip))
            {
                ReadOnlyCollection<T> dataList = GetDataList();
                if ((selectedIndex >= 0) && (selectedIndex < dataList.Count))
                {
                    ButtonXPressed(dataList[selectedIndex]);
                }
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.TakeView))
            {
                ReadOnlyCollection<T> dataList = GetDataList();
                if ((selectedIndex >= 0) && (selectedIndex < dataList.Count))
                {
                    ButtonYPressed(dataList[selectedIndex]);
                }
            }
            base.HandleInput();
        }


        /// <summary>
        /// Switch to the screen to the "left" of this one in the UI, if any.
        /// </summary>
        protected virtual void PageScreenLeft() { }


        /// <summary>
        /// Switch to the screen to the "right" of this one in the UI, if any.
        /// </summary>
        protected virtual void PageScreenRight() { }


        /// <summary>
        /// Respond to the triggering of the Back action.
        /// </summary>
        protected virtual void BackTriggered()
        {
            ExitScreen();
        }


        /// <summary>
        /// Respond to the triggering of the Select action.
        /// </summary>
        protected virtual void SelectTriggered(T entry) { }


        /// <summary>
        /// Respond to the triggering of the X button (and related key).
        /// </summary>
        protected virtual void ButtonXPressed(T entry) { }


        /// <summary>
        /// Respond to the triggering of the Y button (and related key).
        /// </summary>
        protected virtual void ButtonYPressed(T entry) { }


        #endregion


        #region Drawing


        /// <summary>
        /// Draws the screen.
        /// </summary>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // get the content list
            ReadOnlyCollection<T> dataList = GetDataList();

            // turn off the buttons if the list is empty
            if (dataList.Count <= 0)
            {
                selectButtonText = String.Empty;
                xButtonText = String.Empty;
                yButtonText = String.Empty;
            }

            // fix the indices for the current list size
            SelectedIndex = (int)MathHelper.Clamp(SelectedIndex, 0, dataList.Count - 1);
            startIndex = (int)MathHelper.Clamp(startIndex, 0, 
                dataList.Count - MaximumListEntries);
            endIndex = Math.Min(startIndex + MaximumListEntries, dataList.Count);

            spriteBatch.Begin();

            DrawBackground();
            if (dataList.Count > 0)
            {
                DrawListPosition(SelectedIndex + 1, dataList.Count);
            }
            DrawButtons();
            DrawPartyGold();
            DrawColumnHeaders();
            DrawTitle();

            // draw each item currently shown
            Vector2 position = listEntryStartPosition + 
                new Vector2(0f, listLineSpacing / 2);
            if (startIndex >= 0)
            {
                for (int index = startIndex; index < endIndex; index++)
                {
                    T entry = dataList[index];
                    if (index == selectedIndex)
                    {
                        DrawSelection(position);
                        DrawEntry(entry, position, true);
                        DrawSelectedDescription(entry);
                    }
                    else
                    {
                        DrawEntry(entry, position, false);
                    }
                    position.Y += listLineSpacing;
                }
            }

            spriteBatch.End();
        }


        /// <summary>
        /// Draw the entry at the given position in the list.
        /// </summary>
        /// <param name="entry">The entry to draw.</param>
        /// <param name="position">The position to draw the entry at.</param>
        /// <param name="isSelected">If true, this entry is selected.</param>
        protected abstract void DrawEntry(T entry, Vector2 position, bool isSelected);


        /// <summary>
        /// Draw the selection graphics over the selected item.
        /// </summary>
        /// <param name="position"></param>
        protected virtual void DrawSelection(Vector2 position)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Draw(highlightTexture, 
                new Vector2(highlightStartPosition.X, position.Y), Color.White);
            spriteBatch.Draw(selectionArrowTexture,
                new Vector2(selectionArrowPosition.X, position.Y + 10f), Color.White);
        }


        /// <summary>
        /// Draw the background of the screen.
        /// </summary>
        protected virtual void DrawBackground()
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Draw(fadeTexture, backgroundDestination, Color.White);
            spriteBatch.Draw(backgroundTexture, backgroundDestination, Color.White);
            spriteBatch.Draw(listTexture, listTexturePosition, Color.White);
        }


        /// <summary>
        /// Draw the current list position in the appropriate location on the screen.
        /// </summary>
        /// <param name="position">The current position in the list.</param>
        /// <param name="total">The total elements in the list.</param>
        protected virtual void DrawListPosition(int position, int total)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // draw the top number - the current position in the list
            string listPositionTopText = position.ToString();
            Vector2 drawPosition = listPositionTopPosition;
            drawPosition.X -= (float)Math.Ceiling(
                Fonts.GearInfoFont.MeasureString(listPositionTopText).X / 2);
            spriteBatch.DrawString(Fonts.GearInfoFont, listPositionTopText,
                drawPosition, Fonts.CountColor);

            // draw the bottom number - the current position in the list
            string listPositionBottomText = total.ToString();
            drawPosition = listPositionBottomPosition;
            drawPosition.X -= (float)Math.Ceiling(
                Fonts.GearInfoFont.MeasureString(listPositionBottomText).X / 2);
            spriteBatch.DrawString(Fonts.GearInfoFont, listPositionBottomText,
                drawPosition, Fonts.CountColor);
        }

        
        /// <summary>
        /// Draw the party gold text.
        /// </summary>
        protected virtual void DrawPartyGold()
        {
            if (!IsActive)
            {
                return;
            }

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Draw(goldTexture, goldTexturePosition, Color.White);
            spriteBatch.DrawString(Fonts.ButtonNamesFont,
                Fonts.GetGoldString(Session.Party.PartyGold), goldTextPosition,
                Color.White);
        }


        /// <summary>
        /// Draw the description of the selected item.
        /// </summary>
        protected abstract void DrawSelectedDescription(T entry);


        /// <summary>
        /// Draw the column headers above the list.
        /// </summary>
        protected abstract void DrawColumnHeaders();


        /// <summary>
        /// Draw all of the buttons used by the screen.
        /// </summary>
        protected virtual void DrawButtons()
        {
            if (!IsActive)
            {
                return;
            }

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // draw the left trigger texture and text
            if ((leftTriggerTexture != null) && !String.IsNullOrEmpty(leftTriggerText))
            {
                Vector2 position = leftTriggerTexturePosition + new Vector2(
                    leftTriggerTexture.Width / 2f - (float)Math.Ceiling(
                    Fonts.PlayerStatisticsFont.MeasureString(leftTriggerText).X / 2f),
                    90f);
                spriteBatch.Draw(leftTriggerTexture, leftTriggerTexturePosition, 
                    Color.White);
                spriteBatch.DrawString(Fonts.PlayerStatisticsFont, leftTriggerText,
                    position, Color.Black);
            }

            // draw the right trigger texture and text
            if ((rightTriggerTexture != null) && !String.IsNullOrEmpty(rightTriggerText))
            {
                Vector2 position = rightTriggerTexturePosition + new Vector2(
                    rightTriggerTexture.Width / 2f - (float)Math.Ceiling(
                    Fonts.PlayerStatisticsFont.MeasureString(rightTriggerText).X / 2f),
                    90f);
                spriteBatch.Draw(rightTriggerTexture, rightTriggerTexturePosition,
                    Color.White);
                spriteBatch.DrawString(Fonts.PlayerStatisticsFont, rightTriggerText,
                    position, Color.Black);
            }

            // draw the left trigger texture and text
            if ((backButtonTexture != null) && !String.IsNullOrEmpty(backButtonText))
            {
                spriteBatch.Draw(backButtonTexture, backButtonTexturePosition,
                    Color.White);
                spriteBatch.DrawString(Fonts.ButtonNamesFont, backButtonText,
                    backButtonTextPosition, Color.White);
            }

            // draw the left trigger texture and text
            if ((selectButtonTexture != null) && !String.IsNullOrEmpty(selectButtonText))
            {
                spriteBatch.Draw(selectButtonTexture, selectButtonTexturePosition,
                    Color.White);
                Vector2 position = selectButtonTexturePosition - new Vector2(
                    Fonts.ButtonNamesFont.MeasureString(selectButtonText).X, 0f) +
                    new Vector2(0f, 5f);
                spriteBatch.DrawString(Fonts.ButtonNamesFont, selectButtonText,
                    position, Color.White);
            }

            // draw the left trigger texture and text
            if ((xButtonTexture != null) && !String.IsNullOrEmpty(xButtonText))
            {
                spriteBatch.Draw(xButtonTexture, xButtonTexturePosition,
                    Color.White);
                spriteBatch.DrawString(Fonts.ButtonNamesFont, xButtonText,
                    xButtonTextPosition, Color.White);
            }

            // draw the left trigger texture and text
            if ((yButtonTexture != null) && !String.IsNullOrEmpty(yButtonText))
            {
                spriteBatch.Draw(yButtonTexture, yButtonTexturePosition,
                    Color.White);
                Vector2 position = yButtonTexturePosition - new Vector2(
                    Fonts.ButtonNamesFont.MeasureString(yButtonText).X, 0f) +
                    new Vector2(0f, 5f);
                spriteBatch.DrawString(Fonts.ButtonNamesFont, yButtonText,
                    position, Color.White);
            }
        }


        /// <summary>
        /// Draw the title of the screen, if any.
        /// </summary>
        protected virtual void DrawTitle()
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // draw the left trigger texture and text
            if ((plankTexture != null) && !String.IsNullOrEmpty(titleText))
            {
                Vector2 titleTextSize = Fonts.HeaderFont.MeasureString(titleText);
                Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
                Vector2 position = new Vector2(
                    (float)Math.Floor(viewport.X + viewport.Width / 2 - 
                    titleTextSize.X / 2f), 90f);
                spriteBatch.Draw(plankTexture, plankTexturePosition, Color.White);
                spriteBatch.DrawString(Fonts.HeaderFont, titleText, position,
                    Fonts.TitleColor);
            }
        }


        #endregion
    }
}
