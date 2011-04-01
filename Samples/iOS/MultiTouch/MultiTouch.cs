using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Samples.MultiTouch
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

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

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>

        Texture2D Brush;

        TouchCollection touchStateCollection;
        bool Cls = true;
        List<Color> drawColors = new List<Color>();
        Dictionary<int, Color> LineColors = new Dictionary<int, Color>();

        int ShakeTime = 0;
        float LastAccelX = 0f;

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load in a single pixel to use as the brush
            Brush = Content.Load<Texture2D>("sqbrush");

            // Set the random colors for multi touch painting
            drawColors.Add(Color.Orange);
            drawColors.Add(Color.Yellow);
            drawColors.Add(Color.Green);
            drawColors.Add(Color.Cyan);
            drawColors.Add(Color.HotPink);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Check if the x force on the Zune has changed by more than 0.4g in the past half second - if so, it's shaking
            ShakeTime += gameTime.TotalGameTime.Milliseconds;
            if (ShakeTime >= 500)
            {
                Vector3 acceleration = Accelerometer.GetState().Acceleration;
                if (Math.Abs(acceleration.X - LastAccelX) > 0.4)
                {
                    Cls = true;
                }
                LastAccelX = acceleration.X;
                ShakeTime = 0;
            }

            // Update touch panel state
            touchStateCollection = TouchPanel.GetState();
			
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (Cls)
            {
                Cls = false;
                graphics.GraphicsDevice.Clear(Color.Black);
            }

            spriteBatch.Begin(SpriteSortMode.Deferred,BlendState.AlphaBlend);

			foreach (TouchLocation t in touchStateCollection)
            {
				TouchLocation PrevLocation = new TouchLocation();
                if (t.TryGetPreviousLocation(out PrevLocation))
                {
                    if (!LineColors.ContainsKey(t.Id))
                    {
                        if (touchStateCollection.Count > 1)
                        {
                            Random randomizer = new Random();
                            LineColors[t.Id] = drawColors[randomizer.Next(0, 4)];
                        }
                        else
                        {
                            LineColors[t.Id] = Color.White;
                        }
                    }

                    spriteBatch.Draw(Brush, PrevLocation.Position, null, 
					                 LineColors[t.Id], (float)Math.Atan2((double)(t.Position.Y - PrevLocation.Position.Y), (double)(t.Position.X - PrevLocation.Position.X)), Vector2.Zero, 
					                 new Vector2(Vector2.Distance(PrevLocation.Position, t.Position), 1f), SpriteEffects.None, 0f);
                }
            }

            spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
