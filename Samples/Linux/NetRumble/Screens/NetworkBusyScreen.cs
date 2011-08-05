#region File Description
//-----------------------------------------------------------------------------
// NetworkBusyScreen.cs
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

namespace NetRumble
{
    /// <summary>
    /// When an asynchronous network operation (for instance searching for or joining a
    /// session) is in progress, we want to display some sort of busy indicator to let
    /// the user know the game hasn't just locked up. We also want to make sure they
    /// can't pick some other menu option before the current operation has finished.
    /// This screen takes care of both requirements in a single stroke. It monitors
    /// the IAsyncResult returned by an asynchronous network call, displaying a busy
    /// indicator for as long as the call is still in progress. When it notices the
    /// IAsyncResult has completed, it raises an event to let the game know it should
    /// proceed to the next step, after which the busy screen automatically goes away.
    /// Because this screen is on top of all others for as long as the asynchronous
    /// operation is in progress, it automatically takes over all user input,
    /// preventing any other menu entries being selected until the operation completes.
    /// </summary>
    /// <remarks>Based on a class in the Network Game State Management sample.</remarks>
    class NetworkBusyScreen : GameScreen
    {
        #region Constants


        const float busyTextureScale = 0.8f;


        #endregion


        #region Fields


        /// <summary>
        /// The message displayed in the screen.
        /// </summary>
        string message;

        /// <summary>
        /// The async result polled by the screen.
        /// </summary>
        IAsyncResult asyncResult;

        /// <summary>
        /// The rotating "activity" texture in the screen.
        /// </summary>
        Texture2D busyTexture;


        #endregion


        #region Events


        public event EventHandler<OperationCompletedEventArgs> OperationCompleted;


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a network busy screen for the specified asynchronous operation.
        /// </summary>
        public NetworkBusyScreen(string message, IAsyncResult asyncResult)
        {
            this.message = message;
            this.asyncResult = asyncResult;

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.1);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);
        }

        
        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent NetworkBusyScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            busyTexture = content.Load<Texture2D>("Textures/chatTalking");
        }


        #endregion


        #region Update and Draw


        /// <summary>
        /// Updates the NetworkBusyScreen.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Has our asynchronous operation completed?
            if ((asyncResult != null) && asyncResult.IsCompleted)
            {
                // If so, raise the OperationCompleted event.
                if (OperationCompleted != null)
                {
                    OperationCompleted(this,
                                       new OperationCompletedEventArgs(asyncResult));
                }

                ExitScreen();

                asyncResult = null;
            }
        }


        /// <summary>
        /// Draws the NetworkBusyScreen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            const int hPad = 32;
            const int vPad = 16;

            // Center the message text in the viewport.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 textSize = font.MeasureString(message);

            // Add enough room to spin a texture.
            Vector2 busyTextureSize = new Vector2(busyTexture.Width * busyTextureScale);
            Vector2 busyTextureOrigin = new Vector2(busyTexture.Width / 2, 
                busyTexture.Height / 2);

            textSize.X = Math.Max(textSize.X, busyTextureSize.X);
            textSize.Y += busyTextureSize.Y + vPad;

            Vector2 textPosition = (viewportSize - textSize) / 2;

            // The background includes a border somewhat larger than the text itself.
            Rectangle backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
                                                          (int)textPosition.Y - vPad,
                                                          (int)textSize.X + hPad * 2,
                                                          (int)textSize.Y + vPad * 2);

            // Fade the popup alpha during transitions.
            Color color = new Color(255, 255, 255, TransitionAlpha);

            // Draw the background rectangle.
            Rectangle backgroundRectangle2 = new Rectangle(backgroundRectangle.X - 1, 
                backgroundRectangle.Y - 1, backgroundRectangle.Width + 2, 
                backgroundRectangle.Height + 2);
            ScreenManager.DrawRectangle(backgroundRectangle2, new Color(128, 128, 128,
                (byte)(192.0f * (float)TransitionAlpha / 255.0f)));
            ScreenManager.DrawRectangle(backgroundRectangle, new Color(0, 0, 0,
                (byte)(232.0f * (float)TransitionAlpha / 255.0f)));

            //spriteBatch.Begin(0,BlendState.NonPremultiplied, null, null, null);
			spriteBatch.Begin();
            // Draw the message box text.
            spriteBatch.DrawString(font, message, textPosition, color);

            // Draw the spinning cat progress indicator.
            float busyTextureRotation = (float)gameTime.TotalGameTime.TotalSeconds * 3;

            Vector2 busyTexturePosition = new Vector2(textPosition.X + textSize.X / 2,
                textPosition.Y + textSize.Y - busyTextureSize.Y / 2);

            spriteBatch.Draw(busyTexture, busyTexturePosition, null, color,
                busyTextureRotation, busyTextureOrigin, busyTextureScale, 
                SpriteEffects.None, 0);

            spriteBatch.End();
        }


        #endregion
    }
}
