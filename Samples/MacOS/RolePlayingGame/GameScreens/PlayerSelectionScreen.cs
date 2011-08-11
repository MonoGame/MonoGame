#region File Description
//-----------------------------------------------------------------------------
// PlayerSelectionScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using RolePlayingGameData;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Shows a list of players and allows the user to equip or use items.
    /// </summary>
    class PlayerSelectionScreen : GameScreen
    {
        private Gear usedGear;


        #region Player Data


        private bool isUseAllowed;
        private List<int> selectedPlayers;
        private StatisticsValue previewStatisticsModifier = new StatisticsValue();
        private Int32Range previewDamageRange = new Int32Range();
        private Int32Range previewHealthDefenseRange = new Int32Range();
        private Int32Range previewMagicDefenseRange = new Int32Range();


        #endregion


        #region Graphics Data


        private Texture2D playerInfoScreen;
        private Texture2D backButton;
        private Texture2D selectButton;
        private Texture2D scoreBoard;
        private Texture2D fadeTexture;
        private Texture2D tickMarkTexture;
        private Texture2D lineTexture;
        private Texture2D playerSelTexture;
        private Texture2D playerUnSelTexture;

        private readonly Vector2 textPosition = new Vector2(264f, 199f);
        private Vector2 currentTextPosition;
        private readonly Vector2 namePosition = new Vector2(394f, 39f);
        private Vector2 titlePosition;
        private readonly Vector2 scoreBoardPosition = new Vector2(972f, 235f);
        private readonly Vector2 selectButtonPosition = new Vector2(891f, 550f);
        private readonly Vector2 backButtonPosition = new Vector2(331f, 550f);
        private Vector2 popupPosition;
        private Vector2 playerNamePosition;
        private Vector2 portraitPosition;
        private readonly Point startPositionScreen = new Point(204, 44);
        private readonly Rectangle screenRect = new Rectangle(0, 0, 872, 633);


        #endregion


        #region Selection Data


        private int selectionMark;
        private bool isGearUsed;
        private int startIndex;
        private int endIndex;
        private int drawMaximum;


        #endregion


        #region Initialization


        /// <summary>
        /// Creates a new PlayerSelectionScreen object.
        /// </summary>
        public PlayerSelectionScreen(Gear gear)
        {
            // check the parameter
            if (gear == null)
            {
                throw new ArgumentNullException("gear");
            }

            this.IsPopup = true;
            this.usedGear = gear;

            isGearUsed = false;
            drawMaximum = 3;
            selectedPlayers = new List<int>();

            ResetValues();
            Reset();
        }


        /// <summary>
        /// Load the graphics content from the content manager.
        /// </summary>
        public override void  LoadContent()
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            ContentManager content = ScreenManager.Game.Content;

            fadeTexture = content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");

            // Display screens
            playerInfoScreen = 
                content.Load<Texture2D>(@"Textures\GameScreens\PopupScreen");
            popupPosition = new Vector2(viewport.Width / 2f, viewport.Height / 2f);
            popupPosition.X -= playerInfoScreen.Width / 2;
            popupPosition.Y -= playerInfoScreen.Height / 2;

            scoreBoard = 
                content.Load<Texture2D>(@"Textures\GameScreens\CountShieldWithArrow");
            lineTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\SeparationLine");
            selectButton = content.Load<Texture2D>(@"Textures\Buttons\AButton");
            backButton = content.Load<Texture2D>(@"Textures\Buttons\BButton");
            tickMarkTexture = content.Load<Texture2D>(@"Textures\GameScreens\TickMark");
            playerSelTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\PlayerSelected");
            playerUnSelTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\PlayerUnSelected");

            titlePosition = new Vector2(
                (viewport.Width - Fonts.HeaderFont.MeasureString("Choose Player").X) / 2,
                (viewport.Height - playerInfoScreen.Height) / 2 + 70f);
        }


        /// <summary>
        /// Reset the selection and player data.
        /// </summary>
        public void Reset()
        {
            if (selectionMark != -1)
            {
                isUseAllowed = true;
                if (usedGear != null)
                {
                    isUseAllowed = usedGear.CheckRestrictions(
                        Session.Party.Players[selectionMark]);
                }

                CalculateSelectedPlayers();
                CalculateForPreview();
            }
        }


        /// <summary>
        /// Reset the Variables to the Initial values
        /// </summary>
        private void ResetValues()
        {
            startIndex = 0;
            if (drawMaximum > Session.Party.Players.Count)
            {
                endIndex = Session.Party.Players.Count;
            }
            else
            {
                endIndex = drawMaximum;
            }
            selectionMark = 0;
            CalculateSelectedPlayers();
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
            // use the item or close the screen
            else if (isUseAllowed && 
                InputManager.IsActionTriggered(InputManager.Action.Ok))
            {
                if (isGearUsed)
                {
                    ExitScreen();
                    return;
                }
                else
                {
                    if (usedGear is Equipment)
                    {
                        Equipment equipment = usedGear as Equipment;
                        Equipment oldEquipment = null;
                        if (Session.Party.Players[selectionMark].Equip(equipment,
                            out oldEquipment))
                        {
                            Session.Party.RemoveFromInventory(usedGear, 1);
                            if (oldEquipment != null)
                            {
                                Session.Party.AddToInventory(oldEquipment, 1);
                            }
                            isGearUsed = true;
                        }
                    }
                    else if (usedGear is Item)
                    {
                        Item item = usedGear as Item;
                        if ((item.Usage & Item.ItemUsage.NonCombat) > 0)
                        {
                            if (Session.Party.RemoveFromInventory(item, 1))
                            {
                                Session.Party.Players[selectionMark].
                                    StatisticsModifiers +=
                                    item.TargetEffectRange.GenerateValue(Session.Random);
                                Session.Party.Players[selectionMark].StatisticsModifiers.
                                    ApplyMaximum(new StatisticsValue());
                                isGearUsed = true;
                            }
                            else
                            {
                                ExitScreen();
                                return;
                            }
                        }
                    }
                }
                return;
            }
            // cursor up
            else if (!isGearUsed && 
                InputManager.IsActionTriggered(InputManager.Action.CursorUp))
            {
                if (selectionMark > 0)
                {
                    ResetFromPreview();
                    selectionMark--;

                    if (selectionMark < startIndex)
                    {
                        startIndex--;
                        endIndex--;
                    }
                    isUseAllowed = true;
                    if (usedGear != null)
                    {
                        isUseAllowed = usedGear.CheckRestrictions(
                            Session.Party.Players[selectionMark]);
                    }

                    CalculateSelectedPlayers();
                    CalculateForPreview();
                }
            }
            // cursor down
            else if (!isGearUsed && 
                InputManager.IsActionTriggered(InputManager.Action.CursorDown))
            {
                isGearUsed = false;
                if (selectionMark < Session.Party.Players.Count - 1)
                {
                    ResetFromPreview();

                    selectionMark++;

                    if (selectionMark == endIndex)
                    {
                        endIndex++;
                        startIndex++;
                    }
                    isUseAllowed = true;
                    if (usedGear != null)
                    {
                        isUseAllowed = usedGear.CheckRestrictions(
                            Session.Party.Players[selectionMark]);
                    }

                    CalculateSelectedPlayers();
                    CalculateForPreview();
                }
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw the character stats screen and text
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.Draw(fadeTexture, new Rectangle(0, 0, 1280, 720), Color.White);

            currentTextPosition = textPosition;

            spriteBatch.Draw(playerInfoScreen, popupPosition, Color.White);

            // DrawButtons
            DrawButtons();

            // Draw Heros
            DrawViewablePlayers();

            // Display Title of the Screen
            spriteBatch.DrawString(Fonts.HeaderFont, "Choose Player", titlePosition,
                Fonts.TitleColor);

            spriteBatch.End();
        }


        /// <summary>
        /// Draw a player's Details
        /// </summary>
        /// <param name="player">Players whose details have to be drawn</param>
        /// <param name="isSelected">Whether player is selected or not</param>
        private void DrawPlayerDetails(Player player, bool isSelected)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            Vector2 position = new Vector2();
            Vector2 equipEffectPosition = new Vector2();
            Color textColor;
            Color nameColor, classColor, levelColor;
            string text;
            int length;

            if (isSelected)
            {
                textColor = Color.Black;
                nameColor = new Color(241, 173, 10);
                classColor = new Color(207, 131, 42);
                levelColor = new Color(151, 150, 148);
            }
            else
            {
                textColor = Color.DarkGray;
                nameColor = new Color(117, 88, 18);
                classColor = new Color(125, 78, 24);
                levelColor = new Color(110, 106, 99);
            }

            position = currentTextPosition;
            position.Y -= 5f;
            if (isSelected)
            {
                spriteBatch.Draw(playerSelTexture, position, Color.White);
            }
            else
            {
                spriteBatch.Draw(playerUnSelTexture, position, Color.White);
            }
            position.Y += 5f;

            // Draw portrait
            portraitPosition.X = position.X + 3f;
            portraitPosition.Y = position.Y + 16f;
            spriteBatch.Draw(player.ActivePortraitTexture, portraitPosition, 
                Color.White);
            if (isGearUsed && isSelected)
            {
                spriteBatch.Draw(tickMarkTexture, position, Color.White);
            }

            // Draw Player Name
            playerNamePosition.X = position.X + 90f;
            playerNamePosition.Y = position.Y + 15f;
            spriteBatch.DrawString(Fonts.PlayerNameFont,
                player.Name.ToUpper(), playerNamePosition, nameColor);

            // Draw Player Class
            playerNamePosition.Y += 25f;
            spriteBatch.DrawString(Fonts.PlayerNameFont, player.CharacterClass.Name,
                playerNamePosition, classColor);

            // Draw Player Level
            playerNamePosition.Y += 26f;
            spriteBatch.DrawString(Fonts.PlayerNameFont, "LEVEL: " +
                player.CharacterLevel,
                playerNamePosition, levelColor);
            position = currentTextPosition;
            position.X += playerSelTexture.Width + 5f;
            DrawPlayerStats(player, isSelected, ref position);

            equipEffectPosition = position;
            equipEffectPosition.X += 100f;
            equipEffectPosition.Y = currentTextPosition.Y;

            text = "Weapon Atk: (";
            length = (int)Fonts.DescriptionFont.MeasureString(text).X;

            spriteBatch.DrawString(Fonts.DescriptionFont, text, equipEffectPosition,
                Fonts.CountColor);
            equipEffectPosition.X += length;

            // calculate weapon damage
            previewDamageRange = new Int32Range();
            previewHealthDefenseRange = new Int32Range();
            previewMagicDefenseRange = new Int32Range();
            if (isSelected && isUseAllowed && !isGearUsed)
            {
                if (usedGear is Equipment)
                {
                    Equipment equipment = usedGear as Equipment;
                    if (equipment is Weapon)
                    {
                        Weapon weapon = equipment as Weapon;
                        previewDamageRange = weapon.TargetDamageRange;
                        Weapon equippedWeapon = player.GetEquippedWeapon();
                        if (equippedWeapon != null)
                        {
                            previewDamageRange -= equippedWeapon.TargetDamageRange;
                            previewDamageRange -=
                                equippedWeapon.OwnerBuffStatistics.PhysicalOffense;
                            previewHealthDefenseRange -= 
                                equippedWeapon.OwnerBuffStatistics.PhysicalDefense;
                            previewMagicDefenseRange -= 
                                equippedWeapon.OwnerBuffStatistics.MagicalDefense;
                        }
                    }
                    else if (equipment is Armor)
                    {
                        Armor armor = usedGear as Armor;
                        previewHealthDefenseRange = armor.OwnerHealthDefenseRange;
                        previewMagicDefenseRange = armor.OwnerMagicDefenseRange;
                        Armor equippedArmor = player.GetEquippedArmor(armor.Slot);
                        if (equippedArmor != null)
                        {
                            previewHealthDefenseRange -=
                                equippedArmor.OwnerHealthDefenseRange;
                            previewMagicDefenseRange -=
                                equippedArmor.OwnerMagicDefenseRange;
                            previewDamageRange -=
                                equippedArmor.OwnerBuffStatistics.PhysicalOffense;
                            previewHealthDefenseRange -=
                                equippedArmor.OwnerBuffStatistics.PhysicalDefense;
                            previewMagicDefenseRange -=
                                equippedArmor.OwnerBuffStatistics.MagicalDefense;
                        }
                    }
                    previewDamageRange += equipment.OwnerBuffStatistics.PhysicalOffense;
                    previewHealthDefenseRange +=
                        equipment.OwnerBuffStatistics.PhysicalDefense;
                    previewMagicDefenseRange +=
                        equipment.OwnerBuffStatistics.MagicalDefense;
                }
            }

            Int32Range drawWeaponDamageRange = player.TargetDamageRange +
                previewDamageRange + player.CharacterStatistics.PhysicalOffense;
            text = drawWeaponDamageRange.Minimum.ToString();
            length = (int)Fonts.DescriptionFont.MeasureString(text).X;

            textColor = GetRangeColor(previewDamageRange.Minimum, isSelected);
            spriteBatch.DrawString(Fonts.DescriptionFont, text,
                equipEffectPosition, textColor);
            equipEffectPosition.X += length;

            text = ",";
            length = (int)Fonts.DescriptionFont.MeasureString(text).X;

            spriteBatch.DrawString(Fonts.DescriptionFont,
                text, equipEffectPosition,
                Fonts.CountColor);
            equipEffectPosition.X += length;

            text = drawWeaponDamageRange.Maximum.ToString(); ;
            length = (int)Fonts.DescriptionFont.MeasureString(text).X;

            textColor = GetRangeColor(previewDamageRange.Maximum, isSelected);
            spriteBatch.DrawString(Fonts.DescriptionFont, text,
                equipEffectPosition, textColor);
            equipEffectPosition.X += length;

            spriteBatch.DrawString(Fonts.DescriptionFont, ")", equipEffectPosition,
                Fonts.CountColor);

            equipEffectPosition.X = position.X + 100f;
            equipEffectPosition.Y += Fonts.DescriptionFont.LineSpacing;
            text = "Weapon Def: (";
            length = (int)Fonts.DescriptionFont.MeasureString(text).X;

            spriteBatch.DrawString(Fonts.DescriptionFont, text, equipEffectPosition,
                Fonts.CountColor);
            equipEffectPosition.X += length;

            Int32Range drawHealthDefenseRange = player.HealthDefenseRange + 
                previewHealthDefenseRange + player.CharacterStatistics.PhysicalDefense;
            text = drawHealthDefenseRange.Minimum.ToString();
            length = (int)Fonts.DescriptionFont.MeasureString(text).X;

            textColor = GetRangeColor(previewHealthDefenseRange.Minimum, isSelected);
            spriteBatch.DrawString(Fonts.DescriptionFont, text,
                equipEffectPosition, textColor);
            equipEffectPosition.X += length;

            text = ",";
            length = (int)Fonts.DescriptionFont.MeasureString(text).X;

            spriteBatch.DrawString(Fonts.DescriptionFont, text, equipEffectPosition,
                Fonts.CountColor);
            equipEffectPosition.X += length;

            text = drawHealthDefenseRange.Maximum.ToString();
            length = (int)Fonts.DescriptionFont.MeasureString(text).X;

            textColor = GetRangeColor(previewHealthDefenseRange.Maximum, isSelected);
            spriteBatch.DrawString(Fonts.DescriptionFont, text,
                equipEffectPosition, textColor);
            equipEffectPosition.X += length;

            spriteBatch.DrawString(Fonts.DescriptionFont, ")", equipEffectPosition,
                Fonts.CountColor);


            equipEffectPosition.X = position.X + 100f;
            equipEffectPosition.Y += Fonts.DescriptionFont.LineSpacing;
            text = "Spell Def: (";
            length = (int)Fonts.DescriptionFont.MeasureString(text).X;

            spriteBatch.DrawString(Fonts.DescriptionFont, text, equipEffectPosition,
                Fonts.CountColor);
            equipEffectPosition.X += length;

            Int32Range drawMagicDefenseRange = player.MagicDefenseRange + 
                previewMagicDefenseRange + player.CharacterStatistics.MagicalDefense;
            text = drawMagicDefenseRange.Minimum.ToString();
            length = (int)Fonts.DescriptionFont.MeasureString(text).X;

            textColor = GetRangeColor(previewMagicDefenseRange.Minimum, isSelected);
            spriteBatch.DrawString(Fonts.DescriptionFont, text,
                equipEffectPosition, textColor);
            equipEffectPosition.X += length;

            text = ",";
            length = (int)Fonts.DescriptionFont.MeasureString(text).X;

            spriteBatch.DrawString(Fonts.DescriptionFont, text, equipEffectPosition,
                Fonts.CountColor);
            equipEffectPosition.X += length;

            text = drawMagicDefenseRange.Maximum.ToString();
            length = (int)Fonts.DescriptionFont.MeasureString(text).X;

            textColor = GetRangeColor(previewMagicDefenseRange.Maximum, isSelected);
            spriteBatch.DrawString(Fonts.DescriptionFont, text,
                equipEffectPosition, textColor);
            equipEffectPosition.X += length;

            spriteBatch.DrawString(Fonts.DescriptionFont, ")", equipEffectPosition,
                Fonts.CountColor);

            currentTextPosition.Y = position.Y + 3f;

            spriteBatch.Draw(lineTexture, currentTextPosition, Color.White);

            currentTextPosition.Y += 20f;
        }


        /// <summary>
        /// Draw a Player's stats 
        /// </summary>
        /// <param name="player">Player whose stats have to be drawn</param>
        /// <param name="isSelected">Whether player is selected or not</param>
        /// <param name="position">Position as to 
        /// where to start drawing the stats</param>
        private void DrawPlayerStats(Player player, bool isSelected,
            ref Vector2 position)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            Color color;
            string detail1, detail2;
            float length1, length2;

            StatisticsValue playersStatisticsModifier = new StatisticsValue();
            if (isSelected && isUseAllowed && !isGearUsed)
            {
                playersStatisticsModifier = previewStatisticsModifier;
                if (usedGear is Armor)
                {
                    Armor armor = usedGear as Armor;
                    Armor existingArmor = player.GetEquippedArmor(armor.Slot);
                    if (existingArmor != null)
                    {
                        playersStatisticsModifier -= existingArmor.OwnerBuffStatistics;
                    }
                }
                else if (usedGear is Weapon)
                {
                    Weapon weapon = usedGear as Weapon;
                    Weapon existingWeapon = player.GetEquippedWeapon();
                    if (existingWeapon != null)
                    {
                        playersStatisticsModifier -= existingWeapon.OwnerBuffStatistics;
                    }
                }
            }

            // Calculate HP and MP string Length
            detail1 = "HP: " + player.CurrentStatistics.HealthPoints + "/" + 
                player.CharacterStatistics.HealthPoints;
            length1 = Fonts.DescriptionFont.MeasureString(detail1).X;
            detail2 = "MP: " + player.CurrentStatistics.MagicPoints + "/" + 
                player.CharacterStatistics.MagicPoints;
            length2 = Fonts.DescriptionFont.MeasureString(detail2).X;

            StatisticsValue drawCurrentStatistics = player.CurrentStatistics;
            StatisticsValue drawCharacterStatistics = player.CharacterStatistics;
            if (isSelected)
            {
                drawCurrentStatistics += playersStatisticsModifier;
                drawCharacterStatistics += playersStatisticsModifier;
            }

            // Draw the character Health Points
            color = GetStatColor(playersStatisticsModifier.HealthPoints, isSelected);
            spriteBatch.DrawString(Fonts.DescriptionFont, "HP: " +
                drawCurrentStatistics.HealthPoints + "/" +
                drawCharacterStatistics.HealthPoints,
                position, color);

            // Draw the character Mana Points
            position.Y += Fonts.DescriptionFont.LineSpacing;
            color = GetStatColor(playersStatisticsModifier.MagicPoints, isSelected);
            spriteBatch.DrawString(Fonts.DescriptionFont, "MP: " +
                drawCurrentStatistics.MagicPoints + "/" +
                drawCharacterStatistics.MagicPoints,
                position, color);

            // Draw the physical offense
            position.X += 150f;
            position.Y -= Fonts.DescriptionFont.LineSpacing;
            color = GetStatColor(playersStatisticsModifier.PhysicalOffense, isSelected);
            spriteBatch.DrawString(Fonts.DescriptionFont, "PO: " +
                drawCurrentStatistics.PhysicalOffense, position, color);

            // Draw the physical defense
            position.Y += Fonts.DescriptionFont.LineSpacing;
            color = GetStatColor(playersStatisticsModifier.PhysicalDefense, isSelected);
            spriteBatch.DrawString(Fonts.DescriptionFont, "PD: " +
                drawCurrentStatistics.PhysicalDefense, position, color);

            // Draw the Magic offense
            position.Y += Fonts.DescriptionFont.LineSpacing;
            color = GetStatColor(playersStatisticsModifier.MagicalOffense, isSelected);
            spriteBatch.DrawString(Fonts.DescriptionFont, "MO: " +
                drawCurrentStatistics.MagicalOffense, position, color);

            // Draw the Magical defense
            position.Y += Fonts.DescriptionFont.LineSpacing;
            color = GetStatColor(playersStatisticsModifier.MagicalDefense, isSelected);
            spriteBatch.DrawString(Fonts.DescriptionFont, "MD: " +
                drawCurrentStatistics.MagicalDefense, position, color);
            

            position.Y += Fonts.DescriptionFont.LineSpacing;
        }


        /// <summary>
        /// Draw the Character Stats and Character Icons
        /// </summary>
        private void DrawViewablePlayers()
        {
            bool isSelectedPlayer = false;

            // Compute Start Index
            if (startIndex < 0)
            {
                startIndex = 0;
                selectionMark = 0;
                CalculateSelectedPlayers();
            }
            // Compute EndIndex
            if (endIndex > Session.Party.Players.Count)
            {
                endIndex = Session.Party.Players.Count;
                selectionMark = endIndex - 1;
                CalculateSelectedPlayers();
            }
            for (int playerIndex = startIndex; playerIndex < endIndex; playerIndex++)
            {
                isSelectedPlayer = false;
                for (int i = 0; i < selectedPlayers.Count; i++)
                {
                    if (playerIndex == selectedPlayers[i])
                    {
                        isSelectedPlayer = true;
                        break;
                    }
                }
                DrawPlayerDetails(Session.Party.Players[playerIndex], isSelectedPlayer);
            }
            // Draw the Scroll button only if player count exceed the Max items
            if (selectionMark != -1)
            {
                if (Session.Party.Players.Count > drawMaximum)
                {
                    DrawCharacterCount();
                }
            }
        }


        /// <summary>
        /// Draw the Current player Selected and total no.of
        /// Session.Party.Players in the list
        /// </summary>
        private void DrawCharacterCount()
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Vector2 position = new Vector2();

            // Draw the ScoreBoard
            spriteBatch.Draw(scoreBoard, scoreBoardPosition, Color.White);

            position = scoreBoardPosition;
            position.X += 29;
            position.Y += 100;

            // Display Current Selected Player
            spriteBatch.DrawString(Fonts.GearInfoFont,
                (selectionMark + 1).ToString(),
                position, Fonts.CountColor);
            position.Y += 30;
            // Display Total Players count
            spriteBatch.DrawString(Fonts.GearInfoFont, 
                Session.Party.Players.Count.ToString(), position, Fonts.CountColor);
        }


        /// <summary>
        /// Draw Select and Drop Button
        /// </summary>
        private void DrawButtons()
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Vector2 position;
            Vector2 placeTextMid;
            string selectText;

            if (usedGear == null)
            {
                selectText = "Use";
            }
            else if (usedGear is Item)
            {
                selectText = "Use";
            }
            else
            {
                selectText = "Equip";
            }
            if (CombatEngine.IsActive)
            {
                if (selectionMark != -1)
                {
                    isUseAllowed = true;
                }
            }

            if (isUseAllowed && !isGearUsed)
            {
                // Draw Select Button
                spriteBatch.Draw(selectButton, selectButtonPosition, Color.White);
                // Display Text
                position = selectButtonPosition;
                placeTextMid = Fonts.ButtonNamesFont.MeasureString(selectText);
                position.X -= placeTextMid.X + 10;
                spriteBatch.DrawString(Fonts.ButtonNamesFont, selectText, position,
                    Color.White);
            }

            // Draw Back Button
            spriteBatch.Draw(backButton, backButtonPosition, Color.White);
            // Display Back Text
            position = backButtonPosition;
            position.X += backButton.Width + 10;
            spriteBatch.DrawString(Fonts.ButtonNamesFont, "Back", position, Color.White);
        }


        /// <summary>
        /// Gets Font color for stat display based on whether the stat has changed
        /// </summary>
        /// <param name="change">How the stat has changed</param>
        /// <param name="isSelected">Character's selection status</param>
        /// <returns>Returns Color for display of stat</returns>
        private Color GetStatColor(int change, bool isSelected)
        {
            if (isSelected && isUseAllowed)
            {
                if (change < 0)
                {
                    return Color.Red;
                }
                else if (change > 0)
                {
                    return Color.Green;
                }
                // fall through when == 0
            }
            return Fonts.CountColor;
        }



        /// <summary>
        /// Decides min/max of Weapon Attack/Weapon Def/Spell Def of player
        /// </summary>
        /// <param name="value">Describes if min/max of range has
        /// changed or not</param>
        /// <param name="isSelected">Character's selection status</param>
        /// <returns>Returns the color to display min/max of the range</returns>
        private Color GetRangeColor(int value, bool isSelected)
        {
            if (isSelected && isUseAllowed)
            {
                if (value > 0)
                {
                    return Color.Green;
                }
                else if (value < 0)
                {
                    return Color.Red;
                }
                else
                {
                    return Fonts.CountColor;
                }
            }
            return Fonts.CountColor;
        }


        #endregion


        #region Preview Calculation


        /// <summary>
        /// Calculate selected Session.Party.Players around on the selection mark 
        /// based on the range for items. Incase of equipment range is considered as 0
        /// </summary>
        private void CalculateSelectedPlayers()
        {
            int range = 0;
            int selMark = selectionMark;

            selectedPlayers.Clear();

            Item item = usedGear as Item;
            if (item != null)
            {
                range = item.AdjacentTargets;
            }

            selectedPlayers.Add(selMark);
            for (int i = 1; i <= range; i++)
            {
                if ((selMark >= i) && 
                    !Session.Party.Players[selMark - i].IsDeadOrDying)
                {
                    selectedPlayers.Add(selMark - i);
                }
                if ((selMark < (Session.Party.Players.Count - i)) && 
                    !Session.Party.Players[selMark + i].IsDeadOrDying)
                {
                    selectedPlayers.Add(selMark + i);
                }
            }
        }


        /// <summary>
        /// Calculate for selected Session.Party.Players stats for preview
        /// </summary>
        private void CalculateForPreview()
        {
            previewStatisticsModifier = new StatisticsValue();
            previewDamageRange = new Int32Range();
            previewHealthDefenseRange = new Int32Range();
            previewMagicDefenseRange = new Int32Range();
            if (isUseAllowed && !isGearUsed)
            {
                if (usedGear is Item)
                {
                    // no preview for items
                }
                else if (usedGear is Armor)
                {
                    Armor armor = usedGear as Armor;
                    previewStatisticsModifier = armor.OwnerBuffStatistics;
                    previewHealthDefenseRange = armor.OwnerHealthDefenseRange;
                    previewMagicDefenseRange = armor.OwnerMagicDefenseRange;
                }
                else if (usedGear is Weapon)
                {
                    Weapon weapon = usedGear as Weapon;
                    previewStatisticsModifier = weapon.OwnerBuffStatistics;
                    previewDamageRange = weapon.TargetDamageRange;
                }
            }
        }


        /// <summary>
        /// Reset Stats of previously selected Session.Party.Players stats from preview
        /// </summary>
        private void ResetFromPreview()
        {
            previewStatisticsModifier = new StatisticsValue();
            previewDamageRange = new Int32Range();
            previewHealthDefenseRange = new Int32Range();
            previewMagicDefenseRange = new Int32Range();
        }

        #endregion
    }
}
