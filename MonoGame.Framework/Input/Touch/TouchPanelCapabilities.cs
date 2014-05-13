// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
#if ANDROID
using Android.Content.PM;
#endif

namespace Microsoft.Xna.Framework.Input.Touch
{
    /// <summary>
    /// Allows retrieval of capabilities information from touch panel device.
    /// </summary>
    public struct TouchPanelCapabilities
    {
		private bool hasPressure;
		private bool isConnected;
		private int maximumTouchCount;
        private bool initialized;

        internal void Initialize()
        {
            if (!initialized)
            {
                initialized = true;

                // There does not appear to be a way of finding out if a touch device supports pressure.
                // XNA does not expose a pressure value, so let's assume it doesn't support it.
                hasPressure = false;

#if WINDOWS_STOREAPP
                // Is a touch device present?
                var caps = new Windows.Devices.Input.TouchCapabilities();
                isConnected = caps.TouchPresent != 0;

                // Iterate through all pointer devices and find the maximum number of concurrent touches possible
                maximumTouchCount = 0;
                var pointerDevices = Windows.Devices.Input.PointerDevice.GetPointerDevices();
                foreach (var pointerDevice in pointerDevices)
                    maximumTouchCount = Math.Max(maximumTouchCount, (int)pointerDevice.MaxContacts);
#elif WINDOWS
                maximumTouchCount = GetSystemMetrics(SM_MAXIMUMTOUCHES);
                isConnected = (maximumTouchCount > 0);
#elif ANDROID
                // http://developer.android.com/reference/android/content/pm/PackageManager.html#FEATURE_TOUCHSCREEN
                var pm = Game.Activity.PackageManager;
                isConnected = pm.HasSystemFeature(PackageManager.FeatureTouchscreen);
                if (pm.HasSystemFeature(PackageManager.FeatureTouchscreenMultitouchJazzhand))
                    maximumTouchCount = 5;
                else if (pm.HasSystemFeature(PackageManager.FeatureTouchscreenMultitouchDistinct))
                    maximumTouchCount = 2;
                else
                    maximumTouchCount = 1;
#else
                isConnected = true;
                maximumTouchCount = 8;
#endif
            }
		}

        public bool HasPressure
        {
            get
            {
                return hasPressure;
            }
        }

        /// <summary>
        /// Returns true if a device is available for use.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
        }

        /// <summary>
        /// Returns the maximum number of touch locations tracked by the touch panel device.
        /// </summary>
        public int MaximumTouchCount
        {
            get
            {
                return maximumTouchCount;
            }
        }

#if WINDOWS
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
        static extern int GetSystemMetrics(int nIndex);

        const int SM_MAXIMUMTOUCHES = 95;
#endif
    }
}