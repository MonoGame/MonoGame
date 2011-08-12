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
using System.IO;

using MonoMac.Foundation;
using MonoMac.AppKit;
#endregion

namespace Blackjack
{
	class Program
	{
		static void Main (string[] args)
		{
			NSApplication.Init ();

			using (var p = new NSAutoreleasePool ()) 
			{
				NSApplication.SharedApplication.Delegate = new AppDelegate();
				
				// Check for Receipt
				// if (File.Exists(NSBundle.MainBundle.ResourcePath+"/../_MASReceipt/receipt"))
				{		
					// Work out Hash
					NSApplication.Main (args);
				}
				/*else
				{
					// Exit with code 173 tp be prompted for another purchase.
				}*/
			}
		}
	}

	class AppDelegate : NSApplicationDelegate
	{
		private BlackjackGame game;

		public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
		{
			game = new BlackjackGame();
			game.Run ();
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}
	}
}

