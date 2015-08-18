// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
#if ANDROID
using Android.Content.PM;
#endif
#if IOS
using UIKit;
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

#if WINDOWS_STOREAPP || WINDOWS_UAP
                // Is a touch device present?
                // Iterate through all pointer devices and find the maximum number of concurrent touches possible
                maximumTouchCount = 0;
                var pointerDevices = Windows.Devices.Input.PointerDevice.GetPointerDevices();
                foreach (var pointerDevice in pointerDevices)
                {
                    maximumTouchCount = Math.Max(maximumTouchCount, (int)pointerDevice.MaxContacts);

                    if (pointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                        isConnected = true;
                }
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
#elif IOS
                //iPhone supports 5, iPad 11
                isConnected = true;
                if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
                    maximumTouchCount = 5;
                else //Pad
                    maximumTouchCount = 11;
#elif WINDOWS_PHONE
                // There is no API on WP8, XNA returns 4 according to the docs
                // http://msdn.microsoft.com/en-nz/library/ff434208.aspx
                // http://en.wikipedia.org/wiki/Windows_Phone_8#Hardware_requirements
                isConnected = true;
                maximumTouchCount = 4;
#else
                //Touch isn't implemented in OpenTK, so no linux or mac https://github.com/opentk/opentk/issues/80
                isConnected = false;
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