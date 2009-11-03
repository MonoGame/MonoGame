using MonoTouch.Foundation;
using MonoTouch.UIKit;
using XnaTouch;
using XnaTouch.Samples;

namespace XnaTouch.Samples.SpriteFont
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
