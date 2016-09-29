using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Issue1355 {
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager Graphics;
        SpriteBatch SpriteBatch;
        Texture2D White;
        SpriteFont Font;

        public Game () {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.PreferredBackBufferWidth = 700;
            Graphics.PreferredBackBufferHeight = 200;

            Content.RootDirectory = "Content";

#if MONOGAME
            Window.Title = "Issue1355 (MonoGame)";
#else
            Window.Title = "Issue1355 (XNA)";
#endif
        }

        protected override void LoadContent () {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            White = new Texture2D(GraphicsDevice, 1, 1);
            White.SetData(new[] { Color.White });

            Font = Content.Load<SpriteFont>("DutchAndHarley");
        }

        protected override void Draw (GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            const string testText = "{Dutch & Harley}";

            SpriteBatch.Begin();

            var textSize = Font.MeasureString(testText);
            var textPosition = new Vector2(32, 32);

            SpriteBatch.Draw(White, textPosition, null, Color.Red * 0.25f, 0, Vector2.Zero, textSize, SpriteEffects.None, 0);

            SpriteBatch.DrawString(Font, testText, textPosition, Color.White);

            SpriteBatch.End();

            base.Draw(gameTime);
        }

        public static void Main () {
            new Game().Run();
        }
    }
}
