
using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace ${Namespace}
{
    [Register("AppDelegate")]
    class Program : UIApplicationDelegate
    {
        ${ProjectName}Game game;
        public override void FinishedLaunching(UIApplication app)
        {
            // Fun begins..
            game = new ${ProjectName}Game();
            game.Run();
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

