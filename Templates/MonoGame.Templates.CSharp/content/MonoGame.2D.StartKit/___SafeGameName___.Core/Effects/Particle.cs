using System;
using Microsoft.Xna.Framework;

namespace ___SafeGameName___.Core.Effects;


// <summary>
/// The data for a single particle in this game's particle systems.
/// </summary>
public class Particle
{
    public Color Color;

    public float LifeTime;

    public Vector2 Position;
    public Vector2 PreviousPosition;

    public float Scale;

    /// <summary>
    /// The length of the lines drawn for each particle.
    /// </summary>
    public float TailLength;

    public Vector2 Velocity;

    /// <summary>
    /// Check if the particle is still alive
    /// </summary>
    public bool IsAlive => LifeTime > 0;

    /// <summary>
    /// Triggered when the particle "dies"
    /// Be careful or circular referencing emitters or you'll have endless particles.
    /// </summary>
    public event Action<Vector2> OnDeath;

    public Particle(Vector2 position, Vector2 velocity, float lifeTime, Color color, float scale, float tailLength = 0f)
    {
        Position = position;
        PreviousPosition = position; // Initialize previous position
        Velocity = velocity;
        LifeTime = lifeTime;
        Color = color;
        Scale = scale;
        TailLength = tailLength;
    }


    public void Update(GameTime gameTime)
    {
        // Store the elapsedTime, to avoid Property getter overhead
        // As well as just into it ticks over a second giving use weird results
        var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Store previous position before updating
        PreviousPosition = Position;

        // Update particle's position
        Position += Velocity * elapsedTime;

        // reduce particle's lifespan
        LifeTime -= elapsedTime;

        // Fade particle colour over lifetime
        Color.A = (byte)(255f * LifeTime / 0.5f);

        if (!IsAlive)
        {
            OnDeath?.Invoke(Position);
        }
    }
}