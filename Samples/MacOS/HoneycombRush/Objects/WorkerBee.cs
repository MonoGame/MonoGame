#region File Description
//-----------------------------------------------------------------------------
// WorkerBee.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#endregion

namespace HoneycombRush
{
    /// <summary>
    /// A component that represents a worker bee.
    /// </summary>
    public class WorkerBee : Bee
    {
        #region Properties


        protected override int MaxVelocity
        {
            get
            {
                var configuration = ConfigurationManager.ModesConfiguration[ConfigurationManager.DifficultyMode.Value];

                return (int)configuration.MaxWorkerBeeVelocity;
            }
        }

        protected override float AccelerationFactor
        {
            get
            {
                return 1.5f;
            }
        }


        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new worker bee instance.
        /// </summary>
        /// <param name="game">The associated game object.</param>
        /// <param name="gamePlayScreen">The gameplay screen where the bee is displayed</param>
        /// <param name="beehive">The bee's associated beehive.</param>
        public WorkerBee(Game game, GameplayScreen gamePlayScreen, Beehive beehive)
            : base(game, gamePlayScreen, beehive)
        {
            AnimationKey = "WorkerBee";
        }

        protected override void LoadContent()
        {
            texture = Game.Content.Load<Texture2D>("Textures/beeWingFlap");

            base.LoadContent();
        }


        #endregion
    }
}
