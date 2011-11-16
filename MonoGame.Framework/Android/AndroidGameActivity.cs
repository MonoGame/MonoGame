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

namespace Microsoft.Xna.Framework
{
    public class AndroidGameActivity : Activity
    {
        public Game Game { get; set; }
		
		private OrientationListener o;
		
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			o = new OrientationListener(this);	
			if (o.CanDetectOrientation())
			{
				o.Enable();				
			}
		}

        protected override void OnPause()
        {
            base.OnPause();
            if (Game != null) Game.EnterBackground();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (Game != null) Game.EnterForeground();
        }
    }
	
	internal class OrientationListener : OrientationEventListener
	{
		AndroidGameActivity activity;
		
		public OrientationListener(AndroidGameActivity activity) : base(activity, 1)
		{
			this.activity = activity;
		}
		
		 public override void OnOrientationChanged (int orientation)
		{
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
			
			activity.Game.Window.SetOrientation(disporientation);
			

		}
	}
		
}