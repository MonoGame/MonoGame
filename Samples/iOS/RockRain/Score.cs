#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace RockRainIphone
{
    /// <summary>
    /// This is a game component that implements the Game Score.
    /// </summary>
    public class Score : DrawableGameComponent
    {
        // Spritebatch
        protected SpriteBatch spriteBatch = null;

        // Score Position
        protected Vector2 position = new Vector2();

        // Values
        protected int value;
        protected int power;

        protected readonly SpriteFont font;
        protected readonly Color fontColor;

        public Score(Game game, SpriteFont font, Color fontColor)
            : base(game)
        {
            this.font = font;
            this.fontColor = fontColor;
            // Get the current spritebatch
            spriteBatch = (SpriteBatch) 
                            Game.Services.GetService(typeof (SpriteBatch));
        }

        /// <summary>
        /// Points value
        /// </summary>
        public int Value
        {
            get { return value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Power Value
        /// </summary>
        public int Power
        {
            get { return power; }
            set { power = value; }
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
        /// Allows the game component to draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            string TextToDraw = string.Format("Score: {0}", value);

            // Draw the text shadow
            spriteBatch.DrawString(font, TextToDraw, new Vector2(position.X + 1,
                                    position.Y + 1), Color.Black);
            // Draw the text item
            spriteBatch.DrawString(font, TextToDraw, 
                                    new Vector2(position.X, position.Y), 
                                    fontColor);

            TextToDraw = string.Format("Power: {0}", power);
            // Draw the text shadow
            spriteBatch.DrawString(font, TextToDraw, 
                new Vector2(position.X + 151, position.Y + 1), 
                Color.Black);
            // Draw the text item
            spriteBatch.DrawString(font, TextToDraw, 
                new Vector2(position.X+150, position.Y), 
                fontColor);
			 
            base.Draw(gameTime);
        }
    }
}