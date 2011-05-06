#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace RockRainIphone.Core
{
    /// <summary>
    /// This is a game component that draw a image.
    /// </summary>
    public class ImageComponent : DrawableGameComponent
    {
        public enum DrawMode
        {
            Center = 1,
            Stretch,
        } ;

        // Texture to draw
        protected readonly Texture2D texture;
        // Draw Mode
        protected readonly DrawMode drawMode;
        // Spritebatch
        protected SpriteBatch spriteBatch = null;
        // Image Rectangle
        protected Rectangle imageRect;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game object</param>
        /// <param name="texture">Texture to Draw</param>
        /// <param name="drawMode">Draw Mode</param>
        public ImageComponent(Game game, Texture2D texture, DrawMode drawMode)
            : base(game)
        {
            this.texture = texture;
            this.drawMode = drawMode;
            // Get the current spritebatch
            spriteBatch = (SpriteBatch) 
                Game.Services.GetService(typeof (SpriteBatch));

            // Create a rectangle with the size and position of the image
            switch (drawMode)
            {
                case DrawMode.Center:
                    imageRect = new Rectangle((Game.Window.ClientBounds.Width - 
                        texture.Width)/2,(Game.Window.ClientBounds.Height - 
                        texture.Height)/2,texture.Width, texture.Height);
                    break;
                case DrawMode.Stretch:
                    imageRect = new Rectangle(0, 0, Game.Window.ClientBounds.Width, 
                        Game.Window.ClientBounds.Height);
                    break;
            }
        }

        /// <summary>
        /// Allows the game component to draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(texture, imageRect, Color.White);
            base.Draw(gameTime);
        }
    }
}