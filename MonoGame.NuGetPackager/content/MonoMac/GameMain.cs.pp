using System;
using MonoMac.AppKit;
using MonoMac.Foundation;


namespace $rootnamespace$
{
	class Program
	{
		static void Main (string[] args)
		{
			NSApplication.Init ();

			using (var p = new NSAutoreleasePool ()) {
				NSApplication.SharedApplication.Delegate = new AppDelegate ();

				// Set our Application Icon
				//NSImage appIcon = NSImage.ImageNamed ("<Your Game Icon>");
				//NSApplication.SharedApplication.ApplicationIconImage = appIcon;
				
				NSApplication.Main (args);
			}
		}
	}

	class AppDelegate : NSApplicationDelegate
	{
		private Game1 game;

		public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
		{
			game = new Game1();
			game.Run();
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}
	}
}
