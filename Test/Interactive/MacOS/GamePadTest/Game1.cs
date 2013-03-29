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

namespace GamePadTest
{
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";

		}

		protected override void Initialize ()
		{
			// TODO: Add your initialization logic here

			base.Initialize ();
		}

		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);
			Services.AddService (typeof(SpriteBatch), spriteBatch);
		}

		protected override void UnloadContent ()
		{
			// TODO: Unload any non ContentManager content here
		}

		protected override void Update (GameTime gameTime)
		{

			base.Update (gameTime);
		}

		protected override void Draw (GameTime gameTime)
		{

			GraphicsDevice.Clear (Color.CornflowerBlue);

			base.Draw(gameTime);

		}

	}
}
