using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;




namespace SoundTest
{
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		Texture2D tExplosion;
		SoundEffect sExplosion;
		Random rnd = new Random();
		
		class explosion
		{
			public Vector2 Position;
			public float Size;
			
			public explosion(Vector2 pos)
			{
				Position = pos;
				Size = 1f;
			}
		}
		
		List<explosion> Explosions = new List<explosion>();
		TimeSpan timer = TimeSpan.Zero;
		int Interval = 1000; //Milliseconds. The duration of the sound is about 1.5 sec.

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			graphics.PreferredBackBufferWidth =  1024;;
			graphics.PreferredBackBufferHeight = 768;
			graphics.IsFullScreen = false;
		}

		protected override void Initialize()
		{

			base.Initialize();

		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);

			sExplosion = Content.Load<SoundEffect>("ExplosionSound");
			//sExplosion = Content.Load<SoundEffect>("laser1");
			//sExplosion = Content.Load<SoundEffect>("FillingHoneyPot_Loop");
			tExplosion = Content.Load<Texture2D>("Explosion");
		}


		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			//update explosions
			for (int i = 0; i<Explosions.Count;i++)
			{
				Explosions[i].Size -= 0.01f;
				if (Explosions[i].Size < 0.1f)
				{
					Explosions.RemoveAt(i);
					i--;
				}
			}
			
			//Check for next explosion
			timer += gameTime.ElapsedGameTime;
			if (timer.TotalMilliseconds > Interval)
			{
				timer = TimeSpan.Zero;
				float x = rnd.Next(24,1000);
				Explosions.Add(new explosion(new Vector2(x, rnd.Next(50,700))));
				sExplosion.Play(1f, 1f, (x / 512f) -1);
			}
			//Check for exit
			KeyboardState state = new KeyboardState();
			state = Keyboard.GetState();
			if (state.IsKeyDown(Keys.Escape) || state.IsKeyDown(Keys.Space) || state.IsKeyDown(Keys.Enter))
				this.Exit();

		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			base.Draw(gameTime);
			spriteBatch.Begin();
			//Draw explosions
			foreach (explosion e in Explosions)
				spriteBatch.Draw(tExplosion, e.Position, null, Color.White, 0, Vector2.Zero,e.Size, SpriteEffects.None, 1);
			spriteBatch.End();
		}
	}
}
