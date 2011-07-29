#region File Description
//-----------------------------------------------------------------------------
// SpriteEffects.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace SpriteEffects
{
	/// <summary>
	/// Sample demonstrating how shaders can be used to
	/// apply special effects to sprite rendering.
	/// </summary>
	public class SpriteEffectsGame : Microsoft.Xna.Framework.Game
	{
	#region Fields


		GraphicsDeviceManager graphics;
		KeyboardState lastKeyboardState = new KeyboardState ();
		GamePadState lastGamePadState = new GamePadState ();
		KeyboardState currentKeyboardState = new KeyboardState ();
		GamePadState currentGamePadState = new GamePadState ();


		// Enum describes all the rendering techniques used in this demo.
		enum DemoEffect
		{
			Desaturate,
			Disappear,
			RefractCat,
			RefractGlacier,
			Normalmap,
		}

		DemoEffect currentEffect = DemoEffect.RefractGlacier;

		// Effects used by this sample.
		Effect desaturateEffect;
		Effect disappearEffect;
		Effect normalmapEffect;
		Effect refractionEffect;


		// Textures used by this sample.
		Texture2D catTexture;
		Texture2D catNormalmapTexture;
		Texture2D glacierTexture;
		Texture2D waterfallTexture;


		// SpriteBatch instance used to render all the effects.
		SpriteBatch spriteBatch;

	#endregion

	#region Initialization


		public SpriteEffectsGame ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";
		}


		/// <summary>
		/// Loads graphics content.
		/// </summary>
		protected override void LoadContent ()
		{
			//desaturateEffect = Content.Load<Effect> ("desaturate");
			//disappearEffect = Content.Load<Effect> ("disappear");
			//normalmapEffect = Content.Load<Effect> ("normalmap");
			//refractionEffect = Content.Load<Effect> ("refraction");
			desaturateEffect = new DesaturateEffect(GraphicsDevice);
			disappearEffect = new DisappearEffect(GraphicsDevice);
			normalmapEffect = new NormalmapEffect(GraphicsDevice);
			refractionEffect = new RefractionEffect(GraphicsDevice);
			catTexture = Content.Load<Texture2D> ("cat");
			catNormalmapTexture = Content.Load<Texture2D> ("cat_normalmap");
			glacierTexture = Content.Load<Texture2D> ("glacier");
			waterfallTexture = Content.Load<Texture2D> ("waterfall");

			spriteBatch = new SpriteBatch (graphics.GraphicsDevice);
		}


	#endregion

	#region Update and Draw


		/// <summary>
		/// Allows the game to run logic.
		/// </summary>
		protected override void Update (GameTime gameTime)
		{
			HandleInput ();

			if (NextButtonPressed ()) {
				currentEffect++;

				if (currentEffect > DemoEffect.Normalmap)
					currentEffect = 0;
			}

			base.Update (gameTime);
		}


		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		protected override void Draw (GameTime gameTime)
		{
			switch (currentEffect) {
			case DemoEffect.Desaturate:
				DrawDesaturate (gameTime);
				break;

			case DemoEffect.Disappear:
				DrawDisappear (gameTime);
				break;

			case DemoEffect.Normalmap:
				DrawNormalmap (gameTime);
				break;

			case DemoEffect.RefractCat:
				DrawRefractCat (gameTime);
				break;

			case DemoEffect.RefractGlacier:
				DrawRefractGlacier (gameTime);
				break;
			}

			base.Draw (gameTime);
		}


		/// <summary>
		/// Effect dynamically changes color saturation.
		/// </summary>
		void DrawDesaturate (GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.White);
			// Begin the sprite batch, using our custom effect.
			spriteBatch.Begin (0, null, null, null, null, desaturateEffect);
			
			// Draw four copies of the same sprite with different saturation levels.
			// The saturation amount is passed into the effect using the alpha of the
			// SpriteBatch.Draw color parameter. This isn't as flexible as using a
			// regular effect parameter, but makes it easy to draw many sprites with
			// a different saturation amount for each. If we had used an effect
			// parameter for this, we would have to end the sprite batch, then begin
			// a new one, each time we wanted to change the saturation setting.

			byte pulsate = (byte)Pulsate (gameTime, 4, 0, 255);

			spriteBatch.Draw (glacierTexture, 
				QuarterOfScreen (0, 0), 
				new Color (255, 255, 255, 0));

			spriteBatch.Draw (glacierTexture, 
				QuarterOfScreen (1, 0), 
				new Color (255, 255, 255, 64));

			spriteBatch.Draw (glacierTexture, 
				QuarterOfScreen (0, 1), 
				new Color (255, 255, 255, 255));

			spriteBatch.Draw (glacierTexture, 
				QuarterOfScreen (1, 1), 
				new Color (255, 255, 255, pulsate));

			// End the sprite batch.
			spriteBatch.End ();
		}


		/// <summary>
		/// Effect uses a scrolling overlay texture to make different parts of
		/// an image fade in or out at different speeds.
		/// </summary>
		void DrawDisappear (GameTime gameTime)
		{
			//GraphicsDevice.Clear(Color.White);
			// Draw the background image.
			spriteBatch.Begin ();
			spriteBatch.Draw (glacierTexture, GraphicsDevice.Viewport.Bounds, Color.White);
			spriteBatch.End ();

			// Set an effect parameter to make our overlay
			// texture scroll in a giant circle.
			//TextureSampler 
			disappearEffect.Parameters ["OverlayScroll"].SetValue (
						MoveInCircle (gameTime, 0.8f) * 0.25f);
			
			// Set the overlay texture (as texture 1,
			// because 0 will be the main sprite texture).
			graphics.GraphicsDevice.Textures [1] = waterfallTexture;

			// Begin the sprite batch.
			spriteBatch.Begin (0, null, null, null, null, disappearEffect);

			// Draw the sprite, passing the fade amount as the
			// alpha of the SpriteBatch.Draw color parameter.
			byte fade = (byte)Pulsate (gameTime, 2, 0, 255);



			spriteBatch.Draw (catTexture, 
				MoveInCircle (gameTime, catTexture, 1), 
				new Color (255, 255, 255, fade));			

			// End the sprite batch.
			spriteBatch.End ();
		}


		/// <summary>
		// Effect uses a scrolling displacement texture to offset the position of
		// the main texture.
		/// </summary>
		void DrawRefractCat (GameTime gameTime)
		{
			// Draw the background image.
			spriteBatch.Begin ();
			spriteBatch.Draw (glacierTexture, GraphicsDevice.Viewport.Bounds, Color.White);
			spriteBatch.End ();

			// Set an effect parameter to make the
			// displacement texture scroll in a giant circle.
			refractionEffect.Parameters ["DisplacementScroll"].SetValue (
							MoveInCircle (gameTime, 0.1f));

			// Set the displacement texture.
			graphics.GraphicsDevice.Textures [1] = waterfallTexture;

			// Begin the sprite batch.
			spriteBatch.Begin (0, null, null, null, null, refractionEffect);

			// Draw the sprite.
			spriteBatch.Draw (catTexture, 
				MoveInCircle (gameTime, catTexture, 1), 
				Color.White);

			// End the sprite batch.
			spriteBatch.End ();
		}


		/// <summary>
		// Effect uses a scrolling displacement texture to offset the position of
		// the main texture.
		/// </summary>
		void DrawRefractGlacier (GameTime gameTime)
		{

			// Set an effect parameter to make the
			// displacement texture scroll in a giant circle.
			refractionEffect.Parameters ["DisplacementScroll"].SetValue (
							MoveInCircle (gameTime, 0.2f));

			// Set the displacement texture.
			graphics.GraphicsDevice.Textures [1] = glacierTexture;

			// Begin the sprite batch.
			spriteBatch.Begin (0, null, null, null, null, refractionEffect);
			//spriteBatch.Begin();
			
			// Because the effect will displace the texture coordinates before
			// sampling the main texture, the coordinates could sometimes go right
			// off the edges of the texture, which looks ugly. To prevent this, we
			// adjust our sprite source region to leave a little border around the
			// edge of the texture. The displacement effect will then just move the
			// texture coordinates into this border region, without ever hitting
			// the edge of the texture.

			Rectangle croppedGlacier = new Rectangle (32, 32,
						glacierTexture.Width - 64,
						glacierTexture.Height - 64);
			spriteBatch.Draw (glacierTexture, 
				GraphicsDevice.Viewport.Bounds, 
				//croppedGlacier, 
				Color.White);

			// End the sprite batch.
			spriteBatch.End ();
		}


		/// <summary>
		/// Effect uses a normalmap texture to apply realtime lighting while
		/// drawing a 2D sprite.
		/// </summary>
		void DrawNormalmap (GameTime gameTime)
		{
			// Draw the background image.
			spriteBatch.Begin ();
			spriteBatch.Draw (glacierTexture, GraphicsDevice.Viewport.Bounds, Color.White);
			spriteBatch.End ();

			// Animate the light direction.
			Vector2 spinningLight = MoveInCircle (gameTime, 1.5f);

			double time = gameTime.TotalGameTime.TotalSeconds;

			float tiltUpAndDown = 0.5f + (float)Math.Cos (time * 0.75) * 0.1f;

			Vector3 lightDirection = new Vector3 (spinningLight * tiltUpAndDown,
						1 - tiltUpAndDown);

			lightDirection.Normalize ();

			normalmapEffect.Parameters ["LightDirection"].SetValue (lightDirection);

			// Set the normalmap texture.
			graphics.GraphicsDevice.Textures [1] = catNormalmapTexture;

			// Begin the sprite batch.
			spriteBatch.Begin (0, null, null, null, null, normalmapEffect);

			// Draw the sprite.
			spriteBatch.Draw (catTexture, CenterOnScreen (catTexture), Color.Azure);

			// End the sprite batch.
			spriteBatch.End ();
			// End the sprite batch.
			spriteBatch.End ();			
		}


		/// <summary>
		/// Helper calculates the destination rectangle needed
		/// to draw a sprite to one quarter of the screen.
		/// </summary>
		Rectangle QuarterOfScreen (int x, int y)
		{
			Viewport viewport = graphics.GraphicsDevice.Viewport;

			int w = viewport.Width / 2;
			int h = viewport.Height / 2;

			return new Rectangle (w * x, h * y, w, h);
		}


		/// <summary>
		/// Helper calculates the destination position needed
		/// to center a sprite in the middle of the screen.
		/// </summary>
		Vector2 CenterOnScreen (Texture2D texture)
		{
			Viewport viewport = graphics.GraphicsDevice.Viewport;

			int x = (viewport.Width - texture.Width) / 2;
			int y = (viewport.Height - texture.Height) / 2;

			return new Vector2 (x, y);
		}


		/// <summary>
		/// Helper computes a value that oscillates over time.
		/// </summary>
		static float Pulsate (GameTime gameTime, float speed, float min, float max)
		{
			double time = gameTime.TotalGameTime.TotalSeconds * speed;

			return min + ((float)Math.Sin (time) + 1) / 2 * (max - min);
		}


		/// <summary>
		/// Helper for moving a value around in a circle.
		/// </summary>
		static Vector2 MoveInCircle (GameTime gameTime, float speed)
		{
			double time = gameTime.TotalGameTime.TotalSeconds * speed;

			float x = (float)Math.Cos (time);
			float y = (float)Math.Sin (time);

			return new Vector2 (x, y);
		}


		/// <summary>
		/// Helper for moving a sprite around in a circle.
		/// </summary>
		Vector2 MoveInCircle (GameTime gameTime, Texture2D texture, float speed)
		{
			Viewport viewport = graphics.GraphicsDevice.Viewport;

			float x = (viewport.Width - texture.Width) / 2;
			float y = (viewport.Height - texture.Height) / 2;

			return MoveInCircle (gameTime, speed) * 128 + new Vector2 (x, y);
		}


	#endregion

	#region Handle Input


		/// <summary>
		/// Handles input for quitting the game.
		/// </summary>
		private void HandleInput ()
		{
			lastKeyboardState = currentKeyboardState;
			lastGamePadState = currentGamePadState;

			currentKeyboardState = Keyboard.GetState ();
			currentGamePadState = GamePad.GetState (PlayerIndex.One);

			// Check for exit.
			if (currentKeyboardState.IsKeyDown (Keys.Escape) || 
		currentGamePadState.Buttons.Back == ButtonState.Pressed) {
				Exit ();
			}
		}


		/// <summary>
		/// Checks whether the user wants to advance to the next rendering effect.
		/// </summary>
		bool NextButtonPressed ()
		{
			// Have they pressed the space bar?
			if (currentKeyboardState.IsKeyDown (Keys.Space) && 
		!lastKeyboardState.IsKeyDown (Keys.Space))
				return true;

			// Have they pressed the gamepad A button?
			if (currentGamePadState.Buttons.A == ButtonState.Pressed && 
		lastGamePadState.Buttons.A == ButtonState.Released)
				return true;

			return false;
		}


	#endregion
	}

}
