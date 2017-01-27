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
        /// <summary>
        /// Constructor. SensorDelay.Ui is passed to the base class as this orientation listener 
        /// is just used for flipping the screen orientation, therefore high frequency data is not required.
        /// </summary>
        public OrientationListener(Context context)
            : base(context, SensorDelay.Ui)
        {
        }

        public override void OnOrientationChanged(int orientation)
        {
            if (orientation == OrientationEventListener.OrientationUnknown)
                return;

            // Avoid changing orientation whilst the screen is locked
            if (ScreenReceiver.ScreenLocked)
                return;

            var disporientation = AndroidCompatibility.GetAbsoluteOrientation(orientation);

            // Only auto-rotate if target orientation is supported and not current
            AndroidGameWindow gameWindow = (AndroidGameWindow)Game.Instance.Window;
            if ((gameWindow.GetEffectiveSupportedOrientations() & disporientation) != 0 &&
                disporientation != gameWindow.CurrentOrientation)
            {
                gameWindow.SetOrientation(disporientation, true);
            }
        }
    }
}