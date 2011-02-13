#if IPHONE
using Microsoft.Xna.Framework;
#else
using Microsoft.Xna.Framework;
#endif

namespace ZuneLib
{
	/*
	 * PortraitZuneGame is a simple class that just makes sure to create
	 * the GraphicsDeviceManager with the correct width and height, as
	 * well as making the mouse visible on Windows.
	 */
	public class PortraitZuneGame : Game
	{
		/// <summary>
		/// Gets the GraphicsDeviceManager for the game.
		/// </summary>
		public GraphicsDeviceManager Graphics { get; private set; }

		public PortraitZuneGame()
		{
			Graphics = new GraphicsDeviceManager(this)
           	{
           		PreferredBackBufferWidth = 272,
           		PreferredBackBufferHeight = 480
           	};

			IsMouseVisible = true;
		}

		/// <summary>
		/// Updates the ZuneInput class. Call this at the top of your game's Update method.
		/// </summary>
		protected void UpdateZuneInput()
		{
			ZuneInput.Update();
		}
	}
}