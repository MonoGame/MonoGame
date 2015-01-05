#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;

using MonoMac.AppKit;
using MonoMac.Foundation;
#endregion

namespace ${Namespace}
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
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
		private static Game1 game;

		public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
		{
			// Handle a Xamarin.Mac Upgrade
			AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs a) =>  {
			if (a.Name.StartsWith("MonoMac")) {
				return typeof(MonoMac.AppKit.AppKitFramework).Assembly;
			}
			return null;
			};
			game = new Game1();
			game.Run();
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}
	}  

}

