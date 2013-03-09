using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
using System.Runtime.InteropServices;
 
namespace MonoTest
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
		Game1 game;
 
        public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
        {
            game = new Game1();
            game.Run ();
        }
 
        public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
        {
            return true;
        }
    }
}