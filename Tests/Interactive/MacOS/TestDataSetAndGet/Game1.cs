#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace TestDataSetAndGet
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PerPixelCollisionGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        // The images we will draw
        Texture2D personTexture;
        Texture2D blockTexture;
		
		Texture2D characterTexture;

        // The color data for the images; used for per pixel collision
        Color[] personTextureData;
        Color[] blockTextureData;
		Color[] characterTextureData;

        // The images will be drawn with this SpriteBatch
        SpriteBatch spriteBatch;

        // Person 
        Vector2 personPosition;
        const int PersonMoveSpeed = 5;

        // Blocks
        List<Vector2> blockPositions = new List<Vector2>();
        float BlockSpawnProbability = 0.01f;
        const int BlockFallSpeed = 2;

        Random random = new Random();

        // For when a collision is detected
        bool personHit = false;

        // The sub-rectangle of the drawable area which should be visible on all TVs
        Rectangle safeBounds;
        // Percentage of the screen on every side is the safe area
        const float SafeAreaPortion = 0.05f;


        public PerPixelCollisionGame()
        {
            graphics = new GraphicsDeviceManager(this);
			graphics.SynchronizeWithVerticalRetrace = false;
            Content.RootDirectory = "Content";
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to
        /// run. This is where it can query for any required services and load any
        /// non-graphic related content.  Calling base.Initialize will enumerate through
        /// any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Calculate safe bounds based on current resolution
            Viewport viewport = graphics.GraphicsDevice.Viewport;
            safeBounds = new Rectangle(
                (int)(viewport.Width * SafeAreaPortion),
                (int)(viewport.Height * SafeAreaPortion),
                (int)(viewport.Width * (1 - 2 * SafeAreaPortion)),
                (int)(viewport.Height * (1 - 2 * SafeAreaPortion)));
            // Start the player in the center along the bottom of the screen
            personPosition.X = (safeBounds.Width - personTexture.Width) / 2;
            personPosition.Y = safeBounds.Height - personTexture.Height;
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load textures
            blockTexture = Content.Load<Texture2D>("Block");
            personTexture = Content.Load<Texture2D>("Character");
            //personTexture = Content.Load<Texture2D>("Warrior3Active");
			characterTexture = new RenderTarget2D(GraphicsDevice, personTexture.Width, personTexture.Height);
            // Extract collision data
            blockTextureData =
                new Color[blockTexture.Width * blockTexture.Height];
            blockTexture.GetData(blockTextureData);
            personTextureData =
                new Color[personTexture.Width * personTexture.Height];
            personTexture.GetData(personTextureData);
			characterTexture.SetData(personTextureData);
            characterTextureData =
                new Color[characterTexture.Width * characterTexture.Height];
			characterTexture.GetData(characterTextureData);
		//personTexture.SetData(characterTextureData);
            // Create a sprite batch to draw those textures
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Get input
            KeyboardState keyboard = Keyboard.GetState();
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (gamePad.Buttons.Back == ButtonState.Pressed ||
                keyboard.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            // Move the player left and right with arrow keys or d-pad
            if (keyboard.IsKeyDown(Keys.Left) ||
                gamePad.DPad.Left == ButtonState.Pressed)
            {
                personPosition.X -= PersonMoveSpeed;
            }
            if (keyboard.IsKeyDown(Keys.Right) ||
                gamePad.DPad.Right == ButtonState.Pressed)
            {
                personPosition.X += PersonMoveSpeed;
            }

            // Prevent the person from moving off of the screen
            personPosition.X = MathHelper.Clamp(personPosition.X,
                safeBounds.Left, safeBounds.Right - personTexture.Width);

            // Spawn new falling blocks
            if (random.NextDouble() < BlockSpawnProbability)
            {
                float x = (float)random.NextDouble() *
                    (Window.ClientBounds.Width - blockTexture.Width);
                blockPositions.Add(new Vector2(x, -blockTexture.Height));
            }

            // Get the bounding rectangle of the person
            Rectangle personRectangle =
                new Rectangle((int)personPosition.X, (int)personPosition.Y,
                personTexture.Width, personTexture.Height);

            // Update each block
            personHit = false;
            for (int i = 0; i < blockPositions.Count; i++)
            {
                // Animate this block falling
                blockPositions[i] =
                    new Vector2(blockPositions[i].X,
                                blockPositions[i].Y + BlockFallSpeed);

                // Get the bounding rectangle of this block
                Rectangle blockRectangle =
                    new Rectangle((int)blockPositions[i].X, (int)blockPositions[i].Y,
                    blockTexture.Width, blockTexture.Height);

                // Check collision with person
                if (IntersectPixels(personRectangle, personTextureData,
                                    blockRectangle, blockTextureData))
                {
                    personHit = true;
                }

                // Remove this block if it have fallen off the screen
                if (blockPositions[i].Y > Window.ClientBounds.Height)
                {
                    blockPositions.RemoveAt(i);

                    // When removing a block, the next block will have the same index
                    // as the current block. Decrement i to prevent skipping a block.
                    i--;
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            // Change the background to red when the person was hit by a block
            if (personHit)
            {
                device.Clear(Color.Red);
            }
            else
            {
                device.Clear(Color.CornflowerBlue);
            }

            spriteBatch.Begin();

            // Draw person
            spriteBatch.Draw(personTexture, personPosition, Color.White);
			Vector2 characterPosition = personPosition;
			characterPosition.X += personTexture.Width;
			spriteBatch.Draw(characterTexture, characterPosition, Color.White);
			
            // Draw blocks
            foreach (Vector2 blockPosition in blockPositions)
                spriteBatch.Draw(blockTexture, blockPosition, Color.White);

            spriteBatch.End();


            base.Draw(gameTime);
        }


        /// <summary>
        /// Determines if there is overlap of the non-transparent pixels
        /// between two sprites.
        /// </summary>
        /// <param name="rectangleA">Bounding rectangle of the first sprite</param>
        /// <param name="dataA">Pixel data of the first sprite</param>
        /// <param name="rectangleB">Bouding rectangle of the second sprite</param>
        /// <param name="dataB">Pixel data of the second sprite</param>
        /// <returns>True if non-transparent pixels overlap; false otherwise</returns>
        static bool IntersectPixels(Rectangle rectangleA, Color[] dataA,
                                    Rectangle rectangleB, Color[] dataB)
        {
            // Find the bounds of the rectangle intersection
            int top = Math.Max(rectangleA.Top, rectangleB.Top);
            int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
            int left = Math.Max(rectangleA.Left, rectangleB.Left);
            int right = Math.Min(rectangleA.Right, rectangleB.Right);

            // Check every point within the intersection bounds
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    // Get the color of both pixels at this point
                    Color colorA = dataA[(x - rectangleA.Left) +
                                         (y - rectangleA.Top) * rectangleA.Width];
                    Color colorB = dataB[(x - rectangleB.Left) +
                                         (y - rectangleB.Top) * rectangleB.Width];

                    // If both pixels are not completely transparent,
                    if (colorA.A != 0 && colorB.A != 0)
                    {
                        // then an intersection has been found
                        return true;
                    }
                }
            }

            // No intersection found
            return false;
        }
    }
}
