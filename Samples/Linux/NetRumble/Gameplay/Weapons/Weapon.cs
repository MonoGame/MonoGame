#region File Description
//-----------------------------------------------------------------------------
// Weapon.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace NetRumble
{
    /// <summary>
    /// Base public class for all weapons that exist in the game.
    /// </summary>
    abstract public class Weapon
    {
        #region Gameplay Data


        /// <summary>
        /// The ship that owns this weapon.
        /// </summary>
        protected Ship owner = null;

        /// <summary>
        /// The amount of time remaining before this weapon can fire again.
        /// </summary>
        protected float timeToNextFire = 0f;

        /// <summary>
        /// The minimum amount of time between each firing of this weapon.
        /// </summary>
        protected float fireDelay = 0f;


        #endregion


        #region Audio Data


        /// <summary>
        /// The name of the sound effect played when this weapon fires.
        /// </summary>
        protected string fireSoundEffect = String.Empty;


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Constructs a new weapon.
        /// </summary>
        /// <param name="owner">The ship that owns this weapon.</param>
        protected Weapon(Ship owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            this.owner = owner;
        }


        #endregion


        #region Updating Methods


        /// <summary>
        /// Update the weapon.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public virtual void Update(float elapsedTime)
        {
            // count down to when the weapon can fire again
            if (timeToNextFire > 0f)
            {
                timeToNextFire = MathHelper.Max(timeToNextFire - elapsedTime, 0f);
            }
        }


        #endregion


        #region Interaction Methods


        /// <summary>
        /// Fire the weapon in the direction given.
        /// </summary>
        /// <param name="direction">The direction that the weapon is firing in.</param>
        public virtual void Fire(Vector2 direction)
        {
            // if we can't fire yet, then we're done
            if (timeToNextFire > 0f)
            {
                return;
            }

            // the owner is no longer safe from damage
            owner.Safe = false;

            // set the timer
            timeToNextFire = fireDelay;

            // create and spawn the projectile
            CreateProjectiles(direction);

            // play the sound effect for firing
            if (String.IsNullOrEmpty(fireSoundEffect) == false)
            {
                AudioManager.PlaySoundEffect(fireSoundEffect);
            }
        }


        /// <summary>
        /// Create and spawn the projectile(s) from a firing from this weapon.
        /// </summary>
        /// <param name="direction">The direction that the projectile will move.</param>
        protected abstract void CreateProjectiles(Vector2 direction);


        #endregion
    }
}
