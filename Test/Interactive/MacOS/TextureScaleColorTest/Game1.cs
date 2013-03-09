using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;




namespace MonoTest
{
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		static GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		public static Texture2D BlankTexture;
		public static SpriteFont FontCalibri14;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			graphics.PreferredBackBufferWidth =  640;;
			graphics.PreferredBackBufferHeight = 480;
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

			FontCalibri14 = Content.Load<SpriteFont>("FontCalibri14");

			BlankTexture = Content.Load<Texture2D>("Blank");

//			BlankTexture = new Texture2D(GraphicsDevice,1,1);
//			BlankTexture.SetData(new Color[] {Color.White});
		}


		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			base.Draw(gameTime);
			spriteBatch.Begin();
			spriteBatch.Draw(BlankTexture,
				new Vector2(100,100),
				null,
				Color.Green,
				0,
				Vector2.Zero,
				new Vector2(400,240),
				SpriteEffects.None,
				0);
            spriteBatch.DrawString(FontCalibri14, "There should be no transparency\nnear the corners?\n\nThis box should be solid green.", new Vector2(150,150), Color.Silver, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
			spriteBatch.End();
		}
	}
}
