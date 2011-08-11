#region File Description
//-----------------------------------------------------------------------------
// GameOverScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using RolePlayingGameData;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Displays the game-over screen, after the player has lost.
    /// </summary>
    class GameOverScreen : GameScreen
    {
        #region Graphics Data


        private Texture2D backTexture;
        private Texture2D selectIconTexture;
        private Texture2D fadeTexture;
        private Vector2 backgroundPosition;
        private Vector2 titlePosition;
        private Vector2 gameOverPosition;
        private Vector2 selectPosition;
        private Vector2 selectIconPosition;


        #endregion


        #region Text Data


        private readonly string titleString = "Game Over";
        private readonly string gameOverString = "The party has been defeated.";
        private readonly string selectString = "Continue";


        #endregion


        #region Initialization


        /// <summary>
        /// Create a new GameOverScreen object.
        /// </summary>
        public GameOverScreen()
            : base()
        {
            AudioManager.PushMusic("LoseTheme");
            this.Exiting += new EventHandler(GameOverScreen_Exiting);
        }


        void GameOverScreen_Exiting(object sender, EventArgs e)
        {
            AudioManager.PopMusic();
        }


        /// <summary>
        /// Load the graphics data from the content manager.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            fadeTexture = content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");
            backTexture = content.Load<Texture2D>(@"Textures\GameScreens\PopupScreen");
            selectIconTexture = content.Load<Texture2D>(@"Textures\Buttons\AButton");

            backgroundPosition.X = (viewport.Width - backTexture.Width) / 2;
            backgroundPosition.Y = (viewport.Height - backTexture.Height) / 2;

            titlePosition.X = (viewport.Width -
                Fonts.HeaderFont.MeasureString(titleString).X) / 2;
            titlePosition.Y = backgroundPosition.Y + 70f;

            gameOverPosition.X = (viewport.Width -
                Fonts.ButtonNamesFont.MeasureString(titleString).X) / 2;
            gameOverPosition.Y = backgroundPosition.Y + backTexture.Height / 2;

            selectIconPosition.X = viewport.Width / 2 + 260;
            selectIconPosition.Y = backgroundPosition.Y + 530f;
            selectPosition.X = selectIconPosition.X -
                Fonts.ButtonNamesFont.MeasureString(selectString).X - 10f;
            selectPosition.Y = backgroundPosition.Y + 530f;
        }


        #endregion


        #region Updating


        /// <summary>
        /// Handles user input.
        /// </summary>
        public override void HandleInput()
        {
            if (InputManager.IsActionTriggered(InputManager.Action.Ok) ||
                InputManager.IsActionTriggered(InputManager.Action.Back))
            {
                ExitScreen();
                ScreenManager.AddScreen(new MainMenuScreen());
                return;
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draws the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            // Draw fading screen
            spriteBatch.Draw(fadeTexture, new Rectangle(0, 0, 1280, 720), Color.White);

            // Draw popup texture
            spriteBatch.Draw(backTexture, backgroundPosition, Color.White);

            // Draw title
            spriteBatch.DrawString(Fonts.HeaderFont, titleString, titlePosition,
                Fonts.TitleColor);

            // Draw Gameover text
            spriteBatch.DrawString(Fonts.ButtonNamesFont, gameOverString,
                gameOverPosition, Fonts.CountColor);

            // Draw select button
            spriteBatch.DrawString(Fonts.ButtonNamesFont, selectString, selectPosition,
                Color.White);
            spriteBatch.Draw(selectIconTexture, selectIconPosition, Color.White);

            spriteBatch.End();
        }


        #endregion
    }
}
