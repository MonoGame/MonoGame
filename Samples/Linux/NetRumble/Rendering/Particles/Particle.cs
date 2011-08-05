#region File Description
//-----------------------------------------------------------------------------
// Particle.cs
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

namespace NetRumble
{
    /// <summary>
    /// A single particle in a particle-based effect.
    /// </summary>
    public class Particle
    {
        #region Status Data
        

        /// <summary>
        /// The time remaining for this particle.
        /// </summary>
        public float TimeRemaining;


        #endregion


        #region Graphics Data


        /// <summary>
        /// The position of this particle.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The velocity of this particle.
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// The acceleration of this particle.
        /// </summary>
        public Vector2 Acceleration;

        /// <summary>
        /// The scale applied to this particle.
        /// </summary>
        public float Scale = 1f;

        /// <summary>
        /// The rotation applied to this particle.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// The opacity of the particle.
        /// </summary>
        public float Opacity = 1f;


        #endregion


        #region Updating Methods


        /// <summary>
        /// Update the particle.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="angularVelocity">The angular velocity of the particle.</param>
        /// <param name="scaleDeltaPerSecond">The change in the scale</param>
        /// <param name="opacityDeltaPerSecond">The change in the opacity.</param>
        public void Update(float elapsedTime, float angularVelocity, 
            float scaleDeltaPerSecond, float opacityDeltaPerSecond)
        {
            Velocity.X += Acceleration.X * elapsedTime;
            Velocity.Y += Acceleration.Y * elapsedTime;

            Position.X += Velocity.X * elapsedTime;
            Position.Y += Velocity.Y * elapsedTime;

            Rotation += angularVelocity * elapsedTime;
            Scale += scaleDeltaPerSecond * elapsedTime;
            if (Scale < 0f)
            {
                Scale = 0f;
            }

            Opacity = MathHelper.Clamp(Opacity + opacityDeltaPerSecond * elapsedTime,
                0f, 1f);
        }


        #endregion
    }
}
