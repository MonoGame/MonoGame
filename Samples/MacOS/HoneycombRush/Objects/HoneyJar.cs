#region File Description
//-----------------------------------------------------------------------------
// HoneyJar.cs
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
    /// A game component that represent the Honey Jar
    /// </summary>
    public class HoneyJar : TexturedDrawableGameComponent
    {
        #region Fields/Properties


        const string HoneyText = "Honey";

        ScoreBar score;
        SpriteFont font16px;
        Vector2 honeyTextSize;

        public bool CanCarryMore
        {
            get
            {
                return score.CurrentValue < score.MaxValue;
            }
        }

        public bool HasHoney
        {
            get
            {
                return score.CurrentValue > score.MinValue;
            }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new instance of the component.
        /// </summary>
        /// <param name="game">The associated game object.</param>
        /// <param name="gamePlayScreen">The gameplay screen where the component will be rendered.</param>
        /// <param name="position">The position of the component.</param>
        /// <param name="score">Scorebar representing the amount of honey in the jar.</param>
        public HoneyJar(Game game, GameplayScreen gamePlayScreen, Vector2 position, ScoreBar score)
            : base(game, gamePlayScreen)
        {
            this.position = position;
            this.score = score;
        }

        /// <summary>
        /// Loads the content used by the component.
        /// </summary>
        protected override void LoadContent()
        {
            font16px = Game.Content.Load<SpriteFont>("Fonts/GameScreenFont16px");
            texture = Game.Content.Load<Texture2D>("Textures/HoneyJar");
            honeyTextSize = font16px.MeasureString(HoneyText) * scaledSpriteBatch.ScaleVector;

            base.LoadContent();
        }


        #endregion

        #region Render


        /// <summary>
        /// Draws the component.
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
            scaledSpriteBatch.DrawString(font16px, HoneyText, position +
                new Vector2(Bounds.Width / 2 - honeyTextSize.X / 2, Bounds.Height * 4 / 3), Color.White);
            scaledSpriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Increases honey stored in the jar by the specified amount.
        /// </summary>
        /// <param name="value">The amount of honey to add to the jar.</param>
        public void IncreaseHoney(int value)
        {
            score.IncreaseCurrentValue(value);
        }

        /// <summary>
        /// Decreases honey stored in the jar by the specified amount.
        /// </summary>
        /// <param name="value">The amount of honey to remove from the jar.</param>
        public void DecreaseHoney(int value)
        {
            score.DecreaseCurrentValue(value);
        }

        /// <summary>
        /// Decrease the amount of honey in the jar by a specified percent of the jar's total capacity.
        /// </summary>
        /// <param name="percent">The percent of the jar's capacity by which to decrease the current amount
        /// of honey. If the jar's capacity is 100 and this value is 20, then the amount of honey will be reduced
        /// by 20.</param>
        public int DecreaseHoneyByPercent(int percent)
        {
            return score.DecreaseCurrentValue(percent * score.MaxValue / 100, true);
        }


        #endregion
    }
}
