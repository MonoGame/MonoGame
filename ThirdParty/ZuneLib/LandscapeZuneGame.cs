#if IPHONE
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#else
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif

namespace ZuneLib
{
	/*
	 * LandscapeZuneGame has some decent graphical logic behind it to achieve the
	 * correct orientation. Given that the XNA Framework doesn't support any notion
	 * of orientations, on the Zune we use a RenderTarget2D to capture all drawing
	 * and then use a SpriteBatch to draw that image rotated 90 degrees. This allows
	 * us to draw graphics the same whether we're on the Zune or PC.
	 * 
	 * We use the #if ZUNE to wrap those areas because the PC build of the game doesn't
	 * need that functionality since it can natively draw at 480x272.
	 */
	public class LandscapeZuneGame : Game
	{
#if !WINDOWS
		private RenderTarget2D renderTarget;
		private SpriteBatch spriteBatch;
#endif

		/// <summary>
		/// Gets the GraphicsDeviceManager for the game.
		/// </summary>
		public GraphicsDeviceManager Graphics { get; private set; }

		/// <summary>
		/// Gets or sets the landscape orientation desired for the game.
		/// </summary>
		public LandscapeOrientation Orientation { get; set; }

		public LandscapeZuneGame()
		{
			Graphics = new GraphicsDeviceManager(this)
           	{
           		PreferredBackBufferWidth = 480,
           		PreferredBackBufferHeight = 272
           	};

			IsMouseVisible = true;
		}

		/// <summary>
		/// Updates the ZuneInput class. Call this at the top of your game's Update method.
		/// </summary>
		protected void UpdateZuneInput()
		{
			ZuneInput.Update(Orientation);
		}

#if !WINDOWS
		protected override void LoadContent()
		{
			renderTarget = new RenderTarget2D(GraphicsDevice, 480, 272, 1, SurfaceFormat.Color);
			spriteBatch = new SpriteBatch(GraphicsDevice);

			base.LoadContent();
		}

		// override BeginDraw so we can set the render target and Viewport before
		// any game drawing occurs.
		protected override bool BeginDraw()
		{
			if (base.BeginDraw())
			{
				GraphicsDevice.SetRenderTarget(0, renderTarget);
				GraphicsDevice.Viewport = new Viewport
              	{
              		X = 0,
              		Y = 0,
              		Width = 480,
              		Height = 272,
              		MinDepth = GraphicsDevice.Viewport.MinDepth,
              		MaxDepth = GraphicsDevice.Viewport.MaxDepth
              	};
				return true;
			}
			return false;
		}

		// override EndDraw to handle unsetting the render target, resetting the Viewport,
		// and drawing the render target's contents to the screen
		protected override void EndDraw()
		{
			GraphicsDevice.SetRenderTarget(0, null);
			GraphicsDevice.Viewport = new Viewport
          	{
          		X = 0,
          		Y = 0,
          		Width = 272,
          		Height = 480,
          		MinDepth = GraphicsDevice.Viewport.MinDepth,
          		MaxDepth = GraphicsDevice.Viewport.MaxDepth
          	};

			GraphicsDevice.Clear(Color.Black);

			spriteBatch.Begin();
			spriteBatch.Draw(
				renderTarget.GetTexture(),
				new Vector2(136f, 240f),
				null,
				Color.White, 
				Orientation == LandscapeOrientation.Right ? MathHelper.PiOver2 : -MathHelper.PiOver2,
				new Vector2(240f, 136f),
				1f,
				SpriteEffects.None, 
				0);
			spriteBatch.End();

			base.EndDraw();
		}
#endif
	}
}