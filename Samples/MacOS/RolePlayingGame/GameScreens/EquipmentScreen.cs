#region File Description
//-----------------------------------------------------------------------------
// EquipmentScreen.cs
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
    /// Lists the player's equipped gear, and allows the user to unequip them.
    /// </summary>
    class EquipmentScreen : ListScreen<Equipment>
    {
        #region Columns


        protected string nameColumnText = "Name";
        private const int nameColumnInterval = 80;

        protected string powerColumnText = "Power (min, max)";
        private const int powerColumnInterval = 270;

        protected string slotColumnText = "Slot";
        private const int slotColumnInterval = 400;


        #endregion


        #region Data Access


        /// <summary>
        /// The FightingCharacter object whose equipment is displayed.
        /// </summary>
        private FightingCharacter fightingCharacter;


        /// <summary>
        /// Get the list that this screen displays.
        /// </summary>
        public override ReadOnlyCollection<Equipment> GetDataList()
        {
            return fightingCharacter.EquippedEquipment.AsReadOnly();
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Creates a new EquipmentScreen object for the given player.
        /// </summary>
        public EquipmentScreen(FightingCharacter fightingCharacter)
            : base()
        {
            // check the parameter
            if (fightingCharacter == null)
            {
                throw new ArgumentNullException("fightingCharacter");
            }
            this.fightingCharacter = fightingCharacter;

            // sort the player's equipment
            this.fightingCharacter.EquippedEquipment.Sort(
                delegate(Equipment equipment1, Equipment equipment2)
                {
                    // handle null values
                    if (equipment1 == null)
                    {
                        return (equipment2 == null ? 0 : 1);
                    }
                    else if (equipment2 == null)
                    {
                        return -1;
                    }

                    // handle weapons - they're always first in the list
                    if (equipment1 is Weapon)
                    {
                        return (equipment2 is Weapon ?
                            equipment1.Name.CompareTo(equipment2.Name) : -1);
                    }
                    else if (equipment2 is Weapon)
                    {
                        return 1;
                    }

                    // compare armor slots
                    Armor armor1 = equipment1 as Armor;
                    Armor armor2 = equipment2 as Armor;
                    if ((armor1 != null) && (armor2 != null))
                    {
                        return armor1.Slot.CompareTo(armor2.Slot);
                    }

                    return 0;
                });

            // configure the menu text
            titleText = "Equipped Gear";
            selectButtonText = String.Empty;
            backButtonText = "Back";
            xButtonText = "Unequip";
            yButtonText = String.Empty;
            leftTriggerText = String.Empty;
            rightTriggerText = String.Empty;
        }


        #endregion


        #region Input Handling


        /// <summary>
        /// Respond to the triggering of the X button (and related key).
        /// </summary>
        protected override void ButtonXPressed(Equipment entry)
        {
            // remove the equipment from the player's equipped list
            fightingCharacter.Unequip(entry);

            // add the equipment back to the party's inventory
            Session.Party.AddToInventory(entry, 1);
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw the equipment at the given position in the list.
        /// </summary>
        /// <param name="contentEntry">The equipment to draw.</param>
        /// <param name="position">The position to draw the entry at.</param>
        /// <param name="isSelected">If true, this item is selected.</param>
        protected override void DrawEntry(Equipment entry, Vector2 position,
            bool isSelected)
        {
            // check the parameter
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Vector2 drawPosition = position;

            // draw the icon
            spriteBatch.Draw(entry.IconTexture, drawPosition + iconOffset, Color.White);

            // draw the name
            Color color = isSelected ? Fonts.HighlightColor : Fonts.DisplayColor;
            drawPosition.Y += listLineSpacing / 4;
            drawPosition.X += nameColumnInterval;
            spriteBatch.DrawString(Fonts.GearInfoFont, entry.Name, drawPosition, color);

            // draw the power
            drawPosition.X += powerColumnInterval;
            string powerText = entry.GetPowerText();
            Vector2 powerTextSize = Fonts.GearInfoFont.MeasureString(powerText);
            Vector2 powerPosition = drawPosition;
            powerPosition.Y -= (float)Math.Ceiling((powerTextSize.Y - 30f) / 2);
            spriteBatch.DrawString(Fonts.GearInfoFont, powerText,
                powerPosition, color);

            // draw the slot
            drawPosition.X += slotColumnInterval;
            if (entry is Weapon)
            {
                spriteBatch.DrawString(Fonts.GearInfoFont, "Weapon", 
                    drawPosition, color);
            }
            else if (entry is Armor)
            {
                Armor armor = entry as Armor;
                spriteBatch.DrawString(Fonts.GearInfoFont, armor.Slot.ToString(),
                    drawPosition, color);
            }

            // turn on or off the unequip button
            if (isSelected)
            {
                xButtonText = "Unequip";
            }
        }


        /// <summary>
        /// Draw the description of the selected item.
        /// </summary>
        protected override void DrawSelectedDescription(Equipment entry)
        {
            // check the parameter
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Vector2 position = descriptionTextPosition;

            // draw the description
            // -- it's up to the content owner to fit the description
            string text = entry.Description;
            if (!String.IsNullOrEmpty(text))
            {
                spriteBatch.DrawString(Fonts.DescriptionFont, text, position,
                    Fonts.DescriptionColor);
                position.Y += Fonts.DescriptionFont.LineSpacing;
            }

            // draw the modifiers
            text = entry.OwnerBuffStatistics.GetModifierString();
            if (!String.IsNullOrEmpty(text))
            {
                spriteBatch.DrawString(Fonts.DescriptionFont, text, position,
                    Fonts.DescriptionColor);
                position.Y += Fonts.DescriptionFont.LineSpacing;
            }

            // draw the restrictions
            text = entry.GetRestrictionsText();
            if (!String.IsNullOrEmpty(text))
            {
                spriteBatch.DrawString(Fonts.DescriptionFont, text, position,
                    Fonts.DescriptionColor);
                position.Y += Fonts.DescriptionFont.LineSpacing;
            }
        }


        /// <summary>
        /// Draw the column headers above the list.
        /// </summary>
        protected override void DrawColumnHeaders()
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Vector2 position = listEntryStartPosition;

            position.X += nameColumnInterval;
            if (!String.IsNullOrEmpty(nameColumnText))
            {
                spriteBatch.DrawString(Fonts.CaptionFont, nameColumnText, position,
                    Fonts.CaptionColor);
            }

            position.X += powerColumnInterval;
            if (!String.IsNullOrEmpty(powerColumnText))
            {
                spriteBatch.DrawString(Fonts.CaptionFont, powerColumnText, position,
                    Fonts.CaptionColor);
            }

            position.X += slotColumnInterval;
            if (!String.IsNullOrEmpty(slotColumnText))
            {
                spriteBatch.DrawString(Fonts.CaptionFont, slotColumnText, position,
                    Fonts.CaptionColor);
            }
        }


        #endregion
    }
}