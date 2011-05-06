using System;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Samples.SpriteFont
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;		
		Microsoft.Xna.Framework.Graphics.SpriteFont font;
		float rotation;
		Vector2 textSize;
		
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
			
			graphics.IsFullScreen = true;		
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();				
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

			font = Content.Load<Microsoft.Xna.Framework.Graphics.SpriteFont>("SpriteFont1");
			textSize = font.MeasureString("MonoGame");
			textSize = new Vector2(textSize.X/2,textSize.Y/2);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here		
			rotation += 0.1f;
			if (rotation > MathHelper.TwoPi) {
				rotation = 0.0f;
			}
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
           	graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
			
			spriteBatch.Begin();
			
			spriteBatch.DrawString(font,"MonoGame",new Vector2(101,99),Color.Black);
			spriteBatch.DrawString(font,"MonoGame",new Vector2(101,101),Color.Black);
			spriteBatch.DrawString(font,"MonoGame",new Vector2(99,99),Color.Black);
			spriteBatch.DrawString(font,"MonoGame",new Vector2(99,101),Color.Black);
			spriteBatch.DrawString(font,"MonoGame",new Vector2(100,100),Color.White);
            spriteBatch.DrawString(font,"MonoGame", new Vector2(100, 100),Color.Yellow, MathHelper.PiOver2, Vector2.Zero, 1.0f,SpriteEffects.None, 1);
            spriteBatch.DrawString(font,"MonoGame", new Vector2(100, 100), Color.Yellow, MathHelper.PiOver4, Vector2.Zero, 1.0f, SpriteEffects.None, 1);
			spriteBatch.DrawString(font,"MonoGame", new Vector2(160, 340),Color.Red, rotation, textSize, 1.0f,SpriteEffects.None, 1);
			
			spriteBatch.End();
			
            base.Draw(gameTime);
        }
    }
}
