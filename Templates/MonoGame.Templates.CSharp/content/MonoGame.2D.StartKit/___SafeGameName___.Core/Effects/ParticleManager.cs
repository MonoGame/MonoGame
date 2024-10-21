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
    public int ParticleCount {
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
            case ParticleEffectType.Fireworks:
                EmitFireworks(numberOfParticles, color);
                break;
            case ParticleEffectType.Sparkles:
                EmitSparkles(numberOfParticles, color);
                break;
            case ParticleEffectType.Confetti:
                EmitConfetti(numberOfParticles, color);
                break;
        }
    }

    // Emit particles for Fireworks effect
    private void EmitFireworks(int numberOfParticles, Color? color)
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

            particles.Add(new Particle(position, velocity, lifetime, actualParticalColor, scale));
        }
    }

    // Emit particles for Sparkles effect
    private void EmitSparkles(int numberOfParticles, Color? color)
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

            particles.Add(new Particle(position, velocity, lifetime, actualParticalColor, scale));
        }
    }

    // Emit particles for Confetti effect
    private void EmitConfetti(int numberOfParticles, Color? color)
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

            particles.Add(new Particle(position, velocity, lifetime, actualParticalColor, scale));
        }
    }

    public void Update(GameTime gameTime)
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            particles[i].Update(gameTime);
            if (!particles[i].IsAlive())
            {
                particles.RemoveAt(i);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (Particle particle in particles)
        {
            if (particle.IsAlive())
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
