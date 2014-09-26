using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
namespace $safeprojectname$
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Vector3 eye;
        Vector3 up;
        Vector3 at;

        float fov;
        float ar;
        //camera properties
        float zNear;
        float zFar;

        BasicEffect basicEffect;
        DynamicVertexBuffer vertexBuffer;
        DynamicIndexBuffer indexBuffer;
        VertexPositionColor[] cube;
        float cubeRotation = 0.0f;
        static ushort[] cubeIndices =
		{
			0,2,1, // -x
			1,2,3,

			4,5,6, // +x
			5,7,6,

			0,1,5, // -y
			0,5,4,

			2,6,7, // +y
			2,7,3,

			0,4,6, // -z
			0,6,2,

			1,3,7, // +z
			1,7,5,
		};
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            zNear = 0.001f;
            zFar = 1000.0f;
            fov = MathHelper.Pi * 70.0f / 180.0f;
            eye = new Vector3(0.0f, 0.7f, 1.5f);
            at = new Vector3(0.0f, 0.0f, 0.0f);
            up = new Vector3(0.0f, 1.0f, 0.0f);

            cube = new VertexPositionColor[8];
            cube[0] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), new Color(0.0f, 0.0f, 0.0f));
            cube[1] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), new Color(0.0f, 0.0f, 1.0f));
            cube[2] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, -0.5f), new Color(0.0f, 1.0f, 0.0f));
            cube[3] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), new Color(0.0f, 1.0f, 1.0f));
            cube[4] = new VertexPositionColor(new Vector3(0.5f, -0.5f, -0.5f), new Color(1.0f, 0.0f, 0.0f));
            cube[5] = new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), new Color(1.0f, 0.0f, 1.0f));
            cube[6] = new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), new Color(1.0f, 1.0f, 0.0f));
            cube[7] = new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), new Color(1.0f, 1.0f, 1.0f));

            vertexBuffer = new DynamicVertexBuffer(graphics.GraphicsDevice, typeof(VertexPositionColor), 8, BufferUsage.WriteOnly);
            indexBuffer = new DynamicIndexBuffer(graphics.GraphicsDevice, typeof(ushort), 36, BufferUsage.WriteOnly);

            basicEffect = new BasicEffect(graphics.GraphicsDevice); //(device, null);
            basicEffect.LightingEnabled = false;
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = false;

            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Compute camera matrices.
            Matrix View = Matrix.CreateLookAt(eye, at, up);

            Matrix Projection = Matrix.CreatePerspectiveFieldOfView(fov, GraphicsDevice.Viewport.AspectRatio, zNear, zFar);
            cubeRotation += (0.001f) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Matrix World = Matrix.CreateRotationY(cubeRotation);
            // TODO: Add your drawing code here

            vertexBuffer.SetData(cube, 0, 8, SetDataOptions.Discard);
            indexBuffer.SetData(cubeIndices, 0, 36, SetDataOptions.Discard);

            GraphicsDevice device = basicEffect.GraphicsDevice;
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;

            basicEffect.View = View;
            basicEffect.Projection = Projection;
            basicEffect.World = World;
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 36);
            }
            base.Draw(gameTime);
        }
    }
}
