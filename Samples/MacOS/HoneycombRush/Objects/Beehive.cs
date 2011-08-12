#region File Description
//-----------------------------------------------------------------------------
// Beehive.cs
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
    /// Represent a single beehive
    /// </summary>
    public class Beehive : TexturedDrawableGameComponent
    {
        #region Fields/Properties


        ScoreBar score;
        TimeSpan intervalToAddHoney = TimeSpan.FromMilliseconds(600);
        TimeSpan lastTimeHoneyAdded;

        bool allowBeesToGenerate = true;
        public bool AllowBeesToGenerate
        {
            get
            {
                return allowBeesToGenerate;
            }
            set
            {
                allowBeesToGenerate = value;
            }
        }

        public Boolean HasHoney
        {
            get
            {
                return score.CurrentValue > score.MinValue;
            }
        }

        public override Rectangle Bounds
        {
            get
            {
                Rectangle baseBounds = base.Bounds;
                int widthMargin = baseBounds.Width / 10;
                int width = baseBounds.Width - widthMargin;
                int height = baseBounds.Height / 3;

                return new Rectangle(baseBounds.X + widthMargin, baseBounds.Y + height, width - widthMargin, height);
            }
        }

        public override Rectangle CentralCollisionArea
        {
            get
            {
                Rectangle bounds = Bounds;
                int height = (int)Bounds.Height / 10 * 5;
                int width = (int)Bounds.Width / 10 * 4;

                int offsetY = ((int)Bounds.Height - height) / 2;
                int offsetX = ((int)Bounds.Width - width) / 2;

                return new Rectangle((int)Bounds.X + offsetX, (int)Bounds.Y + offsetY, width, height);
            }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new beehive instance.
        /// </summary>
        /// <param name="game">The game object.</param>
        /// <param name="gamePlayScreen">The gameplay screen.</param>
        /// <param name="texture">The texture representing the beehive.</param>
        /// <param name="score">Score object representing the amount of honey in the
        /// hive.</param>
        /// <param name="position">The beehive's position.</param>
        public Beehive(Game game, GameplayScreen gamePlayScreen, Texture2D texture, ScoreBar score, Vector2 position)
            : base(game, gamePlayScreen)
        {
            this.texture = texture;
            this.score = score;
            this.position = position;

            AllowBeesToGenerate = true;

            DrawOrder = (int)position.Y;
        }


        #endregion

        #region Update


        /// <summary>
        /// Updates the beehive's status.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Update(GameTime gameTime)
        {
            if (!gamePlayScreen.IsActive)
            {
                base.Update(gameTime);
                return;
            }

            // Initialize the first time honey was added
            if (lastTimeHoneyAdded == TimeSpan.Zero)
            {
                lastTimeHoneyAdded = gameTime.TotalGameTime;
                score.IncreaseCurrentValue(1);
            }
            else
            {
                // If enough time has passed add more honey
                if (lastTimeHoneyAdded + intervalToAddHoney < gameTime.TotalGameTime)
                {
                    lastTimeHoneyAdded = gameTime.TotalGameTime;
                    score.IncreaseCurrentValue(1);
                }
            }

            base.Update(gameTime);
        }


        #endregion

        #region Render


        /// <summary>
        /// Render the beehive.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            if (!gamePlayScreen.IsActive)
            {
                base.Draw(gameTime);
                return;
            }

            scaledSpriteBatch.Begin();            
            scaledSpriteBatch.Draw(texture, position, Color.White);
            scaledSpriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Public methods


        public void DecreaseHoney(int amount)
        {
            score.DecreaseCurrentValue(amount);
        }


        #endregion
    }
}
