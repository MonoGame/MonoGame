#region File Description
//-----------------------------------------------------------------------------
// StoreSellScreen.cs
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
    /// Displays the gear in the party inventory and allows the user to sell them.
    /// </summary>
    class StoreSellScreen : InventoryScreen
    {
        #region Graphics Data


        /// <summary>
        /// The left-facing quantity arrow.
        /// </summary>
        private Texture2D leftQuantityArrow;

        /// <summary>
        /// The right-facing quantity arrow.
        /// </summary>
        private Texture2D rightQuantityArrow;


        #endregion


        #region Columns


        private const int nameColumnInterval = 80;
        private const int powerColumnInterval = 270;
        private const int quantityColumnInterval = 340;

        private string priceColumnText = "Price";
        private const int priceColumnInterval = 120;


        #endregion


        #region Data Access


        /// <summary>
        /// The store whose goods are being displayed.
        /// </summary>
        private Store store;


        /// <summary>
        /// The selected quantity of the current entry.
        /// </summary>
        private int selectedQuantity = 0;


        /// <summary>
        /// Resets the selected quantity to the maximum value for the selected entry.
        /// </summary>
        private void ResetQuantities()
        {
            selectedQuantity = 1;
        }

        
        #endregion


        #region List Navigation


        /// <summary>
        /// Move the current selection up one entry.
        /// </summary>
        protected override void MoveCursorUp()
        {
            base.MoveCursorUp();
            ResetQuantities();
        }


        /// <summary>
        /// Move the current selection down one entry.
        /// </summary>
        protected override void MoveCursorDown()
        {
            base.MoveCursorDown();
            ResetQuantities();
        }


        /// <summary>
        /// Decrease the selected quantity by one.
        /// </summary>
        protected override void MoveCursorLeft()
        {
            ReadOnlyCollection<ContentEntry<Gear>> entries = GetDataList();
            if (entries.Count > 0)
            {
                // loop to one if the selected quantity is already at maximum.
                if (selectedQuantity > 1)
                {
                    selectedQuantity--;
                }
                else
                {
                    selectedQuantity = entries[SelectedIndex].Count;
                }
            }
            else
            {
                selectedQuantity = 0;
            }
        }


        /// <summary>
        /// Increase the selected quantity by one.
        /// </summary>
        protected override void MoveCursorRight()
        {
            ReadOnlyCollection<ContentEntry<Gear>> entries = GetDataList();
            if (entries.Count > 0)
            {
                // loop to one if the selected quantity is already at maximum.
                selectedQuantity = selectedQuantity < entries[SelectedIndex].Count ?
                    selectedQuantity + 1 : 1;
            }
            else
            {
                selectedQuantity = 0;
            }
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Creates a new StoreSellScreen object for the given store.
        /// </summary>
        public StoreSellScreen(Store store)
            : base(true)
        {
            // check the parameter
            if ((store == null) || (store.StoreCategories.Count <= 0))
            {
                throw new ArgumentNullException("store");
            }
            this.store = store;

            // configure the menu text
            selectButtonText = "Sell";
            backButtonText = "Back";
            xButtonText = String.Empty;
            yButtonText = String.Empty;

            ResetQuantities();
        }

        
        /// <summary>
        /// Load the graphics content from the content manager.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();

            ContentManager content = ScreenManager.Game.Content;

            leftQuantityArrow =
                content.Load<Texture2D>(@"Textures\Buttons\QuantityArrowLeft");
            rightQuantityArrow =
                content.Load<Texture2D>(@"Textures\Buttons\QuantityArrowRight");
        }


        #endregion


        #region Input Handling


        /// <summary>
        /// Respond to the triggering of the Select action.
        /// </summary>
        protected override void SelectTriggered(ContentEntry<Gear> entry) 
        {
            // check the parameter
            if ((entry == null) || (entry.Content == null))
            {
                return;
            }

            // make sure the selected quantity is valid
            selectedQuantity = Math.Min(selectedQuantity, entry.Count);

            // add the gold to the party's inventory
            Session.Party.PartyGold += selectedQuantity *
                (int)Math.Ceiling(entry.Content.GoldValue * store.SellMultiplier);

            // remove the items from the party's inventory
            Session.Party.RemoveFromInventory(entry.Content, selectedQuantity);

            // reset the quantities - either gold has gone down or the total was bad
            ResetQuantities();
        }


        /// <summary>
        /// Switch to the previous store category.
        /// </summary>
        protected override void PageScreenLeft() 
        {
            isItems = !isItems;
            ResetTriggerText();
            ResetQuantities();
        }


        /// <summary>
        /// Switch to the next store category.
        /// </summary>
        protected override void PageScreenRight() 
        {
            isItems = !isItems;
            ResetTriggerText();
            ResetQuantities();
        }


        /// <summary>
        /// Reset the trigger button text to the names of the 
        /// previous and next UI screens.
        /// </summary>
        protected override void ResetTriggerText()
        {
            leftTriggerText = rightTriggerText = isItems ? "Equipment" : "Items";
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw the entry at the given position in the list.
        /// </summary>
        /// <param name="contentEntry">The entry to draw.</param>
        /// <param name="position">The position to draw the entry at.</param>
        /// <param name="isSelected">If true, this item is selected.</param>
        protected override void DrawEntry(ContentEntry<Gear> entry, Vector2 position,
            bool isSelected)
        {
            // check the parameter
            if ((entry == null) || (entry.Content == null))
            {
                throw new ArgumentNullException("entry");
            }

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Vector2 drawPosition = position;

            // draw the icon
            spriteBatch.Draw(entry.Content.IconTexture, drawPosition + iconOffset, 
                Color.White);

            // draw the name
            Color color = isSelected ? Fonts.HighlightColor : Fonts.DisplayColor;
            drawPosition.Y += listLineSpacing / 4;
            drawPosition.X += nameColumnInterval;
            spriteBatch.DrawString(Fonts.GearInfoFont, entry.Content.Name, 
                drawPosition, color);

            // draw the power
            drawPosition.X += powerColumnInterval;
            string powerText = entry.Content.GetPowerText();
            Vector2 powerTextSize = Fonts.GearInfoFont.MeasureString(powerText);
            Vector2 powerPosition = drawPosition;
            powerPosition.Y -= (float)Math.Ceiling((powerTextSize.Y - 30f) / 2);
            spriteBatch.DrawString(Fonts.GearInfoFont, powerText,
                powerPosition, color);

            // draw the quantity
            drawPosition.X += quantityColumnInterval;
            if (isSelected)
            {
                Vector2 quantityPosition = drawPosition;
                // draw the left selection arrow
                quantityPosition.X -= leftQuantityArrow.Width;
                spriteBatch.Draw(leftQuantityArrow,
                    new Vector2(quantityPosition.X, quantityPosition.Y - 4),
                    Color.White);
                quantityPosition.X += leftQuantityArrow.Width;
                // draw the selected quantity ratio
                string quantityText = selectedQuantity.ToString() + "/" +
                    entry.Count.ToString();
                spriteBatch.DrawString(Fonts.GearInfoFont, quantityText,
                    quantityPosition, color);
                quantityPosition.X +=
                    Fonts.GearInfoFont.MeasureString(quantityText).X;
                // draw the right selection arrow
                spriteBatch.Draw(rightQuantityArrow,
                    new Vector2(quantityPosition.X, quantityPosition.Y - 4),
                    Color.White);
                quantityPosition.X += rightQuantityArrow.Width;
                // draw the purchase button
                selectButtonText = "Sell";
            }
            else
            {
                spriteBatch.DrawString(Fonts.GearInfoFont, entry.Count.ToString(),
                    drawPosition, color);
            }

            // draw the price
            drawPosition.X += priceColumnInterval;
            string priceText = String.Empty;
            if (isSelected)
            {
                int totalPrice = selectedQuantity *
                    (int)Math.Ceiling(entry.Content.GoldValue * store.SellMultiplier);
                priceText = totalPrice.ToString();
            }
            else
            {
                priceText = ((int)Math.Ceiling(entry.Content.GoldValue * 
                    store.SellMultiplier)).ToString();
            }
            spriteBatch.DrawString(Fonts.GearInfoFont, priceText,
                drawPosition, color);
        }


        /// <summary>
        /// Draw the description of the selected item.
        /// </summary>
        protected override void DrawSelectedDescription(ContentEntry<Gear> entry)
        {
            // check the parameter
            if ((entry == null) || (entry.Content == null))
            {
                throw new ArgumentNullException("entry");
            }

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Vector2 position = descriptionTextPosition;

            // draw the description
            // -- it's up to the content owner to fit the description
            string text = entry.Content.Description;
            if (!String.IsNullOrEmpty(text))
            {
                spriteBatch.DrawString(Fonts.DescriptionFont, text, position,
                    Fonts.DescriptionColor);
                position.Y += Fonts.DescriptionFont.LineSpacing;
            }

            // draw the modifiers
            Equipment equipment = entry.Content as Equipment;
            if (equipment != null)
            {
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

            position.X += quantityColumnInterval;
            if (!String.IsNullOrEmpty(quantityColumnText))
            {
                spriteBatch.DrawString(Fonts.CaptionFont, quantityColumnText, position,
                    Fonts.CaptionColor);
            }

            position.X += priceColumnInterval;
            if (!String.IsNullOrEmpty(priceColumnText))
            {
                spriteBatch.DrawString(Fonts.CaptionFont, priceColumnText, position,
                    Fonts.CaptionColor);
            }
        }


        #endregion
    }
}
