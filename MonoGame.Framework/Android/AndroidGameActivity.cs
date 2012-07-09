using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
    public class AndroidGameActivity : Activity
    {
		public static Game Game { get; set; }
		
		private OrientationListener o;		
		private ScreenReceiver screenReceiver;

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
			o = new OrientationListener(this);	
			if (o.CanDetectOrientation())
			{
				o.Enable();				
			}					

			IntentFilter filter = new IntentFilter();
		    filter.AddAction(Intent.ActionScreenOff);
		    filter.AddAction(Intent.ActionScreenOn);
		    filter.AddAction(Intent.ActionUserPresent);
		    
		    screenReceiver = new ScreenReceiver();
		    RegisterReceiver(screenReceiver, filter);

            RequestWindowFeature(WindowFeatures.NoTitle);
		}

        public static event EventHandler Paused;

		public override void OnConfigurationChanged (Android.Content.Res.Configuration newConfig)
		{
			// we need to refresh the viewport here.			
			base.OnConfigurationChanged (newConfig);
		}

		/// <summary>
		/// Called when another app comes into the foreground or
		/// if the screen is locked
		/// </summary>
        protected override void OnPause()
        {
		    base.OnPause();
            if (Paused != null)
                Paused(this, EventArgs.Empty);

            //if (Game.GraphicsDevice != null)
             //   Game.GraphicsDevice.ResourcesLost = true;

			//if (Game.Window != null && Game.Window.Parent != null && (Game.Window.Parent is FrameLayout))
			//{				
            //  ((FrameLayout)Game.Window.Parent).RemoveAllViews();
			//}
        }

        public static event EventHandler Resumed;

		/// <summary>
		/// Happens when the user returns to the activity
		/// and when it first starts
		/// </summary>
        protected override void OnResume()
        {
			base.OnResume();
            if (Resumed != null)
                Resumed(this, EventArgs.Empty);

            var deviceManager = (IGraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
            if (deviceManager == null)
                return;

            (deviceManager as GraphicsDeviceManager).ForceSetFullScreen();
            //Game.Window.RequestFocus();
            //Game.GraphicsDevice.Initialize(Game.Platform);		
        }

		protected override void OnStart ()
		{
			base.OnStart ();
		}

		protected override void OnStop ()
		{
			base.OnStop ();
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
		}

		protected override void OnRestart ()
		{
			base.OnRestart ();
		}
    }
	
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
