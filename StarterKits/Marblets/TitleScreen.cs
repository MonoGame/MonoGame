#region File Description
//-----------------------------------------------------------------------------
// TitleScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
#if IPHONE
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#else
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endif
#endregion

namespace Marblets
{
    /// <summary>
    /// Title screen is the initial screen for Marblets. Allows you to select 2d or 3d 
    /// and displays the high scores
    /// </summary>
    public class TitleScreen : Screen
    {
        public static TouchCollection touchCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleScreen"/> class.
        /// </summary>
        /// <param name="game">The game object to use</param>
        /// <param name="backgroundImage">The background image to use when this is 
        /// visible</param>
        /// <param name="backgroundMusic">The background music to play when this is 
        /// visible</param>
        public TitleScreen(Game game, string backgroundImage)
            : base(game, backgroundImage)
        {
        }

        /// <summary>
        /// Checks for game start button presses
        /// </summary>
        /// 
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            GameState returnValue = GameState.None;

            // Added functionality to check for the Start button being touched.
            touchCollection = TouchPanel.GetState();

            Rectangle startButton = Rectangle.Empty;

            switch (MarbletsGame.screenOrientation)
            {
                case MarbletsGame.ScreenOrientation.LandscapeRight:
                    startButton.X = 285;
                    startButton.Width = 25;
                    startButton.Y = 35;
                    startButton.Height = 65;
                    break;

                case MarbletsGame.ScreenOrientation.LandscapeLeft:
                    startButton.X = 272 - 225 - 25;
                    startButton.Width = 25;
                    startButton.Y = 480 - 45 -65;
                    startButton.Height = 65;
                    break;

                default:
                    break;
            }

            if (touchCollection.Count > 0)
            {
                if ((touchCollection[0].Position.X >= startButton.X &&
                    touchCollection[0].Position.X <= (startButton.X +
                    startButton.Width)) &&
                    (touchCollection[0].Position.Y >= startButton.Y &&
                    touchCollection[0].Position.Y <= (startButton.Y +
                    startButton.Height)))
                {
                    returnValue = GameState.Play2D;
                }
            }

            MarbletsGame.NextGameState = returnValue;
        }

        /// <summary>
        /// Draws the high scores
        /// </summary>
        /// 
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            //If the guide is visible - then there are no high scores yet and the array
            //will be null
            if (MarbletsGame.HighScores != null)
            {
                SpriteBatch.Begin(SpriteSortMode.Deferred,SpriteBlendMode.AlphaBlend);

                int xPosition = 0;
                int yPosition = 0;

                //Draw the high scores
                for (int i = 0; i < 5; i++)
                {
                    if (MarbletsGame.HighScores[i] != 0)
                    {
                        // Added functionality to account for the screen orientation.
                        switch (MarbletsGame.screenOrientation)
                        {
                            case MarbletsGame.ScreenOrientation.LandscapeRight:
                                xPosition = 215 + (int)(i * 48 * Font.ZuneFontScale);
                                yPosition = 230;
                                break;

                            case MarbletsGame.ScreenOrientation.LandscapeLeft:
                                xPosition = 272 - 160 - (int)(i * 48 * 
                                    Font.ZuneFontScale);
                                yPosition = 480 - 260;
                                break;

                            default:
                                break;
                        }
						
                        Font.Draw(SpriteBatch, FontStyle.Large, xPosition, yPosition,MarbletsGame.HighScores[i]);
                    }
                }

                SpriteBatch.End();
            }
        }
    }
}
