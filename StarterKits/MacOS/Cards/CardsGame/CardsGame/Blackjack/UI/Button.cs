#region File Description
//-----------------------------------------------------------------------------
// Button.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
using CardsFramework;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace Blackjack
{
    public class Button : AnimatedGameComponent
    {
        #region Fields and Properties
        bool isKeyDown = false;
        bool isPressed = false;
        SpriteBatch spriteBatch;

        public Texture2D RegularTexture { get; set; }
        public Texture2D PressedTexture { get; set; }
        public SpriteFont Font { get; set; }
        public Rectangle Bounds { get; set; }

        string regularTexture;
        string pressedTexture;

        public event EventHandler Click;
        InputState input;

        InputHelper inputHelper;

        #endregion

        #region Initiaizations
        /// <summary>
        /// Creates a new instance of the <see cref="Button"/> class.
        /// </summary>
        /// <param name="regularTexture">The name of the button's texture.</param>
        /// <param name="pressedTexture">The name of the texture to display when the 
        /// button is pressed.</param>
        /// <param name="input">A <see cref="GameStateManagement.InputState"/> object
        /// which can be used to retrieve user input.</param>
        /// <param name="cardGame">The associated card game.</param>
        /// <remarks>Texture names are relative to the "Images" content 
        /// folder.</remarks>
        public Button(string regularTexture, string pressedTexture, InputState input,
            CardsGame cardGame)
            : base(cardGame, null)
        {
            this.input = input;
            this.regularTexture = regularTexture;
            this.pressedTexture = pressedTexture;
        }

        /// <summary>
        /// Initializes the button.
        /// </summary>
        public override void Initialize()
        {
#if WINDOWS_PHONE || IOS || ANDROID
            // Enable tab gesture
            TouchPanel.EnabledGestures = GestureType.Tap;
#endif
            // Get Xbox curser
            inputHelper = null;
            for (int componentIndex = 0; componentIndex < Game.Components.Count; componentIndex++)
            {
                if (Game.Components[componentIndex] is InputHelper)
                {
                    inputHelper = (InputHelper)Game.Components[componentIndex];
                    break;
                }
            }

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            base.Initialize();
        }
        #endregion

        #region Loading
        /// <summary>
        /// Loads the content required bt the button.
        /// </summary>
        protected override void LoadContent()
        {
            if (regularTexture != null)
            {
                RegularTexture = Game.Content.Load<Texture2D>(@"Images\" + regularTexture);
            }
            if (pressedTexture != null)
            {
                PressedTexture = Game.Content.Load<Texture2D>(@"Images\" + pressedTexture);
            }

            base.LoadContent();
        }
        #endregion

        #region Update and Render
        /// <summary>
        /// Performs update logic for the button.
        /// </summary>
        /// <param name="gameTime">The time that has passed since the last call to 
        /// this method.</param>
        public override void Update(GameTime gameTime)
        {
            if (RegularTexture != null)
            {
                HandleInput(Mouse.GetState());
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Handle the input of adding chip on all platform
        /// </summary>
        /// <param name="mouseState">Mouse input information.</param>
        /// <param name="inputHelper">Input of Xbox simulated cursor.</param>
        private void HandleInput(MouseState mouseState)
        {
            bool pressed = false;
            Vector2 position = Vector2.Zero;

#if WINDOWS_PHONE || IOS || ANDROID
            if ((input.Gestures.Count > 0) && input.Gestures[0].GestureType == GestureType.Tap)
            {
                pressed = true;
                position = input.Gestures[0].Position;
            }
#else
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                pressed = true;
                position = new Vector2(mouseState.X, mouseState.Y);
            }
            else if (inputHelper.IsPressed)
            {
                pressed = true;
                position = inputHelper.PointPosition;
            }
            else
            {
                if (isPressed)
                {
                    if (IntersectWith(new Vector2(mouseState.X, mouseState.Y)) ||
                        IntersectWith(inputHelper.PointPosition))
                    {
                        FireClick();
                        isPressed = false;
                    }
                    else
                    {
                        
                        isPressed = false;
                    }
                }

                isKeyDown = false;
            }
#endif

            if (pressed)
            {
                if (!isKeyDown)
                {
                    if (IntersectWith(position))
                    {
                        isPressed = true;
#if WINDOWS_PHONE || IOS || ANDROID
                        FireClick();
                        isPressed = false;
#endif
                    }
                    isKeyDown = true;
                }
            }
            else
            {
                isKeyDown = false;
            }
        }

        /// <summary>
        /// Checks if the button intersects with a specified position
        /// </summary>
        /// <param name="position">The position to check intersection against.</param>
        /// <returns>True if the position intersects with the button, 
        /// false otherwise.</returns>
        private bool IntersectWith(Vector2 position)
        {
            Rectangle touchTap = new Rectangle((int)position.X - 1, (int)position.Y - 1, 2, 2);
            return Bounds.Intersects(touchTap);
        }

        /// <summary>
        /// Fires the button's click event.
        /// </summary>
        public void FireClick()
        {
            if (Click != null)
            {
                Click(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Draws the button.
        /// </summary>
        /// <param name="gameTime">The time that has passed since the last call to 
        /// this method.</param>
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();


            spriteBatch.Draw(isPressed ? PressedTexture : RegularTexture, Bounds, Color.White);
            if (Font != null)
            {
                Vector2 textPosition = Font.MeasureString(Text);
                textPosition = new Vector2(Bounds.Width - textPosition.X,
                    Bounds.Height - textPosition.Y);
                textPosition /= 2;
                textPosition.X += Bounds.X;
                textPosition.Y += Bounds.Y;
                spriteBatch.DrawString(Font, Text, textPosition, Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            Click = null;
            base.Dispose(disposing);
        }
    }
}
