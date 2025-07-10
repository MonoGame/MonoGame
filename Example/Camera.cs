namespace Example;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Camera(GraphicsDevice graphicsDevice)
{
    readonly GraphicsDevice graphicsDevice = graphicsDevice;

    public Vector2 Position { get; set; }

    public float Zoom { get; set; }

    public Matrix Transform { get; private set; } = new();

    public void Update()
    {
        Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) * Matrix.CreateScale(Zoom) * Matrix.CreateTranslation(graphicsDevice.Viewport.Width / 2f, graphicsDevice.Viewport.Height / 2f, 0);
    }
}
