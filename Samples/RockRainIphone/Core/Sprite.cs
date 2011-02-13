#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace RockRainIphone.Core
{
    /// <summary>
    /// This is a game component that implements a Animated Sprite.
    /// </summary>
    public class Sprite : DrawableGameComponent
    {
        private int activeFrame;
        private readonly Texture2D texture;
        private List<Rectangle> frames;

        protected Vector2 position;
        protected TimeSpan elapsedTime = TimeSpan.Zero;
        protected Rectangle currentFrame;
        protected long frameDelay;
        protected SpriteBatch sbBatch;

        /// <summary>
        /// Default construtor
        /// </summary>
        /// <param name="game">The game object</param>
        /// <param name="theTexture">Texture that contains the sprite frames</param>
        public Sprite(Game game, ref Texture2D theTexture)
            : base(game)
        {
            texture = theTexture;
            activeFrame = 0;
        }

        /// <summary>
        /// List with the frames of the animation
        /// </summary>
        public List<Rectangle> Frames
        {
            get { return frames; }
            set { frames = value; }
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to 
        /// before starting to run.  This is where it can query for any required
        ///  services and load content.
        /// </summary>
        public override void Initialize()
        {
            // Get the current spritebatch
            sbBatch = (SpriteBatch) Game.Services.GetService(typeof (SpriteBatch));

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            // it's time to a next frame?
            if (elapsedTime > TimeSpan.FromMilliseconds(frameDelay))
            {
                elapsedTime -= TimeSpan.FromMilliseconds(frameDelay);
                activeFrame++;
                if (activeFrame == frames.Count)
                {
                    activeFrame = 0;
                }
                // Get the current frame
                currentFrame = frames[activeFrame];
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Draw the sprite.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            sbBatch.Draw(texture, position, currentFrame, Color.White);

            base.Draw(gameTime);
        }
    }
}