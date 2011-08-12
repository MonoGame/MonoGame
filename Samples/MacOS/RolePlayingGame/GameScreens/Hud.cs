#region File Description
//-----------------------------------------------------------------------------
// Hud.cs
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
    /// Displays each player's basic statistics and the combat action menu.
    /// </summary>
    class Hud
    {
        private ScreenManager screenManager;

        public const int HudHeight = 183;


        #region Graphics Data


        private Texture2D backgroundHudTexture;
        private Texture2D topHudTexture;
        private Texture2D combatPopupTexture;
        private Texture2D activeCharInfoTexture;
        private Texture2D inActiveCharInfoTexture;
        private Texture2D cantUseCharInfoTexture;
        private Texture2D selectionBracketTexture;
        private Texture2D menuTexture;
        private Texture2D statsTexture;
        private Texture2D deadPortraitTexture;
        private Texture2D charSelFadeLeftTexture;
        private Texture2D charSelFadeRightTexture;
        private Texture2D charSelArrowLeftTexture;
        private Texture2D charSelArrowRightTexture;
        private Texture2D actionTexture;
        private Texture2D yButtonTexture;
        private Texture2D startButtonTexture;

        private Vector2 topHudPosition = new Vector2(353f, 30f);
        private Vector2 charSelLeftPosition = new Vector2(70f, 600f);
        private Vector2 charSelRightPosition = new Vector2(1170f, 600f);
        private Vector2 yButtonPosition = new Vector2(0f, 560f + 20f);
        private Vector2 startButtonPosition = new Vector2(0f, 560f + 35f);
        private Vector2 yTextPosition = new Vector2(0f, 560f + 70f);
        private Vector2 startTextPosition = new Vector2(0f, 560f + 70f);
        private Vector2 actionTextPosition = new Vector2(640f, 55f);
        private Vector2 backgroundHudPosition = new Vector2(0f, 525f);
        private Vector2 portraitPosition = new Vector2(640f, 55f);
        private Vector2 startingInfoPosition = new Vector2(0f, 550f);
        private Vector2 namePosition;
        private Vector2 levelPosition;
        private Vector2 detailPosition;

        private readonly Color activeNameColor = new Color(200, 200, 200);
        private readonly Color inActiveNameColor = new Color(100, 100, 100);
        private readonly Color nonSelColor = new Color(86, 26, 5);
        private readonly Color selColor = new Color(229, 206, 144);


        #endregion


        #region Action Text


        /// <summary>
        /// The text that is shown in the action bar at the top of the combat screen.
        /// </summary>
        private string actionText = String.Empty;

        /// <summary>
        /// The text that is shown in the action bar at the top of the combat screen.
        /// </summary>
        public string ActionText
        {
            get { return actionText; }
            set { actionText = value; }
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Creates a new Hud object using the given ScreenManager.
        /// </summary>
        public Hud(ScreenManager screenManager)
        {
            // check the parameter
            if (screenManager == null)
            {
                throw new ArgumentNullException("screenManager");
            }
            this.screenManager = screenManager;
        }
        

        /// <summary>
        /// Load the graphics content from the content manager.
        /// </summary>
        public void LoadContent()
        {
            ContentManager content = screenManager.Game.Content;

            backgroundHudTexture = 
                content.Load<Texture2D>(@"Textures\HUD\HudBkgd");
            topHudTexture = 
                content.Load<Texture2D>(@"Textures\HUD\CombatStateInfoStrip");
            activeCharInfoTexture =
                content.Load<Texture2D>(@"Textures\HUD\PlankActive");
            inActiveCharInfoTexture =
                content.Load<Texture2D>(@"Textures\HUD\PlankInActive");
            cantUseCharInfoTexture = 
                content.Load<Texture2D>(@"Textures\HUD\PlankCantUse");
            selectionBracketTexture = 
                content.Load<Texture2D>(@"Textures\HUD\SelectionBrackets");
            deadPortraitTexture = 
                content.Load<Texture2D>(@"Textures\Characters\Portraits\Tombstone");
            combatPopupTexture = 
                content.Load<Texture2D>(@"Textures\HUD\CombatPopup");
            charSelFadeLeftTexture =
                content.Load<Texture2D>(@"Textures\Buttons\CharSelectFadeLeft");
            charSelFadeRightTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\CharSelectFadeRight");
            charSelArrowLeftTexture =
                content.Load<Texture2D>(@"Textures\Buttons\CharSelectHlLeft");
            charSelArrowRightTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\CharSelectHlRight");
            actionTexture = 
                content.Load<Texture2D>(@"Textures\HUD\HudSelectButton");
            yButtonTexture =
                content.Load<Texture2D>(@"Textures\Buttons\YButton");
            startButtonTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\StartButton");
            menuTexture = 
                content.Load<Texture2D>(@"Textures\HUD\Menu");
            statsTexture =
                content.Load<Texture2D>(@"Textures\HUD\Stats");
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw the screen.
        /// </summary>
        public void Draw()
        {
            SpriteBatch spriteBatch = screenManager.SpriteBatch;

            spriteBatch.Begin();

            startingInfoPosition.X = 640f;

            startingInfoPosition.X -= Session.Party.Players.Count / 2 * 200f;
            if (Session.Party.Players.Count % 2 != 0)
            {
                startingInfoPosition.X -= 100f;
            } 

            spriteBatch.Draw(backgroundHudTexture, backgroundHudPosition, Color.White);

            if (CombatEngine.IsActive)
            {
                DrawForCombat();
            }
            else
            {
                DrawForNonCombat();
            }

            spriteBatch.End();
        }


        /// <summary>
        /// Draws HUD for Combat Mode
        /// </summary>
        private void DrawForCombat()
        {
            SpriteBatch spriteBatch = screenManager.SpriteBatch;
            Vector2 position = startingInfoPosition;

            foreach (CombatantPlayer combatantPlayer in CombatEngine.Players)
            {
                DrawCombatPlayerDetails(combatantPlayer, position);
                position.X += activeCharInfoTexture.Width - 6f;
            }

            charSelLeftPosition.X = startingInfoPosition.X - 5f - 
                charSelArrowLeftTexture.Width;
            charSelRightPosition.X = position.X + 5f;
            // Draw character Selection Arrows
            if (CombatEngine.IsPlayersTurn)
            {
                spriteBatch.Draw(charSelArrowLeftTexture, charSelLeftPosition,
                    Color.White);
                spriteBatch.Draw(charSelArrowRightTexture, charSelRightPosition,
                    Color.White);
            }
            else
            {
                spriteBatch.Draw(charSelFadeLeftTexture, charSelLeftPosition,
                    Color.White);
                spriteBatch.Draw(charSelFadeRightTexture, charSelRightPosition,
                    Color.White);
            }

            if (actionText.Length > 0)
            {
                spriteBatch.Draw(topHudTexture, topHudPosition, Color.White);
                // Draw Action Text
                Fonts.DrawCenteredText(spriteBatch, Fonts.PlayerStatisticsFont,
                    actionText, actionTextPosition, Color.Black);
            }
        }


        /// <summary>
        /// Draws HUD for non Combat Mode
        /// </summary>
        private void DrawForNonCombat()
        {
            SpriteBatch spriteBatch = screenManager.SpriteBatch;

            Vector2 position = startingInfoPosition;

            foreach (Player player in Session.Party.Players)
            {
                DrawNonCombatPlayerDetails(player, position);

                position.X += inActiveCharInfoTexture.Width - 6f;
            }

            yTextPosition.X = position.X + 5f;
            yButtonPosition.X = position.X + 9f;

            // Draw Select Button
            spriteBatch.Draw(statsTexture, yTextPosition, Color.White);
            spriteBatch.Draw(yButtonTexture, yButtonPosition, Color.White);

            startTextPosition.X = startingInfoPosition.X - 
                startButtonTexture.Width - 25f;
            startButtonPosition.X = startingInfoPosition.X - 
                startButtonTexture.Width - 10f;

            // Draw Back Button
            spriteBatch.Draw(menuTexture, startTextPosition, Color.White);
            spriteBatch.Draw(startButtonTexture, startButtonPosition, Color.White);
        }


        enum PlankState
        {
            Active,
            InActive,
            CantUse,
        }


        /// <summary>
        /// Draws Player Details
        /// </summary>
        /// <param name="playerIndex">Index of player details to draw</param>
        /// <param name="position">Position where to draw</param>
        private void DrawCombatPlayerDetails(CombatantPlayer player, Vector2 position)
        {
            SpriteBatch spriteBatch = screenManager.SpriteBatch;

            PlankState plankState;
            bool isPortraitActive = false;
            bool isCharDead = false;
            Color color;

            portraitPosition.X = position.X + 7f;
            portraitPosition.Y = position.Y + 7f;

            namePosition.X = position.X + 84f;
            namePosition.Y = position.Y + 12f;

            levelPosition.X = position.X + 84f;
            levelPosition.Y = position.Y + 39f;

            detailPosition.X = position.X + 25f;
            detailPosition.Y = position.Y + 66f;

            position.X -= 2;
            position.Y -= 4;

            if (player.IsTurnTaken)
            {
                plankState = PlankState.CantUse;

                isPortraitActive = false;
            }
            else
            {
                plankState = PlankState.InActive;

                isPortraitActive = true;
            }

            if (((CombatEngine.HighlightedCombatant == player) && !player.IsTurnTaken) ||
                (CombatEngine.PrimaryTargetedCombatant == player) ||
                (CombatEngine.SecondaryTargetedCombatants.Contains(player)))
            {
                plankState = PlankState.Active;
            }

            if (player.IsDeadOrDying)
            {
                isCharDead = true;
                isPortraitActive = false;
                plankState = PlankState.CantUse;
            }

            // Draw Info Slab
            if (plankState == PlankState.Active)
            {
                color = activeNameColor;

                spriteBatch.Draw(activeCharInfoTexture, position, Color.White);

                // Draw Brackets
                if ((CombatEngine.HighlightedCombatant == player) && !player.IsTurnTaken)
                {
                    spriteBatch.Draw(selectionBracketTexture, position, Color.White);
                }

                if (isPortraitActive &&
                    (CombatEngine.HighlightedCombatant == player) &&
                    (CombatEngine.HighlightedCombatant.CombatAction == null) &&
                    !CombatEngine.IsDelaying)
                {
                    position.X += activeCharInfoTexture.Width / 2;
                    position.X -= combatPopupTexture.Width / 2;
                    position.Y -= combatPopupTexture.Height;
                    // Draw Action
                    DrawActionsMenu(position);
                }
            }
            else if (plankState == PlankState.InActive)
            {
                color = inActiveNameColor;
                spriteBatch.Draw(inActiveCharInfoTexture, position, Color.White);
            }
            else
            {
                color = Color.Black;
                spriteBatch.Draw(cantUseCharInfoTexture, position, Color.White);
            }

            if (isCharDead)
            {
                spriteBatch.Draw(deadPortraitTexture, portraitPosition, Color.White);
            }
            else
            {
                // Draw Player Portrait
                DrawPortrait(player.Player, portraitPosition, plankState);
            }

            // Draw Player Name
            spriteBatch.DrawString(Fonts.PlayerStatisticsFont,
                player.Player.Name,
                namePosition, color);

            color = Color.Black;
            // Draw Player Details
            spriteBatch.DrawString(Fonts.HudDetailFont,
                "Lvl: " + player.Player.CharacterLevel,
                levelPosition, color);

            spriteBatch.DrawString(Fonts.HudDetailFont,
                "HP: " + player.Statistics.HealthPoints +
                "/" + player.Player.CharacterStatistics.HealthPoints,
                detailPosition, color);

            detailPosition.Y += 30f;
            spriteBatch.DrawString(Fonts.HudDetailFont,
                "MP: " + player.Statistics.MagicPoints +
                "/" + player.Player.CharacterStatistics.MagicPoints,
                detailPosition, color);
        }


        /// <summary>
        /// Draws Player Details
        /// </summary>
        /// <param name="playerIndex">Index of player details to draw</param>
        /// <param name="position">Position where to draw</param>
        private void DrawNonCombatPlayerDetails(Player player, Vector2 position)
        {
            SpriteBatch spriteBatch = screenManager.SpriteBatch;

            PlankState plankState;
            bool isCharDead = false;
            Color color;

            portraitPosition.X = position.X + 7f;
            portraitPosition.Y = position.Y + 7f;

            namePosition.X = position.X + 84f;
            namePosition.Y = position.Y + 12f;

            levelPosition.X = position.X + 84f;
            levelPosition.Y = position.Y + 39f;

            detailPosition.X = position.X + 25f;
            detailPosition.Y = position.Y + 66f;

            position.X -= 2;
            position.Y -= 4;

            plankState = PlankState.Active;

            // Draw Info Slab
            if (plankState == PlankState.Active)
            {
                color = activeNameColor;

                spriteBatch.Draw(activeCharInfoTexture, position, Color.White);
            }
            else if (plankState == PlankState.InActive)
            {
                color = inActiveNameColor;
                spriteBatch.Draw(inActiveCharInfoTexture, position, Color.White);
            }
            else
            {
                color = Color.Black;
                spriteBatch.Draw(cantUseCharInfoTexture, position, Color.White);
            }

            if (isCharDead)
            {
                spriteBatch.Draw(deadPortraitTexture, portraitPosition, Color.White);
            }
            else
            {
                // Draw Player Portrait
                DrawPortrait(player, portraitPosition, plankState);
            }

            // Draw Player Name
            spriteBatch.DrawString(Fonts.PlayerStatisticsFont,
                player.Name,
                namePosition, color);

            color = Color.Black;
            // Draw Player Details
            spriteBatch.DrawString(Fonts.HudDetailFont,
                "Lvl: " + player.CharacterLevel,
                levelPosition, color);

            spriteBatch.DrawString(Fonts.HudDetailFont,
                "HP: " + player.CurrentStatistics.HealthPoints +
                "/" + player.CharacterStatistics.HealthPoints,
                detailPosition, color);

            detailPosition.Y += 30f;
            spriteBatch.DrawString(Fonts.HudDetailFont,
                "MP: " + player.CurrentStatistics.MagicPoints +
                "/" + player.CharacterStatistics.MagicPoints,
                detailPosition, color);
        }


        /// <summary>
        /// Draw the portrait of the given player at the given position.
        /// </summary>
        private void DrawPortrait(Player player, Vector2 position, 
            PlankState plankState)
        {
            switch (plankState)
            {
                case PlankState.Active:
                    screenManager.SpriteBatch.Draw(player.ActivePortraitTexture, 
                        position, Color.White);
                    break;
                case PlankState.InActive:
                    screenManager.SpriteBatch.Draw(player.InactivePortraitTexture, 
                        position, Color.White);
                    break;
                case PlankState.CantUse:
                    screenManager.SpriteBatch.Draw(player.UnselectablePortraitTexture, 
                        position, Color.White);
                    break;
            }
        }
        
        
        #endregion


        #region Combat Action Menu


        /// <summary>
        /// The list of entries in the combat action menu.
        /// </summary>
        private string[] actionList = new string[5]
            {
                "Attack",
                "Spell",
                "Item",
                "Defend",
                "Flee",
            };


        /// <summary>
        /// The currently highlighted item.
        /// </summary>
        private int highlightedAction = 0;


        /// <summary>
        /// Handle user input to the actions menu.
        /// </summary>
        public void UpdateActionsMenu()
        {
            // cursor up
            if (InputManager.IsActionTriggered(InputManager.Action.CursorUp))
            {
                if (highlightedAction > 0)
                {
                    highlightedAction--;
                }
                return;
            }
            // cursor down
            if (InputManager.IsActionTriggered(InputManager.Action.CursorDown))
            {
                if (highlightedAction < actionList.Length - 1)
                {
                    highlightedAction++;
                }
                return;
            }
            // select an action
            if (InputManager.IsActionTriggered(InputManager.Action.Ok))
            {
                switch (actionList[highlightedAction])
                {
                    case "Attack":
                        {
                            ActionText = "Performing a Melee Attack";
                            CombatEngine.HighlightedCombatant.CombatAction =
                                new MeleeCombatAction(CombatEngine.HighlightedCombatant);
                            CombatEngine.HighlightedCombatant.CombatAction.Target =
                                CombatEngine.FirstEnemyTarget;
                        }
                        break;

                    case "Spell":
                        {
                            SpellbookScreen spellbookScreen = new SpellbookScreen(
                                CombatEngine.HighlightedCombatant.Character,
                                CombatEngine.HighlightedCombatant.Statistics);
                            spellbookScreen.SpellSelected +=
                                new SpellbookScreen.SpellSelectedHandler(
                                spellbookScreen_SpellSelected);
                            Session.ScreenManager.AddScreen(spellbookScreen);
                        }
                        break;

                    case "Item":
                        {
                            InventoryScreen inventoryScreen = new InventoryScreen(true);
                            inventoryScreen.GearSelected +=
                                new InventoryScreen.GearSelectedHandler(
                                inventoryScreen_GearSelected);
                            Session.ScreenManager.AddScreen(inventoryScreen);
                        }
                        break;

                    case "Defend":
                        {
                            ActionText = "Defending";
                            CombatEngine.HighlightedCombatant.CombatAction =
                                new DefendCombatAction(
                                CombatEngine.HighlightedCombatant);
                            CombatEngine.HighlightedCombatant.CombatAction.Start();
                        }
                        break;

                    case "Flee":
                        CombatEngine.AttemptFlee();
                        break;
                }
                return;
            }
        }


        /// <summary>
        /// Recieves the spell from the Spellbook screen and casts it.
        /// </summary>
        void spellbookScreen_SpellSelected(Spell spell)
        {
            if (spell != null)
            {
                ActionText = "Casting " + spell.Name;
                CombatEngine.HighlightedCombatant.CombatAction =
                    new SpellCombatAction(CombatEngine.HighlightedCombatant, spell);
                if (spell.IsOffensive)
                {
                    CombatEngine.HighlightedCombatant.CombatAction.Target =
                        CombatEngine.FirstEnemyTarget;
                }
                else
                {
                    CombatEngine.HighlightedCombatant.CombatAction.Target =
                        CombatEngine.HighlightedCombatant;
                }
            }
        }


        /// <summary>
        /// Receives the item back from the Inventory screen and uses it.
        /// </summary>
        void inventoryScreen_GearSelected(Gear gear)
        {
            Item item = gear as Item;
            if (item != null)
            {
                ActionText = "Using " + item.Name;
                CombatEngine.HighlightedCombatant.CombatAction =
                    new ItemCombatAction(CombatEngine.HighlightedCombatant, item);
                if (item.IsOffensive)
                {
                    CombatEngine.HighlightedCombatant.CombatAction.Target =
                        CombatEngine.FirstEnemyTarget;
                }
                else
                {
                    CombatEngine.HighlightedCombatant.CombatAction.Target =
                        CombatEngine.HighlightedCombatant;
                }
            }
        }


        /// <summary>
        /// Draws the combat action menu.
        /// </summary>
        /// <param name="position">The position of the menu.</param>
        private void DrawActionsMenu(Vector2 position)
        {
            ActionText = "Choose an Action";

            SpriteBatch spriteBatch = screenManager.SpriteBatch;

            Vector2 arrowPosition;
            float height = 25f;

            spriteBatch.Draw(combatPopupTexture, position, Color.White);

            position.Y += 21f;
            arrowPosition = position;

            arrowPosition.X += 10f;
            arrowPosition.Y += 2f;
            arrowPosition.Y += height * (int)highlightedAction;
            spriteBatch.Draw(actionTexture, arrowPosition, Color.White);

            position.Y += 4f;
            position.X += 50f;

            // Draw Action Text
            for (int i = 0; i < actionList.Length; i++)
            {
                spriteBatch.DrawString(Fonts.GearInfoFont, actionList[i], position,
                    i == highlightedAction ? selColor : nonSelColor);
                position.Y += height;
            }
        }


        #endregion
    }
}
