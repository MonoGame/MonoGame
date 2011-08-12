#region File Description
//-----------------------------------------------------------------------------
// InventoryScreen.cs
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
    /// Displays the inventory of the party, either showing items or equipment.
    /// </summary>
    class InventoryScreen : ListScreen<ContentEntry<Gear>>
    {
        #region Columns


        protected string nameColumnText = "Name";
        private const int nameColumnInterval = 80;

        protected string powerColumnText = "Power (min, max)";
        private const int powerColumnInterval = 270;

        protected string quantityColumnText = "Qty";
        private const int quantityColumnInterval = 450;


        #endregion


        #region Data Access


        /// <summary>
        /// If true, the menu is only displaying items; otherwise, only equipment.
        /// </summary>
        protected bool isItems;


        /// <summary>
        /// Retrieve the list of gear shown in this menu.
        /// </summary>
        /// <returns></returns>
        public override ReadOnlyCollection<ContentEntry<Gear>> GetDataList()
        {
            List<ContentEntry<Gear>> dataList = new List<ContentEntry<Gear>>();
            ReadOnlyCollection<ContentEntry<Gear>> inventory = Session.Party.Inventory;

            // build a new list of only the desired gear
            foreach (ContentEntry<Gear> gearEntry in inventory)
            {
                if (isItems)
                {
                    if (gearEntry.Content is Item)
                    {
                        dataList.Add(gearEntry);
                    }
                }
                else
                {
                    if (gearEntry.Content is Equipment)
                    {
                        dataList.Add(gearEntry);
                    }
                }
            }

            // sort the list by name
            dataList.Sort(
                delegate(ContentEntry<Gear> gearEntry1, ContentEntry<Gear> gearEntry2)
                {
                    // handle null values
                    if ((gearEntry1 == null) || (gearEntry1.Content == null))
                    {
                        return ((gearEntry2 == null) || (gearEntry2.Content == null) ?
                            0 : 1);
                    }
                    else if ((gearEntry2 == null) || (gearEntry2.Content == null))
                    {
                        return -1;
                    }

                    // sort by name
                    return gearEntry1.Content.Name.CompareTo(gearEntry2.Content.Name);
                });

            return dataList.AsReadOnly();
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new InventoryScreen object.
        /// </summary>
        public InventoryScreen(bool isItems)
            : base()
        {
            this.isItems = isItems;

            // configure the menu text
            titleText = "Inventory";
            selectButtonText = "Select";
            backButtonText = "Back";
            xButtonText = "Drop";
            yButtonText = String.Empty;
            ResetTriggerText();
        }


        #endregion


        #region Input Handling


        /// <summary>
        /// Delegate for item-selection events.
        /// </summary>
        public delegate void GearSelectedHandler(Gear gear);


        /// <summary>
        /// Responds when an item is selected by this menu.
        /// </summary>
        /// <remarks>
        /// Typically used by the calling menu, like the combat HUD menu, 
        /// to respond to selection.
        /// </remarks>
        public event GearSelectedHandler GearSelected;


        /// <summary>
        /// Respond to the triggering of the Select action (and related key).
        /// </summary>
        protected override void SelectTriggered(ContentEntry<Gear> entry)
        {
            // check the parameter
            if ((entry == null) || (entry.Content == null))
            {
                return;
            }

            // if the event is valid, fire it and exit this screen
            if (GearSelected != null)
            {
                GearSelected(entry.Content);
                ExitScreen();
                return;
            }

            // otherwise, open the selection screen over this screen
            ScreenManager.AddScreen(new PlayerSelectionScreen(entry.Content));
        }


        /// <summary>
        /// Respond to the triggering of the X button (and related key).
        /// </summary>
        protected override void ButtonXPressed(ContentEntry<Gear> entry)
        {
            // check the parameter
            if ((entry == null) || (entry.Content == null))
            {
                return;
            }

            // check whether the gear could be dropped
            if (!entry.Content.IsDroppable)
            {
                return;
            }

            // add a message box confirming the drop
            MessageBoxScreen dropEquipmentConfirmationScreen =
                new MessageBoxScreen("Are you sure you want to drop the " +
                entry.Content.Name + "?");
            dropEquipmentConfirmationScreen.Accepted +=
                new EventHandler<EventArgs>(delegate(object sender, EventArgs args)
                {
                    Session.Party.RemoveFromInventory(entry.Content, 1);
                });
            ScreenManager.AddScreen(dropEquipmentConfirmationScreen);
        }


        /// <summary>
        /// Switch to the screen to the "left" of this one in the UI.
        /// </summary>
        protected override void PageScreenLeft()
        {
            if (CombatEngine.IsActive)
            {
                return;
            }

            if (isItems)
            {
                ExitScreen();
                ScreenManager.AddScreen(new StatisticsScreen(Session.Party.Players[0]));
            }
            else
            {
                isItems = !isItems;
                ResetTriggerText();
            }
        }


        /// <summary>
        /// Switch to the screen to the "right" of this one in the UI.
        /// </summary>
        protected override void PageScreenRight()
        {
            if (CombatEngine.IsActive)
            {
                return;
            }

            if (isItems)
            {
                isItems = !isItems;
                ResetTriggerText();
            }
            else
            {
                ExitScreen();
                ScreenManager.AddScreen(new QuestLogScreen(null));
            }
        }

        
        /// <summary>
        /// Reset the trigger button text to the names of the 
        /// previous and next UI screens.
        /// </summary>
        protected virtual void ResetTriggerText()
        {
            if (CombatEngine.IsActive)
            {
                leftTriggerText = rightTriggerText = String.Empty;
            }
            else
            {
                if (isItems)
                {
                    leftTriggerText = "Statistics";
                    rightTriggerText = "Equipment";
                }
                else
                {
                    leftTriggerText = "Items";
                    rightTriggerText = "Quests";
                }
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw the gear's content entry at the given position in the list.
        /// </summary>
        /// <param name="contentEntry">The content entry to draw.</param>
        /// <param name="position">The position to draw the entry at.</param>
        /// <param name="isSelected">If true, this item is selected.</param>
        protected override void DrawEntry(ContentEntry<Gear> entry, Vector2 position,
            bool isSelected)
        {
            // check the parameter
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            Gear gear = entry.Content as Gear;
            if (gear == null)
            {
                return;
            }

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Vector2 drawPosition = position;

            // draw the icon
            spriteBatch.Draw(gear.IconTexture, drawPosition + iconOffset, Color.White);

            // draw the name
            Color color = isSelected ? Fonts.HighlightColor : Fonts.DisplayColor;
            drawPosition.Y += listLineSpacing / 4;
            drawPosition.X += nameColumnInterval;
            spriteBatch.DrawString(Fonts.GearInfoFont, gear.Name, drawPosition, color);

            // draw the power
            drawPosition.X += powerColumnInterval;
            string powerText = gear.GetPowerText();
            Vector2 powerTextSize = Fonts.GearInfoFont.MeasureString(powerText);
            Vector2 powerPosition = drawPosition;
            powerPosition.Y -= (float)Math.Ceiling((powerTextSize.Y - 30f) / 2);
            spriteBatch.DrawString(Fonts.GearInfoFont, powerText,
                powerPosition, color);

            // draw the quantity
            drawPosition.X += quantityColumnInterval;
            spriteBatch.DrawString(Fonts.GearInfoFont, entry.Count.ToString(),
                drawPosition, color);

            // turn on or off the select and drop buttons
            if (isSelected)
            {
                selectButtonText = "Select";
                xButtonText = entry.Content.IsDroppable ? "Drop" : String.Empty;
            }
        }


        /// <summary>
        /// Draw the description of the selected item.
        /// </summary>
        protected override void DrawSelectedDescription(ContentEntry<Gear> entry)
        {
            // check the parameter
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            Gear gear = entry.Content as Gear;
            if (gear == null)
            {
                return;
            }

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Vector2 position = descriptionTextPosition;

            // draw the description
            // -- it's up to the content owner to fit the description
            string text = gear.Description;
            if (!String.IsNullOrEmpty(text))
            {
                spriteBatch.DrawString(Fonts.DescriptionFont, text, position,
                    Fonts.DescriptionColor);
                position.Y += Fonts.DescriptionFont.LineSpacing;
            }

            // draw additional information for equipment
            Equipment equipment = entry.Content as Equipment;
            if (equipment != null)
            {
                // draw the modifiers
                text = equipment.OwnerBuffStatistics.GetModifierString();
                if (!String.IsNullOrEmpty(text))
                {
                    spriteBatch.DrawString(Fonts.DescriptionFont, text, position,
                        Fonts.DescriptionColor);
                    position.Y += Fonts.DescriptionFont.LineSpacing;
                }
            }

            // draw the restrictions
            text = entry.Content.GetRestrictionsText();
            if (!String.IsNullOrEmpty(text))
            {
                spriteBatch.DrawString(Fonts.DescriptionFont, text, position,
                    Fonts.DescriptionColor);
                position.Y += Fonts.DescriptionFont.LineSpacing;
            }
        }


        /// <summary>
        /// Draw the column headers above the gear list.
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

            position.X += quantityColumnInterval;
            if (!String.IsNullOrEmpty(quantityColumnText))
            {
                spriteBatch.DrawString(Fonts.CaptionFont, quantityColumnText, position,
                    Fonts.CaptionColor);
            }
        }


        #endregion
    }
}