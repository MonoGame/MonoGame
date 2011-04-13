#region File Description
//-----------------------------------------------------------------------------
// ExplosionSmokeParticleSystem.cs
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
    /// ExplosionSmokeParticleSystem is a specialization of ParticleSystem which
    /// creates a circular pattern of smoke. It should be combined with
    /// ExplosionParticleSystem for best effect.
    /// </summary>
    public class ExplosionSmokeParticleSystem : ParticleSystem
    {
        public ExplosionSmokeParticleSystem(ParticleSampleGame game, int howManyEffects)
            : base(game, howManyEffects)
        {            
        }

        /// <summary>
        /// Set up the constants that will give this particle system its behavior and
        /// properties.
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "smoke";

            // less initial speed than the explosion itself
            minInitialSpeed = 20;
            maxInitialSpeed = 200;

            // acceleration is negative, so particles will accelerate away from the
            // initial velocity.  this will make them slow down, as if from wind
            // resistance. we want the smoke to linger a bit and feel wispy, though,
            // so we don't stop them completely like we do ExplosionParticleSystem
            // particles.
            minAcceleration = -10;
            maxAcceleration = -50;

            // explosion smoke lasts for longer than the explosion itself, but not
            // as long as the plumes do.
            minLifetime = 1.0f;
            maxLifetime = 2.5f;

            minScale = 1.0f;
            maxScale = 2.0f;

            minNumParticles = 10;
            maxNumParticles = 20;

            minRotationSpeed = -MathHelper.PiOver4;
            maxRotationSpeed = MathHelper.PiOver4;

            spriteBlendMode = BlendState.AlphaBlend;

            DrawOrder = AlphaBlendDrawOrder;
        }
    }
}
