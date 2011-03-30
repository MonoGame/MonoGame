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
using GameStateManagement;

#endregion

namespace MemoryMadness
{
    class BackgroundScreen : GameScreen
    {
        #region Fields

        Texture2D background;
        Texture2D leftDoor;
        Texture2D rightDoor;

        Vector2 leftDoorPosition;
        Vector2 rightDoorPosition;

        bool animateDoors;
        bool doorsInTranistion;
        bool doorsHitFinalPosition = false;
        bool doorsBounceStarted = false;

        #endregion

        #region Initialization

        public BackgroundScreen(bool animateDoors)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            this.animateDoors = animateDoors;
            if (animateDoors)
            {
                AudioManager.PlaySound("doorOpen");
            }
        }

        #endregion

        #region Loading

        /// <summary>
        /// Load screen resources
        /// </summary>
        public override void LoadContent()
        {
            background = Load<Texture2D>(@"Textures\Backgrounds\titleBG");
            leftDoor = Load<Texture2D>(@"Textures\Backgrounds\leftDoor");
            rightDoor = Load<Texture2D>(@"Textures\Backgrounds\rightDoor");

            // Prepare to run the doors' animation
            if (animateDoors)
                doorsInTranistion = true;

            // Set the doors' start position
            leftDoorPosition = Settings.LeftDoorClosedPosition;
            rightDoorPosition = Settings.RightDoorClosedPosition;
        }

        #endregion

        #region Update

        /// <summary>
        /// Update the screen
        /// </summary>
        /// <param name="gameTime">The game time</param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                            bool coveredByOtherScreen)
        {
            if (doorsInTranistion && animateDoors)
                AnimateDoors();

            base.Update(gameTime, otherScreenHasFocus, false);
        }

        private void AnimateDoors()
        {
            if (!doorsHitFinalPosition || doorsBounceStarted)
            {
                // Update door X positions between the opened and closed states
                leftDoorPosition.X = MathHelper.Clamp(
                    leftDoorPosition.X - Settings.DoorsAnimationStep,
                    Settings.LeftDoorOpenedPosition.X,
                    Settings.LeftDoorClosedPosition.X);


                rightDoorPosition.X = MathHelper.Clamp(
                    rightDoorPosition.X + Settings.DoorsAnimationStep,
                    Settings.RightDoorClosedPosition.X,
                    Settings.RightDoorOpenedPosition.X);

                // If both doors reach their final position, raise a flag
                if (leftDoorPosition == Settings.LeftDoorOpenedPosition &&
                    rightDoorPosition == Settings.RightDoorOpenedPosition)
                {
                    if (!doorsHitFinalPosition)
                        doorsHitFinalPosition = true;
                    else
                        doorsInTranistion = false;
                }
            }
            else if (doorsHitFinalPosition)
            {              
                // Move the doors back towards their original opened position slightly 
                // to create a bouncing effect
                leftDoorPosition.X = MathHelper.Clamp(
                    leftDoorPosition.X + Settings.DoorsAnimationStep / 2,
                    Settings.LeftDoorOpenedPosition.X,
                    Settings.LeftDoorOpenedPosition.X + Settings.DoorsAnimationStep * 3);


                rightDoorPosition.X = MathHelper.Clamp(
                    rightDoorPosition.X - Settings.DoorsAnimationStep / 2,
                    Settings.RightDoorOpenedPosition.X - Settings.DoorsAnimationStep * 3,
                    Settings.RightDoorOpenedPosition.X);

                if ((leftDoorPosition.X == Settings.LeftDoorOpenedPosition.X +
                    Settings.DoorsAnimationStep * 3) &&
                    (rightDoorPosition.X == Settings.RightDoorOpenedPosition.X -
                    Settings.DoorsAnimationStep * 3))
                {
                    doorsBounceStarted = true;
                }
            }
        }

        #endregion

        #region Render

        /// <summary>
        /// Renders the screen
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw background
            spriteBatch.Draw(background, new Vector2(0, 0),
                 Color.White * TransitionAlpha);

            // Draw the doors
            spriteBatch.Draw(leftDoor, leftDoorPosition, Color.White * TransitionAlpha);
            spriteBatch.Draw(rightDoor, rightDoorPosition, Color.White * TransitionAlpha);

            spriteBatch.End();
        }

        #endregion
    }
}
