#region File Description
//-----------------------------------------------------------------------------
// RocketProjectile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
#endregion

namespace NetRumble
{
    /// <summary>
    /// A rocket projectile.
    /// </summary>
    public class RocketProjectile : Projectile
    {
        #region Constants


        /// <summary>
        /// The initial speed of the rocket.
        /// </summary>
        const float initialSpeed = 650f;


        #endregion


        #region Static Graphics Data


        /// <summary>
        /// Texture for all rocket projectiles.
        /// </summary>
        private static Texture2D texture;


        /// <summary>
        /// The particle-effect manager which recieves the effects from rockets.
        /// </summary>
        public static ParticleEffectManager ParticleEffectManager;


        #endregion


        #region Graphics Data


        /// <summary>
        /// The trailing effect behind a rocket.
        /// </summary>
        protected ParticleEffect rocketTrailEffect = null;


        #endregion


        #region Audio Data


        /// <summary>
        /// The sound effect of the rocket as it flies.
        /// </summary>
        protected SoundEffectInstance rocketSound = null;


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new rocket projectile.
        /// </summary>
        /// <param name="owner">The ship that fired this projectile, if any.</param>
        /// <param name="direction">The initial direction for this projectile.</param>
        public RocketProjectile(Ship owner, Vector2 direction)
            : base(owner, direction)
        {
            // set the gameplay data
            this.velocity = initialSpeed * direction;

            // set the collision data
            this.radius = 14f;
            this.mass = 10f;

            // set the projectile data
            this.duration = 4f;
            this.damageAmount = 150f;
            this.damageOwner = false;
            this.damageRadius = 128f;
            this.rotation += MathHelper.Pi;
        }


        /// <summary>
        /// Initialize the rocket projectile to it's default gameplay states.
        /// </summary>
        public override void Initialize()
        {
            if (!active)
            {
                // get and play the rocket-flying sound effect

                AudioManager.PlaySoundEffect("rocket", true, out rocketSound);

                // start the rocket-trail effect
                if (ParticleEffectManager != null)
                {
                    rocketTrailEffect = ParticleEffectManager.SpawnEffect(
                        ParticleEffectType.RocketTrail, this);
                }
            }
            
            base.Initialize();
        }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draw the rocket.
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
                    // play the explosion sound
                    AudioManager.PlaySoundEffect("explosion_medium");

                    // display the rocket explosion
                    if (ParticleEffectManager != null)
                    {
                        ParticleEffectManager.SpawnEffect(
                            ParticleEffectType.RocketExplosion, Position);
                    }
                }

                // stop the rocket-flying sound effect
                if (rocketSound != null)
                {
                    rocketSound.Stop(true);
                    rocketSound.Dispose();
                    rocketSound = null;
                }

                // stop the rocket-trail effect
                if (rocketTrailEffect != null)
                {
                    rocketTrailEffect.Stop(false);
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
            texture = contentManager.Load<Texture2D>("Textures/rocket");
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
