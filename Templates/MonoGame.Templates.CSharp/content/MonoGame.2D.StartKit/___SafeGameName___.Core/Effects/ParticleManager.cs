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
    private Vector2 emitterPosition;

    public ParticleManager(Texture2D texture, Vector2 position)
    {
        particles = new List<Particle>();
        random = new Random();
        this.texture = texture;
        emitterPosition = position;
    }

    // Emit particles based on the effect type
    public void Emit(int numberOfParticles, ParticleEffectType effectType)
    {
        switch (effectType)
        {
            case ParticleEffectType.Fireworks:
                EmitFireworks(numberOfParticles);
                break;
            case ParticleEffectType.Sparkles:
                EmitSparkles(numberOfParticles);
                break;
            case ParticleEffectType.Confetti:
                EmitConfetti(numberOfParticles);
                break;
        }
    }

    // Emit particles for Fireworks effect
    private void EmitFireworks(int numberOfParticles)
    {
        for (int i = 0; i < numberOfParticles; i++)
        {
            Vector2 velocity = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1)) * 150;
            float lifetime = (float)random.NextDouble() * 2f + 1f;
            Color color = new Color(random.Next(256), random.Next(256), random.Next(256)); // Random colors for fireworks
            float scale = (float)random.NextDouble() * 0.5f + 0.5f;

            particles.Add(new Particle(emitterPosition, velocity, lifetime, color, scale));
        }
    }

    // Emit particles for Sparkles effect
    private void EmitSparkles(int numberOfParticles)
    {
        for (int i = 0; i < numberOfParticles; i++)
        {
            Vector2 velocity = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1)) * 50;
            float lifetime = (float)random.NextDouble() * 1f + 0.5f;
            Color color = Color.White * ((float)random.NextDouble() * 0.5f + 0.5f); // Light sparkly effect
            float scale = (float)random.NextDouble() * 0.2f + 0.2f;

            particles.Add(new Particle(emitterPosition, velocity, lifetime, color, scale));
        }
    }

    // Emit particles for Confetti effect
    private void EmitConfetti(int numberOfParticles)
    {
        for (int i = 0; i < numberOfParticles; i++)
        {
            Vector2 velocity = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble())) * 100;
            float lifetime = (float)random.NextDouble() * 3f + 1f;
            Color color = new Color(random.Next(256), random.Next(256), random.Next(256)); // Bright colors for confetti
            float scale = (float)random.NextDouble() * 0.3f + 0.3f;

            particles.Add(new Particle(emitterPosition, velocity, lifetime, color, scale));
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
                spriteBatch.Draw(texture, particle.Position, null, particle.Color, 0f,
                    new Vector2(texture.Width / 2, texture.Height / 2), particle.Scale, SpriteEffects.None, 0f);
            }
        }
    }
}
