// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
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
        public static bool IsSupported
        {
            get
            {
                return PlatformIsSupported();
            }
        }

        /// <summary>
        /// Gets the current state of the accelerometer. The value is a member of the SensorState enumeration.
        /// </summary>
        public SensorState State
        {
            get
            {
                return PlatformSensorState();
            }
        }

        /// <summary>
        /// Creates a new instance of the Accelerometer object.
        /// </summary>
        public Accelerometer()
        {
            PlatformAccelerometer();
        }

        /// <summary>
        /// Initializes the platform resources required for the accelerometer sensor.
        /// </summary>
        static void Initialize()
        {
            PlatformInitialize();
        }

        /// <summary>
        /// Starts data acquisition from the accelerometer.
        /// </summary>
        public override void Start()
        {
            PlatformStart();
        }

        /// <summary>
        /// Stops data acquisition from the accelerometer.
        /// </summary>
        public override void Stop()
        {
            PlatformStop();
        }

        protected override void Dispose(bool disposing)
        {
            PlatformDispose(disposing);

            base.Dispose(disposing);
        }
    }
}