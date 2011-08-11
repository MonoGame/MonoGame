#region File Description
//-----------------------------------------------------------------------------
// MessageBoxScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    /// <remarks>
    /// Similar to a class found in the Game State Management sample on the 
    /// XNA Creators Club Online website (http://creators.xna.com).
    /// </remarks>
    class MessageBoxScreen : GameScreen
    {
        #region Fields


        string message;

        private Texture2D backgroundTexture;
        private Vector2 backgroundPosition;

        private Texture2D loadingBlackTexture;
        private Rectangle loadingBlackTextureDestination;

        private Texture2D backTexture;
        private Vector2 backPosition;

        private Texture2D selectTexture;
        private Vector2 selectPosition;

        private Vector2 confirmPosition, messagePosition;


        #endregion


        #region Events

        public event EventHandler<EventArgs> Accepted;
        public event EventHandler<EventArgs> Cancelled;

        #endregion


        #region Initialization


        /// <summary>
        /// Constructor lets the caller specify the message.
        /// </summary>
        public MessageBoxScreen(string message)
        {
            this.message = message;

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);
        }


        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            backgroundTexture = content.Load<Texture2D>(@"Textures\MainMenu\Confirm");
            backTexture = content.Load<Texture2D>(@"Textures\Buttons\BButton");
            selectTexture = content.Load<Texture2D>(@"Textures\Buttons\AButton");
            loadingBlackTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            backgroundPosition = new Vector2(
                (viewport.Width - backgroundTexture.Width) / 2,
                (viewport.Height - backgroundTexture.Height) / 2);
            loadingBlackTextureDestination = new Rectangle(viewport.X, viewport.Y,
                viewport.Width, viewport.Height);

            backPosition = backgroundPosition + new Vector2(50f,
                backgroundTexture.Height - 100);
            selectPosition = backgroundPosition + new Vector2(
                backgroundTexture.Width - 100, backgroundTexture.Height - 100);

            confirmPosition.X = backgroundPosition.X + (backgroundTexture.Width -
                Fonts.HeaderFont.MeasureString("Confirmation").X) / 2f;
            confirmPosition.Y = backgroundPosition.Y + 47;

            message = Fonts.BreakTextIntoLines(message, 36, 10);
            messagePosition.X = backgroundPosition.X + (int)((backgroundTexture.Width -
                Fonts.GearInfoFont.MeasureString(message).X) / 2);
            messagePosition.Y = (backgroundPosition.Y * 2) - 20;
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput()
        {
            if (InputManager.IsActionTriggered(InputManager.Action.Ok))
            {
                // Raise the accepted event, then exit the message box.
                if (Accepted != null)
                    Accepted(this, EventArgs.Empty);

                ExitScreen();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.Back))
            {
                // Raise the cancelled event, then exit the message box.
                if (Cancelled != null)
                    Cancelled(this, EventArgs.Empty);

                ExitScreen();
            }
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.Draw(loadingBlackTexture, loadingBlackTextureDestination, 
                Color.White);
            spriteBatch.Draw(backgroundTexture, backgroundPosition, Color.White);
            spriteBatch.Draw(backTexture, backPosition, Color.White);
            spriteBatch.Draw(selectTexture, selectPosition, Color.White);
            spriteBatch.DrawString(Fonts.ButtonNamesFont, "No",
                new Vector2(backPosition.X + backTexture.Width + 5, backPosition.Y + 5),
                Color.White);
            spriteBatch.DrawString(Fonts.ButtonNamesFont, "Yes",
                new Vector2(
                selectPosition.X - Fonts.ButtonNamesFont.MeasureString("Yes").X,
                selectPosition.Y + 5), Color.White);
            spriteBatch.DrawString(Fonts.HeaderFont, "Confirmation", confirmPosition,
                Fonts.CountColor);
            spriteBatch.DrawString(Fonts.GearInfoFont, message, messagePosition, 
                Fonts.CountColor);

            spriteBatch.End();
        }


        #endregion
    }
}
