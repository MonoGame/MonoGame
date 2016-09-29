using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlockingRun
{
    public class BlockingRunGame : Game
    {
        public BlockingRunGame()
        {
            new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        protected override void LoadContent()
        {
            base.LoadContent();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("SimpleFont");
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, "A magical blocking Game.Run!", Vector2.Zero, Color.White);
            _spriteBatch.End();
        }
    }
}
