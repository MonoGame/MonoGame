#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;

using MonoTouch.Foundation;
using MonoTouch.GameKit;
using MonoTouch.UIKit;

using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework.GamerServices
{
	class GuideViewController : UIViewController
	{
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return this.ParentViewController.ShouldAutorotateToInterfaceOrientation(toInterfaceOrientation);
		}

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			/*
			switch(this.InterfaceOrientation)
			{
			case UIInterfaceOrientation.LandscapeLeft:
			case UIInterfaceOrientation.LandscapeRight:
				Game.View.Frame = new System.Drawing.RectangleF(this.View.Frame.Location,new System.Drawing.SizeF(this.View.Frame.Height,this.View.Frame.Width));
				break;
			default:				
				Game.View.Frame = new System.Drawing.RectangleF(this.View.Frame.Location,new System.Drawing.SizeF(this.View.Frame.Width,this.View.Frame.Height));
				break;
			}
			//Game.View.Frame = this.View.Frame;
			Console.WriteLine("Main View's Frame:" + Game.View.Frame);
			*/
			base.DidRotate (fromInterfaceOrientation);
		}
	}

	public static class Guide
	{
		private static int _showKeyboardInputRequestCount;

		private static bool isMessageBoxShowing = false;
		private static GKLeaderboardViewController leaderboardController;
		private static GKAchievementViewController achievementController;
		private static GKPeerPickerController peerPickerController;
		private static GKMatchmakerViewController matchmakerViewController;
        private static KeyboardInputViewController keyboardViewController;
		private static GuideViewController viewController = null;
		private static GestureType prevGestures;

		private static bool _isInitialised;
		private static UIWindow _window;
		private static UIViewController _gameViewController;

		internal static void Initialise(Game game)
		{
			_window = (UIWindow)game.Services.GetService (typeof(UIWindow));
			if (_window == null)
				throw new InvalidOperationException(
					"iOSGamePlatform must add the main UIWindow to Game.Services");

			_gameViewController = (UIViewController)game.Services.GetService (typeof(UIViewController));
			if (_gameViewController == null)
				throw new InvalidOperationException(
					"iOSGamePlatform must add the game UIViewController to Game.Services");

			game.Exiting += Game_Exiting;

			_isInitialised = true;
		}

		private static void Uninitialise(Game game)
		{
			game.Exiting -= Game_Exiting;
			_window = null;
			_gameViewController = null;
			_isInitialised = false;
		}

		#region Properties

		public static bool IsScreenSaverEnabled { get; set; }

		private static bool isTrialMode;
		public static bool IsTrialMode {
			get { return isTrialMode || SimulateTrialMode; }
			set { isTrialMode = value; }
		}

		private static bool isVisible;
		public static bool IsVisible {
			get {
				AssertInitialised ();
				return isVisible;
			}
			set {
				AssertInitialised ();
				// FIXME: Need to hide any visible UI here.
				isVisible = value;
			}
		}

		public static bool SimulateTrialMode { get; set; }

		public static NotificationPosition NotificationPosition { get; set; }

		#endregion

		private static void Game_Exiting (object sender, EventArgs e)
		{
			Uninitialise ((Game) sender);
		}

		private static void AssertInitialised ()
		{
			if (!_isInitialised)
				throw new InvalidOperationException(
					"Gamer services functionality has not been initialized.");
		}

		public static IAsyncResult BeginShowKeyboardInput (
			PlayerIndex player, string title, string description, string defaultText,
			AsyncCallback callback, Object state)
		{
			AssertInitialised ();
			return BeginShowKeyboardInput(player, title, description, defaultText, callback, state, false );
		}

		public static IAsyncResult BeginShowKeyboardInput (
			PlayerIndex player, string title, string description, string defaultText,
			AsyncCallback callback, Object state, bool usePasswordMode)
		{
			AssertInitialised ();

            if (keyboardViewController != null)
                return null;

			int requestCount = Interlocked.Increment (ref _showKeyboardInputRequestCount);
			if (requestCount != 1) {
				Interlocked.Decrement (ref _showKeyboardInputRequestCount);
				// FIXME: Return the in-progress IAsyncResult?
				return null;
			}

			isVisible = true;

			keyboardViewController = new KeyboardInputViewController(
				title, description, defaultText, usePasswordMode);

            (_gameViewController.View as iOSGameView).PreserveFrameBuffer = true;

			_gameViewController.PresentModalViewController (keyboardViewController, true);

			keyboardViewController.View.InputAccepted += (sender, e) => {
                _gameViewController.DismissModalViewControllerAnimated(true);
				Interlocked.Decrement (ref _showKeyboardInputRequestCount);
			};

			keyboardViewController.View.InputCanceled += (sender, e) => {
                _gameViewController.DismissModalViewControllerAnimated(true);
				Interlocked.Decrement (ref _showKeyboardInputRequestCount);
			};

			return new KeyboardInputAsyncResult (keyboardViewController, callback, state);
		}

		public static string EndShowKeyboardInput (IAsyncResult result)
		{
			AssertInitialised ();

            (_gameViewController.View as iOSGameView).PreserveFrameBuffer = false;
            keyboardViewController = null;

			if (!(result is KeyboardInputAsyncResult))
				throw new ArgumentException ("result");

			return (result as KeyboardInputAsyncResult).GetResult();
		}

		delegate Nullable<int> ShowMessageBoxDelegate(
			string title, string text, IEnumerable<string> buttons, int focusButton, MessageBoxIcon icon);

		private static Nullable<int> ShowMessageBox(
			string title, string text, IEnumerable<string> buttons, int focusButton, MessageBoxIcon icon)
		{
			Nullable<int> result = null;
			
			if ( !isMessageBoxShowing )
			{
				isMessageBoxShowing = true;	
	
				UIAlertView alert = new UIAlertView();
				alert.Title = title;
				foreach( string btn in buttons )
				{
					alert.AddButton(btn);
				}
				alert.Message = text;
				alert.Dismissed += delegate(object sender, UIButtonEventArgs e) 
								{ 
									result = e.ButtonIndex;
									isMessageBoxShowing = false;
								};
				alert.Clicked += delegate(object sender, UIButtonEventArgs e) 
								{ 
									result = e.ButtonIndex; 
									isMessageBoxShowing = false;
								};
				
				UIApplication.SharedApplication.InvokeOnMainThread(delegate {
					alert.Show();
				});
				
			}
			
			isVisible = isMessageBoxShowing;

			return result;
		}

		public static IAsyncResult BeginShowMessageBox(
			PlayerIndex player, string title, string text, IEnumerable<string> buttons, int focusButton,
			MessageBoxIcon icon, AsyncCallback callback, Object state)
		{	
			ShowMessageBoxDelegate smb = ShowMessageBox; 

			return smb.BeginInvoke(title, text, buttons, focusButton, icon, callback, smb);			
		}

		public static IAsyncResult BeginShowMessageBox (
			string title, string text, IEnumerable<string> buttons, int focusButton, MessageBoxIcon icon,
			AsyncCallback callback, Object state
		)
		{
			return BeginShowMessageBox(PlayerIndex.One, title, text, buttons, focusButton, icon, callback, state);
		}

		public static Nullable<int> EndShowMessageBox (IAsyncResult result)
		{
			try
			{
				ShowMessageBoxDelegate smbd = (ShowMessageBoxDelegate)result.AsyncState; 

				return smbd.EndInvoke(result);
			} 
			finally 
			{
				isVisible = false;
			}
		}


		public static void ShowMarketplace (PlayerIndex player)
		{
			AssertInitialised ();

			NSUrl url = new NSUrl("http://phobos.apple.com/WebObjects/MZStore.woa/wa/viewSoftware?id=306469222&mt=8");
			if (!UIApplication.SharedApplication.OpenUrl(url)) {
				// Error
			}
		}

		public static void ShowSignIn (int paneCount, bool onlineOnly)
		{
			AssertInitialised ();

			if ( paneCount != 1 )
			{
				new ArgumentException("paneCount Can only be 1 on iPhone");
				return;
			}

			if (GamerServicesComponent.LocalNetworkGamer == null)
			{
				GamerServicesComponent.LocalNetworkGamer = new LocalNetworkGamer();
			}
			else
			{
				GamerServicesComponent.LocalNetworkGamer.SignedInGamer.BeginAuthentication(null, null);
			}
		}

		public static void ShowLeaderboard()
		{
			AssertInitialised ();

			if ( ( Gamer.SignedInGamers.Count > 0 ) && ( Gamer.SignedInGamers[0].IsSignedInToLive ) )
			{
				// Lazy load it
				if ( leaderboardController == null )
				{			    	
					leaderboardController = new GKLeaderboardViewController();
				}

			    if (leaderboardController != null)			
			    {
					leaderboardController.DidFinish += delegate(object sender, EventArgs e) 
					{
						leaderboardController.DismissModalViewControllerAnimated(true);
						isVisible = false;
						TouchPanel.EnabledGestures=prevGestures;
 					};

					if (_window != null)
					{						
						if(viewController == null)
						{
							viewController = new GuideViewController();
							_window.Add(viewController.View);
							viewController.View.Hidden = true;
						}
						
						prevGestures=TouchPanel.EnabledGestures;
						TouchPanel.EnabledGestures=GestureType.None;
						viewController.PresentModalViewController(leaderboardController, true);
						isVisible = true;
					}
			    }
			}
			else
			{
				UIAlertView alert = new UIAlertView("Error","You need to be logged into Game Center to view the Leaderboard.",null,"Ok");
				alert.Show();
				ShowSignIn(1,true);
			}
		}

		public static void ShowAchievements()
		{
			AssertInitialised ();

			if ( ( Gamer.SignedInGamers.Count > 0 ) && ( Gamer.SignedInGamers[0].IsSignedInToLive ) )
			{
				// Lazy load it
				if ( achievementController == null )
				{
					achievementController = new GKAchievementViewController();
				}

			    if (achievementController != null)		
			    {					
					achievementController.DidFinish += delegate(object sender, EventArgs e) 
					{									 
						achievementController.DismissModalViewControllerAnimated(true);
						isVisible = false;
						TouchPanel.EnabledGestures=prevGestures;
					};

					if (_window != null)
					{
						if(viewController == null)
						{
							viewController = new GuideViewController();
							_window.Add(viewController.View);
							viewController.View.Hidden = true;
						}

						prevGestures=TouchPanel.EnabledGestures;
						TouchPanel.EnabledGestures=GestureType.None;
						viewController.PresentModalViewController(achievementController, true);						
						isVisible = true;
					}
			    }
			}
			else
			{
				UIAlertView alert = new UIAlertView("Error","You need to be logged into Game Center to view Achievements.",null,"Ok");
				alert.Show();
				ShowSignIn(1,true);
			}
		}
		
		public static void ShowPeerPicker(GKPeerPickerControllerDelegate aPeerPickerControllerDelegate)
		{
			AssertInitialised ();

			if ( ( Gamer.SignedInGamers.Count > 0 ) && ( Gamer.SignedInGamers[0].IsSignedInToLive ) )
			{
				// Lazy load it
				if ( peerPickerController == null )
				{
					peerPickerController = new GKPeerPickerController();
				}

			    if (peerPickerController != null)		
			    {			
					peerPickerController.ConnectionTypesMask = GKPeerPickerConnectionType.Nearby;
					peerPickerController.Delegate = aPeerPickerControllerDelegate;
					peerPickerController.Show();					
			    }
			}
		}
		
		
		public static void ShowMatchMaker()
		{
			AssertInitialised ();

			if ( ( Gamer.SignedInGamers.Count > 0 ) && ( Gamer.SignedInGamers[0].IsSignedInToLive ) )
			{
				// Lazy load it
				if ( matchmakerViewController == null )
				{
					matchmakerViewController = new GKMatchmakerViewController();
				}

			    if (matchmakerViewController != null)		
			    {			
					matchmakerViewController.DidFailWithError += delegate(object sender, GKErrorEventArgs e) {
						matchmakerViewController.DismissModalViewControllerAnimated(true);
						isVisible = false;
						TouchPanel.EnabledGestures=prevGestures;
					};
					
					matchmakerViewController.DidFindMatch += delegate(object sender, GKMatchEventArgs e) {
						
					};
						
					matchmakerViewController.DidFindPlayers += delegate(object sender, GKPlayersEventArgs e) {
						
					};
					
					matchmakerViewController.WasCancelled += delegate(object sender, EventArgs e) {
						matchmakerViewController.DismissModalViewControllerAnimated(true);
						isVisible = false;
						TouchPanel.EnabledGestures=prevGestures;
					};

					if (_window != null)
					{
						if(viewController == null)
						{
							viewController = new GuideViewController();
							_window.Add(viewController.View);
							viewController.View.Hidden = true;
						}

						prevGestures=TouchPanel.EnabledGestures;
						TouchPanel.EnabledGestures=GestureType.None;
						viewController.PresentModalViewController(matchmakerViewController, true);						
						isVisible = true;
					}				
			    }
			}
		}

		public static IAsyncResult BeginShowStorageDeviceSelector( AsyncCallback callback, Object state )
		{
			return null;
		}

		public static StorageDevice EndShowStorageDeviceSelector( IAsyncResult result )
		{
			return null;
		}
	}
}