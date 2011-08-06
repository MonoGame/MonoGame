#region File Description
//-----------------------------------------------------------------------------
// TripleLaserWeapon.cs
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
    /// A weapon that shoots a triple stream of laser projectiles.
    /// </summary>
    public class TripleLaserWeapon : LaserWeapon
    {
        #region Constants


        /// <summary>
        /// The spread of the second and third laser projectiles' directions, in radians
        /// </summary>
        static readonly float laserSpreadRadians = MathHelper.ToRadians(2.5f);


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Constructs a new triple-laser weapon.
        /// </summary>
        /// <param name="owner">The ship that owns this weapon.</param>
        public TripleLaserWeapon(Ship owner)
            : base(owner) 
        {
            fireDelay = 0.3f;
        }


        #endregion


        #region Interaction Methods


        /// <summary>
        /// Create and spawn the projectile(s) from a firing from this weapon.
        /// </summary>
        /// <param name="direction">The direction that the projectile will move.</param>
        protected override void CreateProjectiles(Vector2 direction)
        {
            // calculate the direction vectors for the second and third projectiles
            float rotation = (float)Math.Acos(Vector2.Dot(new Vector2(0f, -1f), 
                direction));
            rotation *= (Vector2.Dot(new Vector2(0f, -1f), 
                new Vector2(direction.Y, -direction.X)) > 0f) ? 1f : -1f;
            Vector2 direction2 = new Vector2(
                 (float)Math.Sin(rotation - laserSpreadRadians), 
                -(float)Math.Cos(rotation - laserSpreadRadians));
            Vector2 direction3 = new Vector2(
                 (float)Math.Sin(rotation + laserSpreadRadians), 
                -(float)Math.Cos(rotation + laserSpreadRadians));

            // create the first projectile
            LaserProjectile projectile = new LaserProjectile(owner,
                direction);
            projectile.Initialize();
            owner.Projectiles.Add(projectile);

            // create the second projectile
            projectile = new LaserProjectile(owner, direction2);
            projectile.Initialize();
            owner.Projectiles.Add(projectile);

            // create the third projectile
            projectile = new LaserProjectile(owner, direction3);
            projectile.Initialize();
            owner.Projectiles.Add(projectile);
        }


        #endregion
    }
}
