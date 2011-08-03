using System;
using System.Collections.Generic;

#if ANDROID
using Android.App;
#endif

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Samples.Sound
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;	
		KeyboardState oldSate;
		SoundEffect sound;
		SpriteBatch spriteBatch;
		SoundEffectInstance soundInst;
		SpriteFont font;
		
#if ANDROID 
		public Game1 (Activity activity) : base (activity)
#else 
        public Game1 ()  
#endif
		{
			graphics = new GraphicsDeviceManager (this);
			
			Content.RootDirectory = "Content";
			
			graphics.PreferMultiSampling = true;
			graphics.IsFullScreen = false;	

			Window.AllowUserResizing = false;

			graphics.SupportedOrientations = DisplayOrientation.Portrait | DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight | DisplayOrientation.PortraitUpsideDown;
		}
		
		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize ()
		{
			// TODO: Add your initialization logic here
			
			base.Initialize ();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			sound = Content.Load<SoundEffect>("Explosion1");
			soundInst = sound.CreateInstance();
			
			font = Content.Load<SpriteFont>("spriteFont1");
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime)
		{
			KeyboardState ks = Keyboard.GetState();
			
			
			if (ks[Keys.Escape] == KeyState.Down)
				base.Exit();
			
			if (ks.IsKeyDown(Keys.A) && oldSate.IsKeyUp(Keys.A))
				soundInst.Play();
			
			if (ks.IsKeyDown(Keys.B) && oldSate.IsKeyUp(Keys.B))
				soundInst.Stop();
			
			if (ks.IsKeyDown(Keys.C) && oldSate.IsKeyUp(Keys.C))
				soundInst.Pause();
			
			if (ks.IsKeyDown(Keys.D) && oldSate.IsKeyUp(Keys.D))
				soundInst.IsLooped = !soundInst.IsLooped;
			
			if (ks.IsKeyDown(Keys.E) && oldSate.IsKeyUp(Keys.E))
				soundInst.Stop(true);			
			
			if (ks.IsKeyDown(Keys.X))
				soundInst.Volume = MathHelper.Clamp(soundInst.Volume + 0.01f, 0f, 1f);
			else if (ks.IsKeyDown(Keys.Z))
				soundInst.Volume = MathHelper.Clamp(soundInst.Volume - 0.01f, 0f, 1f);;
			
			oldSate = ks;

			base.Update (gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);

			spriteBatch.Begin();

			spriteBatch.DrawString(font, "A: play\nB: stop\nC: pause\nD: toggle looping\nE: immediate stop\nX/Z volume\nStatus: " +
			                       soundInst.State.ToString() + "\nLooping: " +
			                       soundInst.IsLooped.ToString() + "\nVolume: " +
			                       soundInst.Volume.ToString()
			                       , Vector2.Zero, Color.White);
			
			base.Draw(gameTime);
			
			spriteBatch.End();
		}
	}
}
