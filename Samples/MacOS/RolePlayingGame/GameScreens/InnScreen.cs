#region File Description
//-----------------------------------------------------------------------------
// InnScreen.cs
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
    /// Displays the options for an inn that the party can stay at.
    /// </summary>
    class InnScreen : GameScreen
    {
        private Inn inn;


        #region Graphics Data


        private Texture2D backgroundTexture;
        private Texture2D plankTexture;
        private Texture2D selectIconTexture;
        private Texture2D backIconTexture;
        private Texture2D highlightTexture;
        private Texture2D arrowTexture;
        private Texture2D conversationTexture;
        private Texture2D fadeTexture;
        private Texture2D goldIcon;


        #endregion


        #region Position Data


        private readonly Vector2 stayPosition = new Vector2(620f, 250f);
        private readonly Vector2 leavePosition = new Vector2(620f, 300f);
        private readonly Vector2 costPosition = new Vector2(470, 450);
        private readonly Vector2 informationPosition = new Vector2(470, 490);
        private readonly Vector2 selectIconPosition = new Vector2(1150, 640);
        private readonly Vector2 backIconPosition = new Vector2(80, 640);
        private readonly Vector2 goldStringPosition = new Vector2(565, 648);
        private readonly Vector2 stayArrowPosition = new Vector2(520f, 234f);
        private readonly Vector2 leaveArrowPosition = new Vector2(520f, 284f);
        private readonly Vector2 stayHighlightPosition = new Vector2(180f, 230f);
        private readonly Vector2 leaveHighlightPosition = new Vector2(180f, 280f);
        private readonly Vector2 innKeeperPosition = new Vector2(290, 370);
        private readonly Vector2 conversationStripPosition = new Vector2(210f, 405f);
        private readonly Vector2 goldIconPosition = new Vector2(490, 640);
        private Vector2 plankPosition;
        private Vector2 backgroundPosition;
        private Vector2 namePosition;
        private Vector2 selectTextPosition;
        private Vector2 backTextPosition;
        private Rectangle screenRectangle;


        #endregion


        #region Dialog Text


        private List<string> welcomeMessage;
        private List<string> serviceRenderedMessage;
        private List<string> noGoldMessage;
        private List<string> currentDialogue;
        private const int maxWidth = 570;
        private const int maxLines = 3;

        private string costString;
        private readonly string stayString = "Stay";
        private readonly string leaveString = "Leave";
        private readonly string selectString = "Select";
        private readonly string backString = "Leave";


        #endregion


        #region Selection Data


        private int selectionMark;
        private int endIndex;


        #endregion


        #region Initialization


        /// <summary>
        /// Creates a new InnScreen object.
        /// </summary>
        public InnScreen(Inn inn)
            : base()
        {
            // check the parameter
            if (inn == null)
            {
                throw new ArgumentNullException("inn");
            }

            this.IsPopup = true;
            this.inn = inn;

            welcomeMessage = Fonts.BreakTextIntoList(inn.WelcomeMessage, 
                Fonts.DescriptionFont, maxWidth);
            serviceRenderedMessage = Fonts.BreakTextIntoList(inn.PaidMessage, 
                Fonts.DescriptionFont, maxWidth);
            noGoldMessage = Fonts.BreakTextIntoList(inn.NotEnoughGoldMessage, 
                Fonts.DescriptionFont, maxWidth);

            selectionMark = 1;
            ChangeDialogue(welcomeMessage);
        }


        /// <summary>
        /// Load the graphics content
        /// </summary>
        /// <param name="batch">SpriteBatch object</param>
        /// <param name="screenWidth">Width of screen</param>
        /// <param name="screenHeight">Height of the screen</param>
        public override void LoadContent()
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            ContentManager content = ScreenManager.Game.Content;

            backgroundTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\GameScreenBkgd");
            plankTexture = 
                content.Load<Texture2D>(@"Textures\MainMenu\MainMenuPlank03");
            selectIconTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\AButton");
            backIconTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\BButton");
            highlightTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\HighlightLarge");
            arrowTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\SelectionArrow");
            conversationTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\ConversationStrip");
            goldIcon = content.Load<Texture2D>(@"Textures\GameScreens\GoldIcon");
            fadeTexture = content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");

            screenRectangle = new Rectangle(viewport.X, viewport.Y,
                viewport.Width, viewport.Height);

            plankPosition = new Vector2((viewport.Width - plankTexture.Width) / 2, 67f);

            backgroundPosition = new Vector2(
                (viewport.Width - backgroundTexture.Width) / 2,
                (viewport.Height - backgroundTexture.Height) / 2);

            namePosition = new Vector2(
                (viewport.Width - Fonts.HeaderFont.MeasureString(inn.Name).X) / 2, 90f);

            selectTextPosition = selectIconPosition;
            selectTextPosition.X -= 
                Fonts.ButtonNamesFont.MeasureString(selectString).X + 10;
            selectTextPosition.Y += 5;

            backTextPosition = backIconPosition;
            backTextPosition.X += backIconTexture.Width + 10;
            backTextPosition.Y += 5;
        }


        #endregion


        #region Updating


        /// <summary>
        /// Handle user input.
        /// </summary>
        public override void HandleInput()
        {
            // exit the screen
            if (InputManager.IsActionTriggered(InputManager.Action.Back))
            {
                ExitScreen();
                return;
            }
            // move the cursor up
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorUp))
            {
                if (selectionMark == 2)
                {
                    selectionMark = 1;
                }
            }
            // move the cursor down
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorDown))
            {
                if (selectionMark == 1)
                {
                    selectionMark = 2;
                }
            }
            // select an option
            else if (InputManager.IsActionTriggered(InputManager.Action.Ok))
            {
                if (selectionMark == 1)
                {
                    int partyCharge = GetChargeForParty(Session.Party);
                    if (Session.Party.PartyGold >= partyCharge)
                    {
                        AudioManager.PlayCue("Money");
                        Session.Party.PartyGold -= partyCharge;
                        selectionMark = 2;
                        ChangeDialogue(serviceRenderedMessage);
                        HealParty(Session.Party);
                    }
                    else
                    {
                        selectionMark = 2;
                        ChangeDialogue(noGoldMessage);
                    }
                }
                else
                {
                    ExitScreen();
                    return;
                }
            }
        }


        /// <summary>
        /// Change the current dialogue.
        /// </summary>
        private void ChangeDialogue(List<string> newDialogue)
        {
            currentDialogue = newDialogue;
            endIndex = maxLines;
            if (endIndex > currentDialogue.Count)
            {
                endIndex = currentDialogue.Count;
            }
        }


        /// <summary>
        /// Calculate the charge for the party's stay at the inn.
        /// </summary>
        private int GetChargeForParty(Party party)
        {
            // check the parameter 
            if (party == null)
            {
                throw new ArgumentNullException("party");
            }

            return inn.ChargePerPlayer * party.Players.Count;
        }



        /// <summary>
        /// Heal the party back to their correct values for level + gear.
        /// </summary>
        private void HealParty(Party party)
        {
            // check the parameter 
            if (party == null)
            {
                throw new ArgumentNullException("party");
            }

            // reset the statistics for each player
            foreach (Player player in party.Players)
            {
                player.StatisticsModifiers = new StatisticsValue();
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Vector2 dialogPosition = informationPosition;

            spriteBatch.Begin();

            // Draw fade screen
            spriteBatch.Draw(fadeTexture, screenRectangle, Color.White);

            // Draw the background
            spriteBatch.Draw(backgroundTexture, backgroundPosition, Color.White);
            // Draw the wooden plank
            spriteBatch.Draw(plankTexture, plankPosition, Color.White);
            // Draw the select icon
            spriteBatch.Draw(selectIconTexture, selectIconPosition, Color.White);
            // Draw the back icon
            spriteBatch.Draw(backIconTexture, backIconPosition, Color.White);
            // Draw the inn name on the wooden plank
            spriteBatch.DrawString(Fonts.HeaderFont, inn.Name, namePosition,
                Fonts.DisplayColor);

            // Draw the stay and leave option texts based on the current selection
            if (selectionMark == 1)
            {
                spriteBatch.Draw(highlightTexture, stayHighlightPosition, Color.White);
                spriteBatch.Draw(arrowTexture, stayArrowPosition, Color.White);
                spriteBatch.DrawString(
                    Fonts.GearInfoFont, stayString, stayPosition, Fonts.HighlightColor);
                spriteBatch.DrawString(
                    Fonts.GearInfoFont, leaveString, leavePosition, Fonts.DisplayColor);
            }
            else
            {
                spriteBatch.Draw(highlightTexture, leaveHighlightPosition, Color.White);
                spriteBatch.Draw(arrowTexture, leaveArrowPosition, Color.White);
                spriteBatch.DrawString(Fonts.GearInfoFont, stayString, stayPosition,
                    Fonts.DisplayColor);
                spriteBatch.DrawString(Fonts.GearInfoFont, leaveString, leavePosition, 
                    Fonts.HighlightColor);
            }
            // Draw the amount of gold
            spriteBatch.DrawString(Fonts.ButtonNamesFont,
                Fonts.GetGoldString(Session.Party.PartyGold), goldStringPosition,
                Color.White);
            // Draw the select button text
            spriteBatch.DrawString(
                Fonts.ButtonNamesFont, selectString, selectTextPosition, Color.White);
            // Draw the back button text
            spriteBatch.DrawString(Fonts.ButtonNamesFont, backString, backTextPosition,
                Color.White);

            // Draw Conversation Strip
            spriteBatch.Draw(conversationTexture, conversationStripPosition,
                Color.White);

            // Draw Shop Keeper
            spriteBatch.Draw(inn.ShopkeeperTexture, innKeeperPosition, Color.White);
            // Draw the cost to stay
            costString = "Cost: " + GetChargeForParty(Session.Party) + " Gold";
            spriteBatch.DrawString(Fonts.DescriptionFont, costString, costPosition,
                Color.DarkRed);
            // Draw the innkeeper dialog
            for (int i = 0; i < endIndex; i++)
            {
                spriteBatch.DrawString(Fonts.DescriptionFont, currentDialogue[i],
                    dialogPosition, Color.Black);
                dialogPosition.Y += Fonts.DescriptionFont.LineSpacing;
            }

            // Draw Gold Icon
            spriteBatch.Draw(goldIcon, goldIconPosition, Color.White);

            spriteBatch.End();
        }


        #endregion
    }
}
