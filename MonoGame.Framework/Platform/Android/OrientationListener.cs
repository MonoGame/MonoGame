// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Views;
using Android.Provider;

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

            // Check if screen orientation is locked by user: if it's locked, do not change orientation.
            try
            {
                if (Settings.System.GetInt(Application.Context.ContentResolver, "accelerometer_rotation") == 0)
                    return;
            }
            catch (Settings.SettingNotFoundException)
            {
                // Do nothing (or log warning?). In case android API or Xamarin do not support this Android system property.
            }

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