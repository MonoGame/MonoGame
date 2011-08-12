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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace GooCursor
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        Cursor cursor;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            cursor = new Cursor(this,10);
            cursor.BorderColor = Color.White;
            cursor.FillColor = Color.Black;
            Components.Add(cursor);
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
            spriteFont = Content.Load<SpriteFont>("SimpleFont");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            KeyboardState state = Keyboard.GetState();

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (state.IsKeyDown(Keys.Q))
                cursor.StartScale += 1.0f * elapsed;
            if (state.IsKeyDown(Keys.A))
                cursor.StartScale -= 1.0f * elapsed;


            if (state.IsKeyDown(Keys.W))
                cursor.EndScale += 1.0f * elapsed;
            if (state.IsKeyDown(Keys.S))
                cursor.EndScale -= 1.0f * elapsed;


            if (state.IsKeyDown(Keys.E))
                cursor.LerpExponent += 1.0f * elapsed;
            if (state.IsKeyDown(Keys.D))
                cursor.LerpExponent -= 1.0f * elapsed;

            if (state.IsKeyDown(Keys.R))
                cursor.BorderSize += 10.0f * elapsed;
            if (state.IsKeyDown(Keys.F))
                cursor.BorderSize -= 10.0f * elapsed;


            if (state.IsKeyDown(Keys.T))
                cursor.TrailStiffness += 1000.0f * elapsed;
            if (state.IsKeyDown(Keys.G))
                cursor.TrailStiffness -= 1000.0f * elapsed;


            if (state.IsKeyDown(Keys.Y))
                cursor.TrailDamping += 100.0f * elapsed;
            if (state.IsKeyDown(Keys.H))
                cursor.TrailDamping -= 100.0f * elapsed;


            if (state.IsKeyDown(Keys.U))
                cursor.TrailNodeMass += 1.0f * elapsed;
            if (state.IsKeyDown(Keys.J))
                cursor.TrailNodeMass -= 1.0f * elapsed;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);

            spriteBatch.Begin();
            DrawString("[Q/A] Start Scale   : " + cursor.StartScale, new Vector2(20, 20));
            DrawString("[W/S] End Scale     : " + cursor.EndScale, new Vector2(20, 40));
            DrawString("[E/D] Lerp Exponent : " + cursor.LerpExponent, new Vector2(20, 60));
            DrawString("[R/F] Border Size   : " + cursor.BorderSize, new Vector2(20, 80));

            DrawString("[T/G] Stiffness     : " + cursor.TrailStiffness, new Vector2(400, 20));
            DrawString("[Y/H] Damping       : " + cursor.TrailDamping, new Vector2(400, 40));
            DrawString("[U/J] Node Mass     : " + cursor.TrailNodeMass, new Vector2(400, 60));

            spriteBatch.End();
        }

        private void DrawString(String text, Vector2 position)
        {
            spriteBatch.DrawString(spriteFont, text, position + new Vector2(1, 1), Color.Black);
            spriteBatch.DrawString(spriteFont, text, position , Color.White);
        }
    }
}
