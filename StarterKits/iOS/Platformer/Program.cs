#region Using Clause
using System;
#if IPHONE
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Microsoft.Xna;
using Microsoft.Xna.Framework.Media;
#else
using Microsoft.Xna.Framework;
#endif
#endregion Using Clause

namespace Platformer
{
#if IPHONE
    [Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			using (PlatformerGame game = new PlatformerGame())
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
            using (PlatformerGame game = new PlatformerGame())
            {
                game.Run();
            }
        }
    }
#endif
}