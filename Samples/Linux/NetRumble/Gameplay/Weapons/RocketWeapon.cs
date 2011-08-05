#region File Description
//-----------------------------------------------------------------------------
// RocketWeapon.cs
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
    /// A weapon that shoots a rockets.
    /// </summary>
    public class RocketWeapon : Weapon
    {
        #region Initialization Methods


        /// <summary>
        /// Constructs a new rocket-launching weapon.
        /// </summary>
        /// <param name="owner">The ship that owns this weapon.</param>
        public RocketWeapon(Ship owner)
            : base(owner)
        {
            fireDelay = 0.5f;

            // Pick one of the rocket sound variations for this instance.
            if (RandomMath.Random.Next(2) == 0)
                fireSoundEffect = "fire_rocket1";
            else
                fireSoundEffect = "fire_rocket2";
        }


        #endregion


        #region Interaction Methods


        /// <summary>
        /// Create and spawn the projectile(s) from a firing from this weapon.
        /// </summary>
        /// <param name="direction">The direction that the projectile will move.</param>
        protected override void CreateProjectiles(Vector2 direction)
        {
            // create the new projectile
            RocketProjectile projectile = new RocketProjectile(owner, direction);
            projectile.Initialize();
            owner.Projectiles.Add(projectile);
        }


        #endregion
    }
}
