#region File Description
//-----------------------------------------------------------------------------
// ScaledAnimation.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region File Information
//-----------------------------------------------------------------------------
// ScaledAnimation.cs
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
    /// <summary>
    /// Supports animation playback.
    /// </summary>
    public class ScaledAnimation
    {
        #region Fields


        Texture2D animatedCharacter;
        Point sheetSize;
        public Point currentFrame;
        public Point frameSize;

        private TimeSpan lastestChangeTime;
        private TimeSpan timeInterval = TimeSpan.Zero;

        private int startFrame;
        private int endFrame;
        private int lastSubFrame = -1;

        bool drawWasAlreadyCalledOnce = false;

        public int FrameCount
        {
            get
            {
                return sheetSize.X * sheetSize.Y;
            }
        }

        public Vector2 Offset { get; set; }

        public int FrameIndex
        {
            get
            {
                return sheetSize.X * currentFrame.Y + currentFrame.X;
            }
            set
            {
                if (value >= sheetSize.X * sheetSize.Y + 1)
                {
                    throw new InvalidOperationException("Specified frame index exceeds available frames");
                }

                currentFrame.Y = value / sheetSize.X;
                currentFrame.X = value % sheetSize.X;
            }
        }

        public bool IsActive { get; private set; }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new instance of the animation class
        /// </summary>
        /// <param name="frameSheet">Texture which is a sheet containing 
        /// the animation frames.</param>
        /// <param name="size">The size of a single frame.</param>
        /// <param name="frameSheetSize">The size of the entire animation sheet.</param>
        public ScaledAnimation(Texture2D frameSheet, Point size, Point frameSheetSize)
        {
            animatedCharacter = frameSheet;
            frameSize = size;
            sheetSize = frameSheetSize;
            Offset = Vector2.Zero;
        }


        #endregion

        #region Update and Render


        /// <summary>
        /// Updates the animation's progress.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="isInMotion">Whether or not the animation element itself is
        /// currently in motion.</param>
        public void Update(GameTime gameTime, bool isInMotion)
        {
            Update(gameTime, isInMotion, false);
        }

        /// <summary>
        /// Updates the animation's progress.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="isInMotion">Whether or not the animation element itself is
        /// currently in motion.</param>
        /// <param name="runSubAnimation"></param>
        public void Update(GameTime gameTime, bool isInMotion, bool runSubAnimation)
        {
            if (IsActive && gameTime.TotalGameTime != lastestChangeTime)
            {
                // See if a time interval between frames is defined
                if (timeInterval != TimeSpan.Zero)
                {
                    // Do nothing until an interval passes
                    if (lastestChangeTime + timeInterval > gameTime.TotalGameTime)
                    {
                        return;
                    }
                }

                lastestChangeTime = gameTime.TotalGameTime;
                if (FrameIndex >= FrameCount)
                {
                    FrameIndex = 0; // Reset the animation
                }
                else
                {
                    // Only advance the animation if the animation element is moving
                    if (isInMotion)
                    {
                        if (runSubAnimation)
                        {
                            // Initialize the animation
                            if (lastSubFrame == -1)
                            {
                                lastSubFrame = startFrame;
                            }

                            // Calculate the currentFrame, which depends on the current
                            // frame in the parent animation
                            currentFrame.Y = lastSubFrame / sheetSize.X;
                            currentFrame.X = lastSubFrame % sheetSize.X;

                            // Move to the next Frame
                            lastSubFrame += 1;
                            // Loop the animation
                            if (lastSubFrame > endFrame)
                            {
                                lastSubFrame = startFrame;
                            }
                        }
                        else
                        {
                            // Do not advance frames before the first draw operation
                            if (drawWasAlreadyCalledOnce)
                            {
                                currentFrame.X++;
                                if (currentFrame.X >= sheetSize.X)
                                {
                                    currentFrame.X = 0;
                                    currentFrame.Y++;
                                }
                                if (currentFrame.Y >= sheetSize.Y)
                                    currentFrame.Y = 0;

                                if (lastSubFrame != -1)
                                {
                                    lastSubFrame = -1;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Render the animation.
        /// </summary>
        /// <param name="ScaledSpriteBatch">ScaledSpriteBatch with which the current 
        /// frame will be rendered.</param>
        /// <param name="position">The position to draw the current frame.</param>
        /// <param name="spriteEffect">SpriteEffect to apply to the 
        /// current frame.</param>
        public void Draw(ScaledSpriteBatch ScaledSpriteBatch, Vector2 position, SpriteEffects spriteEffect)
        {
            Draw(ScaledSpriteBatch, position, 1.0f, spriteEffect);
        }

        /// <summary>
        /// Render the animation.
        /// </summary>
        /// <param name="ScaledSpriteBatch">ScaledSpriteBatch with which the current frame
        /// will be rendered.</param>
        /// <param name="position">The position to draw the current frame.</param>
        /// <param name="scale">Scale factor to apply to the current frame.</param>
        /// <param name="spriteEffect">SpriteEffect to apply to the 
        /// current frame.</param>
        public void Draw(ScaledSpriteBatch ScaledSpriteBatch, Vector2 position, float scale, SpriteEffects spriteEffect)
        {
            drawWasAlreadyCalledOnce = true;

            ScaledSpriteBatch.Draw(animatedCharacter, position + Offset,
                new Rectangle(frameSize.X * currentFrame.X, frameSize.Y * currentFrame.Y, frameSize.X, frameSize.Y),
                Color.White, 0f, Vector2.Zero, scale, spriteEffect, 0);
        }

        /// <summary>
        /// Causes the animation to start playing from a specified frame index.
        /// </summary>
        /// <param name="frameIndex">Frame index to play the animation from.</param>
        public void PlayFromFrameIndex(int frameIndex)
        {
            FrameIndex = frameIndex;
            IsActive = true;
            drawWasAlreadyCalledOnce = false;
        }

        /// <summary>
        /// Sets the range of frames which serves as the sub animation.
        /// </summary>
        /// <param name="startFrame">Start frame for the sub-animation.</param>
        /// <param name="endFrame">End frame for the sub-animation.</param>
        public void SetSubAnimation(int startFrame, int endFrame)
        {
            this.startFrame = startFrame;
            this.endFrame = endFrame;
        }

        /// <summary>
        /// Used to set the interval between frames.
        /// </summary>
        /// <param name="interval">The interval between frames.</param>
        public void SetFrameInterval(TimeSpan interval)
        {
            timeInterval = interval;
        }


        #endregion
    }
}
