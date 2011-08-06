#region File Description
//-----------------------------------------------------------------------------
// LaserWeapon.cs
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
    /// A weapon that shoots a single stream of laser projectiles.
    /// </summary>
    public class LaserWeapon : Weapon
    {
        #region Initialization Methods

        /// <summary>
        /// Constructs a new laser weapon.
        /// </summary>
        /// <param name="owner">The ship that owns this weapon.</param>
        public LaserWeapon(Ship owner)
            : base(owner)
        {
            fireDelay = 0.15f;

            // Pick one of the laser sound variations for this instance.
            switch (RandomMath.Random.Next(3))
            {
                case 0:
                    fireSoundEffect = "fire_laser1";
                    break;
                case 1:
                    fireSoundEffect = "fire_laser2";
                    break;
                case 2:
                    fireSoundEffect = "fire_laser3";
                    break;
            }
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
            LaserProjectile projectile = new LaserProjectile(owner, direction);
            projectile.Initialize();
            owner.Projectiles.Add(projectile);
        }


        #endregion
    }
}
