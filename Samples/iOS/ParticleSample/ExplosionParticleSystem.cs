#region File Description
//-----------------------------------------------------------------------------
// ExplosionParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

#endregion

namespace ParticleSample
{
    /// <summary>
    /// ExplosionParticleSystem is a specialization of ParticleSystem which creates a
    /// fiery explosion. It should be combined with ExplosionSmokeParticleSystem for
    /// best effect.
    /// </summary>
    public class ExplosionParticleSystem : ParticleSystem
    {
        public ExplosionParticleSystem(ParticleSampleGame game, int howManyEffects)
            : base(game, howManyEffects)
        {
        }

        /// <summary>
        /// Set up the constants that will give this particle system its behavior and
        /// properties.
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "explosion";

            // high initial speed with lots of variance.  make the values closer
            // together to have more consistently circular explosions.
            minInitialSpeed = 40;
            maxInitialSpeed = 500;

            // doesn't matter what these values are set to, acceleration is tweaked in
            // the override of InitializeParticle.
            minAcceleration = 0;
            maxAcceleration = 0;

            // explosions should be relatively short lived
            minLifetime = .5f;
            maxLifetime = 1.0f;

            minScale = .3f;
            maxScale = 1.0f;

            minNumParticles = 20;
            maxNumParticles = 25;

            minRotationSpeed = -MathHelper.PiOver4;
            maxRotationSpeed = MathHelper.PiOver4;

            // additive blending is very good at creating fiery effects.
            spriteBlendMode = BlendState.Additive;
			
            DrawOrder = AdditiveDrawOrder;
        }

        protected override void InitializeParticle(Particle p, Vector2 where)
        {
            base.InitializeParticle(p, where);
            
            // The base works fine except for acceleration. Explosions move outwards,
            // then slow down and stop because of air resistance. Let's change
            // acceleration so that when the particle is at max lifetime, the velocity
            // will be zero.

            // We'll use the equation vt = v0 + (a0 * t). (If you're not familar with
            // this, it's one of the basic kinematics equations for constant
            // acceleration, and basically says:
            // velocity at time t = initial velocity + acceleration * t)
            // We'll solve the equation for a0, using t = p.Lifetime and vt = 0.
            p.Acceleration = -p.Velocity / p.Lifetime;
        }
    }
}
