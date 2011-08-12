#region File Description
//-----------------------------------------------------------------------------
// StoreBuyScreen.cs
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
    /// Displays the gear from a particular store and allows the user to purchase them.
    /// </summary>
    class StoreBuyScreen : ListScreen<Gear>
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


        private string nameColumnText = "Name";
        private const int nameColumnInterval = 80;

        private string powerColumnText = "Power (min, max)";
        private const int powerColumnInterval = 270;

        private string quantityColumnText = "Qty";
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
        /// The index of the current StoreCategory.
        /// </summary>
        private int currentCategoryIndex = 0;


        /// <summary>
        /// Get the list that this screen displays.
        /// </summary>
        public override ReadOnlyCollection<Gear> GetDataList()
        {
            return 
                store.StoreCategories[currentCategoryIndex].AvailableGear.AsReadOnly();
        }


        /// <summary>
        /// The selected quantity of the current entry.
        /// </summary>
        private int selectedQuantity = 0;


        /// <summary>
        /// The maximum quantity of the current entry.
        /// </summary>
        private int maximumQuantity = 0;


        /// <summary>
        /// Resets the selected quantity to the maximum value for the selected entry.
        /// </summary>
        private void ResetQuantities()
        {
            // check the indices before recalculating
            if ((currentCategoryIndex < 0) ||
                (currentCategoryIndex > store.StoreCategories.Count) ||
                (SelectedIndex < 0) || (SelectedIndex >=
                store.StoreCategories[currentCategoryIndex].AvailableGear.Count))
            {
                return;
            }

            // get the value of the selected gear
            Gear gear = 
                store.StoreCategories[currentCategoryIndex].AvailableGear[SelectedIndex];
            if ((gear == null) || (gear.GoldValue <= 0))
            {
                selectedQuantity = maximumQuantity = 0;
            }

            selectedQuantity = 1;
            maximumQuantity = (int)Math.Floor(Session.Party.PartyGold / 
                (gear.GoldValue * store.BuyMultiplier));
        }

        
        #endregion


        #region List Navigation


        /// <summary>
        /// Move the current selection up one entry.
        /// </summary>
        protected override void MoveCursorUp()
        {
            int oldIndex = SelectedIndex;
            base.MoveCursorUp();
            if (SelectedIndex != oldIndex)
            {
                ResetQuantities();
            }
        }


        /// <summary>
        /// Move the current selection down one entry.
        /// </summary>
        protected override void MoveCursorDown()
        {
            int oldIndex = SelectedIndex;
            base.MoveCursorDown();
            if (SelectedIndex != oldIndex)
            {
                ResetQuantities();
            }
        }


        /// <summary>
        /// Decrease the selected quantity by one.
        /// </summary>
        protected override void MoveCursorLeft()
        {
            if (maximumQuantity > 0)
            {
                // decrement the quantity, looping around if necessary
                selectedQuantity = (selectedQuantity > 1) ?
                    selectedQuantity - 1 : maximumQuantity;
            }
        }


        /// <summary>
        /// Increase the selected quantity by one.
        /// </summary>
        protected override void MoveCursorRight()
        {
            if (maximumQuantity > 0)
            {
                // loop to one if the selected quantity is already at maximum.
                selectedQuantity = selectedQuantity < maximumQuantity ?
                    selectedQuantity + 1 : 1;
            }
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Creates a new StoreBuyScreen object for the given store.
        /// </summary>
        public StoreBuyScreen(Store store)
            : base()
        {
            // check the parameter
            if ((store == null) || (store.StoreCategories.Count <= 0))
            {
                throw new ArgumentNullException("store");
            }
            this.store = store;

            // configure the menu text
            selectButtonText = "Purchase";
            backButtonText = "Back";
            xButtonText = String.Empty;
            yButtonText = String.Empty;
            ResetMenu();

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
        protected override void SelectTriggered(Gear entry) 
        {
            if (entry == null)
            {
                return;
            }

            // purchase the items if possible
            int totalPrice = (int)Math.Floor(entry.GoldValue * selectedQuantity * 
                store.BuyMultiplier);
            if (totalPrice <= Session.Party.PartyGold)
            {
                Session.Party.PartyGold -= totalPrice;
                Session.Party.AddToInventory(entry, selectedQuantity);
            }

            // reset the quantities - either gold has gone down or the total was bad
            ResetQuantities();
        }


        /// <summary>
        /// Switch to the previous store category.
        /// </summary>
        protected override void PageScreenLeft() 
        {
            currentCategoryIndex--;
            if (currentCategoryIndex < 0)
            {
                currentCategoryIndex = store.StoreCategories.Count - 1;
            }
            ResetMenu();
            ResetQuantities();
        }


        /// <summary>
        /// Switch to the next store category.
        /// </summary>
        protected override void PageScreenRight() 
        {
            currentCategoryIndex++;
            if (currentCategoryIndex >= store.StoreCategories.Count)
            {
                currentCategoryIndex = 0;
            }
            ResetMenu();
            ResetQuantities();
        }


        /// <summary>
        /// Reset the menu title and trigger button text for the current store category.
        /// </summary>
        private void ResetMenu()
        {
            // update the title the title
            titleText = store.StoreCategories[currentCategoryIndex].Name;

            // get the left trigger text
            int index = currentCategoryIndex - 1;
            if (index < 0)
            {
                index = store.StoreCategories.Count - 1;
            }
            leftTriggerText = store.StoreCategories[index].Name;

            // get the right trigger text
            index = currentCategoryIndex + 1;
            if (index >= store.StoreCategories.Count)
            {
                index = 0;
            }
            rightTriggerText = store.StoreCategories[index].Name;
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw the entry at the given position in the list.
        /// </summary>
        /// <param name="contentEntry">The entry to draw.</param>
        /// <param name="position">The position to draw the entry at.</param>
        /// <param name="isSelected">If true, this item is selected.</param>
        protected override void DrawEntry(Gear entry, Vector2 position,
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

            // draw the quantity
            drawPosition.X += quantityColumnInterval;
            int priceSingle = (int)Math.Floor(entry.GoldValue * store.BuyMultiplier);
            if (isSelected)
            {
                if (priceSingle <= Session.Party.PartyGold)
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
                        maximumQuantity.ToString();
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
                    selectButtonText = "Purchase";
                }
                else
                {
                    // turn off the purchase button
                    selectButtonText = String.Empty;
                }
            }

            // draw the price
            drawPosition.X += priceColumnInterval;
            string priceText = String.Empty;
            if (isSelected)
            {
                int totalPrice = (int)Math.Floor(entry.GoldValue * store.BuyMultiplier) *
                    selectedQuantity;
                priceText = totalPrice.ToString();
            }
            else
            {
                priceText = ((int)Math.Floor(entry.GoldValue * 
                    store.BuyMultiplier)).ToString();
            }
            spriteBatch.DrawString(Fonts.GearInfoFont, priceText,
                drawPosition, color);
        }


        /// <summary>
        /// Draw the description of the selected item.
        /// </summary>
        protected override void DrawSelectedDescription(Gear entry)
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
            Equipment equipment = entry as Equipment;
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