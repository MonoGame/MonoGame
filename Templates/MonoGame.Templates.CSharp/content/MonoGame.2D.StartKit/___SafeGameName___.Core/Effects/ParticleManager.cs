using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ___SafeGameName___.Core.Effects;

public class ParticleManager
{
    private List<Particle> particles;
    private Random random;
    private Texture2D texture;
    private Vector2 position;
    private Vector2 textureOrigin;

    public Vector2 Position
    {
        get => position;
        set => position = value;
    }
    public Texture2D Texture
    {
        get => texture;
        set => texture = value;
    }
    public int ParticleCount
    {
        get
        {
            if (particles != null)
            {
                return particles.Count;
            }
            else
            {
                return 0;
            }
        }
    }

    public ParticleManager(Texture2D texture, Vector2 position)
    {
        this.particles = new List<Particle>();
        this.random = new Random();
        this.texture = texture;
        this.textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
        this.position = position;
    }

    // Emit particles based on the effect type
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

    // Emit particles for Confetti effect
    private void EmitConfetti(int numberOfParticles, Vector2 emitPosition, Color? color = null)
    {
        for (int i = 0; i < numberOfParticles; i++)
        {
            Vector2 velocity = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble())) * 100;
            float lifetime = (float)random.NextDouble() * 3f + 1f;

            Color actualParticalColor;
            if (color.HasValue)
            {
                actualParticalColor = color.Value;
            }
            else
            {
                actualParticalColor = new Color(random.Next(256), random.Next(256), random.Next(256)); // Bright colors for confetti
            }
            float scale = (float)random.NextDouble() * 0.3f + 0.3f;

            var particle = new Particle(emitPosition, velocity, lifetime, actualParticalColor, scale);
            particles.Add(particle);
        }
    }

    private void EmitExplosions(int numberOfParticles, Vector2 emitPosition, Color? color = null)
    {

        for (int i = 0; i < numberOfParticles; i++)
        {
            // Calculate velocity with more explosive characteristics
            float angle = (float)(random.NextDouble() * Math.PI * 2);
            float speed = (float)(random.NextDouble() * 200 + 100);
            Vector2 velocity = new Vector2(
                (float)Math.Cos(angle) * speed,
                (float)Math.Sin(angle) * speed
            );

            float lifetime = (float)random.NextDouble() * 1.5f + 0.5f;

            Color actualParticleColor = color ?? new Color(
                random.Next(200, 256),  // High red
                random.Next(100, 200),  // Medium green
                random.Next(0, 100)     // Low blue
            );

            float scale = (float)random.NextDouble() * 0.4f + 0.2f;

            // Give these a tail
            var particle = new Particle(emitPosition, velocity, lifetime, actualParticleColor, scale, 10);
            particles.Add(particle);
        }
    }

    // Emit particles for Fireworks effect
    private void EmitFireworks(int numberOfParticles, Vector2 emitPosition, Color? color = null)
    {
        for (int i = 0; i < numberOfParticles; i++)
        {
            Vector2 velocity = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1)) * 150;
            float lifetime = (float)random.NextDouble() * 2f + 1f;

            Color actualParticalColor;
            if (color.HasValue)
            {
                actualParticalColor = color.Value;
            }
            else
            {
                actualParticalColor = new Color(random.Next(256), random.Next(256), random.Next(256)); // Random colors for fireworks
            }
            float scale = (float)random.NextDouble() * 0.5f + 0.5f;

            var particle = new Particle(emitPosition, velocity, lifetime, actualParticalColor, scale);

            // Trigger another emitter when each particle dies.
            particle.OnDeath += FireworkParticle_OnDeath;

            particles.Add(particle);
        }
    }

    // Emit particles for Sparkles effect
    private void EmitSparkles(int numberOfParticles, Vector2 emitPosition, Color? color = null)
    {
        for (int i = 0; i < numberOfParticles; i++)
        {
            Vector2 velocity = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1)) * 50;
            float lifetime = (float)random.NextDouble() * 1f + 0.5f;

            Color actualParticalColor;
            if (color.HasValue)
            {
                actualParticalColor = color.Value;
            }
            else
            {
                actualParticalColor = Color.White * ((float)random.NextDouble() * 0.5f + 0.5f); // Light sparkly effect
            }
            float scale = (float)random.NextDouble() * 0.2f + 0.2f;

            var particle = new Particle(emitPosition, velocity, lifetime, actualParticalColor, scale);
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