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
	           ConfigurationChanges = ConfigChanges.Orientation | 
	                                  ConfigChanges.KeyboardHidden | 
	                                  ConfigChanges.Keyboard)]
	public class Activity1 : AndroidGameActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create our OpenGL view, and display it
			Game1.Activity = this;
			var g = new Game1();
			SetContentView (g.Window);
			g.Run();
		}
		
	}
}

