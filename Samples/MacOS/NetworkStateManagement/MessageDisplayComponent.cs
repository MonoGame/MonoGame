#region File Description
//-----------------------------------------------------------------------------
// MessageDisplayComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace NetworkStateManagement
{
    /// <summary>
    /// Component implements the IMessageDisplay interface. This is used to show
    /// notification messages when interesting events occur, for instance when
    /// gamers join or leave the network session
    /// </summary>
    class MessageDisplayComponent : DrawableGameComponent, IMessageDisplay
    {
        #region Fields

        SpriteBatch spriteBatch;
        SpriteFont font;

        // List of the currently visible notification messages.
        List<NotificationMessage> messages = new List<NotificationMessage>();

        // Coordinates threadsafe access to the message list.
        object syncObject = new object();

        // Tweakable settings control how long each message is visible.
        static readonly TimeSpan fadeInTime = TimeSpan.FromSeconds(0.25);
        static readonly TimeSpan showTime = TimeSpan.FromSeconds(5);
        static readonly TimeSpan fadeOutTime = TimeSpan.FromSeconds(0.5);

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new message display component.
        /// </summary>
        public MessageDisplayComponent(Game game)
            : base(game)
        {
            // Register ourselves to implement the IMessageDisplay service.
            game.Services.AddService(typeof(IMessageDisplay), this);
        }


        /// <summary>
        /// Load graphics content for the message display.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Game.Content.Load<SpriteFont>("menufont");
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the message display component.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            lock (syncObject)
            {
                int index = 0;
                float targetPosition = 0;

                // Update each message in turn.
                while (index < messages.Count)
                {
                    NotificationMessage message = messages[index];

                    // Gradually slide the message toward its desired position.
                    float positionDelta = targetPosition - message.Position;

                    float velocity = (float)gameTime.ElapsedGameTime.TotalSeconds * 2;

                    message.Position += positionDelta * Math.Min(velocity, 1);

                    // Update the age of the message.
                    message.Age += gameTime.ElapsedGameTime;

                    if (message.Age < showTime + fadeOutTime)
                    {
                        // This message is still alive.
                        index++;

                        // Any subsequent messages should be positioned below
                        // this one, unless it has started to fade out.
                        if (message.Age < showTime)
                            targetPosition++;
                    }
                    else
                    {
                        // This message is old, and should be removed.
                        messages.RemoveAt(index);
                    }
                }
            }
        }


        /// <summary>
        /// Draws the message display component.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            lock (syncObject)
            {
                // Early out if there are no messages to display.
                if (messages.Count == 0)
                    return;

                Vector2 position = new Vector2(GraphicsDevice.Viewport.Width - 100, 0);

                spriteBatch.Begin();

                // Draw each message in turn.
                foreach (NotificationMessage message in messages)
                {
                    const float scale = 0.75f;

                    // Compute the alpha of this message.
                    float alpha = 1;

                    if (message.Age < fadeInTime)
                    {
                        // Fading in.
                        alpha = (float)(message.Age.TotalSeconds /
                                        fadeInTime.TotalSeconds);
                    }
                    else if (message.Age > showTime)
                    {
                        // Fading out.
                        TimeSpan fadeOut = showTime + fadeOutTime - message.Age;

                        alpha = (float)(fadeOut.TotalSeconds /
                                        fadeOutTime.TotalSeconds);
                    }

                    // Compute the message position.
                    position.Y = 80 + message.Position * font.LineSpacing * scale;

                    // Compute an origin value to right align each message.
                    Vector2 origin = font.MeasureString(message.Text);
                    origin.Y = 0;

                    // Draw the message text, with a drop shadow.
                    spriteBatch.DrawString(font, message.Text, position + Vector2.One,
                                           Color.Black * alpha, 0,
                                           origin, scale, SpriteEffects.None, 0);

                    spriteBatch.DrawString(font, message.Text, position,
                                           Color.White * alpha, 0,
                                           origin, scale, SpriteEffects.None, 0);
                }

                spriteBatch.End();
            }
        }


        #endregion

        #region Implement IMessageDisplay


        /// <summary>
        /// Shows a new notification message.
        /// </summary>
        public void ShowMessage(string message, params object[] parameters)
        {
            string formattedMessage = string.Format(message, parameters);

            lock (syncObject)
            {
                float startPosition = messages.Count;

                messages.Add(new NotificationMessage(formattedMessage, startPosition));
            }
        }


        #endregion

        #region Nested Types


        /// <summary>
        /// Helper class stores the position and text of a single notification message.
        /// </summary>
        class NotificationMessage
        {
            public string Text;
            public float Position;
            public TimeSpan Age;


            public NotificationMessage(string text, float position)
            {
                Text = text;
                Position = position;
                Age = TimeSpan.Zero;
            }
        }


        #endregion
    }
}
