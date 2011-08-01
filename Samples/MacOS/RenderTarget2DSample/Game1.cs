using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace RenderTarget2DSample
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		/// <summary>
		/// The GraphicsDeviceManager is what creates and automagically manages the game's GraphicsDevice.
		/// </summary>
		GraphicsDeviceManager graphics;

		/// <summary>
		/// We use SpriteBatch to draw all of our 2D graphics.
		/// </summary>
		SpriteBatch spriteBatch;

		/// <summary>
		/// This is the rendertarget we'll be drawing to.
		/// </summary>
		RenderTarget2D renderTarget;

		/// <summary>
		/// This is a texture we'll be using to load a picture of Seamus the dog.
		/// </summary>
		Texture2D mooTheMerciless;

		/// <summary>
		/// This is a texture we'll be using to load a picture of a tileable wood surface.
		/// </summary>
		Texture2D wood;
		bool oneTimeOnly = true;

		/// <summary>
		/// The constructor for our Game1 class.
		/// </summary>
		public Game1 ()
		{
			// Create the GraphicsDeviceManager for our game.
			graphics = new GraphicsDeviceManager (this);

			// Set the root directory of the game's ContentManager to the "Content" folder.
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize ()
		{
			// We don't have anything to initialize.

			base.Initialize ();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent ()
		{

			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);

			// Create a rendertarget that matches the back buffer's dimensions, does not generate mipmaps automatically
			// (the Reach profile requires power of 2 sizing in order to do that), uses an RGBA color format, and
			// has no depth buffer or stencil buffer.
			renderTarget = new RenderTarget2D (GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
				GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

			// Load in the picture of Seamus.
			mooTheMerciless = Content.Load<Texture2D> ("MooTheMerciless");

			// Load in our wood tile.
			wood = Content.Load<Texture2D> ("wood");
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent ()
		{
			// While not strictly necessary, you should always dispose of assets you create
			// (excluding those you load the the ContentManager) when those assets implement
			// IDisposable. RenderTarget2D is one such asset type, so we dispose of it properly.
			if (renderTarget != null) {
				// We put this in a try-catch block. The reason is that if for some odd reason this failed
				// (e.g. we were using threading and nulled out renderTarget on some other thread),
				// then none of the rest of the UnloadContent method would run. Here it doesn't make a
				// difference, but it's good practice nonethless.
				try {
					renderTarget.Dispose ();
					renderTarget = null;
				} catch {
				}
			}
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime)
		{
			// Allows the game to exit. If this is a Windows version, I also like to check for an Esc key press. I put
			// it within an #if WINDOWS .. #endif block since that way it won't run on other platforms.
			if (GamePad.GetState (PlayerIndex.One).Buttons.Back == ButtonState.Pressed
			//#if WINDOWS
 || Keyboard.GetState ().IsKeyDown (Keys.Escape)
//#endif
) {
				this.Exit ();
			}

			// We don't have any update logic since this is just an example usage of RenderTarget2D

			base.Update (gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			
			// A one time only flag to help test for memory leaks
			if (oneTimeOnly) {
				
				oneTimeOnly = false;

				// Set renderTarget as the surface to draw to instead of the back buffer
				GraphicsDevice.SetRenderTarget (renderTarget);

				// Clear the renderTarget. By default it's all a bright purple color. I like to use Color.Transparent to
				// enable easy alpha blending.
				GraphicsDevice.Clear (Color.Transparent);
				Vector2 woodPosition = Vector2.Zero;

				// Begin drawing
				spriteBatch.Begin ();

				int xBlank = 0;
				int yBlank = 0;

				// Use nested do-whiles to fill the rendertarget with tiles. We use some trickery to draw every other tile
				do {
					do {
						// We use the modulus operator to get the remainder of dividing xBlank by 2. If xBlank is odd, it'll
						// return 1 and the spriteBatch.Draw call gets skipped. If it's even, it'll return 0 so
						// spriteBatch.Draw will get called and it'll draw a tile there.
						if (xBlank % 2 == 0) {
							spriteBatch.Draw (wood, woodPosition, Color.White);
						}
						// Increment xBlank by one so that every other tile will get drawn.
						xBlank++;
						// Increase the X coordinate of where we'll draw the wood tile in order to progressively draw
						// each column of tiles.
						woodPosition.X += wood.Width;

						// We draw so long as woodPosition.X is less than our renderTarget's width
					} while (woodPosition.X < renderTarget.Width);

					// We increment yBlank by one. Why is explained below.
					yBlank++;

					// We use the modulus operater to get the remainder of dividing yBlank by 2. If yBlank is odd, we reset
					// xBlank to 1. If it's even, we reset xBlank to 0. This way each row shifts by one so that the tiles
					// are drawn in a checkered pattern rather than in columns.
					if (yBlank % 2 == 0) {
						xBlank = 0;
					} else {
						xBlank = 1;
					}

					// Reset woodPosition.X to zero so that we start drawing from the beginning of the next row.
					woodPosition.X = 0;

					// Increase the Y coord of where we'll draw the wood tile in order to progressively draw each
					// row of tiles.
					woodPosition.Y += wood.Height;

					// We draw so long as woodPosition.Y is less than our renderTarget's width
				} while (woodPosition.Y < renderTarget.Height);

				// Now that we've drawn the wood tiles, we draw Moo the Merciless. We draw him centered in the rendertarget.
				spriteBatch.Draw (mooTheMerciless, 
				new Vector2 ((renderTarget.Width / 2f) - (mooTheMerciless.Width / 2f), (renderTarget.Height / 2f) - (mooTheMerciless.Height / 2f)), 
				Color.White);

				// End the spriteBatch draw.
				spriteBatch.End ();

				// Switch back to drawing onto the back buffer
				GraphicsDevice.SetRenderTarget (null);
			}
			// Now that we're back to drawing onto the back buffer, we want to clear it. If we had done so earlier
			// then when we switched to drawing to the render target, the old back buffer would've just be filled with
			// that bright purple color when we came back to it.
			GraphicsDevice.Clear (Color.CornflowerBlue);

			// Ok. At this point we have everything we drew in renderTarget, which we can use just like a regular Texture2D.
			// To make it look more interesting, we're going to scale up and down based on total elapsed time.
			float scale = 1.0f;

			if (gameTime.TotalGameTime.TotalSeconds % 10 < 5.0) {
				// We're running on a ten second scale timer. For the first five second we scale down from 1f to
				// no less than 0.01f.
				scale = MathHelper.Clamp (1f - (((float)gameTime.TotalGameTime.TotalSeconds % 5) / 5f), 0.01f, 1f);
			} else {
				// For the second five seconds, we scale up from no less than 0.01f up to 1f.
				scale = MathHelper.Clamp (((float)gameTime.TotalGameTime.TotalSeconds % 5) / 5f, 0.01f, 1f);
			}

			// Start spriteBatch again (this time drawing to the back buffer)
			spriteBatch.Begin ();

			// Now we draw our render target to the back buffer so that it will get displayed on the screen. We
			// position it in the center of the screen, but we make the origin be the center of the render target
			// such that it actually gets drawn centered (as opposed to shrinking and exanding with the left corner
			// in the center). We use our scale computation, and specify no SpriteEffects and an unused 0f for layer
			// depth
			spriteBatch.Draw (renderTarget, 
				new Vector2 (GraphicsDevice.PresentationParameters.BackBufferWidth / 2, GraphicsDevice.PresentationParameters.BackBufferHeight / 2), 
				null, Color.White, 0f, new Vector2 (renderTarget.Width / 2, renderTarget.Height / 2), scale, 
				SpriteEffects.None, 0f);

			// End our spriteBatch call.
			spriteBatch.End ();

			base.Draw (gameTime);
		}
	}
}
