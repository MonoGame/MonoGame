#region Using Statements
using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;

#endregion

namespace Blackjack
{
    [Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			using (BlackjackGame game = new BlackjackGame())
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
}
