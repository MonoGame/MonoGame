using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;

#if OUYA
using Ouya.Console.Api;
#endif

using Microsoft.Xna.Framework;

namespace ${Namespace}
{
	[Activity (Label = "${ProjectName}", 
	           MainLauncher = true,
	           Icon = "@drawable/icon",
	           Theme = "@style/Theme.Splash",
               AlwaysRetainTaskState=true,
	           LaunchMode=LaunchMode.SingleInstance,
	           ConfigurationChanges = ConfigChanges.Orientation | 
	                                  ConfigChanges.KeyboardHidden | 
	                                  ConfigChanges.Keyboard |
	                                  ConfigChanges.ScreenSize)]
	#if OUYA
	[IntentFilter(new[] { Intent.ActionMain }
		, Categories = new[] { Intent.CategoryLauncher, OuyaIntent.CategoryGame })]
	#endif
	public class Activity1 : AndroidGameActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			var g = new Game1 ();
			SetContentView (g.Services.GetService<View>());
			g.Run ();
		}
		
	}
}

