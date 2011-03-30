#region Using Statements
using System;
#if IPHONE
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;

#endregion

namespace AlienGameSample
{
#if IPHONE
    [Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			using (AlienGame game = new AlienGame())
            {
                game.Run();
            }
			
			//MediaLibrary lib = new MediaLibrary();
			//object result = lib.Playlists;
		}

		static void Main (string [] args)
		{
			UIApplication.Main (args,null,"AppDelegate");
		}
	}
#else
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (AlienGame game = new AlienGame())
            {
                game.Run();
            }
        }
    }
#endif
}
