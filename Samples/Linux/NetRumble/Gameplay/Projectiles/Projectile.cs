#region File Description
//-----------------------------------------------------------------------------
// Projectile.cs
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
    /// Base public class for all projectiles that exist in the game.
    /// </summary>
    abstract public class Projectile : GameplayObject
    {
        #region Gameplay Data


        /// <summary>
        /// The player who fired this projectile.
        /// </summary>
        protected Ship owner;
        public Ship Owner
        {
            get { return owner; }
        }

        /// <summary>
        /// The amount that this projectile hurts it's target and those around it.
        /// </summary>
        protected float damageAmount = 0f;

        /// <summary>
        /// The radius at which this projectile hurts others when it explodes.
        /// </summary>
        protected float damageRadius = 0f;

        /// <summary>
        /// If true, this object will damage it's owner if it hits it
        /// </summary>
        protected bool damageOwner = true;

        /// <summary>
        /// The amount of time before this projectile dies on it's own.
        /// </summary>
        protected float duration = 0f;


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Constructs a new projectile.
        /// </summary>
        /// <param name="owner">The ship that fired this projectile.</param>
        /// <param name="direction">The initial direction for this projectile.</param>
        protected Projectile(Ship owner, Vector2 direction)
            : base() 
        {
            // safety-check the parameter
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            // apply the parameters
            this.owner = owner;
            this.velocity = direction; // speed will be applied in the subclass

            // initialize the graphics data
            this.position = owner.Position;
            this.rotation = (float)Math.Acos(Vector2.Dot(Vector2.UnitY, direction));
            if (direction.X > 0f)
            {
                this.rotation *= -1f;
            }
        }


        #endregion


        #region Updating Methods


        /// <summary>
        /// Update the projectile.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public override void Update(float elapsedTime)
        {
            // projectiles can "time out"
            if (duration > 0f)
            {
                duration -= elapsedTime;
                if (duration < 0f)
                {
                    Die(null, false);
                }
            }

            base.Update(elapsedTime);
        }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draw the projectile.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="spriteBatch">The SpriteBatch object used to draw.</param>
        public abstract void Draw(float elapsedTime, SpriteBatch spriteBatch);


        #endregion


        #region Interaction


        /// <summary>
        /// Defines the interaction between this projectile and a target GameplayObject
        /// when they touch.
        /// </summary>
        /// <param name="target">The GameplayObject that is touching this one.</param>
        /// <returns>True if the objects meaningfully interacted.</returns>
        public override bool Touch(GameplayObject target)
        {
            // check the target, if we have one
            if (target != null)
            {
                // don't bother hitting any power-ups
                if (target is PowerUp)
                {
                    return false;
                }
                // don't hit the owner if the damageOwner flag isn't set
                if ((this.damageOwner == false) && (target == owner))
                {
                    return false;
                }
                // don't hit other projectiles from the same ship
                Projectile projectile = target as Projectile;
                if ((projectile != null) && (projectile.Owner == this.Owner))
                {
                    return false;
                }
                // damage the target
                target.Damage(this, this.damageAmount);
            }

            // either we hit something or the target is null - in either case, die
            Die(target, false);
            
            return base.Touch(target);
        }
        
        /// <summary>
        /// Kills this projectile, in response to the given GameplayObject.
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
                    CollisionManager.Explode(this, source, damageAmount, Position,
                        damageRadius, damageOwner);
                }
            }

            base.Die(source, cleanupOnly);
        }


        #endregion
    }
}
