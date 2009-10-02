using MonoTouch.Foundation;
using MonoTouch.UIKit;
using XnaTouch;
using XnaTouch.Samples;
using XnaTouch.Samples.GameComponents;
using XnaTouch.Framework;

namespace XnaTouch.Samples.GameComponents
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
			NSAutoreleasePool pool = new NSAutoreleasePool();
			UIApplication.Main (args,null,"AppDelegate");
			pool.Dispose();
		}
	}
}
