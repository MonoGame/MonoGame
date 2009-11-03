#region File Description
//-----------------------------------------------------------------------------
// Program.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using XnaTouch;
using XnaTouch.Framework.Media;
#endregion

namespace Marblets
{
    [Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		private MarbletsGame game;

		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			game = new MarbletsGame();
			game.Run();
			
			//MediaLibrary lib = new MediaLibrary();
			//object result = lib.Playlists;
		}

		static void Main (string [] args)
		{
			UIApplication.Main (args,null,"AppDelegate");
		}
	}
}

