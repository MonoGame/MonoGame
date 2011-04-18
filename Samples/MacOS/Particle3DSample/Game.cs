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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Particle3DSample
{
	/// <summary>
	/// Sample showing how to implement a particle system entirely
	/// on the GPU, using the vertex shader to animate particles.
	/// </summary>
	public class Particle3DSampleGame : Microsoft.Xna.Framework.Game
	{
	#region Fields


		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		SpriteFont font;
		//Model grid;


		// This sample uses five different particle systems.
		ParticleSystem explosionParticles;
		ParticleSystem explosionSmokeParticles;
		ParticleSystem projectileTrailParticles;
		ParticleSystem smokePlumeParticles;
		ParticleSystem fireParticles;


		// The sample can switch between three different visual effects.
		enum ParticleState
		{
			Explosions,
			SmokePlume,
			RingOfFire,
		};

		ParticleState currentState = ParticleState.Explosions;


		// The explosions effect works by firing projectiles up into the
		// air, so we need to keep track of all the active projectiles.
		List<Projectile> projectiles = new List<Projectile> ();
		TimeSpan timeToNextProjectile = TimeSpan.Zero;


		// Random number generator for the fire effect.
		Random random = new Random ();


		// Input state.
		KeyboardState currentKeyboardState;
		GamePadState currentGamePadState;
		KeyboardState lastKeyboardState;
		GamePadState lastGamePadState;


		// Camera state.
		float cameraArc = -5;
		float cameraRotation = 0;
		float cameraDistance = 200;


	#endregion

	#region Initialization


		/// <summary>
		/// Constructor.
		/// </summary>
		public Particle3DSampleGame ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";

			// Construct our particle system components.
			explosionParticles = new ParticleSystem (this, Content, "ExplosionSettings");
			explosionSmokeParticles = new ParticleSystem (this, Content, "ExplosionSmokeSettings");
			projectileTrailParticles = new ParticleSystem (this, Content, "ProjectileTrailSettings");
			smokePlumeParticles = new ParticleSystem (this, Content, "SmokePlumeSettings");
			fireParticles = new ParticleSystem (this, Content, "FireSettings");

			// Set the draw order so the explosions and fire
			// will appear over the top of the smoke.
			smokePlumeParticles.DrawOrder = 100;
			explosionSmokeParticles.DrawOrder = 200;
			projectileTrailParticles.DrawOrder = 300;
			explosionParticles.DrawOrder = 400;
			fireParticles.DrawOrder = 500;

			// Register the particle system components.
			Components.Add (explosionParticles);
			Components.Add (explosionSmokeParticles);
			Components.Add (projectileTrailParticles);
			Components.Add (smokePlumeParticles);
			Components.Add (fireParticles);
		}


		/// <summary>
		/// Load your graphics content.
		/// </summary>
		protected override void LoadContent ()
		{
			spriteBatch = new SpriteBatch (graphics.GraphicsDevice);
			//font = Content.Load<SpriteFont> ("Arial");
			font = Content.Load<SpriteFont> ("font");
			//grid = Content.Load<Model> ("grid");
		}


	#endregion

	#region Update and Draw


		/// <summary>
		/// Allows the game to run logic.
		/// </summary>
		protected override void Update (GameTime gameTime)
		{
			HandleInput ();

			UpdateCamera (gameTime);

			switch (currentState) {
			case ParticleState.Explosions:
				UpdateExplosions (gameTime);
				break;

			case ParticleState.SmokePlume:
				UpdateSmokePlume ();
				break;

			case ParticleState.RingOfFire:
				UpdateFire ();
				break;
			}

			UpdateProjectiles (gameTime);

			base.Update (gameTime);
		}


		/// <summary>
		/// Helper for updating the explosions effect.
		/// </summary>
		void UpdateExplosions (GameTime gameTime)
		{
			timeToNextProjectile -= gameTime.ElapsedGameTime;

			if (timeToNextProjectile <= TimeSpan.Zero) {
				// Create a new projectile once per second. The real work of moving
				// and creating particles is handled inside the Projectile class.
				projectiles.Add (new Projectile (explosionParticles,
						explosionSmokeParticles,
						projectileTrailParticles));

				timeToNextProjectile += TimeSpan.FromSeconds (1);
			}
		}


		/// <summary>
		/// Helper for updating the list of active projectiles.
		/// </summary>
		void UpdateProjectiles (GameTime gameTime)
		{
			int i = 0;

			while (i < projectiles.Count) {
				if (!projectiles [i].Update (gameTime)) {
					// Remove projectiles at the end of their life.
					projectiles.RemoveAt (i);
				} else {
					// Advance to the next projectile.
					i++;
				}
			}
		}


		/// <summary>
		/// Helper for updating the smoke plume effect.
		/// </summary>
		void UpdateSmokePlume ()
		{
			// This is trivial: we just create one new smoke particle per frame.
			smokePlumeParticles.AddParticle (Vector3.Zero, Vector3.Zero);
		}


		/// <summary>
		/// Helper for updating the fire effect.
		/// </summary>
		void UpdateFire ()
		{
		const
			int fireParticlesPerFrame = 20 ; 

			// Create a number of fire particles, randomly positioned around a circle.
			for (int i = 0; i < fireParticlesPerFrame; i++) {
				fireParticles.AddParticle (RandomPointOnCircle (), Vector3.Zero);
			}

			// Create one smoke particle per frmae, too.
			smokePlumeParticles.AddParticle (RandomPointOnCircle (), Vector3.Zero);
		}


		/// <summary>
		/// Helper used by the UpdateFire method. Chooses a random location
		/// around a circle, at which a fire particle will be created.
		/// </summary>
		Vector3 RandomPointOnCircle ()
		{
		const
			float radius = 30 ; 
		const
			float height = 40 ; 

			double angle = random.NextDouble () * Math.PI * 2;

			float x = (float)Math.Cos (angle);
			float y = (float)Math.Sin (angle);

			return new Vector3 (x * radius, y * radius + height, 0);
		}


		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		protected override void Draw (GameTime gameTime)
		{
			GraphicsDevice device = graphics.GraphicsDevice;

			device.Clear (Color.CornflowerBlue);

			// Compute camera matrices.
			float aspectRatio = (float)device.Viewport.Width / 
				(float)device.Viewport.Height;

			Matrix view = Matrix.CreateTranslation (0, -25, 0) * 
			Matrix.CreateRotationY (MathHelper.ToRadians (cameraRotation)) * 
			Matrix.CreateRotationX (MathHelper.ToRadians (cameraArc)) * 
			Matrix.CreateLookAt (new Vector3 (0, 0, -cameraDistance), 
						new Vector3 (0, 0, 0), Vector3.Up);

			Matrix projection = Matrix.CreatePerspectiveFieldOfView (MathHelper.PiOver4, 
								aspectRatio, 
								1, 10000);

			// Pass camera matrices through to the particle system components.
			explosionParticles.SetCamera (view, projection);
			explosionSmokeParticles.SetCamera (view, projection);
			projectileTrailParticles.SetCamera (view, projection);
			smokePlumeParticles.SetCamera (view, projection);
			fireParticles.SetCamera (view, projection);

			// Draw our background grid and message text.
			DrawGrid (view, projection);

			DrawMessage ();

			// This will draw the particle system components.
			base.Draw (gameTime);
		}


		/// <summary>
		/// Helper for drawing the background grid model.
		/// </summary>
		void DrawGrid (Matrix view, Matrix projection)
		{
			GraphicsDevice device = graphics.GraphicsDevice;

			device.BlendState = BlendState.Opaque;
			device.DepthStencilState = DepthStencilState.Default;
			device.SamplerStates [0] = SamplerState.LinearWrap;

			//grid.Draw (Matrix.Identity, view, projection);
		}


		/// <summary>
		/// Helper for drawing our message text.
		/// </summary>
		void DrawMessage ()
		{
			string message = string.Format ("Current effect: {0}!!!\n" + 
					"Hit the A button or space bar to switch.", 
					currentState);

			spriteBatch.Begin ();
			spriteBatch.DrawString (font, message, new Vector2 (50, 50), Color.White);
			spriteBatch.End ();
		}


	#endregion

	#region Handle Input


		/// <summary>
		/// Handles input for quitting the game and cycling
		/// through the different particle effects.
		/// </summary>
		void HandleInput ()
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

			// Check for changing the active particle effect.
			if (((currentKeyboardState.IsKeyDown (Keys.Space) && 
		(lastKeyboardState.IsKeyUp (Keys.Space))) || 
		((currentGamePadState.Buttons.A == ButtonState.Pressed)) && 
		(lastGamePadState.Buttons.A == ButtonState.Released))) {
				currentState++;

				if (currentState > ParticleState.RingOfFire)
					currentState = 0;
			}
		}


		/// <summary>
		/// Handles input for moving the camera.
		/// </summary>
		void UpdateCamera (GameTime gameTime)
		{
			float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

			// Check for input to rotate the camera up and down around the model.
			if (currentKeyboardState.IsKeyDown (Keys.Up) || 
		currentKeyboardState.IsKeyDown (Keys.W)) {
				cameraArc += time * 0.025f;
			}

			if (currentKeyboardState.IsKeyDown (Keys.Down) || 
		currentKeyboardState.IsKeyDown (Keys.S)) {
				cameraArc -= time * 0.025f;
			}

			cameraArc += currentGamePadState.ThumbSticks.Right.Y * time * 0.05f;

			// Limit the arc movement.
			if (cameraArc > 90.0f)
				cameraArc = 90.0f;
			else if (cameraArc < -90.0f)
				cameraArc = -90.0f;

			// Check for input to rotate the camera around the model.
			if (currentKeyboardState.IsKeyDown (Keys.Right) || 
		currentKeyboardState.IsKeyDown (Keys.D)) {
				cameraRotation += time * 0.05f;
			}

			if (currentKeyboardState.IsKeyDown (Keys.Left) || 
		currentKeyboardState.IsKeyDown (Keys.A)) {
				cameraRotation -= time * 0.05f;
			}

			cameraRotation += currentGamePadState.ThumbSticks.Right.X * time * 0.1f;

			// Check for input to zoom camera in and out.
			if (currentKeyboardState.IsKeyDown (Keys.Z))
				cameraDistance += time * 0.25f;

			if (currentKeyboardState.IsKeyDown (Keys.X))
				cameraDistance -= time * 0.25f;

			cameraDistance += currentGamePadState.Triggers.Left * time * 0.5f;
			cameraDistance -= currentGamePadState.Triggers.Right * time * 0.5f;

			// Limit the camera distance.
			if (cameraDistance > 500)
				cameraDistance = 500;
			else if (cameraDistance < 10)
				cameraDistance = 10;

			if (currentGamePadState.Buttons.RightStick == ButtonState.Pressed || 
		currentKeyboardState.IsKeyDown (Keys.R)) {
				cameraArc = -5;
				cameraRotation = 0;
				cameraDistance = 200;
			}
		}


	#endregion
	}


	#region Entry Point

	//    /// <summary>
	//    /// The main entry point for the application.
	//    /// </summary>
	//    static class Program
	//    {
	//        static void Main()
	//        {
	//            using (Particle3DSampleGame game = new Particle3DSampleGame())
	//            {
	//                game.Run();
	//            }
	//        }
	//    }

	#endregion
}
