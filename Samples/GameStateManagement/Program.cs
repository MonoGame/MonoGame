#region Using Statements
using System;
#if IPHONE
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using XnaTouch;
using XnaTouch.Framework.Media;
#else
#endif
#endregion

namespace XnaTouch.Samples.GameStateManagement
{
    #region Entry Point
#if IPHONE
    [Register("AppDelegate")]
    class Program : UIApplicationDelegate
    {
        public override void FinishedLaunching(UIApplication app)
        {
            // Fun begins..
            using (GameStateManagementGame game = new GameStateManagementGame())
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
#else
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (GameStateManagementGame game = new GameStateManagementGame())
            {
                game.Run();
            }
        }
    }    
#endif
    #endregion
}
