using MonoMac.AppKit;
using MonoMac.Foundation;

namespace Microsoft.Xna.Samples.Draw2D
{
	[Register ("AppDelegate")]
	class Program : NSApplicationDelegate 
	{
		private Game1 game;

		public override void FinishedLaunching(NSObject notification)
		{
			// Fun begins..
			game = new Game1();
			game.Run();
		}
		
		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}

		static void Main (string [] args)
		{
			NSApplication.Init ();
			NSApplication.Main (args);
		}
	}
}
