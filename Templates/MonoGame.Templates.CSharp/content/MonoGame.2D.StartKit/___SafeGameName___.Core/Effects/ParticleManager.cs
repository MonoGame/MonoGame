using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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

            particles.Add(new Particle(emitPosition, velocity, lifetime, actualParticalColor, scale));
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

            var particle = new Particle
                (
                    emitPosition,
                    velocity,
                    lifetime,
                    actualParticleColor,
                    scale
                );
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

            // Chain another emitter when each particle dies.
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

            particles.Add(new Particle(emitPosition, velocity, lifetime, actualParticalColor, scale));
        }
    }

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

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (Particle particle in particles)
        {
            if (particle.IsAlive)
            {
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
            }
        }
    }
}