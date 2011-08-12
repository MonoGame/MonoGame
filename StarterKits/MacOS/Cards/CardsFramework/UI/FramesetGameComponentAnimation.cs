#region File Description
//-----------------------------------------------------------------------------
// FramesetGameComponentAnimation.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace CardsFramework
{
    /// <summary>
    /// A "typical" animation that consists of alternating between a set of frames.
    /// </summary>
    public class FramesetGameComponentAnimation : AnimatedGameComponentAnimation
    {
        #region Fields
        Texture2D framesTexture;
        int numberOfFrames;
        int numberOfFramePerRow;
        Vector2 frameSize;

        private double percent = 0;
        #endregion

        #region Initializations
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="framesTexture">The frames texture (animation sheet).</param>
        /// <param name="numberOfFrames">The number of frames in the sheet.</param>
        /// <param name="numberOfFramePerRow">The number of frame per row.</param>
        /// <param name="frameSize">Size of the frame.</param>
        public FramesetGameComponentAnimation(Texture2D framesTexture, int numberOfFrames, 
            int numberOfFramePerRow, Vector2 frameSize)
        {
            this.framesTexture = framesTexture;
            this.numberOfFrames = numberOfFrames;
            this.numberOfFramePerRow = numberOfFramePerRow;
            this.frameSize = frameSize;
        }
        #endregion

        /// <summary>
        /// Runs the frame set animation.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Run(GameTime gameTime)
        {
            if (IsStarted())
            {
                // Calculate the completion percent of the animation
                percent += (((gameTime.ElapsedGameTime.TotalMilliseconds /
                    (Duration.TotalMilliseconds / AnimationCycles)) * 100));

                if (percent >= 100)
                {
                    percent = 0;
                }

                // Calculate the current frame index
                int animationIndex = (int)(numberOfFrames * percent / 100);
                Component.CurrentSegment = 
                    new Rectangle(
                        (int)frameSize.X * (animationIndex % numberOfFramePerRow), 
                        (int)frameSize.Y * (animationIndex / numberOfFramePerRow),
                        (int)frameSize.X, (int)frameSize.Y);
                Component.CurrentFrame = framesTexture;

            }
            else
            {
                Component.CurrentFrame = null;
                Component.CurrentSegment = null;
            }
            base.Run(gameTime);
        }
    }
}
