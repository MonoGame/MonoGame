using System;
using Android.App;
using Android.Content.Res;
using Android.Hardware;
using Android.Views;

namespace Microsoft.Xna.Framework
{
    internal class OrientationListener : OrientationEventListener
    {
        readonly object _orientationChangedLock = new object();
        
        readonly AndroidGameWindow _gameWindow;

        readonly Orientation _defaultOrientation;

        /// <summary>
        /// Constructor. SensorDelay.Ui is passed to the base class as this orientation listener 
        /// is just used for flipping the screen orientation, therefore high frequency data is not required.
        /// </summary>
        public OrientationListener(AndroidGameActivity activity, AndroidGameWindow gameWindow)
            : base(activity, SensorDelay.Ui)
        {
            _gameWindow = gameWindow;

            _defaultOrientation = GetDeviceDefaultOrientation(activity);
        }

        public override void OnOrientationChanged(int orientation)
        {
            // Avoid changing orientation whilst the screen is locked
            if (ScreenReceiver.ScreenLocked)
                return;

            lock (_orientationChangedLock)
            {
                if (_defaultOrientation == Orientation.Landscape)
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
                if ((_gameWindow.GetEffectiveSupportedOrientations() & disporientation) != 0 &&
                    disporientation != _gameWindow.CurrentOrientation)
                {
                    _gameWindow.SetOrientation(disporientation, true);
                }
            }
        }
            
        private Orientation GetDeviceDefaultOrientation(Activity activity)
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