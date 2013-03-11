using System;

namespace MouseGetStateAndIsMouseVisibleTester
{

#if WINDOWS || XBOX || LINUX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }
#endif
#if MAC

	static class Program
	{	
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main (string[] args)
		{
			MonoMac.AppKit.NSApplication.Init ();
			
			using (var p = new MonoMac.Foundation.NSAutoreleasePool ()) {
				MonoMac.AppKit.NSApplication.SharedApplication.Delegate = new AppDelegate();
				MonoMac.AppKit.NSApplication.Main(args);
			}
		}
	}
	
	class AppDelegate : MonoMac.AppKit.NSApplicationDelegate
	{

        Game1 game;
		public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
		{
			Game1 game = new Game1();
			game.Run();
		}
		
		public override bool ApplicationShouldTerminateAfterLastWindowClosed (MonoMac.AppKit.NSApplication sender)
		{
			return true;
		}
	}
#endif
}

