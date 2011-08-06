#region File Description
//-----------------------------------------------------------------------------
// Asteroid.cs
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
    /// Asteroids that fill the game world, blocking the player's shots and movements.
    /// </summary>
    public class Asteroid : GameplayObject
    {
        #region Constants


        /// <summary>
        /// The ratio of the mass of an asteroid to its radius.
        /// </summary>
        const float massRadiusRatio = 0.5f;

        /// <summary>
        /// The amount of drag applied to velocity per second, 
        /// as a percentage of velocity.
        /// </summary>
        const float dragPerSecond = 0.15f;

        /// <summary>
        /// Scalar to convert the velocity / mass ratio into a "nice" rotational value.
        /// </summary>
        const float velocityMassRatioToRotationScalar = 0.0017f;

        /// <summary>
        /// Scalar for calculated damage values that asteroids apply to players.
        /// </summary>
        const float momentumToDamageScalar = 0.007f;

        /// <summary>
        /// The number of variations in textures for asteroids.
        /// </summary>
        const int variations = 3;

        /// <summary>
        /// The minimum possible initial speed for asteroids.
        /// </summary>
        const float initialSpeedMinimum = 32f;

        /// <summary>
        /// The minimum possible initial speed for asteroids.
        /// </summary>
        const float initialSpeedMaximum = 96f;


        #endregion


        #region Static Graphics Data


        /// <summary>
        /// The asteroid textures.
        /// </summary>
        private static Texture2D[] textures = new Texture2D[variations];


        #endregion


        #region Graphics Data


        /// <summary>
        /// The variation of this particular asteroid.
        /// </summary>
        private int variation = 0;
        public int Variation
        {
            get { return variation; }
            set
            {
                if ((value < 0) || (value >= variations))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                variation = value;
            }
        }


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Construct a new asteroid.
        /// </summary>
        /// <param name="world">The world that this asteroid belongs to.</param>
        /// <param name="radius">The size of the asteroid.</param>
        public Asteroid(float radius)
            : base()
        {
            // safety-check the parameters
            if (radius <= 0f)
            {
                throw new ArgumentOutOfRangeException("radius");
            }

            // set the collision data
            this.radius = radius;
            this.mass = this.radius * massRadiusRatio;

            this.Velocity = RandomMath.RandomDirection() * 
                RandomMath.RandomBetween(initialSpeedMinimum, initialSpeedMaximum);
        }


        #endregion


        #region Updating Methods


        /// <summary>
        /// Update the asteroid.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public override void Update(float elapsedTime)
        {
            // spin the asteroid based on the radius and velocity
            float velocityMassRatio = (Velocity.LengthSquared() / Mass);
            rotation += velocityMassRatio * velocityMassRatioToRotationScalar * 
                elapsedTime;

            // apply some drag so the asteroids settle down
            Velocity -= Velocity * (elapsedTime * dragPerSecond);

            base.Update(elapsedTime);
        }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draw the asteroid.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="spriteBatch">The SpriteBatch object used to draw.</param>
        public void Draw(float elapsedTime, SpriteBatch spriteBatch)
        {
            base.Draw(elapsedTime, spriteBatch, textures[variation], null,
                Color.White);
        }


        #endregion


        #region Interaction Methods


        /// <summary>
        /// Defines the interaction between the asteroid and a target GameplayObject
        /// when they touch.
        /// </summary>
        /// <param name="target">The GameplayObject that is touching this one.</param>
        /// <returns>True if the objects meaningfully interacted.</returns>
        public override bool Touch(GameplayObject target)
        {
            // if the asteroid has touched a player, then damage it
            Ship player = target as Ship;
            if (player != null)
            {
                // calculate damage as a function of how much the two GameplayObject's
                // velocities were going towards one another
                Vector2 playerAsteroidVector = Position - player.Position;
                if (playerAsteroidVector.LengthSquared() > 0)
                {
                    playerAsteroidVector.Normalize();

                    float rammingSpeed =
                        Vector2.Dot(playerAsteroidVector, player.Velocity) -
                        Vector2.Dot(playerAsteroidVector, Velocity);
                    float momentum = Mass * rammingSpeed;
                    player.Damage(this, momentum * momentumToDamageScalar);
                }
            }
            // if the asteroid didn't hit a projectile, play the asteroid-touch sound effect
            if ((target is Projectile) == false)
            {
                AudioManager.PlaySoundEffect("asteroid_touch");
            }
            return true;
        }


        #endregion


        #region Static Graphics Methods


        /// <summary>
        /// The number of variations in asteroids.
        /// </summary>
        public static int Variations
        {
            get { return variations; }
        }
        
        
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

            // load each asteroid's texture
            for (int i = 0; i < variations; i++)
            {
                textures[i] = contentManager.Load<Texture2D>(
                    "Textures/asteroid" + i.ToString());
            }
        }

        /// <summary>
        /// Unload all of the static graphics content for this class.
        /// </summary>
        public static void UnloadContent()
        {
            for (int i = 0; i < variations; i++)
            {
                textures[i] = null;
            }
        }


        #endregion
    }
}
