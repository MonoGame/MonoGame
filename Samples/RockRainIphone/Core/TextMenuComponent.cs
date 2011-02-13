#region Using Statements

using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

#endregion

namespace RockRainIphone.Core
{
    /// <summary>
    /// This is a game component that implements a menu with text elements.
    /// </summary>
    public class TextMenuComponent : DrawableGameComponent
    {
        // Spritebatch
        protected SpriteBatch spriteBatch = null;
        // Fonts
        protected readonly SpriteFont regularFont, selectedFont;
        // Colors
        protected Color regularColor = Color.White, selectedColor = Color.Red;
        // Menu Position
        protected Vector2 position = new Vector2();
        // Items
        protected int selectedIndex = 0;

        private readonly List<string> menuItems;
        // Size of menu in pixels
        protected int width, height;
		// Position of menu itens
		protected List<Vector2> menuItenSize;
		
		public bool Selected {get;set;}

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">the main game object</param>
        /// <param name="normalFont">Font to regular items</param>
        /// <param name="selectedFont">Font to selected item</param>
        public TextMenuComponent(Game game, SpriteFont normalFont, 
            SpriteFont selectedFont) : base(game)
        {
            regularFont = normalFont;
            this.selectedFont = selectedFont;
            menuItems = new List<string>();
			Selected = false;

            // Get the current spritebatch
            spriteBatch = (SpriteBatch) 
                Game.Services.GetService(typeof (SpriteBatch));

        }

        /// <summary>
        /// Set the Menu Options
        /// </summary>
        /// <param name="items"></param>
        public void SetMenuItems(string[] items)
        {
            menuItems.Clear();
            menuItems.AddRange(items);
			CalculateBounds();
        }

        /// <summary>
        /// Width of menu in pixels
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Height of menu in pixels
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// Selected menu item index
        /// </summary>
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { selectedIndex = value; }
        }

        /// <summary>
        /// Regular item color
        /// </summary>
        public Color RegularColor
        {
            get { return regularColor; }
            set { regularColor = value; }
        }

        /// <summary>
        /// Selected item color
        /// </summary>
        public Color SelectedColor
        {
            get { return selectedColor; }
            set { selectedColor = value; }
        }

        /// <summary>
        /// Position of component in screen
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Get the menu bounds
        /// </summary>
        protected void CalculateBounds()
        {
            width = 0;
            height = 0;
			menuItenSize = new List<Vector2>();
            foreach (string item in menuItems)
            {
                Vector2 size = selectedFont.MeasureString(item);
				menuItenSize.Add(size);
                if (size.X > width)
                {
                    width = (int) size.X;
                }
                height += selectedFont.LineSpacing;
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
			float y = position.Y;
			selectedIndex = 0;
            foreach (Vector2 item in menuItenSize)
            {
				Rectangle rect = new Rectangle((int)position.X, (int)y,(int)item.X,(int)item.Y);
				if (rect.Contains(Mouse.GetState().X,Mouse.GetState().Y))
				{
					Selected = true;
					break;
				}
                y += selectedFont.LineSpacing;
				selectedIndex ++;
            }
			if (!Selected)
			{
				selectedIndex = 0;
			}
			
    
            base.Update(gameTime);
        }

        /// <summary>
        /// Allows the game component to draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            float y = position.Y;
            for (int i = 0; i < menuItems.Count; i++)
            {
                SpriteFont font;
                Color theColor;
                if (i == SelectedIndex)
                {
                    font = selectedFont;
                    theColor = selectedColor;
                }
                else
                {
                    font = regularFont;
                    theColor = regularColor;
                }

				
                // Draw the text shadow
                spriteBatch.DrawString(font, menuItems[i], 
                    new Vector2(position.X + 1, y + 1), Color.Black);
                // Draw the text item
                spriteBatch.DrawString(font, menuItems[i], 
                    new Vector2(position.X, y), theColor);
                y += font.LineSpacing;
            }

            base.Draw(gameTime);
        }
    }
}