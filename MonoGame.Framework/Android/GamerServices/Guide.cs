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
using System.Runtime.Remoting.Messaging;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


#endregion Using clause

namespace Microsoft.Xna.Framework.GamerServices
{
	public static class Guide
	{
		private static bool isScreenSaverEnabled;
		private static bool isTrialMode;
		private static bool isVisible;
		private static bool simulateTrialMode;

        internal static void Initialise(Game game)
        {
			MonoGameGamerServicesHelper.Initialise(game);        
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
            string result = null;
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            IsVisible = true;

            Game.Activity.RunOnUiThread(() => 
            {
                var alert = new AlertDialog.Builder(Game.Activity);

                alert.SetTitle(title);
                alert.SetMessage(description);

                var input = new EditText(Game.Activity) { Text = defaultText };
                if (defaultText != null)
                {
                    input.SetSelection(defaultText.Length);
                }
                if (usePasswordMode)
                {
                    input.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword;
                }
                alert.SetView(input);

                alert.SetPositiveButton("Ok", (dialog, whichButton) =>
                {
                    result = input.Text;
                    IsVisible = false;
                    waitHandle.Set();
                });

                alert.SetNegativeButton("Cancel", (dialog, whichButton) =>
                {
                    result = null;
                    IsVisible = false;
                    waitHandle.Set();
                });
                alert.SetCancelable(false);
                alert.Show();

            });
            waitHandle.WaitOne();
            IsVisible = false;

            return result;
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
			if (IsVisible)
				throw new GuideAlreadyVisibleException("The function cannot be completed at this time: the Guide UI is already active. Wait until Guide.IsVisible is false before issuing this call.");

			IsVisible = true;

			ShowKeyboardInputDelegate ski = ShowKeyboardInput; 

			return ski.BeginInvoke(player, title, description, defaultText, usePasswordMode, callback, ski);
		}

		public static string EndShowKeyboardInput (IAsyncResult result)
		{
			try 
			{
				return (result.AsyncState as ShowKeyboardInputDelegate).EndInvoke(result);
			} 
			finally 
			{
				IsVisible = false;
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
			Nullable<int> result = null;

			IsVisible = true;
			EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

			Game.Activity.RunOnUiThread(() => {
				AlertDialog.Builder alert = new AlertDialog.Builder(Game.Activity);

				alert.SetTitle(title);
				alert.SetMessage(text);

				alert.SetPositiveButton(buttons.ElementAt(0), (dialog, whichButton) =>
				{ 
					result = 0;
					IsVisible = false;
					waitHandle.Set();
				});

				if (buttons.Count() == 2)
					alert.SetNegativeButton(buttons.ElementAt(1), (dialog, whichButton) => 
					{ 
						result = 1;
						IsVisible = false;
						waitHandle.Set();
					});
				alert.SetCancelable(false);
                
				alert.Show();
			});
			waitHandle.WaitOne();
			IsVisible = false;
	
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
			if (IsVisible)
				throw new GuideAlreadyVisibleException("The function cannot be completed at this time: the Guide UI is already active. Wait until Guide.IsVisible is false before issuing this call.");
			
			IsVisible = true;
			
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
				return (result.AsyncState as ShowMessageBoxDelegate).EndInvoke(result);
			} 
			finally 
			{
				IsVisible = false;
			}
		}


		public static void ShowMarketplace (PlayerIndex player )
		{
			string packageName = Game.Activity.PackageName;
			try
			{
				Intent intent = new Intent(Intent.ActionView);
				intent.SetData(Android.Net.Uri.Parse("market://details?id=" + packageName));
				intent.SetFlags(ActivityFlags.NewTask);
				Game.Activity.StartActivity(intent);
			}
			catch (ActivityNotFoundException)
			{
				Intent intent = new Intent(Intent.ActionView);
				intent.SetData(Android.Net.Uri.Parse("http://play.google.com/store/apps/details?id=" + packageName));
				intent.SetFlags(ActivityFlags.NewTask);
				Game.Activity.StartActivity(intent);
			}
		}

		public static void Show ()
		{
			ShowSignIn(1, false);
		}

		public static void ShowSignIn (int paneCount, bool onlineOnly)
		{
			if ( paneCount != 1 )
			{
				new ArgumentException("paneCount Can only be 1 on iPhone");
				return;
			}
			
			MonoGameGamerServicesHelper.ShowSigninSheet();

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
			if ( ( Gamer.SignedInGamers.Count > 0 ) && ( Gamer.SignedInGamers[0].IsSignedInToLive ) )
			{
				
			}
		}

		public static void ShowAchievements()
		{
			if ( ( Gamer.SignedInGamers.Count > 0 ) && ( Gamer.SignedInGamers[0].IsSignedInToLive ) )
			{
				
			}
		}
		
		public static void ShowPeerPicker()
		{
			if ( ( Gamer.SignedInGamers.Count > 0 ) && ( Gamer.SignedInGamers[0].IsSignedInToLive ) )
			{
				
			}
		}
		
		
		public static void ShowMatchMaker()
		{
			if ( ( Gamer.SignedInGamers.Count > 0 ) && ( Gamer.SignedInGamers[0].IsSignedInToLive ) )
			{
			
			}
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
				return isTrialMode;
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
			internal set
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

		[CLSCompliant(false)]
		public static AndroidGameWindow Window 
		{ 
			get;
			set;
		}
		#endregion
		
	}
}