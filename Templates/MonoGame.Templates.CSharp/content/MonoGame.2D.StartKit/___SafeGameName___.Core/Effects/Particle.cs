using Microsoft.Xna.Framework;

namespace ___SafeGameName___.Core.Effects;

public class Particle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float LifeTime;
    public Color Color;
    public float Scale;

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
    }

    // Check if the particle is still alive
    public bool IsAlive()
    {
        return LifeTime > 0;
    }
}
