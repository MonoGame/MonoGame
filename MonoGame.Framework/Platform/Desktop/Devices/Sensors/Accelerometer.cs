// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Framework.Devices.Sensors
{
    /// <summary>
    /// Provides Desktop applications access to the device's accelerometer sensor.
    /// Currently stubs, for cross-platform development.
    /// </summary>
    public sealed partial class Accelerometer : SensorBase<AccelerometerReading>
    {
        /// <summary>
        /// Gets or sets whether the device on which the application is running supports the accelerometer sensor.
        /// </summary>
        internal static bool PlatformIsSupported()
        {
            return false; // Not in Desktop
        }

        /// <summary>
        /// Gets the current state of the accelerometer. The value is a member of the SensorState enumeration.
        /// </summary>
        internal SensorState PlatformSensorState()
        {
            return SensorState.NotSupported;
        }

        internal void PlatformAccelerometer()
        {
        }

        /// <summary>
        /// Initializes the platform resources required for the accelerometer sensor.
        /// </summary>
        internal static void PlatformInitialize()
        {
        }

        /// <summary>
        /// Starts data acquisition from the accelerometer.
        /// </summary>
        internal void PlatformStart()
        {
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Stops data acquisition from the accelerometer.
        /// </summary>
        internal void PlatformStop()
        {
            throw new PlatformNotSupportedException();
        }

        internal void PlatformDispose(bool disposing)
        {
            
        }
    }
}