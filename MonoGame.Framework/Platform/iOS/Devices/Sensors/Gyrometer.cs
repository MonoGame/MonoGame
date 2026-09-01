using System;

using Microsoft.Xna.Framework;

using CoreMotion;
using Foundation;

namespace MonoGame.Framework.Devices.Sensors
{
    /// <summary>
    /// Provides methods to access gyrometer data from the device
    /// </summary>
    public sealed partial class Gyrometer : SensorBase<GyrometerReading>
    {
        static readonly int MaxSensorCount = 10;
        static int instanceCount;
        private static bool started = false;
        private static SensorState state = IsSupported ? SensorState.Initializing : SensorState.NotSupported;
        static readonly CMMotionManager motionManager = new CMMotionManager();

        /// <summary>
        /// Check if an gyrometer is supported on the current device
        /// </summary>
        /// <returns>true if an gyrometer is supported</returns>
        internal static bool PlatformIsSupported()
        {
            return motionManager.GyroAvailable;
        }

        /// <summary>
        /// Check the current state of the gyrometer
        /// </summary>
        /// <returns>Returns current <see cref="SensorState">SensorState</see></returns>
        public SensorState PlatformSensorState()
        {
            return state;
        }

        private static event CMGyroHandler readingChanged;

        /// <summary>
        /// Create a new instance of Gyrometer
        /// </summary>
        /// <exception cref="GyrometerFailedException">No default sensor is found</exception>
        /// <exception cref="SensorFailedException">The maximum limit of Gyrometer instances has been reached (10)</exception>
        internal void PlatformGyrometer()
        {
            if (!IsSupported)
                throw new GyrometerFailedException("Failed to start gyrometer data acquisition. No default sensor found.", -1);
            else if (instanceCount >= MaxSensorCount)
                throw new SensorFailedException("The limit of 10 simultaneous instances of the Gyrometer class per application has been exceeded.");

            ++instanceCount;

            this.TimeBetweenUpdatesChanged += this.UpdateInterval;
            readingChanged += ReadingChangedHandler;

        }

        /// <inheritdoc cref="IDisposable.Dispose()"/>
        internal void PlatformDispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (started)
                        Stop();
                    --instanceCount;
                }
            }
        }

        /// <summary>
        /// Begin collecting gyrometer data
        /// </summary>
        /// <exception cref="GyrometerFailedException">Gyrometer is already started</exception>
        internal void PlatformStart()
        {
            if (started == false)
            {
                motionManager.StartGyroUpdates(NSOperationQueue.CurrentQueue, GyroHandler);
                started = true;
                state = SensorState.Ready;
            }
            else
                throw new GyrometerFailedException("Failed to start gyrometer data acquisition. Data acquisition already started.", -1);
        }

        /// <summary>
        /// Stop collection gyrometer data
        /// </summary>
        internal void PlatformStop()
        {
            motionManager.StopGyroUpdates();
            started = false;
            state = SensorState.Disabled;
        }

        private void GyroHandler(CMGyroData data, NSError error)
        {
            readingChanged(data, error);
        }

        private void ReadingChangedHandler(CMGyroData data, NSError error)
        {
            GyrometerReading reading = new GyrometerReading();
            this.IsDataValid = error == null;
            if (this.IsDataValid)
            {
                this.IsDataValid = true;
                reading.AngularVelocity = new Vector3((float)data.RotationRate.X, (float)data.RotationRate.Y, (float)data.RotationRate.Z);
                reading.Timestamp = DateTime.UtcNow;
                this.CurrentValue = reading;
                this.IsDataValid = error == null;
            }
        }

        private void UpdateInterval(object sender, EventArgs args)
        {
            motionManager.GyroUpdateInterval = this.TimeBetweenUpdates.TotalSeconds;
        }

        internal static void PlatformInitialize()
        {
        }
    }
}
