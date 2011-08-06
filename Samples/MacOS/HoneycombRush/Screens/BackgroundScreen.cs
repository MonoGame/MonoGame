#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#endregion

namespace HoneycombRush
{
    class BackgroundScreen : GameScreen
    {
        #region Fields


        Texture2D background;
        string backgroundName;


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new background screen.
        /// </summary>
        /// <param name="backgroundName">Name of the background texture to use.</param>
        public BackgroundScreen(string backgroundName)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            
            this.backgroundName = backgroundName;
        }

        /// <summary>
        /// Load screen resources.
        /// </summary>
        public override void LoadContent()
        {
            background = ScreenManager.Game.Content.Load<Texture2D>("Textures/Backgrounds/" + backgroundName);

            base.LoadContent();
        }


        #endregion

        #region Update


        /// <summary>
        /// Update the screen.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="otherScreenHasFocus">Whether or not another screen currently has the focus.</param>
        /// <param name="coveredByOtherScreen">Whether or not this screen is covered by another.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {

            base.Update(gameTime, otherScreenHasFocus, false);
        }
        

        #endregion

        #region Render


        /// <summary>
        /// Renders the screen.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw background
            spriteBatch.Draw(background, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White * TransitionAlpha);

            spriteBatch.End();
        }


        #endregion
    }
}
