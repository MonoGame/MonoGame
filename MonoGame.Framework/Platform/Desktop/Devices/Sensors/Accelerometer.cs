// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;

namespace Microsoft.Devices.Sensors
{
    /// <summary>
    /// Provides Android applications access to the device's accelerometer sensor.
    /// </summary>
    public sealed class Accelerometer : SensorBase<AccelerometerReading>
    {
        /// <summary>
        /// Gets or sets whether the device on which the application is running supports the accelerometer sensor.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                return false; // Not in Desktop
            }
        }

        /// <summary>
        /// Gets the current state of the accelerometer. The value is a member of the SensorState enumeration.
        /// </summary>
        public SensorState State
        {
            get
            {
                return SensorState.NotSupported;
            }
        }

        /// <summary>
        /// Creates a new instance of the Accelerometer object.
        /// </summary>
        public Accelerometer()
        {
        }

        /// <summary>
        /// Initializes the platform resources required for the accelerometer sensor.
        /// </summary>
        static void Initialize()
        {
        }

        void ActivityPaused(object sender, EventArgs eventArgs)
        {
        }

        void ActivityResumed(object sender, EventArgs eventArgs)
        {
        }

        /// <summary>
        /// Starts data acquisition from the accelerometer.
        /// </summary>
        public override void Start()
        {
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Stops data acquisition from the accelerometer.
        /// </summary>
        public override void Stop()
        {
            throw new PlatformNotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {

            base.Dispose(disposing);
        }
    }
}