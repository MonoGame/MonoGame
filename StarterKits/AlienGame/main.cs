using MonoTouch.Foundation;
using MonoTouch.UIKit;
using XnaTouch;
using XnaTouch.Framework.Media;

namespace AlienGameSample
{
	[Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		private AlienGame game;

		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			game = new AlienGame();
			game.Run();
			
			//MediaLibrary lib = new MediaLibrary();
			//object result = lib.Playlists;
		}

		static void Main (string [] args)
		{
			UIApplication.Main (args,null,"AppDelegate");
		}
	}
}
