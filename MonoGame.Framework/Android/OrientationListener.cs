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

namespace Microsoft.Xna.Framework
{
    internal class OrientationListener : OrientationEventListener
    {
        AndroidGameActivity activity;
        private bool inprogress = false;

        /// <summary>
        /// Constructor. SensorDelay.Ui is passed to the base class as this orientation listener 
        /// is just used for flipping the screen orientation, therefore high frequency data is not required.
        /// </summary>
        public OrientationListener(AndroidGameActivity activity)
            : base(activity, SensorDelay.Ui)
        {
            this.activity = activity;
        }

        public override void OnOrientationChanged(int orientation)
        {
            if (!inprogress)
            {
                inprogress = true;
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
                    case 90: disporientation = DisplayOrientation.LandscapeRight;
                        break;
                    case 270: disporientation = DisplayOrientation.LandscapeLeft;
                        break;
                    case 0: disporientation = DisplayOrientation.Portrait;
                        break;
                    case 180: disporientation = DisplayOrientation.PortraitUpsideDown;
                        break;
                    default:
                        disporientation = DisplayOrientation.LandscapeLeft;
                        break;
                }

                // Only auto-rotate if target orientation is supported and not current
                if ((AndroidGameActivity.Game.Window.GetEffectiveSupportedOrientations() & disporientation) != 0 &&
                     disporientation != AndroidGameActivity.Game.Window.CurrentOrientation)
                {
                    AndroidGameActivity.Game.Window.SetOrientation(disporientation);
                }
                inprogress = false;
            }
        }
    }
}