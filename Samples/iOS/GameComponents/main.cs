using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Microsoft.Xna;
using Microsoft.Xna.Samples;
using Microsoft.Xna.Samples.GameComponents;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Samples.GameComponents
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
