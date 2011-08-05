#region File Description
//-----------------------------------------------------------------------------
// MineWeapon.cs
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
    /// A weapon that fires a single mine on a long timer.
    /// </summary>
    public class MineWeapon : Weapon
    {
        #region Constants


        /// <summary>
        /// The distance that the mine spawns behind the ship.
        /// </summary>
        const float mineSpawnDistance = 8f;


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Constructs a new mine-laying weapon.
        /// </summary>
        /// <param name="owner">The ship that owns this weapon.</param>
        public MineWeapon(Ship owner)
            : base(owner)
        {
            fireDelay = 2f;         
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
            MineProjectile projectile = new MineProjectile(owner, direction);
            projectile.Initialize();
            owner.Projectiles.Add(projectile);

            // move the mine out from the ship
            projectile.Position = owner.Position +
                direction * (owner.Radius + projectile.Radius + mineSpawnDistance);
        }


        #endregion
    }
}
