#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using RockRainIphone.Core;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;

#endregion

namespace RockRainIphone
{
    /// <summary>
    /// This is a game component that implements the Game Start Scene.
    /// </summary>
    public class StartScene : GameScene
    {
        // Misc
        protected TextMenuComponent menu;
        protected readonly Texture2D elements;
        // Audio Bank
        protected AudioLibrary audio;
        // Spritebatch
        protected SpriteBatch spriteBatch = null;
        // Gui Stuff
        protected Rectangle rockRect = new Rectangle(3, 2, 197, 49);
        protected Vector2 rockPosition;
        protected Rectangle rainRect = new Rectangle(47, 63, 189, 48);
        protected Vector2 rainPosition;
        protected Rectangle enhancedRect = new Rectangle(7, 110, 122, 46);
        protected Vector2 enhancedPosition;
        protected bool showEnhanced;
        protected TimeSpan elapsedTime = TimeSpan.Zero;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="game">Main game object</param>
        /// <param name="smallFont">Font for the menu items</param>
        /// <param name="largeFont">Font for the menu selcted item</param>
        /// <param name="background">Texture for background image</param>
        /// <param name="elements">Texture with the foreground elements</param>
        public StartScene(Game game, SpriteFont smallFont, SpriteFont largeFont,
                            Texture2D background,Texture2D elements)
            : base(game)
        {
            this.elements = elements;
            Components.Add(new ImageComponent(game, background, 
                                            ImageComponent.DrawMode.Stretch));

            // Create the Menu
            string[] items = {"Play!", "Help", "Quit"};
            menu = new TextMenuComponent(game, smallFont, largeFont);
            menu.SetMenuItems(items);
            Components.Add(menu);

            // Get the current spritebatch
            spriteBatch = (SpriteBatch) Game.Services.GetService(typeof (SpriteBatch));
            // Get the current audiocomponent and play the background music
            audio = (AudioLibrary)Game.Services.GetService(typeof(AudioLibrary));
        }

        /// <summary>
        /// Show the start scene
        /// </summary>
        public override void Show()
        {
			GamePad.Visible = false;
			
            audio.NewMeteor.Play();            

            rockPosition.X = -1*rockRect.Width;
            rockPosition.Y = 20;
            rainPosition.X = Game.Window.ClientBounds.Width;
            rainPosition.Y = 70;
            // Put the menu centered in screen
            menu.Position = new Vector2((Game.Window.ClientBounds.Width - 
                                          menu.Width)/2, 200);

            // These elements will be visible when the 'Rock Rain' title
            // is done.
            menu.Visible = false;
            menu.Enabled = false;
			menu.Selected = false;
            showEnhanced = false;

            base.Show();
        }

        /// <summary>
        /// Hide the start scene
        /// </summary>
        public override void Hide()
        {            
            MediaPlayer.Stop();
            base.Hide();
        }

        /// <summary>
        /// Gets the selected menu option
        /// </summary>
        public int SelectedMenuIndex
        {
            get { return menu.SelectedIndex; }
        }
		
		public bool MenuSelected
        {
            get 
			{
				return menu.Selected; 
			}
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (!menu.Visible)
            {
                if (rainPosition.X >= (Game.Window.ClientBounds.Width - rainRect.Width)/2)
                {
                    rainPosition.X -= 5;
                }

                if (rockPosition.X <= (Game.Window.ClientBounds.Width - rockRect.Width)/2)
                {
                    rockPosition.X += 5;
                }
                else
                {
                    menu.Visible = true;
                    menu.Enabled = true;

                    MediaPlayer.Play(audio.StartMusic);

                    enhancedPosition =
                        new Vector2((rainPosition.X + rainRect.Width - 
                        enhancedRect.Width/2) - 40, rainPosition.Y+20);
                    showEnhanced = true;
                }
            }
            else
            {
                elapsedTime += gameTime.ElapsedGameTime;

                if (elapsedTime > TimeSpan.FromSeconds(1))
                {
                    elapsedTime -= TimeSpan.FromSeconds(1);
                    showEnhanced = !showEnhanced;
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Allows the game component to draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            spriteBatch.Draw(elements, rockPosition, rockRect, Color.White);
            spriteBatch.Draw(elements, rainPosition, rainRect, Color.White);
            if (showEnhanced)
            {
                spriteBatch.Draw(elements, enhancedPosition, enhancedRect,Color.White);
            }
        }
    }
}