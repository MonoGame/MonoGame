#region Using Statements
using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using XnaTouch;
using XnaTouch.Framework.Media;
#endregion

namespace XnaTouch.Samples.MultiTouch
{
    [Register("AppDelegate")]
    class Program : UIApplicationDelegate
    {
        public override void FinishedLaunching(UIApplication app)
        {
            // Fun begins..
            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, "AppDelegate");
        }
    }    
}
