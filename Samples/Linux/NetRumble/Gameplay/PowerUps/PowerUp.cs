#region File Description
//-----------------------------------------------------------------------------
// PowerUp.cs
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
    /// Base public class for all power-ups that exist in the game.
    /// </summary>
    abstract public class PowerUp : GameplayObject
    {
        #region Constant Data


        /// <summary>
        /// The speed of the rotation of the power-up, in radians/sec.
        /// </summary>
        const float rotationSpeed = 2f;

        /// <summary>
        /// The amplitude of the pulse
        /// </summary>
        const float pulseAmplitude = 0.1f;

        /// <summary>
        /// The rate of the pulse.
        /// </summary>
        const float pulseRate = 0.1f;


        public const float PowerUpRadius = 20f;

        #endregion


        #region Graphics Data


        /// <summary>
        /// The time accumulator for the power-up pulse.
        /// </summary>
        private float pulseTime = 0f;


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Constructs a new power-up.
        /// </summary>
        protected PowerUp() 
            : base() 
        {
            // set the collision data
            this.radius = PowerUpRadius;
            this.mass = Int32.MaxValue;
        }


        /// <summary>
        /// Initialize the power-up to it's default gameplay states.
        /// </summary>
        public override void Initialize()
        {
            if (!active)
            {
                // play the spawn sound effect
                AudioManager.PlaySoundEffect("powerup_spawn");
            }

            base.Initialize();
        }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draw the triple-laser power-up.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="spriteBatch">The SpriteBatch object used to draw.</param>
        public abstract void Draw(float elapsedTime, SpriteBatch spriteBatch);
        
        
        /// <summary>
        /// Draw the power-up.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="spriteBatch">The SpriteBatch object used to draw.</param>
        /// <param name="sprite">The texture used to draw this object.</param>
        /// <param name="sourceRectangle">The source rectangle in the texture.</param>
        /// <param name="color">The color of the sprite, ignored here.</param>
        public override void Draw(float elapsedTime, SpriteBatch spriteBatch, 
            Texture2D sprite, Rectangle? sourceRectangle, Color color)
        {
            // update the rotation
            rotation += rotationSpeed * elapsedTime;

            // adjust the radius to affect the scale
            float oldRadius = radius;
            pulseTime += elapsedTime;
            radius *= 1f + pulseAmplitude * (float)Math.Sin(pulseTime / pulseRate);
            base.Draw(elapsedTime, spriteBatch, sprite, sourceRectangle, color);
            radius = oldRadius;
        }


        #endregion


        #region Interaction Methods


        /// <summary>
        /// Defines the interaction between this power-up and a target GameplayObject
        /// when they touch.
        /// </summary>
        /// <param name="target">The GameplayObject that is touching this one.</param>
        /// <returns>True if the objects meaningfully interacted.</returns>
        public override bool Touch(GameplayObject target)
        {
            // if it touched a ship, then create a particle system and play a sound
            Ship ship = target as Ship;
            if (ship != null)
            {
                // play the "power-up picked up" sound effect
                AudioManager.PlaySoundEffect("powerup_touch");

                // kill the power-up
                Die(target, false);

                // the ship keeps going as if it didn't hit anything
                return false;
            }

            return base.Touch(target);
        }


        #endregion
    }
}
