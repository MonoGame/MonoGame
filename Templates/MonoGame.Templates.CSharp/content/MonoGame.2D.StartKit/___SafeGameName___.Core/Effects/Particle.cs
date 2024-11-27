using System;
using Microsoft.Xna.Framework;

namespace ___SafeGameName___.Core.Effects;


// <summary>
/// The data for a single particle in this game's particle systems.
/// </summary>
public class Particle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float LifeTime;
    public Color Color;
    public float Scale;
    public Vector2 PreviousPosition;

    /// <summary>
    /// Check if the particle is still alive
    /// </summary>
    public bool IsAlive => LifeTime > 0;

    /// <summary>
    /// The length of the lines drawn for each particle.
    /// </summary>
    public float TailLength { get; internal set; }

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

    // Update particle's position and reduce its lifespan
    public void Update(GameTime gameTime)
    {
        var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Store previous position before updating
        PreviousPosition = Position;

        Position += Velocity * elapsedTime;
        LifeTime -= elapsedTime;

        if (!IsAlive)
        {
            OnDeath?.Invoke(Position);
        }
    }
}