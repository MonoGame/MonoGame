using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;

namespace Microsoft.Xna.Framework
{
    internal class OrientationListener : OrientationEventListener
    {
        AndroidGameActivity activity;
		private Orientation defaultOrientation;
		
        private bool inprogress = false;

        /// <summary>
        /// Constructor. SensorDelay.Ui is passed to the base class as this orientation listener 
        /// is just used for flipping the screen orientation, therefore high frequency data is not required.
        /// </summary>
        public OrientationListener(AndroidGameActivity activity)
            : base(activity, SensorDelay.Ui)
        {
            this.activity = activity;
			this.defaultOrientation = GetDeviceDefaultOrientation();
        }

        public override void OnOrientationChanged(int orientation)
        {
            // Avoid changing orientation whilst the screen is locked
            if (ScreenReceiver.ScreenLocked)
                return;

            if (!inprogress)
            {
                inprogress = true;
				
				if (defaultOrientation == Orientation.Landscape)
					orientation += 270;
				
                // Divide by 90 into an int to round, then multiply out to one of 5 positions, either 0,90,180,270,360. 
                int ort = (90 * (int)Math.Round(orientation / 90f)) % 360;

                // Convert 360 to 0
                if (ort == 360)
                {
                    ort = 0;
                }

                var disporientation = DisplayOrientation.Unknown;
                switch (ort)
                {
                    case 90: disporientation = AndroidCompatibility.FlipLandscape ? DisplayOrientation.LandscapeLeft : DisplayOrientation.LandscapeRight;
                        break;
					case 270: disporientation = AndroidCompatibility.FlipLandscape ? DisplayOrientation.LandscapeRight : DisplayOrientation.LandscapeLeft;
                        break;
                    case 0: disporientation = DisplayOrientation.Portrait;
                        break;
                    case 180: disporientation = DisplayOrientation.PortraitDown;
                        break;
                    default:
                        disporientation = DisplayOrientation.LandscapeLeft;
                        break;
                }

                // Only auto-rotate if target orientation is supported and not current
                if (AndroidGameActivity.Game != null &&
                    (AndroidGameActivity.Game.Window.GetEffectiveSupportedOrientations() & disporientation) != 0 &&
                     disporientation != AndroidGameActivity.Game.Window.CurrentOrientation)
                {
                    AndroidGameActivity.Game.Window.SetOrientation(disporientation, true);
                }
                inprogress = false;
            }
        }
			
		Orientation GetDeviceDefaultOrientation()
		{
			var windowManager = activity.WindowManager;

			Configuration config = activity.Resources.Configuration;

			SurfaceOrientation rotation = windowManager.DefaultDisplay.Rotation;

			if (((rotation == SurfaceOrientation.Rotation0 || rotation == SurfaceOrientation.Rotation180) &&
				config.Orientation == Orientation.Landscape)
				|| ((rotation == SurfaceOrientation.Rotation90 || rotation == SurfaceOrientation.Rotation270) && 
				config.Orientation == Orientation.Portrait))
			{
				return Orientation.Landscape;
			}
			else
			{
				return Orientation.Portrait;
			}
		}
    }
}