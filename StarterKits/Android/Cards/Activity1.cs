using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Blackjack
{
	[Activity (Label = "Blackjack.Android", MainLauncher = true)]
	public class Activity1 : Activity
	{
		protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var g = new BlackjackGame(this);
            SetContentView(g.Window);
            g.Run();
        }
	}
}


