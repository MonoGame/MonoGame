namespace Example;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Game1 : Game
{
    readonly GraphicsDeviceManager graphics;

    public static Texture2D Pixel { get; set; }

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
    }

    protected override void Initialize()
    {
        Window.Title = "Hello World";

        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        graphics.ApplyChanges();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        base.Draw(gameTime);
    }
}
