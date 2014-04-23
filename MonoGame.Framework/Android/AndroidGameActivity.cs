using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
	[CLSCompliant(false)]
    public class AndroidGameActivity : Activity
    {
        internal Game Game { private get; set; }

        private ScreenReceiver screenReceiver;

        public bool AutoPauseAndResumeMediaPlayer = true;

		/// <summary>
		/// OnCreate called when the activity is launched from cold or after the app
		/// has been killed due to a higher priority app needing the memory
		/// </summary>
		/// <param name='savedInstanceState'>
		/// Saved instance state.
		/// </param>
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			IntentFilter filter = new IntentFilter();
		    filter.AddAction(Intent.ActionScreenOff);
		    filter.AddAction(Intent.ActionScreenOn);
		    filter.AddAction(Intent.ActionUserPresent);
		    
		    screenReceiver = new ScreenReceiver();
		    RegisterReceiver(screenReceiver, filter);

            RequestWindowFeature(WindowFeatures.NoTitle);

			Game.Activity = this;
		}

        public static event EventHandler Paused;

		public override void OnConfigurationChanged (Android.Content.Res.Configuration newConfig)
		{
			// we need to refresh the viewport here.
			base.OnConfigurationChanged (newConfig);
		}

        protected override void OnPause()
        {
            base.OnPause();
            if (Paused != null)
                Paused(this, EventArgs.Empty);

        }

        public static event EventHandler Resumed;
        protected override void OnResume()
        {
            base.OnResume();
            if (Resumed != null)
                Resumed(this, EventArgs.Empty);

            if (Game != null)
            {
                var deviceManager = (IGraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
                if (deviceManager == null)
                    return;
                ((GraphicsDeviceManager)deviceManager).ForceSetFullScreen();
                ((AndroidGameWindow)Game.Window).GameView.RequestFocus();
            }
        }

		protected override void OnDestroy ()
		{
            UnregisterReceiver(screenReceiver);
            if (Game != null)
                Game.Dispose();
            Game = null;
			base.OnDestroy ();
		}
    }

	[CLSCompliant(false)]
	public static class ActivityExtensions
    {
        public static ActivityAttribute GetActivityAttribute(this AndroidGameActivity obj)
        {			
            var attr = obj.GetType().GetCustomAttributes(typeof(ActivityAttribute), true);
			if (attr != null)
			{
            	return ((ActivityAttribute)attr[0]);
			}
			return null;
        }
    }

}
