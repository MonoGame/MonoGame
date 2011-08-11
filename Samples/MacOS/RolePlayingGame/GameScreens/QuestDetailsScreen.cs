#region File Description
//-----------------------------------------------------------------------------
// QuestDetailsScreen.cs
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
    /// Display the details of a particular quest.
    /// </summary>
    class QuestDetailsScreen : GameScreen
    {
        private Quest quest;


        #region Graphics Data


        private Texture2D backgroundTexture;
        private Texture2D backIconTexture;
        private Texture2D scrollTexture;
        private Texture2D fadeTexture;
        private Texture2D lineTexture;

        private Vector2 backgroundPosition;
        private Vector2 screenSize;
        private Vector2 titlePosition;
        private Vector2 textPosition;
        private Vector2 backTextPosition;
        private Vector2 backIconPosition;
        private Vector2 scrollPosition;
        private Vector2 topLinePosition;
        private Vector2 bottomLinePosition;
        private Rectangle fadeDest;

        private Color headerColor = new Color(128, 6, 6);
        private Color textColor = new Color(102, 40, 16);


        #endregion


        #region Dialog Text


        private string titleString = "Quest Details";
        private List<Line> currentDialogue;

        private int startIndex;
        private int endIndex;
        private int maxLines;


        #endregion


        #region Initialization


        /// <summary>
        /// Creates a new QuestDetailsScreen object.
        /// </summary>
        public QuestDetailsScreen(Quest quest)
            : base()
        {
            // check the parameter
            if (quest == null)
            {
                throw new ArgumentNullException("quest");
            }
            this.quest = quest;
            this.IsPopup = true;

            currentDialogue = new List<Line>();
            maxLines = 13;

            textPosition.X = 261f;

            AddStrings(this.quest.Name,
                Fonts.BreakTextIntoList(this.quest.Description,
                Fonts.DescriptionFont, 715), GetRequirements(this.quest));
        }


        /// <summary>
        /// Loads the graphics content from the content manager.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            backgroundTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\PopupScreen");
            backIconTexture =
                content.Load<Texture2D>(@"Textures\Buttons\BButton");
            scrollTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\ScrollButtons");
            lineTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\SeparationLine");
            fadeTexture = content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");

            // Get the screen positions
            screenSize = new Vector2(viewport.Width, viewport.Height);
            fadeDest = new Rectangle(viewport.X, viewport.Y,
                viewport.Width, viewport.Height);

            backgroundPosition = new Vector2(
                (viewport.Width - backgroundTexture.Width) / 2,
                (viewport.Height - backgroundTexture.Height) / 2);
            scrollPosition = backgroundPosition + new Vector2(820f, 200f);

            titlePosition = new Vector2(
                (screenSize.X - Fonts.HeaderFont.MeasureString(titleString).X) / 2,
                backgroundPosition.Y + 70f);

            backTextPosition = new Vector2(screenSize.X / 2 - 250f,
                backgroundPosition.Y + 530f);
            backIconPosition = new Vector2(
                backTextPosition.X - backIconTexture.Width - 10f, backTextPosition.Y);

            topLinePosition = new Vector2(
                (viewport.Width - lineTexture.Width) / 2 - 30f, 200f);
            bottomLinePosition = new Vector2(topLinePosition.X, 550f);
        }


        #endregion


        #region Text Generation


        /// <summary>
        /// A line of text with its own color and font.
        /// </summary>
        private struct Line
        {
            public string text;
            public Color color;
            public SpriteFont font;
        }


        /// <summary>
        /// Add strings to list of lines
        /// </summary>
        /// <param name="name">Name of the quest</param>
        /// <param name="description">Description of the quest</param>
        /// <param name="requirements">Requirements of the quest</param>
        private void AddStrings(string name, List<string> description,
            List<Line> requirements)
        {
            Line line;

            line.color = headerColor;
            line.font = Fonts.DescriptionFont;

            // Title text
            titleString = name;
            titlePosition.X = (screenSize.X -
                Fonts.HeaderFont.MeasureString(titleString).X) / 2;
            titlePosition.Y = backgroundPosition.Y + 70f;

            currentDialogue.Clear();
            line.text = "Description";
            currentDialogue.Add(line);
            foreach (string str in description)
            {
                line.text = str;
                line.color = textColor;
                currentDialogue.Add(line);
            }
            foreach (Line str in requirements)
            {
                currentDialogue.Add(str);
            }
            // Set the start index and end index
            startIndex = 0;
            endIndex = maxLines;
            if (endIndex > currentDialogue.Count)
            {
                textPosition.Y = 375f;
                foreach (Line str in currentDialogue)
                {
                    textPosition.Y -= str.font.LineSpacing / 2;
                }

                endIndex = currentDialogue.Count;
            }
            else
            {
                textPosition.Y = 225f;
            }
        }


        /// <summary>
        /// Get the quest requirements
        /// </summary>
        /// <param name="quest">The particular quest</param>
        /// <returns>List of strings containing formatted output</returns>
        private List<Line> GetRequirements(Quest quest)
        {
            List<Line> reqdList;
            Line reqd;
            int currentCount = 0;
            int totalCount = 0;
            List<string> dialog;

            reqdList = new List<Line>();
            reqd.font = Fonts.DescriptionFont;

            // Add Monster Requirements
            if (quest.MonsterRequirements.Count > 0)
            {
                reqd.color = headerColor;
                reqd.text = String.Empty;
                reqdList.Add(reqd);
                reqd.text = "Monster Progress";
                reqdList.Add(reqd);

                for (int i = 0; i < quest.MonsterRequirements.Count; i++)
                {
                    reqd.color = textColor;
                    currentCount = quest.MonsterRequirements[i].CompletedCount;
                    totalCount = quest.MonsterRequirements[i].Count;
                    Monster monster = quest.MonsterRequirements[i].Content;
                    reqd.text = monster.Name + " = " + currentCount + " / " + 
                        totalCount;

                    if (currentCount == totalCount)
                    {
                        reqd.color = Color.Red;
                    }
                    reqdList.Add(reqd);
                }
            }

            // Add Item Requirements
            if (quest.GearRequirements.Count > 0)
            {
                reqd.color = headerColor;
                reqd.text = String.Empty;
                reqdList.Add(reqd);
                reqd.text = "Item Progress";
                reqdList.Add(reqd);

                for (int i = 0; i < quest.GearRequirements.Count; i++)
                {
                    reqd.color = textColor;
                    currentCount = quest.GearRequirements[i].CompletedCount;
                    totalCount = quest.GearRequirements[i].Count;
                    Gear gear = quest.GearRequirements[i].Content;
                    reqd.text = gear.Name + " = " + currentCount + " / " + totalCount;
                    if (currentCount == totalCount)
                    {
                        reqd.color = Color.Red;
                    }
                    reqdList.Add(reqd);
                }
            }

            // Add Current Objective
            reqd.color = headerColor;
            reqd.text = String.Empty;
            reqdList.Add(reqd);
            reqd.text = "Current Objective";
            reqdList.Add(reqd);
            reqd.color = textColor;

            switch (quest.Stage)
            {
                case Quest.QuestStage.InProgress:
                    dialog = Fonts.BreakTextIntoList(quest.ObjectiveMessage,
                        Fonts.DescriptionFont, 715);
                    for (int i = 0; i < dialog.Count; i++)
                    {
                        reqd.text = dialog[i];
                        reqdList.Add(reqd);
                    }
                    break;

                case Quest.QuestStage.RequirementsMet:
                    dialog = Fonts.BreakTextIntoList(quest.DestinationObjectiveMessage,
                        Fonts.DescriptionFont, 715);
                    for (int i = 0; i < dialog.Count; i++)
                    {
                        reqd.text = dialog[i];
                        reqdList.Add(reqd);
                    }
                    break;

                case Quest.QuestStage.Completed:
                    reqd.font = Fonts.ButtonNamesFont;
                    reqd.color = new Color(139, 21, 73);
                    reqd.text = "Quest Completed";
                    reqdList.Add(reqd);
                    break;
            }

            return reqdList;
        }


        #endregion


        #region Updating


        /// <summary>
        /// Handles user input.
        /// </summary>
        public override void HandleInput()
        {
            // exit the screen
            if (InputManager.IsActionTriggered(InputManager.Action.Back) ||
                InputManager.IsActionTriggered(InputManager.Action.Ok))
            {
                ExitScreen();
                return;
            }
            // scroll up
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorUp))
            {
                if (startIndex > 0)
                {
                    startIndex--;
                    endIndex--;
                }
            }
            // scroll Down
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorDown))
            {
                if (endIndex < currentDialogue.Count)
                {
                    startIndex++;
                    endIndex++;
                }
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw the screen
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Vector2 dialoguePosition = textPosition;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw the fading screen
            spriteBatch.Draw(fadeTexture, fadeDest, Color.White);

            // Draw the popup background
            spriteBatch.Draw(backgroundTexture, backgroundPosition, Color.White);

            // Draw the top line
            spriteBatch.Draw(lineTexture, topLinePosition, Color.White);

            // Draw the bottom line
            spriteBatch.Draw(lineTexture, bottomLinePosition, Color.White);

            // Draw the scrollbar
            spriteBatch.Draw(scrollTexture, scrollPosition, Color.White);

            // Draw the Back button
            spriteBatch.Draw(backIconTexture, backIconPosition, Color.White);
            spriteBatch.DrawString(Fonts.ButtonNamesFont, "Back", backTextPosition,
                Color.White);

            // Draw the title
            spriteBatch.DrawString(Fonts.HeaderFont, titleString, titlePosition,
                Fonts.TitleColor);

            //Draw the information dialog
            for (int i = startIndex; i < endIndex; i++)
            {
                dialoguePosition.X = (int)((screenSize.X -
                    currentDialogue[i].font.MeasureString(
                    currentDialogue[i].text).X) / 2) - 20;

                spriteBatch.DrawString(currentDialogue[i].font,
                    currentDialogue[i].text, dialoguePosition, 
                    currentDialogue[i].color);
                dialoguePosition.Y += currentDialogue[i].font.LineSpacing;
            }

            spriteBatch.End();
        }


        #endregion
    }
}
