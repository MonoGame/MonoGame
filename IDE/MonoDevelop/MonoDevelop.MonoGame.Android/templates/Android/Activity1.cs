using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;

using Microsoft.Xna.Framework;

namespace ${Namespace}
{
	[Activity (Label = "${ProjectName}", 
	           MainLauncher = true,
	           Icon = "@drawable/icon",
	           Theme = "@style/Theme.Splash",
               AlwaysRetainTaskState=true,
	           LaunchMode=LaunchMode.SingleInstance,
			   ScreenOrientation = ScreenOrientation.FullUser,
	           ConfigurationChanges = ConfigChanges.Orientation | 
	                                  ConfigChanges.KeyboardHidden | 
	                                  ConfigChanges.Keyboard |
	                                  ConfigChanges.ScreenSize |
                                      ConfigChanges.ScreenLayout)]
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

