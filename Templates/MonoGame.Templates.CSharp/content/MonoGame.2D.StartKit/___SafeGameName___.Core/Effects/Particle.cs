using System;
using Microsoft.Xna.Framework;

namespace ___SafeGameName___.Core.Effects;

// <summary>
/// The data for a single particle in this game's particle systems.
/// </summary>
public class Particle
{
    /// <summary>
    /// The amount of drag applied to velocity per second, 
    /// Help simulate some air resistance or even gravity
    /// </summary>
    public float DragPerSecond = 0.9f;

    /// <summary>
    /// The current colour of the particle.
    /// </summary>
    public Color Color;

    /// <summary>
    /// The direction that this particle is moving in.
    /// </summary>
    public Vector2 Direction;

    /// <summary>
    /// The total lifetime of the particle.
    /// </summary>
    public float LifeTime;

    /// <summary>
    /// As LifeTime itself changes over time
    /// </summary>
    public float InitialLifeTime;

    /// <summary>
    /// The position of the particle.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// The previous position of the particle.
    /// </summary>
    public Vector2 PreviousPosition;

    /// <summary>
    /// How much to scale the particle.
    /// </summary>
    public float Scale;

    /// <summary>
    /// The length of the lines drawn for each particle.
    /// </summary>
    public float TailLength;

    /// <summary>
    /// The velocity vector of particle.
    /// </summary>
    Vector2 Velocity;


    /// <summary>
    /// Check if the particle is still alive
    /// </summary>
    public bool IsAlive => LifeTime > 0;


    /// <summary>
    /// Triggered when the particle "dies"
    /// Be careful or circular referencing emitters or you'll have endless particles.
    /// </summary>
    public event Action<Vector2> OnDeath;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    /// <param name="lifeTime"></param>
    /// <param name="color"></param>
    /// <param name="scale"></param>
    /// <param name="tailLength"></param>
    public Particle(Vector2 position, Vector2 direction, float speed, float lifeTime, Color color, float scale, float tailLength = 0f)
    {
        Position = position;
        PreviousPosition = position;
        Velocity = direction * speed;
        LifeTime = lifeTime;
        InitialLifeTime = lifeTime;
        Color = color;
        Scale = scale;
        TailLength = tailLength;
    }

    public void Update(GameTime gameTime)
    {
        // Get elapsed time in seconds
        var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Avoid further processing if, for example the game is paused
        if (elapsedTime <= 0)
            return;

        // Store previous position before updating
        PreviousPosition = Position;

        // Update particle's position
        Position += Velocity * elapsedTime;

        // apply a drag factor
        float dragFactor = Math.Max(1 - (elapsedTime * DragPerSecond), 0);
        Velocity *= dragFactor;

        // Reduce particle's lifespan
        LifeTime -= elapsedTime;

        // Fade particle colour over lifetime
        Color.A = (byte)(255f * LifeTime / InitialLifeTime);

        if (!IsAlive)
        {
            OnDeath?.Invoke(Position);
        }
    }
}