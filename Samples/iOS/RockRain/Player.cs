#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace RockRainIphone
{
    /// <summary>
    /// This is a game component that implements the player ship.
    /// </summary>
    public class Player : DrawableGameComponent
    {
        protected Texture2D texture;
        protected Rectangle spriteRectangle;
        protected Vector2 position;
        protected TimeSpan elapsedTime = TimeSpan.Zero;

        // Screen Area
        protected Rectangle screenBounds;

        // Game Stuff
        protected int score;
        protected int power;
        private const int INITIALPOWER = 100;

        public Player(Game game, ref Texture2D theTexture) : base(game)
        {
            texture = theTexture;
            position = new Vector2();

            // Create the source rectangle.
            // This represents where is the sprite picture in surface
            spriteRectangle = new Rectangle(86,11,24,22);

            screenBounds = new Rectangle(0, 0, Game.Window.ClientBounds.Width, 
                Game.Window.ClientBounds.Height);
        }

        /// <summary>
        /// Put the ship in your start position in screen
        /// </summary>
        public void Reset()
        {
            position.X = screenBounds.Width/3;

            position.Y = screenBounds.Height - spriteRectangle.Height;
            score = 0;
            power = INITIALPOWER;
        }

        /// <summary>
        /// Total Points of the Player
        /// </summary>
        public int Score
        {
            get { return score; }
            set
            {
                if (value < 0)
                {
                    score = 0;
                }
                else
                {
                    score = value;
                }
            }
        }

        /// <summary>
        /// Remaining Power
        /// </summary>
        public int Power
        {
            get { return power; }
            set { power = value; }
        }

        /// <summary>
        /// Update the ship position, points and power 
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            HandleInput();
            UpdateShip(gameTime);

            base.Update(gameTime);
        }


        /// <summary>
        /// Get the ship position
        /// </summary>
        protected void HandleInput()
        {
			GamePadState gamepadstatus = GamePad.GetState(PlayerIndex.One);
            // Check the thumbstick
            position.Y += (int)(gamepadstatus.ThumbSticks.Left.Y * -4);
            position.X += (int)(gamepadstatus.ThumbSticks.Left.X * 4);
			
			// Check the accelerometer
			position.Y += (int)(Accelerometer.GetState().Acceleration.Y * -4);
            position.X += (int)(Accelerometer.GetState().Acceleration.X * 4);
        }

        /// <summary>
        /// Update ship status
        /// </summary>
        private void UpdateShip(GameTime gameTime)
        {
            // Keep the player inside the screen
            KeepInBound();

            // Update score
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                score++;
                power--;
            }
        }

        /// <summary>
        /// Keep the ship inside the screen
        /// </summary>
        private void KeepInBound()
        {
            if (position.X < screenBounds.Left)
            {
                position.X = screenBounds.Left;
            }
            if (position.X > screenBounds.Width - spriteRectangle.Width)
            {
                position.X = screenBounds.Width - spriteRectangle.Width;
            }
            if (position.Y < screenBounds.Top)
            {
                position.Y = screenBounds.Top;
            }
            if (position.Y > screenBounds.Height - spriteRectangle.Height)
            {
                position.Y = screenBounds.Height - spriteRectangle.Height;
            }
        }

        /// <summary>
        /// Draw the ship sprite
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Get the current spritebatch
            SpriteBatch sBatch = (SpriteBatch) 
                Game.Services.GetService(typeof (SpriteBatch));

            // Draw the ship
            sBatch.Draw(texture, position, spriteRectangle, Color.White);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Get the bound rectangle of ship position in screen
        /// </summary>
        public Rectangle GetBounds()
        {
            return new Rectangle((int) position.X, (int) position.Y, 
                spriteRectangle.Width, spriteRectangle.Height);
        }
		
		public Vector2 Position
		{
			get
			{
				return position;
			}
		}
    }
}