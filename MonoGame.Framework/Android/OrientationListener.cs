using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Hardware;
using Android.Views;

namespace Microsoft.Xna.Framework
{
    internal class OrientationListener : OrientationEventListener
    {
        private Lazy<Orientation> _naturalOrientation;

        /// <summary>
        /// Constructor. SensorDelay.Ui is passed to the base class as this orientation listener 
        /// is just used for flipping the screen orientation, therefore high frequency data is not required.
        /// </summary>
        public OrientationListener(Context context)
            : base(context, SensorDelay.Ui)
        {
            _naturalOrientation = new Lazy<Orientation>(() => GetDeviceNaturalOrientation(Game.Activity));
        }

        public override void OnOrientationChanged(int orientation)
        {
            if (orientation == OrientationEventListener.OrientationUnknown)
                return;

            // Avoid changing orientation whilst the screen is locked
            if (ScreenReceiver.ScreenLocked)
                return;

            if (_naturalOrientation.Value == Orientation.Landscape)
                orientation += 270;
                
            // Round orientation into one of 4 positions, either 0, 90, 180, 270. 
            int ort = ((orientation + 45) / 90 * 90) % 360;

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
            AndroidGameWindow gameWindow = (AndroidGameWindow)Game.Instance.Window;
            if ((gameWindow.GetEffectiveSupportedOrientations() & disporientation) != 0 &&
                disporientation != gameWindow.CurrentOrientation)
            {
                gameWindow.SetOrientation(disporientation, true);
            }
        }

        private Orientation GetDeviceNaturalOrientation(Activity activity)
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