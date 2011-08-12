#region File Description
//-----------------------------------------------------------------------------
// AnimatedGameComponent.cs
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
using CardsFramework;
#endregion

namespace CardsFramework
{
    /// <summary>
    /// A game component.
    /// Enable variable display while managing and displaying a set of
    /// <see cref="AnimatedGameComponentAnimation">Animations</see>
    /// </summary>
    public class AnimatedGameComponent : DrawableGameComponent
    {
        #region Fields and Properties

        public Texture2D CurrentFrame { get; set; }
        public Rectangle? CurrentSegment { get; set; }
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public bool IsFaceDown = true;
        public Vector2 CurrentPosition { get; set; }
        public Rectangle? CurrentDestination { get; set; }

        List<AnimatedGameComponentAnimation> runningAnimations =
            new List<AnimatedGameComponentAnimation>();

        /// <summary>
        /// Whether or not an animation belonging to the component is running.
        /// </summary>
        public virtual bool IsAnimating { get { return runningAnimations.Count > 0; } }

        public CardsGame CardGame { get; private set; }
        #endregion

        #region Initializatios
        /// <summary>
        /// Initializes a new instance of the class, using black text color.
        /// </summary>
        /// <param name="game">The associated game class.</param>
        public AnimatedGameComponent(Game game)
            : base(game)
        {
            TextColor = Color.Black;
        }

        /// <summary>
        /// Initializes a new instance of the class, using black text color.
        /// </summary>
        /// <param name="game">The associated game class.</param>
        /// <param name="currentFrame">The texture serving as the current frame
        /// to display as the component.</param>
        public AnimatedGameComponent(Game game, Texture2D currentFrame)
            : this(game)
        {
            CurrentFrame = currentFrame;
        }

        /// <summary>
        /// Initializes a new instance of the class, using black text color.
        /// </summary>
        /// <param name="cardGame">The associated card game.</param>
        /// <param name="currentFrame">The texture serving as the current frame
        /// to display as the component.</param>
        public AnimatedGameComponent(CardsGame cardGame, Texture2D currentFrame)
            : this(cardGame.Game)
        {
            CardGame = cardGame;
            CurrentFrame = currentFrame;
        }
        #endregion

        #region Update and Render
        /// <summary>
        /// Keeps track of the component's animations.
        /// </summary>
        /// <param name="gameTime">The time which as elapsed since the last call
        /// to this method.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int animationIndex = 0; animationIndex < runningAnimations.Count; animationIndex++)
            {
                runningAnimations[animationIndex].AccumulateElapsedTime(gameTime.ElapsedGameTime);
                runningAnimations[animationIndex].Run(gameTime);
                if (runningAnimations[animationIndex].IsDone())
                {
                    runningAnimations.RemoveAt(animationIndex);
                    animationIndex--;
                }

            }
        }

        /// <summary>
        /// Draws the animated component and its associated text, if it exists, at
        /// the object's set destination. If a destination is not set, its initial
        /// position is used.
        /// </summary>
        /// <param name="gameTime">The time which as elapsed since the last call
        /// to this method.</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch;

            if (CardGame != null)
            {
                spriteBatch = CardGame.SpriteBatch;
            }
            else
            {
                spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            }

            spriteBatch.Begin();

            // Draw at the destination if one is set
            if (CurrentDestination.HasValue)
            {
                if (CurrentFrame != null)
                {
                    spriteBatch.Draw(CurrentFrame, CurrentDestination.Value, CurrentSegment, Color.White);
                    if (Text != null)
                    {
                        Vector2 size = CardGame.Font.MeasureString(Text);
                        Vector2 textPosition = new Vector2(CurrentDestination.Value.X +
                            CurrentDestination.Value.Width / 2 - size.X / 2,
                            CurrentDestination.Value.Y + CurrentDestination.Value.Height / 2 - size.Y / 2);

                        spriteBatch.DrawString(CardGame.Font, Text, textPosition, TextColor);
                    }
                }
            }
            // Draw at the component's position if there is no destination
            else
            {
                if (CurrentFrame != null)
                {
                    spriteBatch.Draw(CurrentFrame, CurrentPosition, CurrentSegment, Color.White);
                    if (Text != null)
                    {
                        Vector2 size = CardGame.Font.MeasureString(Text);
                        Vector2 textPosition = new Vector2(CurrentPosition.X +
                            CurrentFrame.Bounds.Width / 2 - size.X / 2,
                            CurrentPosition.Y + CurrentFrame.Bounds.Height / 2 - size.Y / 2);

                        spriteBatch.DrawString(CardGame.Font, Text, textPosition, TextColor);
                    }
                }
            }

            spriteBatch.End();
        }
        #endregion

        /// <summary>
        /// Adds an animation to the animated component.
        /// </summary>
        /// <param name="animation">The animation to add.</param>
        public void AddAnimation(AnimatedGameComponentAnimation animation)
        {
            animation.Component = this;
            runningAnimations.Add(animation);
        }

        /// <summary>
        /// Calculate the estimated time at which the longest lasting animation currently managed 
        /// will complete.
        /// </summary>
        /// <returns>The estimated time for animation complete </returns>
        public virtual TimeSpan EstimatedTimeForAnimationsCompletion()
        {
            TimeSpan result = TimeSpan.Zero;

            if (IsAnimating)
            {
                for (int animationIndex = 0; animationIndex < runningAnimations.Count; animationIndex++)
                {
                    if (runningAnimations[animationIndex].EstimatedTimeForAnimationCompletion > result)
                    {
                        result = runningAnimations[animationIndex].EstimatedTimeForAnimationCompletion;
                    }
                }
            }

            return result;
        }
    }
}
