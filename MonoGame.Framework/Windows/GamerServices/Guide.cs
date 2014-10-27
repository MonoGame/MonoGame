﻿#region License
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
#if WINDOWS_PHONE
extern alias MonoGameXnaFramework;
extern alias MicrosoftXnaFramework;
extern alias MicrosoftXnaGamerServices;
using MsXna_Guide = MicrosoftXnaGamerServices::Microsoft.Xna.Framework.GamerServices.Guide;
using MsXna_MessageBoxIcon = MicrosoftXnaGamerServices::Microsoft.Xna.Framework.GamerServices.MessageBoxIcon;
using MsXna_PlayerIndex = MicrosoftXnaFramework::Microsoft.Xna.Framework.PlayerIndex;
using MGXna_Framework = MonoGameXnaFramework::Microsoft.Xna.Framework;
#else
using MGXna_Framework = global::Microsoft.Xna.Framework;
#endif

using System;
using System.Collections.Generic;

#if WINRT
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.System;
#if WINDOWS_STOREAPP
using Microsoft.Xna.Framework.Windows8.GamerServices;
#endif
#else
using System.Runtime.Remoting.Messaging;
#if !(WINDOWS && DIRECTX)
using Microsoft.Xna.Framework.Net;
#endif
#endif

#endregion Using clause

namespace Microsoft.Xna.Framework.GamerServices
{


	public static class Guide
	{
		private static bool isScreenSaverEnabled;
		private static bool isTrialMode = false;
		private static bool isVisible;
		private static bool simulateTrialMode;

#if WINDOWS_STOREAPP
	    private static readonly CoreDispatcher _dispatcher;
#endif 

        static Guide()
        {
#if WINDOWS_STOREAPP
            _dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;


            var licenseInformation = CurrentApp.LicenseInformation;
            licenseInformation.LicenseChanged += () => isTrialMode = !licenseInformation.IsActive || licenseInformation.IsTrial;

            isTrialMode = !licenseInformation.IsActive || licenseInformation.IsTrial;
#endif
        }

		delegate string ShowKeyboardInputDelegate(
         MGXna_Framework.PlayerIndex player,           
         string title,
         string description,
         string defaultText,
		 bool usePasswordMode);

		private static string ShowKeyboardInput(
         MGXna_Framework.PlayerIndex player,           
         string title,
         string description,
         string defaultText,
		 bool usePasswordMode)
        {
#if WINDOWS_STOREAPP
			// If SwapChainBackgroundPanel is null then we are running the non-XAML template
			if (Game.Instance.graphicsDeviceManager.SwapChainBackgroundPanel == null)
			{
				throw new NotImplementedException("This method works only when using the XAML template.");
			}
			
            Task<string> result = null;
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var inputDialog = new InputDialog();
                result = inputDialog.ShowAsync(title, description, defaultText, usePasswordMode);
            }).AsTask().Wait();

            result.Wait();

            return result.Result;
#else
            throw new NotImplementedException();
#endif
		}

		public static IAsyncResult BeginShowKeyboardInput (
         MGXna_Framework.PlayerIndex player,
         string title,
         string description,
         string defaultText,
         AsyncCallback callback,
         Object state)
		{
#if WINDOWS_PHONE

            // Call the Microsoft implementation of BeginShowKeyboardInput using an alias.
            return MsXna_Guide.BeginShowKeyboardInput((MsXna_PlayerIndex)player, title, description, defaultText, callback, state);
#else
			return BeginShowKeyboardInput(player, title, description, defaultText, callback, state, false );
#endif
		}

		public static IAsyncResult BeginShowKeyboardInput (
         MGXna_Framework.PlayerIndex player,
         string title,
         string description,
         string defaultText,
         AsyncCallback callback,
         Object state,
         bool usePasswordMode)
		{
#if WINDOWS_PHONE

            // Call the Microsoft implementation of BeginShowKeyboardInput using an alias.
            return MsXna_Guide.BeginShowKeyboardInput((MsXna_PlayerIndex)player, title, description, defaultText, callback, state, usePasswordMode);
#else
			ShowKeyboardInputDelegate ski = ShowKeyboardInput; 

			return ski.BeginInvoke(player, title, description, defaultText, usePasswordMode, callback, ski);
#endif
		}

		public static string EndShowKeyboardInput (IAsyncResult result)
		{
#if WINDOWS_PHONE

            // Call the Microsoft implementation of BeginShowKeyboardInput using an alias.
            return MsXna_Guide.EndShowKeyboardInput(result);
#else
			ShowKeyboardInputDelegate ski = (ShowKeyboardInputDelegate)result.AsyncState; 

			return ski.EndInvoke(result);		
#endif
		}

		delegate Nullable<int> ShowMessageBoxDelegate( string title,
         string text,
         IEnumerable<string> buttons,
         int focusButton,
         MessageBoxIcon icon);

        private static Nullable<int> ShowMessageBox(string title,
         string text,
         IEnumerable<string> buttons,
         int focusButton,
         MessageBoxIcon icon)
        {
            int? result = null;
            IsVisible = true;

#if WINDOWS_STOREAPP

            MessageDialog dialog = new MessageDialog(text, title);
            foreach (string button in buttons)
                dialog.Commands.Add(new UICommand(button, null, dialog.Commands.Count));

            if (focusButton < 0 || focusButton >= dialog.Commands.Count)
                throw new ArgumentOutOfRangeException("focusButton", "Specified argument was out of the range of valid values.");
            dialog.DefaultCommandIndex = (uint)focusButton;

            // The message box must be popped up on the UI thread.
            Task<IUICommand> dialogResult = null;
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                dialogResult = dialog.ShowAsync().AsTask();
            }).AsTask().Wait();

            dialogResult.Wait();
            result = (int)dialogResult.Result.Id;

#endif
            IsVisible = false;
            return result;
        }

        public static IAsyncResult BeginShowMessageBox(
         MGXna_Framework.PlayerIndex player,
         string title,
         string text,
         IEnumerable<string> buttons,
         int focusButton,
         MessageBoxIcon icon,
         AsyncCallback callback,
         Object state
        )
        {
#if WINDOWS_PHONE

            // Call the Microsoft implementation of BeginShowMessageBox using an alias.
            return MsXna_Guide.BeginShowMessageBox(
                (MsXna_PlayerIndex)player, 
                title, text,
		        buttons, focusButton,
                (MsXna_MessageBoxIcon)icon, 
                callback, state);
#else
            // TODO: GuideAlreadyVisibleException
            if (IsVisible)
                throw new Exception("The function cannot be completed at this time: the Guide UI is already active. Wait until Guide.IsVisible is false before issuing this call.");

            if (player != PlayerIndex.One)
                throw new ArgumentOutOfRangeException("player", "Specified argument was out of the range of valid values.");
            if (title == null)
                throw new ArgumentNullException("title", "This string cannot be null or empty, and must be less than 256 characters long.");
            if (text == null)
                throw new ArgumentNullException("text", "This string cannot be null or empty, and must be less than 256 characters long.");
            if (buttons == null)
                throw new ArgumentNullException("buttons", "Value can not be null.");

            ShowMessageBoxDelegate smb = ShowMessageBox;

            return smb.BeginInvoke(title, text, buttons, focusButton, icon, callback, smb);
#endif
        }

        public static IAsyncResult BeginShowMessageBox(
         string title,
         string text,
         IEnumerable<string> buttons,
         int focusButton,
         MessageBoxIcon icon,
         AsyncCallback callback,
         Object state
        )
        {
            return BeginShowMessageBox(MGXna_Framework.PlayerIndex.One, title, text, buttons, focusButton, icon, callback, state);
        }

        public static Nullable<int> EndShowMessageBox(IAsyncResult result)
        {
#if WINDOWS_PHONE

            // Call the Microsoft implementation of EndShowMessageBox using an alias.
            return MsXna_Guide.EndShowMessageBox(result);
#else
            return ((ShowMessageBoxDelegate)result.AsyncState).EndInvoke(result);
#endif
        }

        public static void ShowMarketplace(MGXna_Framework.PlayerIndex player)
        {
#if WINDOWS_PHONE

            // Call the Microsoft implementation of ShowMarketplace using an alias.
            MsXna_Guide.ShowMarketplace((MsXna_PlayerIndex)player);

#elif WINDOWS_STOREAPP
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var uri = new Uri(@"ms-windows-store:PDP?PFN=" + Package.Current.Id.FamilyName);
                Launcher.LaunchUriAsync(uri).AsTask<bool>().Wait();
            }).AsTask();
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

#if !WINRT && !(WINDOWS && DIRECTX)
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

		public static MGXna_Framework.Storage.StorageDevice EndShowStorageDeviceSelector( IAsyncResult result )
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
#if DEBUG
                return simulateTrialMode || isTrialMode;
#elif WINDOWS_PHONE
			    return MsXna_Guide.IsTrialMode;
#else
                return simulateTrialMode || isTrialMode;
#endif
			}
		}

		public static bool IsVisible 
		{ 
			get
			{
#if WINDOWS_PHONE
				return MsXna_Guide.IsVisible;
#else
				return isVisible;
#endif
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

		public static MGXna_Framework.GameWindow Window 
		{ 
			get;
			set;
		}
		#endregion

        internal static void Initialise(MGXna_Framework.Game game)
        {
#if !DIRECTX
            MonoGameGamerServicesHelper.Initialise(game);
#endif
        }
    }
}