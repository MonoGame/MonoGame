using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ___SafeGameName___.Core.Effects;

public class ParticleManager
{
    private Random random;

    private Vector2 position;
    /// <summary>
    /// Position where these particles eminate from
    /// </summary>
    public Vector2 Position
    {
        get => position;
        set => position = value;
    }

    private Vector2 textureOrigin;
    private Texture2D texture;
    /// <summary>
    /// Texture to be used for this set of particles
    /// </summary>
    public Texture2D Texture
    {
        get => texture;
        set => texture = value;
    }

    private List<Particle> particles;
    /// <summary>
    /// How many particles still left to be shown
    /// </summary>
    public int ParticleCount => particles != null ? particles.Count : 0;

    /// <summary>
    /// ParticleManager constructor
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="position"></param>
    public ParticleManager(Texture2D texture, Vector2 position)
    {
        this.particles = new List<Particle>();
        this.random = new Random();
        this.texture = texture;
        this.textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
        this.position = position;
    }

    /// <summary>
    /// Emit built-in particles based on the effect type
    /// </summary>
    /// <param name="numberOfParticles"></param>
    /// <param name="effectType"></param>
    /// <param name="color"></param>
    public void Emit(int numberOfParticles, ParticleEffectType effectType, Color? color = null)
    {
        switch (effectType)
        {
            case ParticleEffectType.Confetti:
                EmitConfetti(numberOfParticles, position, color);
                break;
            case ParticleEffectType.Explosions:
                EmitExplosions(numberOfParticles, position, color);
                break;
            case ParticleEffectType.Fireworks:
                EmitFireworks(numberOfParticles, position, color);
                break;
            case ParticleEffectType.Sparkles:
                EmitSparkles(numberOfParticles, position, color);
                break;
        }
    }

    /// <summary>
    /// Emit particles for Confetti effect
    /// </summary>
    /// <param name="numberOfParticles"></param>
    /// <param name="emitPosition"></param>
    /// <param name="color"></param>
    private void EmitConfetti(int numberOfParticles, Vector2 emitPosition, Color? color = null)
    {
        for (int i = 0; i < numberOfParticles; i++)
        {
            // Generate a random direction vector
            Vector2 randomDirection = new Vector2(
                (float)(random.NextDouble() * 2 - 1),   // X component in range [-1, 1]
                (float)random.NextDouble()              // Y component in range [0, 1]
            );

            // Normalize the direction vector
            randomDirection.Normalize();

            // Generate a random speed in a reasonable range
            float speed = (float)random.NextDouble() * 200 + 50; // Speed between 50 and 250

            Vector2 velocity = new Vector2((float)(random.NextDouble() * 2 - 1), (float)random.NextDouble()) * 200;
            float lifetime = (float)random.NextDouble() * 3f + 1f;

            // Determine the particle's color
            Color actualParticleColor = color ?? new Color(random.Next(256), random.Next(256), random.Next(256)); // Bright colors for confetti

            float scale = (float)random.NextDouble() * 0.5f + 0.3f;

            var particle = new Particle(emitPosition, randomDirection, speed, lifetime, actualParticleColor, scale);
            particles.Add(particle);
        }
    }

    /// <summary>
    /// Emit particles for Explosions effect
    /// </summary>
    /// <param name="numberOfParticles"></param>
    /// <param name="emitPosition"></param>
    /// <param name="color"></param>
    private void EmitExplosions(int numberOfParticles, Vector2 emitPosition, Color? color = null)
    {
        for (int i = 0; i < numberOfParticles; i++)
        {
            // Calculate a random direction for the explosion
            float angle = (float)(random.NextDouble() * Math.PI * 2); // Random angle in radians
            Vector2 direction = new Vector2(
                (float)Math.Cos(angle),
                (float)Math.Sin(angle)
            );

            // Generate a random speed for explosive velocity
            float speed = (float)(random.NextDouble() * 300 + 100); // Speed between 100 and 400

            // Generate a random lifetime
            float lifetime = (float)random.NextDouble() * 1.5f + 0.5f; // Lifetime between 0.5 and 2 seconds

            // Determine the particle's color
            Color actualParticleColor = color ?? new Color(
                random.Next(200, 256), // High red
                random.Next(100, 200), // Medium green
                random.Next(0, 100)    // Low blue
            );

            // Generate a random scale for the particle
            float scale = (float)random.NextDouble() * 0.5f + 0.2f;

            // Create the particle give it a tail
            var particle = new Particle(emitPosition, direction, speed, lifetime, actualParticleColor, scale, 10);

            // Add the particle to the collection
            particles.Add(particle);
        }
    }

    /// <summary>
    /// Emit particles for Fireworks effect
    /// </summary>
    /// <param name="numberOfParticles"></param>
    /// <param name="emitPosition"></param>
    /// <param name="color"></param>
    private void EmitFireworks(int numberOfParticles, Vector2 emitPosition, Color? color = null)
    {
        for (int i = 0; i < numberOfParticles; i++)
        {
            // Generate a random angle for each particle
            float angle = (float)(random.NextDouble() * Math.PI * 2); // Full 360 degrees in radians

            // Create a unit direction vector based on the angle
            Vector2 direction = new Vector2(
                (float)Math.Cos(angle),
                (float)Math.Sin(angle)
            );

            // Assign a random speed for explosive effect
            float speed = (float)random.NextDouble() * 300 + 100; // Speed between 100 and 400

            // Generate a random lifetime for the particle
            float lifetime = (float)random.NextDouble() * 2f + 1f; // Lifetime between 1 and 3 seconds

            // Assign a color to the particle
            Color actualParticleColor = color ?? new Color(
                random.Next(256), // Random red component
                random.Next(256), // Random green component
                random.Next(256)  // Random blue component
            );

            // Assign a random scale for each particle
            float scale = (float)random.NextDouble() * 0.5f + 0.5f;

            // Create the particle with the direction and speed
            var particle = new Particle(emitPosition, direction, speed, lifetime, actualParticleColor, scale);

            // Attach an event to trigger additional effects on particle death
            particle.OnDeath += FireworkParticle_OnDeath;

            // Add the particle to the collection
            particles.Add(particle);
        }
    }

    /// <summary>
    /// Emit particles for Sparkles effect
    /// </summary>
    /// <param name="numberOfParticles"></param>
    /// <param name="emitPosition"></param>
    /// <param name="color"></param>
    private void EmitSparkles(int numberOfParticles, Vector2 emitPosition, Color? color = null)
    {
        for (int i = 0; i < numberOfParticles; i++)
        {
            // Calculate a random direction for the sparkles
            float angle = (float)(random.NextDouble() * Math.PI * 2); // Random angle in radians
            Vector2 direction = new Vector2(
                (float)Math.Cos(angle),
                (float)Math.Sin(angle)
            );

            // Generate a random speed for the sparkle
            float speed = (float)(random.NextDouble() * 300); // Speed between 0 and 300

            // Generate a random lifetime
            float lifetime = (float)random.NextDouble() * 1f + 0.5f; // Lifetime between 0.5 and 1.5 seconds

            // Determine the particle's color
            Color actualParticleColor = color ?? Color.White * ((float)random.NextDouble() * 0.5f + 0.5f); // Light sparkly effect

            // Generate a random scale for the particle
            float scale = (float)random.NextDouble() * 0.5f + 0.2f;

            // Create the particle using the new constructor
            var particle = new Particle(emitPosition, direction, speed, lifetime, actualParticleColor, scale);

            // Add the particle to the collection
            particles.Add(particle);
        }
    }

    /// <summary>
    /// Event fireed when the Fireworks particle dies
    /// </summary>
    /// <param name="particlePosition"></param>
    private void FireworkParticle_OnDeath(Vector2 particlePosition)
    {
        EmitExplosions(5, particlePosition);
    }

    /// <summary>
    /// Update each Particle that is still alive
    /// </summary>
    /// <param name="gameTime"></param>
    public void Update(GameTime gameTime)
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            particles[i].Update(gameTime);

            if (!particles[i].IsAlive)
            {
                particles.RemoveAt(i);
            }
        }
    }


    /// <summary>
    /// Controls the "density" of the tail
    /// Dense Tail (t += 1f): A continuous, almost solid-looking trail.Ideal for effects like glowing streaks.
	/// Sparse Tail (t += 10f): A dotted, fragmented appearance.Useful for effects like spark trails or light debris.
    /// </summary>
    const float tailDensity = 5f;

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (Particle particle in particles)
        {
            if (particle.IsAlive)
            {
                // Calculate the direction and length of the tail
                Vector2 tailDirection = particle.Position - particle.PreviousPosition;
                float tailLength = particle.TailLength * tailDirection.Length();

                // Normalize the tail direction
                if (tailDirection != Vector2.Zero)
                    tailDirection.Normalize();

                // Draw the this particle's texture
                spriteBatch.Draw(
                    texture,
                    particle.Position,
                    null,
                    particle.Color,
                    0.0f,
                    textureOrigin,
                    particle.Scale,
                    SpriteEffects.None,
                    0f);

                // Draw the particle's tail
                for (float t = 0; t < tailLength; t += tailDensity)
                {
                    Vector2 tailPosition = particle.Position - tailDirection * t;

                    // Fade out the tail
                    float alpha = MathHelper.Clamp(1f - (t / tailLength), 0f, 1f);
                    Color tailColor = particle.Color * alpha;

                    spriteBatch.Draw(
                        texture,
                        tailPosition,
                        null,
                        tailColor,
                        0f,
                        textureOrigin,
                        particle.Scale * 0.8f, // Shrink the tail particle slightly
                        SpriteEffects.None,
                        0f);
                }
            }
        }
    }
}