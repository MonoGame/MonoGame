#region File Description
//-----------------------------------------------------------------------------
// MineProjectile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace NetRumble
{
    /// <summary>
    /// A mine projectile.
    /// </summary>
    public class MineProjectile : Projectile
    {
        #region Constants


        /// <summary>
        /// The initial speed of this projectile.
        /// </summary>
        const float initialSpeed = 64f;

        /// <summary>
        /// The amount of drag applied to velocity per second, 
        /// as a percentage of velocity.
        /// </summary>
        const float dragPerSecond = 0.9f;

        /// <summary>
        /// The radians-per-second that this object rotates at.
        /// </summary>
        const float rotationRadiansPerSecond = 1f;


        #endregion


        #region Static Graphics Data


        /// <summary>
        /// Texture for all mine projectiles.
        /// </summary>
        private static Texture2D texture;


        /// <summary>
        /// The particle-effect manager which recieves the effects from mines.
        /// </summary>
        public static ParticleEffectManager ParticleEffectManager;


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new mine projectile.
        /// </summary>
        /// <param name="owner">The ship that fired this projectile, if any.</param>
        /// <param name="direction">The initial direction for this projectile.</param>
        public MineProjectile(Ship owner, Vector2 direction)
            : base(owner, direction)
        {
            // set the gameplay data
            this.velocity = initialSpeed * direction;

            // set the collision data
            this.radius = 10f;
            this.mass = 5f;

            // set projectile data
            this.duration = 15f;
            this.damageAmount = 200f;
            this.damageOwner = false;
            this.damageRadius = 80f;
        }


        #endregion


        #region Updating Methods


        /// <summary>
        /// Update the mine.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public override void Update(float elapsedTime)
        {
            base.Update(elapsedTime);

            this.velocity -= velocity * (elapsedTime * dragPerSecond);
            this.rotation += elapsedTime * rotationRadiansPerSecond;
        }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draw the mine.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="spriteBatch">The SpriteBatch object used to draw.</param>
        public override void Draw(float elapsedTime, SpriteBatch spriteBatch)
        {
            // ignore the parameter color if we have an owner
            base.Draw(elapsedTime, spriteBatch, texture, null, Color.White);
        }


        #endregion


        #region Interaction Methods


        /// <summary>
        /// Kills this object, in response to the given GameplayObject.
        /// </summary>
        /// <param name="source">The GameplayObject responsible for the kill.</param>
        /// <param name="cleanupOnly">
        /// If true, the object dies without any further effects.
        /// </param>
        public override void Die(GameplayObject source, bool cleanupOnly)
        {
            if (active)
            {
                if (!cleanupOnly)
                {
                    // play the explosion sound effect
                    AudioManager.PlaySoundEffect("explosion_large");

                    // play the mine particle-effect
                    if (ParticleEffectManager != null)
                    {
                        ParticleEffectManager.SpawnEffect(
                            ParticleEffectType.MineExplosion, Position);
                    }
                }
            }

            base.Die(source, cleanupOnly);
        }


        #endregion


        #region Static Graphics Methods


        /// <summary>
        /// Load all of the static graphics content for this class.
        /// </summary>
        /// <param name="contentManager">The content manager to load with.</param>
        public static void LoadContent(ContentManager contentManager)
        {
            // safety-check the parameters
            if (contentManager == null)
            {
                throw new ArgumentNullException("contentManager");
            }

            // load the texture
            texture = contentManager.Load<Texture2D>("Textures/mine");
        }

        /// <summary>
        /// Unload all of the static graphics content for this class.
        /// </summary>
        public static void UnloadContent()
        {
            texture = null;
        }


        #endregion
    }
}
