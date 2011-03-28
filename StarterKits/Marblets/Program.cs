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
#endif

using Microsoft.Xna.Framework;
#endregion

namespace Marblets
{
#if IPHONE
	[Register ("AppDelegate")]
	class  Program : MonoGameProgram
	{
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// Fun begins..
			MonoGameGame = new MarbletsGame(); 
            MonoGameGame.Run();
			
			return true;
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
        static void Main()
        {
            using (MarbletsGame game = new MarbletsGame())
            {
                game.Run();
            }
        }
    }
#endif
}

