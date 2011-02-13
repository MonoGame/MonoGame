using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Microsoft.Xna;
using Microsoft.Xna.Samples;
using Microsoft.Xna.Samples.Draw2D;

namespace Microsoft.Xna.Samples.Draw2D
{
	[Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		private Game1 game;

		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			game = new Game1();
			game.Run();
		}

		static void Main (string [] args)
		{
			UIApplication.Main (args,null,"AppDelegate");
		}
	}
}
