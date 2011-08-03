#region File Description
//-----------------------------------------------------------------------------
// ParticleCache.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace NetRumble
{
    /// <summary>
    /// Cache of Particle objects.
    /// </summary>
    public class ParticleCache
    {
        #region Fields


        /// <summary>
        /// Container of the particles in the cache.
        /// </summary>
        public Particle[] Particles;

        /// <summary>
        /// The particles available to be spawned.
        /// </summary>
        private Queue<Particle> freeParticles;


        #endregion


        #region Statistics Properties


        /// <summary>
        /// The total of all particles in the cache.
        /// </summary>
        public int TotalCount
        {
            get { return Particles.Length; }
        }

        /// <summary>
        /// The number of particles remaining in the cache.
        /// </summary>
        public int FreeCount
        {
            get { return freeParticles.Count; }
        }

        /// <summary>
        /// The number of particles in use.
        /// </summary>
        public int UsedCount
        {
            get { return TotalCount - FreeCount; }
        }


        #endregion
        

        #region Initialization Methods


        /// <summary>
        /// Construct a new particle cache.
        /// </summary>
        /// <param name="count">The number of particles to be allocated.</param>
        public ParticleCache(int count)
        {
            // safety-check the parameter
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            // create the particles
            Particles = new Particle[count];
            for (int i = 0; i < count; i++)
            {
                Particles[i] = new Particle();
            }

            // create the freed list, which initially contains all particles
            freeParticles = new Queue<Particle>(Particles);
        }


        /// <summary>
        /// Reset the particle cache to a freed state.
        /// </summary>
        public void Reset()
        {
            // reset the time on particles
            for (int i = 0; i < Particles.Length; ++i)
            {
                Particles[i].TimeRemaining = 0.0f;
            }

            // recreate the freed list, containing all particles
            freeParticles = new Queue<Particle>(Particles);
        }


        #endregion


        #region Membership Methods


        /// <summary>
        /// Gets the new particle to be used out of the cache.
        /// </summary>
        /// <returns>The new particle.</returns>
        public Particle GetNextParticle()
        {
            return (freeParticles.Count > 0) ? freeParticles.Dequeue() : null;
        }


        /// <summary>
        /// Releases a particle back to the cache.
        /// </summary>
        /// <param name="particle">The particle to be released.</param>
        public void ReleaseParticle(Particle particle)
        {
            if (particle != null)
            {
                freeParticles.Enqueue(particle);
            }
        }


        #endregion
    }
}
