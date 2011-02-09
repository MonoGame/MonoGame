
#region Using Statements
using System;
#if IPHONE
using XnaTouch.Framework;
using XnaTouch.Framework.Media;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
#else
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
#endif
#endregion

namespace Tester
{
	#if IPHONE
	[Register("AppDelegate")]
	class Program : XNATouchProgram
	{
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// Fun begins..
			XNATouchGame = new Game1 ();
			XNATouchGame.Run ();
			
			return true;
		}

		static void Main (string[] args)
		{
			UIApplication.Main (args, null, "AppDelegate");
		}
	}
	#else
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main (string[] args)
		{
			using (Game game = new Game ()) {
				game.Run ();
			}
		}
	}
	#endif
}

