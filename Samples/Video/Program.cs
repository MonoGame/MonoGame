
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Microsoft.Xna;

namespace Microsoft.Xna.Samples.VideoSample
{
	[Register("AppDelegate")]
	class Program : UIApplicationDelegate
	{
		private Game1 game;

		public override void FinishedLaunching (UIApplication app)
		{
			game = new Game1 ();
			game.Run ();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main (string[] args)
		{
			UIApplication.Main (args, null, "AppDelegate");
		}
	}
}
