using System;

using Android.App;
using Android.Content;
using Android.Runtime;
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
	           LaunchMode=Android.Content.PM.LaunchMode.SingleInstance,
	           ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | 
	                                  Android.Content.PM.ConfigChanges.KeyboardHidden | 
	                                  Android.Content.PM.ConfigChanges.Keyboard)]
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

