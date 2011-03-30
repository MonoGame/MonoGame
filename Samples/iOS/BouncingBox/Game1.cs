using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Samples.BouncingBox
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;		
		Texture2D texture;
		Vector2 position;
		Vector2 speed;
		Random randomizer;
		Color backColor;

		public Game1 ()
			{
			randomizer = new Random (DateTime.Now.TimeOfDay.Milliseconds);
			speed = new Vector2 (5 + randomizer.Next (10),5 + randomizer.Next (10));
			position = new Vector2 (250,400);
			GetNewColor ();

			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";
			
			graphics.IsFullScreen = true;		
		}

		private void GetNewColor ()
		{
			backColor = new Color ((byte)randomizer.Next (255),(byte)randomizer.Next (255),(byte)randomizer.Next (255),255);
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
			spriteBatch = new SpriteBatch (GraphicsDevice);

			// TODO: use this.Content to load your game content here
			texture = Content.Load<Texture2D> ("monogameicon");
		}
		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime)
		{
			// TODO: Add your update logic here

			if (texture != null) {
				//  Keep inside the screen
				//  right
				if (position.X + texture.Width + speed.X > Window.ClientBounds.Width) {
					GetNewColor ();
					speed.X = -speed.X;
				}
				//  bottom
				if (position.Y + texture.Height + speed.Y > Window.ClientBounds.Height) {
					GetNewColor ();
					speed.Y = -speed.Y;
				}
				//  left
				if (position.X + speed.X < 0) {
					GetNewColor ();
					speed.X = -speed.X;
				}
				//  top
				if (position.Y + speed.Y < 0) {
					GetNewColor ();
					speed.Y = -speed.Y;
				}
				//  update position
				position += speed;

			}
			base.Update (gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (backColor);

			spriteBatch.Begin ();
			if (texture != null)
				spriteBatch.Draw (texture, position, Color.White);			
			spriteBatch.End ();

			base.Draw (gameTime);
		}
	}
}
