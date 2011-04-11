#region File Description
//-----------------------------------------------------------------------------
// ParticleEmitter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;

#endregion

namespace Particle3DSample
{
	/// <summary>
	/// Helper for objects that want to leave particles behind them as they
	/// move around the world. This emitter implementation solves two related
	/// problems:
	/// 
	/// If an object wants to create particles very slowly, less than once per
	/// frame, it can be a pain to keep track of which updates ought to create
	/// a new particle versus which should not.
	/// 
	/// If an object is moving quickly and is creating many particles per frame,
	/// it will look ugly if these particles are all bunched up together. Much
	/// better if they can be spread out along a line between where the object
	/// is now and where it was on the previous frame. This is particularly
	/// important for leaving trails behind fast moving objects such as rockets.
	/// 
	/// This emitter class keeps track of a moving object, remembering its
	/// previous position so it can calculate the velocity of the object. It
	/// works out the perfect locations for creating particles at any frequency
	/// you specify, regardless of whether this is faster or slower than the
	/// game update rate.
	/// </summary>
	public class ParticleEmitter
	{
		#region Fields

		ParticleSystem particleSystem;
		float timeBetweenParticles;
		Vector3 previousPosition;
		float timeLeftOver;

		#endregion


		/// <summary>
		/// Constructs a new particle emitter object.
		/// </summary>
		public ParticleEmitter (ParticleSystem particleSystem,
				float particlesPerSecond,Vector3 initialPosition)
		{
			this.particleSystem = particleSystem;

			timeBetweenParticles = 1.0f / particlesPerSecond;

			previousPosition = initialPosition;
		}


		/// <summary>
		/// Updates the emitter, creating the appropriate number of particles
		/// in the appropriate positions.
		/// </summary>
		public void Update (GameTime gameTime, Vector3 newPosition)
		{
			if (gameTime == null)
				throw new ArgumentNullException ("gameTime");

			// Work out how much time has passed since the previous update.
			float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

			if (elapsedTime > 0) {
				// Work out how fast we are moving.
				Vector3 velocity = (newPosition - previousPosition) / elapsedTime;

				// If we had any time left over that we didn't use during the
				// previous update, add that to the current elapsed time.
				float timeToSpend = timeLeftOver + elapsedTime;

				// Counter for looping over the time interval.
				float currentTime = -timeLeftOver;

				// Create particles as long as we have a big enough time interval.
				while (timeToSpend > timeBetweenParticles) {
					currentTime += timeBetweenParticles;
					timeToSpend -= timeBetweenParticles;

					// Work out the optimal position for this particle. This will produce
					// evenly spaced particles regardless of the object speed, particle
					// creation frequency, or game update rate.
					float mu = currentTime / elapsedTime;

					Vector3 position = Vector3.Lerp (previousPosition, newPosition, mu);

					// Create the particle.
					particleSystem.AddParticle (position, velocity);
				}

				// Store any time we didn't use, so it can be part of the next update.
				timeLeftOver = timeToSpend;
			}

			previousPosition = newPosition;
		}
	}
}
