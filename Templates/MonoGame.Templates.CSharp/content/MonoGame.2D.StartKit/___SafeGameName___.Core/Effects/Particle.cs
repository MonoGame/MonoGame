using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;

namespace ___SafeGameName___.Core.Effects;

public class Particle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float LifeTime;
    public Color Color;
    public float Scale;

    /// <summary>
    /// Check if the particle is still alive
    /// </summary>
    public bool IsAlive => LifeTime > 0;

    /// <summary>
    /// Triggered when the particle "dies"
    /// Be careful of circular referencing emitters or you'll have endless particles :)
    /// </summary>
    public event Action<Vector2> OnDeath;

    public Particle(Vector2 position, Vector2 velocity, float lifeTime, Color color, float scale)
    {
        Position = position;
        Velocity = velocity;
        LifeTime = lifeTime;
        Color = color;
        Scale = scale;
    }

    // Update particle's position and reduce its lifespan
    public void Update(GameTime gameTime)
    {
        var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Position += Velocity * elapsedTime;
        LifeTime -= elapsedTime;

        if (!IsAlive)
        {
            OnDeath?.Invoke(Position);
        }
    }
}