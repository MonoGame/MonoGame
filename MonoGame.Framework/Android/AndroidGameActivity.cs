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
    public class AndroidGameActivity : Activity
    {
        public static Game Game { get; set; }
		
		private OrientationListener o;		
		
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			o = new OrientationListener(this);	
			if (o.CanDetectOrientation())
			{
				o.Enable();				
			}					

            RequestWindowFeature(WindowFeatures.NoTitle);
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
            Game.GraphicsDevice.ResourcesLost = true;
			if (Game.Window != null && Game.Window.Parent != null && (Game.Window.Parent is FrameLayout))
			{				
              ((FrameLayout)Game.Window.Parent).RemoveAllViews();
			}
        }

        public static event EventHandler Resumed;
        protected override void OnResume()
        {
            base.OnResume();
            if (Resumed != null)
                Resumed(this, EventArgs.Empty);

            var deviceManager = (IGraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
            if (deviceManager == null)
                return;
            (deviceManager as GraphicsDeviceManager).ForceSetFullScreen();
            Game.Window.RequestFocus();
        }

    }
	
	internal class OrientationListener : OrientationEventListener
	{
		AndroidGameActivity activity;
		
		public OrientationListener(AndroidGameActivity activity) : base(activity, SensorDelay.Game)
		{
			this.activity = activity;
		}
		
		private bool inprogress = false;
		
		 public override void OnOrientationChanged (int orientation)
		{
			if (!inprogress) 
			{			
				inprogress = true;
				// Divide by 90 into an int to round, then multiply out to one of 5 positions, either 0,90,180,270,360. 
				int ort = (90*(int)Math.Round(orientation/90f)) % 360;
				
				// Convert 360 to 0
				if(ort == 360)
				{
				    ort = 0;
				}
										
				var disporientation = DisplayOrientation.Unknown;
				
				switch (ort) {
					case 90 : disporientation = DisplayOrientation.LandscapeRight;
						break;
			    	case 270 : disporientation = DisplayOrientation.LandscapeLeft;
						break;
			    	case 0 : disporientation = DisplayOrientation.Portrait;
						break;
					default:
						disporientation = DisplayOrientation.LandscapeLeft;
						break;
				}
				
				if (AndroidGameActivity.Game.Window.CurrentOrientation != disporientation)
				{
				AndroidGameActivity.Game.Window.SetOrientation(disporientation);
				}
				inprogress = false;
			}
			

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
