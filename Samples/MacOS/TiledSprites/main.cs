using MonoMac.AppKit;
using MonoMac.Foundation;

namespace TiledSprites
{
	class Program
	{
		static void Main (string [] args)
		{
			NSApplication.Init ();
			
			using (var p = new NSAutoreleasePool ()) {
				NSApplication.SharedApplication.Delegate = new AppDelegate();				
				NSApplication.Main(args);
			}
		}
	}
	
	class AppDelegate : NSApplicationDelegate
	{
		private TiledSpritesSample game;
		
		public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
		{
			game = new TiledSpritesSample ();
			game.Run ();
		}
		
		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}
	}	
}
