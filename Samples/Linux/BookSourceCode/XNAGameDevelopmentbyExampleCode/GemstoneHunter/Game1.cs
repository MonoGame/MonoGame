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
using Tile_Engine;

namespace Gemstone_Hunter
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Player player;
        SpriteFont pericles8;
        Vector2 scorePosition = new Vector2(20, 580);
        enum GameState { TitleScreen, Playing, PlayerDead, GameOver };
        GameState gameState = GameState.TitleScreen;

        Vector2 gameOverPosition = new Vector2(350, 300);
        Vector2 livesPosition = new Vector2(600, 580);

        Texture2D titleScreen;

        float deathTimer = 0.0f;
        float deathDelay = 5.0f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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

            this.graphics.PreferredBackBufferWidth = 800;
            this.graphics.PreferredBackBufferHeight = 600;
            this.graphics.ApplyChanges();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            TileMap.Initialize(
                Content.Load<Texture2D>(@"Textures\PlatformTiles"));
            TileMap.spriteFont =
                Content.Load<SpriteFont>(@"Fonts\Pericles8");

            pericles8 = Content.Load<SpriteFont>(@"Fonts\Pericles8");

            titleScreen = Content.Load<Texture2D>(@"Textures\TitleScreen");

            Camera.WorldRectangle = new Rectangle(0, 0, 160 * 48, 12 * 48);
            Camera.Position = Vector2.Zero;
            Camera.ViewPortWidth = 800;
            Camera.ViewPortHeight = 600;

            player = new Player(Content);
            LevelManager.Initialize(Content, player);
        }

        private void StartNewGame()
        {
            player.Revive();
            player.LivesRemaining = 3;
            player.WorldLocation = Vector2.Zero;
            LevelManager.LoadLevel(0);
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
			KeyboardState keyState = Keyboard.GetState();
			
            // Allows the game to exit
            if (keyState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back ==
                ButtonState.Pressed)
                this.Exit();

            
            GamePadState gamepadState = GamePad.GetState(PlayerIndex.One);
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (gameState == GameState.TitleScreen)
            {
                if (keyState.IsKeyDown(Keys.Space) ||
                    gamepadState.Buttons.A == ButtonState.Pressed)
                {
                    StartNewGame();
                    gameState = GameState.Playing;
                }
            }

            if (gameState == GameState.Playing)
            {
                player.Update(gameTime);
                LevelManager.Update(gameTime);
                if (player.Dead)
                {
                    if (player.LivesRemaining > 0)
                    {
                        gameState = GameState.PlayerDead;
                        deathTimer = 0.0f;
                    }
                    else
                    {
                        gameState = GameState.GameOver;
                        deathTimer = 0.0f;
                    }
                }
            }

            if (gameState == GameState.PlayerDead)
            {
                player.Update(gameTime);
                LevelManager.Update(gameTime);
                deathTimer += elapsed;
                if (deathTimer > deathDelay)
                {
                    player.WorldLocation = Vector2.Zero;
                    LevelManager.ReloadLevel();
                    player.Revive();
                    gameState = GameState.Playing;
                }
            }

            if (gameState == GameState.GameOver)
            {
                deathTimer += elapsed;
                if (deathTimer > deathDelay)
                {
                    gameState = GameState.TitleScreen;
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(
                SpriteSortMode.BackToFront,
                BlendState.AlphaBlend);

            if (gameState == GameState.TitleScreen)
            {
                spriteBatch.Draw(titleScreen, Vector2.Zero, Color.White);
            }

            if ((gameState == GameState.Playing) ||
                (gameState == GameState.PlayerDead) ||
                (gameState == GameState.GameOver))
            {
                TileMap.Draw(spriteBatch);
                player.Draw(spriteBatch);
                LevelManager.Draw(spriteBatch);
                spriteBatch.DrawString(
                    pericles8,
                    "Score: " + player.Score.ToString(),
                    scorePosition,
                    Color.White);
                spriteBatch.DrawString(
                    pericles8,
                    "Lives Remaining: " + player.LivesRemaining.ToString(),
                    livesPosition,
                    Color.White);
            }

            if (gameState == GameState.PlayerDead)
            {
            }

            if (gameState == GameState.GameOver)
            {
                spriteBatch.DrawString(
                    pericles8,
                    "G A M E  O V E R !",
                    gameOverPosition,
                    Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
