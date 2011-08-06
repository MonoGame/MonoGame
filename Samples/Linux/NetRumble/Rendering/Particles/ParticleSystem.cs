#region File Description
//-----------------------------------------------------------------------------
// ParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace NetRumble
{
    /// <summary>
    /// A single-emitter particle system within a larger effect.
    /// </summary>
    public class ParticleSystem
    {
        #region Description Data


        /// <summary>
        /// The name of the particle system.
        /// </summary>
        private string name = "DefaultParticleSystem";

        /// <summary>
        /// The number of particles in this system.
        /// </summary>
        private int particleCount = 256;


        #endregion


        #region Graphics Data


        /// <summary>
        /// The current position of the system, relative to the effect.
        /// </summary>
        private Vector2 position = Vector2.Zero;

        /// <summary>
        /// The tint of the particles in the system.
        /// </summary>
        private Vector4 color = 
            Microsoft.Xna.Framework.Color.White.ToVector4();

        /// <summary>
        /// The content name of the texture used with this system.
        /// </summary>
        private string textureName = "default_particle";

        /// <summary>
        /// The texture used with this system.
        /// </summary>
        private Texture2D texture = null;

        /// <summary>
        /// The center of the texture used with this system.
        /// </summary>
        /// <remarks>This is derived from the texture, calculated and stored.</remarks>
        private Vector2 textureOrigin;

        /// <summary>
        /// The blending mode for this system.
        /// </summary>
        private SpriteBlendMode blendMode = SpriteBlendMode.AlphaBlend;


        #endregion


        #region Status Data


        /// <summary>
        /// If true, the particle system is currently active.
        /// </summary>
        private bool active = false;
        [XmlIgnore()]
        public bool Active
        {
            get { return (active || (timeRemaining > 0.0f)); }
        }

        /// <summary>
        /// The amount of time that the particle system generates particles.
        /// </summary>
        private float duration = float.MaxValue;

        /// <summary>
        /// The time remaining for the particle system's generation.
        /// </summary>
        private float timeRemaining = float.MaxValue;

        /// <summary>
        /// The initial delay before the particle system starts generating particles.
        /// </summary>
        /// <remarks>Used for offsetting the start of particle systems.</remarks>
        private float initialDelay = 0f;

        /// <summary>
        /// The amount of time left on the initial delay.
        /// </summary>
        private float initialDelayRemaining = 0f;


        #endregion


        #region Particle Creation Data


        /// <summary>
        /// The number of particles that this system releases per second.
        /// </summary>
        private int particlesPerSecond = 128;

        /// <summary>
        /// The number of seconds between particle releases.
        /// </summary>
        /// <remarks>This is derived from particlesPerSecond.</remarks>
        private float releaseRate = 0.25f;

        /// <summary>
        /// The amount of time since the last particle was emitted.
        /// </summary>
        private float releaseTimer = 0f;
        
        /// <summary>
        /// The minimum lifetime possible for new particles.
        /// </summary>
        private float durationMinimum = 1f;

        /// <summary>
        /// The maximum lifetime possible for new particles.
        /// </summary>
        private float durationMaximum = 1f;
        
        /// <summary>
        /// The minimum initial velocity of new particles.
        /// </summary>
        private float velocityMinimum = 16f;

        /// <summary>
        /// The maximum initial velocity of new particles.
        /// </summary>
        private float velocityMaximum = 32f;

        /// <summary>
        /// The minimum acceleration of new particles.
        /// </summary>
        private float accelerationMinimum = 0f;

        /// <summary>
        /// The maximum acceleration of new particles.
        /// </summary>
        private float accelerationMaximum = 0f;

        /// <summary>
        /// The minimum initial scale of new particles.
        /// </summary>
        private float scaleMinimum = 1f;

        /// <summary>
        /// The maximum initial scale of new particles.
        /// </summary>
        private float scaleMaximum = 1f;

        /// <summary>
        /// The minimum initial opacity of new particles.
        /// </summary>
        private float opacityMinimum = 1f;

        /// <summary>
        /// The maximum initial opacity of new particles.
        /// </summary>
        private float opacityMaximum = 1f;

        /// <summary>
        /// The minimum release angle of new particles from the center.
        /// </summary>
        private float releaseAngleMinimum = 0f;

        /// <summary>
        /// The maximum release angle of new particles from the center.
        /// </summary>
        private float releaseAngleMaximum = 360f;

        /// <summary>
        /// The minimum release distance of new particles from the center.
        /// </summary>
        private float releaseDistanceMinimum = 0f;

        /// <summary>
        /// The maximum release distance of new particles from the center.
        /// </summary>
        private float releaseDistanceMaximum = 0f;


        #endregion


        #region Particle Updating Data


        /// <summary>
        /// The angular velocity of particles in this system.
        /// </summary>
        private float angularVelocity = 0f;

        /// <summary>
        /// The change in the scale per second for particles in this system.
        /// </summary>
        private float scaleDeltaPerSecond = 0f;

        /// <summary>
        /// The change in the opacity per second for particles in this system.
        /// </summary>
        private float opacityDeltaPerSecond = 0f;


        #endregion


        #region Particle Cache


        /// <summary>
        /// The cache of all particle objects in this system.
        /// </summary>
        private ParticleCache particles;


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Construct a new particle system.
        /// </summary>
        public ParticleSystem() { }


        /// <summary>
        /// Clone a particle system.
        /// </summary>
        /// <returns>A clone of this particle system.</returns>
        public ParticleSystem Clone()
        {
            ParticleSystem clone = new ParticleSystem();

            clone.name = this.name;
            clone.particleCount = this.particleCount;

            clone.position = this.position;
            clone.color = this.color;
            clone.textureName = this.textureName;
            clone.blendMode = this.blendMode;

            clone.duration = this.duration;
            clone.initialDelay = this.initialDelay;

            clone.particlesPerSecond = this.particlesPerSecond;
            clone.releaseRate = this.releaseRate;
            clone.durationMinimum = this.durationMinimum;
            clone.durationMaximum = this.durationMaximum;
            clone.velocityMinimum = this.velocityMinimum;
            clone.velocityMaximum = this.velocityMaximum;
            clone.accelerationMinimum = this.accelerationMinimum;
            clone.accelerationMaximum = this.accelerationMaximum;
            clone.scaleMinimum = this.scaleMinimum;
            clone.scaleMaximum = this.scaleMaximum;
            clone.opacityMinimum = this.opacityMinimum;
            clone.opacityMaximum = this.opacityMaximum;
            clone.releaseAngleMinimum = this.releaseAngleMinimum;
            clone.releaseAngleMaximum = this.releaseAngleMaximum;
            clone.releaseDistanceMinimum = this.releaseDistanceMinimum;
            clone.releaseDistanceMaximum = this.releaseDistanceMaximum;

            clone.angularVelocity = this.angularVelocity;
            clone.scaleDeltaPerSecond = this.scaleDeltaPerSecond;
            clone.opacityDeltaPerSecond = this.opacityDeltaPerSecond;

            return clone;
        }


        /// <summary>
        /// Initialize the particle system.
        /// </summary>
        /// <param name="content">The content manager that owns the texture.</param>
        public virtual void Initialize(ContentManager content)
        {
            // calculate the release rate
            releaseRate = 1.0f / (float)particlesPerSecond;

            // create the cache
            particles = new ParticleCache(particleCount);

            // load the texture
            try
            {
                texture = content.Load<Texture2D>(textureName);
            }
            catch (ContentLoadException)
            {
                texture = content.Load<Texture2D>("Textures/Particles/defaultParticle");
            }

            // calculate the origin on the texture
            textureOrigin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            
            // allow us to start updating and drawing
            active = true;
        }


        /// <summary>
        /// Resets the particle system.
        /// </summary>
        public virtual void Reset()
        {
            // reset the cache
            particles.Reset();

            // reset the timers
            timeRemaining = duration;
            initialDelayRemaining = initialDelay;

            // allow us to start updating and drawing
            active = true;
        }


        #endregion


        #region Updating Methods


        /// <summary>
        /// Update the particle system.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public virtual void Update(float elapsedTime)
        {
            // if the system isn't active, dont' update at all
            if (!Active)
                return;

            // update the initial delay
            if (initialDelayRemaining > 0.0f)
            {
                initialDelayRemaining -= elapsedTime;
                return;
            }
            
            // generate new particles
            GenerateParticles(elapsedTime);

            // update the existing particles
            UpdateParticles(elapsedTime);

            // update the active flag, based on if there are any used particles
            active = particles.UsedCount > 0;
        }


        /// <summary>
        /// Generate new particles into the system.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        private void GenerateParticles(float elapsedTime)
        {
            if (timeRemaining <= 0.0f)
            {
                return;
            }

            // update the timer
            timeRemaining -= elapsedTime;

            // release some particles if it's time
            releaseTimer += elapsedTime;
            while (releaseTimer >= releaseRate)
            {
                // only get new particles if you can
                Particle particle = particles.GetNextParticle();
                if (particle == null)
                {
                    break;
                }
                else
                {
                    // initialize the new particle 
                    InitializeParticle(particle);
                    // reduce the release timer for the release rate of a particle
                    releaseTimer -= releaseRate;
                }
            }
        }


        /// <summary>
        /// Update all of the individual particles in this system.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        private void UpdateParticles(float elapsedTime)
        {
            for (int i = 0; i < particles.Particles.Length; ++i)
            {
                if (particles.Particles[i].TimeRemaining > 0.0f)
                {
                    // update the timer on the particle
                    particles.Particles[i].TimeRemaining -= elapsedTime;

                    // if the particle just timed out on this update, 
                    // add it to the freed-list.
                    if (particles.Particles[i].TimeRemaining <= 0.0f)
                    {
                        particles.ReleaseParticle(particles.Particles[i]);
                        continue;
                    }

                    // update the particle
                    particles.Particles[i].Update(elapsedTime, angularVelocity, 
                        scaleDeltaPerSecond, opacityDeltaPerSecond);
                }
            }
        }


        /// <summary>
        /// Initialize a new particle using the values in this system.
        /// </summary>
        /// <param name="particle">The particle to be initialized.</param>
        private void InitializeParticle(Particle particle)
        {
            // safety-check the parameter
            if (particle == null)
            {
                throw new ArgumentNullException("particle");
            }

            // set the time remaining on the new particle
            particle.TimeRemaining = RandomMath.RandomBetween(durationMinimum, 
                durationMaximum);

            // generate a random direction
            Vector2 direction = RandomMath.RandomDirection(releaseAngleMinimum, 
                releaseAngleMaximum);

            // set the graphics data on the new particle
            particle.Position = position + direction * 
                RandomMath.RandomBetween(releaseDistanceMinimum, 
                releaseDistanceMaximum);
            particle.Velocity = direction * RandomMath.RandomBetween(velocityMinimum, 
                velocityMaximum);
            if (particle.Velocity.LengthSquared() > 0f)
            {
                particle.Acceleration = direction * 
                    RandomMath.RandomBetween(accelerationMinimum, accelerationMaximum);
            }
            else
            {
                particle.Acceleration = Vector2.Zero;
            }
            particle.Rotation = RandomMath.RandomBetween(0f, MathHelper.TwoPi);
            particle.Scale = RandomMath.RandomBetween(scaleMinimum, scaleMaximum);
            particle.Opacity = RandomMath.RandomBetween(opacityMinimum, opacityMaximum);
        }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draw the particle system.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch object used to draw.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // only draw if we're active
            if (!Active)
                return;

            // draw each particle
            for (int p = 0; p < particles.Particles.Length; ++p)
            {
                Particle particle = particles.Particles[p];

                if (particle.TimeRemaining > 0.0f)
                {
                    color.W = particle.Opacity;
                    spriteBatch.Draw(texture, particle.Position, null, new Color(color),
                        particle.Rotation, textureOrigin, particle.Scale, 
                        SpriteEffects.None, 1f);
                }
            }
        }


        #endregion


        #region Control Methods


        /// <summary>
        /// Stop the particle system.
        /// </summary>
        /// <param name="immediately">
        /// If true, particles are no longer drawn or updated. 
        /// Otherwise, only generation is halted.
        /// </param>
        public void Stop(bool immediately)
        {
            // halt generation
            timeRemaining = 0.0f;

            // halt updating/drawing of the particles if requested
            if (immediately)
            {
                active = false;
            }
        }


        #endregion


        #region Serialization Interface


        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int ParticleCount
        {
            get { return particleCount; }
            set { particleCount = value; }
        }


        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector4 Color
        {
            get { return color; }
            set { color = value; }
        }

        public string TextureName
        {
            get { return textureName; }
            set { textureName = value; }
        }

        public SpriteBlendMode BlendMode
        {
            get { return blendMode; }
            set { blendMode = value; }
        }

        
        public float Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        public float InitialDelay
        {
            get { return initialDelay; }
            set { initialDelay = value; }
        }


        public int ParticlesPerSecond
        {
            get { return particlesPerSecond; }
            set { particlesPerSecond = value; }
        }

        public float DurationMinimum
        {
            get { return durationMinimum; }
            set { durationMinimum = value; }
        }

        public float DurationMaximum
        {
            get { return durationMaximum; }
            set { durationMaximum = value; }
        }

        public float VelocityMinimum
        {
            get { return velocityMinimum; }
            set { velocityMinimum = value; }
        }

        public float VelocityMaximum
        {
            get { return velocityMaximum; }
            set { velocityMaximum = value; }
        }

        public float AccelerationMinimum
        {
            get { return accelerationMinimum; }
            set { accelerationMinimum = value; }
        }

        public float AccelerationMaximum
        {
            get { return accelerationMaximum; }
            set { accelerationMaximum = value; }
        }

        public float ScaleMinimum
        {
            get { return scaleMinimum; }
            set { scaleMinimum = value; }
        }

        public float ScaleMaximum
        {
            get { return scaleMaximum; }
            set { scaleMaximum = value; }
        }

        public float OpacityMinimum
        {
            get { return opacityMinimum; }
            set { opacityMinimum = value; }
        }

        public float OpacityMaximum
        {
            get { return opacityMaximum; }
            set { opacityMaximum = value; }
        }

        public float ReleaseAngleMinimum
        {
            get { return releaseAngleMinimum; }
            set { releaseAngleMinimum = value; }
        }

        public float ReleaseAngleMaximum
        {
            get { return releaseAngleMaximum; }
            set { releaseAngleMaximum = value; }
        }

        public float ReleaseDistanceMinimum
        {
            get { return releaseDistanceMinimum; }
            set { releaseDistanceMinimum = value; }
        }

        public float ReleaseDistanceMaximum
        {
            get { return releaseDistanceMaximum; }
            set { releaseDistanceMaximum = value; }
        }


        public float AngularVelocity
        {
            get { return angularVelocity; }
            set { angularVelocity = value; }
        }
        
        public float ScaleDeltaPerSecond
        {
            get { return scaleDeltaPerSecond; }
            set { scaleDeltaPerSecond = value; }
        }

        public float OpacityDeltaPerSecond
        {
            get { return opacityDeltaPerSecond; }
            set { opacityDeltaPerSecond = value; }
        }


        #endregion
    }
}