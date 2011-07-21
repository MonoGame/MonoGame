using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Microsoft.Xna;

namespace PeerToPeer
{
	[Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		private PeerToPeerGame game;

		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			game = new PeerToPeerGame();
			game.Run();
		}

		static void Main (string [] args)
		{
			UIApplication.Main (args,null,"AppDelegate");
		}
	}
}
