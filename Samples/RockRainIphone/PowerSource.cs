#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RockRainIphone.Core;

#endregion

namespace RockRainIphone
{
    /// <summary>
    /// This is a game component that implements Power Source Element.
    /// </summary>
    public class PowerSource : Sprite
    {
        protected Texture2D texture;
        protected Random random;

        public PowerSource(Game game, ref Texture2D theTexture)
            : base(game, ref theTexture)
        {
            texture = theTexture;

            Frames = new List<Rectangle>();
            Rectangle frame = new Rectangle();
            frame.X = 55;
            frame.Y = 15;
            frame.Width = 14;
            frame.Height = 12;
            Frames.Add(frame);

            frame.Y = 29;
            Frames.Add(frame);

            frame.Y = 42;
            Frames.Add(frame);

            frame.Y = 56;
            Frames.Add(frame);

            frame.Y = 69;
            Frames.Add(frame);

            frame.Y = 81;
            Frames.Add(frame);

            frameDelay = 200;

            // Initialize the random number generator and put the power source in your
            // start position
            random = new Random(GetHashCode());
            PutinStartPosition();
        }

        /// <summary>
        /// Initialize Position and Velocity
        /// </summary>
        public void PutinStartPosition()
        {
            position.X = random.Next(Game.Window.ClientBounds.Width - 
                currentFrame.Width);
            position.Y = -10;
            Enabled = false;
        }

        public override void Update(GameTime gameTime)
        {
            // Check if the still visible
            if (position.Y >= Game.Window.ClientBounds.Height)
            {
                PutinStartPosition();
            }

            // Move
            position.Y += 1;

            base.Update(gameTime);
        }

        /// <summary>
        /// Check if the object intersects with the specified rectangle
        /// </summary>
        /// <param name="rect">test rectangle</param>
        /// <returns>true, if has a collision</returns>
        public bool CheckCollision(Rectangle rect)
        {
            Rectangle spriterect =
                new Rectangle((int) position.X, (int) position.Y, 
                currentFrame.Width, currentFrame.Height);
            return spriterect.Intersects(rect);
        }
    }
}