namespace Example;

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Entry : Game
{
    Camera camera;
    SpriteBatch spriteBatch;
    readonly GraphicsDeviceManager graphics;
    float pixelScale = 1f;

    public static Texture2D Pixel { get; set; }

    public Entry()
    {
        graphics = new GraphicsDeviceManager(this);

        Content.RootDirectory = "Content";
    }

    void MonogameInitialize()
    {
        Window.Title = "Example";

        spriteBatch = new SpriteBatch(GraphicsDevice);

        Window.AllowUserResizing = true;
        IsMouseVisible = true;
        IsFixedTimeStep = false;

        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        graphics.ApplyChanges();

        Window.ClientSizeChanged += (o, e) => pixelScale = Math.Max(GraphicsDevice.Viewport.Height / 180, 1);

        pixelScale = Math.Max(GraphicsDevice.Viewport.Height / 180, 1);

        Pixel = new Texture2D(GraphicsDevice, 1, 1);
        Pixel.SetData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
    }

    protected override void Initialize()
    {
        MonogameInitialize();

        camera = new Camera(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        camera.Update();
        camera.Zoom = pixelScale;

        float speed = 60 * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            camera.Position += new Vector2(0, -1) * speed;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            camera.Position += new Vector2(0, 1) * speed;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            camera.Position += new Vector2(-1, 0) * speed;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            camera.Position += new Vector2(1, 0) * speed;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        base.Draw(gameTime);
    }
}
