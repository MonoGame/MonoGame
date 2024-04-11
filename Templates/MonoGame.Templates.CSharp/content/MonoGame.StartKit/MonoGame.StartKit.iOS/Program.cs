//-----------------------------------------------------------------------------
// Program.cs
//
// MonoGame Foundation Game Platform
// Copyright (C) MonoGame Foundation. All rights reserved.
//-----------------------------------------------------------------------------

#region Using Statements
using System;

using Foundation;
using UIKit;

using ___safegamename___.Core;
#endregion

namespace ___safegamename___.iOS
{
    [Register("AppDelegate")]
    internal class Program : UIApplicationDelegate
    {
        private static ___safegamename___Game game;

        internal static void RunGame()
        {
            game = new ___safegamename___Game();
            game.Run();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, typeof(Program));
        }

        public override void FinishedLaunching(UIApplication app)
        {
            RunGame();
        }
    }
}
