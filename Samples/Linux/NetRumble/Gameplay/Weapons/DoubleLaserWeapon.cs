#region File Description
//-----------------------------------------------------------------------------
// DoubleLaserWeapon.cs
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
    /// A weapon that shoots a double stream of laser projectiles.
    /// </summary>
    public class DoubleLaserWeapon : LaserWeapon
    {
        #region Constants


        /// <summary>
        /// The distance that the laser bolts are moved off of the owner's position.
        /// </summary>
        const float laserSpread = 8f;


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Constructs a new double-laser weapon.
        /// </summary>
        /// <param name="owner">The ship that owns this weapon.</param>
        public DoubleLaserWeapon(Ship owner)
            : base(owner) { }


        #endregion


        #region Interaction Methods


        /// <summary>
        /// Create and spawn the projectile(s) from a firing from this weapon.
        /// </summary>
        /// <param name="direction">The direction that the projectile will move.</param>
        protected override void CreateProjectiles(Vector2 direction)
        {
            // calculate the spread of the laser bolts
            Vector2 cross = Vector2.Multiply(new Vector2(-direction.Y, direction.X),
                laserSpread);

            // create the new projectile
            LaserProjectile projectile = new LaserProjectile(owner,
                direction);
            projectile.Initialize();
            owner.Projectiles.Add(projectile);
            // adjust the position for the laser spread
            projectile.Position += cross;

            // create the second projectile
            projectile = new LaserProjectile(owner, direction);
            projectile.Initialize();
            owner.Projectiles.Add(projectile);
            // adjust the position for the laser spread
            projectile.Position -= cross;
        }


        #endregion
    }
}
