#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

#region Using clause
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework.Storage;

#if WINRT
using Windows.ApplicationModel.Store;
using System.Threading.Tasks;
using Windows.ApplicationModel;    
using Windows.UI.Core;
using Windows.UI.Popups;
#else
using System.Runtime.Remoting.Messaging;
using Microsoft.Xna.Framework.Net;
#endif

#endregion Using clause

namespace Microsoft.Xna.Framework.GamerServices
{


	public static class Guide
	{
		private static bool isScreenSaverEnabled;
		private static bool isTrialMode;
		private static bool isVisible;
		private static bool simulateTrialMode;		

#if WINRT
	    private static readonly CoreDispatcher _dispatcher;
#endif 
        static Guide()
        {
#if WINRT
            _dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
#if DEBUG && !WINDOWS_PHONE
            var licenseInformation = CurrentAppSimulator.LicenseInformation;
#else
            var licenseInformation = CurrentApp.LicenseInformation;
#endif

            licenseInformation.LicenseChanged += () => 
                isTrialMode = !licenseInformation.IsActive || licenseInformation.IsTrial;

            isTrialMode = !licenseInformation.IsActive || licenseInformation.IsTrial;

#endif // WINRT

        }

		delegate string ShowKeyboardInputDelegate(
		 PlayerIndex player,           
         string title,
         string description,
         string defaultText,
		 bool usePasswordMode);

		public static string ShowKeyboardInput(
		 PlayerIndex player,           
         string title,
         string description,
         string defaultText,
		 bool usePasswordMode)
		{
#if WINRT
            // At this time there is no way to popup the 
            // software keyboard on a WinRT device unless 
            // you use a XAML control.
            throw new NotSupportedException();
#else
            throw new NotImplementedException();
#endif
		}

		public static IAsyncResult BeginShowKeyboardInput (
         PlayerIndex player,
         string title,
         string description,
         string defaultText,
         AsyncCallback callback,
         Object state)
		{
			return BeginShowKeyboardInput(player, title, description, defaultText, callback, state, false );
		}

		public static IAsyncResult BeginShowKeyboardInput (
         PlayerIndex player,
         string title,
         string description,
         string defaultText,
         AsyncCallback callback,
         Object state,
         bool usePasswordMode)
		{
			isVisible = true;

			ShowKeyboardInputDelegate ski = ShowKeyboardInput; 

			return ski.BeginInvoke(player, title, description, defaultText, usePasswordMode, callback, ski);
		}

		public static string EndShowKeyboardInput (IAsyncResult result)
		{
			try 
			{
				ShowKeyboardInputDelegate ski = (ShowKeyboardInputDelegate)result.AsyncState; 

				return ski.EndInvoke(result);
			} 
			finally 
			{
				isVisible = false;
			}			
		}

		delegate Nullable<int> ShowMessageBoxDelegate( string title,
         string text,
         IEnumerable<string> buttons,
         int focusButton,
         MessageBoxIcon icon);

		public static Nullable<int> ShowMessageBox( string title,
         string text,
         IEnumerable<string> buttons,
         int focusButton,
         MessageBoxIcon icon)
		{
            int? result = null;

#if WINDOWS_STOREAPP
            var dialog = new MessageDialog(text, title);
		    var index = 0;
            foreach (var b in buttons)
            {
                var cmd = new UICommand(b, null, index);
                dialog.Commands.Add(cmd);
                ++index;
            }
            
            dialog.DefaultCommandIndex = (uint)focusButton;

            // The message box must be popped up on the UI thread.
		    Task<IUICommand> dialogResult = null;
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // We'll get an exception if we try to open a dialog while 
                // one is already up.  Catch the exception and ignore it.
                try
                {
                    dialogResult = dialog.ShowAsync().AsTask();
                }
                catch (Exception)
                {
                }

            }).AsTask().Wait();

            if (dialogResult != null)
            {
                dialogResult.Wait();
                result = (int)dialogResult.Result.Id;
            }

#endif
            return result;
		}

		public static IAsyncResult BeginShowMessageBox(
         PlayerIndex player,
         string title,
         string text,
         IEnumerable<string> buttons,
         int focusButton,
         MessageBoxIcon icon,
         AsyncCallback callback,
         Object state
		)
		{	
			isVisible = true;

			ShowMessageBoxDelegate smb = ShowMessageBox; 

			return smb.BeginInvoke(title, text, buttons, focusButton, icon, callback, smb);			
		}

		public static IAsyncResult BeginShowMessageBox (
         string title,
         string text,
         IEnumerable<string> buttons,
         int focusButton,
         MessageBoxIcon icon,
         AsyncCallback callback,
         Object state
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


		public static void ShowMarketplace(PlayerIndex player)
		{
#if WINRT
            var uri = new Uri(@"ms-windows-store:PDP?PFN=" + Package.Current.Id.FamilyName);
            Task.Run(async () => await Windows.System.Launcher.LaunchUriAsync(uri)).Wait();	
#endif
		}

		public static void Show ()
		{
			ShowSignIn(1, false);
		}

		public static void ShowSignIn (int paneCount, bool onlineOnly)
		{
			if ( paneCount != 1 && paneCount != 2 && paneCount != 4)
			{
				new ArgumentException("paneCount Can only be 1, 2 or 4 on Windows");
				return;
			}

#if !WINRT
            Microsoft.Xna.Framework.GamerServices.MonoGameGamerServicesHelper.ShowSigninSheet();            

            if (GamerServicesComponent.LocalNetworkGamer == null)
            {
                GamerServicesComponent.LocalNetworkGamer = new LocalNetworkGamer();
            }
            else
            {
                GamerServicesComponent.LocalNetworkGamer.SignedInGamer.BeginAuthentication(null, null);
            }
#endif
		}

		public static void ShowLeaderboard()
		{
            //if ( ( Gamer.SignedInGamers.Count > 0 ) && ( Gamer.SignedInGamers[0].IsSignedInToLive ) )
            //{
            //    // Lazy load it
            //    if ( leaderboardController == null )
            //    {			    	
            //        leaderboardController = new GKLeaderboardViewController();
            //    }

            //    if (leaderboardController != null)			
            //    {
            //        leaderboardController.DidFinish += delegate(object sender, EventArgs e) 
            //        {
            //            leaderboardController.DismissModalViewControllerAnimated(true);
            //            isVisible = false;
            //        };

            //        if (Window !=null)
            //        {						
            //            if(viewController == null)
            //            {
            //                viewController = new UIViewController();
            //                Window.Add(viewController.View);
            //                viewController.View.Hidden = true;
            //            }

            //            viewController.PresentModalViewController(leaderboardController, true);
            //            isVisible = true;
            //        }
            //    }
            //}
		}

		public static void ShowAchievements()
		{
            //if ( ( Gamer.SignedInGamers.Count > 0 ) && ( Gamer.SignedInGamers[0].IsSignedInToLive ) )
            //{
            //    // Lazy load it
            //    if ( achievementController == null )
            //    {
            //        achievementController = new GKAchievementViewController();
            //    }

            //    if (achievementController != null)		
            //    {					
            //        achievementController.DidFinish += delegate(object sender, EventArgs e) 
            //        {									 
            //            leaderboardController.DismissModalViewControllerAnimated(true);
            //            isVisible = false;
            //        };

            //        if (Window !=null)
            //        {
            //            if(viewController == null)
            //            {
            //                viewController = new UIViewController();
            //                Window.Add(viewController.View);
            //                viewController.View.Hidden = true;
            //            }

            //            viewController.PresentModalViewController(achievementController, true);						
            //            isVisible = true;
            //        }
            //    }
            //}
		}

		public static IAsyncResult BeginShowStorageDeviceSelector( AsyncCallback callback, object state )
		{
			return null;
		}

		public static StorageDevice EndShowStorageDeviceSelector( IAsyncResult result )
		{
			return null;
		}

		#region Properties
		public static bool IsScreenSaverEnabled 
		{ 
			get
			{
				return isScreenSaverEnabled;
			}
			set
			{
				isScreenSaverEnabled = value;
			}
		}

		public static bool IsTrialMode 
		{ 
			get
			{
				// If simulate trial mode is enabled then 
				// we're in the trial mode.
				return simulateTrialMode || isTrialMode;
			}

			set
			{
				isTrialMode = value;
			}
		}

		public static bool IsVisible 
		{ 
			get
			{
				return isVisible;
			}
			set
			{
				isVisible = value;
			}
		}

		public static bool SimulateTrialMode 
		{ 
			get
			{
				return simulateTrialMode;
			}
			set
			{
				simulateTrialMode = value;
			}
		}

		public static GameWindow Window 
		{ 
			get;
			set;
		}
		#endregion

        internal static void Initialise(Game game)
        {
#if !WINRT
            MonoGameGamerServicesHelper.Initialise(game);
#endif
        }
    }
}