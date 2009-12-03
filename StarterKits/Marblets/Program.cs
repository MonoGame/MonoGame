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
#if IPHONE
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using XnaTouch;
using XnaTouch.Framework.Media;
#else
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
#endif
#endregion

namespace Marblets
{
#if IPHONE
    [Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			using (MarbletsGame game = new MarbletsGame())
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
            using (MarbletsGame game = new MarbletsGame())
            {
                game.Run();
            }
        }
    }
#endif
}

