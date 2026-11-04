// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Framework.Devices.Sensors
{
    /// <summary>
    /// Provides Desktop applications access to the device's gyrometer sensor.
    /// Currently stubs, for cross-platform development.
    /// </summary>
    public sealed partial class Gyrometer : SensorBase<GyrometerReading>
    {
        /// <summary>
        /// Gets or sets whether the device on which the application is running supports the gyrometer sensor.
        /// </summary>
        internal static bool PlatformIsSupported()
        {
            return false; // Not in Desktop
        }

        /// <summary>
        /// Gets the current state of the gyrometer. The value is a member of the SensorState enumeration.
        /// </summary>
        internal SensorState PlatformSensorState()
        {
            return SensorState.NotSupported;
        }

        internal void PlatformGyrometer()
        {
        }

        /// <summary>
        /// Initializes the platform resources required for the gyrometer sensor.
        /// </summary>
        internal static void PlatformInitialize()
        {
        }

        /// <summary>
        /// Starts data acquisition from the gyrometer.
        /// </summary>
        internal void PlatformStart()
        {
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Stops data acquisition from the gyrometer.
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
