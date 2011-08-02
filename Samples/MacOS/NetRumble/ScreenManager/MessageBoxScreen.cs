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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace NetRumble
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    /// <remarks>
    /// This public class is somewhat similar to one of the same name in the 
    /// GameStateManagement sample.
    /// </remarks>
    public class MessageBoxScreen : GameScreen
    {
        #region Constants
        const string usageText = "A button = Okay\n" +
                                 "B button = Cancel";
        #endregion

        #region Fields

        bool pauseMenu = false;
        string message;
        SpriteFont smallFont;

        #endregion

        #region Events

        public event EventHandler<EventArgs> Accepted;
        public event EventHandler<EventArgs> Cancelled;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageBoxScreen(string message)
        {
            this.message = message;

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.25);
            TransitionOffTime = TimeSpan.FromSeconds(0.25);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageBoxScreen(string message, bool pauseMenu) : this(message)
        {
            this.pauseMenu = pauseMenu;
        }


        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the ScreenManager, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded instance.
        /// </summary>
        public override void LoadContent()
        {
            smallFont = ScreenManager.Content.Load<SpriteFont>("Fonts/MessageBox");
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input.MenuSelect && (!pauseMenu || 
                (input.CurrentGamePadState.Buttons.A == ButtonState.Pressed)))
            {
                // Raise the accepted event, then exit the message box.
                if (Accepted != null)
                    Accepted(this, EventArgs.Empty);
            
                ExitScreen();
            }
            else if (input.MenuCancel || (input.MenuSelect && pauseMenu && 
                (input.CurrentGamePadState.Buttons.A == ButtonState.Released)))
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
            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            // Center the message text in the viewport.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 textSize = ScreenManager.Font.MeasureString(message);
            Vector2 textPosition = (viewportSize - textSize) / 2;
            Vector2 usageTextSize = smallFont.MeasureString(usageText);
            Vector2 usageTextPosition = (viewportSize - usageTextSize) / 2;
            usageTextPosition.Y = textPosition.Y + 
                ScreenManager.Font.LineSpacing * 1.1f;

            // Fade the popup alpha during transitions.
            Color color = new Color(255, 255, 255, TransitionAlpha);

            // Draw the background rectangles
            Rectangle rect = new Rectangle(
                (int)(Math.Min(usageTextPosition.X, textPosition.X)),
                (int)(textPosition.Y),
                (int)(Math.Max(usageTextSize.X, textSize.X)),
                (int)(ScreenManager.Font.LineSpacing * 1.1f+ usageTextSize.Y)
                );
            rect.X -= (int)(0.1f * rect.Width);
            rect.Y -= (int)(0.1f * rect.Height);
            rect.Width += (int)(0.2f * rect.Width);
            rect.Height += (int)(0.2f * rect.Height);

            Rectangle rect2 = new Rectangle(rect.X - 1, rect.Y - 1, 
                rect.Width + 2, rect.Height + 2);
            ScreenManager.DrawRectangle(rect2, new Color(128, 128, 128, 
                (byte)(192.0f * (float)TransitionAlpha / 255.0f)));
            ScreenManager.DrawRectangle(rect, new Color(0, 0, 0, 
                (byte)(232.0f * (float)TransitionAlpha / 255.0f)));

            // Draw the message box text.
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, message,
                                                 textPosition, color);
            ScreenManager.SpriteBatch.DrawString(smallFont, usageText,
                                                 usageTextPosition, color);
            ScreenManager.SpriteBatch.End();
        }


        #endregion
    }
}
