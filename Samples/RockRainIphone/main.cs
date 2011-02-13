using System;
using System.Drawing;
using System.Runtime.InteropServices;
using MonoTouch.AudioToolbox;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;
using MonoTouch.ObjCRuntime;
using Microsoft.Xna.Framework;

namespace RockRainIphone
{
	[Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		private Game1 game;

		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			game = new Game1();
			game.Run();
		}
		
		static void Main (string [] args)
		{
			UIApplication.Main (args,null,"AppDelegate");
		}
	}
}
